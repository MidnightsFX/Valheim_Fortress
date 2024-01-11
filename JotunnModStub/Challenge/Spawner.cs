using Jotunn;
using Jotunn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
using static Jotunn.Managers.MinimapManager;

namespace ValheimFortress.Challenge
{
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
            StartCoroutine(SpawnPhase(phase_hoard_configs, shrine, remote_spawn_locations));
            yield break;
        }

        IEnumerator SpawnPhase(List<HoardConfig> phase_hoard_configs, GameObject shrine, Vector3[] remote_spawn_locations)
        {
            foreach (HoardConfig hoard in phase_hoard_configs)
            {
                StartCoroutine(SpawnHorde(hoard, shrine,remote_spawn_locations));
            }
            // wait for all of the hordes to get spawned, then we complete the phase
            while(CurrentCompletedHordes <= phase_hoard_configs.Count)
            {
                if (CurrentCompletedHordes >= phase_hoard_configs.Count) { break; }
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Comparing completed hordes: {CurrentCompletedHordes} to total: {phase_hoard_configs.Count}"); }
                yield return new WaitForSeconds(5);
            }
            CurrentCompletedHordes = 0;
            shrine.GetComponent<Shrine>().phaseCompleted();
            yield break;
        }

        IEnumerator SpawnHorde(HoardConfig hoard, GameObject shrine, Vector3[] remote_spawn_locations)
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
                shrine.GetComponent<Shrine>().addEnemy(creature);
                shrine.GetComponent<Shrine>().IncrementSpawned();
                creature.AddComponent<CreatureTracker>();
                creature.GetComponent<CreatureTracker>().SetShrine(shrine);

                amount_spawned++;
            }
            CompleteHorde();
            yield break;
        }

        public static List<GameObject> DrawMapOverlayAndPortals(Vector3[] remote_spawns, GameObject shrine)
        {
            List<GameObject> portals = new List<GameObject> { };

            int circle_radius = 32;
            // MapDrawing attackOverlay = MinimapManager.Instance.GetMapDrawing("AttackOverlay");
            // Color color = Color.magenta;
            Color[] colorPixels = new Color[circle_radius * circle_radius].Populate(Color.magenta);

            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Spawn portals started"); }
            foreach (Vector3 spawn_location in remote_spawns)
            {
                Quaternion rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
                var tempportal = UnityEngine.Object.Instantiate(ValheimFortress.getPortal(), spawn_location, rotation);
                portals.Add(tempportal);
                // We are going to manipulate the lifetime of this object outside of its timed life.
                Destroy(tempportal.GetComponent<TimedDestruction>());
                tempportal.AddComponent<PortalTracker>();
                tempportal.GetComponent<PortalTracker>().SetShrine(shrine);
                if (VFConfig.EnableMapPings.Value) { Chat.instance.SendPing(spawn_location); }
                // attackOverlay.ForestFilter.SetPixels((int)spawn_location.x, (int)spawn_location.y, circle_radius, circle_radius, colorPixels);
            }
            // Apply the overlay and start the coroutine for deleting it in the future
            // attackOverlay.ForestFilter.Apply();
            // StartCoroutine(CleanUpSpawnNotifiers(portals));
            return portals;
        }

        IEnumerator CleanUpSpawnNotifiers(List<GameObject> portals)
        {
            // This delay should be ~about the same
            yield return new WaitForSeconds(20f);
            Jotunn.Logger.LogInfo("Removing map drawing and creature spawn portals.");
            foreach (GameObject portal in portals)
            {
                Destroy(portal);
            }
            // mapdrawing.Enabled = false;
            // MinimapManager.Instance.RemoveMapDrawing(mapdrawing.Name);
            yield break;
        }

        public static IEnumerator DetermineRemoteSpawnLocations(GameObject shrine)
        {
            Vector3 shrine_position = shrine.transform.position;
            GameObject shrine_spawnpoint = shrine.transform.Find("spawnpoint").gameObject;
            Vector3 shrine_gladiator_spawn_position = shrine_spawnpoint.transform.position;
            float range_increment = VFConfig.MaxSpawnRange.Value / 10;
            float current_max_x = shrine_position.x + range_increment;
            float current_min_x = shrine_position.x - range_increment;
            float current_max_z = shrine_position.z + range_increment;
            float current_min_z = shrine_position.z - range_increment;
            float min_y_difference = shrine_position.y - 32;
            float max_y_difference = shrine_position.y + 32;



            List<Vector3> spawn_locations =new List<Vector3>();

            if (VFConfig.EnableGladiatorMode.Value)
            {
                // The spawn location is divided up into 3 segments currently so we always need three entries.
                // And we want all three of them to be the shrine itself if we are not spawning enemies remotely.
                Jotunn.Logger.LogInfo($"Using Gladiator spawnpoint {shrine_gladiator_spawn_position}.");
                Vector3[] gladiator_spawn = { shrine_gladiator_spawn_position, shrine_gladiator_spawn_position, shrine_gladiator_spawn_position };
                // Jotunn.Logger.LogInfo($"Spawn Position set to Shrine in gladiator mode [{string.Join(", ", gladiator_spawn)}].");
                shrine.GetComponent<Shrine>().SetWaveSpawnPoints(gladiator_spawn);
                yield break;
            }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Starting spawn destination in incrments of {range_increment} from x{shrine_gladiator_spawn_position.x} y{shrine_gladiator_spawn_position.y} z{shrine_gladiator_spawn_position.z}"); }
            int spawn_location_attempts = 0;
            // We want three remote spawn locations, each one will be a wave
            while(spawn_location_attempts < 100 && spawn_locations.Count < 3)
            {
                if((spawn_location_attempts % 10) == 0 && spawn_location_attempts > 1)
                {
                    current_max_x += range_increment;
                    current_min_x -= range_increment;
                    current_max_z += range_increment;
                    current_min_z -= range_increment;
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Increase spawn range, pausing before more attempts."); }
                    yield return new WaitForSeconds(0.5f);
                }
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Starting spawning attempt with x {current_max_x}<->{current_min_x} z {current_max_z}<->{current_min_z}"); }
                spawn_location_attempts++;
                Vector3 potential_spawn = new Vector3(UnityEngine.Random.Range(current_min_x, current_max_x), 200, UnityEngine.Random.Range(current_min_z, current_max_z));
                float height;
                if (ZoneSystem.instance.FindFloor(potential_spawn, out height))
                {
                    potential_spawn.y = height;
                }
                if((bool)EffectArea.IsPointInsideArea(potential_spawn, EffectArea.Type.PlayerBase)) { continue; } // Don't spawn in players bases
                if(potential_spawn.y < 27) { continue;  } // This is a Y check which prevents spawns in a body of water
                if (potential_spawn.y > max_y_difference || potential_spawn.y < min_y_difference) { continue; } // skip spawn setups which have a massive difference in height, this helps prevent portals ontop of pines

                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Valid spawn location determined: x={potential_spawn.x} y={potential_spawn.y} z={potential_spawn.z}"); }
                spawn_locations.Add(potential_spawn);

            }

            // If we found at least one remote location, build out the set of 3 using it.
            if(spawn_locations.Count > 0 && spawn_locations.Count < 3)
            {
                Jotunn.Logger.LogInfo("Determined spawn locations was missing 1 or more entries. Multiple waves will spawn from the same location.");
                while (spawn_locations.Count < 2)
                {
                    spawn_locations.Add(spawn_locations[0]);
                }
            }

            // If we never found a valid remote location, log it and set the spawn to the gladiator point.
            if(spawn_locations.Count == 0)
            {
                // This is a fallback to use the shrines central spawn location if we can't determine anywhere else valid to spawn.
                while(spawn_locations.Count < 3)
                {
                    spawn_locations.Append(shrine_gladiator_spawn_position);
                }
                Jotunn.Logger.LogWarning("No Valid remote spawn locations found, will force-spawn on the shrine.");
            }
            Vector3[] spawn_location_results = spawn_locations.ToArray();
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"spawn Locations [{string.Join(", ", spawn_location_results)}]"); }
            shrine.GetComponent<Shrine>().SetWaveSpawnPoints(spawn_location_results);
            yield break;
        }
    }
}
