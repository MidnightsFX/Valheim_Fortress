using Jotunn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ValheimFortress.Challenge
{
    public class UI : MonoBehaviour
    {
        static GameObject ChallengePanel;

        // this is unset until the shrine building calls for the UI, in which case it is then set
        static Shrine Shrine;

        static List<String> currentLevels = new List<String> {};
        static List<String> availableRewards = new List<String> {};
        public static GameObject levelSelector;
        public static GameObject rewardSelector;
        public static GameObject hardModeToggle;
        public static GameObject bossModeToggle;
        public static GameObject siegeModeToggle;
        public static GameObject estimate_text;
        public static short estimatedRewards = 0;
        public static string estimatedRewardName = "";

        private static GameObject estimate_symbol;
        private static GameObject hardmode_label;
        private static GameObject hardmode_desc;
        private static GameObject hardmode_reward_desc;
        private static GameObject bossmode_label;
        private static GameObject bossmode_desc;
        private static GameObject bossmode_reward_desc;
        private static GameObject siegemode_label;
        private static GameObject siegemode_desc;
        private static GameObject siegemode_reward_desc;


        public static short selected_level = 0;
        public static bool current_hard_mode = false;
        public static bool current_boss_mode = false;
        public static bool current_siege_mode = false;

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

        public static bool IsPanelVisible()
        {
            return ChallengePanel.activeSelf;
        }

        public void Update()
        {
            if (IsPanelVisible()) {
                // Skip the whole thing if we don't need to estimate rewards
                if (VFConfig.EnableRewardsEstimate.Value == false) { return; }
                bool value_changed = false;
                string rewardName = availableRewards[rewardSelector.GetComponent<Dropdown>().value];
                short level = (short)(levelSelector.GetComponent<Dropdown>().value + 1);
                bool hardmode_status = false;
                bool bossmode_status = false;
                bool siegemode_status = false;
                if (rewardName != estimatedRewardName || selected_level != level) { value_changed = true; }
                if (VFConfig.EnableHardModifier.Value)
                {
                    hardmode_status = hardModeToggle.GetComponent<Toggle>().isOn;
                    if (hardmode_status != current_hard_mode) { value_changed = true; }
                    current_hard_mode = hardmode_status;
                }
                if (VFConfig.EnableBossModifier.Value)
                {
                    bossmode_status = bossModeToggle.GetComponent<Toggle>().isOn;
                    if (bossmode_status != current_boss_mode) { value_changed = true; }
                    current_boss_mode = bossmode_status;
                }
                if (VFConfig.EnableSiegeModifer.Value)
                {
                    siegemode_status = siegeModeToggle.GetComponent<Toggle>().isOn;
                    if (siegemode_status != current_siege_mode) { value_changed = true; }
                    current_siege_mode = siegemode_status;
                }
                // Skip the update if the estimate is the same as the previous
                if (value_changed == false) { return; }
                estimatedRewardName = rewardName;
                selected_level = level;
                estimatedRewards = Rewards.DetermineRewardAmount(estimatedRewardName, selected_level, hardmode_status, bossmode_status, siegemode_status);
                estimate_text.GetComponent<Text>().text = $"{estimatedRewards}";
            }
        }

        public void Awake()
        {
            Shrine = this.GetComponent<Shrine>();
            CreateStaticUIObjects(true);
            // CreateChallengeUI();
        }

        private static void UpdateLevelsAndRewards()
        {
            Int16 max_level = 5;
            Dictionary<String, RewardEntry> possible_rewards = Rewards.GetResouceRewards();
            availableRewards = new List<String> { };
            var zs = ZoneSystem.instance;
            foreach (KeyValuePair<string, RewardEntry> entry in possible_rewards)
            {
                if(entry.Value.requiredBoss == "None") { 
                    availableRewards.Add(entry.Key);
                    continue;
                }
                if (!zs) 
                {
                    Jotunn.Logger.LogInfo("Zone system not available, skipping checks for global keys to set rewards options.");
                    // We can only add items that do not require a global key check if there is no zone system.
                    continue; 
                }
                if(entry.Value.requiredBoss == "Eikythr" && zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledEikthyr)) {
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

            if (zs) // If the zonesystem does not exist, we can't check global keys. So skip it. This list will be updated on the UI open anyways.
            {
                if (zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledEikthyr)) { max_level += 5; }
                if (zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledElder)) { max_level += 5; }
                if (zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledBonemass)) { max_level += 5; }
                if (zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledModer)) { max_level += 5; }
                if (zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledYagluth)) { max_level += 5; }
                if (zs.GetGlobalKey("defeated_queen")) { max_level += 5; }
            } else
            {
                Jotunn.Logger.LogInfo("Zone system not available, skipping checks for global keys to increase max shrine levels.");
            }
            // If you have killed all of the tracked bosses, set the max possible level to 50.
            // if (max_level == 35) { max_level = 50; }

            // If the max challenge level is bigger than the level we determined we lower it to that
            if (max_level > (short)VFConfig.MaxChallengeLevel.Value) { max_level = (short)VFConfig.MaxChallengeLevel.Value; }

            // Empty out the current levels if it exists so we don't get duplicates
            currentLevels = new List<String> { };
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
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Levels and rewards updated."); }
        }

        private void StartChallenge()
        {
            HideUI();
            String selected_reward = availableRewards[rewardSelector.GetComponent<Dropdown>().value];
            // Trying to decide if we want to do this, which will use the wave dropdown text value as the entry, or if we should just do the index + 1, which is currently the same
            // TODO: this should be reworked when we switch from wave levels to another system
            // Int16 selected_level = Int16.Parse(levelSelector.GetComponent<Dropdown>().options[levelSelector.GetComponent<Dropdown>().value].text);
            // This takes the dropdowns index (+1 since its zero indexd), which means we always need the full number of levels populated, but that we can rename the levels however
            Int16 selected_level = (Int16)(levelSelector.GetComponent<Dropdown>().value + 1);
            bool hard_mode = false;
            if (VFConfig.EnableHardModifier.Value) { hard_mode = hardModeToggle.GetComponent<Toggle>().isOn; }
            bool boss_mode = false;
            if (VFConfig.EnableBossModifier.Value) { boss_mode = bossModeToggle.GetComponent<Toggle>().isOn; }
            bool siege_mode = false;
            if (VFConfig.EnableSiegeModifer.Value) { siege_mode = siegeModeToggle.GetComponent<Toggle>().isOn; }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Shrine challenge. Selected reward: {selected_reward}, selected level: {selected_level}"); }
            // Start the coroutine that sends the warning text
            PreparePhase(selected_level, boss_mode, Shrine.gameObject);
            Shrine.SetLevel(selected_level);
            Shrine.SetReward(selected_reward);
            if (hard_mode) { Shrine.SetHardMode(); }
            if (boss_mode) { Shrine.SetBossMode(); }
            if (siege_mode) { Shrine.SetSiegeMode(); }
            // This call will trigger the shrine to start building out the wave and running the challenge
            Shrine.SetStartChallenge();
        }

        private static void PreparePhase(Int16 selected_level, bool boss_mode, GameObject shrine)
        {
            String challenge_warning = "This might hurt.";
            if (selected_level < 6) {
                challenge_warning = Localization.instance.Localize("$shrine_warning_meadows");
                if (boss_mode) { challenge_warning = Localization.instance.Localize("$shrine_warning_meadows_boss"); }
            }
            if (selected_level > 5 && selected_level < 11) { 
                challenge_warning = Localization.instance.Localize("$shrine_warning_forest");
                if (boss_mode) { challenge_warning = Localization.instance.Localize("$shrine_warning_forest_boss"); }
            }
            if (selected_level > 10 && selected_level < 16) { 
                challenge_warning = Localization.instance.Localize("$shrine_warning_swamp");
                if (boss_mode) { challenge_warning = Localization.instance.Localize("$shrine_warning_swamp_boss"); }
            }
            if (selected_level > 15 && selected_level < 21) { 
                challenge_warning = Localization.instance.Localize("$shrine_warning_mountain");
                if (boss_mode) { challenge_warning = Localization.instance.Localize("$shrine_warning_mountain_boss"); }
            }
            if (selected_level > 20 && selected_level < 26) { 
                challenge_warning = Localization.instance.Localize("$shrine_warning_plains");
                if (boss_mode) { challenge_warning = Localization.instance.Localize("$shrine_warning_plains_boss"); }
            }
            if (selected_level > 25 && selected_level < 31) {
                challenge_warning = Localization.instance.Localize("$shrine_warning_mistlands");
                if (boss_mode) { challenge_warning = Localization.instance.Localize("$shrine_warning_mistlands_boss"); }
            }
            List<Player> nearby_players = new List<Player> { };
            Player.GetPlayersInRange(shrine.transform.position, VFConfig.ShrineAnnouncementRange.Value, nearby_players);
            foreach(Player localplayer in nearby_players)
            {
                localplayer.Message(MessageHud.MessageType.Center, challenge_warning);
            }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Activated Shrine portal & sent warning message"); }
        }

        public static void PhasePausePhrase(GameObject shrine)
        {
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Picking and sending phase waiting text from {shrine_phase_warnings.Count} phrases."); }
            string selected_message = shrine_phase_warnings[UnityEngine.Random.Range(0, (shrine_phase_warnings.Count -1))];
            List<Player> nearby_players = new List<Player> { };
            Player.GetPlayersInRange(shrine.transform.position, VFConfig.ShrineAnnouncementRange.Value, nearby_players);
            foreach (Player localplayer in nearby_players)
            {
                localplayer.Message(MessageHud.MessageType.Center, selected_message);
            }
        }


        public void DisplayUI()
        {
            CreateStaticUIObjects(true);
            CreateChallengeUI();
            ChallengePanel.SetActive(true);
            GUIManager.BlockInput(true);
            Shrine.SetShrineUIStatus(true);
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Enabled UI from Shrine object."); }
        }

        public void HideUI()
        {
            ChallengePanel.SetActive(false);
            GUIManager.BlockInput(false);
            Shrine.SetShrineUIStatus(false);
        }

        public void CreateStaticUIObjects(bool should_update_ui)
        {
            // This was supposed to allow this is not need to be recreated much/at all
            // However, there are a few edge cases that cause this to b
            if (should_update_ui == false) { return; }
            if (GUIManager.Instance == null)
            {
                Jotunn.Logger.LogError("GUIManager instance is null");
                return;
            }

            if (!GUIManager.CustomGUIFront)
            {
                Jotunn.Logger.LogError("GUIManager CustomGUI is null");
                return;
            }
            // Create the panel object
            ChallengePanel = GUIManager.Instance.CreateWoodpanel(
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0, 0),
                width: 550,
                height: 550,
                draggable: true);
            // This hides the panel immediately
            ChallengePanel.SetActive(false);

            // Create the title
            GUIManager.Instance.CreateText(
                text: Localization.instance.Localize("$shrine_header"),
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(60f, 230f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 30,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 400f,
                height: 40f,
                addContentSizeFitter: false);
            // Subtitle description
            GUIManager.Instance.CreateText(
                text: Localization.instance.Localize("$shrine_description"),
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(45f, 155f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 14,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 500f,
                height: 80f,
                addContentSizeFitter: false);
            GUIManager.Instance.CreateText(
                text: Localization.instance.Localize("$shrine_warning"),
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(85f, 95f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 18,
                color: GUIManager.Instance.ValheimYellow,
                outline: true,
                outlineColor: Color.black,
                width: 400f,
                height: 40f,
                addContentSizeFitter: false);


            // Rewards text
            GUIManager.Instance.CreateText(
                text: Localization.instance.Localize("$shrine_reward_label"),
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(-60f, 50f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 300f,
                height: 40f,
                addContentSizeFitter: false);

            // Level selector text
            GUIManager.Instance.CreateText(
                text: Localization.instance.Localize("$shrine_level_label"),
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(-60f, -5f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 300f,
                height: 40f,
                addContentSizeFitter: false);

            GUIManager.Instance.CreateText(
                text: Localization.instance.Localize("$shrine_modifiers_label"),
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(-138f, -48f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 100f,
                height: 40f,
                addContentSizeFitter: false);
            GUIManager.Instance.CreateText(
                text: Localization.instance.Localize("$shrine_modifiers_rewards_label"),
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(28f, -48f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 200f,
                height: 40f,
                addContentSizeFitter: false);

            // Create the start button object
            GameObject startButtonObj = GUIManager.Instance.CreateButton(
                text: Localization.instance.Localize("$shrine_confirm"),
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0f, -224f),
                width: 150f,
                height: 60f);
            startButtonObj.SetActive(true);

            // Create the close button object
            GameObject cancelButtonObj = GUIManager.Instance.CreateButton(
                text: Localization.instance.Localize("$shrine_cancel"),
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(240f, 240f),
                width: 40f,
                height: 40f);
            cancelButtonObj.SetActive(true);

            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Adding UI Listeners"); }
            // Add a listener to the button to close the panel again
            Button cancelButton = cancelButtonObj.GetComponent<Button>();
            cancelButton.onClick.AddListener(HideUI);
            // Add a listener to the button to close the panel and trigger the challenge scripts
            Button startButton = startButtonObj.GetComponent<Button>();
            startButton.onClick.AddListener(StartChallenge);
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Shrine UI Created."); }
        }

        public void CreateChallengeUI()
        {
            // Always want to update the rewards and challenge levels
            UpdateLevelsAndRewards();
            // We specifically want to be able to completely rebuild the UI when this is called again to update the levels and rewards dynamically

            // Create the rewards selector dropdown
            rewardSelector = GUIManager.Instance.CreateDropDown(
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(45f, 60f),
                fontSize: 16,
                width: 200f,
                height: 40f);
            rewardSelector.GetComponent<Dropdown>().AddOptions(availableRewards);


            if (VFConfig.EnableRewardsEstimate.Value)
            {
                // Shrine reward estimate
                estimate_symbol = GUIManager.Instance.CreateText(
                    text: Localization.instance.Localize("$shrine_reward_estimate"),
                    parent: ChallengePanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(180f, 50f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 16,
                    color: GUIManager.Instance.ValheimBeige,
                    outline: true,
                    outlineColor: Color.black,
                    width: 40f,
                    height: 40f,
                    addContentSizeFitter: false);
                // Destroy the old text before rendering new text if it has been updated
                if (estimate_text != null) { Destroy(estimate_text); }
                estimate_text = GUIManager.Instance.CreateText(
                    text: $"{estimatedRewards}",
                    parent: ChallengePanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(220f, 50f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 16,
                    color: GUIManager.Instance.ValheimBeige,
                    outline: true,
                    outlineColor: Color.black,
                    width: 100f,
                    height: 40f,
                    addContentSizeFitter: false);
            } else {
                // Only destroy things we have previously created.
                if (estimate_symbol != null) { Destroy(estimate_symbol.gameObject); estimate_symbol = null; }
                if (estimate_text != null) { Destroy(estimate_text.gameObject); estimate_text = null; }
            }

            // create the wave selector dropdown
            levelSelector = GUIManager.Instance.CreateDropDown(
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(45f, 5f),
                fontSize: 16,
                width: 200f,
                height: 40f);
            levelSelector.GetComponent<Dropdown>().AddOptions(currentLevels);

            if (VFConfig.EnableHardModifier.Value)
            {
                // Hardmode toggle text
                    hardmode_label = GUIManager.Instance.CreateText(
                        text: Localization.instance.Localize("$shrine_hard_mode_label"),
                        parent: ChallengePanel.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(-140f, -75f),
                        font: GUIManager.Instance.AveriaSerifBold,
                        fontSize: 16,
                        color: GUIManager.Instance.ValheimBeige,
                        outline: true,
                        outlineColor: Color.black,
                        width: 200f,
                        height: 40f,
                        addContentSizeFitter: false);
                    hardmode_desc = GUIManager.Instance.CreateText(
                        text: Localization.instance.Localize("$shrine_hard_mode_description"),
                        parent: ChallengePanel.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(160f, -77f),
                        font: GUIManager.Instance.AveriaSerifBold,
                        fontSize: 14,
                        color: GUIManager.Instance.ValheimBeige,
                        outline: true,
                        outlineColor: Color.black,
                        width: 350f,
                        height: 40f,
                        addContentSizeFitter: false);
                    hardmode_reward_desc = GUIManager.Instance.CreateText(
                        text: Localization.instance.Localize("$shrine_hard_mode_reward"),
                        parent: ChallengePanel.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(105f, -78f),
                        font: GUIManager.Instance.AveriaSerifBold,
                        fontSize: 14,
                        color: GUIManager.Instance.ValheimBeige,
                        outline: true,
                        outlineColor: Color.black,
                        width: 350f,
                        height: 40f,
                        addContentSizeFitter: false);
                    // Hardcore toggle, enables generation with stars
                    hardModeToggle = GUIManager.Instance.CreateToggle(
                        parent: ChallengePanel.transform,
                        width: 40f,
                        height: 40f);
                    // Default the hardcore toggle to off.
                    hardModeToggle.GetComponent<Toggle>().isOn = false;
                    hardModeToggle.transform.localPosition = new Vector2(-85f, -74f); //Manually position the toggle where we want it
            } else {
                // Only destroy things we have previously created.
                if (hardmode_label != null) { Destroy(hardmode_label.gameObject); hardmode_label = null; }
                if (hardmode_desc != null) { Destroy(hardmode_desc.gameObject); hardmode_desc = null; }
                if (hardmode_reward_desc != null) { Destroy(hardmode_reward_desc.gameObject); hardmode_reward_desc = null; }
                if (hardModeToggle != null) { Destroy(hardModeToggle.gameObject); hardModeToggle = null; }
            }

            if (VFConfig.EnableBossModifier.Value)
            {
                // Only update the text if its not null, since the only other option here is we destroy the object
                    bossmode_label = GUIManager.Instance.CreateText(
                        text: Localization.instance.Localize("$shrine_boss_mode"),
                        parent: ChallengePanel.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(-140f, -122f),
                        font: GUIManager.Instance.AveriaSerifBold,
                        fontSize: 16,
                        color: GUIManager.Instance.ValheimBeige,
                        outline: true,
                        outlineColor: Color.black,
                        width: 200f,
                        height: 40f,
                        addContentSizeFitter: false);
                    bossmode_desc = GUIManager.Instance.CreateText(
                        text: Localization.instance.Localize("$shrine_boss_mode_description"),
                        parent: ChallengePanel.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(160f, -124f),
                        font: GUIManager.Instance.AveriaSerifBold,
                        fontSize: 14,
                        color: GUIManager.Instance.ValheimBeige,
                        outline: true,
                        outlineColor: Color.black,
                        width: 350f,
                        height: 40f,
                        addContentSizeFitter: false);
                    bossmode_reward_desc = GUIManager.Instance.CreateText(
                        text: Localization.instance.Localize("$shrine_boss_mode_reward"),
                        parent: ChallengePanel.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(-20f, -125f),
                        font: GUIManager.Instance.AveriaSerifBold,
                        fontSize: 14,
                        color: GUIManager.Instance.ValheimBeige,
                        outline: true,
                        outlineColor: Color.black,
                        width: 100f,
                        height: 40f,
                        addContentSizeFitter: false);
                    // Bossmode toggle
                    bossModeToggle = GUIManager.Instance.CreateToggle(
                        parent: ChallengePanel.transform,
                        width: 40f,
                        height: 40f);
                    // Default the Bossmode toggle to off.
                    bossModeToggle.GetComponent<Toggle>().isOn = false;
                    bossModeToggle.transform.localPosition = new Vector2(-85f, -120f); //Manually position the toggle where we want it
            } else {
                // Only destroy things we have previously created.
                if (bossmode_label != null) { Destroy(bossmode_label.gameObject); bossmode_label = null; }
                if (bossmode_desc != null) { Destroy(bossmode_desc.gameObject); bossmode_desc = null; }
                if (bossmode_reward_desc != null) { Destroy(bossmode_reward_desc.gameObject); bossmode_reward_desc = null; }
                if (bossModeToggle != null) { Destroy(bossModeToggle.gameObject); bossModeToggle = null; }
            }

            if (VFConfig.EnableSiegeModifer.Value)
            {
                    siegemode_label = GUIManager.Instance.CreateText(
                        text: Localization.instance.Localize("$shrine_siege_mode"),
                        parent: ChallengePanel.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(-140f, -172f),
                        font: GUIManager.Instance.AveriaSerifBold,
                        fontSize: 16,
                        color: GUIManager.Instance.ValheimBeige,
                        outline: true,
                        outlineColor: Color.black,
                        width: 200f,
                        height: 40f,
                        addContentSizeFitter: false);
                    siegemode_desc = GUIManager.Instance.CreateText(
                        text: Localization.instance.Localize("$shrine_siege_mode_description"),
                        parent: ChallengePanel.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(160f, -175f),
                        font: GUIManager.Instance.AveriaSerifBold,
                        fontSize: 14,
                        color: GUIManager.Instance.ValheimBeige,
                        outline: true,
                        outlineColor: Color.black,
                        width: 350f,
                        height: 40f,
                        addContentSizeFitter: false);
                    siegemode_reward_desc = GUIManager.Instance.CreateText(
                        text: Localization.instance.Localize("$shrine_siege_mode_reward"),
                        parent: ChallengePanel.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(105f, -175f),
                        font: GUIManager.Instance.AveriaSerifBold,
                        fontSize: 14,
                        color: GUIManager.Instance.ValheimBeige,
                        outline: true,
                        outlineColor: Color.black,
                        width: 350f,
                        height: 40f,
                        addContentSizeFitter: false);
                    // Siegemode toggle
                    siegeModeToggle = GUIManager.Instance.CreateToggle(
                        parent: ChallengePanel.transform,
                        width: 40f,
                        height: 40f);
                    // Default the Siegemode toggle to off.
                    siegeModeToggle.GetComponent<Toggle>().isOn = false;
                    siegeModeToggle.transform.localPosition = new Vector2(-85f, -170f); //Manually position the toggle where we want it
            } else {
                // Only destroy things we have previously created.
                if (siegemode_label != null) { Destroy(siegemode_label.gameObject); siegemode_label = null; }
                if (siegemode_desc != null) { Destroy(siegemode_desc.gameObject); siegemode_desc = null; }
                if (siegemode_reward_desc != null) { Destroy(siegemode_reward_desc.gameObject); siegemode_reward_desc = null; }
                if (siegeModeToggle != null) { Destroy(siegeModeToggle.gameObject); siegeModeToggle = null; }
            }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Shrine UI Dynamic Componets updated."); }
        }
    }
}
