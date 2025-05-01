using System;
using System.Collections.Generic;
using System.Linq;
using ValheimFortress.Common;
using ValheimFortress.Data;


namespace ValheimFortress.Challenge
{
    class Levels
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
        static readonly private String[] biomes = { MEADOWS, BLACKFOREST, SWAMP, MOUNTAIN, PLAINS, MISTLANDS, ASHLANDS };
        static readonly private String[] spawntypes = { COMMON, RARE, ELITE, UNIQUE };

        // Reference used to build a wave template
        public class WaveOutline
        {
            List<String> selectedCreatures = new List<string>();
            List<HoardConfig> commonCreatures = new List<HoardConfig>();
            List<HoardConfig> rareCreatures = new List<HoardConfig>();
            List<HoardConfig> eliteCreatures = new List<HoardConfig>();
            List<HoardConfig> uniqueCreatures = new List<HoardConfig>();

            /// <summary>
            /// Adds a creature to the wave outline, with a total percentage 1-100, and the number of entries that should be broken out from this creature
            /// </summary>
            /// <param name="creature"></param>
            /// <param name="percentage"></param>
            /// <param name="entries"></param>
            public void AddCreatureToWave(String creature, int total_points, float percentage, short min_stars = 0, short max_stars =0)
            {
                short creature_spawn_amount = DetermineCreatureSpawnAmount(creature, total_points, percentage);
                switch (Monsters.SpawnableCreatures[creature].spawnType)
                {
                    case COMMON:
                        commonCreatures.Add(new HoardConfig { creature = creature, prefab = Monsters.SpawnableCreatures[creature].prefabName, amount = creature_spawn_amount, stars = 0 });
                        selectedCreatures.Add(creature);
                        break;
                    case RARE:
                        rareCreatures.Add(new HoardConfig { creature = creature, prefab = Monsters.SpawnableCreatures[creature].prefabName, amount = creature_spawn_amount, stars = 0 });
                        selectedCreatures.Add(creature);
                        break;
                    case ELITE:
                        eliteCreatures.Add(new HoardConfig{ creature = creature, prefab = Monsters.SpawnableCreatures[creature].prefabName, amount = creature_spawn_amount, stars = 0 });
                        selectedCreatures.Add(creature);
                        break;
                    case UNIQUE:
                        uniqueCreatures.Add(new HoardConfig{ creature = creature, prefab = Monsters.SpawnableCreatures[creature].prefabName, amount = creature_spawn_amount, stars = 0 });
                        selectedCreatures.Add(creature);
                        break;
                }
            }

            public List<HoardConfig> getCommonCreatures()
            {
                return commonCreatures;
            }
            public List<HoardConfig> getRareCreatures()
            {
                return rareCreatures;
            }
            public List<HoardConfig> getEliteCreatures()
            {
                return eliteCreatures;
            }
            public List<HoardConfig> getUniqueCreatures()
            {
                return uniqueCreatures;
            }

            public bool HasKey(String key)
            {
                return selectedCreatures.Contains(key);
            }
            public int GetCount() { return selectedCreatures.Count();}
        }


        // Steep logarithmic curve with controllable curve rate and increases.
        // Log(level^2) * (challenge_slope * level) + base_challenge_points
        public static Int16 ComputeChallengePoints(Int16 level)
        {
            Double level_offset = Math.Pow(level, 2);
            Double computed_slope = Math.Log(level_offset);
            Double offset = VFConfig.ChallengeSlope.Value * level;
            Double allocated_additional_points = computed_slope * offset;
            Int16 allocated_challenge_points = (Int16)(VFConfig.BaseChallengePoints.Value + allocated_additional_points);
            Logger.LogInfo($"Challenge points slope:{allocated_challenge_points} = (log({level_offset})[{computed_slope}] * ({VFConfig.ChallengeSlope.Value} * {level})[{offset}])[{allocated_additional_points}] + {VFConfig.BaseChallengePoints.Value} ");
            // Cap the challenge points if they are over the defined max
            if (allocated_challenge_points > VFConfig.MaxChallengePoints.Value)
            {
                allocated_challenge_points = VFConfig.MaxChallengePoints.Value;
            }
            return allocated_challenge_points;
        }

        public static PhasedWaveTemplate generateRandomWaveWithOptions(ChallengeLevelDefinition levelDefinition, bool hard_mode, bool boss_mode, bool siege_mode, short override_max_creatures = 0)
        {
            // The defined levels definition index is what we use for the actual point calculation- meaning you will fight and get rewards based on that level- not the position of the level definition.
            Int16 wave_total_points = ComputeChallengePoints(levelDefinition.levelIndex);
            if (hard_mode) {
                Jotunn.Logger.LogInfo("Hard mode has been enabled, bigger challenge, bigger reward.");
                wave_total_points = (Int16)(wave_total_points * 1.5);
            }
            if (siege_mode)
            {
                Jotunn.Logger.LogInfo("Siege Mode has been enabled, prepare the ballistas.");
            }
            if (boss_mode)
            {
                Jotunn.Logger.LogInfo("Boss Mode has been enabled, the forsaken will find you.");
            }
            Jotunn.Logger.LogInfo($"Wave Challenge points: {wave_total_points}");
            // Builds out a template for wave generation
            Logger.LogInfo($"Building wave template from {levelDefinition.waveFormat}");
            PhasedWaveTemplate wavedefinition = dynamicBuildWaveTemplate(levelDefinition, wave_total_points, boss_mode, siege_mode, override_max_creatures);

            return wavedefinition;
        }

        public static PhasedWaveTemplate dynamicBuildWaveTemplate(ChallengeLevelDefinition defined_level, Int16 max_wave_points, bool boss_mode, bool siege_mode, short override_max_creatures = 0)
        {
            WaveOutline waveOutline = new WaveOutline();
            short phases = defined_level.numPhases;
            // maybe need some sanity checks?
            if (phases == 0) { phases = 4; } // fallback if phases are not defined
            if (siege_mode) { phases *= 2; }

            Int16 max_creatures_from_previous_biomes = defined_level.maxCreatureFromPreviousBiomes;
            Heightmap.Biome selected_biome = defined_level.biome;

            // if (level > 30 && level < 35) { selected_biome = "Ashlands"; }
            short targeted_wave_biome_level = BiomeStringToInt(selected_biome);
            WaveGenerationFormat WaveGenerationFormat = WaveStyles.GetWaveStyle(defined_level.waveFormat);
            if (boss_mode) { WaveGenerationFormat = WaveStyles.GetWaveStyle(defined_level.bossWaveFormat); }
            Int16 creatures_selected_from_previous_biome = 0;
            if(VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Starting wave generation for {selected_biome} - phases: {phases}"); }

            // Validate if we need to check exclusion/inclusion lists
            bool should_check_only_monsters = false;
            bool should_check_exclude_monsters = false;
            if (defined_level.onlySelectMonsters != null) { if (defined_level.onlySelectMonsters.Count > 0) { should_check_only_monsters = true; } }
            if (defined_level.excludeSelectMonsters != null) { if (defined_level.excludeSelectMonsters.Count > 0) { should_check_exclude_monsters = true; } }

            // Build a list of creature keys which are valid targets
            // We build the list once, and shuffle its contents each time to get different creatures picked for the same wave (if there are multiple valid for the role)
            List<string> creatureKeys = new List<string>();
            foreach(KeyValuePair<string, CreatureValues> creature in Monsters.SpawnableCreatures) {
                if(creature.Value.enabled != true) { continue; }
                creatureKeys.Add(creature.Key);
            }

            foreach (WaveFormatEntry entry in WaveGenerationFormat.waveFormats)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Starting wave creature selection for {entry.SpawnType}-{entry.SpawnPercentage}."); }
                string waveType = entry.SpawnType;
                float percentage = entry.SpawnPercentage;
                bool creature_added = false;
                int creature_add_iterations = 0;
                float duplicate_chance = 0.25f;
                while (creature_added == false)
                {
                    if (creature_add_iterations > 3) { if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogWarning($"{waveType}-{percentage}% failed to find an addition, skipping it."); } break; }
                    // we shuffle the monster keys so that we run through them in a different order each time
                    // This ensures that we fill the wave in different ways
                    List<string> this_iteration_key_order = ValheimFortress.shuffleList(creatureKeys);
                    
                    foreach (String skey in this_iteration_key_order)
                    {
                        if (Monsters.SpawnableCreatures[skey].spawnType != waveType) { continue; }
                        // Skip monsters that are not in the onlySelectedMonsters section, if its defined.
                        if (should_check_only_monsters)
                        {
                            if (!defined_level.onlySelectMonsters.Contains(skey)) 
                            {
                                Logger.LogDebug($"creature {skey} is not in the inclusion list, skipping.");
                                continue;
                            }
                        }
                        if (should_check_exclude_monsters)
                        {
                            if (defined_level.excludeSelectMonsters.Contains(skey)) 
                            {
                                Logger.LogDebug($"creature {skey} is in the exclusion list, skipping.");
                                continue; 
                            }
                        }
                        
                        short min_stars = 0;
                        // Creature is of the wrong spawntype, skipping

                        // Creature is not from the right biome, or -1 biome
                        // or we already have all the creatures we need from a previous biome
                        short current_creature_biome_level = BiomeStringToInt(Monsters.SpawnableCreatures[skey].biome);
                        //if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"{skey} Biome check: {current_creature_biome_level} > {targeted_wave_biome_level} || {targeted_wave_biome_level} != {current_creature_biome_level}"); }
                        //if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"{skey} Previous check: {current_creature_biome_level} != {targeted_wave_biome_level} && {creatures_selected_from_previous_biome} >= {max_creatures_from_previous_biomes}"); }
                        // Creature too high of a level
                        if (current_creature_biome_level > targeted_wave_biome_level) {
                            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"creature {skey} from a higher level biome."); }
                            continue; 
                        }
                        // Creature too low of a level
                        if (!(current_creature_biome_level >= (targeted_wave_biome_level - defined_level.previousBiomeSearchRange))) {
                            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"creature {skey} biome is too easy."); }
                            continue;
                        }
                        // This creature is from the previous biome
                        if (current_creature_biome_level != targeted_wave_biome_level)
                        {
                            if (creatures_selected_from_previous_biome >= max_creatures_from_previous_biomes)
                            {
                                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"creature {skey} is from a previous biome, but we already have enough creatures from a previous biome."); }
                                continue;
                            }
                            // Roll to see if we will add this creature, its only a 5% chance by default.
                            float prior_biome_chance = UnityEngine.Random.value;
                            if(defined_level.chancePreviousBiomeCreatureSelected < prior_biome_chance) {
                                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"previous biome creature {skey} failed its add roll {defined_level.chancePreviousBiomeCreatureSelected} < {prior_biome_chance}."); }
                                continue; 
                            }
                        }   

                        // Consider adding a duplicate, with a small chance
                        float duplicate_roll = UnityEngine.Random.value;
                        //if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"{skey} duplicate key check {waveOutline.HasKey(skey)} && {duplicate_roll > duplicate_chance}."); }
                        if (waveOutline.HasKey(skey) && duplicate_roll > duplicate_chance) { continue; }

                        // If the creature is from a previous biome, we note that, and add at least one star to its generation
                        if (current_creature_biome_level != targeted_wave_biome_level) 
                        { 
                            creatures_selected_from_previous_biome += 1;
                            if (defined_level.previousBiomeCreaturesAddedStarPerBiome) { min_stars += (short)(targeted_wave_biome_level - current_creature_biome_level); }
                        }

                        // Add the creature!
                        waveOutline.AddCreatureToWave(skey, max_wave_points, percentage, min_stars, VFConfig.MaxCreatureStars.Value);
                        creature_added = true;
                        duplicate_chance = 0.25f;
                        if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"creature {skey} added as {waveType} {percentage}."); }
                        break;
                    }
                    // If we didn't get something on the first run, we ensure duplicate entries which will gaurentee an entry the second time.
                    if (creature_added == false) { duplicate_chance = 1.0f; }
                    creature_add_iterations += 1;
                }
            }


            List<List<HoardConfig>> finalizedHoards = new List<List<HoardConfig>> { };
            int phases_generated = 0;
            List<HoardConfig> commonCreatures = waveOutline.getCommonCreatures();
            List<HoardConfig> rareCreatures = waveOutline.getRareCreatures();
            List<HoardConfig> eliteCreatures = waveOutline.getEliteCreatures();
            List<HoardConfig> uniqueCreatures = waveOutline.getUniqueCreatures();
            short max_per_wave = override_max_creatures;

            Logger.LogInfo($"Building out wave phases from wave outline with COMMON-{commonCreatures.Count} RARE-{rareCreatures.Count} ELITE-{eliteCreatures.Count} UNIQUE-{uniqueCreatures.Count}");

            while (phases > phases_generated)
            {
                List<HoardConfig> hoardPhase = new List<HoardConfig>();
                
                short common_creatures_in_wave;
                short rare_creatures_in_wave;
                short elite_creatures_in_wave;
                short unique_creatures_in_wave = 0;
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Generating Phase {phases_generated}"); }

                common_creatures_in_wave = GenerateHordePhase(commonCreatures, defined_level.commonSpawnModifiers, phases, phases_generated, hoardPhase, "COMMON");
                rare_creatures_in_wave = GenerateHordePhase(rareCreatures, defined_level.rareSpawnModifiers, phases, phases_generated, hoardPhase, "RARE");
                elite_creatures_in_wave = GenerateHordePhase(eliteCreatures, defined_level.eliteSpawnModifiers, phases, phases_generated, hoardPhase, "ELITE");

                // Last phase to generate, add the boss
                if ((phases_generated + 1) == phases)
                {
                    foreach(HoardConfig entry in uniqueCreatures)
                    {
                        short spawn_amount = entry.amount;
                        if (spawn_amount <= 0) { spawn_amount = 1; }
                        if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adding UNIQUE entry {entry.creature} - {entry.amount}"); }
                        unique_creatures_in_wave += spawn_amount;
                        hoardPhase.Add(new HoardConfig { creature = entry.creature, prefab = entry.prefab, amount = spawn_amount, stars = entry.stars });
                    }
                }

                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Phase creature selection and modifiers completed"); }

                // Post processing on the wave itself
                short total_creatures_in_wave = (short)(common_creatures_in_wave + rare_creatures_in_wave + elite_creatures_in_wave + unique_creatures_in_wave);
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Checking for wave reduction (max {max_per_wave}): Total {total_creatures_in_wave} = C-{common_creatures_in_wave} R-{rare_creatures_in_wave} U-{unique_creatures_in_wave}"); }
                if (total_creatures_in_wave > max_per_wave)
                {
                    ReduceWaveSizeToMax(hoardPhase, total_creatures_in_wave, max_per_wave);
                }
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Phase post-processing completed"); }

                finalizedHoards.Add(hoardPhase);
                phases_generated += 1;
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Starting next phase..."); }
            }

            PhasedWaveTemplate finalizedWaveGeneration = new PhasedWaveTemplate();
            finalizedWaveGeneration.hordePhases = finalizedHoards;

            if (VFConfig.EnableDebugMode.Value)
            {
                List<List<HoardConfig>> finalizedPhases = finalizedWaveGeneration.hordePhases;
                Jotunn.Logger.LogInfo($"Built wave definition with {finalizedPhases.Count} phases.");
                int debugPhases = 0;
                foreach(List<HoardConfig> phase in finalizedPhases)
                {
                    debugPhases += 1;
                    Jotunn.Logger.LogInfo($"Phase {debugPhases}");
                    foreach (HoardConfig hoard in phase)
                    {
                        Jotunn.Logger.LogInfo($"Hoard {hoard.creature} - Amount: {hoard.amount} Stars: {hoard.stars}");
                    }
                }
                
            }

            return finalizedWaveGeneration;
        }

        public static short GenerateHordePhase(List<HoardConfig> creatureEntries, SpawnModifiers creature_modifiers, int phases, int current_phase, List<HoardConfig> current_phase_hoard_config, string creatureType)
        {
            short creatureSumCount = 0;
            foreach (HoardConfig entry in creatureEntries)
            {
                float wavePercent = ApplyWavePercentModifiers(creature_modifiers, phases, current_phase);
                if (wavePercent == 0f) { continue; } 
                short spawn_amount = (short)(entry.amount * wavePercent);
                if (spawn_amount <= 0) { spawn_amount = 1; }
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adding {creatureType} entry {entry.creature} - {spawn_amount}"); }
                creatureSumCount += spawn_amount;
                current_phase_hoard_config.Add(new HoardConfig {creature = entry.creature, prefab = entry.prefab, amount = spawn_amount, stars = entry.stars });
            }
            return creatureSumCount;
        }


        public static float linearDecreaseRandomWaveAdjustment(int phases, int current_phase, float variance = 0.10f)
        {
            float initial_spawn = 1.0f;
            // reduction_from_phase_progression is at most an 80% change, plus/minus the variance, which should be always less than that
            float reduction_from_phase_progression = (0.8f / phases);
            float current_phase_reduction = reduction_from_phase_progression * current_phase;
            float random_variance = UnityEngine.Random.Range(variance * -1, variance);
            // Increase wave spawn towards the front, and reduce is significantly over time
            float current_wave_percentage = initial_spawn - current_phase_reduction + random_variance;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adjusting wave with linearDecreaseRandomWaveAdjustment percent {current_wave_percentage} for phase {current_phase}"); }
            return current_wave_percentage;
        }

        public static float linearIncreaseRandomWaveAdjustment(int phases, int current_phase, float variance = 0.10f)
        {
            float initial_spawn = 0.20f;
            // reduction_from_phase_progression is at most an 80% change, plus/minus the variance, which should be always less than that
            float increase_from_phase_progression = (0.8f / phases);
            float current_phase_increase = increase_from_phase_progression * current_phase;
            float random_variance = UnityEngine.Random.Range(variance * -1, variance);
            // Increase wave spawn towards the front, and reduce is significantly over time
            float current_wave_percentage = initial_spawn + current_phase_increase + random_variance;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adjusting wave with linearIncreaseRandomWaveAdjustment percent {current_wave_percentage} for phase {current_phase}"); }
            return current_wave_percentage;
        }


        // This is primarily a random fractional adjustment, with a min of 60% and max of 100% unless overtuned
        public static float partialRandomWaveAdjustment(float variance = 0.20f)
        {
            // variance allows this up to 100% or down to 60%
            float random_variance = UnityEngine.Random.Range(variance * -1, variance);
            float current_wave_percentage = (1f - (variance/2)) + random_variance;
            if (current_wave_percentage > 1f) { current_wave_percentage = 1f;}
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adjusting wave with partialRandomWaveAdjustment percent {current_wave_percentage}"); }
            return current_wave_percentage;
        }

        public static short DetermineCreatureSpawnAmount(string creature, int total_points, float creature_percentage)
        {
            // The number to spawn is the total amount of wave points this segment represents / the creatures spawn cost
            float number_to_spawn = total_points * (creature_percentage / 100f) / Monsters.SpawnableCreatures[creature].spawnCost;
            float number_to_spawn_variance = number_to_spawn * 0.10f;
            float spawn_amount = UnityEngine.Random.Range((number_to_spawn - number_to_spawn_variance), (number_to_spawn + number_to_spawn_variance));
            if (Monsters.SpawnableCreatures[creature].spawnType == UNIQUE) { spawn_amount = 1; } // uniques only spawn one
            return (short)spawn_amount;
        }

        public static float onlyGenerateInSecondHalf(int phases, int current_phase)
        {
            float wave_percent = 0f;
            if (current_phase >= (phases / 2))
            {
                wave_percent = 1f;
            }
            return wave_percent;
        }

        public static float ApplyWavePercentModifiers(SpawnModifiers waveMods, int phases, int current_phase)
        {
            float wavePercent = 1f;
            // Guard clause for no modifiers
            if (waveMods.AnyEnabled()) {
                return wavePercent;
            }

            if (waveMods.linearDecreaseRandomWaveAdjustment)
            {
                wavePercent = linearDecreaseRandomWaveAdjustment(phases, current_phase);
            }
            if (waveMods.linearIncreaseRandomWaveAdjustment)
            {
                wavePercent = linearIncreaseRandomWaveAdjustment(phases, current_phase);
            }

            if (waveMods.partialRandomWaveAdjustment)
            {
                wavePercent = partialRandomWaveAdjustment(0.3f);
            }

            if (waveMods.onlyGenerateInSecondHalf)
            {
                wavePercent = onlyGenerateInSecondHalf(phases, current_phase);
            }
            return wavePercent;
        }


        public static Int16 BiomeStringToInt(Heightmap.Biome biome)
        {
            if(!biomes.Contains(biome.ToString())) { throw new ArgumentException($"Biome {biome} does not match defined biomes: {string.Join(",", biomes)}"); }
            return (Int16)Array.IndexOf(biomes, biome);
        }


        public static List<HoardConfig> ReduceWaveSizeToMax(List<HoardConfig> hoards, short total_creatures_in_wave, short max_per_wave)
        {
            if (total_creatures_in_wave <= max_per_wave) { return hoards; }

            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Wave has more creatures than allowed from configuration to remove: {total_creatures_in_wave} > {max_per_wave}"); }
            short iterations = 0;
            // This won't scale up infinitely in reducing waves, but could produce waves up to 1/4th the size
            while(iterations < VFConfig.MaxCreatureStars.Value + 1)
            {
                total_creatures_in_wave = EvaluateAndReduceWave(hoards, total_creatures_in_wave, max_per_wave);
                if (total_creatures_in_wave <= max_per_wave) { break; }
                iterations++;
            }

            return hoards;
        }

        private static short EvaluateAndReduceWave(List<HoardConfig> hoards, short total_creatures_in_wave, short max_per_wave, short stop_reduction_at = 5)
        {
            if (total_creatures_in_wave > max_per_wave)
            {
                foreach (HoardConfig entry in hoards)
                {
                    if (entry.amount <= stop_reduction_at) { continue; }
                    if (total_creatures_in_wave <= max_per_wave) { break; }
                    if (Monsters.SpawnableCreatures[entry.creature].spawnType == CONST.UNIQUE) { continue; }
                    total_creatures_in_wave = MutatingReduceHoardConfigSize(entry, total_creatures_in_wave, max_per_wave);
                }
            }
            return total_creatures_in_wave;
        }

        private static short MutatingReduceHoardConfigSize(HoardConfig hoard, short total_creatures_in_wave, short max_per_wave)
        {
            if (total_creatures_in_wave > max_per_wave)
            {
                if (hoard.stars < VFConfig.MaxCreatureStars.Value)
                {
                    short creature_difference = (short)(hoard.amount / 2);
                    hoard.amount = creature_difference;
                    hoard.stars += 1;
                    total_creatures_in_wave = (short)(total_creatures_in_wave - creature_difference);
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"wave {hoard.creature} reduced from {hoard.amount * 2} to {hoard.amount} total creatures in wave now: {total_creatures_in_wave + creature_difference} -> {total_creatures_in_wave}"); }
                }
            }
            return total_creatures_in_wave;
        }
    }
}
