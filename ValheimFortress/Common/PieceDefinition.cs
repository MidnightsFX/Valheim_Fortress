using Jotunn.Configs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ValheimFortress.Common
{
    enum PieceCategory {
        Misc,
        Crafting,
        Furniture,
        Building,
        HeavyBuild,
        Feasts,
        Food,
        Mead,
    }

    class PieceDefinition
    {
        // Metadata
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public PieceCategory Category { get; set; }
        public string prefab { get; set; }
        public string icon { get; set; }

        public BepInEx.Configuration.ConfigEntry<bool> enabled_cfg { get; set; }
        public bool enabled { get; set; } = true;
        public BepInEx.Configuration.ConfigEntry<string> requiredWorkstation_cfg { get; set; }
        public string requiredWorkstation { get; set; }
        public Action<GameObject> setupScripts { get; set; }
        public PieceCostDefinition recipe { get; set; }
    }

    class PieceCostDefinition
    {
        public BepInEx.Configuration.ConfigEntry<string> recipeConfig { get; set; }
        public List<PieceIngredient> recipeItems { get; set; }
        public List<RequirementConfig> recipeReqs { get; set; }
        public Piece.Requirement[] resolvedRequirements { get; set; }
    }

    class PieceIngredient
    {
        public string prefab { get; set; }
        public int amount { get; set; }
        public bool refund { get; set; } = false;
    }
}
