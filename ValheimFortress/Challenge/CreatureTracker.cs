using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

        public void Awake()
        {
            zNetView = GetComponent<ZNetView>();
        }
        // When the object is destroyed, mention
        void OnDestroy()
        {
            // if the znet is shutting down we don't want to or need to do this
            if (!zNetView.IsValid()) {
                shrineReference.DecrementSpecificCreatureSpawned(creature_name);
            }
        }

        public void Update()
        {
            if (!zNetView.IsValid() || !zNetView.IsOwner())
            {
                return;
            }
            if (shrineReference.IsChallengeActive() == false)
            {
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
                Destroy(gameObject);
            }
        }
    }
}
