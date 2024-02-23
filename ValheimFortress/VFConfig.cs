using BepInEx;
using BepInEx.Configuration;
using System;
using System.IO;
using ValheimFortress.Challenge;
using static ValheimFortress.Challenge.Levels;
using Jotunn.Managers;
using Jotunn.Entities;
using System.Collections;

namespace ValheimFortress
{
    class VFConfig
    {
        public static ConfigFile cfg;
        public static ConfigEntry<bool> EnableDebugMode;
        public static ConfigEntry<bool> EnableTurretDebugMode;
        public static ConfigEntry<short> MaxSpawnRange;
        public static ConfigEntry<float> rewardsMultiplier;
        public static ConfigEntry<float> rewardsDifficultyScalar;
        public static ConfigEntry<bool> EnableBossModifier;
        public static ConfigEntry<bool> EnableHardModifier;
        public static ConfigEntry<bool> EnableSiegeModifer;
        public static ConfigEntry<bool> EnableRewardsEstimate;
        public static ConfigEntry<bool> EnableMapPings;
        public static ConfigEntry<short> MaxRewardsPerSecond;
        public static ConfigEntry<short> NotifyCreatureThreshold;
        public static ConfigEntry<short> TeleportCreatureThreshold;
        public static ConfigEntry<float> ShrineAnnouncementRange;
        public static ConfigEntry<float> DistanceBetweenShrines;
        public static ConfigEntry<short> NumberOfEachWildShrine;

        private static CustomRPC monsterSyncRPC;
        private static CustomRPC rewardSyncRPC;
        private static CustomRPC WavesSyncRPC;
        private static CustomRPC LevelsSyncRPC;
        private static CustomRPC WildShrineSyncRPC;

        private static String rewardFilePath = Path.Combine(Paths.ConfigPath, "VFortress", "Rewards.yaml");
        private static String creatureFilePath = Path.Combine(Paths.ConfigPath, "VFortress", "SpawnableCreatures.yaml");
        private static String waveStylesFilePath = Path.Combine(Paths.ConfigPath, "VFortress", "WaveStyles.yaml");
        private static String levelDefinitionsFilePath = Path.Combine(Paths.ConfigPath, "VFortress", "Levels.yaml");
        private static String wildShrineConfigurationFilePath = Path.Combine(Paths.ConfigPath, "VFortress", "WildShrines.yaml");



        public VFConfig(ConfigFile Config)
        {
            // Init with the default plugin config file
            cfg = Config;
            cfg.SaveOnConfigSet = true;
            CreateConfigValues(Config);
            var mainfilepath = Paths.ConfigPath;
            // = Path.Combine(, $"{ValheimFortress.PluginGUID}.cfg");
            FileSystemWatcher maincfgFSWatcher = new FileSystemWatcher();
            maincfgFSWatcher.Path = mainfilepath;
            maincfgFSWatcher.NotifyFilter = NotifyFilters.LastWrite;
            maincfgFSWatcher.Filter = $"{ValheimFortress.PluginGUID}.cfg";
            maincfgFSWatcher.Changed += new FileSystemEventHandler(UpdateMainConfigFile);
            maincfgFSWatcher.Created += new FileSystemEventHandler(UpdateMainConfigFile);
            maincfgFSWatcher.Renamed += new RenamedEventHandler(UpdateMainConfigFile);
            maincfgFSWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            maincfgFSWatcher.EnableRaisingEvents = true;

            Jotunn.Logger.LogInfo("Main config filewatcher initialized.");
        }


        public void SetupConfigRPCs()
        {
            monsterSyncRPC = NetworkManager.Instance.AddRPC("monsteryaml_rpc", OnServerRecieveConfigs, OnClientReceiveCreatureConfigs);
            rewardSyncRPC = NetworkManager.Instance.AddRPC("rewardsyaml_rpc", OnServerRecieveConfigs, OnClientReceiveRewardsConfigs);
            WavesSyncRPC = NetworkManager.Instance.AddRPC("rewardsyaml_rpc", OnServerRecieveConfigs, OnClientReceiveWaveConfigs);
            LevelsSyncRPC = NetworkManager.Instance.AddRPC("levelsyaml_rpc", OnServerRecieveConfigs, OnClientReceiveLevelConfigs);
            WildShrineSyncRPC = NetworkManager.Instance.AddRPC("wildshrineyaml_rpc", OnServerRecieveConfigs, OnClientReceiveWildShrineConfigs);

            SynchronizationManager.Instance.AddInitialSynchronization(monsterSyncRPC, SendCreatureConfigs);
            SynchronizationManager.Instance.AddInitialSynchronization(rewardSyncRPC, SendRewardsConfigs);
            SynchronizationManager.Instance.AddInitialSynchronization(WavesSyncRPC, SendWavesConfigs);
            SynchronizationManager.Instance.AddInitialSynchronization(LevelsSyncRPC, SendLevelsConfigs);
            SynchronizationManager.Instance.AddInitialSynchronization(WildShrineSyncRPC, SendWildShrineConfigs);
        }

        public static string GetSecondaryConfigDirectoryPath()
        {
            var patchesFolderPath = Path.Combine(Paths.ConfigPath, "VFortress");
            var dirInfo = Directory.CreateDirectory(patchesFolderPath);

            return dirInfo.FullName;
        }

        public static void GetYamlConfigFiles()
        {
            string externalConfigFolder = VFConfig.GetSecondaryConfigDirectoryPath();
            bool hasRewardsConfig = false;
            bool hasCreatureConfig = false;
            bool hasWaveStylesConfig = false;
            bool hasLevelsConfig = false;
            bool hasWildShrinesConfig = false;

            string[] presentFiles = Directory.GetFiles(externalConfigFolder);

            foreach (string configFile in presentFiles)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Config file found: {configFile}"); }
                if (configFile.Contains("Rewards.yaml"))
                {
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Found rewards configuration: {configFile}"); }
                    rewardFilePath = configFile;
                    hasRewardsConfig = true;
                }
                if (configFile.Contains("SpawnableCreatures.yaml"))
                {
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Found Creature configuration: {configFile}"); }
                    creatureFilePath = configFile;
                    hasCreatureConfig = true;
                }
                if (configFile.Contains("WaveStyles.yaml"))
                {
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Found WaveStyles configuration: {configFile}"); }
                    waveStylesFilePath = configFile;
                    hasWaveStylesConfig = true;
                }
                if (configFile.Contains("Levels.yaml"))
                {
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Found Levels configuration: {configFile}"); }
                    levelDefinitionsFilePath = configFile;
                    hasLevelsConfig = true;
                }
                if (configFile.Contains("WildShrines.yaml"))
                {
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Found WildShrine configuration: {configFile}"); }
                    wildShrineConfigurationFilePath = configFile;
                    hasWildShrinesConfig = true;
                }
            }

            if (hasRewardsConfig == false)
            {
                Jotunn.Logger.LogInfo("Rewards file missing, recreating.");
                using (StreamWriter writetext = new StreamWriter(rewardFilePath))
                {
                    // writetext.WriteLine(ValheimFortress.ReadEmbeddedResourceFile("Rewards.yaml"));
                    String header = @"#################################################
# Shrine of Challenge Rewards Configuration
#################################################
# Rewards configurations have a number of key values
#  Coin:                                 |- The name of the reward, this will be the diplayed name if there is no localization for this reward, which is likely the case for any custom entries.
#    enabled: true                       |- Whether or not the reward is enabled, you can use this to disable any vanilla rewards you do not want. At least 1 reward must be available at ALL times.
#    resource_cost: 5                    |- This is the cost to gain 1 of the particular reward. Points are generated based on how many monsters are spawned.
#    resource_prefab: ""Coins""            |- This is the unity prefab name for a resource, you will often see mods list the prefabs they have added. Prefabs are also listed on the valheim wiki.
#    required_boss: ""None""               |- This must be one of the following values: ""None"" ""Eikythr"" ""TheElder"" ""BoneMass"" ""Moder"" ""Yagluth"" ""TheQueen""";
                    writetext.WriteLine(header);
                    writetext.WriteLine(RewardsData.YamlRewardsDefinition());
                }
            }
            if (hasWaveStylesConfig == false)
            {
                Jotunn.Logger.LogInfo("WaveStyles file missing, recreating.");
                using (StreamWriter writetext = new StreamWriter(waveStylesFilePath))
                {
                    // ValheimFortress.ReadEmbeddedResourceFile("SpawnableCreatures.yaml")
                    String header = @"#################################################
# Shrine of Challenge WaveStyles Configuration
#################################################
# WaveStyles configurations have a number of key values
# Easy:                      |- This is the key used to lookup this wave definition
#  WaveConfig                |- The wave configuration for each segment of the wave
#   - type: COMMON           |- This is the catagory of creature that will be selected
#     percent: 30            |- This is the percentage of the waves total point pool that will be used for this spawn";
                    writetext.WriteLine(header);
                    writetext.WriteLine(YamlWaveDefinition());
                }
            }

            if (hasLevelsConfig == false)
            {
                Jotunn.Logger.LogInfo("CreatureConfig file missing, recreating.");
                using (StreamWriter writetext = new StreamWriter(levelDefinitionsFilePath))
                {
                    // ValheimFortress.ReadEmbeddedResourceFile("SpawnableCreatures.yaml")
                    String header = @"#################################################
# Shrines of Challenge Levels Configuration
#################################################
# levels:
# - levelIndex: 1                                                  |- LevelIndex is the difficulty this wave is set at, valid values are 1+
#   levelForShrineTypes:                                           |- What shrines will host this level, multiple definitions can be applied
#     challenge: true                                              |-   Shrine of challenge will host this level
#     arena: true                                                  |-   Shrine of the arena will host this level
#   levelMenuLocalization: $shrine_menu_meadow                     |- This is the localization that will be displayed when selecting the level, if no key matches the $lookup the literal string will be used
#   requiredGlobalKey: NONE                                        |- This is the global key required to unlock this level more available here (https://valheim.fandom.com/wiki/Global_Keys)
#   biome: Meadows                                                 |- This is the biome used for this level. This determines what creatures are considered
#   waveFormat: Tutorial                                           |- This is the format of the wave, formates are defined in WaveStyles.yaml, it determines how many creatures, what catagory and percentage of total points they use
#   bossWaveFormat: TutorialBoss                                   |- This is the format if the wave is modified to be a boss wave
#   maxCreatureFromPreviousBiomes: 0                               |- This is the maximum number of creatures that can be selected from prior biomes
#   levelWarningLocalization: $shrine_warning_meadows              |- This is the announcement text that plays when the challenge starts as a normal wave, uses literal value if the localization does not exist
#   bossLevelWarningLocalization: $shrine_warning_meadows_boss     |- This is the announcement text that plays when the challenge starts as a boss wave, localizations are available here https://github.com/MidnightsFX/Valheim_Fortress/blob/master/JotunnModStub/Localizations/English.json
#   onlySelectMonsters: []                                         |- This is an array of monsters that are the only valid targets for this wave
#   excludeSelectMonsters: []                                      |- This is an array of monsters that are to be avoided for the wave
#   commonSpawnModifiers:                                          |- Spawn modifiers are functions applied to each part of the wave, they can be different per catagory of monster
#     linearIncreaseRandomWaveAdjustment: true                     |-   In general, it is best to only use one type of spawn modifier per creature type
#     linearDecreaseRandomWaveAdjustment: false                    |- Linear Decrease/Increase will frontload or backload this creature in the various phase of the wave, meaning more of it will appear earlier or later depending on the modifier
#     partialRandomWaveAdjustment: false                           |- Partial random adjustment will add more significant random variance to the number of creatures that will spawn
#     onlyGenerateInSecondHalf: false                              |- Only generate in second half will prevent this type of creature from spawning in the earlier waves, this is useful for Elites/Rares when LinearDecrease is set for commons
#   rareSpawnModifiers:                                            |-   The start of the wave will have many commons, and they will taper off till the end, while elites would come into play only on the second half of the wave
#     linearIncreaseRandomWaveAdjustment: true
#     linearDecreaseRandomWaveAdjustment: false
#     partialRandomWaveAdjustment: false
#     onlyGenerateInSecondHalf: false
#   eliteSpawnModifiers:
#     linearIncreaseRandomWaveAdjustment: true
#     linearDecreaseRandomWaveAdjustment: false
#     partialRandomWaveAdjustment: false
#     onlyGenerateInSecondHalf: false
#   uniqueSpawnModifiers: ";
                    writetext.WriteLine(header);
                    writetext.WriteLine(YamlLevelsDefinition());
                }
            }

            if (hasCreatureConfig == false)
            {
                Jotunn.Logger.LogInfo("CreatureConfig file missing, recreating.");
                using (StreamWriter writetext = new StreamWriter(creatureFilePath))
                {
                    // ValheimFortress.ReadEmbeddedResourceFile("SpawnableCreatures.yaml")
                    String header = @"#################################################
# Shrine of Challenge Creature Configuration
#################################################
# Creature configurations have a number of key values
# Neck:                    |- This is the name of the creature being added, it is primarily used for display purposes and lookups
#  spawnCost: 5            |- This is how many points from the wave pool it costs to spawn one creature, smaller values allow many more spawns.
#  prefab: ""Neck""          |- This is the creatures prefab, which will be used to spawn it.
#  spawnType: ""common""     |- This can either be: ""common"" or ""rare"" or ""elite"" or ""unique"", uniques are ""bosses"", most of the wave will be made up of more common enemies
#  enabled: true           |- This controls if this creature will be included in wave-generation.
#  dropsEnabled: false     |- This controls if this particular monster should drop loot. Disabled by default for everything.
#  biome: ""Meadows""        |- This must be one of the following values: ""Meadows"", ""BlackForest"", ""Swamp"", ""Mountain"", ""Plains"", ""Mistlands"". The biome determines the levels that will recieve this spawn, and how the spawn might be adjusted to
#                             fit higher difficulty waves. eg: a greydwarf spawning into a swamp level wave will recieve 1 bonus star, since it is from the black forest, which is 1 biome behind the swamp.";
                    writetext.WriteLine(header);
                    writetext.WriteLine(YamlCreatureDefinition());
                }
            }

            if (hasWildShrinesConfig == false)
            {
                Jotunn.Logger.LogInfo("WildShrineConfig file missing, recreating.");
                using (StreamWriter writetext = new StreamWriter(wildShrineConfigurationFilePath))
                {
                    // ValheimFortress.ReadEmbeddedResourceFile("SpawnableCreatures.yaml")
                    String header = @"###################################################################################################################################################
# Wild Shrine Configuration
###################################################################################################################################################
# wildShrines:
# - definitionForWildShrine: VF_wild_shrine_green1                    |- The prefab that this set of configuration will be applied to
#   wildShrineNameLocalization: $wild_shrine_green                    |- The localization for the prefabs name (when hovered over) this uses a lookup value but defaults to its literal value
#   wildShrineRequestLocalization: $wild_shrine_green_request         |- What the shrine says when you interact with it
#   shrineUnacceptedTributeLocalization: $wild_shrine_not_interested  |- What the shrine says when you offer an incorrect tribute
#   shrineLargerTributeRequiredLocalization: $wild_shrine_hungry      |- What the shrine says when you do not offer enough tribute
#   wildShrineLevelsConfig:                                           |- Level configurations related to this shrine
#   - tributeName: TrophyBoar                                         |- The prefab name of the tribute required to activate this level
#     tributeAmount: 4                                                |- Amount of the tribute required to activate this level
#     rewards:                                                        |- Rewards for this level in the format of Prefab: cost eg: RawMeat: 14.
#       LeatherScraps: 14
#       RawMeat: 12
#     hardMode: false                                                 |- If hardmode should be enabled for this level (doubles the spawn point pool and gives 50% more rewards)
#     siegeMode: false                                                |- If siege mode should be enabled for this level (double the number of waves 4->8 and gives 50% more rewards)
#     wildshrineWaveStartLocalization: $wild_boars_attack             |- Localization text to display when this wave starts
#     wildshrineWaveEndLocalization: $wild_boars_defeated             |- Localization text to display when this wave is finished
#     wildLevelDefinition:
#       levelIndex: 2                                                 |- The difficulty level for this wave, valid values are 1+ (Refer to the readme for a breakdown of this equation)
#       biome: Meadows                                                |- The biome this wave is for, this impacts creature selection
#       waveFormat: Tutorial                                          |- The wavestyle this uses (from wavestyles.yml), this governs which catagories and the percentage makeup of the wave
#       levelWarningLocalization: $meadows_warning_wilderness         |- Localization for a between phase warning (often not used)
#       maxCreaturesPerPhaseOverride: 15                              |- Overrides the max creatures per wave to be this value (overrides the global config)
#       onlySelectMonsters:                                           |- Set of monsters that can be selected (From monsters.yml)
#       - Boar
#       - Greyling
#       excludeSelectMonsters:                                        |- Set of monsters that can't be selected (from monsters.yml) best used when OnlySelected is not set.
#       commonSpawnModifiers:                                         |- Spawn modifiers for common creatures
#         linearIncreaseRandomWaveAdjustment: true
#       rareSpawnModifiers:                                           |- Spawn modifiers for rare creatures
#       eliteSpawnModifiers:                                          |- Spawn modifiers for elite creatures
";
                    writetext.WriteLine(header);
                    writetext.WriteLine(WildShrineData.YamlWildShrineDefinition());
                }
            }
            // now that we have ensured the files exist lets read them
            string spawnableCreatureConfigs = File.ReadAllText(creatureFilePath);
            string rewardConfigs = File.ReadAllText(rewardFilePath);
            string waveStyleConfigs = File.ReadAllText(waveStylesFilePath);
            string LevelsConfigs = File.ReadAllText(levelDefinitionsFilePath);
            string WildShrineConfigs = File.ReadAllText(wildShrineConfigurationFilePath);
            try
            {
                var waveStyleValues = CONST.yamldeserializer.Deserialize<WaveFormatCollection>(waveStyleConfigs);
                UpdateWaveDefinition(waveStyleValues);
            }
            catch (Exception e) { Jotunn.Logger.LogWarning($"There was an error updating the waveStyle values, defaults will be used. Exception: {e}"); }
            try
            {
                var creatureValues = CONST.yamldeserializer.Deserialize<SpawnableCreatureCollection>(spawnableCreatureConfigs);
                UpdateSpawnableCreatures(creatureValues);
            } catch ( Exception e) { Jotunn.Logger.LogWarning($"There was an error updating the creature values, defaults will be used. Exception: {e}"); }
            try
            {
                var rewardsValues = CONST.yamldeserializer.Deserialize<RewardEntryCollection>(rewardConfigs);
                RewardsData.UpdateRewardsEntries(rewardsValues);
            } catch ( Exception e) { Jotunn.Logger.LogWarning($"There was an error updating the rewards values, defaults will be used. Exception: {e}"); }
            try
            {
                var levelsValues = CONST.yamldeserializer.Deserialize<ChallengeLevelDefinitionCollection>(LevelsConfigs);
                Levels.UpdateLevelsDefinition(levelsValues);
            }
            catch (Exception e) { Jotunn.Logger.LogWarning($"There was an error updating the rewards values, defaults will be used. Exception: {e}"); }
            try
            {
                var wildshrinesValues = CONST.yamldeserializer.Deserialize<WildShrineConfigurationCollection>(WildShrineConfigs);
                WildShrineData.UpdateWildShrineDefinition(wildshrinesValues);
            }
            catch (Exception e) { Jotunn.Logger.LogWarning($"There was an error updating the WildShrine values, defaults will be used. Exception: {e}"); }

            // File watcher for the Levels
            FileSystemWatcher levelFSWatcher = new FileSystemWatcher();
            levelFSWatcher.Path = externalConfigFolder;
            levelFSWatcher.NotifyFilter = NotifyFilters.LastWrite;
            levelFSWatcher.Filter = "Levels.yaml";
            levelFSWatcher.Changed += new FileSystemEventHandler(UpdateLevelsConfigFileOnChange);
            levelFSWatcher.Created += new FileSystemEventHandler(UpdateLevelsConfigFileOnChange);
            levelFSWatcher.Renamed += new RenamedEventHandler(UpdateLevelsConfigFileOnChange);
            levelFSWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            levelFSWatcher.EnableRaisingEvents = true;

            // File watcher for the waveStyles
            FileSystemWatcher waveFSWatcher = new FileSystemWatcher();
            waveFSWatcher.Path = externalConfigFolder;
            waveFSWatcher.NotifyFilter = NotifyFilters.LastWrite;
            waveFSWatcher.Filter = "WaveStyles.yaml";
            waveFSWatcher.Changed += new FileSystemEventHandler(UpdateWavesConfigFileOnChange);
            waveFSWatcher.Created += new FileSystemEventHandler(UpdateWavesConfigFileOnChange);
            waveFSWatcher.Renamed += new RenamedEventHandler(UpdateWavesConfigFileOnChange);
            waveFSWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            waveFSWatcher.EnableRaisingEvents = true;

            // File watcher for the creatures
            FileSystemWatcher creatureFSWatcher = new FileSystemWatcher();
            creatureFSWatcher.Path = externalConfigFolder;
            creatureFSWatcher.NotifyFilter = NotifyFilters.LastWrite;
            creatureFSWatcher.Filter = "SpawnableCreatures.yaml";
            creatureFSWatcher.Changed += new FileSystemEventHandler(UpdateCreatureConfigFileOnChange);
            creatureFSWatcher.Created += new FileSystemEventHandler(UpdateCreatureConfigFileOnChange);
            creatureFSWatcher.Renamed += new RenamedEventHandler(UpdateCreatureConfigFileOnChange);
            creatureFSWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            creatureFSWatcher.EnableRaisingEvents = true;

            // File watcher for the Rewards
            FileSystemWatcher rewardsFSWatcher = new FileSystemWatcher();
            rewardsFSWatcher.Path = externalConfigFolder;
            rewardsFSWatcher.NotifyFilter = NotifyFilters.LastWrite;
            rewardsFSWatcher.Filter = "Rewards.yaml";
            rewardsFSWatcher.Changed += new FileSystemEventHandler(UpdateRewardsConfigFileOnChange);
            rewardsFSWatcher.Created += new FileSystemEventHandler(UpdateRewardsConfigFileOnChange);
            rewardsFSWatcher.Renamed += new RenamedEventHandler(UpdateRewardsConfigFileOnChange);
            rewardsFSWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            rewardsFSWatcher.EnableRaisingEvents = true;

            // File watcher for the Rewards
            FileSystemWatcher wildShrineFSWatcher = new FileSystemWatcher();
            wildShrineFSWatcher.Path = externalConfigFolder;
            wildShrineFSWatcher.NotifyFilter = NotifyFilters.LastWrite;
            wildShrineFSWatcher.Filter = "WildShrines.yaml";
            wildShrineFSWatcher.Changed += new FileSystemEventHandler(UpdateWildShrineConfigFileOnChange);
            wildShrineFSWatcher.Created += new FileSystemEventHandler(UpdateRewardsConfigFileOnChange);
            wildShrineFSWatcher.Renamed += new RenamedEventHandler(UpdateRewardsConfigFileOnChange);
            wildShrineFSWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            wildShrineFSWatcher.EnableRaisingEvents = true;
        }

        public static IEnumerator OnServerRecieveConfigs(long sender, ZPackage package)
        {
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Server recieved config from client, rejecting due to being the server."); }
            yield return null;
        }

        private static ZPackage SendCreatureConfigs()
        {
            string spawnableCreatureConfigs = File.ReadAllText(creatureFilePath);
            ZPackage package = new ZPackage();
            package.Write(spawnableCreatureConfigs);
            return package;
        }

        private static ZPackage SendWavesConfigs()
        {
            string waveStylesConfigs = File.ReadAllText(waveStylesFilePath);
            ZPackage package = new ZPackage();
            package.Write(waveStylesConfigs);
            return package;
        }

        private static ZPackage SendRewardsConfigs()
        {
            string rewardsConfigs = File.ReadAllText(rewardFilePath);
            ZPackage package = new ZPackage();
            package.Write(rewardsConfigs);
            return package;
        }

        private static ZPackage SendLevelsConfigs()
        {
            string levelsConfigs = File.ReadAllText(levelDefinitionsFilePath);
            ZPackage package = new ZPackage();
            package.Write(levelsConfigs);
            return package;
        }

        private static ZPackage SendWildShrineConfigs()
        {
            string levelsConfigs = File.ReadAllText(wildShrineConfigurationFilePath);
            ZPackage package = new ZPackage();
            package.Write(levelsConfigs);
            return package;
        }

        private static IEnumerator OnClientReceiveCreatureConfigs(long sender, ZPackage package)
        {
            var creatureyaml = package.ReadString();
            // Just write the updated values to the client. This will trigger an update.
            using (StreamWriter writetext = new StreamWriter(creatureFilePath))
            {
                writetext.WriteLine(creatureyaml);
            }
            yield return null;
        }

        private static IEnumerator OnClientReceiveRewardsConfigs(long sender, ZPackage package)
        {
            var rewardsyaml = package.ReadString();
            // Just write the updated values to the client. This will trigger an update.
            using (StreamWriter writetext = new StreamWriter(rewardFilePath))
            {
                writetext.WriteLine(rewardsyaml);
            }
            yield return null;
        }

        private static IEnumerator OnClientReceiveWaveConfigs(long sender, ZPackage package)
        {
            var wavesyaml = package.ReadString();
            // Just write the updated values to the client. This will trigger an update.
            using (StreamWriter writetext = new StreamWriter(waveStylesFilePath))
            {
                writetext.WriteLine(wavesyaml);
            }
            yield return null;
        }

        private static IEnumerator OnClientReceiveLevelConfigs(long sender, ZPackage package)
        {
            var levelsyaml = package.ReadString();
            // Just write the updated values to the client. This will trigger an update.
            using (StreamWriter writetext = new StreamWriter(levelDefinitionsFilePath))
            {
                writetext.WriteLine(levelsyaml);
            }
            yield return null;
        }

        private static IEnumerator OnClientReceiveWildShrineConfigs(long sender, ZPackage package)
        {
            var levelsyaml = package.ReadString();
            // Just write the updated values to the client. This will trigger an update.
            using (StreamWriter writetext = new StreamWriter(wildShrineConfigurationFilePath))
            {
                writetext.WriteLine(levelsyaml);
            }
            yield return null;
        }

        private static void UpdateLevelsConfigFileOnChange(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(levelDefinitionsFilePath)) { return; }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"{e} Creature filewatcher called, updating creature values."); }
            string levelsDefinitions = File.ReadAllText(levelDefinitionsFilePath);
            ChallengeLevelDefinitionCollection levelsDefinitionConfigValues;
            try
            {
                levelsDefinitionConfigValues = CONST.yamldeserializer.Deserialize<ChallengeLevelDefinitionCollection>(levelsDefinitions);
            }
            catch
            {
                if (VFConfig.EnableDebugMode.Value)
                {
                    Jotunn.Logger.LogWarning("Creatures failed deserializing, skipping update.");
                }
                return;
            }
            UpdateLevelsDefinition(levelsDefinitionConfigValues);
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Updated levels in-memory values."); }
            if (GUIManager.IsHeadless())
            {
                try
                {
                    monsterSyncRPC.SendPackage(ZNet.instance.m_peers, SendLevelsConfigs());
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Sent levels configs to clients."); }
                }
                catch
                {
                    Jotunn.Logger.LogError("Error while server syncing creature configs");
                }
            }
            else
            {
                if (VFConfig.EnableDebugMode.Value)
                {
                    Jotunn.Logger.LogInfo("Instance is not a server, and will not send znet creature updates.");
                }
            }

        }

        private static void UpdateCreatureConfigFileOnChange(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(creatureFilePath)) { return; }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"{e} Creature filewatcher called, updating creature values."); }
            string spawnableCreatureConfigs = File.ReadAllText(creatureFilePath);
            SpawnableCreatureCollection creatureConfigValues;
            try
            {
                creatureConfigValues = CONST.yamldeserializer.Deserialize<SpawnableCreatureCollection>(spawnableCreatureConfigs);
            } catch {
                if (VFConfig.EnableDebugMode.Value)
                {
                    Jotunn.Logger.LogWarning("Creatures failed deserializing, skipping update."); 
                }
                return;
            }
            UpdateSpawnableCreatures(creatureConfigValues);
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Updated creature in-memory values."); }
            if (GUIManager.IsHeadless())
            {
                try
                {
                    monsterSyncRPC.SendPackage(ZNet.instance.m_peers, SendCreatureConfigs());
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Sent creature configs to clients."); }
                }
                catch
                {
                    Jotunn.Logger.LogError("Error while server syncing creature configs");
                }
            } else
            {
                if (VFConfig.EnableDebugMode.Value)
                {
                    Jotunn.Logger.LogInfo("Instance is not a server, and will not send znet creature updates.");
                }
            }

        }

        private static void UpdateWavesConfigFileOnChange(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(waveStylesFilePath)) { return; }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"{e} Wavestyles filewatcher called, updating Wavestyles values."); }
            string waveStyleseConfigs = File.ReadAllText(waveStylesFilePath);
            WaveFormatCollection waveStylesConfigValues;
            try
            {
                waveStylesConfigValues = CONST.yamldeserializer.Deserialize<WaveFormatCollection>(waveStyleseConfigs);
            }
            catch
            {
                if (VFConfig.EnableDebugMode.Value)
                {
                    Jotunn.Logger.LogWarning("Wavestyles failed deserializing, skipping update.");
                }
                return;
            }
            UpdateWaveDefinition(waveStylesConfigValues);
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Updated WaveDefinition in-memory values."); }
            if (GUIManager.IsHeadless())
            {
                try
                {
                    monsterSyncRPC.SendPackage(ZNet.instance.m_peers, SendWavesConfigs());
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Sent WaveDefinition configs to clients."); }
                }
                catch
                {
                    Jotunn.Logger.LogError("Error while server syncing Wave configs");
                }
            }
            else
            {
                if (VFConfig.EnableDebugMode.Value)
                {
                    Jotunn.Logger.LogInfo("Instance is not a server, and will not send znet creature updates.");
                }
            }

        }

        private static void UpdateRewardsConfigFileOnChange(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(rewardFilePath)) { return; }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Rewards filewatcher called, updating rewards values."); }
            string rewardConfigs = File.ReadAllText(rewardFilePath);
            RewardEntryCollection rewardsValues;
            try {
                rewardsValues = CONST.yamldeserializer.Deserialize<RewardEntryCollection>(rewardConfigs);
            } catch (Exception) {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogWarning("Rewards failed deserializing, skipping update."); }
                return; 
            }
            RewardsData.UpdateRewardsEntries(rewardsValues);
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Updated rewards in-memory values."); }
            if (GUIManager.IsHeadless())
            {
                try
                {
                    rewardSyncRPC.SendPackage(ZNet.instance.m_peers, SendRewardsConfigs());
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Sent rewards configs to clients."); }
                }
                catch (Exception)
                {
                    Jotunn.Logger.LogError("Error while server syncing rewards configs");
                }
            }
            else
            {
                if (VFConfig.EnableDebugMode.Value)
                {
                    Jotunn.Logger.LogInfo("Instance is not a server, and will not send znet reward updates.");
                }
            }

        }
        
        private static void UpdateWildShrineConfigFileOnChange(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(rewardFilePath)) { return; }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Rewards filewatcher called, updating Wildshrines values."); }
            string wildShrineConfigs = File.ReadAllText(wildShrineConfigurationFilePath);
            WildShrineConfigurationCollection wildshrineValues;
            try
            {
                wildshrineValues = CONST.yamldeserializer.Deserialize<WildShrineConfigurationCollection>(wildShrineConfigs);
            }
            catch (Exception)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogWarning("WildShrineConfigs failed deserializing, skipping update."); }
                return;
            }
            WildShrineData.UpdateWildShrineDefinition(wildshrineValues);
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Updated WildshrineConfigs in-memory values."); }
            if (GUIManager.IsHeadless())
            {
                try
                {
                    rewardSyncRPC.SendPackage(ZNet.instance.m_peers, SendWildShrineConfigs());
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Sent Wildshrine configs to clients."); }
                }
                catch (Exception)
                {
                    Jotunn.Logger.LogError("Error while server syncing Wildshrine configs");
                }
            }
            else
            {
                if (VFConfig.EnableDebugMode.Value)
                {
                    Jotunn.Logger.LogInfo("Instance is not a server, and will not send znet reward updates.");
                }
            }

        }

        private static void UpdateMainConfigFile(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(creatureFilePath)) { return; }
            try
            {
                cfg.SaveOnConfigSet = false;
                cfg.Reload();
                cfg.SaveOnConfigSet = true;
            }
            catch
            {
                Jotunn.Logger.LogError($"There was an issue reloading {ValheimFortress.PluginGUID}.cfg.");
            }
        }

        /// <summary>
        ///  Helper to bind configs for bool types
        /// </summary>
        /// <param name="config_file"></param>
        /// <param name="catagory"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="description"></param>
        /// <param name="advanced"></param>
        /// <returns></returns>
        public ConfigEntry<bool> BindServerConfig(string catagory, string key, bool value, string description, bool advanced = false)
        {
            return cfg.Bind(catagory, key, value,
                new ConfigDescription(description,
                null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, IsAdvanced = advanced })
                );
        }

        /// <summary>
        /// Helper to bind configs for strings
        /// </summary>
        /// <param name="config_file"></param>
        /// <param name="catagory"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="description"></param>
        /// <param name="advanced"></param>
        /// <returns></returns>
        public ConfigEntry<string> BindServerConfig(string catagory, string key, string value, string description, bool advanced = false)
        {
            return cfg.Bind(catagory, key, value,
                new ConfigDescription(description, null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, IsAdvanced = advanced })
                );
        }

        /// <summary>
        /// Helper to bind configs for Shorts
        /// </summary>
        /// <param name="config_file"></param>
        /// <param name="catagory"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="description"></param>
        /// <param name="advanced"></param>
        /// <returns></returns>
        public ConfigEntry<short> BindServerConfig(string catagory, string key, short value, string description, bool advanced = false, short valmin = 0, short valmax = 150)
        {
            return cfg.Bind(catagory, key, value,
                new ConfigDescription(description,
                new AcceptableValueRange<short>(valmin, valmax),
                new ConfigurationManagerAttributes { IsAdminOnly = true, IsAdvanced = advanced })
                );
        }

        /// <summary>
        /// Helper to bind configs for float types
        /// </summary>
        /// <param name="config_file"></param>
        /// <param name="catagory"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="description"></param>
        /// <param name="advanced"></param>
        /// <param name="valmin"></param>
        /// <param name="valmax"></param>
        /// <returns></returns>
        public ConfigEntry<float> BindServerConfig(string catagory, string key, float value, string description, bool advanced = false, float valmin = 0, float valmax = 150)
        {
            return cfg.Bind(catagory, key, value,
                new ConfigDescription(description,
                new AcceptableValueRange<float>(valmin, valmax),
                new ConfigurationManagerAttributes { IsAdminOnly = true, IsAdvanced = advanced })
                );
        }


        // Create Configuration and load it.
        private void CreateConfigValues(ConfigFile Config)
        {
            MaxSpawnRange = BindServerConfig("Shrine of Challenge", "MaxSpawnRange", 100, "The radius around the shrine that enemies can spawn in.", false, 10, 800);
            // Max level the shrine can be set to / min level it can be set to
            EnableHardModifier = BindServerConfig("Shrine of Challenge", "EnableHardModifier", true, "Whether or not the hard mode modifier is available (100% bigger wave size for 50% more rewards)", true);
            EnableBossModifier = BindServerConfig("Shrine of Challenge", "EnableBossModifier", true, "Whether or not boss mod is available as a level modifier (more rewards & spawns the biome specific boss)", true);
            EnableSiegeModifer = BindServerConfig("Shrine of Challenge", "EnableSiegeModifer", true, "Whether or not siege mode is available as a modifier. Siege mode gives much larger pauses between waves, and 100% larger waves for 50% more reward.", true);
            EnableMapPings = BindServerConfig("Shrine of Challenge", "EnableMapPings", false, "Whether or not waves spawning from the shrine of challenge should ping the map when they spawn.", true);
            EnableRewardsEstimate = BindServerConfig("Shrine of Challenge", "EnableRewardsEstimate", true, "Enables showing an estimate of how many rewards you will get for doing the selected level for the specified reward.", true);
            rewardsMultiplier = BindServerConfig("Shrine of Challenge", "rewardsMultiplier", 1.1f, "The base multiplier for rewards, higher values will make every wave more rewarding", true);
            rewardsDifficultyScalar = BindServerConfig("Shrine of Challenge", "rewardsDifficultyScalar", 0.02f, "Multiplier for rewards that scales with level, each level adds this to the value, making high level challenges much more rewarding.", true);
            MaxRewardsPerSecond = BindServerConfig("Shrine of Challenge", "MaxRewardsPerSecond", 120, "Sets how fast the shrine will spawn rewards. Reducing this will reduce the performance impact of spawning so many items at once.", true, 10, 400);
            NotifyCreatureThreshold = BindServerConfig("Shrine of Challenge", "NotifyCreatureThreshold", 10, "Sets the level at which interacting with the shrine will add notifier to remaining creatures.", true, 1, 50);
            TeleportCreatureThreshold = BindServerConfig("Shrine of Challenge", "TeleportCreatureThreshold", 3, "Sets the level at which interacting with the shrine teleport remaining creatures to the shrine.", true, 1, 50);
            ShrineAnnouncementRange = BindServerConfig("Shrine of Challenge", "ShrineAnnouncementRange", 150f, "Sets the range at which announcements will display for shrine of challenge related activities", true, 50f, 800f);
            DistanceBetweenShrines = BindServerConfig("Wild Shrines", "DistanceBetweenShrines", 1000f, "The mimum distance between shrines, setting this higher will result in fewer wild shrines, lower more.", true, 100f, 5000f);
            NumberOfEachWildShrine = BindServerConfig("Wild Shrines", "NumberOfEachWildShrine", 100, "Each wild shrine type will attempt to be placed this many times", true, 5, 200);
            // Client side configurations

            // Debugmode
            EnableDebugMode = Config.Bind("Client config", "EnableDebugMode", false,
                new ConfigDescription("Enables Debug logging for Valheim Fortress.",
                null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, IsAdvanced = true }));

            EnableTurretDebugMode = Config.Bind("Client config", "EnableTurretDebugMode", false,
                new ConfigDescription("Enables debug mode for turrets, this can be noisy.",
                null,
                new ConfigurationManagerAttributes { IsAdminOnly = false, IsAdvanced = true }));

        }
    }
}
