using System;
using System.Collections.Generic;
using ValheimFortress.Challenge;

namespace ValheimFortress.Data
{
    public class Monsters
    {
        const String COMMON = CONST.COMMON;
        const String RARE = CONST.RARE;
        const String ELITE = CONST.ELITE;
        const String UNIQUE = CONST.UNIQUE;
        const String MEADOWS = CONST.MEADOWS;
        const String BLACKFOREST = CONST.BLACKFOREST;
        const String SWAMP = CONST.SWAMP;
        const String MOUNTAIN = CONST.MOUNTAIN;
        const String PLAINS = CONST.PLAINS;
        const String MISTLANDS = CONST.MISTLANDS;
        const String ASHLANDS = CONST.ASHLANDS;

        public static Dictionary<String, CreatureValues> SpawnableCreatures = new Dictionary<string, CreatureValues>
        {
            // Meadow Creatures
            {"Neck", new CreatureValues { spawnCost = 2, prefabName = "Neck", spawnType = COMMON, biome = MEADOWS, enabled = true, dropsEnabled = false } },
            {"Boar", new CreatureValues {spawnCost = 2, prefabName = "Boar", spawnType = COMMON, biome = MEADOWS, enabled = true, dropsEnabled = false } },
            {"Deer", new CreatureValues {spawnCost = 2, prefabName = "Deer", spawnType = COMMON, biome = MEADOWS, enabled = false, dropsEnabled = false } },
            {"Greyling", new CreatureValues {spawnCost = 3, prefabName = "Greyling", spawnType = COMMON, biome = MEADOWS, enabled = true, dropsEnabled = false } },
            // Black Forest Creatures
            {"GreyDwarf", new CreatureValues {spawnCost = 4, prefabName = "Greydwarf", spawnType = COMMON, biome = BLACKFOREST, enabled = true, dropsEnabled = false } },
            {"GreyDwarfBrute", new CreatureValues {spawnCost = 8, prefabName = "Greydwarf_Elite", spawnType = RARE, biome = BLACKFOREST, enabled = true, dropsEnabled = false } },
            {"GreyDwarfShaman", new CreatureValues {spawnCost = 8, prefabName = "Greydwarf_Shaman", spawnType = RARE, biome = BLACKFOREST, enabled = true, dropsEnabled = false } },
            {"Skeleton", new CreatureValues {spawnCost = 4, prefabName = "Skeleton_NoArcher", spawnType = COMMON, biome = BLACKFOREST, enabled = true, dropsEnabled = false } },
            {"SkeletonArcher", new CreatureValues {spawnCost = 5, prefabName = "Skeleton", spawnType = COMMON, biome = BLACKFOREST, enabled = true, dropsEnabled = false } },
            {"RancidSkeleton", new CreatureValues {spawnCost = 9, prefabName = "Skeleton_Poison", spawnType = RARE, biome = BLACKFOREST, enabled = true, dropsEnabled = false } },
            {"Ghost", new CreatureValues {spawnCost = 7, prefabName = "Ghost", spawnType = RARE, biome = BLACKFOREST, enabled = true, dropsEnabled = false } },
            {"Troll", new CreatureValues {spawnCost = 20, prefabName = "Troll", spawnType = ELITE, biome = BLACKFOREST, enabled = true, dropsEnabled = false } },
            // Swamp Creatures
            {"Surtling", new CreatureValues {spawnCost = 6, prefabName = "Surtling", spawnType = RARE, biome = SWAMP, enabled = true, dropsEnabled = false} },
            {"Leech", new CreatureValues {spawnCost = 8, prefabName = "Leech", spawnType = COMMON, biome = SWAMP, enabled = false, dropsEnabled = false} },
            {"Wraith", new CreatureValues {spawnCost = 10, prefabName = "Wraith", spawnType = RARE, biome = SWAMP, enabled = true, dropsEnabled = false} },
            {"Abomination", new CreatureValues {spawnCost = 30, prefabName = "Abomination", spawnType = ELITE, biome = SWAMP, enabled = true, dropsEnabled = false} },
            {"Draugr", new CreatureValues {spawnCost = 10, prefabName = "Draugr", spawnType = COMMON, biome = SWAMP, enabled = true, dropsEnabled = false} },
            {"DraugrArcher", new CreatureValues {spawnCost = 20, prefabName = "Draugr_Ranged", spawnType = RARE, biome = SWAMP, enabled = true, dropsEnabled = false} },
            {"DraugrElite", new CreatureValues {spawnCost = 15, prefabName = "Draugr_Elite", spawnType = RARE, biome = SWAMP, enabled = true, dropsEnabled = false} },
            {"Blob", new CreatureValues {spawnCost = 7, prefabName = "Blob", spawnType = COMMON, biome = SWAMP, enabled = true, dropsEnabled = false} },
            {"BlobElite", new CreatureValues {spawnCost = 15, prefabName = "BlobElite", spawnType = RARE, biome = SWAMP, enabled = true, dropsEnabled = false} },
            // Mountain Creatures
            {"Bat", new CreatureValues {spawnCost = 3, prefabName = "Bat", spawnType = COMMON, biome = MOUNTAIN, enabled = false, dropsEnabled = false} },
            {"IceDrake", new CreatureValues {spawnCost = 25, prefabName = "Hatchling", spawnType = RARE, biome = MOUNTAIN, enabled = true, dropsEnabled = false} },
            {"Wolf", new CreatureValues {spawnCost = 18, prefabName = "Wolf", spawnType = COMMON, biome = MOUNTAIN, enabled = true, dropsEnabled = false} },
            {"Fenring", new CreatureValues {spawnCost = 28, prefabName = "Fenring", spawnType = RARE, biome = MOUNTAIN, enabled = true, dropsEnabled = false} },
            {"Ulv", new CreatureValues {spawnCost = 20, prefabName = "Ulv", spawnType = COMMON, biome = MOUNTAIN, enabled = true, dropsEnabled = false} },
            {"Cultist", new CreatureValues {spawnCost = 40, prefabName = "Fenring_Cultist", spawnType = RARE, biome = MOUNTAIN, enabled = true, dropsEnabled = false} },
            {"StoneGolem", new CreatureValues {spawnCost = 50, prefabName = "StoneGolem", spawnType = ELITE, biome = MOUNTAIN, enabled = true, dropsEnabled = false} },
            // Plains Creatures
            {"Deathsquito", new CreatureValues {spawnCost = 20, prefabName = "Deathsquito", spawnType = COMMON, biome = PLAINS, enabled = true, dropsEnabled = false} },
            {"Fuling", new CreatureValues {spawnCost = 15, prefabName = "Goblin", spawnType = COMMON, biome = PLAINS, enabled = true, dropsEnabled = false} },
            {"FulingArcher", new CreatureValues {spawnCost = 20, prefabName = "GoblinArcher", spawnType = COMMON, biome = PLAINS, enabled = true, dropsEnabled = false} },
            {"FulingBerserker", new CreatureValues {spawnCost = 45, prefabName = "GoblinBrute", spawnType = ELITE, biome = PLAINS, enabled = true, dropsEnabled = false} },
            {"FulingShaman", new CreatureValues {spawnCost = 40, prefabName = "GoblinShaman", spawnType = RARE, biome = PLAINS, enabled = true, dropsEnabled = false} },
            {"Growth", new CreatureValues {spawnCost = 35, prefabName = "BlobTar", spawnType = RARE, biome = PLAINS, enabled = true, dropsEnabled = false} },
            // Mistland Creatures
            {"Seeker", new CreatureValues {spawnCost = 30, prefabName = "Seeker", spawnType = COMMON, biome = MISTLANDS, enabled = true, dropsEnabled = false} },
            {"SeekerSoldier", new CreatureValues {spawnCost = 75, prefabName = "SeekerBrute", spawnType = ELITE, biome = MISTLANDS, enabled = true, dropsEnabled = false} },
            {"SeekerBrood", new CreatureValues {spawnCost = 10, prefabName = "SeekerBrood", spawnType = COMMON, biome = MISTLANDS, enabled = true, dropsEnabled = false} },
            {"Gjall", new CreatureValues {spawnCost = 75, prefabName = "Gjall", spawnType = ELITE, biome = MISTLANDS, enabled = true, dropsEnabled = false} },
            {"Tick", new CreatureValues {spawnCost = 15, prefabName = "Tick", spawnType = COMMON, biome = MISTLANDS, enabled = true, dropsEnabled = false} },
            {"DvergerRouge", new CreatureValues {spawnCost = 40, prefabName = "Dverger", spawnType = RARE, biome = MISTLANDS, enabled = true, dropsEnabled = false} },
            {"DvergerMage", new CreatureValues {spawnCost = 75, prefabName = "DvergerMage", spawnType = RARE, biome = MISTLANDS, enabled = true, dropsEnabled = false} },
            {"DvergerMageFire", new CreatureValues {spawnCost = 75, prefabName = "DvergerMageFire", spawnType = RARE, biome = MISTLANDS, enabled = false, dropsEnabled = false } },
            {"DvergerMageIce", new CreatureValues {spawnCost = 75, prefabName = "DvergerMageIce", spawnType = RARE, biome = MISTLANDS, enabled = false, dropsEnabled = false } },
            {"DvergerMageSupport", new CreatureValues {spawnCost = 75, prefabName = "DvergerMageSupport", spawnType = ELITE, biome = MISTLANDS, enabled = true, dropsEnabled = false} },
            // Ashlands Creatures
            {"Asksvin", new CreatureValues {spawnCost = 70, prefabName = "Asksvin", spawnType = RARE, biome = ASHLANDS, enabled = true, dropsEnabled = false} },
            {"Charred_Archer", new CreatureValues {spawnCost = 60, prefabName = "Charred_Archer", spawnType = COMMON, biome = ASHLANDS, enabled = true, dropsEnabled = false} },
            {"Charred_Twitcher", new CreatureValues {spawnCost = 45, prefabName = "Charred_Twitcher", spawnType = COMMON, biome = ASHLANDS, enabled = true, dropsEnabled = false} },
            {"Charred_Mage", new CreatureValues {spawnCost = 100, prefabName = "Charred_Mage", spawnType = ELITE, biome = ASHLANDS, enabled = true, dropsEnabled = false} },
            {"Charred_Melee", new CreatureValues {spawnCost = 75, prefabName = "Charred_Melee", spawnType = RARE, biome = ASHLANDS, enabled = true, dropsEnabled = false} },
            {"FallenValkyrie", new CreatureValues {spawnCost = 100, prefabName = "FallenValkyrie", spawnType = ELITE, biome = ASHLANDS, enabled = true, dropsEnabled = false} },
            {"BlobLava", new CreatureValues {spawnCost = 75, prefabName = "BlobLava", spawnType = RARE, biome = ASHLANDS, enabled = true, dropsEnabled = false} },
            {"Morgen", new CreatureValues {spawnCost = 85, prefabName = "Morgen", spawnType = ELITE, biome = ASHLANDS, enabled = true, dropsEnabled = false} },
            {"Volture", new CreatureValues {spawnCost = 30, prefabName = "Volture", spawnType = COMMON, biome = ASHLANDS, enabled = true, dropsEnabled = false} },
            // Boss Creatures
            {"Eikthyr", new CreatureValues {spawnCost = 40, prefabName = "Eikthyr", spawnType = UNIQUE, biome = MEADOWS, enabled = true, dropsEnabled = false} },
            {"TheElder", new CreatureValues {spawnCost = 180, prefabName = "gd_king", spawnType = UNIQUE, biome = BLACKFOREST, enabled = true, dropsEnabled = false} },
            {"Bonemass", new CreatureValues {spawnCost = 250, prefabName = "Bonemass", spawnType = UNIQUE, biome = SWAMP, enabled = true, dropsEnabled = false} },
            {"Moder", new CreatureValues {spawnCost = 320, prefabName = "Dragon", spawnType = UNIQUE, biome = MOUNTAIN, enabled = true, dropsEnabled = false} },
            {"Yagluth", new CreatureValues {spawnCost = 450, prefabName = "GoblinKing", spawnType = UNIQUE, biome = PLAINS, enabled = true, dropsEnabled = false} },
            {"TheQueen", new CreatureValues {spawnCost = 600, prefabName = "SeekerQueen", spawnType = UNIQUE, biome = MISTLANDS, enabled = true, dropsEnabled = false} },
            {"Fader", new CreatureValues {spawnCost = 800, prefabName = "Fader", spawnType = UNIQUE, biome = ASHLANDS, enabled = true, dropsEnabled = false} },
        };

        public static void UpdateSpawnableCreatures(SpawnableCreatureCollection spawnables)
        {
            SpawnableCreatures.Clear();
            foreach (KeyValuePair<string, CreatureValues> entry in spawnables.Creatures)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Updating Creature Entry {entry.Key} Prefab:{entry.Value.prefabName} SpawnCost:{entry.Value.spawnCost} Biome:{entry.Value.biome} SpawnType:{entry.Value.spawnType}"); }
                SpawnableCreatures.Add(entry.Key, entry.Value);
            }
        }

        public static string YamlCreatureDefinition()
        {
            var creatureCollection = new SpawnableCreatureCollection();
            creatureCollection.Creatures = SpawnableCreatures;
            var yaml = CONST.yamlserializer.Serialize(creatureCollection);
            return yaml;
        }
    }
}
