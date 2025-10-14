using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.XR;

namespace ValheimFortress.Challenge
{

    class CreatureTracker : MonoBehaviour
    {
        private ZNetView zNetView;
        private GenericShrine shrineReference;
        private String creature_name;

        public void SetShrine(GenericShrine shrine)
        {
            shrineReference = shrine;
        }

        public void setCreatureName(String cname)
        {
            creature_name = cname;
        }

        public void SetRemoveDrops(bool remove_drops) {
            zNetView.GetZDO().Set("VFDrops", remove_drops);
        }

        public void Awake()
        {
            zNetView = GetComponent<ZNetView>();
            //zNetView.GetZDO().Set("VFDrops", false);
        }
        // When the object is destroyed, mention
        void OnDestroy()
        {
            // if the znet is shutting down we don't want to or need to do this
            if (!zNetView.IsValid() && shrineReference != null) {
                shrineReference.DecrementSpecificCreatureSpawned(creature_name);
            }
        }

        public void Update()
        {
            if (zNetView == null) { return; }
            if (!zNetView.IsValid() || !zNetView.IsOwner()) {
                return;
            }
            if (shrineReference != null && shrineReference.IsChallengeActive() == false) {
                DestroySelf();
            }
        }

        private void DestroySelf()
        {
            if (zNetView.IsValid() && zNetView.IsOwner())
            {
                ZNetScene.instance.Destroy(base.gameObject);
            }
            else
            {
                Destroy(base.gameObject);
            }
        }
    }
}
