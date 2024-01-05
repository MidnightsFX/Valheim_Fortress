using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValheimFortress.Challenge;
using Mono.Cecil.Cil;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static ValheimFortress.Challenge.Levels;
using System.Runtime.Remoting.Messaging;
using Jotunn.Managers;
using Jotunn.Entities;
using System.Collections;
using Jotunn.Configs;

namespace ValheimFortress
{
    class VFConfig
    {
        public static ConfigFile cfg;
        public static ConfigEntry<bool> EnableDebugMode;
        public static ConfigEntry<bool> EnableTurretDebugMode;
        public static ConfigEntry<bool> EnableGladiatorMode;
        public static ConfigEntry<short> MaxChallengeLevel;
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

        private static CustomRPC monsterSyncRPC;
        private static CustomRPC rewardSyncRPC;

        private static String rewardFilePath = Path.Combine(Paths.ConfigPath, "VFortress", "Rewards.yaml");
        private static String creatureFilePath = Path.Combine(Paths.ConfigPath, "VFortress", "SpawnableCreatures.yaml");

        

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
            SynchronizationManager.Instance.AddInitialSynchronization(monsterSyncRPC, SendCreatureConfigs);
            SynchronizationManager.Instance.AddInitialSynchronization(rewardSyncRPC, SendRewardsConfigs);
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
# The below configuration values are loaded at the start of the game, and they are not actively watched for changes beyond that. You must restart your game for any changes to take effect.
#
# Rewards configurations have a number of key values
#  Coin:                                 |- The name of the reward, this will be the diplayed name if there is no localization for this reward, which is likely the case for any custom entries.
#    enabled: true                       |- Whether or not the reward is enabled, you can use this to disable any vanilla rewards you do not want. At least 1 reward must be available at ALL times.
#    resource_cost: 5                    |- This is the cost to gain 1 of the particular reward. Points are generated based on how many monsters are spawned.
#    resource_prefab: ""Coins""            |- This is the unity prefab name for a resource, you will often see mods list the prefabs they have added. Prefabs are also listed on the valheim wiki.
#    required_boss: ""None""               |- This must be one of the following values: ""None"" ""Eikythr"" ""TheElder"" ""BoneMass"" ""Moder"" ""Yagluth"" ""TheQueen""";
                    writetext.WriteLine(header);
                    writetext.WriteLine(Rewards.YamlRewardsDefinition());
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
# The below configuration values are loaded at the start of the game, and they are not actively watched for changes beyond that. You must restart your game for any changes to take effect.
#
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
            // now that we have ensured the files exist lets read them
            string spawnableCreatureConfigs = File.ReadAllText(creatureFilePath);
            string rewardConfigs = File.ReadAllText(rewardFilePath);
            try
            {
                var creatureValues = CONST.yamldeserializer.Deserialize<SpawnableCreatureCollection>(spawnableCreatureConfigs);
                UpdateSpawnableCreatures(creatureValues);
            } catch ( Exception ) { Jotunn.Logger.LogWarning("There was an error updating the creature values, defaults will be used."); }
            
            try
            {
                var rewardsValues = CONST.yamldeserializer.Deserialize<RewardEntryCollection>(rewardConfigs);
                Rewards.UpdateRewardsEntries(rewardsValues);
            } catch ( Exception ) { Jotunn.Logger.LogWarning("There was an error updating the rewards values, defaults will be used."); }

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

        }

        private static IEnumerator OnServerRecieveConfigs(long sender, ZPackage package)
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

        private static ZPackage SendRewardsConfigs()
        {
            string rewardsConfigs = File.ReadAllText(rewardFilePath);
            ZPackage package = new ZPackage();
            package.Write(rewardsConfigs);
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
            Rewards.UpdateRewardsEntries(rewardsValues);
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
            MaxSpawnRange = BindServerConfig("Shrine of Challenge", "MaxSpawnRange", 100, "The radius around the shrine that enemies can spawn in. When the shrine is not in gladiator mode.", false, 10, 800);
            // Max level the shrine can be set to / min level it can be set to
            MaxChallengeLevel = BindServerConfig("Shrine of Challenge", "MaxLevel", (short)30, "The Maximum level the shrine can be set to, you must still beat bosses to increase your allowed level. 5 Levels per biome. Setting to 10 will cap challenges out at meadows(1-5) + blackforest(6-10).", false, 1, 30);
            EnableGladiatorMode = BindServerConfig("Shrine of Challenge", "EnableGladiatorMode", false, "Whether the shrine of challenge should default to spawning mobs on itself (gladiator arena), or remotely (fortress siege).", false);
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
