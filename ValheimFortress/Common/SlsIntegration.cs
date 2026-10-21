using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ValheimFortress.Challenge;
using SlsApi = StarLevelSystem.API;

namespace ValheimFortress.Common
{
    // Apply shrine settings through SLS's published optional API wrapper.
    internal static class SlsIntegration
    {
        private static readonly HashSet<string> Warnings = new HashSet<string>();

        private static void WarnOnce(string message)
        {
            if (Warnings.Add(message)) { Jotunn.Logger.LogWarning(message); }
        }

        private static void RequireSuccess(bool success, string method)
        {
            if (!success)
            {
                throw new InvalidOperationException($"SLS {method} returned false.");
            }
        }

        internal static void Apply(Character creature, int level, VFModifierSet modifiers, float healthMultiplier = 1f)
        {
            bool hasModifiers = modifiers != null && !modifiers.IsEmpty();
            if (!SlsApi.IsAvailable)
            {
                creature.SetLevel(level);
                ApplyVanillaHealth(creature, healthMultiplier);
                if (hasModifiers) { WarnOnce("Shrine creatureRules modifiers require StarLevelSystem. Applying stars only."); }
                return;
            }

            if (!SlsApi.SupportsSpawnManaged)
            {
                creature.SetLevel(level);
                ApplyVanillaHealth(creature, healthMultiplier);
                WarnOnce("Shrine creature settings require an SLS version with SetCreatureSpawnManaged and modifier APIs. Upgrade SLS; its spawn rules may override the stars.");
                return;
            }

            try
            {
                // Do this in the spawn frame, before SLS's delayed setup can change the level,
                // multiply the spawn, or remove a creature that Fortress is tracking. SLS needs the
                // level set before modifiers are added, or it rolls its own level for them.
                RequireSuccess(SlsApi.SetCreatureSpawnManaged(creature), nameof(SlsApi.SetCreatureSpawnManaged));
                creature.SetLevel(level);
                RequireSuccess(SlsApi.SetCreatureLevel(creature, level), nameof(SlsApi.SetCreatureLevel));
            }
            catch (Exception exception)
            {
                creature.SetLevel(level);
                ApplyVanillaHealth(creature, healthMultiplier);
                WarnOnce($"Could not apply shrine SLS settings: {Cause(exception).Message}");
                return;
            }

            if (hasModifiers) { AddModifiers(creature, level, modifiers); }
            if (HasHealthMultiplier(healthMultiplier)) { MultiplyBaseHealth(creature, healthMultiplier); }

            try
            {
                RequireSuccess(SlsApi.ApplyCreatureUpdates(creature), nameof(SlsApi.ApplyCreatureUpdates));
            }
            catch (Exception exception)
            {
                WarnOnce($"Could not apply shrine SLS settings: {Cause(exception).Message}");
            }
        }

        private static bool HasHealthMultiplier(float multiplier)
        {
            return multiplier > 0f && multiplier != 1f;
        }

        // SLS keeps base health as a multiplier in its creature cache, which it saves with the creature, so the
        // boost survives reloads and ownership changes. Falls back to vanilla max health if SLS refuses.
        private static void MultiplyBaseHealth(Character creature, float multiplier)
        {
            try
            {
                float current = SlsApi.GetCreatureBaseAttribute(creature, 0);
                if (current <= 0f) { current = 1f; }
                RequireSuccess(SlsApi.SetCreatureBaseAttribute(creature, 0, current * multiplier), nameof(SlsApi.SetCreatureBaseAttribute));
            }
            catch (Exception exception)
            {
                WarnOnce($"Could not apply a shrine health multiplier through SLS: {Cause(exception).Message}");
                ApplyVanillaHealth(creature, multiplier);
            }
        }

        // Without SLS: raises the networked max health. Vanilla recomputes it from the prefab when an undamaged
        // creature reloads, so this lasts while the creature stays loaded.
        private static void ApplyVanillaHealth(Character creature, float multiplier)
        {
            if (!HasHealthMultiplier(multiplier)) { return; }
            float health = creature.GetMaxHealth() * multiplier;
            creature.SetMaxHealth(health);
            creature.SetHealth(health);
        }

        // Adding any modifier stores the creature's modifier list before SLS's setup runs, so SLS does not
        // roll its own random modifiers on top. One failed modifier is skipped without stopping the rest.
        private static void AddModifiers(Character creature, int level, VFModifierSet modifiers)
        {
            Dictionary<string, int> existing = null;
            try { existing = SlsApi.GetCreaturesModifiers(creature); }
            catch (Exception exception) { WarnOnce($"Could not read SLS modifiers: {Cause(exception).Message}"); }
            HashSet<string> present = new HashSet<string>(existing?.Keys ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);

            int added = 0;
            foreach (KeyValuePair<SlsModifierType, List<string>> group in new[] {
                new KeyValuePair<SlsModifierType, List<string>>(SlsModifierType.Major, modifiers.Major),
                new KeyValuePair<SlsModifierType, List<string>>(SlsModifierType.Minor, modifiers.Minor),
                new KeyValuePair<SlsModifierType, List<string>>(SlsModifierType.Boss, modifiers.Boss) })
            {
                if (group.Value == null || group.Value.Count == 0) { continue; }
                int type = (int)group.Key;
                List<string> available;
                try { available = SlsApi.GetPossibleModifiers(type); }
                catch (Exception exception)
                {
                    WarnOnce($"Could not read SLS {group.Key} modifiers: {Cause(exception).Message}");
                    continue;
                }
                foreach (string requested in group.Value)
                {
                    string name = available?.FirstOrDefault(known => string.Equals(known, requested, StringComparison.OrdinalIgnoreCase));
                    if (name == null)
                    {
                        WarnOnce($"Unknown SLS {group.Key} modifier '{requested}'; skipped. Check the {group.Key} modifiers in SLS's Modifiers.yaml.");
                        continue;
                    }
                    // SLS keeps one type per modifier and refuses a modifier the creature already has.
                    if (!present.Add(name)) { continue; }
                    try
                    {
                        if (SlsApi.AddModifierToTargetCreature(creature, name, type)) { added++; }
                        else { WarnOnce($"SLS refused {group.Key} modifier '{name}' for shrine creature {creature.m_name}; skipped."); }
                    }
                    catch (Exception exception)
                    {
                        WarnOnce($"Could not add SLS modifier '{name}': {Cause(exception).Message}");
                    }
                }
            }

            int stars = level - 1;
            if (present.Count > stars)
            {
                WarnOnce($"Shrine creature {creature.m_name} has {present.Count} SLS modifiers but {stars} stars. With SLS's default settings a creature keeps one modifier per star, so the extra ones can be lost when it reloads. Raise minStars to keep them.");
            }
            if (VFConfig.EnableDebugMode.Value)
            {
                Jotunn.Logger.LogInfo($"Added {added} SLS modifiers to shrine creature {creature.m_name}.");
            }
        }

        private static Exception Cause(Exception exception)
        {
            return exception is TargetInvocationException && exception.InnerException != null
                ? exception.InnerException : exception;
        }
    }
}
