using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static ValheimFortress.Challenge.Levels;

namespace ValheimFortress.Challenge
{
    public class CONST
    {
        public const String COMMON = "common";
        public const String RARE = "rare";
        public const String ELITE = "elite";
        public const String UNIQUE = "unique";
        public const String MEADOWS = "Meadows";
        public const String BLACKFOREST = "BlackForest";
        public const String SWAMP = "Swamp";
        public const String MOUNTAIN = "Mountain";
        public const String PLAINS = "Plains";
        public const String MISTLANDS = "Mistlands";
        public const String ASHLANDS = "Ashlands";
    }


    public class HoardConfig
    {
        public String creature;
        public String prefab;
        public Int16 amount;
        public Int16 stars;

        public HoardConfig(
            String creatureName,
            string prefabName,
            Int16 spawnAmount,
            Int16 starsLevel
            )
        {
            prefab = prefabName;
            creature = creatureName;
            amount = spawnAmount;
            stars = starsLevel;
        }

    }


    // Little helper object to contain built out waves
    public class WaveTemplate
    {
        private List<HoardConfig> waves = new List<HoardConfig> { };
        public WaveTemplate() { }

        public void AddHoard(HoardConfig hoard)
        {
            waves.Add(hoard);
        }

        public List<HoardConfig> GetWaves()
        {
            return waves;
        }

    }

    public class PhasedWaveTemplate
    {
        private List<List<HoardConfig>> hordePhases = new List<List<HoardConfig>> { };
        private int currentPhase = 0;
        private int availablePhases = 0;

        public void AddPhase(List<HoardConfig> hoardList) { hordePhases.Add(hoardList); availablePhases += 1; }

        public void ClearPhaseTemplate()
        {
            currentPhase = 0;
            availablePhases = 0;
            hordePhases.Clear();
        }
        public bool RemainingPhases()
        {
            return (availablePhases - currentPhase) > 0;
        }

        public bool IsFirstWave()
        {
            return (currentPhase == 0);
        }

        public bool IsLastWave()
        {
            return (currentPhase == availablePhases);
        }

        public int CountPhases() { return availablePhases; }

        public List<HoardConfig> GetCurrentPhase() 
        {
            List<HoardConfig> currentPhaseDefinition = hordePhases[currentPhase];
            currentPhase += 1;
            return currentPhaseDefinition; 
        }
        public List<List<HoardConfig>> GetAllPhases()
        {
            return hordePhases;
        }
    }

    [DataContract]
    public class CreatureValues
    {
        public short spawnCost { get; set; }
        public String prefabName { get; set; }
        public String spawnType { get; set; }
        public String biome { get; set; }
        public Boolean enabled { get; set; }
        public Boolean dropsEnabled { get; set; }

        // Needed only for Serialization
        public CreatureValues() { }
    }

    public class SpawnableCreatureCollection
    {
        public Dictionary<string, CreatureValues> Creatures { get; set; }
    }

    public class WaveGenerationFormat
    {
        List<Tuple<String, int>> waveFormat = new List<Tuple<string, int>> { };

        public void AddToWaveFormat(String spawnType, int percentage)
        {
            waveFormat.Add(new Tuple<string, int>(spawnType, percentage));
        }

        public WaveGenerationFormat AddMultiToWaveFormat(Tuple<String, int>[] entries)
        {
            foreach (Tuple<String, int> entry in entries)
            {
                this.AddToWaveFormat(entry.Item1, entry.Item2);
            }
            return this;
        }

        // Accessor
        public List<Tuple<string, int>> GetWaveFormat()
        {
            return waveFormat;
        }

        public Int16 Count()
        {
            return (Int16)waveFormat.Count();
        }

        public Int16 CountRareSpawns()
        {
            Int16 count = 0;
            foreach (Tuple<String, int> entry in waveFormat)
            {
                if (entry.Item1 == CONST.RARE) { count += 1; }

            }
            return count;
        }

        public List<Int16> GetRareSpawnFormations()
        {
            List<Int16> rareSpawnPercents = new List<Int16>();
            foreach (Tuple<String, int> entry in waveFormat)
            {
                if (entry.Item1 == CONST.RARE) { rareSpawnPercents.Add((Int16)entry.Item2); }

            }
            return rareSpawnPercents;
        }
        public Int16 CountCommonSpawns()
        {
            Int16 count = 0;
            foreach (Tuple<String, int> entry in waveFormat)
            {
                if (entry.Item1 == CONST.COMMON) { count += 1; }

            }
            return count;
        }

        public List<Int16> GetCommonSpawnFormations()
        {
            List<Int16> commonSpawnPercents = new List<Int16>();
            foreach (Tuple<String, int> entry in waveFormat)
            {
                if (entry.Item1 == CONST.COMMON) { commonSpawnPercents.Add((Int16)entry.Item2); }

            }
            return commonSpawnPercents;
        }

        public Int16 CountUniqueSpawns()
        {
            Int16 count = 0;
            foreach (Tuple<String, int> entry in waveFormat)
            {
                if (entry.Item1 == CONST.UNIQUE) { count += 1; }

            }
            return count;
        }

        public Int16 CountEliteSpawns()
        {
            Int16 count = 0;
            foreach (Tuple<String, int> entry in waveFormat)
            {
                if (entry.Item1 == CONST.ELITE) { count += 1; }

            }
            return count;
        }

        public List<Int16> GetEliteSpawnFormations()
        {
            List<Int16> eliteSpawnPercents = new List<Int16>();
            foreach (Tuple<String, int> entry in waveFormat)
            {
                if (entry.Item1 == CONST.ELITE) { eliteSpawnPercents.Add((Int16)entry.Item2); }

            }
            return eliteSpawnPercents;
        }
    }
}
