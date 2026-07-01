using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;

namespace ValheimFortress
{
    /// <summary>
    /// Public API for Valheim Fortress. Copy this file into your project and declare a soft dependency
    /// on Valheim Fortress to drive Fortress-style horde challenges from your own mod:
    /// <code>[BepInDependency("MidnightsFX.ValheimFortress", BepInDependency.DependencyFlags.SoftDependency)]</code>
    /// All access is through reflection, so your plugin never needs a hard reference to the Valheim
    /// Fortress assembly. Check <see cref="IsAvailable"/> before calling anything else.
    /// Requires a reference to the framework assembly <c>System.Runtime.Serialization</c>.
    /// </summary>
    public static class API
    {
        private static readonly Type APIReceiver;
        private static readonly MethodInfo RunChallengeMethod;
        private static readonly MethodInfo GetSpawnableCreaturesMethod;
        private static readonly MethodInfo GetRewardItemsMethod;
        private static readonly MethodInfo GetWaveStylesMethod;

        /// <summary>True when Valheim Fortress is loaded and the API is callable.</summary>
        public static bool IsAvailable => APIReceiver != null;

        static API()
        {
            APIReceiver = Type.GetType("ValheimFortress.APIReceiver, ValheimFortress");
            if (APIReceiver == null) return;
            RunChallengeMethod = APIReceiver.GetMethod("RunChallenge", BindingFlags.Public | BindingFlags.Static);
            GetSpawnableCreaturesMethod = APIReceiver.GetMethod("GetSpawnableCreatures", BindingFlags.Public | BindingFlags.Static);
            GetRewardItemsMethod = APIReceiver.GetMethod("GetRewardItems", BindingFlags.Public | BindingFlags.Static);
            GetWaveStylesMethod = APIReceiver.GetMethod("GetWaveStyles", BindingFlags.Public | BindingFlags.Static);
        }

        /// <summary>
        /// Runs a wave-based challenge. Creatures emerge from <paramref name="spawnPoints"/> (one is
        /// chosen at random per creature; supply as many as you like for variety), waves advance as creatures
        /// die, and when the final phase is cleared the configured rewards are dropped at
        /// <paramref name="rewardLocation"/>. The runner is networked and self-destroys on completion.
        ///
        /// Must be called in-world. Like the physical shrines this is owner-authoritative, so call it on
        /// the instance that should drive the run (the server in dedicated setups, or the host).
        /// </summary>
        /// <param name="definition">The wave/creature/reward definition (see <see cref="VFChallengeDefinition"/>).</param>
        /// <param name="spawnPoints">World positions creatures spawn from. At least one is required.</param>
        /// <param name="rewardLocation">World position where rewards are dropped on completion.</param>
        /// <returns>True if the challenge was successfully started.</returns>
        public static bool RunChallenge(VFChallengeDefinition definition, Vector3[] spawnPoints, Vector3 rewardLocation)
        {
            if (RunChallengeMethod == null || definition == null) { return false; }
            string json = Serialize(definition);
            if (json == null) { return false; }
            return (bool)RunChallengeMethod.Invoke(null, new object[] { json, spawnPoints, rewardLocation });
        }

        /// <summary>Convenience overload accepting a <see cref="List{T}"/> of spawn points.</summary>
        public static bool RunChallenge(VFChallengeDefinition definition, List<Vector3> spawnPoints, Vector3 rewardLocation)
        {
            return RunChallenge(definition, spawnPoints?.ToArray(), rewardLocation);
        }

        /// <summary>
        /// Returns the names of every spawnable creature. These are the valid values for
        /// <see cref="VFHoardEntry.Creature"/> and for the monster include/exclude lists.
        /// </summary>
        public static List<string> GetSpawnableCreatures()
        {
            if (GetSpawnableCreaturesMethod == null) { return new List<string>(); }
            return (List<string>)GetSpawnableCreaturesMethod.Invoke(null, null);
        }

        /// <summary>Returns the item prefab names valid as reward keys.</summary>
        public static List<string> GetRewardItems()
        {
            if (GetRewardItemsMethod == null) { return new List<string>(); }
            return (List<string>)GetRewardItemsMethod.Invoke(null, null);
        }

        /// <summary>Returns the available wave-style names (valid values for <see cref="VFChallengeDefinition.WaveStyle"/>).</summary>
        public static List<string> GetWaveStyles()
        {
            if (GetWaveStylesMethod == null) { return new List<string>(); }
            return (List<string>)GetWaveStylesMethod.Invoke(null, null);
        }

        private static string Serialize(VFChallengeDefinition definition)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(VFChallengeDefinition));
                    serializer.WriteObject(ms, definition);
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Defines a Fortress challenge: wave/creature selection and rewards in one place (the way WildShrines
    /// bundle them). There are two authoring modes:
    /// <list type="bullet">
    /// <item><b>Tuned / generated</b> (default): set <see cref="Biome"/>, <see cref="Difficulty"/>,
    /// <see cref="WaveStyle"/>, optional monster filters and <see cref="NumPhases"/>; Valheim Fortress
    /// generates the creature waves.</item>
    /// <item><b>Explicit</b>: set <see cref="ExplicitPhases"/> to control the exact creatures, counts and
    /// stars per phase. When present this overrides the tuned fields.</item>
    /// </list>
    /// Rewards can be auto-scaled, fixed, or both.
    /// </summary>
    public class VFChallengeDefinition
    {
        // ---- Tuned / generated mode (used when ExplicitPhases is null/empty) ----

        /// <summary>Biome whose creatures the wave is generated from. Supported: Meadows, BlackForest,
        /// Swamp, Mountain, Plains, Mistlands, AshLands.</summary>
        public Heightmap.Biome Biome { get; set; } = Heightmap.Biome.Meadows;

        /// <summary>Difficulty/level index. Higher means more wave points and larger rewards.</summary>
        public short Difficulty { get; set; } = 1;

        /// <summary>Wave-style name controlling the common/rare/elite mix (see <see cref="API.GetWaveStyles"/>).
        /// Defaults to "Normal" if empty or unrecognized.</summary>
        public string WaveStyle { get; set; } = "Normal";

        /// <summary>Number of wave phases to generate. Defaults to 4 when left at 0.</summary>
        public short NumPhases { get; set; } = 4;

        /// <summary>Optional cap on creatures generated per phase. 0 uses the Fortress default cap.</summary>
        public short MaxCreaturesPerPhase { get; set; } = 0;

        /// <summary>Optional whitelist of creature names; when set, only these creatures are generated.</summary>
        public List<string> OnlySelectMonsters { get; set; }

        /// <summary>Optional blacklist of creature names to exclude from generation.</summary>
        public List<string> ExcludeSelectMonsters { get; set; }

        /// <summary>Increases wave points and rewards.</summary>
        public bool HardMode { get; set; } = false;

        /// <summary>Uses the boss wave style and applies the boss reward multiplier.</summary>
        public bool BossMode { get; set; } = false;

        /// <summary>Doubles the number of phases and applies the siege reward multiplier.</summary>
        public bool SiegeMode { get; set; } = false;

        // ---- Creature behavior ----

        /// <summary>Global toggle: when true, challenge creatures drop their normal loot on death. Defaults
        /// to false, so (like the physical shrines) challenge creatures drop nothing and players are rewarded
        /// only via the configured challenge rewards. Can be overridden per creature via
        /// <see cref="CreatureDropOverrides"/>.</summary>
        public bool EnableCreatureDrops { get; set; } = false;

        /// <summary>Optional per-creature loot-drop overrides, keyed by creature name (see
        /// <see cref="API.GetSpawnableCreatures"/>). A creature listed here uses its mapped value instead of
        /// <see cref="EnableCreatureDrops"/> (true = drops, false = no drops); creatures not listed fall back
        /// to the global value. Unknown creature names are ignored.</summary>
        public Dictionary<string, bool> CreatureDropOverrides { get; set; }

        // ---- Explicit mode (overrides the tuned fields when set) ----

        /// <summary>Explicit per-phase creature lists. Each inner list is one phase. When non-empty this
        /// overrides the tuned/generated fields above.</summary>
        public List<List<VFHoardEntry>> ExplicitPhases { get; set; }

        // ---- Rewards (either, both, or neither) ----

        /// <summary>Auto-scaled rewards. Keys are item prefab names, values are the per-unit point cost;
        /// the spawned amount scales with difficulty, modes and nearby-player count.</summary>
        public Dictionary<string, short> ScaledRewards { get; set; }

        /// <summary>Fixed rewards. Keys are item prefab names, values are the exact counts to spawn.</summary>
        public Dictionary<string, short> FixedRewards { get; set; }

        // ---- Messaging (optional) ----

        /// <summary>Optional message shown to nearby players when the challenge begins (supports $localization keys).</summary>
        public string WaveStartMessage { get; set; }

        /// <summary>Optional message shown to nearby players when the challenge completes (supports $localization keys).</summary>
        public string WaveEndMessage { get; set; }

        /// <summary>Optional phrases shown to nearby players during each between-wave pause. Each entry supports
        /// a $localization key or literal text. When non-empty this replaces the built-in phrase pool for this
        /// run; when null/empty the built-in pool is used. Selection order is controlled by
        /// <see cref="OrderedPhrases"/>.</summary>
        public List<string> BetweenWavePhrases { get; set; }

        /// <summary>Controls how <see cref="BetweenWavePhrases"/> are selected. When true, phrases play in list
        /// order (wrapping when there are more pauses than phrases); when false (default), a phrase is picked at
        /// random each pause.</summary>
        public bool OrderedPhrases { get; set; } = false;

        // ---- Presentation (optional) ----

        /// <summary>When true, each spawn point is marked on the minimap for the duration of the run (cleared
        /// automatically when the challenge finishes) using the vanilla event markers -- the shaded event-area
        /// circle plus the animated event icon -- the same markers the physical shrines and SLS raids use. The
        /// markers are client-local and cosmetic: they only render for the instance that drives the run (the
        /// caller in single-player or on a P2P host), and are skipped on a headless dedicated server. Defaults
        /// to false.</summary>
        public bool DrawMapOverlay { get; set; } = false;
    }

    /// <summary>A single creature entry in an explicit wave phase.</summary>
    public class VFHoardEntry
    {
        /// <summary>The creature name (one of <see cref="API.GetSpawnableCreatures"/>).</summary>
        public string Creature { get; set; }

        /// <summary>How many of this creature to spawn in the phase.</summary>
        public short Amount { get; set; }

        /// <summary>Star level (0 = no stars).</summary>
        public short Stars { get; set; }
    }
}
