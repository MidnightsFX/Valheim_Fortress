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
using ValheimFortress.common;

namespace ValheimFortress
{
    class ValheimFortressPieces
    {
        public ValheimFortressPieces()
        {
            if (VFConfig.EnableDebugMode.Value == true)
            {
                Logger.LogInfo("Loading Pieces.");
            }

            LoadAlterOfChallenge();
            LoadDefenses();
        }

        private void LoadAlterOfChallenge()
        {
            // Alter of Challenge
            new JotunnPiece(
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
                }
            );

            // Alter of the Arena
            new JotunnPiece(
                new Dictionary<string, string>() {
                    { "name", "Alter of the Arena" },
                    { "catagory", "Misc" },
                    { "prefab", "VFshine_of_gladiator" },
                    { "sprite", "alter_of_arena" },
                    { "requiredBench", "piece_workbench" }
                },
                new Dictionary<string, bool>() { },
                new Dictionary<string, Tuple<int, bool>>()
                {
                    { "Stone", Tuple.Create(23, true) },
                    { "Coins", Tuple.Create(100, false) }
                }
            );
        }

        private void LoadDefenses()
        {
            // Stone spikes
            new JotunnPiece(
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
            new JotunnPiece(
                new Dictionary<string, string>() {
                    { "name", "Auto Ballista" },
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
                }
            );
        }

    }
}
