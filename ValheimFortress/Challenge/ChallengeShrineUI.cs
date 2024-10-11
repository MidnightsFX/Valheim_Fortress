using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace ValheimFortress.Challenge
{
    internal class ChallengeShrineUI : GenericShrineUI
    {
        private ChallengeShrine Shrine;
        public override void Awake()
        {
            CreateStaticUIObjects();
            createCancelUI();
            UITriggersUpdatePanelSizeOnConfigChangeChallenge();
            Shrine = this.GetComponent<ChallengeShrine>();
            // Jotunn.Logger.LogInfo($"UI Attached to {Shrine.GetInstanceID()}");
        }

        public override void AddCreatureFlares()
        {
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Adding creature flares."); }
            HideCancelUI();
            Shrine.NotifyRemainingCreatures();
        }


        public override void CancelChallengeButtonClick()
        {
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Cancelling the active challenge."); }
            HideCancelUI();
            Shrine.CancelShrineRun();
        }

        public override void CleanupPortalsButtonClick()
        {
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Cleaning up portals."); }
            HideCancelUI();
            Shrine.CleanupOldPortals(3);
        }

        public override void StartChallenge()
        {
            // Jotunn.Logger.LogInfo($"Starting challenge at {Shrine.GetInstanceID()}");
            HideUI();
            String selected_reward = availableRewards[rewardSelector.GetComponent<Dropdown>().value];
            // Grab the first segment in the level definition, which is the index of the defined level
            string[] level_pieces = levelSelector.GetComponent<Dropdown>().options[levelSelector.GetComponent<Dropdown>().value].text.Split('-');
            string text_level = ValheimFortress.ReplaceWhitespace(level_pieces[0], "");
            short level_lookup_id = (short)(short.Parse(text_level) - 1);
            List<ChallengeLevelDefinition> clevels = Levels.GetChallengeLevelDefinitions();
            bool hard_mode = false;
            if (VFConfig.EnableHardModifier.Value) { hard_mode = hardModeToggle.GetComponent<Toggle>().isOn; }
            bool boss_mode = false;
            if (VFConfig.EnableBossModifier.Value) { boss_mode = bossModeToggle.GetComponent<Toggle>().isOn; }
            bool siege_mode = false;
            if (VFConfig.EnableSiegeModifer.Value) { siege_mode = siegeModeToggle.GetComponent<Toggle>().isOn; }
            // Start the coroutine that sends the warning text
            ChallengeLevelDefinition selectedLevel = clevels.ElementAt(level_lookup_id);
            UserInterfaceData.PreparePhase(selectedLevel, boss_mode, Shrine.gameObject);
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Shrine challenge. Selected reward: {selected_reward}, selected level ID {level_lookup_id} selected level index: {selectedLevel.levelIndex}"); }
            Shrine.SetLevel(level_lookup_id, selectedLevel.levelIndex);
            Shrine.SetReward(selected_reward);
            if (hard_mode) { Shrine.SetHardMode(); }
            if (boss_mode) { Shrine.SetBossMode(); }
            if (siege_mode) { Shrine.SetSiegeMode(); }
            // This call will trigger the shrine to start building out the wave and running the challenge
            Shrine.SetStartChallenge();
        }

        public override void TeleportCreatures()
        {
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Teleporting creatures to the shrine."); }
            HideCancelUI();
            Shrine.TeleportRemainingCreatures();
        }

        public override void DisplayUI()
        {
            CreateChallengeUI("challenge");
            ChallengePanel.SetActive(true);
            GUIManager.BlockInput(true);
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Enabled UI from Shrine object."); }
        }
    }
}
