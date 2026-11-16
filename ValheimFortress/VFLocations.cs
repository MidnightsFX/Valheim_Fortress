using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using ValheimFortress.Challenge;

namespace ValheimFortress
{
    // I'll figure out how tf this should be structured once its gotten some stuff in it
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
            // Ashlands wild shrine placeholder
            AddWildShrineLocationWithWorldGen(cfg,
                LoadShrineOrPlaceholder(EmbeddedResourceBundle, "VF_wild_shrine_red1", "VF_wild_shrine_purple1"),
                Heightmap.Biome.AshLands
                );
            // DeepNorth wild shrine placeholder
            AddWildShrineLocationWithWorldGen(cfg,
                LoadShrineOrPlaceholder(EmbeddedResourceBundle, "VF_wild_shrine_blue3", "VF_wild_shrine_purple1"),
                Heightmap.Biome.DeepNorth
                );
        }

        // The shrine's own prefab from the bundle when it is there; otherwise a copy of the placeholder shrine under the
        // shrine's name. The name is what its WildShrines.yaml entry and its enable config are keyed by, so swapping in
        // the real prefab later needs no other change, and shrines already placed in a world keep working.
        private static GameObject LoadShrineOrPlaceholder(AssetBundle bundle, string name, string placeholder)
        {
            GameObject prefab = bundle.LoadAsset<GameObject>($"Assets/Custom/Locations/VFortress/{name}.prefab");
            if (prefab != null) { return prefab; }
            GameObject source = bundle.LoadAsset<GameObject>($"Assets/Custom/Locations/VFortress/{placeholder}.prefab");
            if (source == null) { return null; }
            Jotunn.Logger.LogInfo($"{name} is not in the asset bundle yet, using a copy of {placeholder}.");
            GameObject copy = ZoneManager.Instance.CreateLocationContainer(source);
            copy.name = name;
            return copy;
        }

        public void AddWildShrineLocationWithWorldGen(VFConfig cfg, GameObject prefab, Heightmap.Biome biome)
        {
            if (prefab == null) { return; }
            bool wildshrine_enabled = VFConfig.BindServerConfig($"Wild Shrines", $"{prefab.name}-Enable", true, $"Enable/Disable the {prefab.name} wildshrine.").Value;
            if (wildshrine_enabled != true) {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Skipped loading location {prefab.name}"); }
                return;
            }

            if (prefab.GetComponent<WildShrine>() == null) { prefab.AddComponent<WildShrine>(); }
            if (prefab.GetComponent<Spawner>() == null) { prefab.AddComponent<Spawner>(); }

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
