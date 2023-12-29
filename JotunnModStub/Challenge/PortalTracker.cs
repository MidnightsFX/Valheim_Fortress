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
        public void SetShrine(GameObject shrine)
        {
            shrineReference = shrine;
        }

        public void Update()
        {
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
