# Valheim Fortress - API

## Overview

This API lets other mods drive Valheim Fortress horde challenges: define a wave set, creature
selection, and rewards in one place (the way the built-in Wild Shrines do), then run that challenge
on demand at any location with caller-chosen spawn points and a reward drop location.

To use it, copy `API.cs` into your project and add a soft dependency on Valheim Fortress to your
plugin class:

```csharp
[BepInDependency("MidnightsFX.ValheimFortress", BepInDependency.DependencyFlags.SoftDependency)]
```

All calls go through reflection, so you never need a hard reference to the Valheim Fortress assembly.

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
    WaveEndMessage = "$shrine_challenge_complete"
};

Vector3 rewardsLocation = Player.m_localPlayer.transform.position;
Vector3[] spawnPoints = {
    player + new Vector3(20, 0, 0),
    player + new Vector3(-20, 0, 0),
    player + new Vector3(0, 0, 20)
};

ValheimFortress.API.RunChallenge(definition, spawnPoints, rewardsLocation);
```

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

### `VFChallengeDefinition`

Tuned mode: `Biome`, `Difficulty`, `WaveStyle`, `NumPhases`, `MaxCreaturesPerPhase`,
`OnlySelectMonsters`, `ExcludeSelectMonsters`, `HardMode`, `BossMode`, `SiegeMode`.

Explicit mode: `ExplicitPhases` (overrides tuned fields when set).

Rewards: `ScaledRewards` (item → per-unit cost) and/or `FixedRewards` (item → exact count).

Messaging: `WaveStartMessage`, `WaveEndMessage` (support `$localization` keys).

Supported biomes for generation: Meadows, BlackForest, Swamp, Mountain, Plains, Mistlands, AshLands.
