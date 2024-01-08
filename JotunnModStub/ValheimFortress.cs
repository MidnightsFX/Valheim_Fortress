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
using System.Collections.Generic;
using System;
using System.Linq;

namespace ValheimFortress
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ValheimFortress : BaseUnityPlugin
    {
        public const string PluginGUID = "MidnightsFX.ValheimFortress";
        public const string PluginName = "ValheimFortress";
        public const string PluginVersion = "0.9.16";

        AssetBundle EmbeddedResourceBundle;
        public VFConfig cfg;
        public static GameObject spawnPortal;
        public static GameObject creatureNotifier;
        public static GameObject portalDestroyVFX;

        private void Awake()
        {
            cfg = new VFConfig(Config);
            cfg.SetupConfigRPCs();
            EmbeddedResourceBundle = AssetUtils.LoadAssetBundleFromResources("ValheimFortress.AssetsEmbedded.vfbundle", typeof(ValheimFortress).Assembly);
            // For debug logging, not working right now, again
            // Logger.LogInfo($"Embedded resources: {string.Join(",", typeof(ValheimFortress).Assembly.GetManifestResourceNames())}");
            AddLocalizations();
            ValheimFortressPieces vfpieces = new ValheimFortressPieces(EmbeddedResourceBundle, cfg);
            SetupVFXObjects(EmbeddedResourceBundle);

            // Yaml configs
            VFConfig.GetYamlConfigFiles();

            // Generate/update/set config values.
            // Levels.UpdateCreatureConfigValues(cfg);
            Levels.UpdateLevelValues(cfg);
            // Rewards.UpdateResouceRewards(cfg);

            // GUIManager.OnCustomGUIAvailable += () => UI.Init(EmbeddedResourceBundle);

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
            // Use this class to add your own localization to the game
            // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
            CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();
            // ValheimFortress.localizations.English.json
            // load all localization files within the localizations directory
            Logger.LogInfo("Loading Localizations.");
            foreach (string embeddedResouce in typeof(ValheimFortress).Assembly.GetManifestResourceNames())
            {
                if (!embeddedResouce.Contains("Localizations")) { continue; }
                // Read the localization file
                string localization = ReadEmbeddedResourceFile(embeddedResouce);
                // since I use comments in the localization that are not valid JSON those need to be stripped
                string cleaned_localization = Regex.Replace(localization, @"\/\/.*", "");
                // Just the localization name
                var localization_name = embeddedResouce.Split('.');
                Logger.LogInfo($"Adding localization: {localization_name[2]}");
                // Logging some characters seem to cause issues sometimes
                // if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo($"Localization Text: {cleaned_localization}"); }
                //Localization.AddTranslation(localization_name[2], localization);
                Localization.AddJsonFile(localization_name[2], cleaned_localization);
            }
        }

        internal static GameObject getPortal() { return spawnPortal; }

        internal static GameObject getNotifier() { return creatureNotifier; }

        internal static GameObject getPortalDestroyVFX() { return portalDestroyVFX; }

        private static void SetupVFXObjects(AssetBundle EmbeddedResourceBundle)
        {
            // Load and register
            GameObject portal = EmbeddedResourceBundle.LoadAsset<GameObject>($"Assets/Custom/Pieces/VFortress/VF_portal.prefab");
            CustomPrefab portalPrefab = new CustomPrefab(portal, false);
            PrefabManager.Instance.AddPrefab(portalPrefab);
            spawnPortal = portalPrefab.Prefab;
            GameObject portal_destroy_vfx = EmbeddedResourceBundle.LoadAsset<GameObject>($"Assets/Custom/Pieces/VFortress/VF_portal_destroy.prefab");
            CustomPrefab portal_destroy_vfx_prefab = new CustomPrefab(portal_destroy_vfx, false);
            PrefabManager.Instance.AddPrefab(portal_destroy_vfx_prefab);
            portalDestroyVFX = portal_destroy_vfx_prefab.Prefab;
            GameObject notify_ga =  EmbeddedResourceBundle.LoadAsset<GameObject>($"Assets/Custom/Pieces/VFortress/VF_creature_notify.prefab");
            CustomPrefab notify_custom_prefab = new CustomPrefab(notify_ga, false);
            PrefabManager.Instance.AddPrefab(notify_custom_prefab);
            creatureNotifier = notify_custom_prefab.Prefab;
        }

        public static string LocalizeOrDefault(string str_to_localize,string default_string)
        {
            string localized = Localization.instance.Localize(str_to_localize);
            if(localized == $"[{str_to_localize.Replace("$", "")}]")
            {
                Jotunn.Logger.LogInfo($"{str_to_localize} was not localized, returning the default: {default_string}");
                return default_string;
            } else
            {
                return localized;
            }
        }


        /// <summary>
        /// This reads an embedded file resouce name, these are all resouces packed into the DLL
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        internal static string ReadEmbeddedResourceFile(string filename)
        {
            using (var stream = typeof(ValheimFortress).Assembly.GetManifestResourceStream(filename))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Fisher-Yates style list sort for string lists.
        /// </summary>
        /// <param name="inputList"></param>
        /// <returns></returns>
        public static List<String> shuffleList(List<String> inputList)
        {    //take any list of GameObjects and return it with Fischer-Yates shuffle
            int i = 0;
            int t = inputList.Count;
            int r = 0;
            String p = null;
            List<String> tempList = new List<String>();
            tempList.AddRange(inputList);

            while (i < t)
            {
                r = UnityEngine.Random.Range(i, tempList.Count);
                p = tempList[i];
                tempList[i] = tempList[r];
                tempList[r] = p;
                i++;
            }

            return tempList;
        }


        /// <summary>
        /// Json pretty printer https://stackoverflow.com/questions/4580397/json-formatter-in-c
        /// </summary>
        /// <param name="json"></param>
        /// <param name="indent"></param>
        /// <returns></returns>
        public static string FormatJson(string json, string indent = "  ")
        {
            var indentation = 0;
            var quoteCount = 0;
            var escapeCount = 0;

            var result =
                from ch in json ?? string.Empty
                let escaped = (ch == '\\' ? escapeCount++ : escapeCount > 0 ? escapeCount-- : escapeCount) > 0
                let quotes = ch == '"' && !escaped ? quoteCount++ : quoteCount
                let unquoted = quotes % 2 == 0
                let colon = ch == ':' && unquoted ? ": " : null
                let nospace = char.IsWhiteSpace(ch) && unquoted ? string.Empty : null
                let lineBreak = ch == ',' && unquoted ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indent, indentation)) : null
                let openChar = (ch == '{' || ch == '[') && unquoted ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indent, ++indentation)) : ch.ToString()
                let closeChar = (ch == '}' || ch == ']') && unquoted ? Environment.NewLine + string.Concat(Enumerable.Repeat(indent, --indentation)) + ch : ch.ToString()
                select colon ?? nospace ?? lineBreak ?? (
                    openChar.Length > 1 ? openChar : closeChar
                );

            return string.Concat(result);
        }


    }
}