using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimFortress.Challenge
{
    internal class PortalTracker : MonoBehaviour
    {
        private ZNetView zNetView;
        private GenericShrine shrineReference;
        private static int destroy_timer = 0;
        public void SetShrine(GenericShrine shrine)
        {
            shrineReference = shrine;
        }

        public void Awake()
        {
            zNetView = GetComponent<ZNetView>();
        }

        public void Update()
        {
            if (!zNetView.IsValid() || !zNetView.IsOwner())
            {
                return;
            }
            // if the shrine reference is not set we start a countdown timer to destroy the portal
            // This only occurs when the portal is orphaned
            if (shrineReference == null) {
                if (destroy_timer >= 30)
                {
                    ZNetScene.instance.Destroy(base.gameObject);
                }
                destroy_timer++;
                return;
            }

            // If the shrine reference is set we check to see if a challenge is active and when its no longer active we destroy the portal
            if (shrineReference.IsChallengeActive() == false)
            {
                GameObject destroyVFX = UnityEngine.Object.Instantiate(ValheimFortress.getPortalDestroyVFX(), this.transform.position, this.transform.rotation);
                ZNetScene.instance.Destroy(base.gameObject);
                Destroy(destroyVFX, 7);
                return;
            }
        }
    }
}
