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
using UnityEngine.InputSystem;

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
                        commonCreatures.Add(new HoardConfig { creature = creature, prefab = SpawnableCreatures[creature].prefabName, amount = creature_spawn_amount, stars = stars });
                        selectedCreatures.Add(creature);
                        break;
                    case RARE:
                        rareCreatures.Add(new HoardConfig { creature = creature, prefab = SpawnableCreatures[creature].prefabName, amount = creature_spawn_amount, stars = stars });
                        selectedCreatures.Add(creature);
                        break;
                    case ELITE:
                        eliteCreatures.Add(new HoardConfig{ creature = creature, prefab = SpawnableCreatures[creature].prefabName, amount = creature_spawn_amount, stars = stars });
                        selectedCreatures.Add(creature);
                        break;
                    case UNIQUE:
                        uniqueCreatures.Add(new HoardConfig{ creature = creature, prefab = SpawnableCreatures[creature].prefabName, amount = creature_spawn_amount, stars = stars });
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

        public static List<ChallengeLevelDefinition> ChallengeLevelDefinitions = new List<ChallengeLevelDefinition>
        {
            new ChallengeLevelDefinition
            {
                // Level index is the internal representation of this level, it will be used to modify the difficulty, this must be greater than 0
                // It does not have to be unique, if multiple levels for the same shrine use the same index, they will be added in order of definition
                // eg: you could have level 9 (black forest) have the same difficulty as level 10 (swamp)
                levelIndex = 1,
                // Shrine type that this level applies to
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                // This is the text used to display the level naming in the shrine level selector
                levelMenuLocalization = "$shrine_menu_meadow",
                // This is the global key required for this label to be displayed, this uses game global keys or NONE
                requiredGlobalKey = "NONE",
                // This is the biome that this level will pull its creature definitions from
                biome = "Meadows",
                // This is the wave style that this wave will generate in
                waveFormat = "Tutorial",
                // If this wave is set to be a boss wave, this is the wave style it will generate in
                bossWaveFormat = "TutorialBoss",
                // This is the number of creatures that could be maximally selected from a previou biome
                maxCreatureFromPreviousBiomes = 0,
                // This is the warning text that is displayed when the round starts, for normal- and then following for boss waves
                levelWarningLocalization = "$shrine_warning_meadows",
                bossLevelWarningLocalization = "$shrine_warning_meadows_boss",
                // This is an inclusion list which specifies what creatures are allowed in this wave, this uses keys from the monster.yaml configuration
                onlySelectMonsters = new List<String> { },
                excludeSelectMonsters = new List<String> { },
                // These are the spawn modifiers that are applied to each type of creature spawn
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                    { "linearDecreaseRandomWaveAdjustment", false },
                    { "partialRandomWaveAdjustment", false },
                    { "onlyGenerateInSecondHalf", false }
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                    { "linearDecreaseRandomWaveAdjustment", false },
                    { "partialRandomWaveAdjustment", false },
                    { "onlyGenerateInSecondHalf", false }
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                    { "linearDecreaseRandomWaveAdjustment", false },
                    { "partialRandomWaveAdjustment", false },
                    { "onlyGenerateInSecondHalf", false }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 2,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_meadow",
                requiredGlobalKey = "NONE",
                biome = "Meadows",
                waveFormat = "Tutorial",
                bossWaveFormat = "TutorialBoss",
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "$shrine_warning_meadows",
                bossLevelWarningLocalization = "$shrine_warning_meadows_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 3,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_meadow",
                requiredGlobalKey = "NONE",
                biome = "Meadows",
                waveFormat = "Tutorial",
                bossWaveFormat = "TutorialBoss",
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "$shrine_warning_meadows",
                bossLevelWarningLocalization = "$shrine_warning_meadows_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 4,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_meadow",
                requiredGlobalKey = "NONE",
                biome = "Meadows",
                waveFormat = "Starter",
                bossWaveFormat = "TutorialBoss",
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "$shrine_warning_meadows",
                bossLevelWarningLocalization = "$shrine_warning_meadows_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 5,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_meadow",
                requiredGlobalKey = "NONE",
                biome = "Meadows",
                waveFormat = "Starter",
                bossWaveFormat = "TutorialBoss",
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "$shrine_warning_meadows",
                bossLevelWarningLocalization = "$shrine_warning_meadows_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 6,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_forest",
                requiredGlobalKey = "defeated_eikthyr",
                biome = "BlackForest",
                waveFormat = "Easy",
                bossWaveFormat = "EasyBoss",
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "$shrine_warning_forest",
                bossLevelWarningLocalization = "$shrine_warning_forest_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 7,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_forest",
                requiredGlobalKey = "defeated_eikthyr",
                biome = "BlackForest",
                waveFormat = "Normal",
                bossWaveFormat = "EasyBoss",
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "$shrine_warning_forest",
                bossLevelWarningLocalization = "$shrine_warning_forest_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 8,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_forest",
                requiredGlobalKey = "defeated_eikthyr",
                biome = "BlackForest",
                waveFormat = "Normal",
                bossWaveFormat = "Boss",
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "$shrine_warning_forest",
                bossLevelWarningLocalization = "$shrine_warning_forest_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 9,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_forest",
                requiredGlobalKey = "defeated_eikthyr",
                biome = "BlackForest",
                waveFormat = "Normal",
                bossWaveFormat = "Boss",
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "$shrine_warning_forest",
                bossLevelWarningLocalization = "$shrine_warning_forest_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 10,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_forest",
                requiredGlobalKey = "defeated_eikthyr",
                biome = "BlackForest",
                waveFormat = "Hard",
                bossWaveFormat = "Boss",
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "$shrine_warning_forest",
                bossLevelWarningLocalization = "$shrine_warning_forest_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 11,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_swamp",
                requiredGlobalKey = "defeated_gdking",
                biome = "Swamp",
                waveFormat = "Easy",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_swamp",
                bossLevelWarningLocalization = "$shrine_warning_swamp_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 12,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_swamp",
                requiredGlobalKey = "defeated_gdking",
                biome = "Swamp",
                waveFormat = "Normal",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_swamp",
                bossLevelWarningLocalization = "$shrine_warning_swamp_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 13,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_swamp",
                requiredGlobalKey = "defeated_gdking",
                biome = "Swamp",
                waveFormat = "Normal",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_swamp",
                bossLevelWarningLocalization = "$shrine_warning_swamp_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 14,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_swamp",
                requiredGlobalKey = "defeated_gdking",
                biome = "Swamp",
                waveFormat = "Hard",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_swamp",
                bossLevelWarningLocalization = "$shrine_warning_swamp_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 15,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_swamp",
                requiredGlobalKey = "defeated_gdking",
                biome = "Swamp",
                waveFormat = "Hard",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_swamp",
                bossLevelWarningLocalization = "$shrine_warning_swamp_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 16,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_mountain",
                requiredGlobalKey = "defeated_bonemass",
                biome = "Mountain",
                waveFormat = "Normal",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mountain",
                bossLevelWarningLocalization = "$shrine_warning_mountain_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 17,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_mountain",
                requiredGlobalKey = "defeated_bonemass",
                biome = "Mountain",
                waveFormat = "Normal",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mountain",
                bossLevelWarningLocalization = "$shrine_warning_mountain_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 18,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_mountain",
                requiredGlobalKey = "defeated_bonemass",
                biome = "Mountain",
                waveFormat = "Hard",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mountain",
                bossLevelWarningLocalization = "$shrine_warning_mountain_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 19,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_mountain",
                requiredGlobalKey = "defeated_bonemass",
                biome = "Mountain",
                waveFormat = "Hard",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mountain",
                bossLevelWarningLocalization = "$shrine_warning_mountain_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 20,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_mountain",
                requiredGlobalKey = "defeated_bonemass",
                biome = "Mountain",
                waveFormat = "Expert",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mountain",
                bossLevelWarningLocalization = "$shrine_warning_mountain_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 21,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_plains",
                requiredGlobalKey = "defeated_dragon",
                biome = "Plains",
                waveFormat = "Normal",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_plains",
                bossLevelWarningLocalization = "$shrine_warning_plains_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 22,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_plains",
                requiredGlobalKey = "defeated_dragon",
                biome = "Plains",
                waveFormat = "Hard",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_plains",
                bossLevelWarningLocalization = "$shrine_warning_plains_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 23,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_plains",
                requiredGlobalKey = "defeated_dragon",
                biome = "Plains",
                waveFormat = "VeryHard",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_plains",
                bossLevelWarningLocalization = "$shrine_warning_plains_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 24,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_plains",
                requiredGlobalKey = "defeated_dragon",
                biome = "Plains",
                waveFormat = "Normal",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_plains",
                bossLevelWarningLocalization = "$shrine_warning_plains_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 25,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_plains",
                requiredGlobalKey = "defeated_dragon",
                biome = "Plains",
                waveFormat = "Extreme",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_plains",
                bossLevelWarningLocalization = "$shrine_warning_plains_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 26,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_mistland",
                requiredGlobalKey = "defeated_goblinking",
                biome = "Mistlands",
                waveFormat = "Hard",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mistlands",
                bossLevelWarningLocalization = "$shrine_warning_mistlands_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 27,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_mistland",
                requiredGlobalKey = "defeated_goblinking",
                biome = "Mistlands",
                waveFormat = "VeryHard",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mistlands",
                bossLevelWarningLocalization = "$shrine_warning_mistlands_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 28,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_mistland",
                requiredGlobalKey = "defeated_goblinking",
                biome = "Mistlands",
                waveFormat = "Expert",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mistlands",
                bossLevelWarningLocalization = "$shrine_warning_mistlands_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 29,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_mistland",
                requiredGlobalKey = "defeated_goblinking",
                biome = "Mistlands",
                waveFormat = "Extreme",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mistlands",
                bossLevelWarningLocalization = "$shrine_warning_mistlands_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 30,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_mistland",
                requiredGlobalKey = "defeated_goblinking",
                biome = "Mistlands",
                waveFormat = "Dynamic",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mistlands",
                bossLevelWarningLocalization = "$shrine_warning_mistlands_boss",
                commonSpawnModifiers = new Dictionary<string, bool> {
                    { "linearDecreaseRandomWaveAdjustment", true },
                },
                rareSpawnModifiers = new Dictionary<string, bool> {
                    { "linearIncreaseRandomWaveAdjustment", true },
                },
                eliteSpawnModifiers = new Dictionary<string, bool> {
                    { "onlyGenerateInSecondHalf", true }
                }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 9,
                levelForShrineTypes = new Dictionary<string, bool> {
                    { "challenge", true },
                    { "arena", true }
                },
                levelMenuLocalization = "$shrine_menu_troll_level",
                requiredGlobalKey = "KilledTroll",
                biome = "BlackForest",
                waveFormat = "ElitesOnly",
                bossWaveFormat = "DynamicBoss",
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_trolls",
                bossLevelWarningLocalization = "$shrine_warning_forest_boss",
                onlySelectMonsters = new List<String> { "Troll" },
            },

        };

        public static List<ChallengeLevelDefinition> GetChallengeLevelDefinitions()
        {
            return ChallengeLevelDefinitions;
        }

        public static void UpdateLevelsDefinition(ChallengeLevelDefinitionCollection levelDefinitions)
        {
            ChallengeLevelDefinitions.Clear();
            ChallengeLevelDefinitions = levelDefinitions.Levels;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Updated Level definitions."); }
        }

        public static string YamlLevelsDefinition()
        {
            var levelCollection = new ChallengeLevelDefinitionCollection();
            levelCollection.Levels = ChallengeLevelDefinitions;
            var yaml = CONST.yamlserializer.Serialize(levelCollection);
            return yaml;
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
            { "Tutorial", new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry( COMMON, 30 ), new WaveFormatEntry(COMMON, 30) } } },
            { "Starter", new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry( COMMON, 25 ), new WaveFormatEntry(COMMON, 25), new WaveFormatEntry(COMMON, 15) } } },
            { "Easy", new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(RARE, 15 ), new WaveFormatEntry(COMMON, 25), new WaveFormatEntry(COMMON, 30) } } },
            { "Normal", new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(RARE, 15 ), new WaveFormatEntry(RARE, 10), new WaveFormatEntry(COMMON, 30), new WaveFormatEntry(COMMON, 20) } } },
            { "Hard", new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(ELITE, 5), new WaveFormatEntry(RARE, 20 ), new WaveFormatEntry(COMMON, 20), new WaveFormatEntry(COMMON, 30) } } },
            { "VeryHard", new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(ELITE, 10), new WaveFormatEntry(RARE, 25 ), new WaveFormatEntry(COMMON, 20), new WaveFormatEntry(COMMON, 30) } } },
            { "Expert", new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(ELITE, 10), new WaveFormatEntry(RARE, 15 ), new WaveFormatEntry(RARE, 15), new WaveFormatEntry(COMMON, 20), new WaveFormatEntry(COMMON, 30) } } },
            { "Extreme", new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(ELITE, 15), new WaveFormatEntry(RARE, 20), new WaveFormatEntry(RARE, 15 ), new WaveFormatEntry(COMMON, 20), new WaveFormatEntry(COMMON, 25) } } },
            { "Dynamic", new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(ELITE, 15), new WaveFormatEntry(RARE, 25 ), new WaveFormatEntry(RARE, 15), new WaveFormatEntry(COMMON, 20), new WaveFormatEntry(COMMON, 25) } } },
            { "ElitesOnly", new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry( ELITE, 25 ), new WaveFormatEntry(ELITE, 25), new WaveFormatEntry(ELITE, 25) } } },
            { "RaresOnly", new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(RARE, 25 ), new WaveFormatEntry(RARE, 25), new WaveFormatEntry(RARE, 25) } } },
            // Boss waves
            { "TutorialBoss", new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(UNIQUE, 100), new WaveFormatEntry( COMMON, 30 ), new WaveFormatEntry(COMMON, 40) } } },
            { "EasyBoss", new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(UNIQUE, 100), new WaveFormatEntry(RARE, 30 ), new WaveFormatEntry(COMMON, 30) } } },
            { "Boss", new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(UNIQUE, 100), new WaveFormatEntry(ELITE, 20 ), new WaveFormatEntry(RARE, 30), new WaveFormatEntry(COMMON, 25) } } },
            { "DynamicBoss", new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(UNIQUE, 100), new WaveFormatEntry(ELITE, 20 ), new WaveFormatEntry(RARE, 20), new WaveFormatEntry(RARE, 20), new WaveFormatEntry(COMMON, 20), new WaveFormatEntry(COMMON, 20) } } },
        };

        public static void UpdateWaveDefinition(WaveFormatCollection waveStyles)
        {
            WaveStyles.Clear();
            foreach (KeyValuePair<string, WaveGenerationFormat> entry in waveStyles.WaveFormats)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Updating Wavestyle Entry {entry.Key} {entry.Value}"); }
                WaveStyles.Add(entry.Key, entry.Value);
            }
        }

        public static string YamlWaveDefinition()
        {
            var waveCollection = new WaveFormatCollection();
            waveCollection.WaveFormats = WaveStyles;
            var yaml = CONST.yamlserializer.Serialize(waveCollection);
            return yaml;
        }


        public static Dictionary<String, CreatureValues> SpawnableCreatures = new Dictionary<string, CreatureValues>
        {
            // Meadow Creatures
            {"Neck", new CreatureValues { spawnCost = 2, prefabName = "Neck", spawnType = COMMON, biome = MEADOWS, enabled = true, dropsEnabled = false } },
            {"Boar", new CreatureValues {spawnCost = 2, prefabName = "Boar", spawnType = COMMON, biome = MEADOWS, enabled = true, dropsEnabled = false } },
            {"Deer", new CreatureValues {spawnCost = 2, prefabName = "Deer", spawnType = COMMON, biome = MEADOWS, enabled = false, dropsEnabled = false } },
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
            {"Leech", new CreatureValues {spawnCost = 8, prefabName = "Leech", spawnType = COMMON, biome = SWAMP, enabled = true, dropsEnabled = false} },
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
            {"Fenring", new CreatureValues {spawnCost = 28, prefabName = "Fenring", spawnType = RARE, biome = MOUNTAIN, enabled = true, dropsEnabled = false} },
            {"Ulv", new CreatureValues {spawnCost = 20, prefabName = "Ulv", spawnType = COMMON, biome = MOUNTAIN, enabled = true, dropsEnabled = false} },
            {"Cultist", new CreatureValues {spawnCost = 40, prefabName = "Fenring_Cultist", spawnType = RARE, biome = MOUNTAIN, enabled = true, dropsEnabled = false} },
            {"StoneGolem", new CreatureValues {spawnCost = 50, prefabName = "StoneGolem", spawnType = ELITE, biome = MOUNTAIN, enabled = true, dropsEnabled = false} },
            // Plains Creatures
            {"Deathsquito", new CreatureValues {spawnCost = 20, prefabName = "Deathsquito", spawnType = COMMON, biome = PLAINS, enabled = true, dropsEnabled = false} },
            {"Fuling", new CreatureValues {spawnCost = 15, prefabName = "Goblin", spawnType = COMMON, biome = PLAINS, enabled = true, dropsEnabled = false} },
            {"FulingArcher", new CreatureValues {spawnCost = 20, prefabName = "GoblinArcher", spawnType = COMMON, biome = PLAINS, enabled = true, dropsEnabled = false} },
            {"FulingBerserker", new CreatureValues {spawnCost = 45, prefabName = "GoblinBrute", spawnType = ELITE, biome = PLAINS, enabled = true, dropsEnabled = false} },
            {"FulingShaman", new CreatureValues {spawnCost = 40, prefabName = "GoblinShaman", spawnType = RARE, biome = PLAINS, enabled = true, dropsEnabled = false} },
            {"Growth", new CreatureValues {spawnCost = 35, prefabName = "BlobTar", spawnType = RARE, biome = PLAINS, enabled = true, dropsEnabled = false} },
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
                true, 12, 200).Value;

            max_creature_stars = cfg.BindServerConfig(
                "shine of challenge - levels",
                "max_creature_stars",
                (short)2,
                "This is the max number of stars a creature can have. CLLC is required for anything over 2.",
                true, 0, 5).Value;
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

        public static PhasedWaveTemplate generateRandomWaveWithOptions(ChallengeLevelDefinition levelDefinition, bool hard_mode, bool boss_mode, bool siege_mode, short override_max_creatures = 0)
        {
            // The defined levels definition index is what we use for the actual point calculation- meaning you will fight and get rewards based on that level- not the position of the level definition.
            Int16 wave_total_points = ComputeChallengePoints(levelDefinition.levelIndex);
            if (hard_mode) {
                Jotunn.Logger.LogInfo("Hard mode has been enabled, bigger challenge, bigger reward.");
                wave_total_points = (Int16)(wave_total_points * 1.5);
            }
            if (siege_mode)
            {
                Jotunn.Logger.LogInfo("Siege Mode has been enabled, prepare the ballistas.");
            }
            if (boss_mode)
            {
                Jotunn.Logger.LogInfo("Boss Mode has been enabled, the forsaken will find you.");
            }
            Jotunn.Logger.LogInfo($"Wave Challenge points: {wave_total_points}");
            // Builds out a template for wave generation
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Building wave template from {levelDefinition.waveFormat}"); }
            PhasedWaveTemplate wavedefinition = dynamicBuildWaveTemplate(levelDefinition, wave_total_points, boss_mode, siege_mode, override_max_creatures);

            return wavedefinition;
        }

        public static PhasedWaveTemplate dynamicBuildWaveTemplate(ChallengeLevelDefinition defined_level, Int16 max_wave_points, bool boss_mode, bool siege_mode, short override_max_creatures = 0, int phases = 4)
        {
            WaveOutline waveOutline = new WaveOutline();

            if (siege_mode) { phases = phases * 2; }

            Int16 max_creatures_from_previous_biomes = defined_level.maxCreatureFromPreviousBiomes;
            String selected_biome = defined_level.biome;

            // if (level > 30 && level < 35) { selected_biome = "Ashlands"; }
            short targeted_wave_biome_level = BiomeStringToInt(selected_biome);
            WaveGenerationFormat WaveGenerationFormat = WaveStyles[defined_level.waveFormat];
            if (boss_mode) { WaveGenerationFormat = WaveStyles[defined_level.bossWaveFormat]; }
            Int16 creatures_selected_from_previous_biome = 0;
            if(VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Starting wave generation for {selected_biome} - phases: {phases}"); }

            // Validate if we need to check exclusion/inclusion lists
            bool should_check_only_monsters = false;
            bool should_check_exclude_monsters = false;
            if (defined_level.onlySelectMonsters != null) { if (defined_level.onlySelectMonsters.Count > 0) { should_check_only_monsters = true; } }
            if (defined_level.excludeSelectMonsters != null) { if (defined_level.excludeSelectMonsters.Count > 0) { should_check_exclude_monsters = true; } } 

            foreach (WaveFormatEntry entry in WaveGenerationFormat.waveFormats)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Starting wave creature selection for {entry.SpawnType}-{entry.SpawnPercentage}."); }
                string waveType = entry.SpawnType;
                float percentage = entry.SpawnPercentage;
                bool creature_added = false;
                int creature_add_iterations = 0;
                float duplicate_chance = 0.25f;
                while (creature_added == false)
                {
                    if (creature_add_iterations > 3) { if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogWarning($"{waveType}-{percentage}% failed to find an addition, skipping it."); } break; }
                    // we shuffle the monster keys so that we run through them in a different order each time
                    // This ensures that we fill the wave in different ways
                    List<string> this_iteration_key_order = ValheimFortress.shuffleList(new List<string>(SpawnableCreatures.Keys));
                    
                    foreach (String skey in this_iteration_key_order)
                    {
                        if (SpawnableCreatures[skey].spawnType != waveType) { continue; }
                        // Skip monsters that are not in the onlySelectedMonsters section, if its defined.
                        if (should_check_only_monsters)
                        {
                            if (!defined_level.onlySelectMonsters.Contains(skey)) 
                            {
                                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"creature {skey} is not in the inclusion list, skipping."); }
                                continue;
                            }
                        }
                        if (should_check_exclude_monsters)
                        {
                            if (defined_level.excludeSelectMonsters.Contains(skey)) 
                            {
                                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"creature {skey} is in the exclusion list, skipping."); }
                                continue; 
                            }
                        }
                        
                        short min_stars = 0;
                        // Creature is of the wrong spawntype, skipping

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
                            if (creatures_selected_from_previous_biome >= max_creatures_from_previous_biomes)
                            {
                                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"creature {skey} is from a previous biome, but we already have enough creatures from a previous biome."); }
                                continue;
                            }
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


            List<List<HoardConfig>> finalizedHoards = new List<List<HoardConfig>> { };
            int phases_generated = 0;
            List<HoardConfig> commonCreatures = waveOutline.getCommonCreatures();
            List<HoardConfig> rareCreatures = waveOutline.getRareCreatures();
            List<HoardConfig> eliteCreatures = waveOutline.getEliteCreatures();
            List<HoardConfig> uniqueCreatures = waveOutline.getUniqueCreatures();
            short max_per_wave = max_creatures_per_wave;
            if (override_max_creatures > 0) 
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Overriding max creatures ({max_per_wave}) per wave to {override_max_creatures}"); }
                max_per_wave = override_max_creatures;
            }

            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Building out wave phases from wave outline with COMMON-{commonCreatures.Count} RARE-{rareCreatures.Count} ELITE-{eliteCreatures.Count} UNIQUE-{uniqueCreatures.Count}"); }

            while (phases > phases_generated)
            {
                List<HoardConfig> hoardPhase = new List<HoardConfig>();
                
                short common_creatures_in_wave;
                short rare_creatures_in_wave;
                short elite_creatures_in_wave;
                short unique_creatures_in_wave = 0;
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Generating Phase {phases_generated}"); }

                common_creatures_in_wave = GenerateHordePhase(commonCreatures, defined_level.commonSpawnModifiers, phases, phases_generated, hoardPhase, "COMMON");
                rare_creatures_in_wave = GenerateHordePhase(rareCreatures, defined_level.rareSpawnModifiers, phases, phases_generated, hoardPhase, "RARE");
                elite_creatures_in_wave = GenerateHordePhase(eliteCreatures, defined_level.eliteSpawnModifiers, phases, phases_generated, hoardPhase, "ELITE");

                // Last phase to generate, add the boss
                if ((phases_generated + 1) == phases)
                {
                    foreach(HoardConfig entry in uniqueCreatures)
                    {
                        short spawn_amount = entry.amount;
                        if (spawn_amount <= 0) { spawn_amount = 1; }
                        if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adding UNIQUE entry {entry.creature} - {entry.amount}"); }
                        unique_creatures_in_wave += spawn_amount;
                        hoardPhase.Add(new HoardConfig { creature = entry.creature, prefab = entry.prefab, amount = spawn_amount, stars = entry.stars });
                    }
                }

                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Phase creature selection and modifiers completed"); }

                // Post processing on the wave itself
                short total_creatures_in_wave = (short)(common_creatures_in_wave + rare_creatures_in_wave + elite_creatures_in_wave + unique_creatures_in_wave);
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Checking for wave reduction (max {max_per_wave}): Total {total_creatures_in_wave} = C-{common_creatures_in_wave} R-{rare_creatures_in_wave} U-{unique_creatures_in_wave}"); }
                if (total_creatures_in_wave > max_per_wave)
                {
                    ReduceWaveSizeToMax(hoardPhase, total_creatures_in_wave, max_per_wave);
                }
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Phase post-processing completed"); }

                finalizedHoards.Add(hoardPhase);
                phases_generated += 1;
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Starting next phase..."); }
            }

            PhasedWaveTemplate finalizedWaveGeneration = new PhasedWaveTemplate();
            finalizedWaveGeneration.hordePhases = finalizedHoards;

            if (VFConfig.EnableDebugMode.Value)
            {
                List<List<HoardConfig>> finalizedPhases = finalizedWaveGeneration.hordePhases;
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

        public static short GenerateHordePhase(List<HoardConfig> creatureEntries, Dictionary<String, bool> creature_modifiers, int phases, int current_phase, List<HoardConfig> current_phase_hoard_config, string creatureType)
        {
            short creatureSumCount = 0;
            foreach (HoardConfig entry in creatureEntries)
            {
                float wavePercent = ApplyWavePercentModifiers(creature_modifiers, phases, current_phase);
                if (wavePercent == 0f) { continue; } 
                short spawn_amount = (short)(entry.amount * wavePercent);
                if (spawn_amount <= 0) { spawn_amount = 1; }
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adding {creatureType} entry {entry.creature} - {spawn_amount}"); }
                creatureSumCount += spawn_amount;
                current_phase_hoard_config.Add(new HoardConfig {creature = entry.creature, prefab = entry.prefab, amount = spawn_amount, stars = entry.stars });
            }
            return creatureSumCount;
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
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adjusting wave with linearDecreaseRandomWaveAdjustment percent {current_wave_percentage} for phase {current_phase}"); }
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
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adjusting wave with linearIncreaseRandomWaveAdjustment percent {current_wave_percentage} for phase {current_phase}"); }
            return current_wave_percentage;
        }


        // This is primarily a random fractional adjustment, with a min of 60% and max of 100% unless overtuned
        public static float partialRandomWaveAdjustment(float variance = 0.20f)
        {
            // variance allows this up to 100% or down to 60%
            float random_variance = UnityEngine.Random.Range(variance * -1, variance);
            float current_wave_percentage = (1f - (variance/2)) + random_variance;
            if (current_wave_percentage > 1f) { current_wave_percentage = 1f;}
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adjusting wave with partialRandomWaveAdjustment percent {current_wave_percentage}"); }
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

        public static float onlyGenerateInSecondHalf(int phases, int current_phase)
        {
            float wave_percent = 0f;
            if (current_phase >= (phases / 2))
            {
                wave_percent = 1f;
            }
            return wave_percent;
        }

        public static float ApplyWavePercentModifiers(Dictionary<string, bool> wavePercentModifiers, int phases, int current_phase)
        {
            float wavePercent = 1f;
            // Guard clause for no modifiers
            if (wavePercentModifiers == null)
            {
                return wavePercent;
            } else if (wavePercentModifiers.Count == 0)
            {
                return wavePercent;
            }

            // non-null and contains a key to operate with
            if (wavePercentModifiers.ContainsKey("linearDecreaseRandomWaveAdjustment")) {
                if (wavePercentModifiers["linearDecreaseRandomWaveAdjustment"] == true)
                {
                    wavePercent = linearDecreaseRandomWaveAdjustment(phases, current_phase);
                }
            }

            if (wavePercentModifiers.ContainsKey("linearIncreaseRandomWaveAdjustment")) {
                if (wavePercentModifiers["linearIncreaseRandomWaveAdjustment"] == true)
                {
                    wavePercent = linearIncreaseRandomWaveAdjustment(phases, current_phase);
                }
            }

            if (wavePercentModifiers.ContainsKey("partialRandomWaveAdjustment")) {
                if (wavePercentModifiers["partialRandomWaveAdjustment"] == true)
                {
                    wavePercent = partialRandomWaveAdjustment(0.3f);
                }
            }

            if (wavePercentModifiers.ContainsKey("onlyGenerateInSecondHalf")) {
                if (wavePercentModifiers["onlyGenerateInSecondHalf"] == true)
                {
                    wavePercent = onlyGenerateInSecondHalf(phases, current_phase);
                }
            }
            return wavePercent;
        }


        public static Int16 BiomeStringToInt(String biome)
        {
            if(!biomes.Contains(biome)) { throw new ArgumentException($"Biome {biome} does not match defined biomes: {string.Join(",", biomes)}"); }
            return (Int16)Array.IndexOf(biomes, biome);
        }


        public static List<HoardConfig> ReduceWaveSizeToMax(List<HoardConfig> hoards, short total_creatures_in_wave, short max_per_wave)
        {
            if (total_creatures_in_wave <= max_per_wave) { return hoards; }

            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Wave has more creatures than allowed from configuration to remove: {total_creatures_in_wave} > {max_per_wave}"); }
            short iterations = 0;
            // This won't scale up infinitely in reducing waves, but could produce waves up to 1/4th the size
            while(iterations < 4)
            {
                total_creatures_in_wave = EvaluateAndReduceWave(hoards, total_creatures_in_wave, max_per_wave);
                if (total_creatures_in_wave <= max_per_wave) { break; }
                iterations++;
            }

            return hoards;
        }

        private static short EvaluateAndReduceWave(List<HoardConfig> hoards, short total_creatures_in_wave, short max_per_wave, short stop_reduction_at = 5)
        {
            if (total_creatures_in_wave > max_per_wave)
            {
                foreach (HoardConfig entry in hoards)
                {
                    if (entry.amount <= stop_reduction_at) { continue; }
                    if (total_creatures_in_wave <= max_per_wave) { break; }
                    if (SpawnableCreatures[entry.creature].spawnType == CONST.UNIQUE) { continue; }
                    total_creatures_in_wave = MutatingReduceHoardConfigSize(entry, total_creatures_in_wave, max_per_wave);
                }
            }
            return total_creatures_in_wave;
        }

        private static short MutatingReduceHoardConfigSize(HoardConfig hoard, short total_creatures_in_wave, short max_per_wave)
        {
            if (total_creatures_in_wave > max_per_wave)
            {
                if (hoard.stars < max_creature_stars)
                {
                    short creature_difference = (short)(hoard.amount / 2);
                    hoard.amount = creature_difference;
                    hoard.stars += 1;
                    total_creatures_in_wave = (short)(total_creatures_in_wave - creature_difference);
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"wave {hoard.creature} reduced from {hoard.amount * 2} to {hoard.amount} total creatures in wave now: {total_creatures_in_wave + creature_difference} -> {total_creatures_in_wave}"); }
                }
            }
            return total_creatures_in_wave;
        }
    }
}
