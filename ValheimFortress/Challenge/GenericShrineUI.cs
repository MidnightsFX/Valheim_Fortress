using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;

namespace ValheimFortress.Challenge
{
    abstract class GenericShrineUI :MonoBehaviour
    {
        protected GameObject ChallengePanel;
        protected GameObject CancelPanel;
        // this is unset until the shrine building calls for the UI, in which case it is then set
        protected GenericShrine Shrine;

        protected List<String> currentLevels = new List<string> { };
        protected List<String> availableRewards = new List<string> { };
        public GameObject levelSelector;
        public GameObject rewardSelector;
        public GameObject hardModeToggle;
        public GameObject bossModeToggle;
        public GameObject siegeModeToggle;
        public GameObject estimate_text;
        public short estimatedRewards = 0;
        public string estimatedRewardName = "";

        protected GameObject estimate_symbol;
        protected GameObject hardmode_label;
        protected GameObject hardmode_desc;
        protected GameObject hardmode_reward_desc;
        protected GameObject bossmode_label;
        protected GameObject bossmode_desc;
        protected GameObject bossmode_reward_desc;
        protected GameObject siegemode_label;
        protected GameObject siegemode_desc;
        protected GameObject siegemode_reward_desc;

        protected GameObject cancelButtonGO;
        protected GameObject startChallengeButtonGO;

        protected GameObject AdminMenuButtonGO;
        protected GameObject AdminMenuPanel;
        protected GameObject adminEntryField;

        protected bool cleanupPortals = true;

        private short selected_level_index = 0;
        public bool current_hard_mode = false;
        public bool current_boss_mode = false;
        public bool current_siege_mode = false;
        public enum adminFunctions {
            filtername
        }

        public abstract void Awake();

        public bool IsPanelVisible()
        {
            // Challenge panel doesn't exist yet, so no its not visible
            if (ChallengePanel == null) { return false; }

            return ChallengePanel.activeSelf;
        }

        public bool IsShrineOrCancelUIVisible()
        {   if (ChallengePanel.activeSelf) { return true; }
            if (CancelPanel.activeSelf) { return true;  }
            // If neither panel is active, we return false
            return false;
        }

        public void Update()
        {
            if (IsPanelVisible())
            {
                // Skip the whole thing if we don't need to estimate rewards
                if (VFConfig.EnableRewardsEstimate.Value == false) { return; }
                bool value_changed = false;
                string rewardName = availableRewards[rewardSelector.GetComponent<Dropdown>().value];
                // Grab the first segment in the level definition, which is the index of the defined level
                string[] level_pieces = levelSelector.GetComponent<Dropdown>().options[levelSelector.GetComponent<Dropdown>().value].text.Split('-');
                string text_level = ValheimFortress.ReplaceWhitespace(level_pieces[0], "");
                short level_definition_lookup_index = (short)(short.Parse(text_level) - 1);

                // if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Looking up level definition with index of text-{text_level} (1 based) int-{level_definition_lookup_index} (zero based)"); }
                List<ChallengeLevelDefinition> clevels = Levels.GetChallengeLevelDefinitions();
                short level_difficulty = clevels.ElementAt(level_definition_lookup_index).levelIndex;
                bool hardmode_status = false;
                bool bossmode_status = false;
                bool siegemode_status = false;
                if (rewardName != estimatedRewardName || selected_level_index != level_definition_lookup_index) { value_changed = true; }
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
                selected_level_index = level_definition_lookup_index;
                estimatedRewards = RewardsData.DetermineRewardAmount(estimatedRewardName, level_difficulty, hardmode_status, bossmode_status, siegemode_status);
                estimate_text.GetComponent<Text>().text = $"{estimatedRewards}";
            }
        }

        public abstract void StartChallenge();

        public abstract void DisplayUI();

        public void HideUI()
        {
            ChallengePanel.SetActive(false);
            HideAdminUI();
            GUIManager.BlockInput(false);
        }

        public void ShowAdminUI()
        {
            CreateAdminSelectionUI();
            AdminMenuPanel.SetActive(true);
        }

        public void HideAdminUI()
        {
            if (AdminMenuPanel) {
                AdminMenuPanel.SetActive(false);
            }
        }

        public void UpdatePossibleRewards(Dropdown reward_dropdown, Dropdown level_dropdown)
        {
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Retrieving level from level dropdown value: {level_dropdown.value}"); }
            string[] level_pieces = level_dropdown.options[level_dropdown.value].text.Split('-');
            string text_level = ValheimFortress.ReplaceWhitespace(level_pieces[0], "");
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Leveltext: {text_level}"); }
            short level_definition_lookup_index = (short)(short.Parse(text_level) - 1);
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"level index: {level_definition_lookup_index}"); }
            List<ChallengeLevelDefinition> clevels = Levels.GetChallengeLevelDefinitions();
            availableRewards = UserInterfaceData.UpdateRewards(clevels.ElementAt(level_definition_lookup_index));
            reward_dropdown.ClearOptions();
            reward_dropdown.AddOptions(availableRewards);
        }

        public void UITriggersUpdatePanelSizeOnConfigChangeArena()
        {
            VFConfig.EnableBossModifier.SettingChanged += UpdatePanelUIOnSettingsChangedArena;
            VFConfig.EnableHardModifier.SettingChanged += UpdatePanelUIOnSettingsChangedArena;
            VFConfig.EnableSiegeModifer.SettingChanged += UpdatePanelUIOnSettingsChangedArena;
        }

        public void UITriggersUpdatePanelSizeOnConfigChangeChallenge()
        {
            VFConfig.EnableBossModifier.SettingChanged += UpdatePanelUIOnSettingsChangedChallenge;
            VFConfig.EnableHardModifier.SettingChanged += UpdatePanelUIOnSettingsChangedChallenge;
            VFConfig.EnableSiegeModifer.SettingChanged += UpdatePanelUIOnSettingsChangedChallenge;
        }

        public void UpdatePanelUIOnSettingsChangedArena(object sender, EventArgs e)
        {
            CreateStaticUIObjects();
            CreateChallengeUI("arena");
        }

        public void UpdatePanelUIOnSettingsChangedChallenge(object sender, EventArgs e)
        {
            CreateStaticUIObjects();
            CreateChallengeUI("challenge");
        }

        public void CreateStaticUIObjects()
        {
            // This was supposed to allow this is not need to be recreated much/at all
            // However, there are a few edge cases that cause this to be null
            // if (ChallengePanel != null) { return; }

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
            if (VFConfig.EnableBossModifier.Value == false && VFConfig.EnableHardModifier.Value == false && VFConfig.EnableSiegeModifer.Value == false)
            {
                // Create the panel object
                ChallengePanel = GUIManager.Instance.CreateWoodpanel(
                    parent: GUIManager.CustomGUIFront.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(0, 0),
                    width: 600,
                    height: 400,
                    draggable: true);

                // Create the title
                GUIManager.Instance.CreateText(
                    text: Localization.instance.Localize("$shrine_header"),
                    parent: ChallengePanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(60f, 165f),
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
                    position: new Vector2(45f, 100f),
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
                    position: new Vector2(85f, 60f),
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
                    position: new Vector2(-60f, -60f),
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
                    position: new Vector2(-60f, 5f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 16,
                    color: GUIManager.Instance.ValheimBeige,
                    outline: true,
                    outlineColor: Color.black,
                    width: 300f,
                    height: 40f,
                    addContentSizeFitter: false);
            } else {
                // Create the panel object
                ChallengePanel = GUIManager.Instance.CreateWoodpanel(
                    parent: GUIManager.CustomGUIFront.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(0, 0),
                    width: 600,
                    height: 600,
                    draggable: true);

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
                    position: new Vector2(-60f, -5f),
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
                    position: new Vector2(-60f, 50f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 16,
                    color: GUIManager.Instance.ValheimBeige,
                    outline: true,
                    outlineColor: Color.black,
                    width: 300f,
                    height: 40f,
                    addContentSizeFitter: false);
            }
            // This hides the panel immediately
            ChallengePanel.SetActive(false);


            if (VFConfig.EnableBossModifier.Value || VFConfig.EnableHardModifier.Value || VFConfig.EnableSiegeModifer.Value)
            {
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
                if (startChallengeButtonGO != null) { Destroy(startChallengeButtonGO.gameObject); startChallengeButtonGO = null; }
                // Create the start button object
                startChallengeButtonGO = GUIManager.Instance.CreateButton(
                    text: Localization.instance.Localize("$shrine_confirm"),
                    parent: ChallengePanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(0f, -224f),
                    width: 150f,
                    height: 60f);
                startChallengeButtonGO.SetActive(true);
                if (cancelButtonGO != null) { Destroy(cancelButtonGO.gameObject); cancelButtonGO = null; }
                // Create the close button object
                cancelButtonGO = GUIManager.Instance.CreateButton(
                    text: Localization.instance.Localize("$shrine_cancel"),
                    parent: ChallengePanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(260f, 260f),
                    width: 40f,
                    height: 40f);
                cancelButtonGO.SetActive(true);

                // Create the admin button object
                AdminMenuButtonGO = GUIManager.Instance.CreateButton(
                    text: Localization.instance.Localize("$shrine_admin_menu_button"),
                    parent: ChallengePanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(220f, 260f),
                    width: 40f,
                    height: 40f);
                AdminMenuButtonGO.SetActive(true);

            } else {
                if (startChallengeButtonGO != null) { Destroy(startChallengeButtonGO.gameObject); startChallengeButtonGO = null; }
                // Create the start button object
                startChallengeButtonGO = GUIManager.Instance.CreateButton(
                    text: Localization.instance.Localize("$shrine_confirm"),
                    parent: ChallengePanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(0f, -125f),
                    width: 150f,
                    height: 60f);
                startChallengeButtonGO.SetActive(true);
                if (cancelButtonGO != null) { Destroy(cancelButtonGO.gameObject); cancelButtonGO = null; }
                // Create the close button object
                cancelButtonGO = GUIManager.Instance.CreateButton(
                    text: Localization.instance.Localize("$shrine_cancel"),
                    parent: ChallengePanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(260f, 165f),
                    width: 40f,
                    height: 40f);
                cancelButtonGO.SetActive(true);

                if (SynchronizationManager.Instance.PlayerIsAdmin)
                {
                    // Create the admin button object
                    AdminMenuButtonGO = GUIManager.Instance.CreateButton(
                        text: Localization.instance.Localize("$shrine_admin_menu_button"),
                        parent: ChallengePanel.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(218f, 165f),
                        width: 40f,
                        height: 40f);
                    AdminMenuButtonGO.SetActive(true);
                }
            }

            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Shrine UI Created."); }
            // Add a listener to the button to close the panel again
            Button cancelButton = cancelButtonGO.GetComponent<Button>();
            cancelButton.onClick.AddListener(HideUI);
            // Add a listener to the button to admin the panel
            Button adminButton = AdminMenuButtonGO.GetComponent<Button>();
            adminButton.onClick.AddListener(ShowAdminUI);
            // Add a listener to the button to close the panel and trigger the challenge scripts
            Button startChallengeButton = startChallengeButtonGO.GetComponent<Button>();
            startChallengeButton.onClick.AddListener(StartChallenge);
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Adding UI Listeners"); }
        }

        public void CreateChallengeUI(string shrine_type)
        {
            // Always want to update the rewards and challenge levels
            currentLevels = UserInterfaceData.UpdateLevels(shrine_type, Shrine.adminLevelLimits.Get());
            // We specifically want to be able to completely rebuild the UI when this is called again to update the levels and rewards dynamically

            if (VFConfig.EnableBossModifier.Value == false && VFConfig.EnableHardModifier.Value == false && VFConfig.EnableSiegeModifer.Value == false)
            {
                if (levelSelector != null) { Destroy(levelSelector.gameObject); levelSelector = null; }
                // create the wave selector dropdown
                levelSelector = GUIManager.Instance.CreateDropDown(
                    parent: ChallengePanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(45f, 10f),
                    fontSize: 16,
                    width: 200f,
                    height: 40f);
                levelSelector.GetComponent<Dropdown>().AddOptions(currentLevels);

                // Always want to update the rewards and challenge levels
                // The first update is always for level 1 because that is the default that will be selected
                availableRewards = UserInterfaceData.UpdateRewardsInitially(currentLevels);

                if (rewardSelector != null) { Destroy(rewardSelector.gameObject); rewardSelector = null; }
                // Create the rewards selector dropdown
                rewardSelector = GUIManager.Instance.CreateDropDown(
                    parent: ChallengePanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(45f, -50f),
                    fontSize: 16,
                    width: 200f,
                    height: 40f);
                rewardSelector.GetComponent<Dropdown>().AddOptions(availableRewards);

            } else  {
                if (levelSelector != null) { Destroy(levelSelector.gameObject); levelSelector = null; }
                // create the wave selector dropdown
                levelSelector = GUIManager.Instance.CreateDropDown(
                    parent: ChallengePanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(45f, 60f),

                    fontSize: 16,
                    width: 200f,
                    height: 40f);
                levelSelector.GetComponent<Dropdown>().AddOptions(currentLevels);

                // Always want to update the rewards and challenge levels
                // The first update is always for level 1 because that is the default that will be selected
                availableRewards = UserInterfaceData.UpdateRewardsInitially(currentLevels);

                if (rewardSelector != null) { Destroy(rewardSelector.gameObject); rewardSelector = null; }
                // Create the rewards selector dropdown
                rewardSelector = GUIManager.Instance.CreateDropDown(
                    parent: ChallengePanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(45f, 5f),
                    fontSize: 16,
                    width: 200f,
                    height: 40f);
                rewardSelector.GetComponent<Dropdown>().AddOptions(availableRewards);
            }

            // Add a listener which will update the available rewards choices when we change the selected level
            levelSelector.GetComponent<Dropdown>().onValueChanged.AddListener(delegate { UpdatePossibleRewards(rewardSelector.GetComponent<Dropdown>(), levelSelector.GetComponent<Dropdown>()); });


            if (VFConfig.EnableRewardsEstimate.Value)
            {
                
                if (VFConfig.EnableBossModifier.Value == false && VFConfig.EnableHardModifier.Value == false && VFConfig.EnableSiegeModifer.Value == false)
                {
                    if (estimate_symbol != null) { Destroy(estimate_symbol.gameObject); estimate_symbol = null; }
                    // Shrine reward estimate
                    estimate_symbol = GUIManager.Instance.CreateText(
                        text: Localization.instance.Localize("$shrine_reward_estimate"),
                        parent: ChallengePanel.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(180f, -57f),
                        font: GUIManager.Instance.AveriaSerifBold,
                        fontSize: 16,
                        color: GUIManager.Instance.ValheimBeige,
                        outline: true,
                        outlineColor: Color.black,
                        width: 40f,
                        height: 40f,
                        addContentSizeFitter: false);
                    // Destroy the old text before rendering new text if it has been updated
                    if (estimate_text != null) { Destroy(estimate_text); estimate_text = null; }
                    estimate_text = GUIManager.Instance.CreateText(
                        text: $"{estimatedRewards}",
                        parent: ChallengePanel.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(220f, -57f),
                        font: GUIManager.Instance.AveriaSerifBold,
                        fontSize: 16,
                        color: GUIManager.Instance.ValheimBeige,
                        outline: true,
                        outlineColor: Color.black,
                        width: 100f,
                        height: 40f,
                        addContentSizeFitter: false);
                } else {
                    if (estimate_symbol != null) { Destroy(estimate_symbol.gameObject); estimate_symbol = null; }
                    // Shrine reward estimate
                    estimate_symbol = GUIManager.Instance.CreateText(
                        text: Localization.instance.Localize("$shrine_reward_estimate"),
                        parent: ChallengePanel.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(180f, -3f),
                        font: GUIManager.Instance.AveriaSerifBold,
                        fontSize: 16,
                        color: GUIManager.Instance.ValheimBeige,
                        outline: true,
                        outlineColor: Color.black,
                        width: 40f,
                        height: 40f,
                        addContentSizeFitter: false);
                    // Destroy the old text before rendering new text if it has been updated
                    if (estimate_text != null) { Destroy(estimate_text); estimate_text = null; }
                    estimate_text = GUIManager.Instance.CreateText(
                        text: $"{estimatedRewards}",
                        parent: ChallengePanel.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(220f, -3f),
                        font: GUIManager.Instance.AveriaSerifBold,
                        fontSize: 16,
                        color: GUIManager.Instance.ValheimBeige,
                        outline: true,
                        outlineColor: Color.black,
                        width: 100f,
                        height: 40f,
                        addContentSizeFitter: false);
                }
            }
            else
            {
                // Only destroy things we have previously created.
                if (estimate_symbol != null) { Destroy(estimate_symbol.gameObject); estimate_symbol = null; }
                if (estimate_text != null) { Destroy(estimate_text.gameObject); estimate_text = null; }
            }

            if (VFConfig.EnableHardModifier.Value)
            {
                if (hardmode_label != null) { Destroy(hardmode_label.gameObject); hardmode_label = null; }
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
                if (hardmode_desc != null) { Destroy(hardmode_desc.gameObject); hardmode_desc = null; }
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
                if (hardmode_reward_desc != null) { Destroy(hardmode_reward_desc.gameObject); hardmode_reward_desc = null; }
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
                if (hardModeToggle != null) { Destroy(hardModeToggle.gameObject); hardModeToggle = null; }
                // Hardcore toggle, enables generation with stars
                hardModeToggle = GUIManager.Instance.CreateToggle(
                    parent: ChallengePanel.transform,
                    width: 40f,
                    height: 40f);
                // Default the hardcore toggle to off.
                hardModeToggle.GetComponent<Toggle>().isOn = false;
                hardModeToggle.transform.localPosition = new Vector2(-85f, -74f); //Manually position the toggle where we want it
            }
            else
            {
                // Only destroy things we have previously created.
                if (hardmode_label != null) { Destroy(hardmode_label.gameObject); hardmode_label = null; }
                if (hardmode_desc != null) { Destroy(hardmode_desc.gameObject); hardmode_desc = null; }
                if (hardmode_reward_desc != null) { Destroy(hardmode_reward_desc.gameObject); hardmode_reward_desc = null; }
                if (hardModeToggle != null) { Destroy(hardModeToggle.gameObject); hardModeToggle = null; }
            }

            if (VFConfig.EnableBossModifier.Value)
            {
                if (bossmode_label != null) { Destroy(bossmode_label.gameObject); bossmode_label = null; }
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
                if (bossmode_desc != null) { Destroy(bossmode_desc.gameObject); bossmode_desc = null; }
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
                if (bossmode_reward_desc != null) { Destroy(bossmode_reward_desc.gameObject); bossmode_reward_desc = null; }
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
                if (bossModeToggle != null) { Destroy(bossModeToggle.gameObject); bossModeToggle = null; }
                bossModeToggle = GUIManager.Instance.CreateToggle(
                    parent: ChallengePanel.transform,
                    width: 40f,
                    height: 40f);
                // Default the Bossmode toggle to off.
                bossModeToggle.GetComponent<Toggle>().isOn = false;
                bossModeToggle.transform.localPosition = new Vector2(-85f, -120f); //Manually position the toggle where we want it
            }
            else
            {
                // Only destroy things we have previously created.
                if (bossmode_label != null) { Destroy(bossmode_label.gameObject); bossmode_label = null; }
                if (bossmode_desc != null) { Destroy(bossmode_desc.gameObject); bossmode_desc = null; }
                if (bossmode_reward_desc != null) { Destroy(bossmode_reward_desc.gameObject); bossmode_reward_desc = null; }
                if (bossModeToggle != null) { Destroy(bossModeToggle.gameObject); bossModeToggle = null; }
            }

            if (VFConfig.EnableSiegeModifer.Value)
            {
                if (siegemode_label != null) { Destroy(siegemode_label.gameObject); siegemode_label = null; }
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
                if (siegemode_desc != null) { Destroy(siegemode_desc.gameObject); siegemode_desc = null; }
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
                if (siegemode_reward_desc != null) { Destroy(siegemode_reward_desc.gameObject); siegemode_reward_desc = null; }
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
                if (siegeModeToggle != null) { Destroy(siegeModeToggle.gameObject); siegeModeToggle = null; }
                siegeModeToggle = GUIManager.Instance.CreateToggle(
                    parent: ChallengePanel.transform,
                    width: 40f,
                    height: 40f);
                // Default the Siegemode toggle to off.
                siegeModeToggle.GetComponent<Toggle>().isOn = false;
                siegeModeToggle.transform.localPosition = new Vector2(-85f, -170f); //Manually position the toggle where we want it
            }
            else
            {
                // Only destroy things we have previously created.
                if (siegemode_label != null) { Destroy(siegemode_label.gameObject); siegemode_label = null; }
                if (siegemode_desc != null) { Destroy(siegemode_desc.gameObject); siegemode_desc = null; }
                if (siegemode_reward_desc != null) { Destroy(siegemode_reward_desc.gameObject); siegemode_reward_desc = null; }
                if (siegeModeToggle != null) { Destroy(siegeModeToggle.gameObject); siegeModeToggle = null; }
            }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Shrine UI Dynamic Componets updated."); }
        }

        public void createCancelUI()
        {
            if (CancelPanel != null) { return; }
            // Create parent panel object
            CancelPanel = GUIManager.Instance.CreateWoodpanel(
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0, 0),
                width: 400,
                height: 400,
                draggable: true);
            // This hides the panel immediately
            CancelPanel.SetActive(false);

            // Create the close button object
            GameObject cancelCancelButtonObj = GUIManager.Instance.CreateButton(
                text: Localization.instance.Localize("$shrine_cancel"),
                parent: CancelPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(175f, 175f),
                width: 40f,
                height: 40f);
            // Create the title
            GUIManager.Instance.CreateText(
                text: Localization.instance.Localize("$shrine_in_progress_header"),
                parent: CancelPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(20f, 165f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 30,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 400f,
                height: 40f,
                addContentSizeFitter: false);
            GUIManager.Instance.CreateText(
                text: Localization.instance.Localize("$shrine_cancel_enemies_remaining"),
                parent: CancelPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(-15f, 115f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 200f,
                height: 40f,
            addContentSizeFitter: false);

            // Create cancel challenge button
            GameObject cancelChallengeObj = GUIManager.Instance.CreateButton(
                text: Localization.instance.Localize("$shrine_cancel_challenge"),
                parent: CancelPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(110f, -150f),
                width: 100f,
                height: 60f);
            GUIManager.Instance.CreateText(
                text: Localization.instance.Localize("$shrine_cancel_challenge_desc"),
                parent: CancelPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(-55f, -150f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 200f,
                height: 40f,
            addContentSizeFitter: false);

            if (cleanupPortals)
            {
                // Create cleanup portals button
                GameObject cleanupPortalsObj = GUIManager.Instance.CreateButton(
                    text: Localization.instance.Localize("$shrine_cleanup_portals"),
                    parent: CancelPanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(110f, -75f),
                    width: 100f,
                    height: 40f);
                GUIManager.Instance.CreateText(
                    text: Localization.instance.Localize("$shrine_cleanup_portals_desc"),
                    parent: CancelPanel.transform,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    position: new Vector2(-55f, -75f),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 16,
                    color: GUIManager.Instance.ValheimBeige,
                    outline: true,
                    outlineColor: Color.black,
                    width: 200f,
                    height: 40f,
                addContentSizeFitter: false);
                Button cleanupPortalsButton = cleanupPortalsObj.GetComponent<Button>();
                cleanupPortalsButton.onClick.AddListener(CleanupPortalsButtonClick);
            }


            // Create enable flares button
            GameObject enableFlaresObj = GUIManager.Instance.CreateButton(
                text: Localization.instance.Localize("$shrine_enable_flares"),
                parent: CancelPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(110f, 0f),
                width: 100f,
                height: 40f);
            GUIManager.Instance.CreateText(
                text: Localization.instance.Localize("$shrine_enable_flares_desc"),
                parent: CancelPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(-55f, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 200f,
                height: 40f,
            addContentSizeFitter: false);

            // Create teleport creatures button
            GameObject teleportCreaturesObj = GUIManager.Instance.CreateButton(
                text: Localization.instance.Localize("$shrine_teleport_creatures"),
                parent: CancelPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(110f, 75f),
                width: 100f,
                height: 40f);
            GUIManager.Instance.CreateText(
                text: Localization.instance.Localize("$shrine_teleport_creatures_desc"),
                parent: CancelPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(-55f, 75f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 200f,
                height: 40f,
            addContentSizeFitter: false);

            Button cancelCancelButton = cancelCancelButtonObj.GetComponent<Button>();
            cancelCancelButton.onClick.AddListener(HideCancelUI);
            Button cancelChallengeButton = cancelChallengeObj.GetComponent<Button>();
            cancelChallengeButton.onClick.AddListener(CancelChallengeButtonClick);
            Button enableFlaresButton = enableFlaresObj.GetComponent<Button>();
            enableFlaresButton.onClick.AddListener(AddCreatureFlares);
            Button teleportCreaturesButton = teleportCreaturesObj.GetComponent<Button>();
            teleportCreaturesButton.onClick.AddListener(TeleportCreatures);
        }

        public void CreateAdminSelectionUI()
        {
            if (AdminMenuPanel != null) { return; }

            // Create the panel object
            AdminMenuPanel = GUIManager.Instance.CreateWoodpanel(
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(482, 0),
                width: 380,
                height: 400,
                draggable: true);

            // This hides the panel immediately
            AdminMenuPanel.SetActive(false);

            // Create the close button object
            GameObject adminCancelButtonGo = GUIManager.Instance.CreateButton(
                text: Localization.instance.Localize("$shrine_cancel"),
                parent: AdminMenuPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(160f, 170f),
                width: 40f,
                height: 40f);
            // Create the title
            GUIManager.Instance.CreateText(
                text: Localization.instance.Localize("$shrine_admin_menu"),
                parent: AdminMenuPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(100f, 165f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 30,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 400f,
                height: 40f,
                addContentSizeFitter: false);
            // Create the description
            GUIManager.Instance.CreateText(
                text: Localization.instance.Localize("$shrine_admin_description"),
                parent: AdminMenuPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(20f, 120f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 300f,
                height: 60f,
                addContentSizeFitter: false);
            // Admin input field
            adminEntryField = GUIManager.Instance.CreateInputField(
                parent: AdminMenuPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0f, -10f),
                contentType: InputField.ContentType.Standard,
                placeholderText: Shrine.adminConfigData.Get(),
                fontSize: 16,
                width: 300f,
                height: 200f);
            // Create the save button
            GameObject adminsaveButtonGo = GUIManager.Instance.CreateButton(
                text: Localization.instance.Localize("$shrine_admin_save"),
                parent: AdminMenuPanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0f, -150f),
                width: 120f,
                height: 40f);

            Button adminCancelButton = adminCancelButtonGo.GetComponent<Button>();
            adminCancelButton.onClick.AddListener(HideAdminUI);

            Button adminSaveButton = adminsaveButtonGo.GetComponent<Button>();
            adminSaveButton.onClick.AddListener(SetupAdminFunctions);
        }

        public void SetupAdminFunctions()
        {
            string input = adminEntryField.gameObject.GetComponent<InputField>().text;
            if (input != null) {
                SaveAdminCommandsToShrine(input);
                string[] input_function_and_values = input.Split(';');
                foreach (string function_and_value in input_function_and_values) {
                    string[] functions_and_values = function_and_value.Split(':');
                    string operation = functions_and_values[0];
                    if (adminFunctions.filtername.ToString() == operation.ToLower())
                    {
                        string values = functions_and_values[1];
                        ApplyAdminFunctions(adminFunctions.filtername, values.Split(',').ToList<string>());
                    } else {
                        Jotunn.Logger.LogWarning($"{operation.ToLower()} Admin function not matched, should be one of ({adminFunctions.filtername.ToString()})");
                    }
                }
            }
        }

        public abstract void ApplyAdminFunctions(adminFunctions function, List<string> values);

        public void SaveAdminCommandsToShrine(string value)
        {
            Shrine.adminConfigData.ForceSet(value);
        }

        public abstract void CleanupPortalsButtonClick();


        public abstract void CancelChallengeButtonClick();

        public void HideCancelUI()
        {
            // Nothing to do if the UI doesn't exist yet
            if (CancelPanel == null) { return; }
            CancelPanel.SetActive(false);
            GUIManager.BlockInput(false);
        }

        public void DisplayCancelUI()
        {
            createCancelUI();
            CancelPanel.SetActive(true);
            GUIManager.BlockInput(true);
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Enabled Cancel UI from Shrine object."); }
        }

        public abstract void AddCreatureFlares();

        public abstract void TeleportCreatures();
    }
}
