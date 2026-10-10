using System;
using System.Collections.Generic;
using ValheimFortress.Common;

namespace ValheimFortress.Challenge
{
    public enum SlsModifierType
    {
        Major = 0,
        Minor = 1,
        Boss = 2
    }

    internal static class WaveCreatureSettings
    {
        internal static void ApplyToWave(PhasedWaveTemplate wave, ChallengeLevelDefinition level)
        {
            if (level.waveOverrides != null)
            {
                foreach (int waveNumber in level.waveOverrides.Keys)
                {
                    if (waveNumber < 1 || waveNumber > wave.hordePhases.Count)
                    {
                        Jotunn.Logger.LogWarning($"waveOverrides entry {waveNumber} is outside this event's waves 1..{wave.hordePhases.Count}; ignored.");
                    }
                }
            }

            for (int phaseIndex = 0; phaseIndex < wave.hordePhases.Count; phaseIndex++)
            {
                WaveSpawnSettingsOverride waveSettings = null;
                level.waveOverrides?.TryGetValue(phaseIndex + 1, out waveSettings);
                foreach (HoardConfig horde in wave.hordePhases[phaseIndex])
                {
                    horde.minimumStars = level.minimumStars;
                    horde.maximumStars = level.maximumStars;
                    horde.slsModifiers = level.slsModifiers;
                    ApplyCreatureOverrides(horde, level.creatureOverrides);
                    ApplyOverride(horde, waveSettings);
                    ApplyCreatureOverrides(horde, waveSettings?.creatureOverrides);
                    NormalizeStars(horde);
                    // Serialize the resolved settings, not the rules, so reloads and spawning
                    // peers receive the same result without needing to resolve the config again.
                    horde.slsModifiers = horde.slsModifiers == null
                        ? null : new Dictionary<string, SlsModifierType>(horde.slsModifiers);
                }
            }
        }

        private static void ApplyCreatureOverrides(HoardConfig horde, List<CreatureSpawnSettingsOverride> rules)
        {
            if (rules == null) { return; }
            // Matching rules apply in YAML list order; later fields take priority.
            foreach (CreatureSpawnSettingsOverride rule in rules)
            {
                if (rule?.creatures != null &&
                    (rule.creatures.Contains(horde.creature) || rule.creatures.Contains(horde.prefab)))
                {
                    ApplyOverride(horde, rule);
                }
            }
        }

        private static void ApplyOverride(HoardConfig horde, SpawnSettingsOverride settings)
        {
            if (settings == null) { return; }
            if (settings.minimumStars.HasValue) { horde.minimumStars = settings.minimumStars.Value; }
            if (settings.maximumStars.HasValue) { horde.maximumStars = settings.maximumStars.Value; }
            // An explicit empty map clears inherited requirements; null/omitted inherits.
            if (settings.slsModifiers != null) { horde.slsModifiers = settings.slsModifiers; }
        }

        private static void NormalizeStars(HoardConfig horde)
        {
            if (horde.minimumStars < 0)
            {
                Jotunn.Logger.LogWarning($"minimumStars {horde.minimumStars} is negative; using 0.");
                horde.minimumStars = 0;
            }
            if (horde.maximumStars < 0)
            {
                Jotunn.Logger.LogWarning($"maximumStars {horde.maximumStars} is negative; using 0.");
                horde.maximumStars = 0;
            }
            if (horde.maximumStars.HasValue && horde.minimumStars > horde.maximumStars.Value)
            {
                Jotunn.Logger.LogWarning($"minimumStars {horde.minimumStars} exceeds maximumStars {horde.maximumStars}; using {horde.maximumStars} for both.");
                horde.minimumStars = horde.maximumStars.Value;
            }
        }

        internal static void ApplyToCreature(Character creature, HoardConfig horde)
        {
            if (creature == null) { return; }
            int minimum = Math.Max(0, horde.minimumStars);
            long stars = Math.Max((long)creature.GetLevel() - 1, Math.Max(horde.stars, minimum));
            if (horde.maximumStars.HasValue)
            {
                stars = Math.Min(stars, Math.Max(0, horde.maximumStars.Value));
            }
            // Game levels are signed integers and include the base (zero-star) level.
            int level = (int)Math.Min(int.MaxValue, stars + 1);
            // SetLevel updates the networked level and vanilla health, unlike assigning m_level.
            // SLS receives the same level and persists its own cache through its public API.
            SlsIntegration.Apply(creature, level, horde.slsModifiers);
            if (VFConfig.EnableDebugMode.Value)
            {
                Jotunn.Logger.LogInfo($"Shrine creature {horde.creature}: minimumStars={minimum}, maximumStars={horde.maximumStars}, level={creature.GetLevel()}.");
            }
        }
    }
}
