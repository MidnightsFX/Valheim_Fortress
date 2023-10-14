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
        static private Double challenge_slope = 0.1;
        static private Int16 base_challenge_points = 100;
        static private Int16 base_challenge_points_increase = 20;
        static private Int16 max_challenge_points = 3000;
        static private String[] requiredBosses = { "Eikythr", "TheElder", "BoneMass", "Moder", "Yagluth", "TheQueen" };
        static private String[] spawnTypes = { "common", "rare", "unique" };

        public class HoardConfig
        {
            public String creature;
            public Int16 amount;
            public Int16 stars;

            public HoardConfig(
                String creatureName,
                Int16 min_spawned,
                Int16 max_spawned,
                Int16 min_stars = 0,
                Int16 max_stars = 0
                )
            {
                creature = creatureName;
                amount = (short)UnityEngine.Random.Range(min_spawned, max_spawned);
                stars = (short)UnityEngine.Random.Range(min_stars, max_stars);
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
            public short maxStars;
            public String requiredBoss;
            public String spawnType;

            /// <summary>
            ///  Defines a spawnable creatures configuration values
            /// </summary>
            /// <param name="spawnCost"></param>
            /// <param name="prefabName"></param>
            /// <param name="maxStars"></param>
            /// <param name="requiredBoss"></param>
            public CreatureValues(short spawnCost, String prefabName, short maxStars, String requiredBoss, String spawnType)
            {
                this.spawnCost = spawnCost;
                this.prefabName = prefabName;
                this.maxStars = maxStars;
                this.requiredBoss = requiredBoss;
                this.spawnType = spawnType;
            }
        }

        static Dictionary<String, CreatureValues> SpawnableCreatures = new Dictionary<string, CreatureValues>
        {
            // Meadow Creatures
            {"Neck", new CreatureValues(spawnCost: 1, "Neck",  maxStars: 2, "false", spawnType: "common") },
            {"Boar", new CreatureValues(spawnCost: 2, "Boar",  maxStars: 2, "false", spawnType: "common") },
            {"Greyling", new CreatureValues(spawnCost: 3, "Greyling", maxStars: 2, "false", spawnType: "common") },
            // Black Forest Creatures
            {"GreyDwarf", new CreatureValues(spawnCost: 4, "GreyDwarf", maxStars: 2, "Eikythr", spawnType: "common") },
            {"GreyDwarfBrute", new CreatureValues(spawnCost: 8, "Greydwarf_Elite", maxStars: 2, "Eikythr", spawnType: "rare") },
            {"GreyDwarfShaman", new CreatureValues(spawnCost: 8, "Greydwarf_Shaman", maxStars: 2, "Eikythr", spawnType: "rare") },
            {"Skeleton", new CreatureValues(spawnCost: 4, "Skeleton_NoArcher", maxStars: 2, "Eikythr", spawnType: "common") },
            {"SkeletonArcher", new CreatureValues(spawnCost: 5, "Skeleton", maxStars: 2, "Eikythr", spawnType: "common") },
            {"RancidSkeleton", new CreatureValues(spawnCost: 9, "Skeleton_Poison", maxStars: 2, "Eikythr", spawnType: "rare") },
            {"Ghost", new CreatureValues(spawnCost: 7, "Greyling", maxStars: 2, "Eikythr", spawnType: "rare") },
            {"Troll", new CreatureValues(spawnCost: 20, "Greyling", maxStars: 1, "Eikythr", spawnType: "rare") },
            // Swamp Creatures
            {"Surtling", new CreatureValues(spawnCost: 6, "Surtling", maxStars: 2, "TheElder", spawnType: "common") },
            {"Wraith", new CreatureValues(spawnCost: 10, "Wraith", maxStars: 2, "TheElder", spawnType: "rare") },
            {"Abomination", new CreatureValues(spawnCost: 30, "Abomination", maxStars: 1, "TheElder", spawnType: "rare") },
            {"Draugr", new CreatureValues(spawnCost: 10, "Draugr", maxStars: 2, "TheElder", spawnType: "common") },
            {"DraugrArcher", new CreatureValues(spawnCost: 12, "Draugr_Ranged", maxStars: 2, "TheElder", spawnType: "common") },
            {"DraugrElite", new CreatureValues(spawnCost: 15, "Draugr_Elite", maxStars: 2, "TheElder", spawnType: "rare") },
            {"Blob", new CreatureValues(spawnCost: 7, "Blob", maxStars: 2, "TheElder", spawnType: "common") },
            {"BlobElite", new CreatureValues(spawnCost: 15, "BlobElite", maxStars: 2, "TheElder", spawnType: "rare") },
            // Mountain Creatures
            {"Bat", new CreatureValues(spawnCost: 3, "Bat", maxStars: 2, "BoneMass", spawnType: "common") },
            {"IceDrake", new CreatureValues(spawnCost: 12, "Hatchling", maxStars: 2, "BoneMass", spawnType: "rare") },
            {"Wolf", new CreatureValues(spawnCost: 15, "Wolf", maxStars: 2, "BoneMass", spawnType: "common") },
            {"Fenring", new CreatureValues(spawnCost: 18, "Fenring", maxStars: 2, "BoneMass", spawnType: "rare") },
            {"Ulv", new CreatureValues(spawnCost: 12, "Ulv", maxStars: 2, "BoneMass", spawnType: "common") },
            {"Cultist", new CreatureValues(spawnCost: 18, "Fenring_Cultist", maxStars: 2, "BoneMass", spawnType: "rare") },
            {"StoneGolemn", new CreatureValues(spawnCost: 40, "StoneGolem", maxStars: 0, "BoneMass", spawnType: "rare") },
            // Plains Creatures
            {"Deathsquito", new CreatureValues(spawnCost: 20, "Deathsquito", maxStars: 0, "Moder", spawnType: "common") },
            {"Fuling", new CreatureValues(spawnCost: 15, "Goblin", maxStars: 2, "Moder", spawnType: "common") },
            {"FulingArcher", new CreatureValues(spawnCost: 17, "GoblinArcher", maxStars: 2, "Moder", spawnType: "common") },
            {"FulingBerserker", new CreatureValues(spawnCost: 35, "GoblinBrute", maxStars: 1, "Moder", spawnType: "rare") },
            {"FulingShaman", new CreatureValues(spawnCost: 25, "GoblinShaman", maxStars: 1, "Moder", spawnType: "rare") },
            {"Growth", new CreatureValues(spawnCost: 25, "BlobTar", maxStars: 1, "Moder", spawnType: "common") },
            // Mistland Creatures
            {"Seeker", new CreatureValues(spawnCost: 30, "Seeker", maxStars: 2, "Yagluth", spawnType: "common") },
            {"SeekerSoldier", new CreatureValues(spawnCost: 45, "SeekerBrute", maxStars: 1, "Yagluth", spawnType: "rare") },
            {"SeekerBrood", new CreatureValues(spawnCost: 10, "SeekerBrood", maxStars: 2, "Yagluth", spawnType: "common") },
            {"Gjall", new CreatureValues(spawnCost: 50, "Gjall", maxStars: 1, "Yagluth", spawnType: "rare") },
            {"Tick", new CreatureValues(spawnCost: 15, "Tick", maxStars: 2, "Yagluth", spawnType: "common") },
            {"DvergerRouge", new CreatureValues(spawnCost: 25, "Dverger", maxStars: 1, "Yagluth", spawnType: "rare") },
            {"DvergerMage", new CreatureValues(spawnCost: 40, "DvergerMage", maxStars: 1, "Yagluth", spawnType: "rare") },
            {"DvergerMageFire", new CreatureValues(spawnCost: 40, "DvergerMageFire", maxStars: 1, "Yagluth", spawnType: "rare") },
            {"DvergerMageIce", new CreatureValues(spawnCost: 40, "DvergerMageIce", maxStars: 1, "Yagluth", spawnType: "rare") },
            {"DvergerMageSupport", new CreatureValues(spawnCost: 40, "DvergerMageSupport", maxStars: 1, "Yagluth", spawnType: "rare") },
            // Boss Creatures
            {"Eikthyr", new CreatureValues(spawnCost: 60, "Eikthyr", maxStars: 0, "false", spawnType: "unique") },
            {"TheElder", new CreatureValues(spawnCost: 180, "gd_king", maxStars: 0, "false", spawnType: "unique") },
            {"BoneMass", new CreatureValues(spawnCost: 250, "DvergerMageSupport", maxStars: 0, "false", spawnType: "unique") },
            {"Moder", new CreatureValues(spawnCost: 320, "DvergerMageSupport", maxStars: 0, "false", spawnType: "unique") },
            {"Yagluth", new CreatureValues(spawnCost: 450, "DvergerMageSupport", maxStars: 0, "false", spawnType: "unique") },
            {"TheQueen", new CreatureValues(spawnCost: 600, "DvergerMageSupport", maxStars: 0, "false", spawnType: "unique") },
        };

        public static void UpdateCreatureConfigValues(VFConfig cfg)
        {

            foreach (KeyValuePair<string, CreatureValues> entry in SpawnableCreatures)
            {
                
                short attempted_spawncost = cfg.BindServerConfig(
                    "shine of challenge - monsters",
                    $"{entry.Key}_creaturevalue",
                    entry.Value.spawnCost,
                    $"the generation cost spawn a {entry.Key}, smaller values (whole numbers only) will allow more of the creature to spawn per challenge.",
                    true).Value;
                if (attempted_spawncost > 1000 || attempted_spawncost < 0)
                {
                    Jotunn.Logger.LogWarning($"{entry.Key}_creaturevalue={attempted_spawncost}. is not valid, reseting to default ({entry.Value.spawnCost}).");
                }
                else
                {
                    SpawnableCreatures[entry.Key].spawnCost = attempted_spawncost;
                }
                Jotunn.Logger.LogInfo($"Config {entry.Key}_creaturevalue Added.");
                SpawnableCreatures[entry.Key].spawnType = cfg.BindServerConfig(
                    "shine of challenge - monsters",
                    $"{entry.Key}_spawntype",
                    entry.Value.spawnType,
                    $"the generation type for {entry.Key}, valid values are: common,rare,unique. this governs how frequently the creature can be added to a waves generation.",
                    true).Value;
                Jotunn.Logger.LogInfo($"Config {entry.Key}_spawntype Added.");
                SpawnableCreatures[entry.Key].maxStars = cfg.BindServerConfig(
                    "shine of challenge - monsters",
                    $"{entry.Key}_maxstars",
                    entry.Value.maxStars,
                    $"the max number of stars for {entry.Key}. in vanilla above 2 is meaningless & bosses do not have multistar varients.",
                    true).Value;
                Jotunn.Logger.LogInfo($"Config {entry.Key}_maxstars Added.");
            }
        }



        // Logarithmic with a cap
        // y = a + b ln x
        // allocated_challenge_points = base_challenge_points + base_challenge_points_increase * computed_slope
        // Computed slope is simply the log of 10 + the challenge slope, resulting in a small to larger increase based on the defined challenge slope.
        // Challenge slope should always be positive.
        public static Int16 ComputeChallengePoints(Int16 level)
        {
            Int16 computed_slope = (Int16)Math.Log(10 + challenge_slope + level);
            Int16 challenge_increase = (Int16)(base_challenge_points_increase * computed_slope);
            Int16 allocated_challenge_points = (Int16)(base_challenge_points + challenge_increase);
            
            // Cap the challenge points if they are over the defined max
            if (allocated_challenge_points > max_challenge_points)
            {
                allocated_challenge_points = max_challenge_points;
            }

            return allocated_challenge_points;
        }

        public static void generateRandomWaveWithOptions(Int16 level, GameObject shrine)
        {
            
            Int16 wave_total_points = ComputeChallengePoints(level);
            Jotunn.Logger.LogInfo($"Wave Challenge points: {wave_total_points}");
            // Builds out a template for wave generation
            WaveTemplate wavedefinition = getLevelTemplate(level, wave_total_points);
            
            // Add the component, and call it
            shrine.AddComponent<Spawner>();
            Spawner spawn_controller = shrine.GetComponent<Spawner>();
            spawn_controller.TrySpawningHoard(wavedefinition.GetWaves(), shrine);
        }

        public static WaveTemplate getLevelTemplate(Int16 level, Int16 max_wave_points)
        {
            WaveTemplate leveldefinition = new WaveTemplate();

            // Tuple is creature_percent_of_wave, min_stars, max_stars
            // This allows having a subset of the same spawn type that is higher/lower star settings
            Dictionary<String, Tuple<int , int, int>> waveComp = new Dictionary<string, Tuple<int, int, int>> { };
            // I was planning on doing something much more programmatic for this, so will probably rewrite
            // but I also hit a lot of writers block and wanted to get an actual working steel thread out before this collapsed in dust
            switch (level)
            {
                case 1:
                    waveComp.Add("Neck", new Tuple<int, int, int>(50, 0, 0));
                    waveComp.Add("Boar", new Tuple<int, int, int>(50, 0, 0));
                    break;
                case 2:
                    waveComp.Add("Neck", new Tuple<int, int, int>(30, 0, 0));
                    waveComp.Add("Boar", new Tuple<int, int, int>(50, 0, 0));
                    waveComp.Add("Greyling", new Tuple<int, int, int>(20, 0, 0));
                    break;
                case 3:
                    waveComp.Add("Neck", new Tuple<int, int, int>(30, 0, 0));
                    waveComp.Add("Boar", new Tuple<int, int, int>(30, 0, 0));
                    waveComp.Add("Greyling", new Tuple<int, int, int>(40, 0, 0));
                    break;
                case 4:
                    waveComp.Add("Neck", new Tuple<int, int, int>(10, 1, 1));
                    waveComp.Add("Boar", new Tuple<int, int, int>(10, 1, 1));
                    waveComp.Add("Greyling", new Tuple<int, int, int>(50, 0, 0));
                    waveComp.Add("GreyDwarf", new Tuple<int, int, int>(30, 0, 0));
                    break;
                case 5:
                    waveComp.Add("Eikthyr", new Tuple<int, int, int>(1, 0, 0));
                    waveComp.Add("Greyling", new Tuple<int, int, int>(45, 0, 0));
                    waveComp.Add("GreyDwarf", new Tuple<int, int, int>(45, 0, 0));
                    break;
                default:
                    Jotunn.Logger.LogWarning($"Wave: {level} was not matched, no waves will spawn.");
                    break;
            }
            Jotunn.Logger.LogInfo($"level={level} with {waveComp.Count} entries");

            foreach (KeyValuePair<string, Tuple<int, int, int>> entry in waveComp)
            {
               String spawnTypeRule = SpawnableCreatures[entry.Key].spawnType;
                Int16 max_percent_spawnable = 5; // switch case assignment availabe in 8+
                switch(spawnTypeRule)
                {
                    case "rare":
                        max_percent_spawnable = 30;
                        break;
                    case "common":
                        max_percent_spawnable = 100;
                        break;
                }
                Int16 suggest_wave_amount = (Int16)entry.Value.Item1;
                // Cap out the amount
                if (suggest_wave_amount > max_percent_spawnable) { suggest_wave_amount = max_percent_spawnable; };

                // max amount spawned = wave points * suggested wave amount / cost to spawn creature;
                // This gets overridden if the type of creature is unique and it should only spawn 1- eg: bosses/minibosses.
                double percent_of_wave = suggest_wave_amount / 100.0;
                double max_spawnable =  ((max_wave_points * percent_of_wave) / SpawnableCreatures[entry.Key].spawnCost);
                // min spawnable allows a variablation, if the type is not unique its up to 20% less
                double min_spawnable = max_spawnable * 0.8;
                if (spawnTypeRule == "unique") { max_spawnable = 1; min_spawnable = 1; };

                Jotunn.Logger.LogInfo($"Max spawnable: {max_spawnable} = ({max_wave_points} * {percent_of_wave}) / ({SpawnableCreatures[entry.Key].spawnCost})");
                Jotunn.Logger.LogInfo($"Min spawnable: {min_spawnable} = {max_spawnable} * 0.8");
                Jotunn.Logger.LogInfo($"hoard: {entry.Key}, {max_spawnable}, stars: {entry.Value.Item2}-{entry.Value.Item3}");
                HoardConfig creatureWave = new HoardConfig(entry.Key, (Int16)min_spawnable, (Int16)max_spawnable, (Int16)entry.Value.Item2, (Int16)entry.Value.Item3);
                leveldefinition.AddHoard(creatureWave);
            }

            Jotunn.Logger.LogInfo("Built wave definition.");
            Jotunn.Logger.LogInfo($"{leveldefinition.GetWaves()}");
            foreach (Levels.HoardConfig hoard in leveldefinition.GetWaves())
            {
                Jotunn.Logger.LogInfo($"Hoard {hoard.creature} - {hoard.amount}");
            }
            return leveldefinition;
        }

       
    }
}
