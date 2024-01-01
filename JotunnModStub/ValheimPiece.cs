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
using ValheimFortress.Defenses;

namespace ValheimFortress
{
    class ValheimFortressPieces
    {
        public ValheimFortressPieces(AssetBundle EmbeddedResourceBundle, VFConfig config)
        {
            if (VFConfig.EnableDebugMode.Value == true)
            {
                Logger.LogInfo("Loading Pieces.");
            }

            LoadRugs(EmbeddedResourceBundle, config);
            LoadGlassWalls(EmbeddedResourceBundle, config);
            LoadAlterOfChallenge(EmbeddedResourceBundle, config);
            LoadDefenses(EmbeddedResourceBundle, config);
        }

        private void LoadRugs(AssetBundle EmbeddedResourceBundle, VFConfig cfg)
        {
            // Green Jute Carpet
            new ValheimPiece(
                EmbeddedResourceBundle,
                cfg,
                new Dictionary<string, string>() {
                    { "name", "Green Jute Carpet" },
                    { "catagory", "VFortress" },
                    { "prefab", "VFgreen_jute_carpet_circle" },
                    { "sprite", "cricle_rug_green" },
                    { "requiredBench", "piece_workbench" }
                },
                new Dictionary<string, bool>() { },
                new Dictionary<string, Tuple<int, bool>>()
                {
                    { "Guck", Tuple.Create(1, true) },
                    { "JuteRed", Tuple.Create(4, true) },
                }
            );
            // Red Jute Carpet
            new ValheimPiece(
                EmbeddedResourceBundle,
                cfg,
                new Dictionary<string, string>() {
                    { "name", "Red Jute Carpet" },
                    { "catagory", "VFortress" },
                    { "prefab", "VFred_jute_carpet_circle" },
                    { "sprite", "cricle_rug_red" },
                    { "requiredBench", "piece_workbench" }
                },
                new Dictionary<string, bool>() { },
                new Dictionary<string, Tuple<int, bool>>()
                {
                    { "JuteRed", Tuple.Create(4, true) }
                }
            );
            // Yellow Jute Carpet
            new ValheimPiece(
                EmbeddedResourceBundle,
                cfg,
                new Dictionary<string, string>() {
                    { "name", "Yellow Jute Carpet" },
                    { "catagory", "VFortress" },
                    { "prefab", "VFyellow_jute_carpet_circle" },
                    { "sprite", "cricle_rug_yellow" },
                    { "requiredBench", "piece_workbench" }
                },
                new Dictionary<string, bool>() { },
                new Dictionary<string, Tuple<int, bool>>()
                {
                    { "Dandelion", Tuple.Create(4, true) },
                    { "JuteRed", Tuple.Create(4, true) },
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
                    { "catagory", "VFortress" },
                    { "prefab", "VFblue_crystal_wall" },
                    { "sprite", "blue_crystal_wall" },
                    { "requiredBench", "piece_workbench" }
                },
                new Dictionary<string, bool>() { },
                new Dictionary<string, Tuple<int, bool>>()
                {
                    { "Blueberries", Tuple.Create(4, true)},
                    { "Crystal", Tuple.Create(2, true)},
                }
            );
            // Red Crystal Wall
            new ValheimPiece(
                EmbeddedResourceBundle,
                cfg,
                new Dictionary<string, string>() {
                    { "name", "Red Crystal Wall" },
                    { "catagory", "VFortress" },
                    { "prefab", "VFred_crystal_wall" },
                    { "sprite", "red_crystal_wall" },
                    { "requiredBench", "piece_workbench" }
                },
                new Dictionary<string, bool>() { },
                new Dictionary<string, Tuple<int, bool>>()
                {
                    { "Raspberry", Tuple.Create(2, true) },
                    { "Crystal", Tuple.Create(2, true) },
                }
            );
            // Yellow Crystal Wall
            new ValheimPiece(
                EmbeddedResourceBundle,
                cfg,
                new Dictionary<string, string>() {
                    { "name", "Yellow Crystal Wall" },
                    { "catagory", "VFortress" },
                    { "prefab", "VFyellow_crystal_wall" },
                    { "sprite", "yellow_crystal_wall" },
                    { "requiredBench", "piece_workbench" }
                },
                new Dictionary<string, bool>() { },
                new Dictionary<string, Tuple<int, bool>>()
                {
                    { "Dandelion", Tuple.Create(2, true) },
                    { "Crystal", Tuple.Create(2, true) },
                }
            );
            // Green Crystal Wall
            new ValheimPiece(
                EmbeddedResourceBundle,
                cfg,
                new Dictionary<string, string>() {
                    { "name", "Green Crystal Wall" },
                    { "catagory", "VFortress" },
                    { "prefab", "VFgreen_crystal_wall" },
                    { "sprite", "green_crystal_wall" },
                    { "requiredBench", "piece_workbench" }
                },
                new Dictionary<string, bool>() { },
                new Dictionary<string, Tuple<int, bool>>()
                {
                    { "Guck", Tuple.Create(2, true) },
                    { "Crystal", Tuple.Create(2, true) },
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
                new Dictionary<string, Tuple<int, bool>>()
                {
                    { "Stone", Tuple.Create(23, true) },
                    { "Ruby", Tuple.Create(4, true) },
                    { "Coins", Tuple.Create(100, false) },
                },
                shrinescript: true
            );
        }

        private void LoadDefenses(AssetBundle EmbeddedResourceBundle, VFConfig cfg)
        {
            // Stone spikes
            new ValheimPiece(
                EmbeddedResourceBundle,
                cfg,
                new Dictionary<string, string>() {
                    { "name", "Stone Spikes" },
                    { "catagory", "Misc" },
                    { "prefab", "VFstone_stakes" },
                    { "sprite", "stone_spikes" },
                    { "requiredBench", "piece_stonecutter" }
                },
                new Dictionary<string, bool>() { },
                new Dictionary<string, Tuple<int, bool>>()
                {
                    { "Stone", Tuple.Create(30, false) },
                    { "Silver", Tuple.Create(2, true) },
                }
            );

            // Modified Turret
            new ValheimPiece(
                EmbeddedResourceBundle,
                cfg,
                new Dictionary<string, string>() {
                    { "name", "Magic Turret" },
                    { "catagory", "Misc" },
                    { "prefab", "VFpiece_turret" },
                    { "sprite", "modified_turret" },
                    { "requiredBench", "piece_artisanstation" }
                },
                new Dictionary<string, bool>() { },
                new Dictionary<string, Tuple<int, bool>>()
                {
                    { "BlackMetal", Tuple.Create(20, false) },
                    { "YggdrasilWood", Tuple.Create(20, false) },
                    { "MechanicalSpring", Tuple.Create(5, true) },
                    { "DragonTear", Tuple.Create(2, true) },
                },
                turretscript: true
            );
        }

        class ValheimPiece
        {
            String[] allowed_catagories = { "Furniture", "Building", "Crafting", "Misc", "VFortress" };
            String[] crafting_stations = { "forge", "piece_workbench", "blackforge", "piece_artisanstation", "piece_stonecutter" };
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
                Dictionary<String, Tuple<int, bool>> recipedata,
                bool shrinescript = false,
                bool turretscript = false
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

                    // These are custom unity componet scripts which have never seen the light of unity. So they arn't baked into the assets
                    // and must be added later. This allows these scripts to do things like be modified by config values, or reference Jotunn
                    if(shrinescript) { prefab.AddComponent<Shrine>(); prefab.AddComponent<UI>(); }
                    if(turretscript) { prefab.AddComponent<VFTurret>(); }

                    // Add the recipe with helper
                    if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo($"Loading {metadata["name"]} updated Recipe."); }
                    RequirementConfig[] recipe = new RequirementConfig[recipedata.Count];
                    int recipe_index = 0;
                    foreach (KeyValuePair<string, Tuple<int, bool>> entry in recipedata)
                    {
                        recipe[recipe_index] = new RequirementConfig { Item = entry.Key, Amount = entry.Value.Item1, Recover = entry.Value.Item2 };
                        recipe_index++;
                    }
                    if (VFConfig.EnableDebugMode.Value == true) {
                        Logger.LogInfo($"Piece {metadata["name"]} Recipe:");
                        foreach (RequirementConfig requirement in recipe)
                        {
                            Logger.LogInfo($"Requires: {requirement.Amount} {requirement.Item} Recover: {requirement.Recover}");
                        }
                        
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
            private void CreateAndLoadConfigValues(VFConfig config, Dictionary<String, String> metadata, Dictionary<String, bool> piecetoggle, Dictionary<String, Tuple<int, bool>> recipedata)
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
                foreach (KeyValuePair<string, Tuple<int, bool>> entry in recipedata)
                {
                    if (recipe_cfg.Length > 0) { recipe_cfg += "|"; }
                    recipe_cfg += $"{entry.Key},{entry.Value.Item1},{entry.Value.Item2}";
                }
                String RawRecipe;
                RawRecipe = config.BindServerConfig($"{metadata["catagory"]} - {metadata["name"]}", $"{metadata["short_item_name"]}-recipe", recipe_cfg, $"Recipe to craft, Find item ids: https://valheim.fandom.com/wiki/Item_IDs, at most 4 costs. Format: resouce_id,craft_cost-recover_flag eg: Wood,8,false|Iron,12,true", true).Value;
                if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo($"recieved rawrecipe data: '{RawRecipe}'"); }
                String[] RawRecipeEntries = RawRecipe.Split('|');
                Dictionary<String, Tuple<int, bool>> updated_recipe = new Dictionary<String, Tuple<int, bool>>();
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
                        if(VFConfig.EnableDebugMode.Value == true)
                        {
                            String split_segments = "";
                            foreach (String segment in recipe_segments)
                            {
                                split_segments += $" {segment}";
                            }
                            Logger.LogInfo($"recipe segments: {split_segments} from {recipe_entry}");
                        }
                        bool recovery = true;
                        if (recipe_segments.Length > 2) { recovery = bool.Parse(recipe_segments[2]); }
                        if (VFConfig.EnableDebugMode.Value) { Logger.LogInfo($"Setting recipe requirement: {recipe_segments[0]}={recipe_segments[1]} recover={recovery}"); }
                        // Add a sanity check to ensure the prefab we are trying to use exists
                        // This is likely going to need to be late-loaded configuration where we always use the default on modload and then switch the configuration values defined by the user
                        // closer the game init, this will allow setting prefabs/crafting stations that are outside of the scope of thsi mod. Will need more sanity checks.

                        updated_recipe.Add(recipe_segments[0], Tuple.Create(Int32.Parse(recipe_segments[1]), recovery));
                    }
                    recipedata.Clear();
                    foreach (KeyValuePair<string, Tuple<int, bool>> entry in updated_recipe)
                    {
                        if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo($"Updated recipe: resouce: {entry.Key} build: {entry.Value.Item1} recovery: {entry.Value.Item2}"); }
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
