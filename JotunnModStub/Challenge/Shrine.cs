using Jotunn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
using static ValheimFortress.Challenge.Levels;

namespace ValheimFortress.Challenge
{
    public class Shrine : MonoBehaviour, Hoverable, Interactable
    {
        private ZNetView zNetView;
        public IntZNetProperty spawned_creatures { get; private set; }
        public BoolZNetProperty hard_mode { get; private set; }
        public BoolZNetProperty boss_mode { get; private set; }
        public BoolZNetProperty siege_mode { get; private set; }
        public BoolZNetProperty challenge_active { get; private set; }

        public BoolZNetProperty start_challenge { get; private set; }

        public IntZNetProperty selected_level { get; private set; }
        public StringZNetProperty selected_reward { get; private set; }
        public BoolZNetProperty end_of_challenge { get; private set; }
        public BoolZNetProperty should_add_creature_beacons { get; private set; }
        private static bool client_set_creature_beacons = false;
        public static bool shrine_ui_active = false;

        private static List<GameObject> enemies = new List<GameObject>();
        private static GameObject shrine_spawnpoint;
        private static PhasedWaveTemplate wave_phases_definitions = new PhasedWaveTemplate();
        private static Vector3[] remote_spawn_locations = new Vector3[0];
        private static bool phase_running = false;
        private static Spawner spawn_controller;
        private static UI ui_controller;

        private void Awake()
        {
            zNetView = GetComponent<ZNetView>();

            if (zNetView.IsValid())
            {
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
                Jotunn.Logger.LogInfo("Created Shrine Znet View Values.");
            }
            Jotunn.Logger.LogInfo("Shrine Awake Finished");
        }

        public void SetHardMode()
        {
            hard_mode.ForceSet(true);
        }
        public void SetBossMode()
        {
            boss_mode.ForceSet(true);
        }
        public void SetSiegeMode()
        {
            siege_mode.ForceSet(true);
        }

        public bool IsChallengeActive()
        {
            return challenge_active.Get();
        }

        public void SetReward(String reward)
        {
            selected_reward.ForceSet(reward);
        }

        public void SetLevel(Int16 level)
        {
            selected_level.ForceSet(level);
        }

        public void SetStartChallenge()
        {
            start_challenge.ForceSet(true);
        }

        public void IncrementSpawned()
        {
            spawned_creatures.Set(spawned_creatures.Get() + 1);
        }

        public void EnablePortal()
        {
            // gets the child object which holds all of the portal fx etc, and enables it
            GameObject shrine_portal = transform.Find("portal").gameObject;
            shrine_portal.SetActive(true);
        }

        public void Disableportal()
        {
            GameObject shrine_portal = transform.Find("portal").gameObject;
            shrine_portal.SetActive(false);
        }

        public void addEnemy(GameObject enemy)
        {
            enemies.Add(enemy);
        }

        public void phaseCompleted()
        {
            phase_running = false;
        }

        public int EnemiesRemaining()
        {
            try
            {
                return spawned_creatures.Get();
            } catch
            {
                Jotunn.Logger.LogInfo("Znet Value not retrieved for enemies remaining.");
                return 0;
            }
            
        }
        
        public void SetWaveDefinition(PhasedWaveTemplate defined_waves)
        {
            wave_phases_definitions = defined_waves;
        }

        public void SetWaveSpawnPoints(Vector3[] spawn_points)
        {
            remote_spawn_locations = spawn_points;
        }

        public void SetShrineUIStatus(bool status)
        {
            shrine_ui_active = status;
        }

        public void StartChallengeMode()
        {
            if (challenge_active.Get() == false)
            {
                Jotunn.Logger.LogInfo($"Challenge started. Level: {selected_level.Get()} Reward: {selected_reward.Get()}");
                Levels.generateRandomWaveWithOptions((short)selected_level.Get(), hard_mode.Get(), boss_mode.Get(), siege_mode.Get(), gameObject);
                challenge_active.Set(true);
                spawn_controller.TrySpawningPhase(8f, false, wave_phases_definitions.GetCurrentPhase(), gameObject, remote_spawn_locations);
                phase_running = true;
                start_challenge.Set(false);
                Jotunn.Logger.LogInfo("Start challenge functions completed. Challenge started!");
            } else
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Challenge mode is already active."); }
            }
            
        }

        public void CleanupOldPortals(short max_cleanup_iterations = 6)
        {
            Jotunn.Logger.LogInfo("Starting cleanup of old portals.");
            // Cleanups up old portals
            int portals_cleaned = 1;
            int limit = (max_cleanup_iterations + 1);
            try
            {
                for (var i = 0; i <= portals_cleaned; i++)
                {
                    if (i > limit) { break; }
                    GameObject old_portal = GameObject.Find("VF_portal(Clone)");
                    if (old_portal != null)
                    {
                        Jotunn.Logger.LogInfo($"Found gameobject: {old_portal.name}.");
                        Destroy(old_portal.gameObject);
                        // we keep doing iterations if we actually find something to clean
                        portals_cleaned++;
                    }
                }
            } catch
            {
                Jotunn.Logger.LogInfo("Cleanup of portals failed.");
            }
        }

        public void DecrementSpawned()
        {
            if (!zNetView.IsOwner())
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Not zview owner, not decrementing."); }
                return;
            }
            // We don't want to try to decrease this past what is expected.
            // The spawned creatures data could be lost at some point so lets avoid going negative.
            if (spawned_creatures.Get() > 0)
            {
                spawned_creatures.Set(spawned_creatures.Get() - 1);
            }
            
        }

        public void Update()
        {
            // We do nothing when this is not a znet object (this happens during object placement)
            if (zNetView.IsValid() != true) { return; }

            // reconnect componets if they go missing
            if (ui_controller == null || spawn_controller == null || shrine_spawnpoint == null)
            {
                spawn_controller = this.gameObject.GetComponent<Spawner>();
                shrine_spawnpoint = this.transform.Find("spawnpoint").gameObject;
                ui_controller = this.gameObject.GetComponent<UI>();
            }

            if (shrine_ui_active && (Input.GetKeyDown(KeyCode.Escape)))
            {
                Jotunn.Logger.LogInfo("Shrine UI detected close commands.");
                ui_controller.HideUI();
                ui_controller.HideCancelUI();
            }
            // Jotunn.Logger.LogInfo("Shrine UI not closing.");

            // So clients and servers see the internal structure portal update
            if (challenge_active.Get() == true)
            {
                EnablePortal();
            } else if (end_of_challenge.Get()) {
                Disableportal();
                end_of_challenge.ForceSet(false);
            }

            // Jotunn.Logger.LogInfo("Shrine portal status not updating.");

            // Everything past here should only be run once, by whatever main thread is controlling the ticks in this region.
            if (!zNetView.IsOwner())
            {
                return;
            }

            // Jotunn.Logger.LogInfo("Entering ZnetOwner States.");

            // Kick off the challenge- even if it was trigger by a non-znet owner
            if (start_challenge.Get() == true)
            {
                StartChallengeMode();
                // we skip to the next update iteration
                return;
            }

            // Jotunn.Logger.LogInfo("Did not need to start a challenge.");

            if (challenge_active.Get() == true)
            {
                if (wave_phases_definitions.CountPhases() > 0)
                {
                    // We need to A. have spawned creatures & there needs to be none of those spawned creatures remaining
                    if (enemies.Count > 0 && spawned_creatures.Get() <= 0 && phase_running == false) {
                        if (wave_phases_definitions.RemainingPhases())
                        {
                            // Start the next phase
                            should_add_creature_beacons.Set(false);
                            spawn_controller.TrySpawningPhase(10f, true, wave_phases_definitions.GetCurrentPhase(), gameObject, remote_spawn_locations);
                            phase_running = true;
                        } else
                        {
                            // No phases remaining, we are done!
                            Jotunn.Logger.LogInfo("Challenge complete! Spawning reward.");
                            List<Player> nearby_players = new List<Player> { };
                            Player.GetPlayersInRange(this.transform.position, VFConfig.ShrineAnnouncementRange.Value, nearby_players);
                            foreach (Player localplayer in nearby_players)
                            {
                                localplayer.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$shrine_challenge_complete"));
                            }
                            StartCoroutine(Rewards.SpawnReward(selected_reward.Get(), (short)selected_level.Get(), gameObject, hard_mode.Get(), boss_mode.Get(), siege_mode.Get()));
                            challenge_active.Set(false);
                            boss_mode.Set(false);
                            hard_mode.Set(false);
                            siege_mode.Set(false);
                            end_of_challenge.Set(true);
                            Disableportal();
                            wave_phases_definitions = new PhasedWaveTemplate(); // Got to clear the template
                        }
                    }
                }
            }
        }

        public string GetHoverText()
        {
            string text = "[<color=yellow><b>$KEY_Use</b></color>] $piece_shrine_of_challenge";
            return Localization.instance.Localize(text);
            // return "\n[<color=yellow><b>E</b></color>] Alter of Challenge";
        }

        public string GetHoverName()
        {
            return Localization.instance.Localize("$piece_shrine_of_challenge");
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (hold)
            {
                return false;
            }

            //TODO: Add in support for ward checks

            if (shrine_ui_active == false)
            {
                if (challenge_active.Get())
                {
                    // Cancel / remaining UI here.
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Creatures remaining {spawned_creatures.Get()}");
                    ui_controller.DisplayCancelUI();
                } else {
                    ui_controller.DisplayUI();
                }
            }

            return true;
        }
        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        public void CancelShrineRun()
        {
            challenge_active.ForceSet(false);
            boss_mode.ForceSet(false);
            hard_mode.ForceSet(false);
            siege_mode.ForceSet(false);
            Disableportal();
        }

        public void NotifyRemainingCreatures()
        {
            // if (should_add_creature_beacons.Get() == false) { return; }
            if (client_set_creature_beacons == true) { return; }
            int alive_creatures = 0;
            foreach (GameObject enemy in enemies)
            {
                if (enemy == null) { continue; }

                alive_creatures += 1;
                GameObject vfx = UnityEngine.Object.Instantiate(ValheimFortress.getNotifier(), enemy.transform.localPosition, enemy.transform.rotation);
                vfx.transform.parent = enemy.transform; // Parent to the creature?
            }
            // If somehow the tracked creatures got de-synced, this has literally never happend but- why not
            if (alive_creatures != spawned_creatures.Get()) { spawned_creatures.Set(alive_creatures); }
            client_set_creature_beacons = true;
        }

        public void TeleportRemainingCreatures()
        {
            if (spawned_creatures.Get() > 6)
            {
                List<Player> nearby_players = new List<Player> { };
                Player.GetPlayersInRange(this.transform.position, VFConfig.ShrineAnnouncementRange.Value, nearby_players);
                foreach (Player localplayer in nearby_players)
                {
                    localplayer.Message(MessageHud.MessageType.Center,  $"{Localization.instance.Localize("$shrine_too_many_creautres_to_teleport")} {spawned_creatures.Get()}.");
                }
                return;
            }
            int alive_creatures = 0;
            foreach (GameObject enemy in enemies)
            {
                if (enemy == null) { continue; }
                enemy.transform.position = shrine_spawnpoint.transform.position; 
                alive_creatures += 1;
            }
            // If somehow the tracked creatures got de-synced, this has literally never happend but- why not
            if (alive_creatures != spawned_creatures.Get()) { spawned_creatures.Set(alive_creatures); }
        }
    }
}
