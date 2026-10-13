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
            if (ZNetScene.instance == null)
            {
                Jotunn.Logger.LogWarning("VF-API: RunChallenge called with no world loaded (ZNetScene is null).");
                return false;
            }
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Jotunn.Logger.LogWarning("VF-API: RunChallenge requires at least one spawn point.");
                return false;
            }

            VFChallengeDefinition def = Deserialize(definitionJson);
            if (def == null)
            {
                Jotunn.Logger.LogWarning("VF-API: RunChallenge could not parse the supplied challenge definition.");
                return false;
            }

            WaveCreatureSettings.Validate(def.CreatureRules, "VF-API:");

            // Spawner.SpawnHorde randomly indexes into whatever spawn locations it's given, so any number
            // (>= 1, already validated above) is fine; creatures emerge from a randomly chosen point per spawn.
            PhasedWaveTemplate waves = BuildWaveTemplate(def);
            if (waves == null || waves.hordePhases == null || waves.hordePhases.Count == 0)
            {
                Jotunn.Logger.LogWarning("VF-API: RunChallenge produced an empty wave template; nothing to run.");
                return false;
            }

            GameObject prefab = PrefabManager.Instance.GetPrefab(ValheimFortress.ApiChallengeRunnerPrefab);
            if (prefab == null)
            {
                Jotunn.Logger.LogError($"VF-API: runner prefab '{ValheimFortress.ApiChallengeRunnerPrefab}' was not found.");
                return false;
            }

            GameObject runner = UnityEngine.Object.Instantiate(prefab, rewardLocation, Quaternion.identity);
            ExternalShrine shrine = runner.GetComponent<ExternalShrine>();
            if (shrine == null)
            {
                Jotunn.Logger.LogError("VF-API: runner prefab is missing its ExternalShrine component.");
                UnityEngine.Object.Destroy(runner);
                return false;
            }

            Dictionary<string, short> creature_drop_overrides = BuildDropOverrides(def.CreatureDropOverrides);

            shrine.BeginApiChallenge(waves, spawnPoints, rewardLocation, def.ScaledRewards, def.FixedRewards,
                def.Difficulty, def.HardMode, def.BossMode, def.SiegeMode, def.EnableCreatureDrops, creature_drop_overrides,
                def.WaveStartMessage, def.WaveEndMessage, def.DrawMapOverlay, def.BetweenWavePhrases, def.OrderedPhrases);

            Jotunn.Logger.LogInfo($"VF-API: started challenge with {waves.hordePhases.Count} phases at {rewardLocation}.");
            return true;
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

            return Levels.generateRandomWaveWithOptions(cld, def.HardMode, def.BossMode, def.SiegeMode, max_creatures);
        }

        private static PhasedWaveTemplate BuildExplicitTemplate(List<List<VFHoardEntry>> phases, List<VFCreatureRule> rules)
        {
            List<List<HoardConfig>> horde_phases = new List<List<HoardConfig>>();
            // Entry modifiers are the most specific setting, so they are applied after the rules.
            List<KeyValuePair<HoardConfig, VFModifierSet>> entry_modifiers = new List<KeyValuePair<HoardConfig, VFModifierSet>>();
            foreach (List<VFHoardEntry> phase in phases)
            {
                if (phase == null) { continue; }
                List<HoardConfig> horde_phase = new List<HoardConfig>();
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
                    if (entry.Modifiers != null) { entry_modifiers.Add(new KeyValuePair<HoardConfig, VFModifierSet>(horde, entry.Modifiers)); }
                }
                if (horde_phase.Count > 0) { horde_phases.Add(horde_phase); }
            }
            if (horde_phases.Count == 0) { return null; }
            PhasedWaveTemplate template = new PhasedWaveTemplate { hordePhases = horde_phases };
            WaveCreatureSettings.ApplyRules(template, rules);
            foreach (KeyValuePair<HoardConfig, VFModifierSet> entry in entry_modifiers)
            {
                WaveCreatureSettings.ApplyModifiers(entry.Key, entry.Value);
            }
            return template;
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
                    if (def.SchemaVersion != VFChallengeDefinition.CurrentSchemaVersion)
                    {
                        Jotunn.Logger.LogWarning($"VF-API: challenge definition schema version mismatch (received {def.SchemaVersion}, expected {VFChallengeDefinition.CurrentSchemaVersion}). " +
                            "The calling mod's copy of API.cs is out of sync with this Valheim Fortress version; re-copy API.cs from the current release. Attempting to continue.");
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
