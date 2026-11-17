using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using ValheimFortress.Challenge;

namespace ValheimFortress
{
    internal class VFLocations
    {
        public VFLocations(AssetBundle EmbeddedResourceBundle, VFConfig cfg) 
        {

            // Meadows wild shrine
            AddWildShrineLocationWithWorldGen(cfg,
                EmbeddedResourceBundle.LoadAsset<GameObject>($"Assets/Custom/Locations/VFortress/VF_wild_shrine_green1.prefab"),
                Heightmap.Biome.Meadows
                );
            // Black forest wild shrine
            AddWildShrineLocationWithWorldGen(cfg,
                EmbeddedResourceBundle.LoadAsset<GameObject>($"Assets/Custom/Locations/VFortress/VF_wild_shrine_blue1.prefab"),
                Heightmap.Biome.BlackForest
                );
            // Swamp wild shrine
            AddWildShrineLocationWithWorldGen(cfg,
                EmbeddedResourceBundle.LoadAsset<GameObject>($"Assets/Custom/Locations/VFortress/VF_wild_shrine_green2.prefab"),
                Heightmap.Biome.Swamp
                );
            // Mountain wild shrine
            AddWildShrineLocationWithWorldGen(cfg,
                EmbeddedResourceBundle.LoadAsset<GameObject>($"Assets/Custom/Locations/VFortress/VF_wild_shrine_blue2.prefab"),
                Heightmap.Biome.Mountain
                );
            // Plains wild shrine
            AddWildShrineLocationWithWorldGen(cfg,
                EmbeddedResourceBundle.LoadAsset<GameObject>($"Assets/Custom/Locations/VFortress/VF_wild_shrine_yellow1.prefab"),
                Heightmap.Biome.Plains
                );
            // Mistlands wild shrine
            AddWildShrineLocationWithWorldGen(cfg,
                EmbeddedResourceBundle.LoadAsset<GameObject>($"Assets/Custom/Locations/VFortress/VF_wild_shrine_purple1.prefab"),
                Heightmap.Biome.Mistlands
                );
            // Ashlands wild shrine
            AddWildShrineLocationWithWorldGen(cfg,
                EmbeddedResourceBundle.LoadAsset<GameObject>($"Assets/Custom/Locations/VFortress/VF_wild_shrine_red1.prefab"),
                Heightmap.Biome.AshLands
                );
            // Deep North wild shrine
            AddWildShrineLocationWithWorldGen(cfg,
                EmbeddedResourceBundle.LoadAsset<GameObject>($"Assets/Custom/Locations/VFortress/VF_wild_shrine_blue3.prefab"),
                Heightmap.Biome.DeepNorth
                );
        }

        public void AddWildShrineLocationWithWorldGen(VFConfig cfg, GameObject prefab, Heightmap.Biome biome)
        {
            if (prefab == null)
            {
                Jotunn.Logger.LogWarning($"A {biome} wild shrine is missing from the asset bundle; it is not placed.");
                return;
            }
            bool wildshrine_enabled = VFConfig.BindServerConfig($"Wild Shrines", $"{prefab.name}-Enable", true, $"Enable/Disable the {prefab.name} wildshrine.").Value;
            if (wildshrine_enabled != true) {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Skipped loading location {prefab.name}"); }
                return;
            }

            prefab.AddComponent<WildShrine>();
            prefab.AddComponent<Spawner>();

            LocationConfig spawnerConfig = new LocationConfig();
            spawnerConfig.Biome = biome;
            spawnerConfig.Quantity = VFConfig.NumberOfEachWildShrine.Value;
            spawnerConfig.Priotized = false;
            spawnerConfig.ExteriorRadius = 5f;
            spawnerConfig.SlopeRotation = true;
            spawnerConfig.MinAltitude = 1f;
            spawnerConfig.ClearArea = true;
            spawnerConfig.RandomRotation = false;
            spawnerConfig.MinDistanceFromSimilar = VFConfig.DistanceBetweenShrines.Value;

            ZoneManager.Instance.AddCustomLocation(new CustomLocation(prefab, true, spawnerConfig));

        }
    }
}
