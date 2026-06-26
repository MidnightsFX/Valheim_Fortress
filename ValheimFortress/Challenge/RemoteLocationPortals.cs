using Jotunn;
using Splatform;
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
        // Vanilla map markers placed at active challenge spawn points -- the same markers SLS raids use: the
        // shaded EventArea image plus the animated RandomEvent icon. Tracked here so they can be removed when
        // the run ends. Pins are client-local, so this set lives on whatever instance drives the run.
        private static readonly List<Minimap.PinData> active_spawn_pins = new List<Minimap.PinData>();
        // Diameter (world units) of the shaded EventArea circle drawn at each spawn point.
        private const float SpawnPinAreaDiameter = 80f;

        public static List<GameObject> DrawMapOverlayAndPortals(Vector3[] remote_spawns, GenericShrine shrine, bool drawOverlay = true)
        {
            List<GameObject> portals = new List<GameObject> { };

            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Starting spawn portal placement."); }
            foreach (Vector3 spawn_location in remote_spawns)
            {
                Quaternion rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
                var tempportal = UnityEngine.Object.Instantiate(ValheimFortress.getPortal(), spawn_location, rotation);
                portals.Add(tempportal);
                // We are going to manipulate the lifetime of this object outside of its timed life.
                Destroy(tempportal.GetComponent<TimedDestruction>());
                tempportal.AddComponent<PortalTracker>();
                tempportal.GetComponent<PortalTracker>().SetShrine(shrine);
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Created portal at {spawn_location}."); }
                if (VFConfig.EnableMapPings.Value) { Chat.instance.SendPing(spawn_location); }
            }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Done placing portals"); }

            if (drawOverlay) { DrawSpawnLocationOverlay(remote_spawns, shrine.transform.position); }
            return portals;
        }

        // Marks each remote spawn location on the minimap with the vanilla event markers used by SLS raids: a
        // shaded EventArea circle plus the animated RandomEvent icon. Existing markers are cleared first so only
        // the current run's points show. Map pins are client-local and cosmetic, so this is a no-op on a
        // headless/dedicated instance (no Minimap) -- the markers render for whatever instance owns/drives the
        // run (single-player or the P2P host).
        public static void DrawSpawnLocationOverlay(Vector3[] remote_spawns, Vector3 origin)
        {
            if (remote_spawns == null || remote_spawns.Length == 0) { return; }
            if (Minimap.instance == null) { return; }

            ClearMapOverlay();

            // The shaded event-area image (the vanilla raid/event circle).
            Minimap.PinData area_pin = Minimap.instance.AddPin(origin, Minimap.PinType.EventArea, "", false, false, author: new PlatformUserID());
            area_pin.m_worldSize = SpawnPinAreaDiameter;
            active_spawn_pins.Add(area_pin);

            foreach (Vector3 spawn_location in remote_spawns) {
                // The animated random-event icon.
                Minimap.PinData icon_pin = Minimap.instance.AddPin(spawn_location, Minimap.PinType.RandomEvent, "", false, false, author: new PlatformUserID());
                icon_pin.m_animate = true;
                icon_pin.m_doubleSize = true;
                active_spawn_pins.Add(icon_pin);
            }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Placed spawn-location map pins for {remote_spawns.Length} location(s)."); }
        }

        // Removes the spawn-location map markers placed by DrawSpawnLocationOverlay. Safe to call from any
        // instance and whether or not any markers were placed. Invoked when a challenge ends or is cancelled.
        public static void ClearMapOverlay()
        {
            if (active_spawn_pins.Count == 0) { return; }
            if (Minimap.instance != null)
            {
                foreach (Minimap.PinData pin in active_spawn_pins)
                {
                    if (pin != null) { Minimap.instance.RemovePin(pin); }
                }
            }
            active_spawn_pins.Clear();
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
            // How many distinct remote spawn points to look for; creatures emerge from a randomly chosen one.
            int target_spawn_points = Mathf.Max(1, VFConfig.NumberOfRemoteSpawnPoints.Value);
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Starting spawn destination in incrments of {range_increment} from x{shrine_gladiator_spawn_position.x} y{shrine_gladiator_spawn_position.y} z{shrine_gladiator_spawn_position.z}"); }
            int spawn_location_attempts = 0;
            // We want target_spawn_points remote spawn locations to draw portals/waves from.
            while (spawn_location_attempts < 100 && spawn_locations.Count < target_spawn_points)
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
            // The spawner randomly indexes into whatever it's given, so any count >= 1 is fine; we don't need
            // to pad up to the target. If we found fewer than requested, creatures simply share those points.
            if (spawn_locations.Count > 0 && spawn_locations.Count < target_spawn_points) {
                Jotunn.Logger.LogInfo($"Found {spawn_locations.Count} of {target_spawn_points} requested spawn locations. Creatures will share the available points.");
            }

            // If we never found a valid remote location, log it and set the spawn to the gladiator point.
            if (spawn_locations.Count == 0)
            {
                // This is a fallback to use the shrines central spawn location if we can't determine anywhere else valid to spawn.
                Jotunn.Logger.LogWarning("No Valid remote spawn locations found, will force-spawn on the shrine.");
                Vector3[] gladiator_spawn = { shrine_gladiator_spawn_position };
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
