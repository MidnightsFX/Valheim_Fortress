using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using Logger = Jotunn.Logger;
using UnityEngine;
using BepInEx.Configuration;
using Jotunn.Configs;
using ValheimFortress.Defenses;
using ValheimFortress.Challenge;

namespace ValheimFortress.common
{
    public class JotunnPiece
    {
        Dictionary<String, String> PieceMetadata;
        Dictionary<String, Tuple<int, bool>> RecipeData;
        Dictionary<String, bool> PieceToggles;

        Dictionary<String, Tuple<int, bool>> UpdatedRecipeData = new Dictionary<string, Tuple<int, bool>>() { };

        GameObject ScenePrefab;

        GameObject PiecePrefab;
        Sprite PieceSprite;

        ConfigEntry<Boolean> EnabledVFConfig;
        ConfigEntry<String> RecipeVFConfig;
        ConfigEntry<String> BuiltAt;

        public JotunnPiece(Dictionary<String, String> metadata, Dictionary<string, bool> pieceToggles, Dictionary<String, Tuple<int, bool>> recipedata)
        {
            PieceMetadata = metadata;
            PieceToggles = pieceToggles;
            RecipeData = recipedata;

            // Add the internal short name
            PieceMetadata["short_item_name"] = string.Join("", metadata["name"].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));

            // Add universal defaults
            if (!PieceToggles.ContainsKey("enabled")) { PieceToggles.Add("enabled", true); }

            // Set asset references
            PiecePrefab = ValheimFortress.EmbeddedResourceBundle.LoadAsset<GameObject>($"Assets/Custom/Pieces/{PieceMetadata["catagory"]}/{PieceMetadata["prefab"]}.prefab");
            PieceSprite = ValheimFortress.EmbeddedResourceBundle.LoadAsset<Sprite>($"Assets/Custom/Icons/{PieceMetadata["sprite"]}.png");

            InitPieceConfigs();
            InitialPieceSetup();

            //Piece Specific
            if (PieceMetadata["prefab"] == "VFpiece_turret")
            {
                PiecePrefab.AddComponent<VFTurret>();
                VFConfig.BallistaDamage.SettingChanged += BallistaDamageChange;
                VFConfig.BallistaRange.SettingChanged += BallistaRange_SettingChanged;
                VFConfig.BallistaAmmoAccuracyPenalty.SettingChanged += BallistaAmmoAccuracyPenalty_SettingChanged;
                VFConfig.BallistaCooldownTime.SettingChanged += BallistaCooldownTime_SettingChanged;
            }

            if (PieceMetadata["prefab"] == "VFshrine_of_challenge") 
            {
                PiecePrefab.AddComponent<ChallengeShrine>();
                PiecePrefab.AddComponent<ChallengeShrineUI>();
                PiecePrefab.AddComponent<Spawner>();
            }

            if (PieceMetadata["prefab"] == "VFshine_of_gladiator")
            {
                PiecePrefab.AddComponent<ArenaShrine>();
                PiecePrefab.AddComponent<ArenaShrineUI>();
                PiecePrefab.AddComponent<Spawner>();
            }

            // Find and register this prefab in the scene, for in-place updates.
            PrefabManager.OnPrefabsRegistered += SetSceneParentPrefab;
        }

        private void BallistaCooldownTime_SettingChanged(object sender, EventArgs e)
        {
            foreach (GameObject go in findPrefabInScene())
            {
                if (VFConfig.EnableDebugMode.Value) { Logger.LogInfo($"Updating attack cooldown duration on {go.name}"); }
                go.GetComponent<VFTurret>().m_attackCooldown = VFConfig.BallistaCooldownTime.Value;
            }
        }

        private void BallistaAmmoAccuracyPenalty_SettingChanged(object sender, EventArgs e)
        {
            foreach (GameObject go in findPrefabInScene())
            {
                if (VFConfig.EnableDebugMode.Value) { Logger.LogInfo($"Updating accuracy on {go.name}"); }
                go.GetComponent<VFTurret>().m_ammo_accuracy = VFConfig.BallistaAmmoAccuracyPenalty.Value;
            }
        }

        private void BallistaRange_SettingChanged(object sender, EventArgs e)
        {
            foreach (GameObject go in findPrefabInScene())
            {
                if (VFConfig.EnableDebugMode.Value) { Logger.LogInfo($"Updating range on {go.name}"); }
                go.GetComponent<VFTurret>().m_viewDistance = VFConfig.BallistaRange.Value;
            }
        }

        private void BallistaDamageChange(object sender, EventArgs e)
        {
            foreach (GameObject go in findPrefabInScene())
            {
                if (VFConfig.EnableDebugMode.Value) { Logger.LogInfo($"Updating dmg on {go.name}"); }
                go.GetComponent<VFTurret>().m_Ammo.m_shared.m_damages.m_pierce = VFConfig.BallistaDamage.Value;
            }
        }

        private IEnumerable<GameObject> findPrefabInScene()
        {
            IEnumerable<GameObject> objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name.StartsWith(PieceMetadata["prefab"]));
            if (VFConfig.EnableDebugMode.Value) { Logger.LogInfo($"Found in scene objects: {objects.Count()}"); }
            return objects;
        }

        private void InitialPieceSetup()
        {
            CreateAndUpdateRecipe();
            RequirementConfig[] recipe = new RequirementConfig[UpdatedRecipeData.Count];
            int recipe_index = 0;
            foreach (KeyValuePair<string, Tuple<int, bool>> entry in UpdatedRecipeData)
            {
                recipe[recipe_index] = new RequirementConfig { Item = entry.Key, Amount = entry.Value.Item1, Recover = entry.Value.Item2 };
                recipe_index++;
            }

            PieceConfig piececfg = new PieceConfig()
            {
                Enabled = EnabledVFConfig.Value,
                CraftingStation = $"{PieceMetadata["requiredBench"]}",
                PieceTable = PieceTables.Hammer,
                Category = PieceMetadata["catagory"],
                Icon = PieceSprite,
                Requirements = recipe
            };

            PieceManager.Instance.AddPiece(new CustomPiece(PiecePrefab, fixReference: true, piececfg));
        }

        private void SetSceneParentPrefab()
        {
            IEnumerable<GameObject> scene_parents = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == PieceMetadata["prefab"]);
            if (VFConfig.EnableDebugMode.Value) { Logger.LogInfo($"Found {PieceMetadata["prefab"]} scene parent objects: {scene_parents.Count()}"); }
            ScenePrefab = scene_parents.First();
        }

        private void CreateAndUpdateRecipe()
        {
            // default recipe VFConfig
            String recipe_cfg_default = "";
            foreach (KeyValuePair<string, Tuple<int, bool>> entry in RecipeData)
            {
                if (recipe_cfg_default.Length > 0) { recipe_cfg_default += "|"; }
                recipe_cfg_default += $"{entry.Key},{entry.Value.Item1},{entry.Value.Item2}";
            }
            RecipeVFConfig = VFConfig.BindServerConfig($"{PieceMetadata["catagory"]} - {PieceMetadata["name"]}", $"{PieceMetadata["short_item_name"]}-recipe", recipe_cfg_default, $"Recipe to craft and upgrade costs. Find item ids: https://valheim.fandom.com/wiki/Item_IDs, at most 4 costs. Format: resouce_id,craft_cost,upgrade_cost eg: Wood,8,2|Iron,12,4|LeatherScraps,4,0", true);
            if (PieceRecipeVFConfigUpdater(RecipeVFConfig.Value, true) == false)
            {
                Logger.LogWarning($"{PieceMetadata["name"]} has an invalid recipe. The default will be used instead.");
                PieceRecipeVFConfigUpdater(recipe_cfg_default, true);
            }
            RecipeVFConfig.SettingChanged += BuildRecipeChanged_SettingChanged;
        }

        private void InitPieceConfigs()
        {
            // Populate defaults if they don't exist
            EnabledVFConfig = VFConfig.BindServerConfig($"{PieceMetadata["catagory"]} - {PieceMetadata["name"]}", $"{PieceMetadata["short_item_name"]}-enabled", PieceToggles["enabled"], $"Enable/Disable the {PieceMetadata["name"]}.");
            PieceToggles["enabled"] = EnabledVFConfig.Value;
            EnabledVFConfig.SettingChanged += BuildRecipeChanged_SettingChanged;

            // Set where the recipe can be crafted
            BuiltAt = VFConfig.BindServerConfig($"{PieceMetadata["catagory"]} - {PieceMetadata["name"]}", $"{PieceMetadata["short_item_name"]}-requiredBench", PieceMetadata["requiredBench"], $"The table required to allow building this piece, eg: 'forge', 'piece_workbench', 'blackforge', 'piece_artisanstation'.");
            PieceMetadata["requiredBench"] = BuiltAt.Value;
            BuiltAt.SettingChanged += RequiredBench_SettingChanged;
        }

        private void BuildRecipeChanged_SettingChanged(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(ConfigEntry<string>))
            {
                ConfigEntry<string> sendEntry = (ConfigEntry<string>)sender;
                if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo($"Recieved new piece VFConfig {sendEntry.Value}"); }
                // return if its an invalid change
                if (PieceRecipeVFConfigUpdater(sendEntry.Value) == false) { return; }
            }

            RequirementConfig[] recipe = new RequirementConfig[UpdatedRecipeData.Count];
            int recipe_index = 0;
            if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo("Validating and building requirementsVFConfig"); }
            foreach (KeyValuePair<string, Tuple<int, bool>> entry in UpdatedRecipeData)
            {
                if (PrefabManager.Instance.GetPrefab(entry.Key) == null)
                {
                    if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo($"{entry.Key} is not a valid prefab, skipping recipe update."); }
                    return;
                }
                if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo($"Checking entry {entry.Key} c:{entry.Value.Item1} r:{entry.Value.Item2}"); }
                recipe[recipe_index] = new RequirementConfig { Item = entry.Key, Amount = entry.Value.Item1, Recover = entry.Value.Item2 };
                recipe_index++;
            }
            if (EnabledVFConfig.Value)
            {
                if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo("Updating Piece."); }
                Piece.Requirement[] newRequirements = new Piece.Requirement[UpdatedRecipeData.Count];
                int index = 0;
                foreach (var recipe_entry in recipe)
                {
                    //recipe_entry.FixReferences();
                    Piece.Requirement piece_req = new Piece.Requirement();
                    piece_req.m_resItem = PrefabManager.Instance.GetPrefab(recipe_entry.Item.Replace("JVLmock_", ""))?.GetComponent<ItemDrop>();
                    piece_req.m_amount = recipe_entry.Amount;
                    piece_req.m_recover = recipe_entry.Recover;
                    newRequirements[index] = piece_req;
                    //newRequirements[index] = recipe_entry.GetRequirement();
                    index++;
                }
                if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo($"Fixed mock requirements {newRequirements.Length}."); }
                ScenePrefab.GetComponent<Piece>().m_resources = newRequirements;
                foreach (GameObject go in findPrefabInScene())
                {
                    if (VFConfig.EnableDebugMode.Value) { Logger.LogInfo($"Updating requirements on {go.name}"); }
                    go.GetComponent<Piece>().m_resources = newRequirements;
                }
            } else {
                // Set this piece not craftable
                ScenePrefab.GetComponent<Piece>().m_enabled = false;
            }
        }

        private void RequiredBench_SettingChanged(object sender, EventArgs e)
        {
            if (BuiltAt.Value == "" || BuiltAt.Value == null || BuiltAt.Value.ToLower() == "NONE")
            {
                Logger.LogInfo("Setting required crafting station to none.");
                ScenePrefab.GetComponent<Piece>().m_craftingStation = null;
                return;
            }

            CraftingStation craftable_at = PrefabManager.Instance.GetPrefab(BuiltAt.Value)?.GetComponent<CraftingStation>();
            if (craftable_at == null)
            {
                Logger.LogWarning($"Required crafting station does not exist or does not have a crafting station componet, check your prefab name ({BuiltAt.Value}).");
                return;
            }

            if (VFConfig.EnableDebugMode.Value == true) { Logger.LogInfo($"Setting crafting station to {BuiltAt.Value}."); }
            ScenePrefab.GetComponent<Piece>().m_craftingStation = craftable_at;
        }

        private bool PieceRecipeVFConfigUpdater(String rawrecipe, bool startup = false)
        {
            String[] RawRecipeEntries = rawrecipe.Split('|');
            // Logger.LogInfo($"{RawRecipeEntries.Length} {string.Join(", ", RawRecipeEntries)}");
            Dictionary<String, Tuple<int, bool>> updated_pieceRecipe = new Dictionary<String, Tuple<int, bool>>();
            // we only clear out the default recipe if there is recipe data provided, otherwise we will continue to use the default recipe
            // TODO: Add a sanity check to ensure that recipe formatting is correct
            if (RawRecipeEntries.Length >= 1)
            {
                foreach (String recipe_entry in RawRecipeEntries)
                {
                    //Logger.LogInfo($"{recipe_entry}");
                    String[] recipe_segments = recipe_entry.Split(',');
                    if (recipe_segments.Length != 3)
                    {
                        Logger.LogWarning($"{recipe_entry} is invalid, it does not have enough segments. Proper format is: PREFABNAME,COST,REFUND_BOOL eg: Wood,8,false");
                        return false;
                    }
                    if (VFConfig.EnableDebugMode.Value == true)
                    {
                        String split_segments = "";
                        foreach (String segment in recipe_segments)
                        {
                            split_segments += $" {segment}";
                        }
                        //Logger.LogInfo($"recipe segments: {split_segments} from {recipe_entry}");
                    }
                    // Add a sanity check to ensure the prefab we are trying to use exists
                    if (startup == false)
                    {
                        if (PrefabManager.Instance.GetPrefab(recipe_segments[0]) == null)
                        {
                            Logger.LogWarning($"{recipe_segments[0]} is an invalid prefab and does not exist.");
                            return false;
                        }
                    }
                    if (recipe_segments[0].Length == 0 || recipe_segments[1].Length == 0 || recipe_segments[2].Length == 0)
                    {
                        Logger.LogWarning($"{recipe_entry} is invalid, one segment does not have enough data. Proper format is: PREFABNAME,CRAFT_COST,REFUND_BOOL eg: Wood,8,false");
                        return false;
                    }
                    bool refund_flag_parse;
                    if (bool.TryParse(recipe_segments[2], out refund_flag_parse) == false)
                    {
                        Logger.LogWarning($"{recipe_entry} is invalid, the REFUND_BOOL could not be parsed to (true/false). Proper format is: PREFABNAME,CRAFT_COST,REFUND_BOOL eg: Wood,8,false");
                        return false;
                    }


                    if (VFConfig.EnableDebugMode.Value == true)
                    {
                        Logger.LogInfo($"prefab: {recipe_segments[0]} c:{recipe_segments[1]} u:{recipe_segments[2]}");
                    }
                    updated_pieceRecipe.Add(recipe_segments[0], new Tuple<int, bool>(Int32.Parse(recipe_segments[1]), refund_flag_parse));
                }
                //Logger.LogInfo("Done parsing recipe");
                UpdatedRecipeData.Clear();
                foreach (KeyValuePair<string, Tuple<int, bool>> entry in updated_pieceRecipe)
                {
                    UpdatedRecipeData.Add(entry.Key, entry.Value);
                }
                //Logger.LogInfo("Set UpdatedRecipe");
                if (VFConfig.EnableDebugMode.Value == true)
                {
                    String recipe_string = "";
                    foreach (KeyValuePair<string, Tuple<int, bool>> entry in updated_pieceRecipe)
                    {
                        recipe_string += $" {entry.Key} c:{entry.Value.Item1} r:{entry.Value.Item2}";
                    }
                    Logger.LogInfo($"Updated recipe:{recipe_string}");
                }
                return true;
            }
            else
            {
                Logger.LogWarning($"Invalid recipe: {rawrecipe}. defaults will be used. Check your prefab names.");
                UpdatedRecipeData = RecipeData;

            }
            return false;
        }
    }
}
