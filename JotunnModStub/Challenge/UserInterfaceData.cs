using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimFortress.Challenge
{
    internal static class UserInterfaceData
    {
        // Right now maxlevel needs to correlate to: defined levels & level warning messages
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
            Localization.instance.Localize("$shrine_phase_warning19")
        };

        public static void PreparePhase(Int16 selected_level, bool boss_mode, GameObject shrine)
        {
            String challenge_warning = "This might hurt.";
            if (selected_level < 6)
            {
                challenge_warning = Localization.instance.Localize("$shrine_warning_meadows");
                if (boss_mode) { challenge_warning = Localization.instance.Localize("$shrine_warning_meadows_boss"); }
            }
            if (selected_level > 5 && selected_level < 11)
            {
                challenge_warning = Localization.instance.Localize("$shrine_warning_forest");
                if (boss_mode) { challenge_warning = Localization.instance.Localize("$shrine_warning_forest_boss"); }
            }
            if (selected_level > 10 && selected_level < 16)
            {
                challenge_warning = Localization.instance.Localize("$shrine_warning_swamp");
                if (boss_mode) { challenge_warning = Localization.instance.Localize("$shrine_warning_swamp_boss"); }
            }
            if (selected_level > 15 && selected_level < 21)
            {
                challenge_warning = Localization.instance.Localize("$shrine_warning_mountain");
                if (boss_mode) { challenge_warning = Localization.instance.Localize("$shrine_warning_mountain_boss"); }
            }
            if (selected_level > 20 && selected_level < 26)
            {
                challenge_warning = Localization.instance.Localize("$shrine_warning_plains");
                if (boss_mode) { challenge_warning = Localization.instance.Localize("$shrine_warning_plains_boss"); }
            }
            if (selected_level > 25 && selected_level < 31)
            {
                challenge_warning = Localization.instance.Localize("$shrine_warning_mistlands");
                if (boss_mode) { challenge_warning = Localization.instance.Localize("$shrine_warning_mistlands_boss"); }
            }
            List<Player> nearby_players = new List<Player> { };
            Player.GetPlayersInRange(shrine.transform.position, VFConfig.ShrineAnnouncementRange.Value, nearby_players);
            foreach (Player localplayer in nearby_players)
            {
                localplayer.Message(MessageHud.MessageType.Center, challenge_warning);
            }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Activated Shrine portal & sent warning message"); }
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


        public static List<String> UpdateRewards() {
            Dictionary<String, RewardEntry> possible_rewards = Rewards.GetResouceRewards();
            List<String> availableRewards = new List<String> { };
            var zs = ZoneSystem.instance;
            foreach (KeyValuePair<string, RewardEntry> entry in possible_rewards)
            {
                if (entry.Value.requiredBoss == "None")
                {
                    availableRewards.Add(entry.Key);
                    continue;
                }
                if (!zs)
                {
                    Jotunn.Logger.LogInfo("Zone system not available, skipping checks for global keys to set rewards options.");
                    // We can only add items that do not require a global key check if there is no zone system.
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
            }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Rewards Updated."); }
            return availableRewards;
        }

        public static List<String> UpdateLevels()
        {
            Int16 max_level = 5;
            var zs = ZoneSystem.instance;
            if (zs) // If the zonesystem does not exist, we can't check global keys. So skip it. This list will be updated on the UI open anyways.
            {
                if (zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledEikthyr)) { max_level += 5; }
                if (zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledElder)) { max_level += 5; }
                if (zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledBonemass)) { max_level += 5; }
                if (zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledModer)) { max_level += 5; }
                if (zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledYagluth)) { max_level += 5; }
                if (zs.GetGlobalKey("defeated_queen")) { max_level += 5; }
            }
            else
            {
                Jotunn.Logger.LogInfo("Zone system not available, skipping checks for global keys to increase max shrine levels.");
            }
            // If you have killed all of the tracked bosses, set the max possible level to 50.
            // if (max_level == 35) { max_level = 50; }

            // If the max challenge level is bigger than the level we determined we lower it to that
            if (max_level > (short)VFConfig.MaxChallengeLevel.Value) { max_level = (short)VFConfig.MaxChallengeLevel.Value; }

            // Empty out the current levels if it exists so we don't get duplicates
            List<String> currentLevels = new List<String> { };
            // Toss in all of the available levels
            String biome_or_boss = "";
            for (int i = 1; i <= max_level; i++)
            {
                if (i > 0 && i < 6) { biome_or_boss = Localization.instance.Localize("$shrine_menu_meadow"); }
                if (i > 5 && i < 11) { biome_or_boss = Localization.instance.Localize("$shrine_menu_forest"); }
                if (i > 10 && i < 16) { biome_or_boss = Localization.instance.Localize("$shrine_menu_swamp"); }
                if (i > 15 && i < 21) { biome_or_boss = Localization.instance.Localize("$shrine_menu_mountain"); }
                if (i > 20 && i < 26) { biome_or_boss = Localization.instance.Localize("$shrine_menu_plains"); }
                if (i > 25 && i < 31) { biome_or_boss = Localization.instance.Localize("$shrine_menu_mistland"); }
                currentLevels.Add($"{i} - {biome_or_boss}");
            }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Levels updated."); }
            return currentLevels;
        }
    }
}
