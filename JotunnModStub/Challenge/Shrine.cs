using Jotunn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimFortress.Challenge
{
    public class Shrine : MonoBehaviour, Hoverable, Interactable
    {
        public Int32 spawned_creatures = 0;
        private static bool challenge_active = false;
        private static string selected_reward = "";
        private static Int16 selected_level = 0;
        private ZNetView zNetView;
        public static bool shrine_ui_active = false;
        public static Int16 spawned_waves = 0;

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

        public void WaveSpawned()
        {
            spawned_waves -= 1;
        }

        public void setSpawnedWaveTarget(Int16 target)
        {
            spawned_waves = target;
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

        public void StartChallengeMode()
        {
            if (zNetView.IsOwner() && challenge_active == false)
            {
                Jotunn.Logger.LogInfo("Challenge started!");
                challenge_active = true;
            } else
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Not scene owner, doing nothing."); }
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
            if (!zNetView.IsOwner())
            {
                //Jotunn.Logger.LogInfo("Not updating challenge status.");
                return;
            }
            if (challenge_active == true)
            {
                if (spawned_creatures == 0 && spawned_waves <= 0)
                {
                    spawned_waves = 0;
                    Jotunn.Logger.LogInfo("Challenge complete! Spawning reward.");
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Challenge Complete!");
                    Rewards.SpawnReward(selected_reward, selected_level, gameObject);
                    challenge_active = false;
                    Destroy(this.GetComponent<Spawner>()); // remove the spawner since its completed its work and will be recreated for the next challenge.
                } else
                {
                    // This is INSANELY verbose because its EVERY update
                    //if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Challege in progress... {spawned_creatures} creatures remaining."); }
                }
            } else
            {
                // challenge mode is not active yet
                if (spawned_creatures > 0 && spawned_waves <= 0)
                {
                    StartChallengeMode();
                    Disableportal();
                }
            }
            if (shrine_ui_active && (Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("Use") || ZInput.GetButtonDown("Inventory")))
            {
                Jotunn.Logger.LogInfo("Shrine UI detected close commands.");
                DisableUI();
            }

        }

        private void Awake()
        {
            zNetView = GetComponent<ZNetView>();
        }

        public string GetHoverText()
        {
            // TODO: Should be replaced with a hugin tutorial text.
            // TODO: Localization
            return "\n[<color=yellow><b>E</b></color>] Alter of Challenge";
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
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"A challenge is active! Creatures remaining {spawned_creatures}");
                } else
                {
                    EnableUI();
                }
                
            }

            return true;
        }
        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        public void EnableUI()
        {
            shrine_ui_active = true;
            UI.DisplayUI(this.gameObject);
        }

        public void DisableUI()
        {
            shrine_ui_active = false;
            UI.HideUI();
        }
    }
}
