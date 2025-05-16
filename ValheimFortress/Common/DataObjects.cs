using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.ComponentModel;
using ValheimFortress.Data;
using static ValheimFortress.Data.WaveStyles;

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
        public const String FADER = "Fader";

        public static IDeserializer yamldeserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        public static ISerializer yamlserializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).DisableAliases().Build();
    }

    public enum ShrineType {
        Challenge,
        Arena,
        Wild
    }

    public class ExternalNoUIShrineConfiguration
    {
        public string shrineName { get; set; }
        public string shrineInteractionMessage { get; set; }
        public string shrineUnacceptedTributeMessage { get; set; }
        public string shrineLargerTributeRequiredMessage { get; set; }
        public List<ExternalShrineLevelConfiguration> shrineLevelsConfig { get; set; }
    }

    public class ExternalShrineLevelConfiguration
    {
        public string tributePrefab { get; set; }
        public short tributeAmount { get; set; }
        public Dictionary<string, short> rewards { get; set; }
        public string waveStartMessage { get; set; }
        public string waveEndMessage { get; set; }

        public ExternalShrineLevel wildLevelDefinition { get; set; }
    }


    public class WildShrineConfigurationCollection
    {
        public List<WildShrineConfiguration> WildShrines { get; set; }
    }

    [DataContract]
    public class WildShrineConfiguration
    {
        public string definitionForWildShrine {  get; set; }
        public String wildShrineNameLocalization { get; set; }
        public String wildShrineRequestLocalization { get; set; }
        public String shrine_unaccepted_tribute_localization { get; set; }
        public String shrine_larger_tribute_required_localization { get; set; }
        public List<WildShrineLevelConfiguration> wildShrineLevelsConfig { get; set; }
    }

    [DataContract]
    public class WildShrineLevelConfiguration
    {
        public String tributeName { get; set; }
        public Int16 tributeAmount { get; set; }
        public Dictionary<String, short> rewards { get; set; }
        public bool hardMode { get; set; }
        public bool siegeMode { get; set; }
        public string wildshrine_wave_start_localization { get; set; }
        public string wildshrine_wave_end_localization { get; set; }
        public WildLevelDefinition wildLevelDefinition { get; set; }
    }

    [DataContract]
    public class HoardConfig
    {
        public String creature { get; set; }
        public String prefab { get; set; }
        public Int16 amount { get; set; }
        public Int16 stars { get; set; }
        public HoardConfig() {}
    }

    [DataContract]
    public class PhasedWaveTemplate
    {
        public List<List<HoardConfig>> hordePhases { get; set; }

        public PhasedWaveTemplate() {}

        //public byte[] ToBinaryArray()
        //{
            
        //}
    }

    [DataContract]
    public class CreatureValues
    {
        public short spawnCost { get; set; }
        public String prefabName { get; set; }
        public String spawnType { get; set; }
        public Heightmap.Biome biome { get; set; }
        public Boolean enabled { get; set; }
        public Boolean dropsEnabled { get; set; }

        // Needed only for Serialization
        public CreatureValues() { }
    }

    [DataContract]
    public class RewardEntry
    {
        public short resourceCost { get; set; }
        public String requiredBoss { get; set; }
        public String resourcePrefab { get; set; }
        public bool enabled { get; set; }
        // required for serialization, since this used to have a custom init
        public RewardEntry() { }
    }

    [DataContract]
    public class WaveGenerationFormat
    {
        public List<WaveFormatEntry> waveFormats {  get; set; }
        // required for serialization, since this uses a custom init
        public WaveGenerationFormat()
        {
        }
    }

    [DataContract]
    public struct WaveFormatEntry
    {
        public WaveFormatEntry(string spawnType, short spawnPercentage)
        {
            SpawnType = spawnType;
            SpawnPercentage = spawnPercentage;
        }
        public string SpawnType { get; set; }
        public short SpawnPercentage { get; set; }
    }

    [DataContract]
    public class ChallengeLevelDefinition
    {
        [DefaultValue("")]
        public string levelName { get; set; }
        public short levelIndex { get; set; }
        [DefaultValue(4)]
        public short numPhases { get; set; }
        public Dictionary<ShrineType, bool> levelForShrineTypes { get; set; }
        public string levelMenuLocalization {  get; set; }
        public string requiredGlobalKey { get; set; }
        public Heightmap.Biome biome { get; set; }
        public WaveStyleName waveFormat { get; set; }
        public WaveStyleName bossWaveFormat { get; set; }
        public short maxCreatureFromPreviousBiomes { get; set; }
        [DefaultValue(1)]
        public short previousBiomeSearchRange { get; set; }
        [DefaultValue(0.05f)]
        public float chancePreviousBiomeCreatureSelected {  get; set; }
        [DefaultValue(true)]
        public bool previousBiomeCreaturesAddedStarPerBiome { get; set; }
        public string levelWarningLocalization { get; set; }
        public string bossLevelWarningLocalization { get; set; }
        public List<String> levelRewardOptionsLimitedTo {  get; set; }
        public List<String> onlySelectMonsters { get; set; }
        public List<String> excludeSelectMonsters { get; set; }
        public SpawnModifiers commonSpawnModifiers { get; set; }
        public SpawnModifiers rareSpawnModifiers { get; set; }
        public SpawnModifiers eliteSpawnModifiers { get; set; }
        public SpawnModifiers uniqueSpawnModifiers { get; set; }
        public ChallengeLevelDefinition() { }
    }

    [DataContract]
    public class WildLevelDefinition
    {
        public short levelIndex { get; set; }
        public Heightmap.Biome biome { get; set; }
        public WaveStyleName waveFormat { get; set; }
        public string levelWarningLocalization { get; set; }
        public short maxCreaturesPerPhaseOverride { get; set; }
        public List<String> onlySelectMonsters { get; set; }
        public List<String> excludeSelectMonsters { get; set; }
        public SpawnModifiers commonSpawnModifiers { get; set; }
        public SpawnModifiers rareSpawnModifiers { get; set; }
        public SpawnModifiers eliteSpawnModifiers { get; set; }
        public WildLevelDefinition() { }

        public ChallengeLevelDefinition ToChallengeLevelDefinition()
        {
            ChallengeLevelDefinition w_as_clevel = new ChallengeLevelDefinition();
            w_as_clevel.levelIndex = this.levelIndex;
            w_as_clevel.levelForShrineTypes = new Dictionary<ShrineType, bool> { { ShrineType.Wild, true } };
            w_as_clevel.levelMenuLocalization = ""; // unused
            w_as_clevel.requiredGlobalKey = "NONE";
            w_as_clevel.biome = this.biome;
            w_as_clevel.waveFormat = this.waveFormat;
            w_as_clevel.bossWaveFormat = WaveStyleName.Normal; // unused
            w_as_clevel.maxCreatureFromPreviousBiomes = 0;
            w_as_clevel.levelWarningLocalization = this.levelWarningLocalization;
            w_as_clevel.bossLevelWarningLocalization = ""; // unused
            w_as_clevel.onlySelectMonsters = this.onlySelectMonsters;
            w_as_clevel.excludeSelectMonsters = this.excludeSelectMonsters;
            w_as_clevel.commonSpawnModifiers = this.commonSpawnModifiers;
            w_as_clevel.rareSpawnModifiers = this.rareSpawnModifiers;
            w_as_clevel.eliteSpawnModifiers = this.eliteSpawnModifiers;
            w_as_clevel.uniqueSpawnModifiers = new SpawnModifiers() { };
            return w_as_clevel;
        }
    }

    public class ExternalShrineLevel
    {
        public Heightmap.Biome Biome { get; set; }
        public short Difficulty { get; set; }
        public WaveStyleName WaveFormat { get; set; }
        public string LevelWarningLocalization { get; set; }
        public LevelModifiers levelModifiers { get; set; }
        public List<String> OnlySelectMonsters { get; set; }
        public List<String> ExcludeSelectMonsters { get; set; }
        public SpawnModifiers CommonSpawnModifiers { get; set; }
        public SpawnModifiers RareSpawnModifiers { get; set; }
        public SpawnModifiers EliteSpawnModifiers { get; set; }

        public ChallengeLevelDefinition ToChallengeLevelDefinition()
        {
            ChallengeLevelDefinition w_as_clevel = new ChallengeLevelDefinition();
            w_as_clevel.levelIndex = this.Difficulty;
            w_as_clevel.levelForShrineTypes = new Dictionary<ShrineType, bool> { { ShrineType.Wild, true } };
            w_as_clevel.levelMenuLocalization = ""; // unused
            w_as_clevel.requiredGlobalKey = "NONE";
            w_as_clevel.biome = this.Biome;
            w_as_clevel.waveFormat = this.WaveFormat;
            w_as_clevel.bossWaveFormat = WaveStyleName.Normal; // unused
            w_as_clevel.maxCreatureFromPreviousBiomes = 0;
            w_as_clevel.levelWarningLocalization = this.LevelWarningLocalization;
            w_as_clevel.bossLevelWarningLocalization = ""; // unused
            w_as_clevel.onlySelectMonsters = this.OnlySelectMonsters;
            w_as_clevel.excludeSelectMonsters = this.ExcludeSelectMonsters;
            w_as_clevel.commonSpawnModifiers = this.CommonSpawnModifiers;
            w_as_clevel.rareSpawnModifiers = this.RareSpawnModifiers;
            w_as_clevel.eliteSpawnModifiers = this.EliteSpawnModifiers;
            w_as_clevel.uniqueSpawnModifiers = new SpawnModifiers() { };
            return w_as_clevel;
        }
    }

    public class SpawnModifiers {
        public bool linearIncreaseRandomWaveAdjustment { get; set; } = false;
        public bool linearIncreaseWaveAdjustment { get; set; } = false;
        public bool linearDecreaseRandomWaveAdjustment { get; set; } = false;
        public bool partialRandomWaveAdjustment { get; set; } = false;
        public bool onlyGenerateInSecondHalf { get; set; } = false;

        public bool AnyEnabled()
        {
            return linearIncreaseRandomWaveAdjustment || linearIncreaseWaveAdjustment || linearDecreaseRandomWaveAdjustment || partialRandomWaveAdjustment || onlyGenerateInSecondHalf;
        }
    }

    public class LevelModifiers
    {
        public bool SiegeMode { get; set; } = false;
        public bool HardMode { get; set; } = false;
        public bool BossMode { get; set; } = false;
    }

    public class ChallengeLevelDefinitionCollection
    {
        public List<ChallengeLevelDefinition> Levels { get; set; }
    }

    public class RewardEntryCollection
    {
        public Dictionary<string, RewardEntry> Rewards { get; set; }
    }

    public class SpawnableCreatureCollection
    {
        public Dictionary<string, CreatureValues> Creatures { get; set; }
    }

    public class WaveFormatCollection
    {
        public Dictionary<WaveStyleName, WaveGenerationFormat> WaveFormats { get; set; }
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

    public class ListStringZNetProperty : ZNetProperty<List<string>>
    {
        BinaryFormatter binFormatter = new BinaryFormatter();
        public ListStringZNetProperty(string key, ZNetView zNetView, List<string> defaultValue) : base(key, zNetView, defaultValue)
        {
        }

        public override List<string> Get()
        {
            var stored = zNetView.GetZDO().GetByteArray(Key);
            // we can't deserialize a null buffer
            if (stored == null) { return new List<string>(); }
            var mStream = new MemoryStream(stored);
            var deserializedDictionary = (List<String>)binFormatter.Deserialize(mStream);
            return deserializedDictionary;
        }

        protected override void SetValue(List<string> value)
        {
            var mStream = new MemoryStream();
            binFormatter.Serialize(mStream, value);

            zNetView.GetZDO().Set(Key, mStream.ToArray());
        }
    }

    public class ArrayVectorZNetProperty : ZNetProperty<Vector3[]>
    {
        BinaryFormatter binFormatter = new BinaryFormatter();
        public ArrayVectorZNetProperty(string key, ZNetView zNetView, Vector3[] defaultValue) : base(key, zNetView, defaultValue)
        {
        }

        public override Vector3[] Get()
        {
            var stored = zNetView.GetZDO().GetByteArray(Key);
            // we can't deserialize a null buffer
            if (stored == null) { return new Vector3[0]; }
            var mStream = new MemoryStream(stored);
            var deserializedstringarr = (string[])binFormatter.Deserialize(mStream);
            if (deserializedstringarr == null || deserializedstringarr.Length == 0) { return null; }
            Vector3[] varr = new Vector3[deserializedstringarr.Length];
            int i = 0;
            foreach (var item in deserializedstringarr) {
                string[] vvals = item.Split(',');
                varr[i] = new Vector3(x: float.Parse(vvals[0]), y: float.Parse(vvals[1]), z: float.Parse(vvals[2]));
                i++;
            }
            return varr;
        }

        protected override void SetValue(Vector3[] value)
        {
            string[] serializable_vectors = new string[value.Length];
            int i = 0;
            foreach (var v in value) {
                serializable_vectors[i] = $"{v.x},{v.y},{v.z}";
                i++;
            }
            var mStream = new MemoryStream();
            binFormatter.Serialize(mStream, serializable_vectors);
            

            zNetView.GetZDO().Set(Key, mStream.ToArray());
        }
    }

    public class DictionaryZNetProperty : ZNetProperty<Dictionary<String, short>>
    {
        BinaryFormatter binFormatter = new BinaryFormatter();
        public DictionaryZNetProperty(string key, ZNetView zNetView, Dictionary<String, short> defaultValue) : base(key, zNetView, defaultValue)
        {
        }

        public override Dictionary<String, short> Get()
        {
            var stored = zNetView.GetZDO().GetByteArray(Key);
            // we can't deserialize a null buffer
            if (stored == null) { return new Dictionary<string, short>(); }
            var mStream = new MemoryStream(stored);
            var deserializedDictionary = (Dictionary<String, short>)binFormatter.Deserialize(mStream);
            return deserializedDictionary;
        }

        protected override void SetValue(Dictionary<String, short> value)
        {
            var mStream = new MemoryStream();
            binFormatter.Serialize(mStream, value);

            zNetView.GetZDO().Set(Key, mStream.ToArray());
        }
    }

    public class ZDOIDZNetProperty : ZNetProperty<ZDOID>
    {
        public ZDOIDZNetProperty(string key, ZNetView zNetView, ZDOID defaultValue) : base(key, zNetView, defaultValue)
        {
        }

        public override ZDOID Get()
        {
            return zNetView.GetZDO().GetZDOID(Key);
        }

        protected override void SetValue(ZDOID value)
        {
            zNetView.GetZDO().Set(Key, value);
        }
    }

    public class ListUINTZNetProperty : ZNetProperty<List<uint>>
    {
        BinaryFormatter binFormatter = new BinaryFormatter();
        public ListUINTZNetProperty(string key, ZNetView zNetView, List<uint> defaultValue) : base(key, zNetView, defaultValue)
        {
        }

        public override List<uint> Get()
        {
            var stored = zNetView.GetZDO().GetByteArray(Key);
            // we can't deserialize a null buffer
            if (stored == null) { return new List<uint>(); }
            var mStream = new MemoryStream(stored);
            var deserializedDictionary = (List<uint>)binFormatter.Deserialize(mStream);
            return deserializedDictionary;
        }

        protected override void SetValue(List<uint> value)
        {
            var mStream = new MemoryStream();
            binFormatter.Serialize(mStream, value);

            zNetView.GetZDO().Set(Key, mStream.ToArray());
        }
    }

    public class DictionaryZDOIDZNetProperty : ZNetProperty<Dictionary<uint, ZDOID>>
    {
        BinaryFormatter binFormatter = new BinaryFormatter();
        public DictionaryZDOIDZNetProperty(string key, ZNetView zNetView, Dictionary<uint, ZDOID> defaultValue) : base(key, zNetView, defaultValue)
        {
        }

        public override Dictionary<uint, ZDOID> Get()
        {
            var stored = zNetView.GetZDO().GetByteArray(Key);
            // we can't deserialize a null buffer
            if (stored == null) { return new Dictionary<uint, ZDOID>(); }
            var mStream = new MemoryStream(stored);
            var deserializedDictionary = (Dictionary<uint, ZDOID>)binFormatter.Deserialize(mStream);
            return deserializedDictionary;
        }

        protected override void SetValue(Dictionary<uint, ZDOID> value)
        {
            var mStream = new MemoryStream();
            binFormatter.Serialize(mStream, value);

            zNetView.GetZDO().Set(Key, mStream.ToArray());
        }
    }
}
