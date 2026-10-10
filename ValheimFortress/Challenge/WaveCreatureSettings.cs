using System;
using System.Collections.Generic;
using System.Linq;
using ValheimFortress.Common;
using ValheimFortress.Data;

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
        private static readonly string[] SpawnTypes = { CONST.COMMON, CONST.RARE, CONST.ELITE, CONST.UNIQUE };

        // Resolves creatureRules onto every horde of a built wave. Rules apply in list order and each matching
        // rule overwrites the settings it sets. The result is stored on the horde, so reloads and peers spawn
        // from the resolved values without needing the rules.
        internal static void ApplyRules(PhasedWaveTemplate wave, List<VFCreatureRule> rules)
        {
            if (wave?.hordePhases == null) { return; }
            int waveCount = wave.hordePhases.Count;
            if (rules != null && VFConfig.EnableDebugMode.Value)
            {
                foreach (int waveNumber in rules.Where(rule => rule?.Waves != null).SelectMany(rule => rule.Waves))
                {
                    if (waveNumber > waveCount || waveNumber < -waveCount)
                    {
                        Jotunn.Logger.LogInfo($"creatureRules wave {waveNumber} is outside this run's waves 1..{waveCount}; it matches nothing this run.");
                    }
                }
            }

            for (int phaseIndex = 0; phaseIndex < waveCount; phaseIndex++)
            {
                foreach (HoardConfig horde in wave.hordePhases[phaseIndex])
                {
                    horde.minStars = null;
                    horde.maxStars = null;
                    horde.modifiers = null;
                    if (rules == null) { continue; }
                    foreach (VFCreatureRule rule in rules)
                    {
                        if (Matches(rule, phaseIndex + 1, waveCount, horde)) { ApplyRule(horde, rule); }
                    }
                    if (horde.modifiers != null && horde.modifiers.IsEmpty()) { horde.modifiers = null; }
                }
            }
        }

        internal static void ApplyModifiers(HoardConfig horde, VFModifierSet modifiers)
        {
            if (modifiers == null) { return; }
            horde.modifiers = (horde.modifiers ?? new VFModifierSet()).Overlay(modifiers);
            if (horde.modifiers.IsEmpty()) { horde.modifiers = null; }
        }

        private static bool Matches(VFCreatureRule rule, int waveNumber, int waveCount, HoardConfig horde)
        {
            if (rule == null) { return false; }
            if (rule.Waves != null && !rule.Waves.Any(n => (n < 0 ? waveCount + n + 1 : n) == waveNumber)) { return false; }
            if (rule.Creatures != null && !rule.Creatures.Any(name => string.Equals(name, horde.creature, StringComparison.OrdinalIgnoreCase))) { return false; }
            if (rule.SpawnTypes != null)
            {
                if (horde.creature == null || !Monsters.SpawnableCreatures.TryGetValue(horde.creature, out CreatureValues creature)) { return false; }
                if (!rule.SpawnTypes.Any(type => string.Equals(type, creature.spawnType, StringComparison.OrdinalIgnoreCase))) { return false; }
            }
            return true;
        }

        // A later rule wins a conflict with earlier ones: its minStars lifts an earlier cap, and its maxStars
        // lowers an earlier minimum. Within one rule, maxStars wins.
        private static void ApplyRule(HoardConfig horde, VFCreatureRule rule)
        {
            if (rule.MinStars.HasValue)
            {
                horde.minStars = Math.Max(0, rule.MinStars.Value);
                if (horde.maxStars < horde.minStars) { horde.maxStars = horde.minStars; }
            }
            if (rule.MaxStars.HasValue)
            {
                horde.maxStars = Math.Max(0, rule.MaxStars.Value);
                if (horde.minStars > horde.maxStars) { horde.minStars = horde.maxStars; }
            }
            ApplyModifiers(horde, rule.Modifiers);
        }

        // Warns about rules that cannot do what they say. Never rejects them: a bad rule just matches nothing.
        internal static void Validate(List<VFCreatureRule> rules, string context)
        {
            if (rules == null) { return; }
            for (int i = 0; i < rules.Count; i++)
            {
                VFCreatureRule rule = rules[i];
                string where = $"{context} creatureRules[{i + 1}]";
                if (rule == null) { Jotunn.Logger.LogWarning($"{where} is empty."); continue; }
                if (rule.Waves != null && rule.Waves.Contains(0))
                {
                    Jotunn.Logger.LogWarning($"{where} has wave 0. Waves start at 1; use negative numbers to count from the last wave.");
                }
                if (rule.Creatures != null)
                {
                    foreach (string name in rule.Creatures.Where(name => !Monsters.SpawnableCreatures.Keys.Any(key => string.Equals(key, name, StringComparison.OrdinalIgnoreCase))))
                    {
                        Jotunn.Logger.LogWarning($"{where} creature '{name}' is not a creature name from SpawnableCreatures.yaml.");
                    }
                }
                if (rule.SpawnTypes != null)
                {
                    foreach (string type in rule.SpawnTypes.Where(type => !SpawnTypes.Any(known => string.Equals(known, type, StringComparison.OrdinalIgnoreCase))))
                    {
                        Jotunn.Logger.LogWarning($"{where} spawn type '{type}' is unknown. Use {string.Join(", ", SpawnTypes)}.");
                    }
                }
                if (rule.MinStars < 0 || rule.MaxStars < 0)
                {
                    Jotunn.Logger.LogWarning($"{where} has negative stars; 0 is used instead.");
                }
                if (rule.MinStars > rule.MaxStars)
                {
                    Jotunn.Logger.LogWarning($"{where} minStars {rule.MinStars} is above maxStars {rule.MaxStars} in the same rule; maxStars wins.");
                }
                if (!rule.MinStars.HasValue && !rule.MaxStars.HasValue && rule.Modifiers == null)
                {
                    Jotunn.Logger.LogWarning($"{where} sets no minStars, maxStars or modifiers, so it does nothing.");
                }
                if (rule.Modifiers != null)
                {
                    // SLS stores one type per modifier name, so a name can only be guaranteed under one type.
                    IEnumerable<string> names = (rule.Modifiers.Major ?? new List<string>())
                        .Concat(rule.Modifiers.Minor ?? new List<string>())
                        .Concat(rule.Modifiers.Boss ?? new List<string>());
                    foreach (string name in names.GroupBy(name => name, StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1).Select(group => group.Key))
                    {
                        Jotunn.Logger.LogWarning($"{where} lists modifier '{name}' more than once; a creature can only have it once, under the first type (major, minor, then boss).");
                    }
                }
            }
        }

        internal static void ApplyToCreature(Character creature, HoardConfig horde)
        {
            if (creature == null) { return; }
            int minimum = Math.Max(0, horde.minStars ?? 0);
            long stars = Math.Max((long)creature.GetLevel() - 1, Math.Max(horde.stars, minimum));
            if (horde.maxStars.HasValue)
            {
                stars = Math.Min(stars, Math.Max(0, horde.maxStars.Value));
            }
            // Game levels are signed integers and include the base (zero-star) level.
            int level = (int)Math.Min(int.MaxValue, stars + 1);
            // SetLevel updates the networked level and vanilla health, unlike assigning m_level.
            // SLS receives the same level and persists its own cache through its public API.
            SlsIntegration.Apply(creature, level, horde.modifiers);
            if (VFConfig.EnableDebugMode.Value)
            {
                Jotunn.Logger.LogInfo($"Shrine creature {horde.creature}: minStars={horde.minStars}, maxStars={horde.maxStars}, level={creature.GetLevel()}.");
            }
        }
    }
}
