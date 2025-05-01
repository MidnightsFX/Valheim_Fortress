using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = ValheimFortress.Common.Logger;

namespace ValheimFortress.Defenses
{
    internal static class BallistaOnChange
    {
        internal static void BallistaCooldownTime_SettingChanged(object sender, EventArgs e)
        {
            foreach (GameObject go in findPrefabInScene())
            {
                Logger.LogInfo($"Updating attack cooldown duration on {go.name}");
                go.GetComponent<VFTurret>().m_attackCooldown = VFConfig.BallistaCooldownTime.Value;
            }
        }

        internal static void BallistaAmmoAccuracyPenalty_SettingChanged(object sender, EventArgs e)
        {
            foreach (GameObject go in findPrefabInScene())
            {
                Logger.LogInfo($"Updating accuracy on {go.name}");
                go.GetComponent<VFTurret>().m_ammo_accuracy = VFConfig.BallistaAmmoAccuracyPenalty.Value;
            }
        }

        internal static void BallistaRange_SettingChanged(object sender, EventArgs e)
        {
            foreach (GameObject go in findPrefabInScene())
            {
                Logger.LogInfo($"Updating range on {go.name}");
                go.GetComponent<VFTurret>().m_viewDistance = VFConfig.BallistaRange.Value;
            }
        }

        internal static void BallistaDamageChange(object sender, EventArgs e)
        {
            foreach (GameObject go in findPrefabInScene())
            {
                Logger.LogInfo($"Updating dmg on {go.name}");
                go.GetComponent<VFTurret>().m_Ammo.m_shared.m_damages.m_pierce = VFConfig.BallistaDamage.Value;
            }
        }

        private static IEnumerable<GameObject> findPrefabInScene()
        {
            IEnumerable<GameObject> objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name.StartsWith("VFpiece_turret"));
            Logger.LogInfo($"Found in scene objects: {objects.Count()}");
            return objects;
        }
    }
}
