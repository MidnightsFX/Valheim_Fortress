using Jotunn;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimFortress.Challenge
{
    internal class RemoteLocationPortals : MonoBehaviour
    {
        public static List<GameObject> DrawMapOverlayAndPortals(Vector3[] remote_spawns, GenericShrine shrine)
        {
            List<GameObject> portals = new List<GameObject> { };

            int circle_radius = 32;
            // MapDrawing attackOverlay = MinimapManager.Instance.GetMapDrawing("AttackOverlay");
            // Color color = Color.magenta;
            Color[] colorPixels = new Color[circle_radius * circle_radius].Populate(Color.magenta);

            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Starting spawn portal placement."); }
            foreach (Vector3 spawn_location in remote_spawns)
            {
                Quaternion rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
                var tempportal = UnityEngine.Object.Instantiate(ValheimFortress.getPortal(), spawn_location, rotation);
                portals.Add(tempportal);
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Created spawn portal"); }
                // We are going to manipulate the lifetime of this object outside of its timed life.
                Destroy(tempportal.GetComponent<TimedDestruction>());
                tempportal.AddComponent<PortalTracker>();
                tempportal.GetComponent<PortalTracker>().SetShrine(shrine); ;
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Added portal tracker and removed timed life."); }
                if (VFConfig.EnableMapPings.Value) { Chat.instance.SendPing(spawn_location); }
                // attackOverlay.ForestFilter.SetPixels((int)spawn_location.x, (int)spawn_location.y, circle_radius, circle_radius, colorPixels);
            }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Done placing portals"); }
            // Apply the overlay and start the coroutine for deleting it in the future
            // attackOverlay.ForestFilter.Apply();
            // StartCoroutine(CleanUpSpawnNotifiers(portals));
            return portals;
        }

        static IEnumerator CleanUpSpawnNotifiers(List<GameObject> portals)
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

        // This will need multiple invocation types if I end up using multiple spawn types for various shrines
        public static IEnumerator DetermineRemoteSpawnLocations(GameObject shrine, GenericShrine challengeShrine)
        {
            LayerMask terrain_lmsk = LayerMask.GetMask("terrain");
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
            float max_difference_from_ground = 1f;


            List<Vector3> spawn_locations = new List<Vector3>();
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Starting spawn destination in incrments of {range_increment} from x{shrine_gladiator_spawn_position.x} y{shrine_gladiator_spawn_position.y} z{shrine_gladiator_spawn_position.z}"); }
            int spawn_location_attempts = 0;
            // We want three remote spawn locations, each one will be a wave
            while (spawn_location_attempts < 100 && spawn_locations.Count < 3)
            {
                if ((spawn_location_attempts % 10) == 0 && spawn_location_attempts > 1)
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
                Physics.Raycast(potential_spawn + Vector3.up * 1f, Vector3.down, out var terrain_hit, 1000f, terrain_lmsk);
                float terrain_diff = terrain_hit.point.y - potential_spawn.y;
                if (Math.Abs(terrain_diff) > max_difference_from_ground)
                {
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Potential spawn location x:{potential_spawn.x},y:{potential_spawn.y},z:{potential_spawn.z} was significantly different than terrain y:{terrain_hit.point.y}. Retrying."); }
                    continue;
                }
                // Don't spawn in players bases
                if ((bool)EffectArea.IsPointInsideArea(potential_spawn, EffectArea.Type.PlayerBase))
                {
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Potential spawn location x:{potential_spawn.x},y:{potential_spawn.y},z:{potential_spawn.z} was in a player base. Retrying."); }
                    continue;
                }
                // This is a Y check which prevents spawns in a body of water
                if (potential_spawn.y < 27) {
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Potential spawn location x:{potential_spawn.x},y:{potential_spawn.y},z:{potential_spawn.z} was below sea level y:27. Retrying."); }
                    continue; 
                } 
                // if (potential_spawn.y > max_y_difference || potential_spawn.y < min_y_difference) { continue; } // skip spawn setups which have a massive difference in height, this helps prevent portals ontop of pines

                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Valid spawn location determined x:{potential_spawn.x} y:{potential_spawn.y} z:{potential_spawn.z}."); }
                spawn_locations.Add(potential_spawn);
            }
            Jotunn.Logger.LogInfo("Spawn locations determined");
            // If we found at least one remote location, build out the set of 3 using it.
            if (spawn_locations.Count > 0 && spawn_locations.Count < 3)
            {
                Jotunn.Logger.LogInfo("Determined spawn locations was missing 1 or more entries. Multiple waves will spawn from the same location.");
                while (spawn_locations.Count < 2)
                {
                    spawn_locations.Add(spawn_locations[0]);
                }
            }

            // If we never found a valid remote location, log it and set the spawn to the gladiator point.
            if (spawn_locations.Count == 0)
            {
                // This is a fallback to use the shrines central spawn location if we can't determine anywhere else valid to spawn.
                Jotunn.Logger.LogWarning("No Valid remote spawn locations found, will force-spawn on the shrine.");
                Vector3[] gladiator_spawn = { shrine_gladiator_spawn_position, shrine_gladiator_spawn_position, shrine_gladiator_spawn_position };
                challengeShrine.SetWaveSpawnPoints(gladiator_spawn);
                yield break;
            }
            Vector3[] spawn_location_results = spawn_locations.ToArray();
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"spawn Locations [{string.Join(", ", spawn_location_results)}]"); }
            challengeShrine.SetWaveSpawnPoints(spawn_location_results);
            yield break;
        }
    }
}
