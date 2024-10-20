# ValheimFortress
---
## What is Valheim Fortress
When the Valheim Devs released the blog ["Fearsome Foes!"](https://www.valheimgame.com/news/development-blog-fearsome-foes) they talked about a concept called "Fortress Time!".
They did not go into details about what this idea was, but I was very excited! However, what that ended up being, while exciting was not what I was expecting.

I wanted a system that would encourage building a massive fortress, defending it, and reaping rewards for doing so!
The core system this mod presents is wave defense in a few different forms. In addition some new special defensive pieces are added to help you along the way.


Got a bug to report or just want to chat about the mod? Drop by the discord or github.
[![discord logo](https://i.imgur.com/uE6umQE.png)](https://discord.gg/Dmr9PQTy9m)
[![github logo](https://i.imgur.com/lvbP5OF.png)](https://github.com/MidnightsFX/Valheim_Fortress)

---

## Table of Contents
<!-- TOC start (generated with https://github.com/derlin/bitdowntoc) -->

- [Features](#features)
   * [The shrine of challenge](#the-shrine-of-challenge)
   * [Shrine of the Arena](#shrine-of-the-arena)
   * [Wild Shrines](#wild-shrines)
   * [Defensive Structures](#defenses)
   * [How to adjust the difficulty](#how-to-adjust-the-difficulty)
- [Configuration](#configuration)
   * [Adding Rewards](#adding-rewards)
   * [Adding Monsters](#adding-monsters)
   * [Add/Editing Levels](#addediting-levels)
   * [Adding/Editing WaveStyles](#addingediting-wavestyles)
   * [Adding/Editing Wildshrine configuration](#addingediting-wildshrine-configuration)
- [FAQ](#faq)
- [Localizations - Translations](#localizations-translations)
- [Future Features / Incomplete things](#future-features-incomplete-things)
- [Other Mods](#other-mods)
- [Credits](#credits)
- [Known issues](#known-issues)

<!-- TOC end -->

<!-- TOC --><a name="features"></a>
## Features

<!-- TOC --><a name="the-shrine-of-challenge"></a>
### The shrine of challenge

![Shrine of Challenge example](https://i.imgur.com/TGjVDoB.gif)
* Weapons shown are from [Valheim Armory](https://valheim.thunderstore.io/package/MidnightMods/ValheimArmory/)

The Shrine of Challenge is a buildable piece that allows you to call waves of enemies to attack you. It is highly configurable, and enemies will spawn outside of your build radius.

The goal with the shrine of challenge is to provide a variable but high level of difficulty raid, which the player(s) can invoke in exchange for a promised reward.

The shrine will gradually unlock more levels and rewards as the server defeats the various bosses. The shrine of challenge supports custom waves and rewards.

<!-- TOC --><a name="shrine-of-the-arena"></a>
### Shrine of the Arena

![Shrine of Arena example](https://i.imgur.com/5BK7C2y.gif)
* Weapons shown are from [Valheim Armory](https://valheim.thunderstore.io/package/MidnightMods/ValheimArmory/)

The Shrine of the Arena is somewhat the inverse of the shrine of challenge. Instead of enemies attacking from random remote locations the enemies all spawn on the shrine platform itself.

This shrine is designed to facilitate Arena fights, build yourself an Arena demonstrate your skills!

Again, this shrine is highly configurable and can use the same levels or seperate levels from the shrine of challenge. Build your own custom waves, select your favorite rewards and fight to the death!

<!-- TOC --><a name="wild-shrines"></a>
### Wild Shrines

![Wild Shrine example](https://i.imgur.com/uum1sAM.gif)
* Weapons shown are from [Valheim Armory](https://valheim.thunderstore.io/package/MidnightMods/ValheimArmory/)

Wild shrines come in a number of different flavors, one type per biome.
Each wild shrine will ask for different tribute, from their respective biome.

Upon providing the shrine with the required tribute it will spawn an easy wave of similar creatures, which will then reward you with resources from that biome upon completion.
This is all configurable!

#### Help! What is the tribute for the wildshrine?!
<details>
<summary>Wildshrine Tribute Spoiler</summary>

The default tribute for wildshrines will be biome specific trophies.
For example, the meadows wildshrine will accept boar trophies, deer trophies and neck trophies.
- The blackforest shrine accepts trophies from the black forest.
- The swamp shrine accepts trophies from the swamp.
- The mountain shrine accepts trophies from the mountain.
- The plains shrine accepts trophies from the plains.
- The mistland shrine accepts trophies from the mistlands.

</details>

<!-- TOC --><a name="defenses"></a>
### Defensive Structures

This mod also adds a few defensive structures to help you with dealing with massive invasions.

<details>
<summary>Defensive Structures</summary>

|Name|Icon|
|--|--|
|Stone Stakes| ![Stone Stakes](https://i.imgur.com/SsCC36a.png)|
|Automated Ballista| ![Automated Ballista](https://i.imgur.com/y3GGKOf.png)|

</details>

<!-- TOC --><a name="how-to-adjust-the-difficulty"></a>
### How to adjust the difficulty
The Shrine of challenge provides a number of key configuration values which can be used to adjust the difficulty level in many different ways.

|Name|Default|What it Does|
|--|--|--|
|level_base_challenge_points | 200 | The base level of points all waves have, this primarily determines early level difficulty. |
|challenge_slope | 15.0 | Multiplier used against the slope, increasing will make everything slightly harder (larger impact on later waves) and lowering will make everything easier. |
|max_challenge_points | 3000 | This is a cap on how many points a wave can generate with, it primarily limits the max wave sizes (its intentionally relatively low, feel free to tune it upwards. Who needs to keep their base in one piece anyways?) |
|creature_star_chance | 0.15 | percentage 0.001-1.0, chance that a creature will spawn as a 1+ star variant, some creatures always spawn as multi-star, others always never spawn at a higher star rate. |

<details>
<summary>Summary of the difficulty equation</summary>

And now you want to know how these values are actually used to compute the challenge points right?
look at this summary below
```
allocated_challenge_points = Log(level^2) * (challenge_slope * level) + level_base_challenge_points

if (allocated_challenge_points > max_challenge_points) { allocated_challenge_points = max_challenge_points; }
```

</details>

<!-- TOC --><a name="configuration"></a>
## Configuration
This mod is HIGHLY configurable. All buildings have configurable crafting recipes and many aspects about the shrine of challenge can be configured to your liking.

This mod uses almost exclusively server sided configuration. The server supports configuration syncing for Rewards, Monsters and the main config file.
However, all configurations related to building pieces (like the ballista cost etc) are not reloaded during runtime, and will require a game restart.

This mod does support [BepinEx in-game Configuration](https://valheim.thunderstore.io/package/Azumatt/Official_BepInEx_ConfigurationManager/)
Configurations can also be edited in the config file within your Bepinex folder `BepinEx\Config\com.midnightsfx.ValheimFortress.cfg`

Finally Creature & Rewards configurations are handled seperatly through yaml, which is defined below.

<details>
<summary>Example in-game configuration</summary>

Basic configuration view in-game.
![basic configs](https://i.imgur.com/6zebaBk.png)

</details>

<!-- TOC --><a name="adding-rewards"></a>
### Adding Rewards
Rewards can be added through yaml definitions. You can add anything, but invalid prefabs will cause errors when spawning your reward, and you will recieve nothing.
Many mods list their resouces prefabs, if you desire to have rewards from the shrine be from another mod.
[The Valheim Wiki](https://valheim.fandom.com/wiki/Valheim_Wiki) is a great resource to find prefabs of vanilla componets.

The yaml configuration can be found within your mods configuration folder, under `VFortress` eg: `BepInEx\config\VFortress`

Rewards have the following structure, which is also listed inside the configuration file itself.

```yaml
#################################################
# Shrine of Challenge Rewards Configuration
#################################################
# The below configuration values are loaded at the start of the game, and they are not actively watched for changes beyond that. You must restart your game for any changes to take effect.
#
# Rewards configurations have a number of key values
#  Coin:                               |- The name of the reward, this will be the diplayed name if there is no localization for this reward, which is likely the case for any custom entries.
#    enabled: true                     |- Whether or not the reward is enabled, you can use this to disable any vanilla rewards you do not want. At least 1 reward must be available at ALL times.
#    resource_cost: 5                  |- This is the cost to gain 1 of the particular reward. Points are generated based on how many monsters are spawned.
#    resource_prefab: "Coins"          |- This is the unity prefab name for a resource, you will often see mods list the prefabs they have added. Prefabs are also listed on the valheim wiki.
#    required_boss: "None"             |- This must be one of the following values: "None" "Eikythr" "TheElder" "BoneMass" "Moder" "Yagluth" "TheQueen"
#    rewardMinLevelIndex: 0            |- (OPTIONAL) This will require that a wave is of a certain strength in order to allow selecting this reward, disabled when set to 0.
#    rewardMaxLevelIndex: 0            |- (OPTIONAL) This reward will not be available on waves above a certain difficulty, disabled when set to 0.
```


<!-- TOC --><a name="adding-monsters"></a>
### Adding Monsters
Monsters can be added through yaml definitions. You can add any monster you want, but some custom creatures might have issues with the spawn modifications, its advised to test custom creature additions
in singleplayer before adding them to a server.

Almost all vanilla creatures are already included in the available spawn pool. But their definitions can also be tuned through these configuration files. Don't want to face golemns? Disable them.
The configuration for spawnable creatures can be found under your mods configuration folder under `VFortress` eg: `BepInEx\config\VFortress`

Creatures have the following definition structure, which is also listed inside the configuration file itself.

```yaml
#################################################
# Shrine of Challenge Creature Configuration
#################################################
# The below configuration values are loaded at the start of the game, and they are not actively watched for changes beyond that. You must restart your game for any changes to take effect.
#
# Creature configurations have a number of key values
# Neck:                    |- This is the name of the creature being added, it is primarily used for display purposes and lookups
#  spawnCost: 5            |- This is how many points from the wave pool it costs to spawn one creature, smaller values allow many more spawns.
#  prefab: "Neck"          |- This is the creatures prefab, which will be used to spawn it.
#  spawnType: "common"     |- This can either be: "common" or "rare" or "unique", uniques are "bosses", most of the wave will be made up of common spawns, with a few rare spawns per wave.
#  biome: "Meadows"        |- This must be one of the following values: "Meadows", "BlackForest", "Swamp", "Mountain", "Plains", "Mistlands". The biome determines the levels that will recieve this spawn, and how the spawn might be adjusted to
#                             fit higher difficulty waves. eg: a greydwarf spawning into a swamp level wave will recieve 1 bonus star, since it is from the black forest, which is 1 biome behind the swamp.
```


<!-- TOC --><a name="addediting-levels"></a>
### Add/Editing Levels
All of the levels used by the shrine of challenge and all other related shrines are collectively defined in the `Levels.yaml` file, which can be found under your mods configuration folder under `VFortress` eg: `BepInEx\config\VFortress`

There are many options available for configuring levels via the config files. Most of which is explained directly below.

However some additional context could be useful, one of the key componets to these level definitions is `waveFormat` which is defined in a similarly associated file and covered in a section below this. 
Wave formats are a collection of definitions that make up the percentages and creature catagories in a wave. For example, a wave could be composed of 30% COMMON enemies, 25% RARE enemies and another batch of 45% COMMON enemies.
Each of these segments will select a different creature and the different catagories (COMMON, RARE, ELITE, UNIQUE) can be manipulated seperately.

```
#################################################
# Shrines of Challenge Levels Configuration
#################################################
# levels:
# - levelIndex: 1                                                  |- LevelIndex is the difficulty this wave is set at, valid values are 1+
#   levelForShrineTypes:                                           |- What shrines will host this level, multiple definitions can be applied
#     challenge: true                                              |-   Shrine of challenge will host this level
#     arena: true                                                  |-   Shrine of the arena will host this level
#   levelMenuLocalization: $shrine_menu_meadow                     |- This is the localization that will be displayed when selecting the level, if no key matches the $lookup the literal string will be used
#   requiredGlobalKey: NONE                                        |- This is the global key required to unlock this level more available here (https://valheim.fandom.com/wiki/Global_Keys)
#   biome: Meadows                                                 |- This is the biome used for this level. This determines what creatures are considered
#   waveFormat: Tutorial                                           |- This is the format of the wave, formates are defined in WaveStyles.yaml, it determines how many creatures, what catagory and percentage of total points they use
#   bossWaveFormat: TutorialBoss                                   |- This is the format if the wave is modified to be a boss wave
#   maxCreatureFromPreviousBiomes: 0                               |- This is the maximum number of creatures that can be selected from prior biomes
#   levelWarningLocalization: $shrine_warning_meadows              |- This is the announcement text that plays when the challenge starts as a normal wave, uses literal value if the localization does not exist
#   bossLevelWarningLocalization: $shrine_warning_meadows_boss     |- This is the announcement text that plays when the challenge starts as a boss wave, localizations are available here https://github.com/MidnightsFX/Valheim_Fortress/blob/master/JotunnModStub/Localizations/English.json
#   onlySelectMonsters: []                                         |- This is an array of monsters that are the only valid targets for this wave
#   excludeSelectMonsters: []                                      |- This is an array of monsters that are to be avoided for the wave
#   levelRewardOptionsLimitedTo:                                   |- When set, only the available rewards can be selected for this level, rewards still have their normal global key requirements
#     - Coin                                                       |- The rewards entry name of rewards that should be available for this level
#   commonSpawnModifiers:                                          |- Spawn modifiers are functions applied to each part of the wave, they can be different per catagory of monster
#     linearIncreaseRandomWaveAdjustment: true                     |-   In general, it is best to only use one type of spawn modifier per creature type
#     linearDecreaseRandomWaveAdjustment: false                    |- Linear Decrease/Increase will frontload or backload this creature in the various phase of the wave, meaning more of it will appear earlier or later depending on the modifier
#     partialRandomWaveAdjustment: false                           |- Partial random adjustment will add more significant random variance to the number of creatures that will spawn
#     onlyGenerateInSecondHalf: false                              |- Only generate in second half will prevent this type of creature from spawning in the earlier waves, this is useful for Elites/Rares when LinearDecrease is set for commons
#   rareSpawnModifiers:                                            |-   The start of the wave will have many commons, and they will taper off till the end, while elites would come into play only on the second half of the wave
#     linearIncreaseRandomWaveAdjustment: true
#     linearDecreaseRandomWaveAdjustment: false
#     partialRandomWaveAdjustment: false
#     onlyGenerateInSecondHalf: false
#   eliteSpawnModifiers:
#     linearIncreaseRandomWaveAdjustment: true
#     linearDecreaseRandomWaveAdjustment: false
#     partialRandomWaveAdjustment: false
#     onlyGenerateInSecondHalf: false
#   uniqueSpawnModifiers: 
```

<!-- TOC --><a name="addingediting-wavestyles"></a>
### Adding/Editing WaveStyles

Wavestyles are the percentage breakdowns of what catagories of creatures make up a wave. This is useful for balancing waves and for providing variety to the way that waves spawn.

For example, a wave with multiple COMMON entries will select as many of the available common entries as it can for the defined level (level restricts biome, potential creature selection etc).
In the meadows this might result in: two groups of boars, or one group of boars and one group of Necks, or one boars and greylings or two boars. (since all creatures in the meadows are common by default).
It is important to pay attention to how waves are defined and how you might have customized your creature definitions.
For example if you made every creature common, many waves would spawn with less than the full amount of creatures and the amount of randomness would increase- but it might feel more like chance and less balanced.

```
#################################################
# Shrine of Challenge WaveStyles Configuration
#################################################
# WaveStyles configurations have a number of key values
# Easy:                      |- This is the key used to lookup this wave definition
#  WaveConfig                |- The wave configuration for each segment of the wave
#   - type: COMMON           |- This is the catagory of creature that will be selected
#     percent: 30            |- This is the percentage of the waves total point pool that will be used for this spawn
```

<!-- TOC --><a name="addingediting-wildshrine-configuration"></a>
### Adding/Editing Wildshrine configuration

Wildshrine configuration is split into three important parts.
First part is the top level definition that defines which shrine this configuration will be applied to. There should only be one configuration per shrine type.
If there are multiple configurations per shrine type it is likely that the first defined one will load. If there are no configurations for a shrine, and that shrine is enabled, you will likey experiance errors.

The second important part here is the wildShrineLevelsConfig array, which is the definition for what tribute and rewards a defined wave will have, in addition to the warning and finish messages. 
There can be as many levels defined for a shrine as you desire, but they should all have unique tribute requirements (the tribute prefab must be unique).

The third part of this definition is the wildLevelDefinition definition, these are abbreviated level definitions which will build out levels for the specified wildshrine based on the tributed required to activate it.
In the example below here there is a wave of boars and greywdarfs that will spawn using the the 'Tutorial' wavestyle, at a difficulty of 2, with a maximum of 15 creatures per phase (4 phases by default, 8 for siege mode)
The final part of the wave definition here are the spawn modifiers, you can find a full list of these modifiers on the Level definitions. `linearIncreaseRandomWaveAdjustment` will start the number of spawns out small,
and increase them until the final wave (with random noise, making the increases less linear).

```
###################################################################################################################################################
# Wild Shrine Configuration
###################################################################################################################################################
# wildShrines:
# - definitionForWildShrine: VF_wild_shrine_green1                    |- The prefab that this set of configuration will be applied to
#   wildShrineNameLocalization: $wild_shrine_green                    |- The localization for the prefabs name (when hovered over) this uses a lookup value but defaults to its literal value
#   wildShrineRequestLocalization: $wild_shrine_green_request         |- What the shrine says when you interact with it
#   shrineUnacceptedTributeLocalization: $wild_shrine_not_interested  |- What the shrine says when you offer an incorrect tribute
#   shrineLargerTributeRequiredLocalization: $wild_shrine_hungry      |- What the shrine says when you do not offer enough tribute
#   wildShrineLevelsConfig:                                           |- Level configurations related to this shrine
#   - tributeName: TrophyBoar                                         |- The prefab name of the tribute required to activate this level
#     tributeAmount: 4                                                |- Amount of the tribute required to activate this level
#     rewards:                                                        |- Rewards for this level in the format of Prefab: cost eg: RawMeat: 14.
#       LeatherScraps: 14
#       RawMeat: 12
#     hardMode: false                                                 |- If hardmode should be enabled for this level (doubles the spawn point pool and gives 50% more rewards)
#     siegeMode: false                                                |- If siege mode should be enabled for this level (double the number of waves 4->8 and gives 50% more rewards)
#     wildshrineWaveStartLocalization: $wild_boars_attack             |- Localization text to display when this wave starts
#     wildshrineWaveEndLocalization: $wild_boars_defeated             |- Localization text to display when this wave is finished
#     wildLevelDefinition:
#       levelIndex: 2                                                 |- The difficulty level for this wave, valid values are 1+ (Refer to the readme for a breakdown of this equation)
#       biome: Meadows                                                |- The biome this wave is for, this impacts creature selection
#       waveFormat: Tutorial                                          |- The wavestyle this uses (from wavestyles.yml), this governs which catagories and the percentage makeup of the wave
#       levelWarningLocalization: $meadows_warning_wilderness         |- Localization for a between phase warning (often not used)
#       maxCreaturesPerPhaseOverride: 15                              |- Overrides the max creatures per wave to be this value (overrides the global config)
#       onlySelectMonsters:                                           |- Set of monsters that can be selected (From monsters.yml)
#       - Boar
#       - Greyling
#       excludeSelectMonsters:                                        |- Set of monsters that can't be selected (from monsters.yml) best used when OnlySelected is not set.
#       commonSpawnModifiers:                                         |- Spawn modifiers for common creatures
#         linearIncreaseRandomWaveAdjustment: true
#       rareSpawnModifiers:                                           |- Spawn modifiers for rare creatures
#       eliteSpawnModifiers:                                          |- Spawn modifiers for elite creatures
```

<!-- TOC --><a name="faq"></a>
## FAQ

Q. I am running an older version and do not see any of the wildshrines, what gives?
	A. You need to run `genloc` (as an admin on the server), this will freeze your client for a little, and add any missing locations to your world (IN UNEXPLORED AREAS). Please keep in mind that genloc can move existing locations etc and is generally advised against running it on large existing servers.

Q. I can't craft all of the pieces from this mod! They arn't visible in the hammer
	A . You should install and use [SearsCatalog](https://valheim.thunderstore.io/package/ComfyMods/SearsCatalog/), this will allow the hammer panels to be resized/scrolled to fit any and all prefabs added. This may no longer be necessary on newer Valheim versions.

Q. There are remaining creatures and I can't find them!?
	A . Interacting with the shrine while the challenge is active gives you the option to summon fireworks on creatures or teleport them to the shrine (if there are only a few left, this is configurable)

Q. I broke my configuration files and want to try again.
	A. You can delete any/all yaml configuration (or the primary config file) from this mod and it will be automatically generated again for you on startup.

Q. Wave generation seems insanely unbalanced, what gives?
	A. Delete your configurations (SpawnableCreatures.yaml in the VFortress folder) and MidnightsFX.ValheimFortress.cfg, this will regenerate new configurations with the defaults.
	If you are trying to increase or lower the difficulty from this base point, it is recommended you start by decreasing/increasing the difficulty slope in small increments	

Q. Rewards are too generous / rewards arn't rewarding enough!
	A. You can configure all of the reward costs, and the bonuses that are applied to determine the pool for rewards. I suggest you read the main mods configuration file and look at the rewards config file.

<!-- TOC --><a name="localizations-translations"></a>
## Localizations - Translations
I accept community translations! Existing localizations can be found [here](https://github.com/MidnightsFX/Valheim_Fortress/tree/master/JotunnModStub/Localizations). 
I will keep the English translation up to date, if you would like to provide a translation feel free to reach out to me on discord or open up a github issue.

<!-- TOC --><a name="future-features-incomplete-things"></a>
## Future Features / Incomplete things
There are a number of things that I plan on adding in the future. Here is the current list.

* Shrine of Challenge
	* Add ward support checking to ensure that the player interacting with the shrine is allowed
	* Add a failsafe for waves to despawn after a long period of time (if the player is unable to kill them etc)

* defenses
	* Another tier of trap for the plains
	* Another tier of walls

<!-- TOC --><a name="other-mods"></a>
## Other Mods
If you like this mod maybe you'll like my other work

[![Valheim Armory](https://i.imgur.com/GofTtar.png)](https://valheim.thunderstore.io/package/MidnightMods/ValheimArmory/)

<!-- TOC --><a name="credits"></a>
## Credits
- A big thank you to Margmas, Venture, Redseiko and Probablykory for providing some examples and answers to my silly questions (and continuing to do so!)
- The valheim team for continuing to develop valheim, even after their initial roadmap!
	- The continued blogposts about upcoming Valheim content, which got my gears grinding to make this mod
- Unity Ultimate VFX for some or partial visual effects
- Traslations credits to: Azathoth

<!-- TOC --><a name="known-issues"></a>
## Known issues
- Mobs can form a 'spawn tower' (especially common when using the Arena spawner with high spawn limits)
