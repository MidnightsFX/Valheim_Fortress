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
        static GameObject Shrine;

        static List<String> currentLevels = new List<String> {};
        static List<String> availableRewards = new List<String> { };
        public static GameObject levelSelector;
        public static GameObject rewardSelector;
        public static Int16 maxChallengeLevel = 5; // TODO: Make this configurable? make level generation dynamic?


        public static bool IsPanelVisible()
        {
            return ChallengePanel.activeSelf;
        }

        public static bool IsPanelVisible(GameObject obj)
        {
            return obj.activeSelf;
        }

        public static void Init(AssetBundle EmbeddedResourceBundle)
        {
            GameObject prefab = EmbeddedResourceBundle.LoadAsset<GameObject>("Assets/Custom/UI/VFShrineUI.prefab");
            Jotunn.Logger.LogInfo("Loaded UI Prefab.");
            // Built the challenge UI, since this is a static class
            // all of these values and UI componets will be used when instanciating the UI for the game below
            CreateChallengeUI();
            Jotunn.Logger.LogInfo("Instanciated UI.");
        }


        private static void UpdateLevelsAndRewards()
        {
            Int16 max_level = 5;
            Dictionary<String, Rewards.RewardEntry> possible_rewards = Rewards.GetResouceRewards();
            availableRewards = new List<String> { };
            var zs = ZoneSystem.instance;
            foreach (KeyValuePair<string, Rewards.RewardEntry> entry in possible_rewards)
            {
                if(entry.Value.required_boss == "None") { 
                    availableRewards.Add(entry.Key);
                    continue;
                }
                if (!zs) 
                { 
                    // We can only add items that do not require a global key check if there is no zone system.
                    continue; 
                }
                if(entry.Value.required_boss == "Eikythr" && zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledEikthyr)) {
                    availableRewards.Add(entry.Key);
                    continue;
                }
                if (entry.Value.required_boss == "TheElder" && zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledElder))
                {
                    availableRewards.Add(entry.Key);
                    continue;
                }
                if (entry.Value.required_boss == "BoneMass" && zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledBonemass))
                {
                    availableRewards.Add(entry.Key);
                    continue;
                }
                if (entry.Value.required_boss == "Moder" && zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledModer))
                {
                    availableRewards.Add(entry.Key);
                    continue;
                }
                if (entry.Value.required_boss == "Yagluth" && zs.GetGlobalKey(Jotunn.Utils.GameConstants.GlobalKey.KilledYagluth))
                {
                    availableRewards.Add(entry.Key);
                    continue;
                }
                if (entry.Value.required_boss == "TheQueen" && zs.GetGlobalKey("defeated_queen"))
                {
                    availableRewards.Add(entry.Key);
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
            }
            // If you have killed all of the tracked bosses, set the max possible level to 50.
            // if (max_level == 35) { max_level = 50; }

            // If the max challenge level is bigger than the level we determined we lower it to that
            if (max_level > maxChallengeLevel) { max_level = maxChallengeLevel; }

            // Empty out the current levels if it exists so we don't get duplicates
            currentLevels = new List<String> { };
            // Toss in all of the available levels
            for (int i = 1; i <= max_level; i++)
            {
                currentLevels.Add($"{i}");
            }
        }

        private static void StartChallenge()
        {
            HideUI();
            String selected_reward = availableRewards[rewardSelector.GetComponent<Dropdown>().value];
            // Trying to decide if we want to do this, which will use the wave dropdown text value as the entry, or if we should just do the index + 1, which is currently the same
            // TODO: this should be reworked when we switch from wave levels to another system
            // Int16 selected_level = Int16.Parse(levelSelector.GetComponent<Dropdown>().options[levelSelector.GetComponent<Dropdown>().value].text);
            // This takes the dropdowns index (+1 since its zero indexd), which means we always need the full number of levels populated, but that we can rename the levels however
            Int16 selected_level = (Int16)(levelSelector.GetComponent<Dropdown>().value + 1);
            Jotunn.Logger.LogInfo($"Shrine challenge. Selected reward: {selected_reward}, selected level: {selected_level}");
            if (Shrine.GetComponent<Shrine>().IsChallengeActive())
            {
                Jotunn.Logger.LogInfo("There is a challenge active, refusing to start another.");
            } else
            {
                // Start the coroutine that sends the warning text
                PreparePhase(selected_level, Shrine);

                Shrine.GetComponent<Shrine>().SetLevel(selected_level);
                Shrine.GetComponent<Shrine>().SetReward(selected_reward);
                Levels.generateRandomWaveWithOptions(selected_level, Shrine);
                Jotunn.Logger.LogInfo($"Challenge started. Level: {selected_level} Reward: {selected_reward}");
            }
        }

        private static void PreparePhase(Int16 selected_level, GameObject shrine)
        {
            String challenge_warning = "";
            switch (selected_level)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    challenge_warning = "The Meadow is no longer calm.";
                    break;
                case 5:
                    challenge_warning = "Eikthyr is angry.";
                    break;
                case 6:
                case 7:
                case 8:
                case 9:
                    challenge_warning = "The Forest grows restless.";
                    break;
                case 10:
                    challenge_warning = "The Forest walks with The Elder.";
                    break;
                case 11:
                case 12:
                case 13:
                case 14:
                    challenge_warning = "The Swamp keeps its dead.";
                    break;
                case 15:
                    challenge_warning = "Bonemass rises from the swamp.";
                    break;
                case 16:
                case 17:
                case 18:
                case 19:
                    challenge_warning = "The Mountain echos in howls.";
                    break;
                case 20:
                    challenge_warning = "Moder descends from the mountain.";
                    break;
                case 21:
                case 22:
                case 23:
                case 24:
                    challenge_warning = "The Plains sends its warbands.";
                    break;
                case 25:
                    challenge_warning = "Yagluth rises for revenge.";
                    break;
                case 26:
                case 27:
                case 28:
                case 29:
                    challenge_warning = "The Mists part to a skittering horde.";
                    break;
                case 30:
                    challenge_warning = "The Queen stalks her prey.";
                    break;
            }

            Player.m_localPlayer.Message(MessageHud.MessageType.Center, challenge_warning);
            shrine.GetComponent<Shrine>().EnablePortal();
            Jotunn.Logger.LogInfo("Activated Shrine portal & sent warning message");
        }


        public static void DisplayUI(GameObject shrine)
        {
            Shrine = shrine;
            ChallengePanel.SetActive(true);
            GUIManager.BlockInput(true);
            Jotunn.Logger.LogInfo("Enabled UI from Shrine object.");
        }

        public static void HideUI()
        {
            ChallengePanel.SetActive(false);
            GUIManager.BlockInput(false);
        }

        public static void ToggleUI()
        {
            // Switch the current state
            bool state = !ChallengePanel.activeSelf;
            // Set the active state of the panel
            ChallengePanel.SetActive(state);
            // Toggle input for the player and camera while displaying the GUI
            GUIManager.BlockInput(state);
        }

        private void Update()
        {

            if (IsPanelVisible() && (Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("Use") || ZInput.GetButtonDown("Inventory")))
            {
                Jotunn.Logger.LogInfo("UI detected close commands.");
                HideUI();
            }

        }

        public static void CreateChallengeUI()
        {
            // Always want to update the rewards and challenge levels
            UpdateLevelsAndRewards();
            // We don't want to recreate the panel if it already exists
            if (ChallengePanel)
            {
                return;
            }

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
                width: 450,
                height: 450,
                draggable: false);
            // This hides the panel immediately
            ChallengePanel.SetActive(false);

            // Create the title
            GUIManager.Instance.CreateText(
                text: "Face Odins Enemies",
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(50f, 175f),
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
                text: "Face an overwhelming challenge in return for a desired reward.\nMore rewards can be unlocked by defeating the world bosses.\nHigher levels give larger rewards.",
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(5f, 100f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 14,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 400f,
                height: 80f,
                addContentSizeFitter: false);
            GUIManager.Instance.CreateText(
                text: "You will face overwhelming odds.",
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(85f, 40f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 14,
                color: GUIManager.Instance.ValheimYellow,
                outline: true,
                outlineColor: Color.black,
                width: 400f,
                height: 40f,
                addContentSizeFitter: false);

            // Create the rewards selector dropdown
            rewardSelector = GUIManager.Instance.CreateDropDown(
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(120f, 6f),
                fontSize: 16,
                width: 150f,
                height: 30f);
            rewardSelector.GetComponent<Dropdown>().AddOptions(availableRewards);
            // Rewards text
            GUIManager.Instance.CreateText(
                text: "Desired Reward",
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(-35f, 10f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 200f,
                height: 40f,
                addContentSizeFitter: true);

            // create the wave selector dropdown
            levelSelector = GUIManager.Instance.CreateDropDown(
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(120f, -64f),
                fontSize: 16,
                width: 150f,
                height: 30f);
            levelSelector.GetComponent<Dropdown>().AddOptions(currentLevels);
            // Level selector text
            GUIManager.Instance.CreateText(
                text: "Wave Strength",
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(-35f, -65f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 200f,
                height: 40f,
                addContentSizeFitter: true);


            // Create the start button object
            GameObject startButtonObj = GUIManager.Instance.CreateButton(
                text: "To Valhalla!",
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0f, -160f),
                width: 150f,
                height: 60f);
            startButtonObj.SetActive(true);

            // Create the close button object
            GameObject cancelButtonObj = GUIManager.Instance.CreateButton(
                text: "x",
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(200f, 200f),
                width: 40f,
                height: 40f);
            cancelButtonObj.SetActive(true);

            // Add a listener to the button to close the panel again
            Button cancelButton = cancelButtonObj.GetComponent<Button>();
            cancelButton.onClick.AddListener(HideUI);
            // Add a listener to the button to close the panel and trigger the challenge scripts
            Button startButton = startButtonObj.GetComponent<Button>();
            startButton.onClick.AddListener(StartChallenge);

        }
    }
}
