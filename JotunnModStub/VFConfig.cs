using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValheimFortress
{
    class VFConfig
    {
        public static ConfigEntry<bool> EnableDebugMode;
        public static ConfigEntry<bool> EnableHordeDrops;
        public static ConfigEntry<bool> EnableBossDrops;
        public static ConfigEntry<bool> EnableGladiatorMode;
        public static ConfigEntry<int> MaxChallengeLevel;
        public static ConfigEntry<int> MaxSpawnRange;
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

            MaxChallengeLevel = Config.Bind("Shrine of Challenge", "MaxLevel", 100,
                new ConfigDescription("The Maximum level the shrine can be set to, you must still beat bosses to increase your allowed level.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true }));

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
        public ConfigEntry<short> BindServerConfig(string catagory, string key, short value, string description, bool advanced = false)
        {
            return file.Bind(catagory, key, value,
                new ConfigDescription(description, null,
                new ConfigurationManagerAttributes { IsAdminOnly = true, IsAdvanced = advanced })
                );
        }
    }
}
