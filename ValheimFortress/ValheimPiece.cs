using System.Collections.Generic;
using ValheimFortress.Challenge;
using ValheimFortress.Common;
using ValheimFortress.Defenses;

namespace ValheimFortress
{
    class ValheimFortressPieces
    {
        JotunnPieceLoader Loader = new JotunnPieceLoader();
        public ValheimFortressPieces() {
            Logger.LogInfo("Loading Pieces.");

            LoadAlterOfChallenge();
            LoadDefenses();
            Loader.BatchSetup(ValheimFortress.EmbeddedResourceBundle);
        }

        private void LoadAlterOfChallenge()
        {
            // Shrine of Challenge
            PieceDefinition challengeAlter = new PieceDefinition();
            challengeAlter.Name = "Alter of Challenge";
            challengeAlter.Category = PieceCategory.Misc;
            challengeAlter.prefab = "VFshrine_of_challenge";
            challengeAlter.icon = "shrine_of_challenge";
            challengeAlter.enabled = true;
            challengeAlter.requiredWorkstation = "piece_workbench";
            challengeAlter.recipe = new PieceCostDefinition() {
                recipeItems = new List<PieceIngredient>() {
                    new PieceIngredient() { prefab = "Stone", amount = 34, refund = true },
                    new PieceIngredient() { prefab = "Ruby", amount = 4, refund = true },
                    new PieceIngredient() { prefab = "Coins", amount = 100, refund = false }
                },
            };
            challengeAlter.setupScripts = (go) => {
                go.AddComponent<ChallengeShrine>();
                go.AddComponent<ChallengeShrineUI>();
                go.AddComponent<Spawner>();
            };
            Loader.AddPiece(challengeAlter);

            // Shrine of the Arena
            PieceDefinition arenaAlter = new PieceDefinition();
            arenaAlter.Name = "Alter of Arena";
            arenaAlter.Category = PieceCategory.Misc;
            arenaAlter.prefab = "VFshine_of_gladiator";
            arenaAlter.icon = "alter_of_arena";
            arenaAlter.enabled = true;
            arenaAlter.requiredWorkstation = "piece_workbench";
            arenaAlter.recipe = new PieceCostDefinition() {
                recipeItems = new List<PieceIngredient>() {
                    new PieceIngredient() { prefab = "Stone", amount = 23, refund = true },
                    new PieceIngredient() { prefab = "Coins", amount = 100, refund = false }
                },
            };
            arenaAlter.setupScripts = (go) => {
                go.AddComponent<ArenaShrine>();
                go.AddComponent<ArenaShrineUI>();
                go.AddComponent<Spawner>();
            };
            Loader.AddPiece(arenaAlter);
        }

        private void LoadDefenses()
        {
            // Stone spikes
            PieceDefinition stoneSpikes = new PieceDefinition();
            stoneSpikes.Name = "Stone Spikes";
            stoneSpikes.Category = PieceCategory.Misc;
            stoneSpikes.prefab = "VFstone_stakes";
            stoneSpikes.icon = "stone_spikes";
            stoneSpikes.enabled = true;
            stoneSpikes.requiredWorkstation = "piece_stonecutter";
            stoneSpikes.recipe = new PieceCostDefinition()
            {
                recipeItems = new List<PieceIngredient>() {
                    new PieceIngredient() { prefab = "Stone", amount = 30, refund = false },
                    new PieceIngredient() { prefab = "Silver", amount = 2, refund = true }
                },
            };
            Loader.AddPiece(stoneSpikes);

            // Auto turret
            PieceDefinition autoBallista = new PieceDefinition();
            autoBallista.Name = "Auto Ballista";
            autoBallista.Category = PieceCategory.Misc;
            autoBallista.prefab = "VFpiece_turret";
            autoBallista.icon = "modified_turret";
            autoBallista.enabled = true;
            autoBallista.requiredWorkstation = "piece_artisanstation";
            autoBallista.recipe = new PieceCostDefinition() {
                recipeItems = new List<PieceIngredient>() {
                    new PieceIngredient() { prefab = "BlackMetal", amount = 20, refund = true },
                    new PieceIngredient() { prefab = "YggdrasilWood", amount = 20, refund = true },
                    new PieceIngredient() { prefab = "MechanicalSpring", amount = 5, refund = true },
                    new PieceIngredient() { prefab = "DragonTear", amount = 2, refund = true }
                },
            };
            autoBallista.setupScripts = (go) => {
                go.AddComponent<VFTurret>();
            };
            VFConfig.BallistaDamage.SettingChanged += BallistaOnChange.BallistaDamageChange;
            VFConfig.BallistaRange.SettingChanged += BallistaOnChange.BallistaRange_SettingChanged;
            VFConfig.BallistaAmmoAccuracyPenalty.SettingChanged += BallistaOnChange.BallistaAmmoAccuracyPenalty_SettingChanged;
            VFConfig.BallistaCooldownTime.SettingChanged += BallistaOnChange.BallistaCooldownTime_SettingChanged;
            Loader.AddPiece(autoBallista);
        }

    }
}
