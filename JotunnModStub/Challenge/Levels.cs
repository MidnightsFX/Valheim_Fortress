using BepInEx.Configuration;
using Jotunn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using static Heightmap;
using System.Net;
using System.Runtime.Serialization;
using YamlDotNet.Core.Tokens;
using static UnityEngine.EventSystems.EventTrigger;

namespace ValheimFortress.Challenge
{
    class Levels
    {
        static private float challenge_slope = 0.5f;
        static private float chance_of_prior_biome_creature = 0.05f;
        static private short max_creature_stars = 2; // This is the vanilla default, and should never be higher unless other mods support it.
        static private short base_challenge_points = 100;
        static private short max_creatures_per_wave = 60;
        static private short max_challenge_points = 3000;
        static private float star_chance = 0.15f;
        const String COMMON = CONST.COMMON;
        const String RARE = CONST.RARE;
        const String ELITE = CONST.ELITE;
        const String UNIQUE = CONST.UNIQUE;
        const String MEADOWS = CONST.MEADOWS;
        const String BLACKFOREST = CONST.BLACKFOREST;
        const String SWAMP = CONST.SWAMP;
        const String MOUNTAIN = CONST.MOUNTAIN;
        const String PLAINS = CONST.PLAINS;
        const String MISTLANDS = CONST.MISTLANDS;
        const String ASHLANDS = CONST.ASHLANDS;
        static readonly private String[] biomes = { MEADOWS, BLACKFOREST, SWAMP, MOUNTAIN, PLAINS, MISTLANDS, ASHLANDS };
        static readonly private String[] spawntypes = { COMMON, RARE, ELITE, UNIQUE };

        // Reference used to build a wave template
        public class WaveOutline
        {
            List<String> selectedCreatures = new List<string>();
            List<HoardConfig> commonCreatures = new List<HoardConfig>();
            List<HoardConfig> rareCreatures = new List<HoardConfig>();
            List<HoardConfig> eliteCreatures = new List<HoardConfig>();
            List<HoardConfig> uniqueCreatures = new List<HoardConfig>();

            /// <summary>
            /// Adds a creature to the wave outline, with a total percentage 1-100, and the number of entries that should be broken out from this creature
            /// </summary>
            /// <param name="creature"></param>
            /// <param name="percentage"></param>
            /// <param name="entries"></param>
            public void AddCreatureToWave(String creature, int total_points, float percentage, short min_stars = 0, short max_stars =0)
            {
                short stars = DetermineCreatureStars(min_stars, max_stars);
                short creature_spawn_amount = DetermineCreatureSpawnAmount(creature, total_points, percentage);
                switch (SpawnableCreatures[creature].spawnType)
                {
                    case COMMON:
                        commonCreatures.Add(new HoardConfig(creature, SpawnableCreatures[creature].prefabName, creature_spawn_amount, stars));
                        selectedCreatures.Add(creature);
                        break;
                    case RARE:
                        rareCreatures.Add(new HoardConfig(creature, SpawnableCreatures[creature].prefabName, creature_spawn_amount, stars));
                        selectedCreatures.Add(creature);
                        break;
                    case ELITE:
                        eliteCreatures.Add(new HoardConfig(creature, SpawnableCreatures[creature].prefabName, creature_spawn_amount, stars));
                        selectedCreatures.Add(creature);
                        break;
                    case UNIQUE:
                        uniqueCreatures.Add(new HoardConfig(creature, SpawnableCreatures[creature].prefabName, creature_spawn_amount, stars));
                        selectedCreatures.Add(creature);
                        break;

                }
            }

            public List<HoardConfig> getCommonCreatures()
            {
                return commonCreatures;
            }
            public List<HoardConfig> getRareCreatures()
            {
                return rareCreatures;
            }
            public List<HoardConfig> getEliteCreatures()
            {
                return eliteCreatures;
            }
            public List<HoardConfig> getUniqueCreatures()
            {
                return uniqueCreatures;
            }

            public bool HasKey(String key)
            {
                return selectedCreatures.Contains(key);
            }
            public int GetCount() { return selectedCreatures.Count();}
        }

        // These should all stay as close as possible to 100% totals
        // There is no rule they can't go over/under, but going over will spawn more enemies than the waves points would normally allocate
        // This is normally only the case for challenge levels and bosses etc
        // Tutorial 60%
        // Starter 65%
        // Easy 70%
        // Normal 75%
        // Hard 80%
        // VeryHard 85%
        // Expert 90%
        // Extreme 95%
        // Dynamic 100%
        static Dictionary<String, WaveGenerationFormat> WaveStyles = new Dictionary<string, WaveGenerationFormat>
        {
            // Standard waves
            { "Tutorial", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(COMMON, 30), Tuple.Create(COMMON, 30)}) },
            { "Starter", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(COMMON, 25), Tuple.Create(COMMON, 25), Tuple.Create(COMMON, 15)}) },
            { "Easy", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(RARE, 15), Tuple.Create(COMMON, 25), Tuple.Create(COMMON, 30)}) },
            { "Normal", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(RARE, 15), Tuple.Create(RARE, 10), Tuple.Create(COMMON, 20), Tuple.Create(COMMON, 30)}) },
            { "Hard", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(ELITE, 5), Tuple.Create(RARE, 20), Tuple.Create(COMMON, 20), Tuple.Create(COMMON, 30)}) },
            { "VeryHard", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(ELITE, 10), Tuple.Create(RARE, 25), Tuple.Create(COMMON, 20), Tuple.Create(COMMON, 30)}) },
            { "Expert", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(ELITE, 10), Tuple.Create(RARE, 15), Tuple.Create(RARE, 15), Tuple.Create(COMMON, 20), Tuple.Create(COMMON, 30)}) },
            { "Extreme", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(ELITE, 15), Tuple.Create(RARE, 20), Tuple.Create(RARE, 15), Tuple.Create(COMMON, 20), Tuple.Create(COMMON, 25)}) },
            { "Dynamic", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(ELITE, 15), Tuple.Create(RARE, 25), Tuple.Create(RARE, 15), Tuple.Create(COMMON, 20), Tuple.Create(COMMON, 25)}) },
            // Boss waves
            { "TutorialBoss", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(UNIQUE, 100), Tuple.Create(COMMON, 30), Tuple.Create(COMMON, 40)}) },
            { "EasyBoss", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(UNIQUE, 100), Tuple.Create(RARE, 30), Tuple.Create(COMMON, 30)}) },
            { "Boss", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(UNIQUE, 100), Tuple.Create(ELITE, 20), Tuple.Create(RARE, 30), Tuple.Create(COMMON, 25)}) },
            { "DynamicBoss", new WaveGenerationFormat().AddMultiToWaveFormat(new Tuple<string, int>[]{Tuple.Create(UNIQUE, 100), Tuple.Create(ELITE, 20), Tuple.Create(RARE, 20), Tuple.Create(RARE, 20), Tuple.Create(COMMON, 20), Tuple.Create(COMMON, 20)}) },
        };
        

        public static Dictionary<String, CreatureValues> SpawnableCreatures = new Dictionary<string, CreatureValues>
        {
            // Meadow Creatures
            {"Neck", new CreatureValues { spawnCost = 2, prefabName = "Neck", spawnType = COMMON, biome = MEADOWS, enabled = true, dropsEnabled = false } },
            {"Boar", new CreatureValues {spawnCost = 2, prefabName = "Boar", spawnType = COMMON, biome = MEADOWS, enabled = true, dropsEnabled = false } },
            {"Greyling", new CreatureValues {spawnCost = 3, prefabName = "Greyling", spawnType = COMMON, biome = MEADOWS, enabled = true, dropsEnabled = false } },
            // Black Forest Creatures
            {"GreyDwarf", new CreatureValues {spawnCost = 4, prefabName = "Greydwarf", spawnType = COMMON, biome = BLACKFOREST, enabled = true, dropsEnabled = false } },
            {"GreyDwarfBrute", new CreatureValues {spawnCost = 8, prefabName = "Greydwarf_Elite", spawnType = RARE, biome = BLACKFOREST, enabled = true, dropsEnabled = false } },
            {"GreyDwarfShaman", new CreatureValues {spawnCost = 8, prefabName = "Greydwarf_Shaman", spawnType = RARE, biome = BLACKFOREST, enabled = true, dropsEnabled = false } },
            {"Skeleton", new CreatureValues {spawnCost = 4, prefabName = "Skeleton_NoArcher", spawnType = COMMON, biome = BLACKFOREST, enabled = true, dropsEnabled = false } },
            {"SkeletonArcher", new CreatureValues {spawnCost = 5, prefabName = "Skeleton", spawnType = COMMON, biome = BLACKFOREST, enabled = true, dropsEnabled = false } },
            {"RancidSkeleton", new CreatureValues {spawnCost = 9, prefabName = "Skeleton_Poison", spawnType = RARE, biome = BLACKFOREST, enabled = true, dropsEnabled = false } },
            {"Ghost", new CreatureValues {spawnCost = 7, prefabName = "Ghost", spawnType = RARE, biome = BLACKFOREST, enabled = true, dropsEnabled = false } },
            {"Troll", new CreatureValues {spawnCost = 20, prefabName = "Troll", spawnType = ELITE, biome = BLACKFOREST, enabled = true, dropsEnabled = false } },
            // Swamp Creatures
            {"Surtling", new CreatureValues {spawnCost = 6, prefabName = "Surtling", spawnType = RARE, biome = SWAMP, enabled = true, dropsEnabled = false} },
            {"Wraith", new CreatureValues {spawnCost = 10, prefabName = "Wraith", spawnType = RARE, biome = SWAMP, enabled = true, dropsEnabled = false} },
            {"Abomination", new CreatureValues {spawnCost = 30, prefabName = "Abomination", spawnType = ELITE, biome = SWAMP, enabled = true, dropsEnabled = false} },
            {"Draugr", new CreatureValues {spawnCost = 10, prefabName = "Draugr", spawnType = COMMON, biome = SWAMP, enabled = true, dropsEnabled = false} },
            {"DraugrArcher", new CreatureValues {spawnCost = 20, prefabName = "Draugr_Ranged", spawnType = RARE, biome = SWAMP, enabled = true, dropsEnabled = false} },
            {"DraugrElite", new CreatureValues {spawnCost = 15, prefabName = "Draugr_Elite", spawnType = RARE, biome = SWAMP, enabled = true, dropsEnabled = false} },
            {"Blob", new CreatureValues {spawnCost = 7, prefabName = "Blob", spawnType = COMMON, biome = SWAMP, enabled = true, dropsEnabled = false} },
            {"BlobElite", new CreatureValues {spawnCost = 15, prefabName = "BlobElite", spawnType = RARE, biome = SWAMP, enabled = true, dropsEnabled = false} },
            // Mountain Creatures
            {"Bat", new CreatureValues {spawnCost = 3, prefabName = "Bat", spawnType = COMMON, biome = MOUNTAIN, enabled = false, dropsEnabled = false} },
            {"IceDrake", new CreatureValues {spawnCost = 25, prefabName = "Hatchling", spawnType = RARE, biome = MOUNTAIN, enabled = true, dropsEnabled = false} },
            {"Wolf", new CreatureValues {spawnCost = 18, prefabName = "Wolf", spawnType = COMMON, biome = MOUNTAIN, enabled = true, dropsEnabled = false} },
            {"Fenring", new CreatureValues {spawnCost = 28, prefabName = "Fenring", spawnType = COMMON, biome = MOUNTAIN, enabled = true, dropsEnabled = false} },
            {"Ulv", new CreatureValues {spawnCost = 20, prefabName = "Ulv", spawnType = COMMON, biome = MOUNTAIN, enabled = true, dropsEnabled = false} },
            {"Cultist", new CreatureValues {spawnCost = 40, prefabName = "Fenring_Cultist", spawnType = RARE, biome = MOUNTAIN, enabled = true, dropsEnabled = false} },
            {"StoneGolem", new CreatureValues {spawnCost = 50, prefabName = "StoneGolem", spawnType = ELITE, biome = MOUNTAIN, enabled = true, dropsEnabled = false} },
            // Plains Creatures
            {"Deathsquito", new CreatureValues {spawnCost = 20, prefabName = "Deathsquito", spawnType = COMMON, biome = PLAINS, enabled = true, dropsEnabled = false} },
            {"Fuling", new CreatureValues {spawnCost = 15, prefabName = "Goblin", spawnType = COMMON, biome = PLAINS, enabled = true, dropsEnabled = false} },
            {"FulingArcher", new CreatureValues {spawnCost = 20, prefabName = "GoblinArcher", spawnType = COMMON, biome = PLAINS, enabled = true, dropsEnabled = false} },
            {"FulingBerserker", new CreatureValues {spawnCost = 45, prefabName = "GoblinBrute", spawnType = ELITE, biome = PLAINS, enabled = true, dropsEnabled = false} },
            {"FulingShaman", new CreatureValues {spawnCost = 40, prefabName = "GoblinShaman", spawnType = RARE, biome = PLAINS, enabled = true, dropsEnabled = false} },
            {"Growth", new CreatureValues {spawnCost = 35, prefabName = "BlobTar", spawnType = COMMON, biome = PLAINS, enabled = true, dropsEnabled = false} },
            // Mistland Creatures
            {"Seeker", new CreatureValues {spawnCost = 30, prefabName = "Seeker", spawnType = COMMON, biome = MISTLANDS, enabled = true, dropsEnabled = false} },
            {"SeekerSoldier", new CreatureValues {spawnCost = 75, prefabName = "SeekerBrute", spawnType = ELITE, biome = MISTLANDS, enabled = true, dropsEnabled = false} },
            {"SeekerBrood", new CreatureValues {spawnCost = 10, prefabName = "SeekerBrood", spawnType = COMMON, biome = MISTLANDS, enabled = true, dropsEnabled = false} },
            {"Gjall", new CreatureValues {spawnCost = 75, prefabName = "Gjall", spawnType = ELITE, biome = MISTLANDS, enabled = true, dropsEnabled = false} },
            {"Tick", new CreatureValues {spawnCost = 15, prefabName = "Tick", spawnType = COMMON, biome = MISTLANDS, enabled = true, dropsEnabled = false} },
            {"DvergerRouge", new CreatureValues {spawnCost = 40, prefabName = "Dverger", spawnType = RARE, biome = MISTLANDS, enabled = true, dropsEnabled = false} },
            {"DvergerMage", new CreatureValues {spawnCost = 75, prefabName = "DvergerMage", spawnType = RARE, biome = MISTLANDS, enabled = true, dropsEnabled = false} },
            {"DvergerMageFire", new CreatureValues {spawnCost = 75, prefabName = "DvergerMageFire", spawnType = RARE, biome = MISTLANDS, enabled = false, dropsEnabled = false } },
            {"DvergerMageIce", new CreatureValues {spawnCost = 75, prefabName = "DvergerMageIce", spawnType = RARE, biome = MISTLANDS, enabled = false, dropsEnabled = false } },
            {"DvergerMageSupport", new CreatureValues {spawnCost = 75, prefabName = "DvergerMageSupport", spawnType = ELITE, biome = MISTLANDS, enabled = true, dropsEnabled = false} },
            // Boss Creatures
            {"Eikthyr", new CreatureValues {spawnCost = 40, prefabName = "Eikthyr", spawnType = UNIQUE, biome = MEADOWS, enabled = true, dropsEnabled = false} },
            {"TheElder", new CreatureValues {spawnCost = 180, prefabName = "gd_king", spawnType = UNIQUE, biome = BLACKFOREST, enabled = true, dropsEnabled = false} },
            {"Bonemass", new CreatureValues {spawnCost = 250, prefabName = "Bonemass", spawnType = UNIQUE, biome = SWAMP, enabled = true, dropsEnabled = false} },
            {"Moder", new CreatureValues {spawnCost = 320, prefabName = "Dragon", spawnType = UNIQUE, biome = MOUNTAIN, enabled = true, dropsEnabled = false} },
            {"Yagluth", new CreatureValues {spawnCost = 450, prefabName = "GoblinKing", spawnType = UNIQUE, biome = PLAINS, enabled = true, dropsEnabled = false} },
            {"TheQueen", new CreatureValues {spawnCost = 600, prefabName = "SeekerQueen", spawnType = UNIQUE, biome = MISTLANDS, enabled = true, dropsEnabled = false} },
        };

        public static void UpdateSpawnableCreatures(SpawnableCreatureCollection spawnables)
        {
            SpawnableCreatures.Clear();
            foreach (KeyValuePair<string, CreatureValues> entry in spawnables.Creatures)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Updating Creature Entry {entry.Key} Prefab:{entry.Value.prefabName} SpawnCost:{entry.Value.spawnCost} Biome:{entry.Value.biome} SpawnType:{entry.Value.spawnType}"); } 
                SpawnableCreatures.Add(entry.Key, entry.Value);
            }
        }

        public static string YamlCreatureDefinition()
        {
            var creatureCollection = new SpawnableCreatureCollection();
            creatureCollection.Creatures = SpawnableCreatures;
            var yaml = CONST.yamlserializer.Serialize(creatureCollection);
            return yaml;
        }

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
                "The base number of points that all waves add. This is especially impactful in early levels (meadows).",
                false,
                100, 1000).Value;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Config Level base_challenge_points updated."); }

            max_challenge_points = cfg.BindServerConfig(
                "shine of challenge - levels",
                "max_challenge_points",
                (short)3000,
                "The absolute max number of points a wave can generate with, higher values will be clamped down to this value.",
                true, 1000, 30000).Value;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Config Level max_challenge_points updated."); }

            star_chance = cfg.BindServerConfig(
                "shine of challenge - levels",
                "creature_star_chance",
                0.15f,
                "The chance that a creature will be an upgraded version of itself.",
                true, 0.0f, 1).Value;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Config creature_star_chance updated."); }

            challenge_slope = cfg.BindServerConfig(
                "shine of challenge - levels",
                "challenge_slope",
                15.0f,
                "The linear regression slope which increases difficulty. If you want harder waves, add 1 and try out the difficulty again.",
                false, 5f, 50f).Value;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Config challenge_slope updated."); }

            chance_of_prior_biome_creature = cfg.BindServerConfig(
                "shine of challenge - levels",
                "chance_of_prior_biome_creature",
                0.05f,
                "The chance that a valid prior biome creature will be selected. Only 1 can be selected per wave. Setting to zero disables generating waves with previous biome creatures.",
                false, 0.00f, 1.0f).Value;

            max_creatures_per_wave = cfg.BindServerConfig(
                "shine of challenge - levels",
                "max_creatures_per_wave",
                (short)60,
                "The max number of creatures that a wave can generate with, creatures will attempt to upgrade and reduce counts based on this.",
                true, 25, 200).Value;

            max_creature_stars = cfg.BindServerConfig(
                "shine of challenge - levels",
                "max_creature_stars",
                (short)2,
                "This is the max number of stars a creature can have. CLLC is required for anything over 2.",
                true, 0, 10).Value;

        }


        // Steep logarithmic curve with controllable curve rate and increases.
        // Log(level^2) * (challenge_slope * level) + base_challenge_points
        public static Int16 ComputeChallengePoints(Int16 level)
        {
            Double level_offset = Math.Pow(level, 2);
            Double computed_slope = Math.Log(level_offset);
            Double offset = challenge_slope * level;
            Double allocated_additional_points = computed_slope * offset;
            Int16 allocated_challenge_points = (Int16)(base_challenge_points + allocated_additional_points);
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Challenge points slope:{allocated_challenge_points} = (log({level_offset})[{computed_slope}] * ({challenge_slope} * {level})[{offset}])[{allocated_additional_points}] + {base_challenge_points} "); }
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
                // point_estimate = (Int16)(point_estimate * 1.5);
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
            PhasedWaveTemplate wavedefinition = dynamicBuildWaveTemplate(level, wave_total_points, boss_mode, siege_mode);

            // Set the wave definition and determine the remote spawn points
            shrine.GetComponent<Shrine>().SetWaveDefinition(wavedefinition);
            Vector3[] remote_spawn_locations =  Spawner.DetermineRemoteSpawnLocations(shrine);
            // Spawn the portals in & set references for destruction later

            // If gladiator mode is enabled we don't spawn remote portals
            if (VFConfig.EnableGladiatorMode.Value == false)
            {
                List<GameObject> portals = Spawner.DrawMapOverlayAndPortals(remote_spawn_locations);
                shrine.GetComponent<Shrine>().setPortals(portals);
            }
            // Spawn the first wave, with a 5s delay, and don't send a pause message- since this is the start
            // Spawner.SpawnPhaseController(2f, false, wavedefinition.GetCurrentPhase(), shrine, remote_spawn_locations);
            shrine.GetComponent<Shrine>().StartChallengeMode();
            // Add the component, and call it
            //shrine.AddComponent<Spawner>();
            //Spawner spawn_controller = shrine.GetComponent<Spawner>();
            //Spawner.TrySpawningPhase(2f, false, wavedefinition.GetCurrentPhase(), shrine, remote_spawn_locations);
            //Jotunn.Logger.LogInfo("After phase spawn attempt.");
        }

        public static WaveGenerationFormat DecideWaveStyle(Int16 level, bool boss_mode)
        {
            // This provides a gradual ramp up in difficulty as the user progresses through the waves
            WaveGenerationFormat waveFormat = WaveStyles["Normal"]; // We set this as a default just incase I forgot something
            // Level curve
            if (!boss_mode)
            {

                // setup tutorial (meadows)
                if (level < 3) { waveFormat = WaveStyles["Tutorial"]; }
                if (level >= 3 && level < 6) { waveFormat = WaveStyles["Starter"]; }
                // First level of every biome past meadows is Normal
                if (level == 6 || level == 11 || level == 16 || level == 21 || level == 26) { waveFormat = WaveStyles["Normal"]; }
                // Second level of every biome is Hard
                if (level == 7 || level == 12 || level == 17 || level == 22 || level == 27) { waveFormat = WaveStyles["Hard"]; }
                // Third level of every biome is VeryHard
                if (level == 8 || level == 13 || level == 18 || level == 23 || level == 28) { waveFormat = WaveStyles["VeryHard"]; }
                // fourth level of every biome is Expert
                if (level == 9 || level == 14 || level == 19 || level == 24 || level == 29) { waveFormat = WaveStyles["Expert"]; }
                // fifth level of every biome is Dynamic
                if (level == 10 || level == 15 || level == 20 || level == 25 || level == 30) { waveFormat = WaveStyles["Dynamic"]; }

                // Extra
                if (level > 30) { waveFormat = WaveStyles["Dynamic"]; }
            } else
            {
                // Boss curve
                if (level <= 5) { waveFormat = WaveStyles["TutorialBoss"]; }
                if (level >= 6 && level <= 9) { waveFormat = WaveStyles["EasyBoss"]; }
                if (level > 9 && level <= 25) { waveFormat = WaveStyles["Boss"]; }
                if (level > 25) { waveFormat = WaveStyles["DynamicBoss"]; }
            }

            return waveFormat;
        }

        public static PhasedWaveTemplate dynamicBuildWaveTemplate(Int16 level, Int16 max_wave_points, bool boss_mode, bool siege_mode, int phases = 4)
        {
            WaveOutline waveOutline = new WaveOutline();

            if (siege_mode) { phases = phases * 2; }

            Int16 max_creatures_from_previous_biomes = 0;
            String selected_biome = MEADOWS;

            // haven't decided if this is worth it or how this should be broken out. Some kind of balance is needed between having only a few monsters from previous biomes
            // and supporting the fact that we can't do previous biome creatures before blackforest, and at each level the creatures need to be stronger to keep up with the current biome
            if (level > 5 && level < 11) { selected_biome = BLACKFOREST; max_creatures_from_previous_biomes += 0; }
            if (level > 10 && level < 16) { selected_biome = SWAMP; max_creatures_from_previous_biomes += 1; }
            if (level > 15 && level < 21) { selected_biome = MOUNTAIN; max_creatures_from_previous_biomes += 1; }
            if (level > 20 && level < 26) { selected_biome = PLAINS; max_creatures_from_previous_biomes += 1; }
            if (level > 25 && level < 31) { selected_biome = MISTLANDS; max_creatures_from_previous_biomes += 2; }
            // if (level > 30 && level < 35) { selected_biome = "Ashlands"; }
            short targeted_wave_biome_level = BiomeStringToInt(selected_biome);
            WaveGenerationFormat WaveGenerationFormat = DecideWaveStyle(level, boss_mode);
            Int16 creatures_selected_from_previous_biome = 0;
            if(VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Starting wave generation for {selected_biome} - phases: {phases} C-{WaveGenerationFormat.GetCommonSpawnFormations().Count} R-{WaveGenerationFormat.GetRareSpawnFormations().Count} E-{WaveGenerationFormat.GetEliteSpawnFormations().Count} U-{WaveGenerationFormat.CountUniqueSpawns()}"); }

            foreach(Tuple<String, int> entry in WaveGenerationFormat.GetWaveFormat())
            {
                string waveType = entry.Item1;
                float percentage = entry.Item2;
                bool creature_added = false;
                int creature_add_iterations = 0;
                float duplicate_chance = 0.25f;
                while (creature_added == false)
                {
                    if (creature_add_iterations > 3) { if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"{waveType}-{percentage}% failed to find an addition, skipping it."); } break; }
                    // we shuffle the monster keys so that we run through them in a different order each time
                    // This ensures that we fill the wave in different ways
                    List<string> this_iteration_key_order = ValheimFortress.shuffleList(new List<string>(SpawnableCreatures.Keys));
                    
                    foreach (String skey in this_iteration_key_order)
                    {
                        short min_stars = 0;
                        // Creature is of the wrong spawntype, skipping
                        if (SpawnableCreatures[skey].spawnType != waveType) { continue; }

                        // Creature is not from the right biome, or -1 biome
                        // or we already have all the creatures we need from a previous biome
                        short current_creature_biome_level = BiomeStringToInt(SpawnableCreatures[skey].biome);
                        //if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"{skey} Biome check: {current_creature_biome_level} > {targeted_wave_biome_level} || {targeted_wave_biome_level} != {current_creature_biome_level}"); }
                        //if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"{skey} Previous check: {current_creature_biome_level} != {targeted_wave_biome_level} && {creatures_selected_from_previous_biome} >= {max_creatures_from_previous_biomes}"); }
                        // Creature too high of a level
                        if (current_creature_biome_level > targeted_wave_biome_level) {
                            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"creature {skey} from a higher level biome."); }
                            continue; 
                        }
                        // Creature too low of a level
                        if (!(current_creature_biome_level >= (targeted_wave_biome_level - 1))) {
                            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"creature {skey} biome is too easy."); }
                            continue;
                        }
                        // This creature is from the previous biome
                        if (current_creature_biome_level != targeted_wave_biome_level)
                        {
                            // Roll to see if we will add this creature, its only a 5% chance by default.
                            float prior_biome_chance = UnityEngine.Random.value;
                            if(chance_of_prior_biome_creature < prior_biome_chance) {
                                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"previous biome creature {skey} failed its add roll {chance_of_prior_biome_creature} < {prior_biome_chance}."); }
                                continue; 
                            }
                        }
                            

                        // Consider adding a duplicate, with a small chance
                        float duplicate_roll = UnityEngine.Random.value;
                        //if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"{skey} duplicate key check {waveOutline.HasKey(skey)} && {duplicate_roll > duplicate_chance}."); }
                        if (waveOutline.HasKey(skey) && duplicate_roll > duplicate_chance) { continue; }

                        // If the creature is from a previous biome, we note that, and add at least one star to its generation
                        if (current_creature_biome_level != targeted_wave_biome_level) { creatures_selected_from_previous_biome += 1; min_stars += 1;  }

                        // Add the creature!
                        waveOutline.AddCreatureToWave(skey, max_wave_points, percentage, min_stars, max_creature_stars);
                        creature_added = true;
                        duplicate_chance = 0.25f;
                        if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"creature {skey} added as {waveType} {percentage}."); }
                        break;
                    }
                    // If we didn't get something on the first run, we ensure duplicate entries which will gaurentee an entry the second time.
                    if (creature_added == false) { duplicate_chance = 1.0f; }
                    creature_add_iterations += 1;
                }
            }


            PhasedWaveTemplate finalizedWaveGeneration = new PhasedWaveTemplate();
            int phases_generated = 0;
            List<HoardConfig> commonCreatures = waveOutline.getCommonCreatures();
            List<HoardConfig> rareCreatures = waveOutline.getRareCreatures();
            List<HoardConfig> eliteCreatures = waveOutline.getEliteCreatures();
            List<HoardConfig> uniqueCreatures = waveOutline.getUniqueCreatures();

            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Building out wave phases from wave outline with COMMON-{commonCreatures.Count} RARE-{rareCreatures.Count} ELITE-{eliteCreatures.Count} UNIQUE-{uniqueCreatures.Count}"); }

            while (phases > phases_generated)
            {
                List<HoardConfig> hoardPhase = new List<HoardConfig>();
                
                short common_creatures_in_wave = 0;
                short rare_creatures_in_wave = 0;
                short elite_creatures_in_wave = 0;
                short unique_creatures_in_wave = 0;
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Generating Phase {phases_generated}"); }

                // Add common creatures to each wave with a linear diminishing curve, frontloading creatures
                foreach (HoardConfig entry in commonCreatures)
                {
                    // The first 5 waves ONLY have common creatures, so we instead buildup the amount sent for these waves
                    float wavePercent = (level < 6) ? linearIncreaseRandomWaveAdjustment(phases, phases_generated) : linearDecreaseRandomWaveAdjustment(phases, phases_generated);
                    short spawn_amount = (short)(entry.amount * wavePercent);
                    if (spawn_amount <= 0) {  spawn_amount = 1; }
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adding COMMON entry {entry.creature} - {spawn_amount}"); }
                    common_creatures_in_wave += spawn_amount;
                    hoardPhase.Add(new HoardConfig(entry.creature, entry.prefab, spawn_amount, entry.stars));
                }

                // Don't add to the first wave, then add progressively more
                if (phases_generated > 1)
                {
                    foreach (HoardConfig entry in rareCreatures)
                    {
                        float wavePercent = linearIncreaseRandomWaveAdjustment(phases, phases_generated);
                        short spawn_amount = (short)(entry.amount * wavePercent);
                        if (spawn_amount <= 0) { spawn_amount = 1; }
                        if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adding RARE entry {entry.creature} - {spawn_amount}"); }
                        rare_creatures_in_wave += spawn_amount;
                        hoardPhase.Add(new HoardConfig(entry.creature, entry.prefab, spawn_amount, entry.stars));
                    }
                }

                // Add elites to the second half of the wave generation
                if (phases_generated >= (phases / 2))
                {
                    foreach (HoardConfig entry in eliteCreatures)
                    {
                        short spawn_amount = entry.amount;
                        if (spawn_amount <= 0) { spawn_amount = 1; }
                        if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adding ELITE entry {entry.creature} - {entry.amount}"); }
                        elite_creatures_in_wave += spawn_amount;
                        hoardPhase.Add(new HoardConfig(entry.creature, entry.prefab, spawn_amount, entry.stars));
                    }
                }

                // Last phase to generate, add the boss
                if ((phases_generated + 1) == phases)
                {
                    foreach(HoardConfig entry in uniqueCreatures)
                    {
                        short spawn_amount = entry.amount;
                        if (spawn_amount <= 0) { spawn_amount = 1; }
                        if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adding UNIQUE entry {entry.creature} - {entry.amount}"); }
                        unique_creatures_in_wave += spawn_amount;
                        hoardPhase.Add(new HoardConfig(entry.creature, entry.prefab, spawn_amount, entry.stars));
                    }
                }

                // Post processing on the wave itself
                short total_creatures_in_wave = (short)(common_creatures_in_wave + rare_creatures_in_wave + elite_creatures_in_wave + unique_creatures_in_wave);
                if(total_creatures_in_wave > max_creatures_per_wave)
                {
                    ReduceWaveSizeToMax(hoardPhase, total_creatures_in_wave);
                }
                

                finalizedWaveGeneration.AddPhase(hoardPhase);
                phases_generated += 1;
            }

            if (VFConfig.EnableDebugMode.Value)
            {
                List<List<HoardConfig>> finalizedPhases =  finalizedWaveGeneration.GetAllPhases();
                Jotunn.Logger.LogInfo($"Built wave definition with {finalizedPhases.Count} phases.");
                int debugPhases = 0;
                foreach(List<HoardConfig> phase in finalizedPhases)
                {
                    debugPhases += 1;
                    Jotunn.Logger.LogInfo($"Phase {debugPhases}");
                    foreach (HoardConfig hoard in phase)
                    {
                        Jotunn.Logger.LogInfo($"Hoard {hoard.creature} - Amount: {hoard.amount} Stars: {hoard.stars}");
                    }
                }
                
            }

            return finalizedWaveGeneration;
        }

        public static short DetermineCreatureStars(short min_stars, short max_stars)
        {
            short creatureStars = min_stars;
            // Random value between 0-1.
            float upgradeRoll = UnityEngine.Random.value;
            if (upgradeRoll > (1 - star_chance))
            {
                creatureStars += 1;
                if (max_stars >= (min_stars + 1)) // Upgrade successful, now can it be upgraded again?
                {
                    float secondUpgradeRoll = UnityEngine.Random.value;
                    if (secondUpgradeRoll > (1 - star_chance)) { creatureStars += 1; }
                }
            }
            return creatureStars;
        }

        public static float linearDecreaseRandomWaveAdjustment(int phases, int current_phase, float variance = 0.10f)
        {
            float initial_spawn = 1.0f;
            // reduction_from_phase_progression is at most an 80% change, plus/minus the variance, which should be always less than that
            float reduction_from_phase_progression = (0.8f / phases);
            float current_phase_reduction = reduction_from_phase_progression * current_phase;
            float random_variance = UnityEngine.Random.Range(variance * -1, variance);
            // Increase wave spawn towards the front, and reduce is significantly over time
            float current_wave_percentage = initial_spawn - current_phase_reduction + random_variance;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adjusting COMMON wave percent {current_wave_percentage} for phase {current_phase}"); }
            return current_wave_percentage;
        }

        public static float linearIncreaseRandomWaveAdjustment(int phases, int current_phase, float variance = 0.10f)
        {
            float initial_spawn = 0.20f;
            // reduction_from_phase_progression is at most an 80% change, plus/minus the variance, which should be always less than that
            float increase_from_phase_progression = (0.8f / phases);
            float current_phase_increase = increase_from_phase_progression * current_phase;
            float random_variance = UnityEngine.Random.Range(variance * -1, variance);
            // Increase wave spawn towards the front, and reduce is significantly over time
            float current_wave_percentage = initial_spawn + current_phase_increase + random_variance;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adjusting COMMON wave percent {current_wave_percentage} for phase {current_phase}"); }
            return current_wave_percentage;
        }


        // This is primarily a random fractional adjustment, with a min of 60% and max of 100% unless overtuned
        public static float partialRandomWaveAdjustment(float variance = 0.20f)
        {
            // variance allows this up to 100% or down to 60%
            float initial_spawn = 0.8f;
            float random_variance = UnityEngine.Random.Range(variance * -1, variance);
            float current_wave_percentage = initial_spawn + random_variance;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adjusting RARE wave percent {current_wave_percentage}"); }
            return current_wave_percentage;
        }

        public static short DetermineCreatureSpawnAmount(string creature, int total_points, float creature_percentage)
        {
            // The number to spawn is the total amount of wave points this segment represents / the creatures spawn cost
            float number_to_spawn = total_points * (creature_percentage / 100f) / SpawnableCreatures[creature].spawnCost;
            float number_to_spawn_variance = number_to_spawn * 0.10f;
            float spawn_amount = UnityEngine.Random.Range((number_to_spawn - number_to_spawn_variance), (number_to_spawn + number_to_spawn_variance));
            if (SpawnableCreatures[creature].spawnType == UNIQUE) { spawn_amount = 1; } // uniques only spawn one
            return (short)spawn_amount;
        }


        public static Int16 BiomeStringToInt(String biome)
        {
            if(!biomes.Contains(biome)) { throw new ArgumentException($"Biome {biome} does not match defined biomes: {string.Join(",", biomes)}"); }
            return (Int16)Array.IndexOf(biomes, biome);
        }

        public static String BiomeIntToString(Int16 biome_index)
        {
            return biomes[biome_index];
        }

        public static List<HoardConfig> ReduceWaveSizeToMax(List<HoardConfig> hoards, short total_creatures_in_wave)
        {
            if (total_creatures_in_wave <= max_creatures_per_wave) { return hoards; }

            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Wave has more creatures than allowed from configuration to remove: {total_creatures_in_wave} > {max_creatures_per_wave}"); }
            // The most this gets us is one iteration through, so this isn't going to scale up insanely
            foreach (HoardConfig entry in hoards)
            {
                // We always skip reductions for unique types since they are always 1.
                if (SpawnableCreatures[entry.creature].spawnType == CONST.UNIQUE) { continue; }
                if (total_creatures_in_wave <= max_creatures_per_wave) { break; }
                // This innately starts with common types, since that is what we added first, and they are generally the most populous except in later levels
                // We don't want to reduce amounts from waves that are too small since we start loosing precision and will either make the wave much harder or much easier
                total_creatures_in_wave = MutatingReduceHoardConfigSize(entry, total_creatures_in_wave);
            }
            // Run a second time, but only for groups that have more than 5 entries
            if (total_creatures_in_wave > max_creatures_per_wave)
            {
                foreach (HoardConfig entry in hoards)
                {
                    if (entry.amount <= 5) { continue; }
                    if (total_creatures_in_wave <= max_creatures_per_wave) { break; }
                    total_creatures_in_wave = MutatingReduceHoardConfigSize(entry, total_creatures_in_wave);
                }
            }

            return hoards;
        }

        private static short MutatingReduceHoardConfigSize(HoardConfig hoard, short total_creatures_in_wave)
        {
            if (total_creatures_in_wave > max_creatures_per_wave)
            {
                if (hoard.stars < max_creature_stars)
                {
                    short creature_difference = (short)(hoard.amount / 2);
                    hoard.amount = creature_difference;
                    hoard.stars += 1;
                    total_creatures_in_wave = (short)(total_creatures_in_wave - creature_difference);
                }
            }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"wave {hoard.creature} reduced from {hoard.amount*2} to {hoard.amount} total creatures in wave now: {total_creatures_in_wave}"); }
            return total_creatures_in_wave;
        }
    }
}
