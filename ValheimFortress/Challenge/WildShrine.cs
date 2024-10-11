using Jotunn.Extensions;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ValheimFortress.Challenge
{
    internal class WildShrine : GenericShrine
    {
        private static int log_slower = 0;
        private WildShrineConfiguration wildShrineConfiguration;
        private static new GameObject shrine_spawnpoint;
        public override void Awake()
        {
            //if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Looking for znet view."); }
            //// Because the netview is attached to the location here not the structure parent
            //zNetView = GetComponent<ZNetView>();
            //if (zNetView == null ) {
            //    zNetView = GetComponentInParent<ZNetView>();
            //    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Looking for a parented znet view."); }
            //}
            //if (zNetView == null)
            //{
            //    GameObject parent = this.transform.parent.gameObject;
            //    zNetView = parent.GetComponent<ZNetView>();
            //    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Looking in manually selected parent {parent.name} container for ZnetView."); }
            //}
            //if (zNetView == null) {
            //    // this is used to check if the znet is valid, and send all of the data around- so this is probably gunna break things
            //    zNetView = new ZNetView();
            //    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("created a new znet view to use."); }
            //}

        }

        private void setupLateNetworking()
        {
            zNetView = GetComponentInParent<ZNetView>();
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Looking for a parented znet view {zNetView}."); }

            spawned_creatures = new IntZNetProperty("spawned_creatures", zNetView, 0);
            hard_mode = new BoolZNetProperty("shrine_hard_mode", zNetView, false);
            boss_mode = new BoolZNetProperty("shrine_boss_mode", zNetView, false);
            siege_mode = new BoolZNetProperty("shrine_siege_mode", zNetView, false);
            challenge_active = new BoolZNetProperty("shrine_challenge_active", zNetView, false);
            start_challenge = new BoolZNetProperty("shrine_start_challenge", zNetView, false);
            selected_level = new IntZNetProperty("shrine_selected_level", zNetView, 0);
            selected_reward = new StringZNetProperty("shrine_selected_reward", zNetView, "coins");
            end_of_challenge = new BoolZNetProperty("end_of_challenge", zNetView, false);
            should_add_creature_beacons = new BoolZNetProperty("should_add_creature_beacons", zNetView, false);
            currentPhase = new IntZNetProperty("shrine_current_phase", zNetView, 0);
            wave_definition_ready = new BoolZNetProperty("wave_definition_ready", zNetView, false);
            spawn_locations_ready = new BoolZNetProperty("spawn_locations_ready", zNetView, false);
            force_next_phase = new BoolZNetProperty("force_next_phase", zNetView, false);
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Created shrine znet values."); }

            Dictionary<String, short> default_creature_dictionary = new Dictionary<String, short>() { };
            alive_creature_list = new DictionaryZNetProperty("alive_creature_list", zNetView, default_creature_dictionary);

            WaveDefinitionRPC = NetworkManager.Instance.AddRPC("levelsyaml_rpc", VFConfig.OnServerRecieveConfigs, OnClientReceivePhaseConfigs);
            SynchronizationManager.Instance.AddInitialSynchronization(WaveDefinitionRPC, SendPhaseConfigs);

            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Added shrine custom RPC."); }
        }

        public override string GetHoverName()
        {
            return Localization.instance.Localize(wildShrineConfiguration.wildShrineNameLocalization);
        }

        public override string GetHoverText()
        {
            string text = $"[<color=yellow><b>$KEY_Use</b></color>] {wildShrineConfiguration.wildShrineNameLocalization}";
            return Localization.instance.Localize(text);
        }

        public override bool Interact(Humanoid user, bool hold, bool alt)
        {
            user.Message(MessageHud.MessageType.Center, Localization.instance.Localize(wildShrineConfiguration.wildShrineRequestLocalization));
            return false;
        }

        public override bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"znet data {start_challenge} {challenge_active}."); }
            if (start_challenge.Get() || challenge_active.Get())
            {
                // Don't want the message that you can't do that, because its already done
                return true;
            }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Evaluating offered tribute {item.m_shared.m_name} {item.m_dropPrefab.name}."); }
            short wild_level_index = 0;
            foreach(WildShrineLevelConfiguration wlevelcfg in wildShrineConfiguration.wildShrineLevelsConfig)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Checking tribute: {wlevelcfg.tributeName} == {item.m_dropPrefab.name} ({(wlevelcfg.tributeName == item.m_dropPrefab.name)})"); }
                if (wlevelcfg.tributeName == item.m_dropPrefab.name)
                {
                    List<ItemDrop.ItemData> user_inventory = user.m_inventory.GetAllItemsSorted();
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"User Inventory contains {user_inventory.Count} items."); }
                    int user_tribute_count = 0;
                    Dictionary<ItemDrop.ItemData, int> user_tribute_indexes = new Dictionary<ItemDrop.ItemData, int>();
                    foreach (ItemDrop.ItemData user_item in user_inventory)
                    {
                        if (user_item.m_dropPrefab.name == wlevelcfg.tributeName)
                        {
                            user_tribute_indexes.Add(user_item, user_item.m_stack);
                            user_tribute_count += user_item.m_stack;
                        }
                        
                    }
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"User Inventory contains {user_tribute_count} of {wlevelcfg.tributeAmount} required tribute type {wlevelcfg.tributeName}."); }
                    if (user_tribute_count >= wlevelcfg.tributeAmount)
                    {
                        int tribute_required_remaining = wlevelcfg.tributeAmount;
                        foreach (KeyValuePair<ItemDrop.ItemData, int> tribute_entries in user_tribute_indexes)
                        {
                            int tribute_index = user_inventory.IndexOf(tribute_entries.Key);
                            if (tribute_required_remaining > tribute_entries.Value)
                            {
                                tribute_required_remaining -= tribute_entries.Value;
                                user_inventory.Remove(tribute_entries.Key);
                            } else
                            {
                                user_inventory[tribute_index].m_stack -= tribute_required_remaining;
                                if (user_inventory[tribute_index].m_stack == 0)
                                {
                                    user_inventory.Remove(tribute_entries.Key);
                                } 
                            }
                        }
                        hard_mode.ForceSet(wlevelcfg.hardMode);
                        siege_mode.ForceSet(wlevelcfg.siegeMode);
                        selected_level.ForceSet(wild_level_index);
                        start_challenge.ForceSet(true);
                        user.Message(MessageHud.MessageType.Center, Localization.instance.Localize(wlevelcfg.wildshrine_wave_start_localization));
                        if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Deducted {wlevelcfg.tributeAmount} {wlevelcfg.tributeName} and enabling challenge setup."); }
                        return true;
                    } else
                    {
                        user.Message(MessageHud.MessageType.Center, Localization.instance.Localize(wildShrineConfiguration.shrine_larger_tribute_required_localization));
                        return true;
                    }
                }
                wild_level_index++;
            }

            user.Message(MessageHud.MessageType.Center, Localization.instance.Localize(wildShrineConfiguration.shrine_unaccepted_tribute_localization));
            return false;
        }

        public override void StartChallengeMode()
        {
            currentPhase.Set(0); //ensure this is zero
            // Must be before portals are placed
            challenge_active.Set(true);
            // Should be before the phase starts
            phase_running = true;
            RemoteLocationPortals.DrawMapOverlayAndPortals(remote_spawn_locations, gameObject.GetComponent<WildShrine>());
            spawn_controller.TrySpawningPhase(5f, false, wave_phases_definitions.hordePhases[currentPhase.Get()], gameObject, remote_spawn_locations);
            SetCurrentCreatureList(wave_phases_definitions.hordePhases[currentPhase.Get()]);
            start_challenge.Set(false);
            currentPhase.Set(currentPhase.Get() + 1);
            Jotunn.Logger.LogInfo($"Challenge started. Level: {selected_level.Get()} Reward: {selected_reward.Get()}");
            Jotunn.Logger.LogInfo("Start challenge functions completed. Challenge started!");

        }

        public override void Update()
        {
            log_slower++;
            if (log_slower > 60) { log_slower = 0; }
            // This is required for the location because the used znetView isn't available until the location is placed.
            if (zNetView == null)
            {
                setupLateNetworking();
                return; // skip the first update tick
            }
            // We do nothing when this is not a znet object (this happens during object placement)
            if (zNetView.IsValid() != true) { return; }
            //if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Znet Valid, starting update."); }
            if (wildShrineConfiguration == null)
            {
                string shrine_name = gameObject.name;
                shrine_name = shrine_name.Remove(shrine_name.Length - 7,7); // remove the last 7 characters which is (Clone)
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Loading level definition for shrine {shrine_name}."); }
                wildShrineConfiguration = WildShrineData.GetWildShrineConfigurationForSpecificShrine(shrine_name);
            }

            // reconnect componets if they go missing
            if (spawn_controller == null || shrine_spawnpoint == null)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Missing required references for shrine, reconnecting."); }
                spawn_controller = this.gameObject.GetComponent<Spawner>();
                shrine_spawnpoint = this.transform.FindDeepChild("spawnpoint").gameObject;
            }
            //if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Componets available."); }

            // So clients and servers see the internal structure portal update
            if (challenge_active.Get() == true)
            {
                // Only need to enable the central portal once.
                // This is done for every client so everyone is in sync, because for some reason otherwise it doesn't show on some clients
                if (CentralPortalActiveStatus() == false)
                {
                    EnablePortal();
                }
            }
            else if (end_of_challenge.Get() == true)
            {
                Disableportal();
                end_of_challenge.ForceSet(false);
            }
            //if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Checked portal status."); }

            // Everything past here should only be run once, by whatever main thread is controlling the ticks in this region.
            if (!zNetView.IsOwner())
            {
                return;
            }
            //if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Entering ZnetOwner States."); }

            // Kick off the challenge- even if it was trigger by a non-znet owner
            if (start_challenge.Get() == true)
            {
                //if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Kicking off wave start."); }
                if (wave_definition_ready.Get() == false && spawn_locations_ready.Get() == false)
                {
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Build spawn locations & wave definition."); }
                    WildShrineLevelConfiguration wLevelDefinition = wildShrineConfiguration.wildShrineLevelsConfig.ElementAt(selected_level.Get());
                    wave_phases_definitions = Levels.generateRandomWaveWithOptions(wLevelDefinition.wildLevelDefinition.ToChallengeLevelDefinition(), hard_mode.Get(), false, siege_mode.Get(), wLevelDefinition.wildLevelDefinition.maxCreaturesPerPhaseOverride);
                    wave_definition_ready.Set(true);
                    selected_reward.Set(wLevelDefinition.rewards.Values.ToString());
                    StartCoroutine(RemoteLocationPortals.DetermineRemoteSpawnLocations(gameObject, gameObject.GetComponent<WildShrine>()));
                }

                // We can only actually start the challenge when all of the data objects are ready
                if (wave_definition_ready.Get() == true && spawn_locations_ready.Get() == true)
                {
                    if (wave_phases_definitions == null || remote_spawn_locations == null)
                    {
                        // we got here but arn't ready yet, DO IT AGAIN
                        wave_definition_ready.Set(false);
                        spawn_locations_ready.Set(false);
                        if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Challenge tried to start but did not have the data objects required to do so, attempting regeneration."); }
                    }
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Starting challenge and sending phase configs to others."); }
                    SendUpdatedPhaseConfigs();
                    StartChallengeMode();
                }
                // we skip to the next update iteration
                return;
            }

            // Jotunn.Logger.LogInfo("Did not need to start a challenge.");
            //if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Challenge start not needed."); }

            if (challenge_active.Get() == true)
            {
                //if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Challenge active."); }
                // Generally this mode is entered when the shrine does not have wave information, but has passed the generation phase
                if (wave_phases_definitions == null)
                {
                    Jotunn.Logger.LogInfo("Starting shrine reconnection to creatures, this will regenerate the wave definition.");
                    StartCoroutine(ReconnectUnlinkedCreatures(shrine_spawnpoint.transform.position, gameObject.GetComponent<WildShrine>()));
                    WildShrineLevelConfiguration wLevelDefinition = wildShrineConfiguration.wildShrineLevelsConfig.ElementAt(selected_level.Get());
                    wave_phases_definitions = Levels.generateRandomWaveWithOptions(wLevelDefinition.wildLevelDefinition.ToChallengeLevelDefinition(), hard_mode.Get(), false, siege_mode.Get(), wLevelDefinition.wildLevelDefinition.maxCreaturesPerPhaseOverride);
                    return;
                }
                if (wave_phases_definitions.hordePhases.Count > 0)
                {
                    // We need to A. have spawned creatures & there needs to be none of those spawned creatures remaining
                    if (VFConfig.EnableDebugMode.Value && log_slower == 60) { Jotunn.Logger.LogInfo($"Checking enemies: {enemies.Count} spawned_creatures: {spawned_creatures.Get()} phase_running: {phase_running}"); }
                    ///if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Checking start for next phase"); }

                    if (force_next_phase.Get() || enemies.Count > 0 && spawned_creatures.Get() <= 0 && phase_running == false)
                    {
                        if (RemainingPhases())
                        {
                            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Starting next phase."); }
                            // Start the next phase
                            should_add_creature_beacons.Set(false);
                            force_next_phase.Set(false);
                            var current_phase = currentPhase.Get();
                            spawn_controller.TrySpawningPhase(10f, true, wave_phases_definitions.hordePhases[current_phase], gameObject, remote_spawn_locations);
                            SetCurrentCreatureList(wave_phases_definitions.hordePhases[current_phase]);
                            phase_running = true;
                            int max_wave_phase = wave_phases_definitions.hordePhases.Count;
                            int expected_next_phase = currentPhase.Get() + 1;
                            if (max_wave_phase <= expected_next_phase)
                            {
                                currentPhase.Set(max_wave_phase);
                            }
                            else
                            {
                                currentPhase.Set(expected_next_phase);
                            }
                        }
                        else
                        {
                            // No phases remaining, we are done!
                            Jotunn.Logger.LogInfo("Challenge complete! Spawning reward.");
                            List<Player> nearby_players = new List<Player> { };
                            Player.GetPlayersInRange(this.transform.position, VFConfig.ShrineAnnouncementRange.Value, nearby_players);
                            foreach (Player localplayer in nearby_players)
                            {
                                localplayer.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$shrine_challenge_complete"));
                            }
                            WildShrineLevelConfiguration wLevelDefinition = wildShrineConfiguration.wildShrineLevelsConfig.ElementAt(selected_level.Get());
                            SpawnMultiRewardsDirectly(wLevelDefinition.rewards, wLevelDefinition.wildLevelDefinition.levelIndex, shrine_spawnpoint.transform.position, hard_mode.Get(), boss_mode.Get(), siege_mode.Get());
                            challenge_active.Set(false);
                            boss_mode.Set(false);
                            hard_mode.Set(false);
                            siege_mode.Set(false);
                            end_of_challenge.Set(true);
                            force_next_phase.Set(false);
                            Disableportal();
                            wave_phases_definitions = new PhasedWaveTemplate() { hordePhases = new List<List<HoardConfig>> { } }; // Got to clear the template
                            SendUpdatedPhaseConfigs();
                            currentPhase.Set(0);
                            wave_definition_ready.Set(false);
                            spawn_locations_ready.Set(false);
                        }
                    }
                }
            }
        }
    }
}
