using Jotunn.Managers;
using System;
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
        public static Int16 maxChallengeLevel = 5;

        // [SerializeField] private Transform uiRoot;

        public static bool IsPanelVisible()
        {
            return ChallengePanel.activeSelf;
        }

        public static void Init(AssetBundle EmbeddedResourceBundle)
        {
            GameObject prefab = EmbeddedResourceBundle.LoadAsset<GameObject>("Assets/Custom/UI/VFShrineUI.prefab");
            Jotunn.Logger.LogInfo("Loaded UI Prefab.");

            Jotunn.Logger.LogInfo("Set inactive gameobject.");

            // Built the challenge UI, since this is a static class
            // all of these values and UI componets will be used when instanciating the UI for the game below
            CreateChallengeUI();
            Jotunn.Logger.LogInfo("Instanciated UI.");
        }


        private static void UpdateLevelsAndRewards()
        {
            Int16 max_level = 5;
            // replace the available rewards before we rebuild it.
            availableRewards = new List<String> { "wood" };
            // These should actually track the global keys...
            if (Jotunn.Utils.GameConstants.GlobalKey.KilledEikthyr == "defeated_eikthyr") {
                max_level += 10;
                availableRewards.Add("fine wood");
            }
            if (Jotunn.Utils.GameConstants.GlobalKey.KilledElder == "defeated_gdking") { 
                max_level += 10;
                availableRewards.Add("copper");
                availableRewards.Add("tin");
            }
            if (Jotunn.Utils.GameConstants.GlobalKey.KilledBonemass == "defeated_bonemass") {
                max_level += 10;
                availableRewards.Add("iron");
            }
            if (Jotunn.Utils.GameConstants.GlobalKey.KilledModer == "defeated_dragon") { 
                max_level += 10;
                availableRewards.Add("silver");
            }
            if (Jotunn.Utils.GameConstants.GlobalKey.KilledYagluth == "defeated_goblinking") {
                max_level += 10;
                availableRewards.Add("darkmetal");
            }
            // If you have killed all of the tracked bosses, set the max possible level to 100.
            // if (max_level == 55) { max_level = 100; }

            // If the max challenge level is bigger than the level we determined we lower it to that
            if (max_level > maxChallengeLevel) { max_level = maxChallengeLevel; }

            // Empty out the current levels if it exists so we don't get duplicates
            currentLevels = new List<String> { };
            // Toss in all of the available levels
            for (int i = 1; i < max_level; i++)
            {
                currentLevels.Add($"{i}");
            }
        }

        private static void StartChallenge()
        {
            HideUI();
            String selected_reward = availableRewards[rewardSelector.GetComponent<Dropdown>().value];
            Int16 selected_level = (Int16)levelSelector.GetComponent<Dropdown>().value;
            Jotunn.Logger.LogInfo($"Shrine challenge started. Selected reward: {selected_reward}, selected level: {selected_level}");
            Levels.generateRandomWaveWithOptions(selected_level, Shrine.transform.position);
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
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(10f, -40f),
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
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(1.5f, -121f),
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
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(72f, -55f),
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
                width: 100f,
                height: 30f);
            rewardSelector.GetComponent<Dropdown>().AddOptions(availableRewards);
            // Rewards text
            GUIManager.Instance.CreateText(
                text: "Desired Reward",
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(60f, -200f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 350f,
                height: 40f,
                addContentSizeFitter: false);

            // create the wave selector dropdown
            levelSelector = GUIManager.Instance.CreateDropDown(
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(124f, -64f),
                fontSize: 16,
                width: 100f,
                height: 30f);
            levelSelector.GetComponent<Dropdown>().AddOptions(currentLevels);
            // Level selector text
            GUIManager.Instance.CreateText(
                text: "Wave Strength",
                parent: ChallengePanel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(60f, -250f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: 350f,
                height: 40f,
                addContentSizeFitter: false);


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
            // TODO: Might want to get this working
            cancelButton.onClick.AddListener(HideUI);
            // Add a listener to the button to close the panel and trigger the challenge scripts
            Button startButton = startButtonObj.GetComponent<Button>();
            startButton.onClick.AddListener(StartChallenge);

        }
    }
}
