using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using static ValheimFortress.Challenge.Levels;
using static ValheimFortress.Challenge.Rewards;
using UnityEngine;

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
        public const String NONE = "None";
        public const String EIKYTHR = "Eikythr";
        public const String ELDER = "TheElder";
        public const String BONEMASS = "BoneMass";
        public const String MODER = "Moder";
        public const String YAGLUTH = "Yagluth";
        public const String QUEEN = "TheQueen";

        public static IDeserializer yamldeserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        public static ISerializer yamlserializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
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

    [DataContract]
    public class RewardEntry
    {
        public short resouceCost { get; set; }
        public String requiredBoss { get; set; }
        public String resourcePrefab { get; set; }
        public bool enabled { get; set; }
        // required for serialization, since this used to have a custom init
        public RewardEntry() { }
    }

    public class RewardEntryCollection
    {
        public Dictionary<string, RewardEntry> Rewards { get; set; }
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

    public abstract class ZNetProperty<T>
    {
        public string Key { get; private set; }
        public T DefaultValue { get; private set; }
        protected readonly ZNetView zNetView;

        protected ZNetProperty(string key, ZNetView zNetView, T defaultValue)
        {
            Key = key;
            DefaultValue = defaultValue;
            this.zNetView = zNetView;
        }

        private void ClaimOwnership()
        {
            if (!zNetView.IsOwner())
            {
                zNetView.ClaimOwnership();
            }
        }

        public void Set(T value)
        {
            SetValue(value);
        }

        public void ForceSet(T value)
        {
            ClaimOwnership();
            Set(value);
        }

        public abstract T Get();

        protected abstract void SetValue(T value);
    }

    public class BoolZNetProperty : ZNetProperty<bool>
    {
        public BoolZNetProperty(string key, ZNetView zNetView, bool defaultValue) : base(key, zNetView, defaultValue)
        {
        }

        public override bool Get()
        {
            return zNetView.GetZDO().GetBool(Key, DefaultValue);
        }

        protected override void SetValue(bool value)
        {
            zNetView.GetZDO().Set(Key, value);
        }
    }

    public class IntZNetProperty : ZNetProperty<int>
    {
        public IntZNetProperty(string key, ZNetView zNetView, int defaultValue) : base(key, zNetView, defaultValue)
        {
        }

        public override int Get()
        {
            return zNetView.GetZDO().GetInt(Key, DefaultValue);
        }

        protected override void SetValue(int value)
        {
            zNetView.GetZDO().Set(Key, value);
        }
    }

    public class StringZNetProperty : ZNetProperty<string>
    {
        public StringZNetProperty(string key, ZNetView zNetView, string defaultValue) : base(key, zNetView, defaultValue)
        {
        }

        public override string Get()
        {
            return zNetView.GetZDO().GetString(Key, DefaultValue);
        }

        protected override void SetValue(string value)
        {
            zNetView.GetZDO().Set(Key, value);
        }
    }

    public class Vector3ZNetProperty : ZNetProperty<Vector3>
    {
        public Vector3ZNetProperty(string key, ZNetView zNetView, Vector3 defaultValue) : base(key, zNetView, defaultValue)
        {
        }

        public override Vector3 Get()
        {
            return zNetView.GetZDO().GetVec3(Key, DefaultValue);
        }

        protected override void SetValue(Vector3 value)
        {
            zNetView.GetZDO().Set(Key, value);
        }
    }

}
