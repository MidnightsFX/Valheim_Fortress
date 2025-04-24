using System;
using System.Collections.Generic;
using ValheimFortress.Challenge;

namespace ValheimFortress.Data
{
    public class WaveStyles
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

        public enum WaveStyleName
        {
            Tutorial,
            Starter,
            Easy,
            Normal,
            Hard,
            VeryHard,
            Expert,
            Extreme,
            Dynamic,
            ElitesOnly,
            RaresOnly,
            CommonOnly,
            TutorialBoss,
            EasyBoss,
            Boss,
            DynamicBoss
        }

        // These should all stay as close as possible to 100% totals
        // There is no rule they can't go over/under, but going over will spawn more enemies than the waves points would normally allocate
        // This is normally only the case for challenge levels and bosses etc
        // Tutorial 60%
        // Starter 65%
        // Easy 70%
        // Normal 75%
        // Hard 80%
        // VeryHard 85%
        // Expert 90%
        // Extreme 95%
        // Dynamic 100%
        static Dictionary<WaveStyleName, WaveGenerationFormat> WaveGenerationStyles = new Dictionary<WaveStyleName, WaveGenerationFormat>
        {
            // Standard waves
            { WaveStyleName.Tutorial, new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry( COMMON, 30 ), new WaveFormatEntry(COMMON, 30) } } },
            { WaveStyleName.Starter, new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry( COMMON, 25 ), new WaveFormatEntry(COMMON, 25), new WaveFormatEntry(COMMON, 15) } } },
            { WaveStyleName.Easy, new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(RARE, 15 ), new WaveFormatEntry(COMMON, 25), new WaveFormatEntry(COMMON, 30) } } },
            { WaveStyleName.Normal, new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(RARE, 15 ), new WaveFormatEntry(RARE, 10), new WaveFormatEntry(COMMON, 30), new WaveFormatEntry(COMMON, 20) } } },
            { WaveStyleName.Hard, new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(ELITE, 5), new WaveFormatEntry(RARE, 20 ), new WaveFormatEntry(COMMON, 20), new WaveFormatEntry(COMMON, 30) } } },
            { WaveStyleName.VeryHard, new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(ELITE, 10), new WaveFormatEntry(RARE, 25 ), new WaveFormatEntry(COMMON, 20), new WaveFormatEntry(COMMON, 30) } } },
            { WaveStyleName.Expert, new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(ELITE, 10), new WaveFormatEntry(RARE, 15 ), new WaveFormatEntry(RARE, 15), new WaveFormatEntry(COMMON, 20), new WaveFormatEntry(COMMON, 30) } } },
            { WaveStyleName.Extreme, new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(ELITE, 15), new WaveFormatEntry(RARE, 20), new WaveFormatEntry(RARE, 15 ), new WaveFormatEntry(COMMON, 20), new WaveFormatEntry(COMMON, 25) } } },
            { WaveStyleName.Dynamic, new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(ELITE, 15), new WaveFormatEntry(RARE, 25 ), new WaveFormatEntry(RARE, 15), new WaveFormatEntry(COMMON, 20), new WaveFormatEntry(COMMON, 25) } } },
            { WaveStyleName.ElitesOnly, new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry( ELITE, 25 ), new WaveFormatEntry(ELITE, 25), new WaveFormatEntry(ELITE, 25) } } },
            { WaveStyleName.RaresOnly, new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(RARE, 25 ), new WaveFormatEntry(RARE, 25), new WaveFormatEntry(RARE, 25) } } },
            { WaveStyleName.CommonOnly, new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(COMMON, 25 ), new WaveFormatEntry(COMMON, 25), new WaveFormatEntry(COMMON, 25) } } },
            // Boss waves
            { WaveStyleName.TutorialBoss, new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(UNIQUE, 100), new WaveFormatEntry( COMMON, 30 ), new WaveFormatEntry(COMMON, 40) } } },
            { WaveStyleName.EasyBoss, new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(UNIQUE, 100), new WaveFormatEntry(RARE, 30 ), new WaveFormatEntry(COMMON, 30) } } },
            { WaveStyleName.Boss, new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(UNIQUE, 100), new WaveFormatEntry(ELITE, 20 ), new WaveFormatEntry(RARE, 30), new WaveFormatEntry(COMMON, 25) } } },
            { WaveStyleName.DynamicBoss, new WaveGenerationFormat { waveFormats = new List<WaveFormatEntry> { new WaveFormatEntry(UNIQUE, 100), new WaveFormatEntry(ELITE, 20 ), new WaveFormatEntry(RARE, 20), new WaveFormatEntry(RARE, 20), new WaveFormatEntry(COMMON, 20), new WaveFormatEntry(COMMON, 20) } } },
        };

        public static WaveGenerationFormat GetWaveStyle(WaveStyleName waveStyle)
        {
            return WaveGenerationStyles[waveStyle];
        }


        public static void UpdateWaveDefinition(WaveFormatCollection waveStyles)
        {
            WaveGenerationStyles.Clear();
            foreach (KeyValuePair<WaveStyleName, WaveGenerationFormat> entry in waveStyles.WaveFormats)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Updating Wavestyle Entry {entry.Key} {entry.Value}"); }
                if (!Enum.IsDefined(typeof(WaveStyleName), entry.Key)) { WaveStyleName newWs = (WaveStyleName)(Enum.GetValues(typeof(WaveStyleName)).Length + 1); }
                // Consider less hacky things than runtime enum adding here
                WaveGenerationStyles.Add(entry.Key, entry.Value);
            }
        }

        public static string YamlWaveDefinition()
        {
            var waveCollection = new WaveFormatCollection();
            waveCollection.WaveFormats = WaveGenerationStyles;
            var yaml = CONST.yamlserializer.Serialize(waveCollection);
            return yaml;
        }
    }
}
