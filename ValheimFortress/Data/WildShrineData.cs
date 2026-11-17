using System;
using System.Collections.Generic;
using System.Linq;
using static ValheimFortress.Data.WaveStyles;

namespace ValheimFortress.Challenge
{
    internal class WildShrineData
    {
        public static List<WildShrineConfiguration> WildLevelDefinitions = new List<WildShrineConfiguration>
        {
            new WildShrineConfiguration
            {
                definitionForWildShrine = "VF_wild_shrine_green1",
                wildShrineNameLocalization = "$wild_shrine_green",
                wildShrineRequestLocalization = "$wild_shrine_green_request",
                shrine_larger_tribute_required_localization = "$wild_shrine_hungry",
                shrine_unaccepted_tribute_localization = "$wild_shrine_not_interested",
                wildShrineLevelsConfig = new List<WildShrineLevelConfiguration>
                {
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyBoar",
                        tributeAmount = 4,
                        rewards = new Dictionary<string, short>
                        {
                            { "LeatherScraps", 14 },
                            { "RawMeat", 12 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_boars_attack",
                        wildshrine_wave_end_localization = "$wild_boars_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 2,
                            maxCreaturesPerPhaseOverride = 15,
                            biome = Heightmap.Biome.Meadows,
                            waveFormat = WaveStyleName.Tutorial,
                            levelWarningLocalization = "$meadows_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Boar", "Greyling" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyDeer",
                        tributeAmount = 3,
                        rewards = new Dictionary<string, short>
                        {
                            { "DeerHide", 16 },
                            { "DeerMeat", 14 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_deer_attack",
                        wildshrine_wave_end_localization = "$wild_deer_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 1,
                            maxCreaturesPerPhaseOverride = 12,
                            biome = Heightmap.Biome.Meadows,
                            waveFormat = WaveStyleName.Tutorial,
                            levelWarningLocalization = "$meadows_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Greyling" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyNeck",
                        tributeAmount = 2,
                        rewards = new Dictionary<string, short>
                        {
                            { "Resin", 14 },
                            { "NeckTail", 10 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_neck_attack",
                        wildshrine_wave_end_localization = "$wild_neck_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 1,
                            maxCreaturesPerPhaseOverride = 12,
                            biome = Heightmap.Biome.Meadows,
                            waveFormat = WaveStyleName.Tutorial,
                            levelWarningLocalization = "$meadows_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Neck" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    }
                }
            },
            new WildShrineConfiguration
            {
                definitionForWildShrine = "VF_wild_shrine_blue1",
                wildShrineNameLocalization = "$wild_shrine_blackforest",
                wildShrineRequestLocalization = "$wild_shrine_blackforest_request",
                shrine_larger_tribute_required_localization = "$wild_shrine_hungry",
                shrine_unaccepted_tribute_localization = "$wild_shrine_not_interested",
                wildShrineLevelsConfig = new List<WildShrineLevelConfiguration>
                {
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyGreydwarf",
                        tributeAmount = 5,
                        rewards = new Dictionary<string, short>
                        {
                            { "Wood", 14 },
                            { "Resin", 18 },
                            { "GreydwarfEye", 16 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_greydwarfs_attack",
                        wildshrine_wave_end_localization = "$wild_greydwarfs_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 3,
                            maxCreaturesPerPhaseOverride = 20,
                            biome = Heightmap.Biome.BlackForest,
                            waveFormat = WaveStyleName.Starter,
                            levelWarningLocalization = "$blackforest_warning_wilderness",
                            onlySelectMonsters = new List<String> { "GreyDwarf" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyGreydwarfBrute",
                        tributeAmount = 2,
                        rewards = new Dictionary<string, short>
                        {
                            { "RoundLog", 16 },
                            { "Flint", 14 },
                            { "GreydwarfEye", 18 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_greydwarfs_attack",
                        wildshrine_wave_end_localization = "$wild_greydwarfs_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 3,
                            maxCreaturesPerPhaseOverride = 20,
                            biome = Heightmap.Biome.BlackForest,
                            waveFormat = WaveStyleName.Easy,
                            levelWarningLocalization = "$blackforest_warning_wilderness",
                            onlySelectMonsters = new List<String> { "GreyDwarf", "GreyDwarfBrute" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyGreydwarfShaman",
                        tributeAmount = 2,
                        rewards = new Dictionary<string, short>
                        {
                            { "GreydwarfEye", 16 },
                            { "Mushroom", 14 },
                            { "CarrotSeeds", 30 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_greydwarfs_attack",
                        wildshrine_wave_end_localization = "$wild_greydwarfs_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 3,
                            maxCreaturesPerPhaseOverride = 20,
                            biome = Heightmap.Biome.BlackForest,
                            waveFormat = WaveStyleName.Easy,
                            levelWarningLocalization = "$blackforest_warning_wilderness",
                            onlySelectMonsters = new List<String> { "GreyDwarf", "GreyDwarfShaman" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyForestTroll",
                        tributeAmount = 4,
                        rewards = new Dictionary<string, short>
                        {
                            { "Amber", 22 },
                            { "Coins", 14 },
                            { "TinOre", 50 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_trolls_attack",
                        wildshrine_wave_end_localization = "$wild_trolls_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 3,
                            maxCreaturesPerPhaseOverride = 20,
                            biome = Heightmap.Biome.BlackForest,
                            waveFormat = WaveStyleName.Easy,
                            levelWarningLocalization = "$blackforest_warning_wilderness",
                            onlySelectMonsters = new List<String> { "GreyDwarf", "Troll" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophySkeleton",
                        tributeAmount = 4,
                        rewards = new Dictionary<string, short>
                        {
                            { "BoneFragments", 16 },
                            { "MushroomYellow", 14 },
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_skeleton_attack",
                        wildshrine_wave_end_localization = "$wild_skeleton_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 4,
                            maxCreaturesPerPhaseOverride = 20,
                            biome = Heightmap.Biome.BlackForest,
                            waveFormat = WaveStyleName.Starter,
                            levelWarningLocalization = "$blackforest_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Skeleton", "SkeletonArcher" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophySkeletonPoison",
                        tributeAmount = 1,
                        rewards = new Dictionary<string, short>
                        {
                            { "BoneFragments", 16 },
                            { "Thistle", 14 },
                            { "CopperOre", 50 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_skeleton_attack",
                        wildshrine_wave_end_localization = "$wild_skeleton_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 3,
                            maxCreaturesPerPhaseOverride = 20,
                            biome = Heightmap.Biome.BlackForest,
                            waveFormat = WaveStyleName.Easy,
                            levelWarningLocalization = "$blackforest_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Skeleton", "SkeletonArcher", "RancidSkeleton" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                }
            },
            new WildShrineConfiguration
            {
                definitionForWildShrine = "VF_wild_shrine_green2",
                wildShrineNameLocalization = "$wild_shrine_swamp",
                wildShrineRequestLocalization = "$wild_shrine_swamp_request",
                shrine_larger_tribute_required_localization = "$wild_shrine_hungry",
                shrine_unaccepted_tribute_localization = "$wild_shrine_not_interested",
                wildShrineLevelsConfig = new List<WildShrineLevelConfiguration>
                {
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyLeech",
                        tributeAmount = 5,
                        rewards = new Dictionary<string, short>
                        {
                            { "Guck", 30 },
                            { "Bloodbag", 18 },
                            { "Entrails", 16 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_undead_attack",
                        wildshrine_wave_end_localization = "$wild_undead_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 7,
                            maxCreaturesPerPhaseOverride = 20,
                            biome = Heightmap.Biome.Swamp,
                            waveFormat = WaveStyleName.Normal,
                            levelWarningLocalization = "$swamp_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Draugr", "DraugrArcher", "Leech" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyDraugr",
                        tributeAmount = 3,
                        rewards = new Dictionary<string, short>
                        {
                            { "ElderBark", 18 },
                            { "Entrails", 18 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_undead_attack",
                        wildshrine_wave_end_localization = "$wild_undead_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 5,
                            maxCreaturesPerPhaseOverride = 20,
                            biome = Heightmap.Biome.Swamp,
                            waveFormat = WaveStyleName.Normal,
                            levelWarningLocalization = "$swamp_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Draugr", "DraugrArcher" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyDraugrElite",
                        tributeAmount = 3,
                        rewards = new Dictionary<string, short>
                        {
                            { "AmberPearl", 18 },
                            { "IronScrap", 60 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_undead_attack",
                        wildshrine_wave_end_localization = "$wild_undead_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 8,
                            maxCreaturesPerPhaseOverride = 20,
                            biome = Heightmap.Biome.Swamp,
                            waveFormat = WaveStyleName.Normal,
                            levelWarningLocalization = "$swamp_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Draugr", "DraugrArcher", "DraugrElite" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyWraith",
                        tributeAmount = 1,
                        rewards = new Dictionary<string, short>
                        {
                            { "Chain", 20 },
                            { "Thistle", 40 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_undead_attack",
                        wildshrine_wave_end_localization = "$wild_undead_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 8,
                            maxCreaturesPerPhaseOverride = 20,
                            biome = Heightmap.Biome.Swamp,
                            waveFormat = WaveStyleName.Normal,
                            levelWarningLocalization = "$swamp_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Draugr", "Wraith" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyAbomination",
                        tributeAmount = 4,
                        rewards = new Dictionary<string, short>
                        {
                            { "Guck", 20 },
                            { "Root", 18 },
                            { "TurnipSeeds", 50 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_undead_attack",
                        wildshrine_wave_end_localization = "$wild_undead_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 7,
                            maxCreaturesPerPhaseOverride = 20,
                            biome = Heightmap.Biome.Swamp,
                            waveFormat = WaveStyleName.Hard,
                            levelWarningLocalization = "$swamp_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Draugr", "DraugrElite", "Abomination" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyBlob",
                        tributeAmount = 4,
                        rewards = new Dictionary<string, short>
                        {
                            { "Ooze", 20 },
                            { "Ruby", 40 },
                            { "Bloodbag", 40 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_undead_attack",
                        wildshrine_wave_end_localization = "$wild_undead_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 8,
                            maxCreaturesPerPhaseOverride = 20,
                            biome = Heightmap.Biome.Swamp,
                            waveFormat = WaveStyleName.Normal,
                            levelWarningLocalization = "$swamp_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Blob", "BlobElite" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophySurtling",
                        tributeAmount = 1,
                        rewards = new Dictionary<string, short>
                        {
                            { "SurtlingCore", 20 },
                            { "Coal", 40 },
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_surtling_attack",
                        wildshrine_wave_end_localization = "$wild_surtling_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 6,
                            maxCreaturesPerPhaseOverride = 20,
                            biome = Heightmap.Biome.Swamp,
                            waveFormat = WaveStyleName.Normal,
                            levelWarningLocalization = "$swamp_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Draugr", "Surtling" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                }
            },
            new WildShrineConfiguration
            {
                definitionForWildShrine = "VF_wild_shrine_blue2",
                wildShrineNameLocalization = "$wild_shrine_mountain",
                wildShrineRequestLocalization = "$wild_shrine_mountain_request",
                shrine_larger_tribute_required_localization = "$wild_shrine_hungry",
                shrine_unaccepted_tribute_localization = "$wild_shrine_not_interested",
                wildShrineLevelsConfig = new List<WildShrineLevelConfiguration>
                {
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyWolf",
                        tributeAmount = 3,
                        rewards = new Dictionary<string, short>
                        {
                            { "WolfPelt", 30 },
                            { "WolfMeat", 18 },
                            { "WolfFang", 40 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_wolves_attack",
                        wildshrine_wave_end_localization = "$wild_wolves_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 12,
                            maxCreaturesPerPhaseOverride = 25,
                            biome = Heightmap.Biome.Mountain,
                            waveFormat = WaveStyleName.Normal,
                            levelWarningLocalization = "$mountain_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Wolf", "Fenring" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyFenring",
                        tributeAmount = 2,
                        rewards = new Dictionary<string, short>
                        {
                            { "OnionSeeds", 60 },
                            { "WolfMeat", 20 },
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_wolves_attack",
                        wildshrine_wave_end_localization = "$wild_wolves_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 14,
                            maxCreaturesPerPhaseOverride = 25,
                            biome = Heightmap.Biome.Mountain,
                            waveFormat = WaveStyleName.Normal,
                            levelWarningLocalization = "$mountain_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Wolf", "Fenring" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophySGolem",
                        tributeAmount = 1,
                        rewards = new Dictionary<string, short>
                        {
                            { "Crystal", 8 },
                            { "Stone", 40 },
                            { "SilverOre", 70 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_golem_attack",
                        wildshrine_wave_end_localization = "$wild_golem_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 14,
                            maxCreaturesPerPhaseOverride = 25,
                            biome = Heightmap.Biome.Mountain,
                            waveFormat = WaveStyleName.Hard,
                            levelWarningLocalization = "$mountain_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Wolf", "IceDrake", "StoneGolem" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyHatchling",
                        tributeAmount = 3,
                        rewards = new Dictionary<string, short>
                        {
                            { "FreezeGland", 22 },
                            { "Obsidian", 40 },
                            { "LeatherScraps", 16 },
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_drake_attack",
                        wildshrine_wave_end_localization = "$wild_drake_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 14,
                            maxCreaturesPerPhaseOverride = 25,
                            biome = Heightmap.Biome.Mountain,
                            waveFormat = WaveStyleName.Normal,
                            levelWarningLocalization = "$mountain_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Bat", "IceDrake" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyUlv",
                        tributeAmount = 3,
                        rewards = new Dictionary<string, short>
                        {
                            { "WolfHairBundle", 20 },
                            { "JuteRed", 50 },
                            { "BoneFragments", 32 },
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_wolves_attack",
                        wildshrine_wave_end_localization = "$wild_wolves_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 14,
                            maxCreaturesPerPhaseOverride = 25,
                            biome = Heightmap.Biome.Mountain,
                            waveFormat = WaveStyleName.Normal,
                            levelWarningLocalization = "$mountain_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Ulv", "Cultist" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyCultist",
                        tributeAmount = 3,
                        rewards = new Dictionary<string, short>
                        {
                            { "WolfHairBundle", 28 },
                            { "JuteRed", 30 },
                            { "BoneFragments", 32 },
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_wolves_attack",
                        wildshrine_wave_end_localization = "$wild_wolves_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 16,
                            maxCreaturesPerPhaseOverride = 25,
                            biome = Heightmap.Biome.Mountain,
                            waveFormat = WaveStyleName.Normal,
                            levelWarningLocalization = "$mountain_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Ulv", "Cultist" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                }
            },
            new WildShrineConfiguration
            {
                definitionForWildShrine = "VF_wild_shrine_yellow1",
                wildShrineNameLocalization = "$wild_shrine_plains",
                wildShrineRequestLocalization = "$wild_shrine_plains_request",
                shrine_larger_tribute_required_localization = "$wild_shrine_hungry",
                shrine_unaccepted_tribute_localization = "$wild_shrine_not_interested",
                wildShrineLevelsConfig = new List<WildShrineLevelConfiguration>
                {
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyGoblin",
                        tributeAmount = 4,
                        rewards = new Dictionary<string, short>
                        {
                            { "Coins", 15 },
                            { "BlackMetalScrap", 45 },
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_fulings_attack",
                        wildshrine_wave_end_localization = "$wild_fulings_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 16,
                            maxCreaturesPerPhaseOverride = 25,
                            biome = Heightmap.Biome.Plains,
                            waveFormat = WaveStyleName.Normal,
                            levelWarningLocalization = "$plains_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Fuling", "FulingArcher", "FulingShaman" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyGoblinShaman",
                        tributeAmount = 2,
                        rewards = new Dictionary<string, short>
                        {
                            { "Coins", 25 },
                            { "BlackMetalScrap", 35 },
                            { "Barley", 45 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_fulings_attack",
                        wildshrine_wave_end_localization = "$wild_fulings_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 17,
                            maxCreaturesPerPhaseOverride = 25,
                            biome = Heightmap.Biome.Plains,
                            waveFormat = WaveStyleName.Normal,
                            levelWarningLocalization = "$plains_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Fuling", "FulingArcher", "FulingShaman" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyGoblinBrute",
                        tributeAmount = 1,
                        rewards = new Dictionary<string, short>
                        {
                            { "Coins", 25 },
                            { "BlackMetalScrap", 35 },
                            { "Flax", 45 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_fulings_attack",
                        wildshrine_wave_end_localization = "$wild_fulings_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 16,
                            maxCreaturesPerPhaseOverride = 25,
                            biome = Heightmap.Biome.Plains,
                            waveFormat = WaveStyleName.Hard,
                            levelWarningLocalization = "$plains_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Fuling", "FulingArcher", "FulingShaman", "FulingBerserker" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyGrowth",
                        tributeAmount = 2,
                        rewards = new Dictionary<string, short>
                        {
                            { "Coins", 25 },
                            { "BlackMetalScrap", 35 },
                            { "Tar", 16 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_fulings_attack",
                        wildshrine_wave_end_localization = "$wild_fulings_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 16,
                            maxCreaturesPerPhaseOverride = 25,
                            biome = Heightmap.Biome.Plains,
                            waveFormat = WaveStyleName.Normal,
                            levelWarningLocalization = "$plains_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Fuling", "FulingArcher", "Growth" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                }
            },
            new WildShrineConfiguration
            {
                definitionForWildShrine = "VF_wild_shrine_purple1",
                wildShrineNameLocalization = "$wild_shrine_mistlands",
                wildShrineRequestLocalization = "$wild_shrine_mistlands_request",
                shrine_larger_tribute_required_localization = "$wild_shrine_hungry",
                shrine_unaccepted_tribute_localization = "$wild_shrine_not_interested",
                wildShrineLevelsConfig = new List<WildShrineLevelConfiguration>
                {
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyHare",
                        tributeAmount = 2,
                        rewards = new Dictionary<string, short>
                        {
                            { "HareMeat", 35 },
                            { "ScaleHide", 45 },
                            { "CopperScrap", 65 },
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_seekers_attack",
                        wildshrine_wave_end_localization = "$wild_seekers_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 20,
                            maxCreaturesPerPhaseOverride = 25,
                            biome = Heightmap.Biome.Mistlands,
                            waveFormat = WaveStyleName.Normal,
                            levelWarningLocalization = "$mistlands_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Seeker", "Tick", "DvergerRouge" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophySeeker",
                        tributeAmount = 3,
                        rewards = new Dictionary<string, short>
                        {
                            { "BugMeat", 55 },
                            { "Carapace", 45 },
                            { "RoyalJelly", 35 },
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_seekers_attack",
                        wildshrine_wave_end_localization = "$wild_seekers_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 20,
                            maxCreaturesPerPhaseOverride = 25,
                            biome = Heightmap.Biome.Mistlands,
                            waveFormat = WaveStyleName.Hard,
                            levelWarningLocalization = "$mistlands_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Seeker", "SeekerBrood", "DvergerRouge", "SeekerSoldier" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophySeekerBrute",
                        tributeAmount = 1,
                        rewards = new Dictionary<string, short>
                        {
                            { "Sap", 55 },
                            { "IronScrap", 45 },
                            { "Mandible", 65 },
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_seekers_attack",
                        wildshrine_wave_end_localization = "$wild_seekers_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 20,
                            maxCreaturesPerPhaseOverride = 25,
                            biome = Heightmap.Biome.Mistlands,
                            waveFormat = WaveStyleName.Hard,
                            levelWarningLocalization = "$mistlands_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Seeker", "SeekerBrood", "DvergerRouge", "SeekerSoldier" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyGjall",
                        tributeAmount = 3,
                        rewards = new Dictionary<string, short>
                        {
                            { "Chain", 55 },
                            { "Bilebag", 45 },
                            { "BlackCore", 65 },
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_seekers_attack",
                        wildshrine_wave_end_localization = "$wild_seekers_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 20,
                            maxCreaturesPerPhaseOverride = 25,
                            biome = Heightmap.Biome.Mistlands,
                            waveFormat = WaveStyleName.Hard,
                            levelWarningLocalization = "$mistlands_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Seeker", "Tick", "DvergerMageFire", "Gjall" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyTick",
                        tributeAmount = 2,
                        rewards = new Dictionary<string, short>
                        {
                            { "SurtlingCore", 55 },
                            { "GiantBloodSack", 45 },
                            { "Softtissue", 65 },
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_seekers_attack",
                        wildshrine_wave_end_localization = "$wild_seekers_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 20,
                            maxCreaturesPerPhaseOverride = 25,
                            biome = Heightmap.Biome.Mistlands,
                            waveFormat = WaveStyleName.Hard,
                            levelWarningLocalization = "$mistlands_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Seeker", "Tick", "DvergerRouge", "DvergerMageSupport", "Gjall" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyDvergr",
                        tributeAmount = 2,
                        rewards = new Dictionary<string, short>
                        {
                            { "MushroomJotunPuffs", 35 },
                            { "JuteBlue", 45 },
                            { "DvergrNeedle", 80 },
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_seekers_attack",
                        wildshrine_wave_end_localization = "$wild_seekers_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 18,
                            maxCreaturesPerPhaseOverride = 25,
                            biome = Heightmap.Biome.Mistlands,
                            waveFormat = WaveStyleName.Dynamic,
                            levelWarningLocalization = "$mistlands_warning_wilderness",
                            onlySelectMonsters = new List<String> { "SeekerBrood", "DvergerRouge", "DvergerMage", "DvergerMageFire", "DvergerMageIce", "DvergerMageSupport" },
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
                        }
                    },
                }
            },
            new WildShrineConfiguration
            {
                definitionForWildShrine = "VF_wild_shrine_red1",
                wildShrineNameLocalization = "$wild_shrine_red",
                wildShrineRequestLocalization = "$wild_shrine_red_request",
                shrine_larger_tribute_required_localization = "$wild_shrine_hungry",
                shrine_unaccepted_tribute_localization = "$wild_shrine_not_interested",
                wildShrineLevelsConfig = new List<WildShrineLevelConfiguration>
                {
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyCharredMelee",
                        tributeAmount = 3,
                        rewards = new Dictionary<string, short>
                        {
                            {"CharcoalResin", 20},
                            {"FlametalOreNew", 20},
                            {"MushroomSmokePuff", 50 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_charred_attack",
                        wildshrine_wave_end_localization = "$wild_charred_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex  = 22,
                            maxCreaturesPerPhaseOverride = 26,
                            biome = Heightmap.Biome.AshLands,
                            waveFormat = WaveStyleName.VeryHard,
                            levelWarningLocalization = "$ashlands_warning_wilderness",
                            onlySelectMonsters = new List<String> { "Charred_Melee", "Charred_Archer", "Charred_Mage", "Asksvin" },
                            commonSpawnModifiers = new SpawnModifiers {linearIncreaseRandomWaveAdjustment = true},
                            rareSpawnModifiers = new SpawnModifiers {linearIncreaseRandomWaveAdjustment = true},
                            eliteSpawnModifiers = new SpawnModifiers {linearIncreaseRandomWaveAdjustment = true}
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyAsksvin",
                        tributeAmount = 3,
                        rewards = new Dictionary<string, short>
                        {
                            { "AsksvinMeat", 50},
                            { "AskHide", 50 },
                            { "AskBladder", 50 },
                            { "VoltureEgg", 50},
                            { "VoltureMeat", 50 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_charred_attack",
                        wildshrine_wave_end_localization = "$wild_charred_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 22,
                            maxCreaturesPerPhaseOverride = 26,
                            biome = Heightmap.Biome.AshLands,
                            waveFormat = WaveStyleName.VeryHard,
                            levelWarningLocalization = "$ashlands_warning_wilderness",
                            onlySelectMonsters = new List<string> { "Asksvin", "Volture", "BlobLava"},
                            commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true},
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true},
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true}
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyMorgen",
                        tributeAmount = 3,
                        rewards = new Dictionary<string, short>
                        {
                            { "MorgenHeart", 20},
                            { "MorgenSinew", 20},
                            { "FlametalOreNew", 50 },
                            { "CelestialFeather", 20 },
                            { "MoltenCore", 20}
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_charred_attack",
                        wildshrine_wave_end_localization = "$wild_charred_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 22,
                            maxCreaturesPerPhaseOverride = 26,
                            biome = Heightmap.Biome.AshLands,
                            waveFormat = WaveStyleName.VeryHard,
                            levelWarningLocalization = "$ashlands_warning_wilderness",
                            onlySelectMonsters = new List<string> { "Morgen", "FallenValkyrie", "Charred_Mage" },
                            commonSpawnModifiers = new SpawnModifiers {linearIncreaseRandomWaveAdjustment = true},
                            rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true},
                            eliteSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true}
                        }
                    }
                }
            },
            new WildShrineConfiguration
            {
                definitionForWildShrine = "VF_wild_shrine_blue3",
                wildShrineNameLocalization = "$wild_shrine_blue3",
                wildShrineRequestLocalization = "$wild_shrine_deepnorth_request",
                shrine_larger_tribute_required_localization = "$wild_shrine_hungry",
                shrine_unaccepted_tribute_localization = "$wild_shrine_not_interested",
                wildShrineLevelsConfig = new List<WildShrineLevelConfiguration>
                {
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyBarka",
                        tributeAmount = 3,
                        rewards = new Dictionary<string, short>
                        {
                            {"BarkaBranch", 50 },
                            {"SealHide", 50 },
                            {"SealBlubber", 50 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_barka_attack",
                        wildshrine_wave_end_localization = "$wild_deepnorth_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 22,
                            maxCreaturesPerPhaseOverride = 26,
                            biome = Heightmap.Biome.DeepNorth,
                            waveFormat = WaveStyleName.Normal,
                            levelWarningLocalization = "$deepnorth_warning_wilderness",
                            onlySelectMonsters = new List<string> {"Barka", "Moose"},
                            commonSpawnModifiers = new SpawnModifiers {linearIncreaseRandomWaveAdjustment = true},
                            rareSpawnModifiers = new SpawnModifiers {linearIncreaseRandomWaveAdjustment = true},
                            eliteSpawnModifiers = new SpawnModifiers {linearIncreaseRandomWaveAdjustment = true}
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyJotunWarrior",
                        tributeAmount = 3,
                        rewards = new Dictionary<string, short>
                        {
                            {"MemorialCoal", 100 },
                            {"Leatherstraps", 50 },
                            {"NornThread", 50 },
                            {"BloodGoldKey", 1000 }
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_deepnorth_attack",
                        wildshrine_wave_end_localization = "$wild_deepnorth_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 25,
                            maxCreaturesPerPhaseOverride = 26,
                            biome = Heightmap.Biome.DeepNorth,
                            waveFormat = WaveStyleName.VeryHard,
                            levelWarningLocalization = "$deepnorth_warning_wilderness",
                            onlySelectMonsters = new List<string> { "JotunWarrior", "JotunWitch", "FallenWarrior", "TrollFrost" },
                            commonSpawnModifiers = new SpawnModifiers {linearIncreaseRandomWaveAdjustment = true},
                            rareSpawnModifiers = new SpawnModifiers {linearIncreaseRandomWaveAdjustment = true},
                            eliteSpawnModifiers = new SpawnModifiers {linearIncreaseRandomWaveAdjustment = true}
                        }
                    },
                    new WildShrineLevelConfiguration
                    {
                        tributeName = "TrophyElaking",
                        tributeAmount = 3,
                        rewards = new Dictionary<string, short>
                        {
                            {"ElakingHairBundle", 50},
                            {"MoleClaws", 50},
                            {"MoldKeys", 160},
                            {"OozeMork", 50},
                            {"NornThread", 50}
                        },
                        hardMode = false,
                        siegeMode = false,
                        wildshrine_wave_start_localization = "$wild_deepnorth_attack",
                        wildshrine_wave_end_localization = "$wild_deepnorth_defeated",
                        wildLevelDefinition = new WildLevelDefinition
                        {
                            levelIndex = 25,
                            maxCreaturesPerPhaseOverride = 26,
                            biome = Heightmap.Biome.DeepNorth,
                            waveFormat = WaveStyleName.VeryHard,
                            levelWarningLocalization = "$deepnorth_warning_wilderness",
                            onlySelectMonsters = new List<string> { "Elaking", "BlobMork", "ElakingMole", "JotunWitch" },
                            commonSpawnModifiers = new SpawnModifiers {linearIncreaseRandomWaveAdjustment = true},
                            rareSpawnModifiers = new SpawnModifiers {linearIncreaseRandomWaveAdjustment = true},
                            eliteSpawnModifiers = new SpawnModifiers {linearIncreaseRandomWaveAdjustment = true}
                        }
                    }
                }
            }
        };

        private static readonly List<WildShrineConfiguration> DefaultDefinitions = new List<WildShrineConfiguration>(WildLevelDefinitions);

        public static WildShrineConfiguration GetWildShrineConfigurationForSpecificShrine(string shrine_name)
        {
            foreach (WildShrineConfiguration entry in WildLevelDefinitions)
            {
                if (entry.definitionForWildShrine.ToLower() == shrine_name.ToLower())
                {
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Wildshrine entry {entry.definitionForWildShrine.ToLower()} match for {shrine_name.ToLower()}"); }
                    return entry;
                }
            }
            return null;
        }

        public static List<WildShrineConfiguration> GetWildShrineDefinitions()
        {
            return WildLevelDefinitions;
        }

        public static void UpdateWildShrineDefinition(WildShrineConfigurationCollection wildShrineDefinitions)
        {
            WildLevelDefinitions.Clear();
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Updating Wildshrines"); }
            foreach (WildShrineConfiguration entry in wildShrineDefinitions.WildShrines)
            {
                WildLevelDefinitions.Add(entry);
                if (entry?.wildShrineLevelsConfig == null) { continue; }
                for (int i = 0; i < entry.wildShrineLevelsConfig.Count; i++)
                {
                    WaveCreatureSettings.Validate(entry.wildShrineLevelsConfig[i]?.wildLevelDefinition?.creatureRules, $"WildShrines.yaml {entry.definitionForWildShrine} level {i + 1}");
                }
            }

            // WildShrines.yaml is only written when it is missing, so one from an older version lacks the shrines added
            // since. Those are still placed in the world, and without settings they throw on every hover.
            List<string> added = new List<string>();
            foreach (WildShrineConfiguration builtin in DefaultDefinitions)
            {
                if (WildLevelDefinitions.Any(entry => string.Equals(entry?.definitionForWildShrine, builtin.definitionForWildShrine, StringComparison.OrdinalIgnoreCase))) { continue; }
                WildLevelDefinitions.Add(builtin);
                added.Add(builtin.definitionForWildShrine);
            }
            if (added.Count > 0) { Jotunn.Logger.LogInfo($"WildShrines.yaml has no entry for {string.Join(", ", added)}; using the built-in settings. Add an entry to change them."); }
        }

        public static string YamlWildShrineDefinition()
        {
            var wildshrineCollection = new WildShrineConfigurationCollection();
            wildshrineCollection.WildShrines = WildLevelDefinitions;
            var yaml = CONST.yamlserializer.Serialize(wildshrineCollection);
            return yaml;
        }
    }
}
