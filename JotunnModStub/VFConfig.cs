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
        public ConfigEntry<bool> EnableDebugMode;
        public ConfigEntry<int> MaxSpawnRange;
        public ConfigEntry<int> MaxChallengeLevel;
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
            MaxSpawnRange = Config.Bind("Shrine of Challenge", "MaxSpawnRange", 100,
                new ConfigDescription("Maximum spawn radius around the shrine of challenge.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true }));

            // Max Spawn Radius around the shrine
            MaxChallengeLevel = Config.Bind("Shrine of Challenge", "MaxLevel", 100,
                new ConfigDescription("The Maximum level the shrine can be set to, you must still beat bosses to increase your allowed level.",
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
        /// Helper to bind configs for Ints
        /// </summary>
        /// <param name="config_file"></param>
        /// <param name="catagory"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="description"></param>
        /// <param name="advanced"></param>
        /// <returns></returns>
        public ConfigEntry<int> BindServerConfig(string catagory, string key, int value, string description, bool advanced = false)
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
