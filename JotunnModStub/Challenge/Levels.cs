using BepInEx.Configuration;
using Jotunn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimFortress.Challenge
{
    class Levels
    {
        // TODO once these values are someone defined in a sane way and have a reasonable tuning scale, lets make them configurable
        static private float challenge_slope = 0.2f;
        static private short max_creature_stars = 2; // This is the vanilla default, and should never be higher unless other mods support it.
        static private short base_challenge_points = 100;
        static private short base_challenge_points_increase = 10;
        static private short max_challenge_points = 3000;
        static private float star_chance = 0.15f;
        const String COMMON = "common";
        const String RARE = "rare";
        const String UNIQUE = "unique";
        const String MEADOWS = "Meadows";
        const String BLACKFOREST = "BlackForest";
        const String SWAMP = "Swamp";
        const String MOUNTAIN = "Mountain";
        const String PLAINS = "Plains";
        const String MISTLANDS = "Mistlands";
        const String ASHLANDS = "Ashlands";
        static readonly private String[] biomes = { MEADOWS, BLACKFOREST, SWAMP, MOUNTAIN, PLAINS, MISTLANDS, ASHLANDS };


        public class HoardConfig
        {
            public String creature;
            public String prefab;
            public Int16 amount;
            public Int16 stars;

            public HoardConfig(
                String creatureName,
                string prefabName,
                Int16 spawnAmount,
                Int16 starsLevel
                )
            {
                prefab = prefabName;
                creature = creatureName;
                amount = spawnAmount;
                stars = starsLevel;
            }

        }

        // Reference used to build a wave template
        public class WaveOutline
        {
            private Dictionary<String, Tuple<Int16, Int16, Int16, Int16>> waves = new Dictionary<String, Tuple<Int16, Int16, Int16, Int16>> { };

            /// <summary>
            /// Adds a creature to the wave outline, with a total percentage 1-100, and the number of entries that should be broken out from this creature
            /// </summary>
            /// <param name="creature"></param>
            /// <param name="percentage"></param>
            /// <param name="entries"></param>
            public void AddCreatureToWave(String creature, Int16 percentage, Int16 entries, Int16 min_stars = 0, Int16 max_stars =0)
            {
                // Clamp down the values
                // if we end up supporting mods which add more star levels, then this could go up more
                if (percentage > 100) { percentage = 100; }
                if (percentage < 1) { percentage = 1; }
                if (max_stars > 2) { max_stars = 2; }
                if (min_stars > 2) { min_stars = 2; }
                waves.Add(creature, new Tuple<Int16, Int16, Int16, Int16>(percentage, entries, min_stars, max_stars));
            }

            // percent, entries, min_stars, max_stars
            public Dictionary<String, Tuple<Int16, Int16, Int16, Int16>> GetOutline()
            {
                return waves;
            }
            public bool HasKey(String key)
            {
                return waves.ContainsKey(key);
            }
        }

        public class WaveGenerationFormat
        {
            List<Tuple<String, int>> waveFormat = new List<Tuple<string, int>> { };
            
            public void AddToWaveFormat(String spawnType, int percentage)
            {
                waveFormat.Add(new Tuple<string, int>(spawnType, percentage));
            }

            public WaveGenerationFormat AddMultiToWaveFormat(Tuple<String, int>[] entries)
            {
                foreach (Tuple<String, int> entry in entries)
                {
                    this.AddToWaveFormat(entry.Item1, entry.Item2);
                }
                return this;
            }
            
            // Accessor
            public List<Tuple<string, int>> GetWaveFormat()
            {
                return waveFormat;
            }

            public Int16 Count()
            {
                return (Int16)waveFormat.Count();
            }

            public Int16 CountRareSpawns()
            {
                Int16 count = 0;
                foreach(Tuple<String, int> entry in waveFormat )
                {
                    if(entry.Item1 == RARE) { count += 1; }
                    
                }
                return count;
            }

            public List<Int16> GetRareSpawnFormations()
            {
                List<Int16> rareSpawnPercents = new List<Int16>();
                foreach (Tuple<String, int> entry in waveFormat)
                {
                    if (entry.Item1 == RARE) { rareSpawnPercents.Add((Int16)entry.Item2); }

                }
                return rareSpawnPercents;
            }
            public Int16 CountCommonSpawns()
            {
                Int16 count = 0;
                foreach (Tuple<String, int> entry in waveFormat)
                {
                    if (entry.Item1 == COMMON) { count += 1; }

                }
                return count;
            }

            public List<Int16> GetCommonSpawnFormations()
            {
                List<Int16> rareSpawnPercents = new List<Int16>();
                foreach (Tuple<String, int> entry in waveFormat)
                {
                    if (entry.Item1 == COMMON) { rareSpawnPercents.Add((Int16)entry.Item2); }

                }
                return rareSpawnPercents;
            }

            public Int16 CountUniqueSpawns()
            {
                Int16 count = 0;
                foreach (Tuple<String, int> entry in waveFormat)
                {
                    if (entry.Item1 == UNIQUE) { count += 1; }

                }
                return count;
            }
        }

        // Little helper object to contain built out waves
        public class WaveTemplate
        {
            private List<HoardConfig> waves = new List<HoardConfig> {};
            public WaveTemplate() { }

            public void AddHoard(HoardConfig hoard)
            {
                waves.Add(hoard);
            }

            public List<HoardConfig> GetWaves()
            {
                return waves;
            }
        }

        class CreatureValues
        {
            public short spawnCost;
            public String prefabName;
            public String spawnType;
            public String biomeFrom;

            /// <summary>
            /// Defines a summonable creature
            /// </summary>
            /// <param name="spawnCost"></param>
            /// <param name="prefabName"></param>
            /// <param name="spawnType"></param>
            /// <param name="biome"></param>
            public CreatureValues(short spawnCost, String prefabName, String spawnType, String biome)
            {
                this.spawnCost = spawnCost;
                this.prefabName = prefabName;
                this.spawnType = spawnType;
                this.biomeFrom = biome;
            }
        }

        static Dictionary<String, WaveGenerationFormat> WaveStyles = new Dictionary<string, WaveGenerationFormat>
        {
            { "Tutorial", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(COMMON, 50), Tuple.Create(COMMON, 50)}) },
            { "Starter", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(COMMON, 50), Tuple.Create(COMMON, 30), Tuple.Create(COMMON, 20)}) },
            { "Easy", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(RARE, 15), Tuple.Create(COMMON, 30), Tuple.Create(COMMON, 20), Tuple.Create(COMMON, 50)}) },
            { "Normal", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(RARE, 15), Tuple.Create(RARE, 15), Tuple.Create(COMMON, 30), Tuple.Create(COMMON, 20), Tuple.Create(COMMON, 50)}) },
            { "Hard", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(RARE, 10), Tuple.Create(RARE, 10), Tuple.Create(RARE, 10), Tuple.Create(COMMON, 30), Tuple.Create(COMMON, 20), Tuple.Create(COMMON, 50)}) },
            { "Expert", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(RARE, 15), Tuple.Create(RARE, 15), Tuple.Create(RARE, 15), Tuple.Create(COMMON, 30), Tuple.Create(COMMON, 20), Tuple.Create(COMMON, 50)}) },
            { "Boss", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(UNIQUE, 100), Tuple.Create(RARE, 25), Tuple.Create(COMMON, 30), Tuple.Create(COMMON, 20), Tuple.Create(COMMON, 60)}) },
            { "TutorialBoss", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(UNIQUE, 100), Tuple.Create(COMMON, 30), Tuple.Create(COMMON, 40), Tuple.Create(COMMON, 60)}) },
            { "Dynamic", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(RARE, 15), Tuple.Create(RARE, 10), Tuple.Create(RARE, 5), Tuple.Create(COMMON, 30), Tuple.Create(COMMON, 30), Tuple.Create(COMMON, 20), Tuple.Create(COMMON, 20), Tuple.Create(COMMON, 10)}) },
            { "DynamicBoss", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(UNIQUE, 100), Tuple.Create(RARE, 15), Tuple.Create(RARE, 10), Tuple.Create(RARE, 5), Tuple.Create(COMMON, 30), Tuple.Create(COMMON, 30), Tuple.Create(COMMON, 20), Tuple.Create(COMMON, 20), Tuple.Create(COMMON, 10)}) },
        };
        

        static Dictionary<String, CreatureValues> SpawnableCreatures = new Dictionary<string, CreatureValues>
        {
            // Meadow Creatures
            {"Neck", new CreatureValues(spawnCost: 2, "Neck", spawnType: COMMON, biome: MEADOWS) },
            {"Boar", new CreatureValues(spawnCost: 2, "Boar", spawnType: COMMON, biome: MEADOWS) },
            {"Greyling", new CreatureValues(spawnCost: 3, "Greyling", spawnType: COMMON, biome: MEADOWS) },
            // Black Forest Creatures
            {"GreyDwarf", new CreatureValues(spawnCost: 4, "Greydwarf", spawnType: COMMON, biome: BLACKFOREST) },
            {"GreyDwarfBrute", new CreatureValues(spawnCost: 8, "Greydwarf_Elite", spawnType: RARE, biome: BLACKFOREST) },
            {"GreyDwarfShaman", new CreatureValues(spawnCost: 8, "Greydwarf_Shaman", spawnType: RARE, biome: BLACKFOREST) },
            {"Skeleton", new CreatureValues(spawnCost: 4, "Skeleton_NoArcher", spawnType: COMMON, biome: BLACKFOREST) },
            {"SkeletonArcher", new CreatureValues(spawnCost: 5, "Skeleton", spawnType: COMMON, biome: BLACKFOREST) },
            {"RancidSkeleton", new CreatureValues(spawnCost: 9, "Skeleton_Poison", spawnType: RARE, biome: BLACKFOREST) },
            {"Ghost", new CreatureValues(spawnCost: 7, "Greyling", spawnType: RARE, biome: BLACKFOREST) },
            {"Troll", new CreatureValues(spawnCost: 20, "Troll", spawnType: RARE, biome: BLACKFOREST) },
            // Swamp Creatures
            {"Surtling", new CreatureValues(spawnCost: 6, "Surtling", spawnType: COMMON, biome: SWAMP) },
            {"Wraith", new CreatureValues(spawnCost: 10, "Wraith", spawnType: RARE, biome: SWAMP) },
            {"Abomination", new CreatureValues(spawnCost: 30, "Abomination", spawnType: RARE, biome: SWAMP) },
            {"Draugr", new CreatureValues(spawnCost: 10, "Draugr", spawnType: COMMON, biome: SWAMP) },
            {"DraugrArcher", new CreatureValues(spawnCost: 12, "Draugr_Ranged", spawnType: COMMON, biome: SWAMP) },
            {"DraugrElite", new CreatureValues(spawnCost: 15, "Draugr_Elite", spawnType: RARE, biome: SWAMP) },
            {"Blob", new CreatureValues(spawnCost: 7, "Blob", spawnType: COMMON, biome: SWAMP) },
            {"BlobElite", new CreatureValues(spawnCost: 15, "BlobElite", spawnType: RARE, biome: SWAMP) },
            // Mountain Creatures
            {"Bat", new CreatureValues(spawnCost: 3, "Bat", spawnType: COMMON, biome: MOUNTAIN) },
            {"IceDrake", new CreatureValues(spawnCost: 12, "Hatchling", spawnType: RARE, biome: MOUNTAIN) },
            {"Wolf", new CreatureValues(spawnCost: 15, "Wolf", spawnType: COMMON, biome: MOUNTAIN) },
            {"Fenring", new CreatureValues(spawnCost: 18, "Fenring", spawnType: COMMON, biome: MOUNTAIN) },
            {"Ulv", new CreatureValues(spawnCost: 12, "Ulv", spawnType: COMMON, biome: MOUNTAIN) },
            {"Cultist", new CreatureValues(spawnCost: 18, "Fenring_Cultist", spawnType: RARE, biome: MOUNTAIN) },
            {"StoneGolemn", new CreatureValues(spawnCost: 40, "StoneGolem", spawnType: RARE, biome: MOUNTAIN) },
            // Plains Creatures
            {"Deathsquito", new CreatureValues(spawnCost: 20, "Deathsquito", spawnType: COMMON, biome: PLAINS) },
            {"Fuling", new CreatureValues(spawnCost: 15, "Goblin", spawnType: COMMON, biome: PLAINS) },
            {"FulingArcher", new CreatureValues(spawnCost: 17, "GoblinArcher", spawnType: COMMON, biome: PLAINS) },
            {"FulingBerserker", new CreatureValues(spawnCost: 35, "GoblinBrute", spawnType: RARE, biome: PLAINS) },
            {"FulingShaman", new CreatureValues(spawnCost: 25, "GoblinShaman", spawnType: RARE, biome: PLAINS) },
            {"Growth", new CreatureValues(spawnCost: 25, "BlobTar", spawnType: COMMON, biome: PLAINS) },
            // Mistland Creatures
            {"Seeker", new CreatureValues(spawnCost: 30, "Seeker", spawnType: COMMON, biome: MISTLANDS) },
            {"SeekerSoldier", new CreatureValues(spawnCost: 50, "SeekerBrute", spawnType: RARE, biome: MISTLANDS) },
            {"SeekerBrood", new CreatureValues(spawnCost: 10, "SeekerBrood", spawnType: COMMON, biome: MISTLANDS) },
            {"Gjall", new CreatureValues(spawnCost: 50, "Gjall", spawnType: RARE, biome: MISTLANDS) },
            {"Tick", new CreatureValues(spawnCost: 15, "Tick", spawnType: COMMON, biome: MISTLANDS) },
            {"DvergerRouge", new CreatureValues(spawnCost: 25, "Dverger", spawnType: RARE, biome: MISTLANDS) },
            {"DvergerMage", new CreatureValues(spawnCost: 45, "DvergerMage", spawnType: RARE, biome: MISTLANDS) },
            {"DvergerMageFire", new CreatureValues(spawnCost: 45, "DvergerMageFire", spawnType: RARE, biome: MISTLANDS) },
            {"DvergerMageIce", new CreatureValues(spawnCost: 45, "DvergerMageIce", spawnType: RARE, biome: MISTLANDS) },
            {"DvergerMageSupport", new CreatureValues(spawnCost: 45, "DvergerMageSupport", spawnType: RARE, biome: MISTLANDS) },
            // Boss Creatures
            {"Eikthyr", new CreatureValues(spawnCost: 40, "Eikthyr", spawnType: UNIQUE, biome: MEADOWS) },
            {"TheElder", new CreatureValues(spawnCost: 180, "gd_king", spawnType: UNIQUE, biome: BLACKFOREST) },
            {"Bonemass", new CreatureValues(spawnCost: 250, "Bonemass", spawnType: UNIQUE, biome: SWAMP) },
            {"Moder", new CreatureValues(spawnCost: 320, "Dragon", spawnType: UNIQUE, biome: MOUNTAIN) },
            {"Yagluth", new CreatureValues(spawnCost: 450, "GoblinKing", spawnType: UNIQUE, biome: PLAINS) },
            {"TheQueen", new CreatureValues(spawnCost: 600, "SeekerQueen", spawnType: UNIQUE, biome: MISTLANDS) },
        };

        public static void UpdateCreatureConfigValues(VFConfig cfg)
        {

            foreach (KeyValuePair<string, CreatureValues> entry in SpawnableCreatures)
            {
                
                short attempted_spawncost = cfg.BindServerConfig(
                    "shine of challenge - monsters",
                    $"{entry.Key}_creaturevalue",
                    entry.Value.spawnCost,
                    $"cost to spawn a {entry.Key}, smaller values (whole numbers only) will allow more of the creature to spawn per challenge.",
                    true).Value;
                if (attempted_spawncost > 1000 || attempted_spawncost < 0)
                {
                    Jotunn.Logger.LogWarning($"{entry.Key}_creaturevalue={attempted_spawncost}. is not valid, reseting to default ({entry.Value.spawnCost}).");
                }
                else
                {
                    SpawnableCreatures[entry.Key].spawnCost = attempted_spawncost;
                }
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Config {entry.Key}_creaturevalue Added."); }
            }
        }

        public static void UpdateLevelValues(VFConfig cfg)
        {
            
            base_challenge_points = cfg.BindServerConfig(
                "shine of challenge - levels",
                "level_base_challenge_points",
                (short)100,
                "The base number of points that all waves add and multiply from. Lowering this will make all waves easier, increasing it will make all waves harder.",
                false,
                100, 1000).Value;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Config Level base_challenge_points updated."); }


            base_challenge_points_increase = cfg.BindServerConfig(
                "shine of challenge - levels",
                "base_challenge_points_increase",
                (short)10,
                "The base number of points that are added to the base value each wave (level x this value) + base_points. This is the linear difficulty modifier.",
                false, 1, 1000).Value;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Config Level base_challenge_points_increase updated."); }

            max_challenge_points = cfg.BindServerConfig(
                "shine of challenge - levels",
                "max_challenge_points",
                (short)3000,
                "The absolute max number of points a wave can generate with, higher values will be clamped down to this value.",
                false, 1000, 30000).Value;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Config Level max_challenge_points updated."); }

            star_chance = cfg.BindServerConfig(
                "shine of challenge - levels",
                "creature_star_chance",
                0.15f,
                "The absolute max number of points a wave can generate with, higher values will be clamped down to this value.",
                true, 0.0f, 1).Value;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Config creature_star_chance updated."); }

            challenge_slope = cfg.BindServerConfig(
                "shine of challenge - levels",
                "challenge_slope",
                0.2f,
                "The linear regression slope which increases difficulty ",
                false, 0.01f, 5.0f).Value;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Config challenge_slope updated."); }

        }



        // Logarithmic with a cap
        // y = a + b ln x
        // allocated_challenge_points = base_challenge_points + base_challenge_points_increase * computed_slope
        // Computed slope is simply the log of (10 + the challenge slope + level), resulting in a small to larger increase based on the defined challenge slope.
        // Challenge slope should always be positive.
        public static Int16 ComputeChallengePoints(Int16 level)
        {
            Double computed_slope = Math.Log((10 + level) * (1 + challenge_slope));
            Double challenge_increase = (base_challenge_points_increase * level) * computed_slope;
            Int16 allocated_challenge_points = (Int16)(base_challenge_points + challenge_increase);
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Challenge points slope:{computed_slope}, point_increase: {challenge_increase}"); }
            // Cap the challenge points if they are over the defined max
            if (allocated_challenge_points > max_challenge_points)
            {
                allocated_challenge_points = max_challenge_points;
            }

            return allocated_challenge_points;
        }

        public static void generateRandomWaveWithOptions(Int16 level, bool hard_mode, bool boss_mode, bool siege_mode, GameObject shrine)
        {
            Int16 point_estimate = level;
            if (hard_mode) {
                Jotunn.Logger.LogInfo("Hard mode has been enabled, bigger challenge, bigger reward.");
                point_estimate = (Int16)(point_estimate * 1.5);
            }
            if (siege_mode)
            {
                Jotunn.Logger.LogInfo("Siege Mode has been enabled, prepare the ballistas.");
                point_estimate = (Int16)(point_estimate * 1.5);
            }
            if (boss_mode)
            {
                Jotunn.Logger.LogInfo("Boss Mode has been enabled, the forsaken will find you.");
            }
            Int16 wave_total_points = ComputeChallengePoints(point_estimate);
            
            Jotunn.Logger.LogInfo($"Wave Challenge points: {wave_total_points}");
            // Builds out a template for wave generation
            // Enable flag for old style generation?
            // WaveTemplate wavedefinition = getLevelTemplate(level, wave_total_points);
            WaveTemplate wavedefinition = dynamicBuildWaveTemplate(level, wave_total_points, boss_mode);

            // Add the component, and call it
            shrine.AddComponent<Spawner>();
            Spawner spawn_controller = shrine.GetComponent<Spawner>();
            spawn_controller.TrySpawningHoard(wavedefinition.GetWaves(), siege_mode, shrine);
        }

        public static WaveGenerationFormat DecideWaveStyle(Int16 level, bool boss_mode)
        {
            // This provides a gradual ramp up in difficulty as the user progresses through the waves
            // Once the user reaches lvl 16 every wave is fully fleshed out dynamically
            WaveGenerationFormat waveFormat = WaveStyles["Dynamic"];
            if (level == 1) { waveFormat = WaveStyles["Tutorial"]; }
            if (level == 2 || level == 3 || level == 4) { waveFormat = WaveStyles["Starter"]; }
            if (level == 6 || level == 7) { waveFormat = WaveStyles["Easy"]; }
            if (level == 8 || level == 9) { waveFormat = WaveStyles["Normal"]; }
            if (level == 11 || level == 12) { waveFormat = WaveStyles["Hard"]; }
            if (level == 13 || level == 14) { waveFormat = WaveStyles["Expert"]; }
            if (level < 6 && boss_mode) { waveFormat = WaveStyles["TutorialBoss"]; }
            if (level > 6 && boss_mode) { waveFormat = WaveStyles["Boss"]; }
            if (level > 16 && boss_mode) { waveFormat = WaveStyles["DynamicBoss"]; }
            return waveFormat;
        }

        public static WaveTemplate dynamicBuildWaveTemplate(Int16 level, Int16 max_wave_points, bool boss_mode)
        {
            WaveOutline completeWaveOutline = new WaveOutline();


            Int16 max_creatures_from_previous_biomes = 0;
            String selected_biome = MEADOWS;

            // haven't decided if this is worth it or how this should be broken out. Some kind of balance is needed between having only a few monsters from previous biomes
            // and supporting the fact that we can't do previous biome creatures before blackforest, and at each level the creatures need to be stronger to keep up with the current biome
            if (level > 5 && level < 11) { selected_biome = BLACKFOREST; max_creatures_from_previous_biomes += 1; }
            if (level > 10 && level < 16) { selected_biome = SWAMP; max_creatures_from_previous_biomes += 1; }
            if (level > 15 && level < 21) { selected_biome = MOUNTAIN; max_creatures_from_previous_biomes += 2; }
            if (level > 20 && level < 26) { selected_biome = PLAINS; max_creatures_from_previous_biomes += 2; }
            if (level > 25 && level < 31) { selected_biome = MISTLANDS; max_creatures_from_previous_biomes += 3; }
            // if (level > 30 && level < 35) { selected_biome = "Ashlands"; }
            short targeted_wave_biome_level = BiomeStringToInt(selected_biome);
            WaveGenerationFormat WaveGenerationFormat = DecideWaveStyle(level, boss_mode);
            Int16 elites_to_pick = WaveGenerationFormat.CountRareSpawns();
            Int16 commons_to_pick = WaveGenerationFormat.CountCommonSpawns();
            Int16 unqiues_to_pick = WaveGenerationFormat.CountUniqueSpawns();
            Int16 total_creatures_to_select = WaveGenerationFormat.Count();

            Int16 uniques_selected = 0;
            Int16 elites_selected = 0;
            Int16 commons_selected = 0;
            Int16 creatures_selected_from_previous_biome = 0;
            Int16 selection_iterations = 0;

            while ((commons_selected + elites_selected + uniques_selected) < total_creatures_to_select)
            {
                if ((commons_selected + elites_selected) >= total_creatures_to_select) { break; }
                Jotunn.Logger.LogInfo($"Creatures Selected: C-{commons_selected} R-{elites_selected} to select: {total_creatures_to_select}");
                // we shuffle the monster keys so that we run through them in a different order each time
                List<string> this_iteration_key_order = ValheimFortress.shuffleList(new List<string>(SpawnableCreatures.Keys));
                selection_iterations += 1;
                if(selection_iterations > 3) {
                    // Add more search targets in every round if the first few rounds couldn't fill the roster.
                    max_creatures_from_previous_biomes += 1;
                    Jotunn.Logger.LogInfo("Not enough creatures added, expanding search.");
                } 
                foreach (String skey in this_iteration_key_order)
                {

                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Evaluating {skey} eligibility."); }
                    if ((commons_selected + elites_selected + uniques_selected) >= total_creatures_to_select) { break; } // break out early if we got what we came for     
                    if (completeWaveOutline.HasKey(skey)) {
                        if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"{skey} creature already added, skipping iteration."); }
                        continue; 
                    } // we only want to add a key once, besides the entries are split later
                    short current_creature_biome_level = BiomeStringToInt(SpawnableCreatures[skey].biomeFrom);
                    // Skip this entry if the creature does not match the selected biome and we don't need anymore creatures from previous biomes
                    if (SpawnableCreatures[skey].biomeFrom != selected_biome && max_creatures_from_previous_biomes > creatures_selected_from_previous_biome)
                    {
                        if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Creature is from a different biome."); }
                        if (SpawnableCreatures[skey].biomeFrom != selected_biome && current_creature_biome_level < targeted_wave_biome_level && (current_creature_biome_level + 2) >= targeted_wave_biome_level)
                        {
                            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Creature biome is within -2 of current."); }
                            // The creature is from a different biome than the target AND we still need creatures from other biomes
                            // The creature is also within -2 of the current targeted biome for generation
                            if (SpawnableCreatures[skey].spawnType == UNIQUE) { continue; } // We only add bosses for the current biome
                            // might want to support dynamic boss waves using bosses from a different biome
                            if (SpawnableCreatures[skey].spawnType == RARE && elites_selected < elites_to_pick)
                            {
                                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adding as prior biome rare type creature."); }
                                // we only add previous biomes elites with 1 cohort, meaning they will all have the same level characteristics, which is innately boosted by their biome level difference.
                                short creature_stars = (short)(targeted_wave_biome_level - current_creature_biome_level);
                                short percent = WaveGenerationFormat.GetRareSpawnFormations()[elites_selected];
                                completeWaveOutline.AddCreatureToWave(skey, percent, 1, min_stars: creature_stars, max_stars: max_creature_stars);
                                elites_selected += 1;
                                creatures_selected_from_previous_biome += 1;
                                continue;
                            }
                            if (SpawnableCreatures[skey].spawnType == COMMON && commons_selected < commons_to_pick)
                            {
                                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adding as prior biome common type creature."); }
                                // boost the creatures star level relative to how far back the biome is it was from 1-2
                                short creature_stars = (short)(targeted_wave_biome_level - current_creature_biome_level);
                                short percent = WaveGenerationFormat.GetCommonSpawnFormations()[commons_selected];
                                completeWaveOutline.AddCreatureToWave(skey, percent, 1, min_stars: creature_stars, max_stars: max_creature_stars);
                                commons_selected += 1;
                                creatures_selected_from_previous_biome += 1;
                                continue;
                            }
                            
                        }
                        else
                        {
                            continue; // Biome is higher level than the target biome, won't get creatures from it
                        }

                    }

                    // we only want to add creatures from the current biome below here.
                    if(SpawnableCreatures[skey].biomeFrom != selected_biome) { continue; }
                    // Creature is from this biome
                    if (SpawnableCreatures[skey].spawnType == COMMON && commons_selected < commons_to_pick)
                    {
                        if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adding as current biome common type creature."); }
                        // if this a common type enemy from the current biome we leave its max stars at 1, but generate it in 3 cohorts, each with a fractional chance to get stars.
                        short percent = WaveGenerationFormat.GetCommonSpawnFormations()[commons_selected];
                        completeWaveOutline.AddCreatureToWave(skey, (short)(percent / 3), 3, min_stars: 0, max_stars: 1);
                        commons_selected += 1;
                        continue;
                    }
                    if (SpawnableCreatures[skey].spawnType == RARE && elites_selected < elites_to_pick)
                    {
                        if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adding as current biome rare type creature."); }
                        // Rare type enemy from the current biome, generates in 1 cohort
                        short percent = WaveGenerationFormat.GetRareSpawnFormations()[elites_selected];
                        completeWaveOutline.AddCreatureToWave(skey, percent, 1, min_stars: 0, max_stars: max_creature_stars);
                        elites_selected += 1;
                        continue;
                    }
                    if (SpawnableCreatures[skey].spawnType == UNIQUE && uniques_selected < unqiues_to_pick)
                    {
                        if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adding as current biome unique type creature."); }
                        // Bosses always only generate one instance of themselves (for now)
                        // Bosses don't currently get stars
                        completeWaveOutline.AddCreatureToWave(skey, 100, 1, min_stars: 0, max_stars: 0);
                        uniques_selected += 1;
                        continue;
                    }
                }
            }

            WaveTemplate finalizedHordes = new WaveTemplate();
            foreach(KeyValuePair<String, Tuple<Int16, Int16, Int16, Int16>> waveEntry in completeWaveOutline.GetOutline())
            {
                Int16 cohorts_created = 0;
                while (waveEntry.Value.Item2 > cohorts_created)
                {
                    Int16 creatureStars = 0;
                    // There is positive variance in the potential stars for this creature, we will roll to see if it gets upgraded.
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Checking if entry can be upgraded: min-{waveEntry.Value.Item4} max-{waveEntry.Value.Item3}"); }
                    if (waveEntry.Value.Item3 > 0) { creatureStars = waveEntry.Value.Item3; }
                    if (waveEntry.Value.Item4 > waveEntry.Value.Item3)
                    {
                        // Random value between 0-1.
                        float upgradeRoll = UnityEngine.Random.value;
                        if (upgradeRoll > (1 - star_chance))
                        {
                            creatureStars += 1;
                            // Upgrade successful, now can it be upgraded again?
                            if (waveEntry.Value.Item4 > (waveEntry.Value.Item3 + 1))
                            {
                                float secondUpgradeRoll = UnityEngine.Random.value;
                                if (secondUpgradeRoll > (1 - star_chance))
                                {
                                    creatureStars += 1;
                                }
                            }
                        }
                    }

                    // Add a small amount of noise to the number of creatures that will spawn from a given section
                    float number_to_spawn = max_wave_points * (waveEntry.Value.Item2 / 100f);
                    float number_to_spawn_variance = number_to_spawn * 0.10f;
                    float spawn_amount = UnityEngine.Random.Range((number_to_spawn - number_to_spawn_variance), (number_to_spawn + number_to_spawn_variance));
                    if (SpawnableCreatures[waveEntry.Key].spawnType == UNIQUE) { spawn_amount = 1; } // uniques only spawn one

                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"number_to_spawn={number_to_spawn} variance={number_to_spawn_variance} spawn_amount={spawn_amount}"); }
                    if (spawn_amount < 1) { spawn_amount = 1; } // safety to ensure we alwasy spawn at least one.
                    finalizedHordes.AddHoard(new HoardConfig(waveEntry.Key, SpawnableCreatures[waveEntry.Key].prefabName, (short)spawn_amount, creatureStars));
                    cohorts_created += 1;
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Cohort {cohorts_created} created: {waveEntry.Key} A:{spawn_amount} S:{creatureStars}"); }
                }
                
            }

            Jotunn.Logger.LogInfo("Built wave definition.");
            foreach (Levels.HoardConfig hoard in finalizedHordes.GetWaves())
            {
                Jotunn.Logger.LogInfo($"Hoard {hoard.creature} - Amount: {hoard.amount} Stars: {hoard.stars}");
            }


            return finalizedHordes;
        }


        public static Int16 BiomeStringToInt(String biome)
        {
            if(!biomes.Contains(biome)) { throw new ArgumentException($"Biome {biome} does not match defined biomes: {biomes}"); }
            return (Int16)Array.IndexOf(biomes, biome);
        }

        public static String BiomeIntToString(Int16 biome_index)
        {
            return biomes[biome_index];
        }
    }
}
