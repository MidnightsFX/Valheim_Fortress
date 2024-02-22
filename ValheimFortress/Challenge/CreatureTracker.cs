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

        public void SetShrine(GenericShrine shrine)
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
            shrineReference.DecrementSpawned();
        }

        public void Update()
        {
            if (!zNetView.IsValid() || !zNetView.IsOwner())
            {
                return;
            }
            if (shrineReference.challenge_active.Get() == false)
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
