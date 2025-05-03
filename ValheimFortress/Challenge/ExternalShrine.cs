using Jotunn.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;
using ValheimFortress.Common;

namespace ValheimFortress.Challenge
{
    internal class ExternalShrine : GenericShrine
    {
        private IntZNetProperty ShrineSelectedLevel;
        private ExternalShrineDefinition extShrineDef;
        private List<ExternalShrineLevel> levels;
        private static new GameObject[] shrine_spawnpoint;
        
        public override void Awake()
        { }

        private void setupLateNetworking()
        {
            zNetView = GetComponentInParent<ZNetView>();
            Jotunn.Logger.LogDebug($"Looking for a parented znet view {zNetView}.");

            spawned_creatures = new IntZNetProperty("spawned_creatures", zNetView, 0);
            hard_mode = new BoolZNetProperty("shrine_hard_mode", zNetView, false);
            boss_mode = new BoolZNetProperty("shrine_boss_mode", zNetView, false);
            siege_mode = new BoolZNetProperty("shrine_siege_mode", zNetView, false);
            challenge_active = new BoolZNetProperty("shrine_challenge_active", zNetView, false);
            start_challenge = new BoolZNetProperty("shrine_start_challenge", zNetView, false);
            selected_level = new IntZNetProperty("shrine_selected_level", zNetView, 0);
            selected_reward = new StringZNetProperty("shrine_selected_reward", zNetView, "coins");
            portal_disabled = new BoolZNetProperty("end_of_challenge", zNetView, false);
            should_add_creature_beacons = new BoolZNetProperty("should_add_creature_beacons", zNetView, false);
            currentPhase = new IntZNetProperty("shrine_current_phase", zNetView, 0);
            wave_definition_ready = new BoolZNetProperty("wave_definition_ready", zNetView, false);
            spawn_locations_ready = new BoolZNetProperty("spawn_locations_ready", zNetView, false);
            force_next_phase = new BoolZNetProperty("force_next_phase", zNetView, false);
            remote_spawn_locations = new ArrayVectorZNetProperty("remote_spawn_locations", zNetView, null);

            alive_creature_list = new DictionaryZNetProperty("alive_creature_list", zNetView, new Dictionary<String, short>() { });

            ShrineSelectedLevel = new IntZNetProperty("shrine_selected_level", zNetView, 0);

            WaveDefinitionRPC = NetworkManager.Instance.AddRPC("levelsyaml_rpc", VFConfig.OnServerRecieveConfigs, OnClientReceivePhaseConfigs);
            SynchronizationManager.Instance.AddInitialSynchronization(WaveDefinitionRPC, SendPhaseConfigs);
        }

        public override string GetHoverName()
        {
            return Localization.instance.Localize(extShrineDef.NameLocalization);
        }

        public override string GetHoverText()
        {
            string text = $"[<color=yellow><b>$KEY_Use</b></color>] {extShrineDef.NameLocalization}";
            return Localization.instance.Localize(text);
        }

        public override bool Interact(Humanoid user, bool hold, bool alt) {
            if (extShrineDef.RequiresSacrifice) {
                user.Message(MessageHud.MessageType.Center, Localization.instance.Localize(extShrineDef.RequestLocalization));
            } else {
                if (challenge_active.Get() == false) {
                    if (extShrineDef.LevelSelection == LevelSelectionStrategy.Random) {
                        RandomlySelectLevel();
                    }
                }
            }
            return false;
        }

        public void SetShrineLevel(int level)
        {
            ShrineSelectedLevel.ForceSet(level);
        }

        private void RandomlySelectLevel()
        {
            int sel_level = UnityEngine.Random.Range(0, levels.Count);
            siege_mode.ForceSet(levels[sel_level].levelModifiers.SiegeMode);
            hard_mode.ForceSet(levels[sel_level].levelModifiers.HardMode);
            selected_level.ForceSet(sel_level);
            start_challenge.ForceSet(true);
        }

        public override void StartChallengeMode()
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
