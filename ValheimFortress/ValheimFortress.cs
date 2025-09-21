using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace ValheimFortress
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ValheimFortress : BaseUnityPlugin
    {
        public const string PluginGUID = "MidnightsFX.ValheimFortress";
        public const string PluginName = "ValheimFortress";
        public const string PluginVersion = "0.32.8";

        internal static Harmony Harmony = new Harmony(PluginGUID);
        public static AssetBundle EmbeddedResourceBundle;
        public VFConfig cfg;
        internal static CustomLocalization LocalizationInstance;
        public static ManualLogSource Log;
        public static GameObject spawnPortal;
        public static GameObject creatureNotifier;
        public static GameObject portalDestroyVFX;

        public void Awake()
        {
            cfg = new VFConfig(Config);
            Log = this.Logger;
            cfg.SetupConfigRPCs();
            EmbeddedResourceBundle = AssetUtils.LoadAssetBundleFromResources("ValheimFortress.AssetsEmbedded.vfbundle", typeof(ValheimFortress).Assembly);
            AddLocalizations();
            new ValheimFortressPieces();
            SetupVFXObjects(EmbeddedResourceBundle);
            new VFLocations(EmbeddedResourceBundle, cfg);

            // Yaml configs
            VFConfig.GetYamlConfigFiles();

            // GUIManager.OnCustomGUIAvailable += () => UI.Init(EmbeddedResourceBundle);

            Logger.LogInfo("Valheim Fortress loaded.");
            // EmbeddedResourceBundle.Unload(false); // unload anything extra
            Assembly assembly = Assembly.GetExecutingAssembly();
            Harmony.PatchAll(assembly);
        }


        // This loads all localizations within the localization directory.
        // Localizations should be plain JSON objects with each of the two required entries being seperate eg:
        // "item_sword": "sword-name-here",
        // "item_sword_description": "sword-description-here",
        // the localization file itself should be a casematched language as defined by one of the "folder" language names from here:
        // https://valheim-modding.github.io/Jotunn/data/localization/language-list.html
        private void AddLocalizations()
        {
            LocalizationInstance = LocalizationManager.Instance.GetLocalization();
            //LocalizationManager.Instance.AddLocalization(Localization);

            // Ensure localization folder exists
            var translationFolder = Path.Combine(BepInEx.Paths.ConfigPath, "VFortress", "Localizations");
            Directory.CreateDirectory(translationFolder);
            //SimpleJson.SimpleJson.CurrentJsonSerializerStrategy


            // ValheimArmory.localizations.English.json,ValheimArmory.localizations.German.json,ValheimArmory.localizations.Russian.json
            // load all localization files within the localizations directory
            foreach (string embeddedResouce in typeof(ValheimFortress).Assembly.GetManifestResourceNames())
            {
                if (!embeddedResouce.Contains("Localizations")) { continue; }
                // Read the localization file

                string localization = ReadEmbeddedResourceFile(embeddedResouce);
                // since I use comments in the localization that are not valid JSON those need to be stripped
                string cleaned_localization = Regex.Replace(localization, @"\/\/.*", "");
                Dictionary<string, string> internal_localization = SimpleJson.SimpleJson.DeserializeObject<Dictionary<string, string>>(cleaned_localization);
                // Just the localization name
                var localization_name = embeddedResouce.Split('.');
                if (File.Exists($"{translationFolder}/{localization_name[2]}.json"))
                {
                    string cached_translation_file = File.ReadAllText($"{translationFolder}/{localization_name[2]}.json");
                    try
                    {
                        Dictionary<string, string> cached_localization = SimpleJson.SimpleJson.DeserializeObject<Dictionary<string, string>>(cached_translation_file);
                        UpdateLocalizationWithMissingKeys(internal_localization, cached_localization);
                        Logger.LogDebug($"Reading {translationFolder}/{localization_name[2]}.json");
                        File.WriteAllText($"{translationFolder}/{localization_name[2]}.json", SimpleJson.SimpleJson.SerializeObject(cached_localization));
                        string updated_local_translation = File.ReadAllText($"{translationFolder}/{localization_name[2]}.json");
                        LocalizationInstance.AddJsonFile(localization_name[2], updated_local_translation);
                    }
                    catch
                    {
                        File.WriteAllText($"{translationFolder}/{localization_name[2]}.json", cleaned_localization);
                        Logger.LogDebug($"Reading {embeddedResouce}");
                        LocalizationInstance.AddJsonFile(localization_name[2], cleaned_localization);
                    }
                }
                else
                {
                    File.WriteAllText($"{translationFolder}/{localization_name[2]}.json", cleaned_localization);
                    Logger.LogDebug($"Reading {embeddedResouce}");
                    LocalizationInstance.AddJsonFile(localization_name[2], cleaned_localization);
                }

                Logger.LogDebug($"Added localization: '{localization_name[2]}'");
                // Logging some characters seem to cause issues sometimes
                // if (VAConfig.EnableDebugMode.Value == true) { Logger.LogInfo($"Localization Text: {cleaned_localization}"); }
                //Localization.AddTranslation(localization_name[2], localization);
                // Localization.AddJsonFile(localization_name[2], cleaned_localization);
            }
        }

        private void UpdateLocalizationWithMissingKeys(Dictionary<string, string> internal_localization, Dictionary<string, string> cached_localization)
        {
            if (internal_localization.Keys.Count != cached_localization.Keys.Count)
            {
                Logger.LogDebug("Cached localization was missing some entries. They will be added.");
                foreach (KeyValuePair<string, string> entry in internal_localization)
                {
                    if (!cached_localization.ContainsKey(entry.Key))
                    {
                        cached_localization.Add(entry.Key, entry.Value);
                    }
                }
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
            string localized = LocalizationInstance.TryTranslate(str_to_localize);
            if(localized == $"[{str_to_localize.Replace("$", "")}]") {
                Jotunn.Logger.LogDebug($"{str_to_localize} was not localized, returning the default: {default_string}");
                return default_string;
            } else {
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

        // Regex instanced whitespace trim
        private static readonly Regex sWhitespace = new Regex(@"\s+");
        public static string ReplaceWhitespace(string input, string replacement)
        {
            return sWhitespace.Replace(input, replacement);
        }


    }
}