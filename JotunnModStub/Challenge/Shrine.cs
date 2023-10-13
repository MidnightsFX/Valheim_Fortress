using System;
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

        public void StartChallengeMode()
        {
            if (zNetView.IsOwner())
            {
                Jotunn.Logger.LogInfo("Challenge started!");
                challenge_active = true;
            } else
            {
                Jotunn.Logger.LogInfo("Not scene owner, doing nothing.");
            }
            
        }

        public void DecrementSpawned()
        {
            if (!zNetView.IsOwner())
            {
                Jotunn.Logger.LogInfo("Not scene owner, not decrementing.");
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
                Jotunn.Logger.LogInfo("Not updating challenge status.");
                return;
            }
            if (challenge_active == true)
            {
                if (spawned_creatures == 0)
                {
                    Jotunn.Logger.LogInfo("Challenge complete! Spawning reward.");
                    Rewards.SpawnReward(selected_reward, selected_level, this.gameObject);
                    challenge_active = false;
                } else
                {
                    // Jotunn.Logger.LogInfo($"Challege in progress... {spawned_creatures} creatures remaining.");
                }
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

            if (!UI.IsPanelVisible())
            {
                Jotunn.Logger.LogInfo("Attempting to spawn UI with shrine ref.");
                // This, for the shrine object passthrough to tell the spawner script where tf we are
                UI.DisplayUI(this.gameObject);
            }

            return true;
        }
        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }
    }
}
