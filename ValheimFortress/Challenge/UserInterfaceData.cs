using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ValheimFortress.Challenge
{
    internal static class UserInterfaceData
    {
        private static List<String> shrine_phase_warnings = new List<String>
        {
            Localization.instance.Localize("$shrine_phase_warning"),
            Localization.instance.Localize("$shrine_phase_warning2"),
            Localization.instance.Localize("$shrine_phase_warning3"),
            Localization.instance.Localize("$shrine_phase_warning4"),
            Localization.instance.Localize("$shrine_phase_warning5"),
            Localization.instance.Localize("$shrine_phase_warning6"),
            Localization.instance.Localize("$shrine_phase_warning7"),
            Localization.instance.Localize("$shrine_phase_warning8"),
            Localization.instance.Localize("$shrine_phase_warning9"),
            Localization.instance.Localize("$shrine_phase_warning10"),
            Localization.instance.Localize("$shrine_phase_warning11"),
            Localization.instance.Localize("$shrine_phase_warning12"),
            Localization.instance.Localize("$shrine_phase_warning13"),
            Localization.instance.Localize("$shrine_phase_warning14"),
            Localization.instance.Localize("$shrine_phase_warning15"),
            Localization.instance.Localize("$shrine_phase_warning16"),
            Localization.instance.Localize("$shrine_phase_warning17"),
            Localization.instance.Localize("$shrine_phase_warning18"),
            Localization.instance.Localize("$shrine_phase_warning19"),
            Localization.instance.Localize("$shrine_phase_warning20"),
            Localization.instance.Localize("$shrine_phase_warning21"),
            Localization.instance.Localize("$shrine_phase_warning22"),
            Localization.instance.Localize("$shrine_phase_warning23"),
            Localization.instance.Localize("$shrine_phase_warning24")
        };

        public static void PreparePhase(ChallengeLevelDefinition level_definition, bool boss_mode, GameObject shrine)
        {
            string challenge_warning;
            if (boss_mode)
            {
                challenge_warning = Localization.instance.Localize(level_definition.bossLevelWarningLocalization);
            } else {
                challenge_warning = Localization.instance.Localize(level_definition.levelWarningLocalization);
            }
            List<Player> nearby_players = new List<Player> { };
            Player.GetPlayersInRange(shrine.transform.position, VFConfig.ShrineAnnouncementRange.Value, nearby_players);
            foreach (Player localplayer in nearby_players)
            {
                localplayer.Message(MessageHud.MessageType.Center, challenge_warning);
            }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Sent warning message"); }
        }

        public static void PhasePausePhrase(GameObject shrine)
        {
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Picking and sending phase waiting text from {shrine_phase_warnings.Count} phrases."); }
            string selected_message = shrine_phase_warnings[UnityEngine.Random.Range(0, (shrine_phase_warnings.Count - 1))];
            List<Player> nearby_players = new List<Player> { };
            Player.GetPlayersInRange(shrine.transform.position, VFConfig.ShrineAnnouncementRange.Value, nearby_players);
            foreach (Player localplayer in nearby_players)
            {
                localplayer.Message(MessageHud.MessageType.Center, selected_message);
            }
        }


        public static List<String> UpdateRewards(short level_index) {
            Dictionary<String, RewardEntry> possible_rewards = RewardsData.resourceRewards;
            List<String> availableRewards = new List<String> { };
            var zs = ZoneSystem.instance;
            foreach (KeyValuePair<string, RewardEntry> entry in possible_rewards)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Checking reward:{entry.Key} required key: {entry.Value.requiredBoss} min level: {entry.Value.rewardMinLevelIndex} max level: {entry.Value.rewardMaxLevelIndex}"); }
                if ((entry.Value.rewardMinLevelIndex != 0 && entry.Value.rewardMinLevelIndex > level_index) || (entry.Value.rewardMaxLevelIndex != 0 && entry.Value.rewardMaxLevelIndex < level_index))
                {
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Reward {entry.Key} skipped due to level requirement not being met: {entry.Value.rewardMinLevelIndex} > {level_index} or {entry.Value.rewardMaxLevelIndex} < {level_index}."); }
                    continue;
                }
                if (entry.Value.requiredBoss == "None")
                {
                    availableRewards.Add(entry.Key);
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Reward that does not require any kills, enabling reward {entry.Key}."); }
                    continue;
                }
                if (!zs)
                {
                    Jotunn.Logger.LogInfo("Zone system not available, skipping checks for global keys to set rewards options.");
                    // We can only add items that do not require a global key check if there is no zone system.
                    continue;
                }
                // Support literal global keys
                if (zs.GetGlobalKey(entry.Value.requiredBoss))
                {
                    availableRewards.Add(entry.Key);
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Global key {entry.Value.requiredBoss} found, enabling reward {entry.Key}."); }
                    continue;
                }
                if (entry.Value.requiredBoss == "Eikythr" && zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledEikthyr))
                {
                    availableRewards.Add(entry.Key);
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Killed Eikythr, enabling reward {entry.Key}."); }
                    continue;
                }
                if (entry.Value.requiredBoss == "TheElder" && zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledElder))
                {
                    availableRewards.Add(entry.Key);
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Killed TheElder, enabling rewards {entry.Key}."); }
                    continue;
                }
                if (entry.Value.requiredBoss == "BoneMass" && zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledBonemass))
                {
                    availableRewards.Add(entry.Key);
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Killed BoneMass, enabling rewards {entry.Key}."); }
                    continue;
                }
                if (entry.Value.requiredBoss == "Moder" && zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledModer))
                {
                    availableRewards.Add(entry.Key);
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Killed Moder, enabling rewards {entry.Key}."); }
                    continue;
                }
                if (entry.Value.requiredBoss == "Yagluth" && zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledYagluth))
                {
                    availableRewards.Add(entry.Key);
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Killed Yagluth, enabling rewards {entry.Key}."); }
                    continue;
                }
                if (entry.Value.requiredBoss == "TheQueen" && zs.GetGlobalKey("defeated_queen"))
                {
                    availableRewards.Add(entry.Key);
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Killed TheQueen, enabling rewards {entry.Key}."); }
                    continue;
                }
                if (entry.Value.requiredBoss == "Fader" && zs.GetGlobalKey("defeated_fader"))
                {
                    availableRewards.Add(entry.Key);
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Killed Fader, enabling rewards {entry.Key}."); }
                    continue;
                }
            }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Rewards Updated."); }
            return availableRewards;
        }

        public static List<String> UpdateLevels(string shrine_type = "challenge")
        {
            var zs = ZoneSystem.instance;
            List<ChallengeLevelDefinition> clevels = Levels.GetChallengeLevelDefinitions();
            List<String> currentLevels = new List<String> { };
            short level_index = 1;
            if (zs) // If the zonesystem does not exist, we can't check global keys. So skip it. This list will be updated on the UI open anyways.
            {
                foreach(ChallengeLevelDefinition level in clevels)
                {
                    if (level.levelForShrineTypes[shrine_type] != true) { continue; }
                    if(level.requiredGlobalKey == "NONE")
                    {
                        currentLevels.Add($"{level_index} - {Localization.instance.Localize(level.levelMenuLocalization)}");
                        level_index += 1;
                        continue;
                    }
                    if (zs.GetGlobalKey(level.requiredGlobalKey)) 
                    {
                        currentLevels.Add($"{level_index} - {Localization.instance.Localize(level.levelMenuLocalization)}");
                    }
                    level_index += 1;
                }
            }
            else
            {
                Jotunn.Logger.LogInfo("Zone system not available, skipping checks for global keys to add shrine levels.");
            }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Levels updated."); }
            return currentLevels;
        }
    }
}
