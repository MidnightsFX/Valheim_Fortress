using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Logger = Jotunn.Logger;
using UnityEngine;
using Jotunn.Utils;
using BepInEx.Configuration;
using Jotunn.Configs;
using ValheimFortress.Challenge;

namespace ValheimFortress
{
    class ValheimFortressPieces
    {
        public ValheimFortressPieces(AssetBundle EmbeddedResourceBundle, VFConfig config)
        {
            if (VFConfig.EnableDebugMode.Value == true)
            {
                Logger.LogInfo("Loading Items.");
            }

            LoadRugs(EmbeddedResourceBundle, config);
            LoadGlassWalls(EmbeddedResourceBundle, config);
            LoadAlterOfChallenge(EmbeddedResourceBundle, config);
        }

        private void LoadRugs(AssetBundle EmbeddedResourceBundle, VFConfig cfg)
        {
            // Green Jute Carpet
            new ValheimPiece(
                EmbeddedResourceBundle,
                cfg,
                new Dictionary<string, string>() {
                    { "name", "Green Jute Carpet" },
                    { "catagory", "Furniture" },
                    { "prefab", "VFgreen_jute_carpet_circle" },
                    { "sprite", "arrow_greenmetal" },
                    { "requiredBench", "piece_workbench" }
                },
                new Dictionary<string, bool>() { },
                new Dictionary<string, int>()
                {
                    { "Guck", 1 },
                    { "JuteRed", 4 },
                }
            );
            // Red Jute Carpet
            new ValheimPiece(
                EmbeddedResourceBundle,
                cfg,
                new Dictionary<string, string>() {
                    { "name", "Red Jute Carpet" },
                    { "catagory", "Furniture" },
                    { "prefab", "VFred_jute_carpet_circle" },
                    { "sprite", "arrow_greenmetal" },
                    { "requiredBench", "piece_workbench" }
                },
                new Dictionary<string, bool>() { },
                new Dictionary<string, int>()
                {
                    { "JuteRed", 4 }
                }
            );
            // Yellow Jute Carpet
            new ValheimPiece(
                EmbeddedResourceBundle,
                cfg,
                new Dictionary<string, string>() {
                    { "name", "Yellow Jute Carpet" },
                    { "catagory", "Furniture" },
                    { "prefab", "VFyellow_jute_carpet_circle" },
                    { "sprite", "arrow_greenmetal" },
                    { "requiredBench", "piece_workbench" }
                },
                new Dictionary<string, bool>() { },
                new Dictionary<string, int>()
                {
                    { "Dandelion", 4 },
                    { "JuteRed", 4 },
                }
            );
        }

        private void LoadGlassWalls(AssetBundle EmbeddedResourceBundle, VFConfig cfg)
        {
            // Blue Crystal Wall
            new ValheimPiece(
                EmbeddedResourceBundle,
                cfg,
                new Dictionary<string, string>() {
                    { "name", "Blue Crystal Wall" },
                    { "catagory", "Building" },
                    { "prefab", "VFblue_crystal_wall" },
                    { "sprite", "blue_crystal_wall" },
                    { "requiredBench", "piece_workbench" }
                },
                new Dictionary<string, bool>() { },
                new Dictionary<string, int>()
                {
                    { "Blueberries", 4 },
                    { "Crystal", 2 },
                }
            );
            // Red Crystal Wall
            new ValheimPiece(
                EmbeddedResourceBundle,
                cfg,
                new Dictionary<string, string>() {
                    { "name", "Red Crystal Wall" },
                    { "catagory", "Building" },
                    { "prefab", "VFred_crystal_wall" },
                    { "sprite", "red_crystal_wall" },
                    { "requiredBench", "piece_workbench" }
                },
                new Dictionary<string, bool>() { },
                new Dictionary<string, int>()
                {
                    { "Raspberry", 2 },
                    { "Crystal", 2 },
                }
            );
            // Yellow Crystal Wall
            new ValheimPiece(
                EmbeddedResourceBundle,
                cfg,
                new Dictionary<string, string>() {
                    { "name", "Yellow Crystal Wall" },
                    { "catagory", "Building" },
                    { "prefab", "VFyellow_crystal_wall" },
                    { "sprite", "yellow_crystal_wall" },
                    { "requiredBench", "piece_workbench" }
                },
                new Dictionary<string, bool>() { },
                new Dictionary<string, int>()
                {
                    { "Dandelion", 2 },
                    { "Crystal", 2 },
                }
            );
            // Green Crystal Wall
            new ValheimPiece(
                EmbeddedResourceBundle,
                cfg,
                new Dictionary<string, string>() {
                    { "name", "Green Crystal Wall" },
                    { "catagory", "Building" },
                    { "prefab", "VFgreen_crystal_wall" },
                    { "sprite", "green_crystal_wall" },
                    { "requiredBench", "piece_workbench" }
                },
                new Dictionary<string, bool>() { },
                new Dictionary<string, int>()
                {
                    { "Guck", 2 },
                    { "Crystal", 2 },
                }
            );
        }

        private void LoadAlterOfChallenge(AssetBundle EmbeddedResourceBundle, VFConfig cfg)
        {
            // Alter of Challenge
            new ValheimPiece(
                EmbeddedResourceBundle,
                cfg,
                new Dictionary<string, string>() {
                    { "name", "Alter of Challenge" },
                    { "catagory", "Misc" },
                    { "prefab", "VFshrine_of_challenge" },
                    { "sprite", "shrine_of_challenge" },
                    { "requiredBench", "piece_workbench" }
                },
                new Dictionary<string, bool>() { },
                new Dictionary<string, int>()
                {
                    { "Stone", 23 },
                    { "Ruby", 4 },
                    { "Coins", 100 },
                },
                true
            );
        }

        class ValheimPiece
        {
            String[] allowed_catagories = { "Furniture", "Building", "Crafting", "Misc" };
            String[] crafting_stations = { "forge", "piece_workbench", "blackforge", "piece_artisanstation" };
            /// <summary>
            /// 
            /// </summary>
            /// <param name="EmbeddedResourceBundle"> The embedded assets</param>
            /// <param name="cfg"> config file to add things to</param>
            /// <param name="metadata">Key(string)-Value(string) dictionary of item metadata eg: "name" = "Green Metal Arrow"</param>
            /// <param name="itemdata">Key(string)-Value(Tuple) dictionary of item metadata with config metadata eg: "blunt" = < 15(value), 0(min), 200(max), true(cfg_enable_flag) > </param>
            /// <param name="itemtoggles">Key(string)-Value(bool) dictionary of true/false config toggles for this item.</param>
            /// <param name="recipedata">Key(string)-Value(Tuple) dictionary of recipe requirements (limit 4) eg: "SerpentScale" = < 3(creation requirement), 2(levelup requirement)> </param>
            public ValheimPiece(
                AssetBundle EmbeddedResourceBundle,
                VFConfig cfg,
                Dictionary<String, String> metadata,
                Dictionary<String, bool> piecetoggle,
                Dictionary<String, int> recipedata,
                bool prefabscript = false
                )
            {
                // Validate inputs are valid
                if (!allowed_catagories.Contains(metadata["catagory"])) { throw new ArgumentException($"Catagory {metadata["catagory"]} must be an allowed catagory: {allowed_catagories}"); }
                if (!metadata.ContainsKey("name")) { throw new ArgumentException($"Item must have a name"); }
                if (!metadata.ContainsKey("prefab")) { throw new ArgumentException($"Item must have a prefab"); }
                if (!metadata.ContainsKey("sprite")) { throw new ArgumentException($"Item must have a sprite"); }
                if (!metadata.ContainsKey("requiredBench")) { throw new ArgumentException($"Item must have a requiredBench"); }
                if (!crafting_stations.Contains(metadata["requiredBench"])) { throw new ArgumentException($"Catagory {metadata["requiredBench"]} must be a valid crafting station: {crafting_stations}"); }
                if (recipedata.Count > 4) { throw new ArgumentException($"Recipe data can't have more than 4 requirements"); }
                if (!piecetoggle.ContainsKey("enabled")) { piecetoggle.Add("enabled", true); }
                // needed metadata - item name without spaces
                metadata["short_item_name"] = string.Join("", metadata["name"].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));

                // create config
                if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo($"Creating Configuration Values for {metadata["name"]}"); }
                CreateAndLoadConfigValues(cfg, metadata, piecetoggle, recipedata);

                // If the item is not enabled we do not load it
                if (piecetoggle["enabled"] != false)
                {
                    // load assets
                    if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo($"Loading bundled assets for {metadata["name"]}"); }
                    GameObject prefab = EmbeddedResourceBundle.LoadAsset<GameObject>($"Assets/Custom/Pieces/{metadata["catagory"]}/{metadata["prefab"]}.prefab");
                    Sprite sprite = EmbeddedResourceBundle.LoadAsset<Sprite>($"Assets/Custom/Icons/piece_icons/{metadata["sprite"]}.png");

                    // This is the interaction script for the shrine of challenge.
                    if(prefabscript)
                    {
                        prefab.AddComponent<Shrine>();
                    }

                    // Add the recipe with helper
                    if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo($"Loading {metadata["name"]} updated Recipe."); }
                    RequirementConfig[] recipe = new RequirementConfig[recipedata.Count];
                    int recipe_index = 0;
                    foreach (KeyValuePair<string, int> entry in recipedata)
                    {
                        recipe[recipe_index] = new RequirementConfig { Item = entry.Key, Amount = entry.Value };
                        recipe_index++;
                    }
                    if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo($"Building Piececonfig for {metadata["name"]}."); }
                    PieceConfig piececfg = new PieceConfig()
                    {
                        CraftingStation = $"{metadata["requiredBench"]}",
                        PieceTable = PieceTables.Hammer,
                        Category = metadata["catagory"],
                        Icon = sprite,
                        Requirements = recipe
                    };
                    PieceManager.Instance.AddPiece(new CustomPiece(prefab, fixReference: true, piececfg));
                    if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo($"Piece {metadata["name"]} Added!"); }
                }
                else
                {
                    if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo($"{metadata["name"]} is not enabled, and was not loaded."); }
                }
            }

            /// <summary>
            ///  Creates configuration values with automated segmentation on type
            /// </summary>
            /// <param name="config"></param>
            /// <param name="metadata"></param>
            /// <param name="itemdata"></param>
            /// <param name="itemtoggles"></param>
            private void CreateAndLoadConfigValues(VFConfig config, Dictionary<String, String> metadata, Dictionary<String, bool> piecetoggle, Dictionary<String, int> recipedata)
            {
                piecetoggle["enabled"] = config.BindServerConfig($"{metadata["catagory"]} - {metadata["name"]}", $"{metadata["short_item_name"]}-Enable", piecetoggle["enabled"], $"Enable/Disable the {metadata["name"]}.").Value;

                // Item bolean flag configs
                foreach (KeyValuePair<string, bool> entry in piecetoggle)
                {
                    if (entry.Key == "enabled") { continue; }
                    piecetoggle[entry.Key] = config.BindServerConfig($"{metadata["catagory"]} - {metadata["name"]}", $"{metadata["short_item_name"]}-{entry.Key}", entry.Value, $"{entry.Key} enable(true)/disable(false).", true).Value;
                }
                // Recipe Configs
                String recipe_cfg = "";
                foreach (KeyValuePair<string, int> entry in recipedata)
                {
                    if (recipe_cfg.Length > 0) { recipe_cfg += "|"; }
                    recipe_cfg += $"{entry.Key},{entry.Value}";
                }
                String RawRecipe;
                RawRecipe = config.BindServerConfig($"{metadata["catagory"]} - {metadata["name"]}", $"{metadata["short_item_name"]}-recipe", recipe_cfg, $"Recipe to craft, Find item ids: https://valheim.fandom.com/wiki/Item_IDs, at most 4 costs. Format: resouce_id,craft_cost eg: Wood,8|Iron,12", true).Value;
                if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo($"recieved rawrecipe data: '{RawRecipe}'"); }
                String[] RawRecipeEntries = RawRecipe.Split('|');
                Dictionary<String, int> updated_recipe = new Dictionary<String, int>();
                // we only clear out the default recipe if there is recipe data provided, otherwise we will continue to use the default recipe
                // TODO: Add a sanity check to ensure that recipe formatting is correct
                if (VFConfig.EnableDebugMode.Value == true)
                {
                    Logger.LogInfo($"recipe entries: {RawRecipeEntries.Length} : {RawRecipeEntries}");
                }
                if (RawRecipeEntries.Length >= 1)
                {
                    foreach (String recipe_entry in RawRecipeEntries)
                    {
                        String[] recipe_segments = recipe_entry.Split(',');
                        if (VFConfig.EnableDebugMode.Value == true)
                        {
                            String split_segments = "";
                            foreach (String segment in recipe_segments)
                            {
                                split_segments += $" {segment}";
                            }
                            Logger.LogInfo($"recipe segments: {split_segments} from {recipe_entry}");
                        }
                        Logger.LogInfo($"Setting recipe requirement: {recipe_segments[0]}={recipe_segments[1]}");
                        // Add a sanity check to ensure the prefab we are trying to use exists
                        updated_recipe.Add(recipe_segments[0], (Int32.Parse(recipe_segments[1])));
                    }
                    recipedata.Clear();
                    foreach (KeyValuePair<string, int> entry in updated_recipe)
                    {
                        if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo($"Updated recipe: resouce: {entry.Key} build: {entry.Value}"); }
                        recipedata.Add(entry.Key, entry.Value);
                    }
                }
                else
                {
                    Logger.LogWarning($"Configuration '{metadata["catagory"]} - {metadata["name"]} - {metadata["short_item_name"]}-recipe' was invalid and will be ignored, the default will be used.");
                }
            }

        }
    }
}
