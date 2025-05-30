﻿using Jotunn.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimFortress.Challenge
{
    internal class ArenaShrine : GenericShrine
    {
        private ArenaShrineUI ui_controller;
        static new Vector3[] remote_spawn_locations = new Vector3[0];
        private short fail_to_start = 0;
        // We statically set the remote spawn locations to the internal shrine spawnpoint, because this is the gladiator shrine and thats the only place it spawns things from.

        public override string GetHoverName()
        {
            return Localization.instance.Localize("$piece_shrine_of_gladiator");
        }

        public override string GetHoverText()
        {
            string text = "[<color=yellow><b>$KEY_Use</b></color>] $piece_shrine_of_gladiator";
            return Localization.instance.Localize(text);
        }

        public override bool Interact(Humanoid user, bool hold, bool alt)
        {              
            if (hold)
            {
                return false;
            }

            //TODO: Add in support for ward checks

            if (challenge_active.Get())
            {
                // Cancel / remaining UI here.
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Creatures remaining {spawned_creatures.Get()}");
                ui_controller.DisplayCancelUI();
            }
            else
            {
                ui_controller.DisplayUI();
            }

            return true;
        }

        public override void StartChallengeMode()
        {
            if (challenge_active.Get() == false)
            {
                challenge_active.Set(true);
                currentPhase.Set(0); //ensure this is zero
                spawn_controller.TrySpawningPhase(5f, false, wave_phases_definitions.hordePhases[currentPhase.Get()], gameObject, remote_spawn_locations);
                SetCurrentCreatureList(wave_phases_definitions.hordePhases[currentPhase.Get()]);
                phase_running = true;
                Jotunn.Logger.LogInfo($"Challenge started. Level: {selected_level.Get()} Reward: {selected_reward.Get()}");
                start_challenge.Set(false);
                currentPhase.Set(currentPhase.Get() + 1);
                Jotunn.Logger.LogInfo("Start challenge functions completed. Challenge started!");
            }
            else
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Challenge mode is already active."); }
                fail_to_start++;
            }

            // this needs to be not super small due to how many times this function is triggered
            if (fail_to_start > 3)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Challenge mode failed to start, resetting."); }
                // reset all of the characteristics so that we get it all rebuilt to ensure the next try will actually work
                challenge_active.Set(false);
                wave_definition_ready.Set(false);
                spawn_locations_ready.Set(false);
                fail_to_start = 0;
                currentPhase.Set(0);
                start_challenge.Set(true);
            }

        }

        public override void Update()
        {
            // We do nothing when this is not a znet object (this happens during object placement)
            if (zNetView.IsValid() != true) { return; }

            // reconnect componets if they go missing
            // Set the spawnpoint to the internal portal
            if (ui_controller == null || spawn_controller == null || shrine_spawnpoint == null || remote_spawn_locations == null)
            {
                spawn_controller = this.gameObject.GetComponent<Spawner>();
                remote_spawn_locations = new Vector3[] { shrine_spawnpoint.transform.position, shrine_spawnpoint.transform.position, shrine_spawnpoint.transform.position };
                ui_controller = this.gameObject.GetComponent<ArenaShrineUI>();
            }

            if (ui_controller.IsShrineOrCancelUIVisible() && (Input.GetKeyDown(KeyCode.Escape)))
            {
                // Jotunn.Logger.LogInfo("Shrine UI detected close commands.");
                ui_controller.HideUI();
                ui_controller.HideCancelUI();
            }
            // Jotunn.Logger.LogInfo("Shrine UI not closing.");

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

            // Everything past here should only be run once, by whatever main thread is controlling the ticks in this region.
            if (!zNetView.IsOwner())
            {
                return;
            }

            // Jotunn.Logger.LogInfo("Entering ZnetOwner States.");

            // Kick off the challenge- even if it was trigger by a non-znet owner
            if (start_challenge.Get() == true)
            {
                if (wave_definition_ready.Get() == false)
                {
                    List<ChallengeLevelDefinition> clevels = Levels.GetChallengeLevelDefinitions();
                    ChallengeLevelDefinition levelDefinition = clevels.ElementAt(selected_level.Get());
                    wave_phases_definitions = Levels.generateRandomWaveWithOptions(levelDefinition, hard_mode.Get(), boss_mode.Get(), siege_mode.Get(), VFConfig.ArenaShrineMaxCreaturesPerWave.Value);
                    wave_definition_ready.Set(true);
                }

                // We can only actually start the challenge when all of the data objects are ready
                if (wave_definition_ready.Get() == true)
                {
                    SendUpdatedPhaseConfigs();
                    StartChallengeMode();
                }
                // we skip to the next update iteration
                return;
            }

            // Jotunn.Logger.LogInfo("Did not need to start a challenge.");

            if (challenge_active.Get() == true)
            {
                // the challenge should be running but there are no phase definitions. This happens when the shrine has become disconnected.
                if (wave_phases_definitions == null || enemies.Count == 0 && spawned_creatures.Get() > 0 && phase_running == false)
                {
                    Jotunn.Logger.LogInfo("Starting shrine reconnection to creatures, this will regenerate the wave definition.");
                    StartCoroutine(ReconnectUnlinkedCreatures(shrine_spawnpoint.transform.position, gameObject.GetComponent<ArenaShrine>()));
                    List<ChallengeLevelDefinition> clevels = Levels.GetChallengeLevelDefinitions();
                    ChallengeLevelDefinition levelDefinition = clevels.ElementAt(selected_level.Get());
                    wave_phases_definitions = Levels.generateRandomWaveWithOptions(levelDefinition, hard_mode.Get(), boss_mode.Get(), siege_mode.Get(), VFConfig.ChallengeShrineMaxCreaturesPerWave.Value);
                    wave_definition_ready.Set(true);
                    return;
                    // Ideally everything else is currently still correct since the last time this object was loaded
                }

                if (wave_phases_definitions.hordePhases.Count > 0)
                {
                    // We need to A. have spawned creatures & there needs to be none of those spawned creatures remaining
                    if (force_next_phase.Get() || enemies.Count > 0 && spawned_creatures.Get() <= 0 && phase_running == false)
                    {
                        if (RemainingPhases())
                        {
                            // Start the next phase
                            should_add_creature_beacons.Set(false);
                            force_next_phase.Set(false);
                            phase_running = true;
                            var current_phase = currentPhase.Get();
                            spawn_controller.TrySpawningPhase(10f, true, wave_phases_definitions.hordePhases[current_phase], gameObject, remote_spawn_locations);
                            SetCurrentCreatureList(wave_phases_definitions.hordePhases[current_phase]);
                            currentPhase.Set(current_phase + 1);
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
                            SpawnReward(shrine_spawnpoint.transform.position);
                            challenge_active.Set(false);
                            boss_mode.Set(false);
                            hard_mode.Set(false);
                            siege_mode.Set(false);
                            end_of_challenge.Set(true);
                            force_next_phase.Set(false);
                            Disableportal();
                            wave_phases_definitions = new PhasedWaveTemplate() { hordePhases = new List<List<HoardConfig>> { } }; // Got to clear the template
                            SendUpdatedPhaseConfigs();
                            wave_definition_ready.Set(false);
                            spawn_locations_ready.Set(false);
                        }
                    }
                }
            }
        }
    }
}
