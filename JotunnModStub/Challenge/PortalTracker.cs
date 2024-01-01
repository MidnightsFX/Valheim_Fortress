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
        private GameObject shrineReference;
        private static int destroy_timer = 0;
        public void SetShrine(GameObject shrine)
        {
            shrineReference = shrine;
        }

        public void Update()
        {
            // if the shrine reference is not set we start a countdown timer to destroy the portal
            // This only occurs when the portal is orphaned
            if (shrineReference == null) {
                if (destroy_timer >= 120) { Destroy(gameObject); }

                destroy_timer++;
                return;
            }

            // If the shrine reference is set we check to see if a challenge is active and when its no longer active we destroy the portal
            if (shrineReference.GetComponent<Shrine>().challenge_active.Get() == false)
            {
                GameObject destroyVFX = UnityEngine.Object.Instantiate(ValheimFortress.getPortalDestroyVFX(), this.transform.position, this.transform.rotation);
                Destroy(gameObject, 3);
                Destroy(destroyVFX, 7);
                return;
            }
        }
    }
}
