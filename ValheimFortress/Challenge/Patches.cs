using HarmonyLib;
using UnityEngine;

namespace ValheimFortress.Challenge
{
    public static class Patches
    {
        // The Elder's native root cast starts its floor ray 1000m above the target.
        // Inside a shrine room that selects the ceiling. Only adjust challenge-owned
        // root abilities; normal world bosses and other summons retain their settings.
        [HarmonyPatch(typeof(SpawnAbility), nameof(SpawnAbility.Setup))]
        public static class ShrineRootFloor
        {
            public static void Prefix(SpawnAbility __instance, Character owner)
            {
                if (owner == null || owner.GetZDOID().IsNone()) { return; }
                ZNetView view = owner.GetComponent<ZNetView>();
                if (view == null || !view.IsValid() || !view.GetZDO().GetBool("VFChallengeCreature")) { return; }
                if (__instance.m_spawnPrefab == null || __instance.m_spawnPrefab.Length == 0) { return; }
                foreach (GameObject prefab in __instance.m_spawnPrefab)
                {
                    if (prefab == null || prefab.name != "TentaRoot") { return; }
                }
                __instance.m_getSolidHeightMargin = 2;
            }
        }

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
