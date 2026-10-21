using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;
using ValheimFortress.Challenge;
using ValheimFortress.Data;
using static ValheimFortress.Data.WaveStyles;

namespace ValheimFortress
{
    // Internal implementation behind the public reflection facade (ValheimFortress.API). Consumers never
    // reference this directly; the facade resolves it with Type.GetType and invokes these static methods.
    // Only framework types (strings, collections, Unity Vector3) cross that reflection boundary -- the
    // challenge definition itself arrives as a JSON string and is deserialized here.
    public static class APIReceiver
    {
        private static readonly Heightmap.Biome[] supported_biomes =
        {
            Heightmap.Biome.Meadows, Heightmap.Biome.BlackForest, Heightmap.Biome.Swamp,
            Heightmap.Biome.Mountain, Heightmap.Biome.Plains, Heightmap.Biome.Mistlands, Heightmap.Biome.AshLands
        };

        // Runs a wave-based challenge. The definition is a JSON-serialized VFChallengeDefinition (see the
        // public API), spawnPoints are the world positions creatures emerge from, and rewardLocation is
        // where the rewards are dropped. Must be called in-world (ZNetScene loaded) on the instance that
        // should own/drive the run (the server in dedicated setups, or the host).
        public static bool RunChallenge(string definitionJson, Vector3[] spawnPoints, Vector3 rewardLocation)
        {
            return Launch(definitionJson, spawnPoints, rewardLocation) != null;
        }

        // Schema 4: the same as RunChallenge, returning a handle (the runner's ZDOID) for GetChallengeState and
        // StopChallenge, or null when the run could not start.
        public static string StartChallenge(string definitionJson, Vector3[] spawnPoints, Vector3 rewardLocation)
        {
            ExternalShrine shrine = Launch(definitionJson, spawnPoints, rewardLocation);
            ZNetView view = shrine != null ? shrine.GetComponent<ZNetView>() : null;
            if (view == null || view.GetZDO() == null) { return null; }
            ZDOID id = view.GetZDO().m_uid;
            return $"{id.UserID}:{id.ID}";
        }

        // Reads the runner's ZDO, so it works on any peer that has the run's area loaded, owner or not. Only framework
        // types cross the API boundary, so the state is a flat name -> int map (booleans as 0/1).
        public static Dictionary<string, int> GetChallengeState(string handle)
        {
            if (!TryParseHandle(handle, out ZDOID id)) { return null; }
            Dictionary<string, int> state = new Dictionary<string, int>();
            ZDO zdo = ZDOMan.instance?.GetZDO(id);
            if (zdo == null)
            {
                state["exists"] = 0;
                return state;
            }
            state["exists"] = 1;
            state["starting"] = zdo.GetBool(ExternalShrine.StartKey) ? 1 : 0;
            state["active"] = zdo.GetBool(ExternalShrine.ActiveKey) ? 1 : 0;
            state["phase"] = zdo.GetInt(ExternalShrine.PhaseKey);
            state["phases"] = zdo.GetInt(ExternalShrine.TotalPhasesKey);
            state["alive"] = zdo.GetInt(ExternalShrine.AliveKey);
            state["completed"] = zdo.GetBool(ExternalShrine.CompletedKey) ? 1 : 0;
            state["stopped"] = zdo.GetBool(ExternalShrine.StoppedKey) ? 1 : 0;
            return state;
        }

        // Asks the run's owner to stop it. When nobody owns it (its area is unloaded everywhere) this peer takes it
        // over and leaves a stop request that the next owner to load it carries out.
        public static bool StopChallenge(string handle)
        {
            if (!TryParseHandle(handle, out ZDOID id)) { return false; }
            ZDO zdo = ZDOMan.instance?.GetZDO(id);
            if (zdo == null || ZRoutedRpc.instance == null) { return false; }
            long owner = zdo.GetOwner();
            if (owner == 0L)
            {
                zdo.SetOwner(ZDOMan.GetSessionID());
                zdo.Set(ExternalShrine.StopRequestedKey, true);
                return true;
            }
            ZRoutedRpc.instance.InvokeRoutedRPC(owner, id, ExternalShrine.StopRpc);
            return true;
        }

        private static bool TryParseHandle(string handle, out ZDOID id)
        {
            id = ZDOID.None;
            if (string.IsNullOrEmpty(handle)) { return false; }
            string[] parts = handle.Split(':');
            if (parts.Length != 2 || !long.TryParse(parts[0], out long user) || !uint.TryParse(parts[1], out uint number)) { return false; }
            id = new ZDOID(user, number);
            return id != ZDOID.None;
        }

        private static ExternalShrine Launch(string definitionJson, Vector3[] spawnPoints, Vector3 rewardLocation)
        {
            if (ZNetScene.instance == null)
            {
                Jotunn.Logger.LogWarning("VF-API: RunChallenge called with no world loaded (ZNetScene is null).");
                return null;
            }
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Jotunn.Logger.LogWarning("VF-API: RunChallenge requires at least one spawn point.");
                return null;
            }

            VFChallengeDefinition def = Deserialize(definitionJson);
            if (def == null)
            {
                Jotunn.Logger.LogWarning("VF-API: RunChallenge could not parse the supplied challenge definition.");
                return null;
            }

            WaveCreatureSettings.Validate(def.CreatureRules, "VF-API:");

            // Spawner.SpawnHorde randomly indexes into whatever spawn locations it's given, so any number
            // (>= 1, already validated above) is fine; creatures emerge from a randomly chosen point per spawn.
            PhasedWaveTemplate waves = BuildWaveTemplate(def);
            if (waves == null || waves.hordePhases == null || waves.hordePhases.Count == 0)
            {
                Jotunn.Logger.LogWarning("VF-API: RunChallenge produced an empty wave template; nothing to run.");
                return null;
            }

            GameObject prefab = PrefabManager.Instance.GetPrefab(ValheimFortress.ApiChallengeRunnerPrefab);
            if (prefab == null)
            {
                Jotunn.Logger.LogError($"VF-API: runner prefab '{ValheimFortress.ApiChallengeRunnerPrefab}' was not found.");
                return null;
            }

            GameObject runner = UnityEngine.Object.Instantiate(prefab, rewardLocation, Quaternion.identity);
            ExternalShrine shrine = runner.GetComponent<ExternalShrine>();
            if (shrine == null)
            {
                Jotunn.Logger.LogError("VF-API: runner prefab is missing its ExternalShrine component.");
                UnityEngine.Object.Destroy(runner);
                return null;
            }

            Dictionary<string, short> creature_drop_overrides = BuildDropOverrides(def.CreatureDropOverrides);

            shrine.BeginApiChallenge(waves, spawnPoints, rewardLocation, def.ScaledRewards, def.FixedRewards,
                def.Difficulty, def.HardMode, def.BossMode, def.SiegeMode, def.EnableCreatureDrops, creature_drop_overrides,
                def.WaveStartMessage, def.WaveEndMessage, def.DrawMapOverlay, def.BetweenWavePhrases, def.OrderedPhrases,
                def.SpawnPortalVisuals);

            Jotunn.Logger.LogInfo($"VF-API: started challenge with {waves.hordePhases.Count} phases at {rewardLocation}.");
            return shrine;
        }

        // Names of every creature that can be spawned (the keys used in VFHoardEntry.Creature).
        public static List<string> GetSpawnableCreatures()
        {
            return Monsters.SpawnableCreatures.Keys.ToList();
        }

        // Item prefab names valid as reward keys (the scaled-reward catalog with sensible costs).
        public static List<string> GetRewardItems()
        {
            return RewardsData.resourceRewards.Values.Select(entry => entry.resourcePrefab).Distinct().ToList();
        }

        // Names of every available wave style (valid values for VFChallengeDefinition.WaveStyle).
        public static List<string> GetWaveStyles()
        {
            return Enum.GetNames(typeof(WaveStyleName)).ToList();
        }

        // The challenge a wild shrine runs for the given tribute, as a JSON-serialized VFChallengeDefinition (the
        // caller deserializes it with the same settings), or null when there is no such shrine or tribute.
        // Reads the wild shrine config currently loaded, which is the server's once it has synced.
        public static string GetShrineChallengeDefinition(string shrineName, string tributePrefab)
        {
            if (string.IsNullOrEmpty(shrineName) || string.IsNullOrEmpty(tributePrefab)) { return null; }
            // Accept a placed shrine's gameObject.name too.
            if (shrineName.EndsWith("(Clone)")) { shrineName = shrineName.Substring(0, shrineName.Length - 7); }

            WildShrineConfiguration shrine = WildShrineData.GetWildShrineConfigurationForSpecificShrine(shrineName);
            if (shrine == null)
            {
                Jotunn.Logger.LogWarning($"VF-API: no wild shrine named '{shrineName}'. Known shrines: {string.Join(", ", GetShrineTributes().Keys)}.");
                return null;
            }

            // First match wins, the same as offering the item at the shrine.
            WildShrineLevelConfiguration level = shrine.wildShrineLevelsConfig?.FirstOrDefault(entry =>
                entry != null && string.Equals(entry.tributeName, tributePrefab, StringComparison.OrdinalIgnoreCase));
            if (level == null || level.wildLevelDefinition == null)
            {
                Jotunn.Logger.LogWarning($"VF-API: wild shrine '{shrine.definitionForWildShrine}' does not accept tribute '{tributePrefab}'. " +
                    $"Accepted: {string.Join(", ", TributeNames(shrine))}.");
                return null;
            }

            return Serialize(BuildShrineDefinition(level));
        }

        // Every wild shrine prefab name mapped to the tribute item prefabs it accepts.
        public static Dictionary<string, List<string>> GetShrineTributes()
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            foreach (WildShrineConfiguration shrine in WildShrineData.GetWildShrineDefinitions())
            {
                // The lookup by name takes the first match, so a duplicate name is never reachable.
                if (string.IsNullOrEmpty(shrine?.definitionForWildShrine) || result.ContainsKey(shrine.definitionForWildShrine)) { continue; }
                result[shrine.definitionForWildShrine] = TributeNames(shrine);
            }
            return result;
        }

        private static List<string> TributeNames(WildShrineConfiguration shrine)
        {
            if (shrine.wildShrineLevelsConfig == null) { return new List<string>(); }
            return shrine.wildShrineLevelsConfig.Where(entry => !string.IsNullOrEmpty(entry?.tributeName)).Select(entry => entry.tributeName).ToList();
        }

        // Mirrors what WildShrine does with the level: the wave is generated from ToChallengeLevelDefinition with
        // the level's hard/siege flags and creature cap, the rewards are scaled by levelIndex, and loot drops
        // follow each creature's dropsEnabled setting (GenericShrine.ShouldDropLoot).
        private static VFChallengeDefinition BuildShrineDefinition(WildShrineLevelConfiguration level)
        {
            WildLevelDefinition wild = level.wildLevelDefinition;
            Dictionary<string, bool> drop_overrides = Monsters.SpawnableCreatures
                .Where(entry => entry.Value != null && entry.Value.dropsEnabled)
                .ToDictionary(entry => entry.Key, entry => true);

            return new VFChallengeDefinition
            {
                Biome = wild.biome,
                Difficulty = wild.levelIndex,
                WaveStyle = wild.waveFormat.ToString(),
                // Wild levels have no phase count; wave generation falls back to 4.
                NumPhases = 4,
                MaxCreaturesPerPhase = wild.maxCreaturesPerPhaseOverride,
                OnlySelectMonsters = wild.onlySelectMonsters,
                ExcludeSelectMonsters = wild.excludeSelectMonsters,
                CommonSpawnModifiers = wild.commonSpawnModifiers?.ToApi(),
                RareSpawnModifiers = wild.rareSpawnModifiers?.ToApi(),
                EliteSpawnModifiers = wild.eliteSpawnModifiers?.ToApi(),
                CreatureRules = wild.creatureRules,
                HardMode = level.hardMode,
                SiegeMode = level.siegeMode,
                BossMode = false,
                ScaledRewards = level.rewards,
                WaveStartMessage = level.wildshrine_wave_start_localization,
                WaveEndMessage = level.wildshrine_wave_end_localization,
                EnableCreatureDrops = false,
                CreatureDropOverrides = drop_overrides.Count > 0 ? drop_overrides : null,
                DrawMapOverlay = VFConfig.EnableShrineMapOverlay.Value
            };
        }

        private static PhasedWaveTemplate BuildWaveTemplate(VFChallengeDefinition def)
        {
            // Explicit per-phase mode takes precedence when supplied.
            if (def.ExplicitPhases != null && def.ExplicitPhases.Count > 0)
            {
                return BuildExplicitTemplate(def.ExplicitPhases, def.CreatureRules);
            }

            // Tuned / generated mode.
            if (!supported_biomes.Contains(def.Biome))
            {
                Jotunn.Logger.LogWarning($"VF-API: biome '{def.Biome}' is not supported for generated waves. Supported: {string.Join(", ", supported_biomes)}.");
                return null;
            }

            WaveStyleName wave_style = WaveStyleName.Normal;
            if (!string.IsNullOrEmpty(def.WaveStyle) && !Enum.TryParse(def.WaveStyle, out wave_style))
            {
                Jotunn.Logger.LogWarning($"VF-API: unknown wave style '{def.WaveStyle}', defaulting to Normal.");
                wave_style = WaveStyleName.Normal;
            }

            // A zero max would make the generator reduce waves to almost nothing; fall back to the
            // shrine's configured cap so callers get a sensibly sized wave by default.
            short max_creatures = def.MaxCreaturesPerPhase > 0 ? def.MaxCreaturesPerPhase : VFConfig.ChallengeShrineMaxCreaturesPerWave.Value;

            ChallengeLevelDefinition cld = new ChallengeLevelDefinition
            {
                levelName = "VF_API",
                levelIndex = def.Difficulty,
                numPhases = def.NumPhases > 0 ? def.NumPhases : (short)4,
                levelForShrineTypes = new Dictionary<ShrineType, bool> { { ShrineType.Wild, true } },
                levelMenuLocalization = "",
                requiredGlobalKey = "NONE",
                biome = def.Biome,
                waveFormat = wave_style,
                bossWaveFormat = wave_style,
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "",
                bossLevelWarningLocalization = "",
                onlySelectMonsters = def.OnlySelectMonsters,
                excludeSelectMonsters = def.ExcludeSelectMonsters,
                commonSpawnModifiers = SpawnModifiers.FromApi(def.CommonSpawnModifiers),
                rareSpawnModifiers = SpawnModifiers.FromApi(def.RareSpawnModifiers),
                eliteSpawnModifiers = SpawnModifiers.FromApi(def.EliteSpawnModifiers),
                uniqueSpawnModifiers = new SpawnModifiers(),
                creatureRules = def.CreatureRules
            };

            PhasedWaveTemplate generated = Levels.generateRandomWaveWithOptions(cld, def.HardMode, def.BossMode, def.SiegeMode, max_creatures);
            if (generated?.hordePhases != null && def.FinalPhase != null && def.FinalPhase.Count > 0)
            {
                AppendFinalPhase(generated, def.FinalPhase, def.CreatureRules);
            }
            return generated;
        }

        private static PhasedWaveTemplate BuildExplicitTemplate(List<List<VFHoardEntry>> phases, List<VFCreatureRule> rules)
        {
            List<List<HoardConfig>> horde_phases = new List<List<HoardConfig>>();
            // Entry settings are the most specific, so they are applied after the rules.
            List<KeyValuePair<HoardConfig, VFHoardEntry>> entry_settings = new List<KeyValuePair<HoardConfig, VFHoardEntry>>();
            foreach (List<VFHoardEntry> phase in phases)
            {
                List<HoardConfig> horde_phase = BuildPhase(phase, entry_settings);
                if (horde_phase.Count > 0) { horde_phases.Add(horde_phase); }
            }
            if (horde_phases.Count == 0) { return null; }
            PhasedWaveTemplate template = new PhasedWaveTemplate { hordePhases = horde_phases };
            WaveCreatureSettings.ApplyRules(template, rules);
            ApplyEntrySettings(entry_settings);
            return template;
        }

        // Schema 4: FinalPhase adds one explicit phase after the generated waves (a boss fight, for example).
        // Rules are resolved again over the whole run so wave -1 means that final phase.
        private static void AppendFinalPhase(PhasedWaveTemplate template, List<VFHoardEntry> final_phase, List<VFCreatureRule> rules)
        {
            List<KeyValuePair<HoardConfig, VFHoardEntry>> entry_settings = new List<KeyValuePair<HoardConfig, VFHoardEntry>>();
            List<HoardConfig> horde_phase = BuildPhase(final_phase, entry_settings);
            if (horde_phase.Count == 0) { return; }
            template.hordePhases.Add(horde_phase);
            WaveCreatureSettings.ApplyRules(template, rules);
            ApplyEntrySettings(entry_settings);
        }

        private static List<HoardConfig> BuildPhase(List<VFHoardEntry> phase, List<KeyValuePair<HoardConfig, VFHoardEntry>> entry_settings)
        {
            List<HoardConfig> horde_phase = new List<HoardConfig>();
            if (phase == null) { return horde_phase; }
            foreach (VFHoardEntry entry in phase)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Creature)) { continue; }
                if (entry.Amount <= 0) { continue; }
                if (!Monsters.SpawnableCreatures.ContainsKey(entry.Creature))
                {
                    Jotunn.Logger.LogWarning($"VF-API: explicit creature '{entry.Creature}' is not a known VF creature, skipping. Use GetSpawnableCreatures() for valid names.");
                    continue;
                }
                HoardConfig horde = new HoardConfig
                {
                    creature = entry.Creature,
                    prefab = Monsters.SpawnableCreatures[entry.Creature].prefabName,
                    amount = entry.Amount,
                    stars = entry.Stars
                };
                horde_phase.Add(horde);
                if (entry.Modifiers != null || entry.HealthMultiplier.HasValue) { entry_settings.Add(new KeyValuePair<HoardConfig, VFHoardEntry>(horde, entry)); }
            }
            return horde_phase;
        }

        private static void ApplyEntrySettings(List<KeyValuePair<HoardConfig, VFHoardEntry>> entry_settings)
        {
            foreach (KeyValuePair<HoardConfig, VFHoardEntry> entry in entry_settings)
            {
                WaveCreatureSettings.ApplyModifiers(entry.Key, entry.Value.Modifiers);
                if (entry.Value.HealthMultiplier.HasValue) { entry.Key.healthMultiplier = entry.Value.HealthMultiplier.Value; }
            }
        }

        // Converts the public per-creature drop override map (creature name -> bool) into the short-encoded
        // form stored on the runner ZDO (1 = drops, 0 = no drops). Unknown creature names are skipped with a
        // warning, mirroring the explicit-creature validation above.
        private static Dictionary<string, short> BuildDropOverrides(Dictionary<string, bool> overrides)
        {
            Dictionary<string, short> result = new Dictionary<string, short>();
            if (overrides == null) { return result; }
            foreach (KeyValuePair<string, bool> entry in overrides)
            {
                if (string.IsNullOrEmpty(entry.Key)) { continue; }
                if (!Monsters.SpawnableCreatures.ContainsKey(entry.Key))
                {
                    Jotunn.Logger.LogWarning($"VF-API: creature drop override '{entry.Key}' is not a known VF creature, skipping. Use GetSpawnableCreatures() for valid names.");
                    continue;
                }
                result[entry.Key] = (short)(entry.Value ? 1 : 0);
            }
            return result;
        }

        // Must stay in lockstep with API.Serialize/Deserialize on the caller side (the copied API.cs); JSON
        // strings are the only thing that crosses the soft-dependency boundary, in both directions, so both
        // sides must agree on the format. UseSimpleDictionaryFormat keeps the reward/override maps as plain
        // JSON objects ({"Coins":5}).
        private static readonly DataContractJsonSerializerSettings SerializerSettings =
            new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true };

        private static string Serialize(VFChallengeDefinition def)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(VFChallengeDefinition), SerializerSettings);
                    serializer.WriteObject(ms, def);
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError($"VF-API: failed to serialize challenge definition: {ex}");
                return null;
            }
        }

        private static VFChallengeDefinition Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json)) { return null; }
            try
            {
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(VFChallengeDefinition), SerializerSettings);
                    // Safe cast: an unexpected result yields null (handled by the caller) instead of an
                    // InvalidCastException escaping the deserializer.
                    VFChallengeDefinition def = serializer.ReadObject(ms) as VFChallengeDefinition;
                    if (def == null)
                    {
                        Jotunn.Logger.LogError("VF-API: challenge definition deserialized to null / an unexpected type.");
                        return null;
                    }
                    // Every field added since schema 1 is optional, so an older caller's definition reads fine; only a
                    // caller built against a newer API may be using settings this version does not know.
                    if (def.SchemaVersion > VFChallengeDefinition.CurrentSchemaVersion)
                    {
                        Jotunn.Logger.LogWarning($"VF-API: challenge definition schema version mismatch (received {def.SchemaVersion}, expected {VFChallengeDefinition.CurrentSchemaVersion}). " +
                            "The calling mod was built for a newer Valheim Fortress; settings this version does not know are ignored. Attempting to continue.");
                    }
                    return def;
                }
            }
            catch (Exception ex)
            {
                // Log the full exception chain (ToString includes type, stack, and inner exceptions) so the
                // real DataContractJsonSerializer cause is captured rather than just the outer message.
                Jotunn.Logger.LogError($"VF-API: failed to deserialize challenge definition: {ex}");
                return null;
            }
        }
    }
}
