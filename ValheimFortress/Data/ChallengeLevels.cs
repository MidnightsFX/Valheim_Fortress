using System;
using System.Collections.Generic;
using static ValheimFortress.Data.WaveStyles;
using ValheimFortress.Challenge;

namespace ValheimFortress.Data
{
    public class ChallengeLevels
    {
        public static List<ChallengeLevelDefinition> ChallengeLevelDefinitions = new List<ChallengeLevelDefinition>
        {
            new ChallengeLevelDefinition
            {
                // Level index is the internal representation of this level, it will be used to modify the difficulty, this must be greater than 0
                // It does not have to be unique, if multiple levels for the same shrine use the same index, they will be added in order of definition
                // eg: you could have level 9 (black forest) have the same difficulty as level 10 (swamp)
                levelIndex = 1,
                // Shrine type that this level applies to
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                // This is the text used to display the level naming in the shrine level selector
                levelMenuLocalization = "$shrine_menu_meadow",
                // This is the global key required for this label to be displayed, this uses game global keys or NONE
                requiredGlobalKey = "NONE",
                // This is the biome that this level will pull its creature definitions from
                biome = Heightmap.Biome.Meadows,
                // This is the wave style that this wave will generate in
                waveFormat = WaveStyleName.Tutorial,
                // If this wave is set to be a boss wave, this is the wave style it will generate in
                bossWaveFormat = WaveStyleName.TutorialBoss,
                // This is the number of creatures that could be maximally selected from a previou biome
                maxCreatureFromPreviousBiomes = 0,
                // This is the warning text that is displayed when the round starts, for normal- and then following for boss waves
                levelWarningLocalization = "$shrine_warning_meadows",
                bossLevelWarningLocalization = "$shrine_warning_meadows_boss",
                // This is an inclusion list which specifies what creatures are allowed in this wave, this uses keys from the monster.yaml configuration
                onlySelectMonsters = new List<String> { },
                excludeSelectMonsters = new List<String> { },
                // These are the spawn modifiers that are applied to each type of creature spawn
                commonSpawnModifiers = new SpawnModifiers {
                    linearIncreaseRandomWaveAdjustment = true,
                    linearDecreaseRandomWaveAdjustment = false,
                    partialRandomWaveAdjustment = false,
                    onlyGenerateInSecondHalf = false
                },
                rareSpawnModifiers = new SpawnModifiers {
                    linearIncreaseRandomWaveAdjustment = true,
                    linearDecreaseRandomWaveAdjustment = false,
                    partialRandomWaveAdjustment = false,
                    onlyGenerateInSecondHalf = false
                },
                eliteSpawnModifiers = new SpawnModifiers {
                    linearIncreaseRandomWaveAdjustment = true,
                    linearDecreaseRandomWaveAdjustment = false,
                    partialRandomWaveAdjustment = false,
                    onlyGenerateInSecondHalf = false
                },
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 2,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_meadow",
                requiredGlobalKey = "NONE",
                biome = Heightmap.Biome.Meadows,
                waveFormat = WaveStyleName.Tutorial,
                bossWaveFormat = WaveStyleName.TutorialBoss,
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "$shrine_warning_meadows",
                bossLevelWarningLocalization = "$shrine_warning_meadows_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 3,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_meadow",
                requiredGlobalKey = "NONE",
                biome = Heightmap.Biome.Meadows,
                waveFormat = WaveStyleName.Tutorial,
                bossWaveFormat = WaveStyleName.TutorialBoss,
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "$shrine_warning_meadows",
                bossLevelWarningLocalization = "$shrine_warning_meadows_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 4,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_meadow",
                requiredGlobalKey = "NONE",
                biome = Heightmap.Biome.Meadows,
                waveFormat = WaveStyleName.Starter,
                bossWaveFormat = WaveStyleName.TutorialBoss,
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "$shrine_warning_meadows",
                bossLevelWarningLocalization = "$shrine_warning_meadows_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 5,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_meadow",
                requiredGlobalKey = "NONE",
                biome = Heightmap.Biome.Meadows,
                waveFormat = WaveStyleName.Starter,
                bossWaveFormat = WaveStyleName.TutorialBoss,
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "$shrine_warning_meadows",
                bossLevelWarningLocalization = "$shrine_warning_meadows_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 6,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_forest",
                requiredGlobalKey = "defeated_eikthyr",
                biome = Heightmap.Biome.BlackForest,
                waveFormat = WaveStyleName.Easy,
                bossWaveFormat = WaveStyleName.EasyBoss,
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "$shrine_warning_forest",
                bossLevelWarningLocalization = "$shrine_warning_forest_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 7,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_forest",
                requiredGlobalKey = "defeated_eikthyr",
                biome = Heightmap.Biome.BlackForest,
                waveFormat = WaveStyleName.Normal,
                bossWaveFormat = WaveStyleName.EasyBoss,
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "$shrine_warning_forest",
                bossLevelWarningLocalization = "$shrine_warning_forest_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 8,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_forest",
                requiredGlobalKey = "defeated_eikthyr",
                biome = Heightmap.Biome.BlackForest,
                waveFormat = WaveStyleName.Normal,
                bossWaveFormat = WaveStyleName.Boss,
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "$shrine_warning_forest",
                bossLevelWarningLocalization = "$shrine_warning_forest_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 9,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_forest",
                requiredGlobalKey = "defeated_eikthyr",
                biome = Heightmap.Biome.BlackForest,
                waveFormat = WaveStyleName.Normal,
                bossWaveFormat = WaveStyleName.Boss,
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "$shrine_warning_forest",
                bossLevelWarningLocalization = "$shrine_warning_forest_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 10,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_forest",
                requiredGlobalKey = "defeated_eikthyr",
                biome = Heightmap.Biome.BlackForest,
                waveFormat = WaveStyleName.Hard,
                bossWaveFormat = WaveStyleName.Boss,
                maxCreatureFromPreviousBiomes = 0,
                levelWarningLocalization = "$shrine_warning_forest",
                bossLevelWarningLocalization = "$shrine_warning_forest_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 11,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_swamp",
                requiredGlobalKey = "defeated_gdking",
                biome = Heightmap.Biome.Swamp,
                waveFormat = WaveStyleName.Easy,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_swamp",
                bossLevelWarningLocalization = "$shrine_warning_swamp_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 12,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_swamp",
                requiredGlobalKey = "defeated_gdking",
                biome = Heightmap.Biome.Swamp,
                waveFormat = WaveStyleName.Normal,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_swamp",
                bossLevelWarningLocalization = "$shrine_warning_swamp_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 13,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_swamp",
                requiredGlobalKey = "defeated_gdking",
                biome = Heightmap.Biome.Swamp,
                waveFormat = WaveStyleName.Normal,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_swamp",
                bossLevelWarningLocalization = "$shrine_warning_swamp_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 14,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_swamp",
                requiredGlobalKey = "defeated_gdking",
                biome = Heightmap.Biome.Swamp,
                waveFormat = WaveStyleName.Hard,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_swamp",
                bossLevelWarningLocalization = "$shrine_warning_swamp_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 15,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_swamp",
                requiredGlobalKey = "defeated_gdking",
                biome = Heightmap.Biome.Swamp,
                waveFormat = WaveStyleName.Hard,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_swamp",
                bossLevelWarningLocalization = "$shrine_warning_swamp_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 16,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_mountain",
                requiredGlobalKey = "defeated_bonemass",
                biome = Heightmap.Biome.Mountain,
                waveFormat = WaveStyleName.Normal,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mountain",
                bossLevelWarningLocalization = "$shrine_warning_mountain_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 17,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_mountain",
                requiredGlobalKey = "defeated_bonemass",
                biome = Heightmap.Biome.Mountain,
                waveFormat = WaveStyleName.Normal,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mountain",
                bossLevelWarningLocalization = "$shrine_warning_mountain_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 18,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_mountain",
                requiredGlobalKey = "defeated_bonemass",
                biome = Heightmap.Biome.Mountain,
                waveFormat = WaveStyleName.Hard,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mountain",
                bossLevelWarningLocalization = "$shrine_warning_mountain_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 19,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_mountain",
                requiredGlobalKey = "defeated_bonemass",
                biome = Heightmap.Biome.Mountain,
                waveFormat = WaveStyleName.Hard,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mountain",
                bossLevelWarningLocalization = "$shrine_warning_mountain_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 20,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_mountain",
                requiredGlobalKey = "defeated_bonemass",
                biome = Heightmap.Biome.Mountain,
                waveFormat = WaveStyleName.Expert,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mountain",
                bossLevelWarningLocalization = "$shrine_warning_mountain_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 21,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_plains",
                requiredGlobalKey = "defeated_dragon",
                biome = Heightmap.Biome.Plains,
                waveFormat = WaveStyleName.Normal,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_plains",
                bossLevelWarningLocalization = "$shrine_warning_plains_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 22,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_plains",
                requiredGlobalKey = "defeated_dragon",
                biome = Heightmap.Biome.Plains,
                waveFormat = WaveStyleName.Hard,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_plains",
                bossLevelWarningLocalization = "$shrine_warning_plains_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 23,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_plains",
                requiredGlobalKey = "defeated_dragon",
                biome = Heightmap.Biome.Plains,
                waveFormat = WaveStyleName.VeryHard,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_plains",
                bossLevelWarningLocalization = "$shrine_warning_plains_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 24,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_plains",
                requiredGlobalKey = "defeated_dragon",
                biome = Heightmap.Biome.Plains,
                waveFormat = WaveStyleName.Normal,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_plains",
                bossLevelWarningLocalization = "$shrine_warning_plains_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 25,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_plains",
                requiredGlobalKey = "defeated_dragon",
                biome = Heightmap.Biome.Plains,
                waveFormat = WaveStyleName.Expert,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_plains",
                bossLevelWarningLocalization = "$shrine_warning_plains_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 26,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_mistland",
                requiredGlobalKey = "defeated_goblinking",
                biome = Heightmap.Biome.Mistlands,
                waveFormat = WaveStyleName.Hard,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mistlands",
                bossLevelWarningLocalization = "$shrine_warning_mistlands_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 27,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_mistland",
                requiredGlobalKey = "defeated_goblinking",
                biome = Heightmap.Biome.Mistlands,
                waveFormat = WaveStyleName.VeryHard,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mistlands",
                bossLevelWarningLocalization = "$shrine_warning_mistlands_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 28,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_mistland",
                requiredGlobalKey = "defeated_goblinking",
                biome = Heightmap.Biome.Mistlands,
                waveFormat = WaveStyleName.Expert,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mistlands",
                bossLevelWarningLocalization = "$shrine_warning_mistlands_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 29,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_mistland",
                requiredGlobalKey = "defeated_goblinking",
                biome = Heightmap.Biome.Mistlands,
                waveFormat = WaveStyleName.Extreme,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mistlands",
                bossLevelWarningLocalization = "$shrine_warning_mistlands_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 30,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_mistland",
                requiredGlobalKey = "defeated_goblinking",
                biome = Heightmap.Biome.Mistlands,
                waveFormat = WaveStyleName.Dynamic,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_mistlands",
                bossLevelWarningLocalization = "$shrine_warning_mistlands_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 31,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_ashland",
                requiredGlobalKey = "defeated_fader",
                biome = Heightmap.Biome.AshLands,
                waveFormat = WaveStyleName.Hard,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_ashlands",
                bossLevelWarningLocalization = "$shrine_warning_ashlands_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 32,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_ashland",
                requiredGlobalKey = "defeated_fader",
                biome = Heightmap.Biome.AshLands,
                waveFormat = WaveStyleName.VeryHard,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_ashlands",
                bossLevelWarningLocalization = "$shrine_warning_ashlands_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 33,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_ashland",
                requiredGlobalKey = "defeated_fader",
                biome = Heightmap.Biome.AshLands,
                waveFormat = WaveStyleName.Expert,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_ashlands",
                bossLevelWarningLocalization = "$shrine_warning_ashlands_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 34,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_ashland",
                requiredGlobalKey = "defeated_fader",
                biome = Heightmap.Biome.AshLands,
                waveFormat = WaveStyleName.Extreme,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_ashlands",
                bossLevelWarningLocalization = "$shrine_warning_ashlands_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 35,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, true },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_ashland",
                requiredGlobalKey = "defeated_fader",
                biome = Heightmap.Biome.AshLands,
                waveFormat = WaveStyleName.Dynamic,
                bossWaveFormat = WaveStyleName.DynamicBoss,
                maxCreatureFromPreviousBiomes = 1,
                levelWarningLocalization = "$shrine_warning_ashlands",
                bossLevelWarningLocalization = "$shrine_warning_ashlands_boss",
                commonSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                rareSpawnModifiers = new SpawnModifiers { linearIncreaseRandomWaveAdjustment = true },
                eliteSpawnModifiers = new SpawnModifiers { onlyGenerateInSecondHalf = true }
            },
            new ChallengeLevelDefinition
            {
                levelIndex = 9,
                levelForShrineTypes = new Dictionary<ShrineType, bool> {
                    { ShrineType.Challenge, false },
                    { ShrineType.Arena, true }
                },
                levelMenuLocalization = "$shrine_menu_troll_level",
                requiredGlobalKey = "KilledTroll",
                biome = Heightmap.Biome.BlackForest,
                waveFormat = WaveStyleName.ElitesOnly,
                bossWaveFormat = WaveStyleName.DynamicBoss,
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
    }
}
