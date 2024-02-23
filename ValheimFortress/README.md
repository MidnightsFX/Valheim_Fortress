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
   * [Cosmetics](#cosmetics)
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
- [Changelog](#changelog)

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

<!-- TOC --><a name="cosmetics"></a>
### Cosmetics
The secondary goal of this mod is to help in providing cosmetics and functional base building pieces!

These pieces likely need better color balancing, and might have other oddities at this point. I've got a slew of cosmetics that I would like to add. It will take time though.

<details>
<summary>Cosmetics</summary>

|Name|Icon|
|--|--|
|Green Circle Rug| ![Green circle rug](https://i.imgur.com/59WStBA.png)|
|Red Circle Rug| ![Red circle rug](https://i.imgur.com/lpGMbPz.png)|
|Yellow Circle Rug| ![Yellow circle rug](https://i.imgur.com/BDuAfcO.png)|
|Green Crystal wall| ![Green crystal wall](https://i.imgur.com/7uC5Td1.png)|
|Blue Crystal wall| ![Blue crystal wall](https://i.imgur.com/2XjuTHZ.png)|
|Red Crystal wall| ![Red crystal wall](https://i.imgur.com/ESTRoai.png)|
|Yellow Crystal wall| ![Yellow crystal wall](https://i.imgur.com/28FaMnE.png)|

</details>

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
	A . You should install and use [SearsCatalog](https://valheim.thunderstore.io/package/ComfyMods/SearsCatalog/), this will allow the hammer panels to be resized/scrolled to fit any and all prefabs added.
		If you downloaded from Thunderstore sears catalog will be included automatically as a dependency. However VF does not have a hard dependency on SearsCatalog and can be used without it if desired.

Q. There are remaining creatures and I can't find them!?
	A . Interacting with the shrine while the challenge is active gives you the option to summon fireworks on creatures or teleport them to the shrine (if there are only a few left, this is configurable)

Q. I broke my configuration files and want to try again.
	A. You can delete any/all yaml configuration (or the primary config file) from this mod and it will be automatically generated again for you on startup.

Q. Wave generation seems insanely unbalanced, what gives?
	A. Delete your configurations (SpawnableCreatures.yaml in the VFortress folder) and MidnightsFX.ValheimFortress.cfg, this will regenerate new configurations with the defaults.
	If you are trying to increase or lower the difficulty from this base point, it is recommended you start by decreasing/increasing the difficulty slope in small increments

Q. The skeltons are attacking the greydwarfs again
	A. The faction changes (and drop removal etc) are not persisted across game restarts. So if you save/quit during a challenge the remaining creatures will not act the same when you log back in, 
		and will revert back to their vanilla settings. Loosing stars, gaining their loot, loosing their connection to the shrine. It won't cause issues for your game. But you will still have to kill them normally, and won't get shrine rewards for it.
 
Q. My game freezes or becomes very slow for some period of time during waves or when creatures are spawning, what do I do?
	A. This mod is fairly intensive on the CPU (during wave generation), in order to perform all of the calculations that go into creating an interesting and varied wave- then actually spawning all of those creatures (and the potentially massive number of interactions with them)
	   can be very taxing on your computer. If you find this to be an issue consider reducing the configured number of maximum creatures to spawn or increasing the number of cores used for GC collection.
		A. Reduce `max_creatures_per_wave`
		A. Optionally, Increase GC collection duration. From Steam, Right Click on Valheim -> Manage -> Browse Local Files. Go to valheim_Data folder and open boot config file. change `gc-max-time-slice=3` to `gc-max-time-slice=10`	
	    this will allow scripts to execute for a longer timeperiod, which will allow these long-running scripts to execute without killing your game.
		

Q. My game freezes, crashes or becomes very slow when the reward is spawned, what do I do?
	A. Reduce the value of `MaxRewardsPerSecond`, this will spread out reward spawning over a longer period and reduce the impact on your game.
	A. Optionally, Increase GC collection duration. From Steam, Right Click on Valheim -> Manage -> Browse Local Files. Go to valheim_Data folder and open boot config file. change `gc-max-time-slice=3` to `gc-max-time-slice=10`
	this will allow scripts to execute for a longer timeperiod, which will allow these long-running scripts to execute without killing your game.

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

* cosmetics
	* Whitemarble cosmetic variant of blackmarble
	* Torch color variants
	* Lampost color variants
	* Darkwood variant to aesthetically fit other tar coated wood pieces

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
- Building pieces sometimes don't have destructable bits
- Building pieces don't have wear and tear
- Mobs can form a 'spawn tower' (especially common when using the Arena spawner with high spawn limits)
- Automated turret likes to fire off into space instead of hitting its target occassionally (its aim isn't perfect, so sometimes this is intended)
	- If you can reliably reproduce this issue please report it on the Github or Discord
- Singleplayer logging out during a challenge will result in the challenge dissappearing (looking into improvements)

<!-- TOC --><a name="changelog"></a>
## Changelog

**0.20.0**
```
- Optimized enable/disable of the central shrine portal VFX
- Added more hidden debug logging to clarify actions as they occur around portal spawn/enablement
- Added rough translation text for all remaining 26 languages with untranslated localization entries
	- If you find a localization that is inaccurate or would like to improve it, I take community translations!
- Migrated all custom pieces to vanilla piecetabs in the hammer
	- It is possible that there are now too many pieces, it is recommended that you use SearsCatalog to view/expand the hammer panel
	- SearsCatalog is now a soft dependency of VF. VF runs without it, but it is highly recommended to ensure you can actually place all the pieces added
- Removed the gladiator configuration option for the shrine of challenge
- Added a shrine of the arena which takes the place of the gladiator config option. Uses a new building graphic that is much less obtrusive for arena fights
- Removed MaxChallengeLevel
	- Levels are now defined by yaml configuration, you can add your own custom levels or tweak existing ones
- Exposed level most elements of level configuration
	- Levels.yaml contains the definition for all levels by default
	- Server-sync'd values, with hot-reloading, you can edit/update levels in the game (changes are applied on save)
- Added wildshrines!
	- Wild shrines currently spawn in Meadows, Blackforest, Swamp, Mountains, Plains and Mistlands.
	- Wild shrines have configurable level definitions, that are activited by providing the correct tribute
		- eg: Shrine of the Meadows will accept NeckTrophies and spawn a challenge consisting largely of Necks, for a reward of Meadows resources
	- Wildshrine configuration is almost entirely exposed via yaml. You can edit rewards, difficulty, monsters included in generation of the waves etc
	- Wildshrines can be configured to use any monsters defined in the Monsters.yml, and spawn any rewards (does not need to be configured in the rewards yaml)
- Fixes for an edgecase where the owner of a znet region rapidly changes during a shrine challenge
- Fixed a bug where disabling a creature from level generation would still include it in the generation pool, but would not spawn it (it is now properly completely excluded)
- Reduced texture sheen of the shrine of challenge and wildshrines
```

<details>
  <summary>Full Changelog</summary>

**0.9.20**
```
- Fixes a freeze that may occur when shrine spawn radius is 100% invalid
```

**0.9.19**
```
- Made spawn point determination async. This may delay the time it takes to start a wave a little bit but will support a much larger number of attempts to spawn a portal
- For non-gladiator mode, portal generation still moves in segments roughly 10% of the maximum distance at a time.
- Fixes freezes that may occur related to being unable to determine a spawn location around the shrine in a short period of time (freeze when you click 'To Valhalla')
```

**0.9.18**
```
- Fix for Gladiator mode not skipping portal generation
- Reducing the maximum number of portal generation attempts to help prevent primary threadlock
```

**0.9.17**
```
- Updating Jotunn minimum version
- Updating BepInEx minimum version
- Changing inclusion of the yaml.net lib to be repacked instead of merged to avoid issues with thunderstores new assembly scanner
```

**0.9.16**
```
- Fixes a NPE error that could occur when unloading and reloading distant portals
- Fixed a consistency issue with creatures that are destroyed by the shrine not always staying dead, which could occur on reloading
- Added more flavor text variety between waves
```

**0.9.15**
```
- Disabled some extra debug logging lines
- Fixed a misspelling in template example/definition for rewards
- Fixes TeleportCreatureThreshold to be respected and configurable
```

**0.9.14**
```
- Fixes an issue where the shrine UI would not function normally after being placed in the current play session, but would work fine after a reload
- Fixes an error related to the cancel UI not being available
- Changes how creatures are destroyed to prevent client desynchronization on challenge forfeit
- Adds spanish translation!
```

**0.9.13**
```
- Fixes for portals reappearing after world/region reload
- Optimized vfx textures a little
```

**0.9.12**
```
- Added a new UI for the shrine providing in-challenge actions
	- Cancel challenge option (kills spawned enemies)
	- Enable flares on existing enemies (to help find them)
	- Teleport last enemies to the shrine (must be less than 6 enemies)
	- Cleanup portals
- Sync improvements to number of creatures remaining, fixing non-znet hosts from accidentally stopping challenges
- Increased the default shrine announcement range
- Added Zsync'd state for post-challenge cleanup to prevent cleanup scripts from running regularly
- Removed the auto-cancel challenge if shrine is interacted with and there are zero enemies
- Removed the configuration option to cleanup portals on a regular iterval
- Added more safety checks to the portal removal process
- Changed the default portal removal process and added some vfx
```

**0.9.11**
```
- Improvements to portal cleanup, now with more clients getting portals cleaned up!
```

**0.9.10**
```
- Fix for potentially unresponsive UI that primarily occurs after the shrine is built
- Fixes for portals not being visible to everyone in multiplayer
- Fixes for shrine announcements not being visible to every player in multiplayer
- Fixes for multiplayer area sychronization issues
- Interact to reset if the shrine gets stuck at (0) creatures
- Cleanup methods for removing orphaned portals
```

**0.9.9**
```
- Fixes a potential phase-skip issue
- Improves the wave-generations abilities to reduce the wave size to smaller quantities
- Reduced the minimum wave reduction size, so you can now face smaller waves with more powerful enemies, if you so choose
- Added spawning jitter for each phase, adding a delay when spawning large amounts of enemies
- Randomized the orientation of spawn portals
- Added more wave pause flavortext
```

**0.9.8**
```
- fix for rare NPE exceptions with multiple players loading the shrine at different points
```

**0.9.7**
```
- bugfix for a case where parts of the UI might not regenerate after being re-opened
```

**0.9.6**
```
- Improved support for toggling various shrine settings on/off during gameplay
- Improved support for estimating values
- Added a system to send flares & teleport remaining creatures to shrine, with configuration options- for when you just can't find those remaining enemies
- Fixed situations where the UI could generate without its button clicks being wired up
```

**0.9.5**
```
- Optimized the shrine menu calculations for how many rewards will be recieved
- Optimized reward spawning code to distribute it over many updates
- Provided configuration to increase/decrease how fast rewards spawn
```

**0.9.4**
```
- More improvements to consistency of server sync'd configurations (building settings are not applied during without a restart)
- Added configuration to tune rewards value increase per level
- Added configuration to tune rewards base increase
- Added configuration to enable/disable displaying of an estimate for the amount of rewards you will recieve
- Updated default values of rewards to scale considerably higher
- Fix for boss waves not spawning their bosses
```

**0.9.3**
```
- Fix rewards/main config server sync interchange
```

**0.9.2**
```
- Fixed server file sync and config file location for linux servers
- Added configuration for the max number of creatures in a wave, the generator will attempt to reduce creatures to this point (by upgrading their stars)
- Added a configuration option for the max stars (0-15), more than 2 stars will have no effect if you do not have CLLC
- Added filesync support for the primary config file
	- Recipes (like whats required to build something) are not hot-reloaded.
```

**0.9.1**
```
- Fix for potential error from generating a wave and adding a duplicate creature
- Added max_creatures_per_wave as a configuration option, reducing this will reduce the number of creatures that spawn at once
	- Reducing this will result in more creatures being upgraded to higher stars, the overall difficulty remains largely the same
- Reduced the default spawn radius for shrine portals to be 100
```

**0.9.0**
```
- Overhauled main configuration & creature configuration (IT IS RECOMMEND YOU DELETE YOUR CONFIGS!)
- Added a filewatcher for the Rewards.yaml & SpawnableCreatures.yaml, meaning edits during gameplay will be reflected if they are valid
- Overhauled spawning wave generation
	- Rewards are now significantly larger
	- Waves now spawn in seperated segments, providing a small amount of time for recovery in-between
	- Rebalanced wave generation form
	- Waves now have a chance to spawn from all portals at the same time
	- Increased rewards scaling with levels, now higher levels will give a much larger reward
	- Allowed duplicate creature types in a wave, which will result in more of a singular type spawning
	- Reduced the chance for creatures from previous biomes to spawn in the current biome wave to 5% (from 20%)
	- Added a configuration option to control how frequently previous biome creatures are added
- Chanced challenge modes slightly
	- Boss mode will now generate a boss on the final part of a wave (instead of earlier)
	- Hardmode will now double the pointpool for spawns (for every part of the wave)
	- Siege mode will double the number of waves faced for a challenge
- Added a seperate debug configuration for the turrets, since they are very noisy
```

**0.8.3**
```
- !!CHANGED CONFIG LOCATION!! now MidnightsFX.ValheimFortress.cfg
- Fixed boss loot enable/disable, now correctly will allow boss loot
- Removed more informational logging
- Fixed some configs that were not Admin only (server enforced)
- More wave generation tuning, mountains, plains, and mistlands should no longer feel impossible on their first levels
- Added an enabled/disabled config for all spawnable creatures, if you really hate fighting something you can just disable it now
```

**0.8.2**
```
- Added an additional spawn type 'elite' which now includes especially challenging creatures like: trolls, abominations, golemns etc.
- Added a configuration option to turn on/off the map ping on wave spawn (defaults to off)
- Significant tuning to the way waves are spawned
	- Each biome now increases in difficulty as you go up in level, and resets some of the difficulty upon starting a new biome
	- Waves will no-longer scale non-linearly with the amount of points, this reduces the exponential creature explosions at high levels
	- Waves now have fewer common enemies and slightly more rare/elite enemies
- Fixed the way stars were being assigned to creatures, all creatures from previous biomes are now 1-3 stars
- There is a configurable chance for a fraction of a spawn type to spawn as 1 stars, this is rolled for each spawn cohort
- Made wave spawn portals only dissappear once the waves have been killed
```

**0.8.1**
```
- Optimized download size, removed potential duplicate embedded libraries
```

**0.8.0**
```
- Added Dynamic yaml configuration for Rewards and available creature spawns
	- Additional rewards can be added through new entries, existing rewards can be modified and disabled in the same way
	- Monsters can be added as possible spawns, configured for which biome they spawn from and how much their spawncosts are, or disabled entirely.
```

** 0.7.3**
```
- Fixed level 5 having an infinite loading loop crash to using the wrong level data
```

** 0.7.2**
```
- Disable map drawing overlay due to potential errors
- Enable max level configuration
	- Max level is currently set to 30, 5 levels for each biome
- Made all of the shrine modifiers enable/disable-able
```

**0.7.1**
```
- Added spawn portals at the remote locations where enemies will spawn
- Fixed the readme formatting :)
```

**0.7.0**
```
- Overhauled the spawning system for the shrine of challenge
	- Support for starred creatures has been added
	- More dynamic wave formation is now possible
	- Changed the configuration values available to tune horde generation, removed some configs
	- Exposed all the primary difficulty scale variables as configs
	- Added modifiers! Earn more rewards for more significant challenges
	- Bosses are now spawned as a modifier
- Fixed turrets firing whenever they aquired a target, instead of firing their first shot at the actual target
- Reduced chances the turret will hit itself when firing
- Removed some debugging logspam related to turret build visualization
- Fixed the turrets preview aiming pattern visualization
- Moved more log output behind the debug flag
```

**0.6.1**
```
- Fixed Shrine of Challenge interaction with custom hammer mods
- Added a custom ballista that is more expensive and does not require ammo (and does not shoot at friendlies!)
- Added configuration values for Shrine base difficulty, ramp per level, difficulty slope and the maximum points that any wave can have
```

**0.6.0**
```
- Updated recipe parsing to support recovery of crafting componets for structures
- Fix monster types fighting each other, all spawned enemies are now 'boss' type, and will not be fought by anything else
- Localization of the rewards selector
- Hugin tutorial for the shrine of challenge
- Shrine of challenge now has more collision with the world around it
- Added stone stakes! Filling the gap between Corewood & Dverger stakes, they are lower damage, but take longer to destroy
```

**0.5.2**
```
- Localization works in almost every spot now (rewards selector text being the exception)
- Added a small example of what its like using this mod
```

**0.5.1**
```
- Fixes for the UI immediately closing when being opened with a key that is also used for other keybinds
- Fixes for the UI being unopenable by players that are not currently in control of the region
- Fixes for the spawn-in portal being enabled at the wrong times
- Reduction in the amount of log-spam that debug mode has when spawning creatures.
```

**0.5.0**
```
- Initial beta release!
```

</details>
