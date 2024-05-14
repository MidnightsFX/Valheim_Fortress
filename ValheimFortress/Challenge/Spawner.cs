using Jotunn.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ValheimFortress.Challenge
{
    // Potentially refactor into base class and customized classes
    // Expose more configurations around spawn timing
    class Spawner : MonoBehaviour
    {
        private static int CurrentCompletedHordes = 0;
        private static void CompleteHorde()
        {
            CurrentCompletedHordes ++;
        }

        public int GetCurrentHorde() { return CurrentCompletedHordes; }

        public void TrySpawningPhase(float wave_regroup_duration, bool send_message, List<HoardConfig> phase_hoard_configs, GameObject shrine, Vector3[] remote_spawn_locations)
        {
            Jotunn.Logger.LogInfo($"Attempting phase spawn with {phase_hoard_configs.Count} hordes.");
            // Jotunn.Logger.LogInfo($"Starting Coroutine with: wave_regroup-{wave_regroup_duration}, send_message-{send_message}, phase_hoard_configs-{phase_hoard_configs}, shrine-{shrine}, remote_spawn_locations-{remote_spawn_locations}");
            StartCoroutine(SpawnPhaseController(wave_regroup_duration, send_message, phase_hoard_configs, shrine, remote_spawn_locations));
        }

        IEnumerator SpawnPhaseController(float wave_regroup_duration, bool send_message, List<HoardConfig> phase_hoard_configs, GameObject shrine, Vector3[] remote_spawn_locations)
        {
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Starting phase, message while we wait? {send_message}"); }
            if (send_message == true)
            {
                UserInterfaceData.PhasePausePhrase(shrine);
            }
            yield return new WaitForSeconds(wave_regroup_duration);

            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Starting Phase spawn."); }
            StartCoroutine(SpawnPhase(phase_hoard_configs, shrine.GetComponent<GenericShrine>(), remote_spawn_locations));
            yield break;
        }

        IEnumerator SpawnPhase(List<HoardConfig> phase_hoard_configs, GenericShrine shrine, Vector3[] remote_spawn_locations)
        {
            foreach (HoardConfig hoard in phase_hoard_configs)
            {
                StartCoroutine(SpawnHorde(hoard, shrine, remote_spawn_locations));
            }
            // wait for all of the hordes to get spawned, then we complete the phase
            while(CurrentCompletedHordes <= phase_hoard_configs.Count)
            {
                if (CurrentCompletedHordes >= phase_hoard_configs.Count) { break; }
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Comparing completed hordes: {CurrentCompletedHordes} to total: {phase_hoard_configs.Count}"); }
                yield return new WaitForSeconds(5);
            }
            CurrentCompletedHordes = 0;
            // record the wave completed
            try { shrine.phaseCompleted(); } catch { }


            yield break;
        }

        IEnumerator SpawnHorde(HoardConfig hoard, GenericShrine shrine, Vector3[] remote_spawn_locations)
        {
            int spawn_failures = 0;
            int spawn_segment_size = UnityEngine.Random.Range(6, 12);
            if (hoard.amount < 6) { spawn_segment_size = 1; }
            int pause_between_segments = UnityEngine.Random.Range(2, 5);
            int amount_spawned = 0;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Starting hoardspawn for {hoard.creature} - {hoard.amount}"); }
            GameObject gameObject = PrefabManager.Instance.GetPrefab(hoard.prefab);
            for (int i = 0; i < hoard.amount; i++)
            {
                if (amount_spawned >= spawn_segment_size)
                {
                    amount_spawned = 0;
                    yield return new WaitForSeconds(pause_between_segments);
                }
                // Randomize the rotation of this individual spawn, this helps prevent spawn towers
                Quaternion rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
                // Randomly select one of the 3 spawn portals as the location for this mob to come out of
                int spawn_location_selected = UnityEngine.Random.Range(0, 3);
                if (spawn_failures > 10)
                {
                    Jotunn.Logger.LogWarning($"Too many spawn failures when trying to spawn {hoard.creature} wave.");
                    break;
                }
                GameObject creature = UnityEngine.Object.Instantiate(gameObject, remote_spawn_locations[spawn_location_selected], rotation);
                // Attempt to set the stars, it is possible that this will fail
                if (hoard.stars > 0)
                {
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Upgrading {hoard.creature} to {hoard.stars} stars."); }
                    Character creature_character = creature.GetComponent<Character>();
                    if ((bool)creature_character) { creature_character.m_level = (hoard.stars + 1); }
                }

                // Enable drops for hoard creatures or bosses, if configured, else destroy
                if (Levels.SpawnableCreatures[hoard.creature].dropsEnabled == false)
                {
                    Destroy(creature.GetComponent<CharacterDrop>());
                }

                // Set the creatures to the same faction so they don't fight each other
                Humanoid creature_metadata = creature.GetComponent<Humanoid>();
                if (creature_metadata != null)
                {
                    creature_metadata.m_faction = Character.Faction.Boss;
                }
                else
                {
                    // This creatures faction isn't set so destroy it and try again.
                    Destroy(creature);
                    i = i - 1;
                    spawn_failures++;
                    continue;
                }


                // Set the AI to hunt the nearby player
                BaseAI ai = creature.GetComponent<BaseAI>();
                if (ai != null)
                {
                    ai.SetHuntPlayer(true);
                }
                else
                {
                    // This creatures AI isn't set to target the player, and it won't go on the attack
                    Destroy(creature);
                    i = i - 1;
                    spawn_failures++;
                    continue;
                }

                // Add the rewards tracker, and set the reference shrine
                creature.AddComponent<CreatureTracker>();
                creature.GetComponent<CreatureTracker>().SetShrine(shrine);
                creature.GetComponent<CreatureTracker>().setCreatureName(hoard.prefab);
                try
                {
                    shrine.addEnemy(creature);
                    shrine.IncrementSpawned();
                }
                catch { }

                amount_spawned++;
            }
            CompleteHorde();
            yield break;
        }
    }
}
