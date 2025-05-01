using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValheimFortress.Challenge;
using static ValheimFortress.Data.WaveStyles;

namespace ValheimFortress
{
    public static class API
    {

        

        // This is just a helper for building the ExternalShrineLevel object
        public static ExternalShrineLevel AddExternalLevelDefinition(
            Heightmap.Biome biome, 
            short difficulty, 
            WaveStyleName waveFormat, 
            string levelWarningLocalization, 
            List<string> onlySelectMonsters,
            List<string> excludeSelectMonsters,
            SpawnModifiers commonSpawnModifiers,
            SpawnModifiers rareSpawnModifiers,
            SpawnModifiers eliteSpawnModifiers,
            LevelModifiers levelModifiers
        ){
            ExternalShrineLevel level = new ExternalShrineLevel();
            level.Biome = biome;
            level.Difficulty = difficulty;
            level.WaveFormat = waveFormat;
            level.levelModifiers = levelModifiers;
            level.LevelWarningLocalization = levelWarningLocalization;
            level.OnlySelectMonsters = onlySelectMonsters;
            level.ExcludeSelectMonsters = excludeSelectMonsters;
            level.CommonSpawnModifiers = commonSpawnModifiers;
            level.RareSpawnModifiers = rareSpawnModifiers;
            level.EliteSpawnModifiers = eliteSpawnModifiers;

            return level;
        }
    }
}
