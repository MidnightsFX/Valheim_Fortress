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
        public Int32 spawned_creatures = 0;
        public static bool hard_mode = false;
        public static bool boss_mode = false;
        public static bool siege_mode = false;
        private static bool challenge_active = false;
        private static string selected_reward = "";
        private static Int16 selected_level = 0;
        private ZNetView zNetView;
        public static bool shrine_ui_active = false;
        public List<GameObject> portals = new List<GameObject>();

        private static List<GameObject> enemies = new List<GameObject>();
        private static GameObject shrine_spawnpoint;
        private static PhasedWaveTemplate wave_phases_definitions = new PhasedWaveTemplate();
        private static Vector3[] remote_spawn_locations = new Vector3[0];
        private static bool phase_running = false;
        private static Spawner spawn_controller;
        private static UI ui_controller;
        private static bool creature_beacons = false;

        public void SetHardMode()
        {
            hard_mode = true;
        }
        public void SetBossMode()
        {
            boss_mode = true;
        }
        public void SetSiegeMode()
        {
            siege_mode = true;
        }

        public bool IsChallengeActive()
        {
            return challenge_active;
        }

        public void SetReward(String reward)
        {
            selected_reward = reward;
        }

        public void SetLevel(Int16 level)
        {
            selected_level = level;
        }

        public void IncrementSpawned()
        {
            spawned_creatures += 1;
        }

        public void setPortals(List<GameObject> portal_list)
        {
            portals.Clear();
            portals = portal_list;
        }

        public void destroyPortals()
        {
            foreach(GameObject portal in portals)
            {
                Destroy(portal, UnityEngine.Random.Range(1, 5));
            }
            portals.Clear();
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
            if (zNetView.IsOwner() && challenge_active == false)
            {
                Jotunn.Logger.LogInfo("Challenge started!");
                challenge_active = true;
                spawn_controller.TrySpawningPhase(8f, false, wave_phases_definitions.GetCurrentPhase(), gameObject, remote_spawn_locations);
                phase_running = true;
                // Spawner.TrySpawningPhase(2f, false, wave_phases_definitions.GetCurrentPhase(), this.gameObject, remote_spawn_locations);
            } else
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Challenge mode is already active."); }
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
            if (spawned_creatures > 0)
            {
                spawned_creatures -= 1;
            }
            
        }

        public void Update()
        {
            if (shrine_ui_active && (Input.GetKeyDown(KeyCode.Escape)))
            {
                Jotunn.Logger.LogInfo("Shrine UI detected close commands.");
                ui_controller.HideUI();
            }
            // Everything past here should only be run once, by whatever main thread is controlling the ticks in this region.
            if (!zNetView.IsOwner())
            {
                return;
            }
            if (challenge_active == true)
            {
                if (wave_phases_definitions.CountPhases() > 0)
                {
                    // We need to A. have spawned creatures & there needs to be none of those spawned creatures remaining
                    if (enemies.Count > 0 && spawned_creatures <= 0 && phase_running == false) {
                        if (wave_phases_definitions.RemainingPhases())
                        {
                            // Start the next phase
                            creature_beacons = false;
                            spawn_controller.TrySpawningPhase(10f, true, wave_phases_definitions.GetCurrentPhase(), gameObject, remote_spawn_locations);
                            phase_running = true;
                        } else
                        {
                            // No phases remaining, we are done!
                            Jotunn.Logger.LogInfo("Challenge complete! Spawning reward.");
                            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Challenge Complete!");
                            StartCoroutine(Rewards.SpawnReward(selected_reward, selected_level, gameObject, hard_mode, boss_mode, siege_mode));
                            challenge_active = false;
                            boss_mode = false;
                            hard_mode = false;
                            siege_mode = false;
                            Disableportal();
                            wave_phases_definitions = new PhasedWaveTemplate(); // Got to clear the template
                            if (portals.Count > 0)
                            {
                                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Destroying wave portals"); }
                                destroyPortals();
                            }
                        }
                    }
                }
            }
        }

        private void Awake()
        {
            zNetView = GetComponent<ZNetView>();
            this.gameObject.AddComponent<Spawner>();
            spawn_controller = this.gameObject.GetComponent<Spawner>();
            this.gameObject.AddComponent<UI>();
            ui_controller = this.gameObject.GetComponent<UI>();
            shrine_spawnpoint = this.transform.Find("spawnpoint").gameObject;
        }

        public string GetHoverText()
        {
            string text = "[<color=yellow><b>$KEY_Use</b></color>] $piece_shrine_of_challenge";
            return Localization.instance.Localize(text);
            // return "\n[<color=yellow><b>E</b></color>] Alter of Challenge";
        }

        public string GetHoverName()
        {
            // TODO: Localization
            return "Shrine of Challenge";
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (hold)
            {
                return false;
            }

            //TODO: Add in support for ward checks

            if (!shrine_ui_active)
            {
                if (challenge_active) 
                {
                    if (spawned_creatures <= VFConfig.NotifyCreatureThreshold.Value && creature_beacons == false)
                    {
                        NotifyRemainingCreatures();
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"A few Creatures remaining ({spawned_creatures}) sending flares.");
                        creature_beacons = true;
                    } else if (spawned_creatures <= VFConfig.TeleportCreatureThreshold.Value)
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Teleporting final creatures to the shrine.");
                        TeleportRemainingCreatures();
                    } else
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Creatures remaining {spawned_creatures}");
                    }
                } else
                {
                    ui_controller.DisplayUI();
                }
                
            }

            return true;
        }
        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        public void NotifyRemainingCreatures()
        {
            int alive_creatures = 0;
            foreach (GameObject enemy in enemies)
            {
                if (enemy == null) { continue; }

                alive_creatures += 1;
                GameObject vfx = UnityEngine.Object.Instantiate(ValheimFortress.getNotifier(), enemy.transform.localPosition, enemy.transform.rotation);
                vfx.transform.parent = enemy.transform; // Parent to the creature?
            }
            // If somehow the tracked creatures got de-synced, this has literally never happend but- why not
            if (alive_creatures != spawned_creatures) { spawned_creatures = alive_creatures; }
        }

        public void TeleportRemainingCreatures()
        {
            int alive_creatures = 0;
            foreach (GameObject enemy in enemies)
            {
                if (enemy == null) { continue; }
                enemy.transform.position = shrine_spawnpoint.transform.position; 
                alive_creatures += 1;
            }
            // If somehow the tracked creatures got de-synced, this has literally never happend but- why not
            if (alive_creatures != spawned_creatures) { spawned_creatures = alive_creatures; }
        }
    }
}
