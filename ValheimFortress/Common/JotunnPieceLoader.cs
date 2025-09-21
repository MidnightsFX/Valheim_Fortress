using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ValheimFortress.Common
{
    internal class JotunnPieceLoader
    {
        internal static AssetBundle Assets;
        internal static List<PieceDefinition> resourceDefinitions = new List<PieceDefinition>();

        public bool AddPiece(PieceDefinition itemdef)
        {
            resourceDefinitions.Add(itemdef);
            return true;
        }
        public bool BatchSetup(AssetBundle assetBundle, bool reverse_order = true)
        {
            Assets = assetBundle;

            if (reverse_order) {
                resourceDefinitions.Reverse();
            }

            WireConfigs();

            bool on_server = false;
            if (ZNet.instance != null && ZNet.instance.IsServerInstance()) {
                on_server = true;
            }

            if (on_server == false) {
                // This is not needed on the server
                // The server does not actually do anything with prefabs, and is not responsible for modifying them
                BatchAddPieces();
                SetupOnChange();
            }


            // Flush to disk
            VFConfig.cfg.Save();
            VFConfig.SaveOnSet(true);
            return true;
        }

        private static bool WireConfigs()
        {
            foreach (PieceDefinition piecedef in resourceDefinitions) {
                //metadata
                piecedef.DisplayName = string.Join("", piecedef.Name.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));

                piecedef.enabled_cfg = VFConfig.BindServerConfig($"{piecedef.Category} - {piecedef.Name}", $"{piecedef.DisplayName}-craftable", piecedef.enabled, $"Enable/Disables {piecedef.Name}.");
                piecedef.requiredWorkstation_cfg = VFConfig.BindServerConfig($"{piecedef.Category} - {piecedef.Name}", $"{piecedef.DisplayName}-requiredBench", piecedef.requiredWorkstation, $"Sets the required crafting station for {piecedef.Name}.");

                piecedef.recipe.recipeConfig = VFConfig.BindServerConfig($"{piecedef.Category} - {piecedef.Name}", $"{piecedef.DisplayName}-recipe", BuildStringCraftingCostFromItemDef(piecedef), $"Recipe for {piecedef.Name}. Should be in the format of Prefab,Amount,refund|Prefab,Amount,refund eg: Wood,12,false|Stone,2,true");
                if (ValidateRecipeConfig(piecedef) == false) {
                    BuildRecipeReqsFromDefault(piecedef);
                }
            }
            return true;
        }

        private static bool BatchAddPieces() {
            foreach (PieceDefinition piecedef in resourceDefinitions) {
                GameObject PiecePrefab = Assets.LoadAsset<GameObject>($"Assets/Custom/Pieces/{piecedef.Category}/{piecedef.prefab}.prefab");
                Sprite PieceSprite = Assets.LoadAsset<Sprite>($"Assets/Custom/Icons/{piecedef.icon}.png");

                if(piecedef.setupScripts != null) {
                    piecedef.setupScripts(PiecePrefab);
                }

                PieceConfig piececfg = new PieceConfig() {
                    PieceTable = PieceTables.Hammer,
                    CraftingStation = $"{piecedef.requiredWorkstation_cfg.Value}",
                    Enabled = piecedef.enabled_cfg.Value,
                    Icon = PieceSprite,
                    Category = piecedef.Category.ToString(),
                    Requirements = piecedef.recipe.recipeReqs.ToArray()
                };
                PieceManager.Instance.AddPiece(new CustomPiece(PiecePrefab, fixReference: true, piececfg));
            }
            return true;
        }

        private static bool SetupOnChange() {
            foreach (PieceDefinition piecedef in resourceDefinitions) {

                // Enable/Disable the the piece from being built
                piecedef.enabled_cfg.SettingChanged += (_, EventArgs) => {
                    if (ZNet.instance.enabled == false) { return; }
                    Piece gopiece = PrefabManager.Instance.GetPrefab(piecedef.prefab).GetComponent<Piece>();
                    if (gopiece != null) {
                        Logger.LogInfo($"Setting {piecedef.Name} to {piecedef.enabled_cfg.Value}.");
                        gopiece.m_enabled = piecedef.enabled_cfg.Value; 
                    }
                };

                // Change the required crafting station
                piecedef.requiredWorkstation_cfg.SettingChanged += (_, EventArgs) => {
                    if (ZNet.instance.enabled == false) { return; }
                    RequiredBench_SettingChanged(piecedef);
                };

                // Onchange crafting cost
                piecedef.recipe.recipeConfig.SettingChanged += (_, EventArgs) => {
                    if (ZNet.instance.enabled == false) { return; }
                    if (ValidateRecipeConfig(piecedef)) {
                        BuildRequirements(piecedef);
                        Piece gopiece = PrefabManager.Instance.GetPrefab(piecedef.prefab).GetComponent<Piece>();
                        if (gopiece != null) {
                            Logger.LogInfo($"Updating crafting cost for {piecedef.prefab}");
                            gopiece.m_resources = piecedef.recipe.resolvedRequirements;
                        }
                    }
                };
            }

            return true;
        }

        private static void BuildRequirements(PieceDefinition piecedef) {
            Piece.Requirement[] newRequirements = new Piece.Requirement[piecedef.recipe.recipeReqs.Count];
            int index = 0;
            foreach (var recipe_entry in piecedef.recipe.recipeReqs)
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
            piecedef.recipe.resolvedRequirements = newRequirements;
        }

        private static void RequiredBench_SettingChanged(PieceDefinition piecedef)
        {
            CraftingStation targetCraftingStation = null;
            if (piecedef.requiredWorkstation_cfg.Value == "" || piecedef.requiredWorkstation_cfg.Value == null || piecedef.requiredWorkstation_cfg.Value.ToLower() == "NONE")
            {
                Logger.LogInfo("Setting required crafting station to none.");
                targetCraftingStation = null;
            } else {
                targetCraftingStation = PrefabManager.Instance.GetPrefab(piecedef.requiredWorkstation_cfg.Value)?.GetComponent<CraftingStation>();
                if (targetCraftingStation == null) {
                    Logger.LogWarning($"Required crafting station does not exist or does not have a crafting station componet, check your prefab name ({piecedef.requiredWorkstation_cfg.Value}).");
                    return;
                }
            }
            Logger.LogInfo($"Setting required crafting station to {piecedef.requiredWorkstation_cfg.Value}.");
            Piece gopiece = PrefabManager.Instance.GetPrefab(piecedef.prefab).GetComponent<Piece>();
            if (gopiece != null) { gopiece.m_craftingStation = targetCraftingStation; }
        }

        private static void Enabled_cfg_SettingChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void BuildRecipeReqsFromDefault(PieceDefinition piecedef)
        {
            List<RequirementConfig> requirements = new List<RequirementConfig>();
            foreach (var recipeIng in piecedef.recipe.recipeItems) {
                requirements.Add(new RequirementConfig { Item = recipeIng.prefab, Amount = recipeIng.amount, Recover = recipeIng.refund });
            }
            piecedef.recipe.recipeReqs = requirements;
        }

        private static string BuildStringCraftingCostFromItemDef(PieceDefinition piecedef)
        {
            List<string> recipe = new List<string>() { };
            foreach (var req in piecedef.recipe.recipeItems)
            {
                recipe.Add($"{req.prefab},{req.amount},{req.refund}");
            }
            return string.Join("|", recipe);
        }

        private static bool ValidateRecipeConfig(PieceDefinition piecedef)
        {
            List<RequirementConfig> requirements = new List<RequirementConfig>();
            try {
                string[] recipeConfig = piecedef.recipe.recipeConfig.Value.Split('|');
                foreach (string ingredient in recipeConfig) {
                    // Logger.LogInfo($"Ingrediant details: {ingredient}");
                    string[] ingredientConfig = ingredient.Split(',');
                    if (ingredientConfig.Length == 1) {
                        // This is the first run or deleted config entry scenario
                        return false;
                    }
                    if (ingredientConfig.Length != 3) {
                        Logger.LogWarning($"Invalid ({piecedef.Name}) cost config detected: {ingredient}. Needs three entries eg: Wood,1,true");
                        return false;
                    }
                    requirements.Add(new RequirementConfig { Item = ingredientConfig[0], Amount = int.Parse(ingredientConfig[1]), Recover = bool.Parse(ingredientConfig[2]) });
                }
                // Only happens if the recipe is valid
                piecedef.recipe.recipeReqs = requirements;
                return true;
            } catch {
                Logger.LogWarning($"Recipe is Invalid. Should have the format of Wood,1,1|Stone,2,0 - Prefab,cost,upgrade.");
                return false;
            }
        }
    }
}
