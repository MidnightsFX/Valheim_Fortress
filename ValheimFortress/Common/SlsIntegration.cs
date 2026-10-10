using System;
using System.Collections.Generic;
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

        internal static void Apply(Character creature, int level, Dictionary<string, SlsModifierType> modifiers)
        {
            bool hasModifiers = modifiers != null && modifiers.Count > 0;
            if (!SlsApi.IsAvailable)
            {
                creature.SetLevel(level);
                if (hasModifiers) { WarnOnce("Shrine slsModifiers require StarLevelSystem. Applying stars only."); }
                return;
            }

            if (!SlsApi.SupportsSpawnManaged)
            {
                creature.SetLevel(level);
                WarnOnce("Shrine creature settings require an SLS version with SetCreatureSpawnManaged and modifier APIs. Upgrade SLS; its spawn rules may override the stars.");
                return;
            }

            try
            {
                // Do this in the spawn frame, before SLS's delayed setup can change the level,
                // multiply the spawn, or remove a creature that Fortress is tracking.
                RequireSuccess(SlsApi.SetCreatureSpawnManaged(creature), nameof(SlsApi.SetCreatureSpawnManaged));
                creature.SetLevel(level);
                RequireSuccess(SlsApi.SetCreatureLevel(creature, level), nameof(SlsApi.SetCreatureLevel));

                if (hasModifiers)
                {
                    Dictionary<string, int> existing = SlsApi.GetCreaturesModifiers(creature);
                    foreach (KeyValuePair<string, SlsModifierType> modifier in modifiers)
                    {
                        int type = (int)modifier.Value;
                        if (!Enum.IsDefined(typeof(SlsModifierType), modifier.Value))
                        {
                            WarnOnce($"Invalid SLS modifier type {type} for '{modifier.Key}'; skipped.");
                            continue;
                        }
                        List<string> available = SlsApi.GetPossibleModifiers(type);
                        if (available == null || !available.Contains(modifier.Key))
                        {
                            WarnOnce($"Unknown SLS {modifier.Value} modifier '{modifier.Key}'; skipped. Names are case-sensitive.");
                            continue;
                        }
                        // SLS returns false for a modifier it already rolled. Do not add it twice.
                        if (existing != null && existing.ContainsKey(modifier.Key)) { continue; }
                        RequireSuccess(SlsApi.AddModifierToTargetCreature(creature, modifier.Key, type), nameof(SlsApi.AddModifierToTargetCreature));
                    }
                }
                RequireSuccess(SlsApi.ApplyCreatureUpdates(creature), nameof(SlsApi.ApplyCreatureUpdates));
            }
            catch (Exception exception)
            {
                creature.SetLevel(level);
                Exception cause = exception is TargetInvocationException && exception.InnerException != null
                    ? exception.InnerException : exception;
                WarnOnce($"Could not apply shrine SLS settings: {cause.Message}");
            }
        }
    }
}
