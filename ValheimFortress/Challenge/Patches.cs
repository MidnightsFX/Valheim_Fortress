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
                if (__instance.m_nview.GetZDO().GetBool("VFDrops", true) == false) {
                    __instance.m_onDeath = null;
                }
            }
        }

        // A dying creature's ragdoll takes its loot list from CharacterDrop directly (Ragdoll.SaveLootList), outside
        // m_onDeath, and drops it when the corpse fades. The spawner only removes CharacterDrop on the peer that spawned
        // the creature, so another peer's copy, or one reloaded after its run ended, still has it.
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
