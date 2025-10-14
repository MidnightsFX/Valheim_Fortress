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
    }
}
