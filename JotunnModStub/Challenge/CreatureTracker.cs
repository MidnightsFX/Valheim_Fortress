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
        private GameObject shrineReference;

        public void SetShrine(GameObject shrine)
        {
            shrineReference = shrine;
        }
        // When the object is destroyed, mention
        void OnDestroy()
        {
            shrineReference.GetComponent<Shrine>().DecrementSpawned();
            //Jotunn.Logger.LogInfo("Creature Destroyed");
        }
    }
}
