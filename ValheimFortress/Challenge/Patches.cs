using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ValheimFortress.Challenge
{
    public static class Patches
    {
        [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
        [HarmonyPriority(Priority.VeryHigh)]
        public static class DisableDropsCheck
        {
            public static void Prefix(Character __instance)
            {
                //Jotunn.Logger.LogInfo($"[Humanoid] Checking for removal of drops {__instance.m_nview.GetZDO().GetBool("VFDrops", true)} == false ?.");
                if (__instance.m_nview.GetZDO().GetBool("VFDrops", true) == false) {
                    //Jotunn.Logger.LogInfo($"Skipping CharacterDrop");
                    __instance.m_onDeath = null;
                }
            }
        }

        // Corpses lingering after the challenge complete drop loot since the
        // challenge ends and nothing references it anymore. This surpresses it

        [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.GenerateDropList))]
        public static class SuppressChallengeDrops
        {
            public static bool Prefix(CharacterDrop __instance, ref List<KeyValuePair<GameObject, int>> __result)
            {
                Character character = __instance.m_character != null ? __instance.m_character : __instance.GetComponent<Character>();
                ZNetView view = character != null ? character.m_nview : null;
                ZDO zdo = view != null && view.IsValid() ? view.GetZDO() : null;
                if (zdo == null || zdo.GetBool("VFDrops", true)) { return true; }

                __result = new List<KeyValuePair<GameObject, int>>();
                return false;
            }
        }
    }
}
