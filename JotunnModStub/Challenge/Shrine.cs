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

        private void Awake()
        {
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
