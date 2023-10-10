using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Logger = Jotunn.Logger;
using UnityEngine;
using Jotunn.Utils;
using System.Text.RegularExpressions;
using System.IO;
using ValheimFortress.Challenge;
using BepInEx.Configuration;

namespace ValheimFortress
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ValheimFortress : BaseUnityPlugin
    {
        public const string PluginGUID = "com.midnightsfx.ValheimFortress";
        public const string PluginName = "ValheimFortress";
        public const string PluginVersion = "0.0.1";

        internal static ValheimFortress Instance { get; private set; }

        AssetBundle EmbeddedResourceBundle;
        public VFConfig cfg;


        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            cfg = new VFConfig(Config);
            EmbeddedResourceBundle = AssetUtils.LoadAssetBundleFromResources("ValheimFortress.AssetsEmbedded.vfbundle", typeof(ValheimFortress).Assembly);
            // For debug logging, not working right now, again
            // Logger.LogInfo($"Embedded resources: {string.Join(",", typeof(ValheimFortress).Assembly.GetManifestResourceNames())}");

            ValheimFortressPieces vfpieces = new ValheimFortressPieces(EmbeddedResourceBundle, cfg);
            AddLocalizations();

            GUIManager.OnCustomGUIAvailable += () => UI.Init(EmbeddedResourceBundle);

            Logger.LogInfo("Valheim Fortress loaded.");
            // EmbeddedResourceBundle.Unload(false); // unload anything extra
        }


        // This loads all localizations within the localization directory.
        // Localizations should be plain JSON objects with each of the two required entries being seperate eg:
        // "item_sword": "sword-name-here",
        // "item_sword_description": "sword-description-here",
        // the localization file itself should be a casematched language as defined by one of the "folder" language names from here:
        // https://valheim-modding.github.io/Jotunn/data/localization/language-list.html
        private void AddLocalizations()
        {
            Localization = LocalizationManager.Instance.GetLocalization();

            // ValheimFortress.localizations.English.json
            // load all localization files within the localizations directory
            foreach (string embeddedResouce in typeof(ValheimFortress).Assembly.GetManifestResourceNames())
            {
                if (!embeddedResouce.Contains("localizations")) { continue; }
                // Read the localization file
                string localization = ReadEmbeddedResourceFile(embeddedResouce);
                // since I use comments in the localization that are not valid JSON those need to be stripped
                string cleaned_localization = Regex.Replace(localization, @"\/\/.*", "");
                // Just the localization name
                var localization_name = embeddedResouce.Split('.');
                Logger.LogInfo($"Adding localization: {localization_name[2]}");
                // Logging some characters seem to cause issues sometimes
                // if (cfg.EnableDebugMode.Value == true) { Logger.LogInfo($"Localization Text: {cleaned_localization}"); }
                //Localization.AddTranslation(localization_name[2], localization);
                Localization.AddJsonFile(localization_name[2], cleaned_localization);
            }
        }


        // This reads an embedded file resouce name, these are all resouces packed into the DLL
        // they all have a format following this:
        // ValheimArmory.localizations.English.json
        private string ReadEmbeddedResourceFile(string filename)
        {
            using (var stream = typeof(ValheimFortress).Assembly.GetManifestResourceStream(filename))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

    }
}