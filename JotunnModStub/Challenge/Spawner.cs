using Jotunn;
using Jotunn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Jotunn.Managers.MinimapManager;

namespace ValheimFortress.Challenge
{
    class Spawner : MonoBehaviour
    {
        static private List<String> bosses = new List<String>(new string[] { "Eikythr", "TheElder", "BoneMass", "Moder", "Yagluth", "TheQueen" });

        public void TrySpawningHoard(List<Levels.HoardConfig> hoards, bool siege_mode, GameObject shrine)
        {
            // Should check if you are the runtime owner of this chunk
            Jotunn.Logger.LogInfo($"Trying to spawn {hoards.Count} hoards.");
            Vector3[] remote_spawn_locations = DetermineRemoteSpawnLocations(shrine);
            shrine.GetComponent<Shrine>().setSpawnedWaveTarget((Int16)hoards.Count);

            if (VFConfig.EnableGladiatorMode.Value == false)
            {
                DrawMapOverlayAndPortals(remote_spawn_locations);
            }

            foreach (Levels.HoardConfig hoard in hoards)
            {
                Jotunn.Logger.LogInfo($"Starting spawn for {hoard.amount} {hoard.creature}");
                StartCoroutine(Spawn(hoard, shrine, siege_mode, remote_spawn_locations));
            }
        }

        IEnumerator Spawn(Levels.HoardConfig hoard, GameObject shrine, bool siege_mode, Vector3[] remote_spawn_locations)
        {
            float wave_spawn_delay = 5f;
            float initial_wait = 0.0f;
            if(VFConfig.EnableGladiatorMode.Value == false) { initial_wait = 5.0f; } else { wave_spawn_delay = wave_spawn_delay * 2; }
            if (siege_mode) { wave_spawn_delay = wave_spawn_delay * 5; } // Increase delay between waves significantly
            yield return new WaitForSeconds(initial_wait);

            GameObject gameObject = PrefabManager.Instance.GetPrefab(hoard.prefab);
            int hoard_frac = hoard.amount / 3;
            bool should_pause_during_horde = true;
            if (hoard_frac == 0) 
            { 
                should_pause_during_horde = false;
                Jotunn.Logger.LogInfo($"Hoard {hoard.creature} does not have any pausepoints, and will spawn immediately.");
            } else {
                Jotunn.Logger.LogInfo($"Hoard {hoard.creature} pausepoints {hoard_frac} {hoard_frac * 2}.");
            }

            int pause_point_1 = hoard_frac;
            int pause_point_2 = (hoard_frac * 2);
            Vector3 spawn_position = remote_spawn_locations[0];
            // Spawn our requested number of creatures, modify them as required.
            for (int i = 0; i < hoard.amount; i++)
            {
                // Update the spawnpoint based on which pausepoint we are at, doesn't matter if we actually pause.
                // We don't change the spawn if there are no pausepoints for this horde (boss horde)
                Quaternion rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
                
                if (i == pause_point_1 && pause_point_1 > 0)
                {
                    spawn_position = remote_spawn_locations[1];
                }
                if (i == pause_point_2 && pause_point_2 > 0)
                {
                    spawn_position = remote_spawn_locations[2];
                }
                
                // This is the fractional pause section. This area is useful because it is only triggered once for each spawn position
                if (should_pause_during_horde && i == hoard_frac || i == (hoard_frac * 2))
                {
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Pausing {hoard.creature} spawning for wave-delay of {wave_spawn_delay} seconds."); }
                    yield return new WaitForSeconds(wave_spawn_delay);
                }
                // This is really verbose
                // if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Spawning {hoard.creature}"); }
                GameObject creature = UnityEngine.Object.Instantiate(gameObject, spawn_position, rotation);
                // creature.GetComponent<ZNetView>(); // but why
                shrine.GetComponent<Shrine>().IncrementSpawned();

                if (hoard.stars > 0)
                {
                    Jotunn.Logger.LogInfo($"Upgrading {hoard.creature} to {hoard.stars} stars.");
                    Character creature_character = creature.GetComponent<Character>();
                    if ((bool)creature_character) { creature_character.m_level = hoard.stars; }
                }
                creature.GetComponent<Humanoid>().m_faction = Character.Faction.Boss;
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
                // Set the AI to hunt the nearby player
                BaseAI ai = creature.GetComponent<BaseAI>();
                if (ai != null)
                {
                    ai.SetHuntPlayer(true);
                }
                
            }

            shrine.GetComponent<Shrine>().WaveSpawned();
            yield break;
        }

        public void DrawMapOverlayAndPortals(Vector3[] remote_spawns)
        {
            Quaternion rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
            List<GameObject> portals = new List<GameObject> { };

            int circle_radius = 32;
            MapDrawing attackOverlay = MinimapManager.Instance.GetMapDrawing("AttackOverlay");
            // Color color = Color.magenta;
            Color[] colorPixels = new Color[circle_radius * circle_radius].Populate(Color.magenta);

            Jotunn.Logger.LogInfo("Spawn portals started");
            foreach (Vector3 spawn_location in remote_spawns)
            {
                var tempportal = UnityEngine.Object.Instantiate(ValheimFortress.getPortal(), spawn_location, rotation);
                portals.Add(tempportal);
                Chat.instance.SendPing(spawn_location);
                attackOverlay.ForestFilter.SetPixels((int)spawn_location.x, (int)spawn_location.y, circle_radius, circle_radius, colorPixels);
            }
            // Apply the overlay and start the coroutine for deleting it in the future
            attackOverlay.ForestFilter.Apply();
            StartCoroutine(CleanUpSpawnNotifiers(attackOverlay, portals));
        }

        IEnumerator CleanUpSpawnNotifiers(MapDrawing mapdrawing, List<GameObject> portals)
        {
            // This delay should be ~about the same
            yield return new WaitForSeconds(20f);
            Jotunn.Logger.LogInfo("Removing map drawing and creature spawn portals.");
            foreach (GameObject portal in portals)
            {
                Destroy(portal);
            }
            mapdrawing.Enabled = false;
            MinimapManager.Instance.RemoveMapDrawing(mapdrawing.Name);
            yield break;
        }

        public Vector3[] DetermineRemoteSpawnLocations(GameObject shrine)
        {
            Vector3 shrine_position = shrine.transform.position;
            GameObject shrine_spawnpoint = shrine.transform.Find("spawnpoint").gameObject;
            Vector3 shrine_gladiator_spawn_position = shrine_spawnpoint.transform.position;
            float range_increment = VFConfig.MaxSpawnRange.Value / 10;
            float current_max_x = shrine_position.x + range_increment;
            float current_min_x = shrine_position.x - range_increment;
            float current_max_z = shrine_position.z + range_increment;
            float current_min_z = shrine_position.z - range_increment;
            

            List<Vector3> spawn_locations =new List<Vector3>();

            if (VFConfig.EnableGladiatorMode.Value)
            {
                // The spawn location is divided up into 3 segments currently so we always need three entries.
                // And we want all three of them to be the shrine itself if we are not spawning enemies remotely.
                spawn_locations.Append(shrine_gladiator_spawn_position);
                spawn_locations.Append(shrine_gladiator_spawn_position);
                spawn_locations.Append(shrine_gladiator_spawn_position);
            }
            Jotunn.Logger.LogInfo($"Starting spawn destination in incrments of {range_increment} from x{shrine_gladiator_spawn_position.x} y{shrine_gladiator_spawn_position.y} z{shrine_gladiator_spawn_position.z}");
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
                }
                Jotunn.Logger.LogInfo($"Starting spawning attempt with x {current_max_x}<->{current_min_x} z {current_max_z}<->{current_min_z}");
                spawn_location_attempts++;
                Vector3 potential_spawn = new Vector3(UnityEngine.Random.Range(current_min_x, current_max_x), 200, UnityEngine.Random.Range(current_min_z, current_max_z));
                float height;
                if (ZoneSystem.instance.FindFloor(potential_spawn, out height))
                {
                    potential_spawn.y = height;
                }
                if((bool)EffectArea.IsPointInsideArea(potential_spawn, EffectArea.Type.PlayerBase)) { continue; } // Don't spawn in players bases
                if(potential_spawn.y < 28) { continue;  } // This is a Y check which prevents spawns in a body of water

                Jotunn.Logger.LogInfo($"Valid spawn location determined: x={potential_spawn.x} y={potential_spawn.y} z={potential_spawn.z}");
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
                Jotunn.Logger.LogInfo("No Valid remote spawn locations found, will force-spawn on the shrine.");
            }
            Vector3[] spawn_location_results = spawn_locations.ToArray();
            Jotunn.Logger.LogInfo($"spawn Locations [{string.Join(", ", spawn_location_results)}]");
            return spawn_location_results;
        }
    }
}
