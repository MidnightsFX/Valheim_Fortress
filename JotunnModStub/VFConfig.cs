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

namespace ValheimFortress
{
    class VFConfig
    {
        public static ConfigEntry<bool> EnableDebugMode;
        public static ConfigEntry<bool> EnableHordeDrops;
        public static ConfigEntry<bool> EnableBossDrops;
        public static ConfigEntry<bool> EnableGladiatorMode;
        public static ConfigEntry<short> MaxChallengeLevel;
        public static ConfigEntry<int> MaxSpawnRange;
        public static ConfigEntry<bool> EnableBossModifier;
        public static ConfigEntry<bool> EnableHardModifier;
        public static ConfigEntry<bool> EnableSiegeModifer;
        public static ConfigEntry<bool> EnableMapPings;
        public ConfigFile file;

        public VFConfig(ConfigFile Config)
        {
            // Init with the default plugin config file
            CreateConfigValues(Config);
            file = Config;
        }

        // Create Configuration and load it.
        private void CreateConfigValues(ConfigFile Config)
        {
            Config.SaveOnConfigSet = true;

            // Max Spawn Radius around the shrine
            MaxSpawnRange = Config.Bind("Shrine of Challenge", "MaxSpawnRange", 400,
                new ConfigDescription("The radius around the shrine that enemies can spawn in. When the shrine is not in gladiator mode.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false }));

            MaxChallengeLevel = Config.Bind("Shrine of Challenge", "MaxLevel", (short)30,
                new ConfigDescription("The Maximum level the shrine can be set to, you must still beat bosses to increase your allowed level. 5 Levels per biome. Setting to 10 will cap challenges out at meadows(1-5) + blackforest(6-10).",
                new AcceptableValueRange<short>(1, 30),
                new ConfigurationManagerAttributes { IsAdvanced = false }));

            EnableGladiatorMode = Config.Bind("Shrine of Challenge", "EnableGladiatorMode", false,
                new ConfigDescription("Whether the shrine of challenge should default to spawning mobs on itself (gladiator arena), or remotely (fortress siege).",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true }));

            EnableHordeDrops = Config.Bind("Shrine of Challenge", "EnableHordeDrops", false,
                new ConfigDescription("Whether or not creatures spawned from the shrine should drop their usual loot (this can be overwhelming overpowered).",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true }));

            EnableBossDrops = Config.Bind("Shrine of Challenge", "EnableBossDrops", false,
                new ConfigDescription("Whether or not bosses spawned from the shrine should drop their usual loot.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true }));

            // Debugmode
            EnableDebugMode = Config.Bind("Client config", "EnableDebugMode", false,
                new ConfigDescription("Enables Debug logging for Valheim Fortress.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true }));


            EnableHardModifier = Config.Bind("Shrine of Challenge", "EnableHardModifier", true,
                new ConfigDescription("Whether or not the hard mode modifier is available (100% bigger wave size for 50% more rewards)",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true }));

            EnableBossModifier = Config.Bind("Shrine of Challenge", "EnableBossModifier", true,
                new ConfigDescription("Whether or not boss mod is available as a level modifier (more rewards & spawns the biome specific boss)",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true }));

            EnableSiegeModifer = Config.Bind("Shrine of Challenge", "EnableSiegeModifer", true,
                new ConfigDescription("Whether or not siege mode is available as a modifier. Siege mode gives much larger pauses between waves, and 100% larger waves for 50% more reward.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true }));

            EnableMapPings = Config.Bind("Shrine of Challenge", "EnableMapPings", false,
                new ConfigDescription("Whether or not waves spawning from the shrine of challenge should ping the map when they spawn.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true }));

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
            
            string rewardFilePath = $"{externalConfigFolder}\\Rewards.yaml";
            string spawnableCreaturesPath = $"{externalConfigFolder}\\SpawnableCreatures.yaml";

            string[] presentFiles = Directory.GetFiles(externalConfigFolder);

            foreach (string configFile in presentFiles)
            {
                Jotunn.Logger.LogInfo($"Config file found: {configFile}");
                if (configFile.Contains("Rewards.yaml"))
                {
                    Jotunn.Logger.LogInfo($"Found rewards configuration: {configFile}");
                    rewardFilePath = configFile;
                    hasRewardsConfig = true;
                }
                if (configFile.Contains("SpawnableCreatures.yaml"))
                {
                    Jotunn.Logger.LogInfo($"Found Creature configuration: {configFile}");
                    spawnableCreaturesPath = configFile;
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
#    resouce_cost: 5                     |- This is the cost to gain 1 of the particular reward. Points are generated based on how many monsters are spawned.
#    resource_prefab: ""Coins""            |- This is the unity prefab name for a resource, you will often see mods list the prefabs they have added. Prefabs are also listed on the valheim wiki.
#    required_boss: ""None""               |- This must be one of the following values: ""None"" ""Eikythr"" ""TheElder"" ""BoneMass"" ""Moder"" ""Yagluth"" ""TheQueen""";
                    writetext.WriteLine(header);
                    writetext.WriteLine(Rewards.YamlRewardsDefinition());
                }
            }
            if (hasCreatureConfig == false)
            {
                Jotunn.Logger.LogInfo("CreatureConfig file missing, recreating.");
                using (StreamWriter writetext = new StreamWriter(spawnableCreaturesPath))
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
#  biome: ""Meadows""        |- This must be one of the following values: ""Meadows"", ""BlackForest"", ""Swamp"", ""Mountain"", ""Plains"", ""Mistlands"". The biome determines the levels that will recieve this spawn, and how the spawn might be adjusted to
#                             fit higher difficulty waves. eg: a greydwarf spawning into a swamp level wave will recieve 1 bonus star, since it is from the black forest, which is 1 biome behind the swamp.";
                    writetext.WriteLine(header);
                    writetext.WriteLine(YamlCreatureDefinition());
                }
            }
            // now that we have ensured the files exist lets read them
            string spawnableCreatureConfigs = File.ReadAllText(spawnableCreaturesPath);
            string rewardConfigs = File.ReadAllText(rewardFilePath);

            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            var creatureValues = deserializer.Deserialize<SpawnableCreatureCollection>(spawnableCreatureConfigs);
            UpdateSpawnableCreatures(creatureValues);
            //Jotunn.Logger.LogInfo($"deserialized creatures: {creatureValues}");
            var rewardsValues = deserializer.Deserialize<Rewards.RewardEntryCollection>(rewardConfigs);
            Rewards.UpdateRewardsEntries(rewardsValues);
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
            return file.Bind(catagory, key, value,
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
            return file.Bind(catagory, key, value,
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
            return file.Bind(catagory, key, value,
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
            return file.Bind(catagory, key, value,
                new ConfigDescription(description,
                new AcceptableValueRange<float>(valmin, valmax),
                new ConfigurationManagerAttributes { IsAdminOnly = true, IsAdvanced = advanced })
                );
        }
    }
}
