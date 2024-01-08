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
        private GameObject shrineReference;

        public void SetShrine(GameObject shrine)
        {
            shrineReference = shrine;
        }

        public void Awake()
        {
            zNetView = GetComponent<ZNetView>();
        }
        // When the object is destroyed, mention
        void OnDestroy()
        {
            shrineReference.GetComponent<Shrine>().DecrementSpawned();
        }

        public void Update()
        {
            if (!zNetView.IsValid() || !zNetView.IsOwner())
            {
                return;
            }

            if (shrineReference.GetComponent<Shrine>().challenge_active.Get() == false)
            {
                if (zNetView.IsValid() && zNetView.IsOwner())
                {
                    ZNetScene.instance.Destroy(base.gameObject);
                } else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
