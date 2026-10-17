# Valheim Fortress - API

## Overview

### Exact waves in WildShrines.yaml

`wildLevelDefinition.numPhases` sets the generated wave count (zero or omitted
keeps four). `wildLevelDefinition.explicitPhases` instead supplies exact waves,
using the same creature catalogue keys and entry fields as `ExplicitPhases` in
the API. `GetShrineChallengeDefinition` preserves both fields, so physical wild
shrines and API callers use the same event definition.

```yaml
wildLevelDefinition:
  levelIndex: 1
  biome: BlackForest
  explicitPhases:
  - - creature: GreyDwarf
      amount: 6
      stars: 2
  - - creature: Troll
      amount: 1
      stars: 3
```

Explicit lists take precedence over generated wave count, budgets, wave styles,
caps, and hard/siege composition changes. Existing reward settings still apply.
Creature rules still apply, including wave-number selectors and exact star caps.
Use actual keys from your SpawnableCreatures.yaml; the example names are not
prefab lookups. As with the existing explicit API, invalid entries are skipped.

Elder root abilities cast by Fortress-spawned creatures start their downward
floor search two metres above the target instead of 1,000 metres. This avoids
starting above the ceiling of an interior room. Other summons and world bosses
are unchanged. Root lifetime and stats retain their native behavior.

This API lets other mods drive Valheim Fortress horde challenges: define a wave set, creature
selection, and rewards in one place (the way the built-in Wild Shrines do), then run that challenge
on demand at any location with caller-chosen spawn points and a reward drop location.

To use it, copy `API.cs` into your project and add a soft dependency on Valheim Fortress to your
plugin class:

```csharp
[BepInDependency("MidnightsFX.ValheimFortress", BepInDependency.DependencyFlags.SoftDependency)]
```

All calls go through reflection, so you never need a hard reference to the Valheim Fortress assembly.

> **Keep `API.cs` up to date.** The challenge definition is serialized and crosses the soft-dependency
> boundary as JSON, so your copy of `API.cs` and the installed Valheim Fortress must agree on the format.
> The contract is version-tolerant (missing/extra fields are handled) and stamped with a schema version:
> if they drift too far, Valheim Fortress logs a clear warning telling you to re-copy `API.cs`. Re-copy
> `API.cs` from the release you build against. **Note:** the 0.36.0 release changed the wire format
> (simplified reward maps + schema version), 0.38.0 added creature rules (schema version 2), and 0.39.0
> added spawn modifiers (schema version 3); re-copy `API.cs` when updating to any of them.

### Required references

`API.cs` uses the framework assembly `System.Runtime.Serialization` (to serialize the definition
across the soft-dependency boundary). Add it to your project if it isn't already referenced:

```xml
<Reference Include="System.Runtime.Serialization" />
```

It also uses `UnityEngine` (`Vector3`) and `assembly_valheim` (`Heightmap.Biome`), which any Valheim
mod already references.

## Usage

Check that the API is available before calling it:

```csharp
if (ValheimFortress.API.IsAvailable) {
    // Valheim Fortress is loaded; safe to use.
}
```

> **Where to call it:** `RunChallenge` must be called in-world (a world/save loaded). Like the
> physical shrines it is owner-authoritative, so call it on the instance that should drive the run —
> the server in a dedicated setup, or the host in a peer-to-peer game.

### Run a tuned/generated challenge with scaled rewards

Valheim Fortress generates the creature waves from a biome, difficulty, and wave style:

```csharp
var definition = new ValheimFortress.VFChallengeDefinition {
    Biome = Heightmap.Biome.Meadows,
    Difficulty = 5,
    WaveStyle = "Normal",       // see API.GetWaveStyles()
    NumPhases = 3,
    HardMode = false,
    // Scaled rewards: key = item prefab, value = per-unit point cost.
    // The spawned amount scales with difficulty, modes, and nearby players.
    ScaledRewards = new Dictionary<string, short> { { "Coins", 5 } },
    WaveStartMessage = "The horde approaches!",
    WaveEndMessage = "$shrine_challenge_complete",
    // Shown during each between-wave pause. Supports $localization keys or literals.
    // OrderedPhrases = true plays them in order (wrapping); false (default) picks at random.
    BetweenWavePhrases = new List<string> { "Regroup!", "$shrine_phase_warning", "They return!" },
    OrderedPhrases = true
};

Vector3 rewardsLocation = Player.m_localPlayer.transform.position;
Vector3[] spawnPoints = {
    player + new Vector3(20, 0, 0),
    player + new Vector3(-20, 0, 0),
    player + new Vector3(0, 0, 20)
};

ValheimFortress.API.RunChallenge(definition, spawnPoints, rewardsLocation);
```

You can supply any number of spawn points (at least one). Each creature emerges from a randomly chosen
point, so more points spread the horde over a wider area.

### Run an explicit challenge with fixed rewards

Specify the exact creatures, counts, and stars per phase. When `ExplicitPhases` is set it overrides
the tuned fields. Creature names come from `API.GetSpawnableCreatures()`.

```csharp
var definition = new ValheimFortress.VFChallengeDefinition {
    ExplicitPhases = new List<List<ValheimFortress.VFHoardEntry>> {
        new List<ValheimFortress.VFHoardEntry> {
            new ValheimFortress.VFHoardEntry { Creature = "Greyling", Amount = 5, Stars = 0 }
        },
        new List<ValheimFortress.VFHoardEntry> {
            new ValheimFortress.VFHoardEntry { Creature = "Troll", Amount = 1, Stars = 2 }
        }
    },
    // Fixed rewards: key = item prefab, value = exact count to spawn (no scaling).
    FixedRewards = new Dictionary<string, short> { { "Wood", 20 } }
};

ValheimFortress.API.RunChallenge(definition, spawnPoints, rewardLocation);
```

You can supply both `ScaledRewards` and `FixedRewards` on the same definition; both are granted.

### Set stars and Star Level System modifiers

`CreatureRules` sets star limits and guaranteed [Star Level System](https://thunderstore.io/c/valheim/p/MidnightsFX/StarLevelSystem/)
(SLS) modifiers for the creatures in a challenge, in tuned or explicit mode. It is the same format as `creatureRules`
in the Valheim Fortress `Levels.yaml` and `WildShrines.yaml` configs.

```csharp
var definition = new ValheimFortress.VFChallengeDefinition {
    Biome = Heightmap.Biome.BlackForest,
    Difficulty = 8,
    CreatureRules = new List<ValheimFortress.VFCreatureRule> {
        // No selectors: every creature in every wave gets 1-3 stars and the Fast modifier.
        new ValheimFortress.VFCreatureRule {
            MinStars = 1, MaxStars = 3,
            Modifiers = new ValheimFortress.VFModifierSet { Minor = new List<string> { "Fast" } }
        },
        // Later rules overwrite what they set: Trolls get 4 stars (lifting the cap of 3) and Fire as well as Fast.
        new ValheimFortress.VFCreatureRule {
            Creatures = new List<string> { "Troll" },
            MinStars = 4,
            Modifiers = new ValheimFortress.VFModifierSet { Major = new List<string> { "Fire" } }
        },
        // Wave -1 is the last wave, where bosses spawn.
        new ValheimFortress.VFCreatureRule {
            Waves = new List<int> { -1 },
            SpawnTypes = new List<string> { "unique" },
            Modifiers = new ValheimFortress.VFModifierSet { Boss = new List<string> { "BossSummoner" } }
        }
    },
    ScaledRewards = new Dictionary<string, short> { { "Coins", 5 } }
};
```

In explicit mode an entry can also carry its own modifiers, which are applied after the rules:

```csharp
new ValheimFortress.VFHoardEntry {
    Creature = "Troll", Amount = 1, Stars = 2,
    Modifiers = new ValheimFortress.VFModifierSet { Major = new List<string> { "Frost" } }
}
```

Modifiers need SLS; without it only stars are applied. A creature given any modifiers this way does not also get
SLS's random ones, and by default SLS keeps one modifier per star, so give creatures at least as many stars as
modifiers.

### Shape how each creature category spreads over the waves

In tuned mode, `CommonSpawnModifiers`, `RareSpawnModifiers` and `EliteSpawnModifiers` control how many creatures
of that category each wave gets. They match the `commonSpawnModifiers`/`rareSpawnModifiers`/`eliteSpawnModifiers`
settings in `Levels.yaml` and `WildShrines.yaml`.

```csharp
var definition = new ValheimFortress.VFChallengeDefinition {
    Biome = Heightmap.Biome.Swamp,
    Difficulty = 10,
    // Commons start small and ramp up; elites only show up in the second half.
    CommonSpawnModifiers = new ValheimFortress.VFSpawnModifiers { LinearIncreaseRandomWaveAdjustment = true },
    EliteSpawnModifiers = new ValheimFortress.VFSpawnModifiers { OnlyGenerateInSecondHalf = true },
    ScaledRewards = new Dictionary<string, short> { { "Coins", 5 } }
};
```

### Run a wild shrine's challenge

Wild shrines start a challenge when offered a tribute item. You can ask Valheim Fortress for the challenge a
shrine runs for a given tribute and run it wherever you like. The definition comes from the wild shrine config
currently loaded (the server's `WildShrines.yaml` once it has synced), so it follows the server admin's changes.

```csharp
// Shrine prefab name -> tribute item prefabs it accepts, e.g. "VF_wild_shrine_green1" -> ["TrophyBoar", ...]
Dictionary<string, List<string>> tributes = ValheimFortress.API.GetShrineTributes();

var definition = ValheimFortress.API.GetShrineChallengeDefinition("VF_wild_shrine_green1", "TrophyBoar");
if (definition != null) {
    // The result is a normal definition: tweak anything before running it.
    definition.DrawMapOverlay = true;
    ValheimFortress.API.RunChallenge(definition, spawnPoints, rewardLocation);
}
```

The shrine name and tribute are case-insensitive, and a trailing `(Clone)` on the shrine name is ignored, so a
placed shrine's `gameObject.name` works. When the shrine accepts the same tribute on more than one level, the
first one is used, just like offering it at the shrine. The returned definition does not include the tribute
amount: whether and what the player pays is up to your mod. `null` means the shrine or tribute was not found;
Valheim Fortress logs a warning listing the valid names.

## Reference

### `ValheimFortress.API`

| Member | Description |
| --- | --- |
| `bool IsAvailable` | True when Valheim Fortress is loaded. |
| `bool RunChallenge(VFChallengeDefinition definition, Vector3[] spawnPoints, Vector3 rewardLocation)` | Starts a challenge. Returns true on success. |
| `bool RunChallenge(VFChallengeDefinition definition, List<Vector3> spawnPoints, Vector3 rewardLocation)` | Convenience overload. |
| `List<string> GetSpawnableCreatures()` | Valid `VFHoardEntry.Creature` / monster-filter names. |
| `List<string> GetRewardItems()` | Item prefab names suitable as reward keys. |
| `List<string> GetWaveStyles()` | Valid `WaveStyle` values. |
| `VFChallengeDefinition GetShrineChallengeDefinition(string shrineName, string tributePrefab)` | The challenge a wild shrine runs for that tribute, or `null` if not found. |
| `Dictionary<string, List<string>> GetShrineTributes()` | Every wild shrine prefab name and the tribute item prefabs it accepts. |

### `VFChallengeDefinition`

Tuned mode: `Biome`, `Difficulty`, `WaveStyle`, `NumPhases`, `MaxCreaturesPerPhase`,
`OnlySelectMonsters`, `ExcludeSelectMonsters`, `HardMode`, `BossMode`, `SiegeMode`,
`CommonSpawnModifiers`, `RareSpawnModifiers`, `EliteSpawnModifiers`.

| `VFSpawnModifiers` member | Description |
| --- | --- |
| `bool LinearDecreaseRandomWaveAdjustment` | Many in the first wave, tapering off in later waves (with some randomness). |
| `bool LinearIncreaseRandomWaveAdjustment` | Few in the first wave, ramping up in later waves (with some randomness). |
| `bool PartialRandomWaveAdjustment` | Randomly reduces each wave's amount, by up to about half. |
| `bool OnlyGenerateInSecondHalf` | None until the second half of the waves. |

Set one per category; when several are set, the last one in the table wins. With none set, every wave gets the
full amount.

Explicit mode: `ExplicitPhases` (overrides tuned fields when set). Each `VFHoardEntry` has `Creature`,
`Amount`, `Stars` and optional `Modifiers`.

Creature rules: `CreatureRules` — optional list of `VFCreatureRule`, used in both modes. Rules apply in
list order, and each rule that matches a creature overwrites only the settings it sets.

| `VFCreatureRule` member | Description |
| --- | --- |
| `List<int> Waves` | 1-based wave numbers in spawn order; negative counts from the end (`-1` = last wave). Null matches every wave. |
| `List<string> Creatures` | Creature names from `API.GetSpawnableCreatures()`, case-insensitive. Null matches every creature. |
| `List<string> SpawnTypes` | `common`, `rare`, `elite` or `unique`. Null matches every type. |
| `int? MinStars` | Lowest star count. |
| `int? MaxStars` | Highest star count; 0 forces no stars. When stars conflict the later rule wins; within one rule `MaxStars` wins. |
| `VFModifierSet Modifiers` | Guaranteed SLS modifiers: `Major`, `Minor` and `Boss` name lists. Each list set replaces that type's list from earlier rules; an empty list clears it. |

Creature behavior: `EnableCreatureDrops` — global toggle; when true, challenge creatures drop their
normal loot on death. Defaults to false (creatures drop nothing; players are rewarded only via the
challenge rewards). `CreatureDropOverrides` — optional `creature name → bool` map that overrides the
global toggle per creature (creatures not listed use the global value; unknown names are ignored).

Rewards: `ScaledRewards` (item → per-unit cost) and/or `FixedRewards` (item → exact count).

Messaging: `WaveStartMessage`, `WaveEndMessage` (support `$localization` keys). `BetweenWavePhrases` —
optional list of phrases (each a `$localization` key or literal) shown during each between-wave pause;
when non-empty it replaces the built-in phrase pool for the run (empty/null uses the built-in pool).
`OrderedPhrases` — when true the phrases play in list order (wrapping when there are more pauses than
phrases); when false (default) one is picked at random each pause.

Presentation: `DrawMapOverlay` — when true, marks each spawn point on the minimap for the duration of
the run using the vanilla event markers (shaded event-area circle plus the animated event icon, the same
markers the physical shrines and SLS raids use), cleared automatically on completion. Local/cosmetic
only: renders for the instance driving the run (single-player or the P2P host) and is skipped on a
headless dedicated server. Defaults to false.

Supported biomes for generation: Meadows, BlackForest, Swamp, Mountain, Plains, Mistlands, AshLands.
