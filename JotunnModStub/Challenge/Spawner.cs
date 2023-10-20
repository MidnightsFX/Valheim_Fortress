using Jotunn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimFortress.Challenge
{
    class Spawner : MonoBehaviour
    {
        static private float wave_spawn_delay = 5f;
        static private List<String> bosses = new List<String>(new string[] { "Eikythr", "TheElder", "BoneMass", "Moder", "Yagluth", "TheQueen" });

        public void TrySpawningHoard(List<Levels.HoardConfig> hoards, GameObject shrine)
        {
            Jotunn.Logger.LogInfo($"Trying to spawn {hoards.Count} hoards.");
            // Should check if you are the runtime owner of this chunk
            foreach (Levels.HoardConfig hoard in hoards)
            {
                Jotunn.Logger.LogInfo($"Starting spawn for {hoard.amount} {hoard.creature}");
                StartCoroutine(Spawn(hoard, shrine));
            }
        }

        IEnumerator Spawn(Levels.HoardConfig hoard, GameObject shrine)
        {
            yield return new WaitForSeconds(5f);
            GameObject shrine_spawnpoint = shrine.transform.Find("spawnpoint").gameObject;
            Vector3 spawn_position = shrine_spawnpoint.transform.position;
            float height;
            if (ZoneSystem.instance.FindFloor(spawn_position, out height))
            {
                spawn_position.y = height;
            }
            GameObject gameObject = PrefabManager.Instance.GetPrefab(hoard.creature);
            int hoard_frac = hoard.amount / 3;
            bool should_pause_during_horde = true;
            if (hoard_frac == 0) 
            { 
                should_pause_during_horde = false;
            } else {
                Jotunn.Logger.LogInfo($"Hoard {hoard.creature} pausepoints {hoard_frac} {hoard_frac * 2}");
            }

            // Spawn our requested number of creatures, modify them as required.
            for (int i = 0; i < hoard.amount; i++)
            {

                if (should_pause_during_horde && i == hoard_frac || i == (hoard_frac * 2))
                {
                    yield return new WaitForSeconds(wave_spawn_delay);
                }
                Quaternion rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
                GameObject creature = UnityEngine.Object.Instantiate(gameObject, spawn_position, rotation);
                creature.GetComponent<ZNetView>(); // but why
                shrine.GetComponent<Shrine>().IncrementSpawned();

                // TODO: set to the same faction so they won't fight other spawns?
                // creature.GetComponent<Humanoid>().m_faction = "Undead";
                // Set the Itemdrop script to be disabled for these creatures, otherwise these hoards are likely to be more rewarding than the reward
                if (!VFConfig.EnableHordeDrops.Value)
                {
                    Destroy(creature.GetComponent<CharacterDrop>());
                }
                if (!VFConfig.EnableBossDrops.Value && bosses.Contains(hoard.creature))
                {
                    Destroy(creature.GetComponent<CharacterDrop>());
                }
                // Add the rewards tracker, and set the reference shrine
                creature.AddComponent<CreatureTracker>();
                creature.GetComponent<CreatureTracker>().SetShrine(shrine);
                // Add the tracker script & set the shrine gameobject
                BaseAI ai = creature.GetComponent<BaseAI>();
                if (ai != null)
                {
                    ai.SetHuntPlayer(true);
                }
                if (hoard.stars > 0)
                {
                    Character character = creature.GetComponent<Character>();
                    if ((bool)character)
                    {
                        character.SetLevel(hoard.stars);
                    }
                }
            }

            shrine.GetComponent<Shrine>().StartChallengeMode();
            shrine.GetComponent<Shrine>().Disableportal();
            // Jotunn.Logger.LogDebug("Spawned: " + hoard.creature);
            yield break;
        }
    }
}
