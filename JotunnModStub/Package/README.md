# ValheimFortress
---
## What is Valheim Fortress
When the Valheim Devs released the blog ["Fearsome Foes!"](https://www.valheimgame.com/news/development-blog-fearsome-foes) they talked about a concept called "Fortress Time!".
They did not go into details about what this idea was, but I was very excited! However, what that ended up being, while exciting was not what I was expecting.

I wanted a system that would encourage building a massive fortress, defending it, and reaping rewards for doing so!

So now you know half of what this mod means to be. The other half is primarily cosmetic, and ease of use.
That being, more colorways for buildable pieces along with new variants with the goal of helping ensure large-scale base defense does not become too tedious.

Got a bug to report or just want to chat about the mod? Drop by the discord or github.
|||||
|--|--|--|--|
| Discord | [![discord logo](https://i.imgur.com/uE6umQE.png)](https://discord.gg/Dmr9PQTy9m) | Github | [![github logo](https://i.imgur.com/lvbP5OF.png)](https://github.com/MidnightsFX/Valheim_Fortress) |


### Localizations - Translations
I accept community translations! Existing localizations can be found [here](https://github.com/MidnightsFX/Valheim_Fortress/tree/master/JotunnModStub/Localizations). 
I will keep the English translation up to date, if you would like to provide a translation feel free to reach out to me on discord or open up a github issue.

## Features
Wave survival through the Shrine of Challenge. Cosmetic building variants, and functional building variants.

### The shrine of challenge

![Shrine of Challenge example](https://i.imgur.com/InPMrmL.gif)

This is what the vast majority of the code from this mod supports. A building which allows you to call dangerous enemies to attack you, in exchange for a reward.
Do you like fighting? Well you can now fight to get more resources!

The goal with the shrine of challenge is to provide a variable but high level of difficulty raid, which the player(s) can invoke in exchange for a promised reward.

The shrine will gradually unlock more levels and rewards as the server defeats the various bosses.

The shrine of challenge is highly configurable. However it only updates configuration values at startup. This is something I am considering changing, but it massively simplifies this mod.


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
#    resouce_cost: 5                   |- This is the cost to gain 1 of the particular reward. Points are generated based on how many monsters are spawned.
#    resource_prefab: "Coins"          |- This is the unity prefab name for a resource, you will often see mods list the prefabs they have added. Prefabs are also listed on the valheim wiki.
#    required_boss: "None"             |- This must be one of the following values: "None" "Eikythr" "TheElder" "BoneMass" "Moder" "Yagluth" "TheQueen"
```


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

## FAQ

Q. The portals from my last attempt didn't dissapear!
	A . Starting a new challenge with `EnablePortalCleanupMode` on will remove a number of local portals that failed to get deleted beforehand. You can also turn off this configuration if you find it causes lag for you.

Q. There are remaining creatures and I can't find them!?
	A . By default, interacting with the shrine when at or below 10 creatures will summon fireworks on them. Interacting at or below 3 creatures will summon those creatures directly to the shrine.

Q. I broke my configuration files and want to try again.
	A. You can delete any/all yaml configuration (or the primary config file) from this mod and it will be automatically generated again for you on startup.

Q. Wave generation seems insanely unbalanced, what gives?
	A. Delete your configurations (SpawnableCreatures.yaml in the VFortress folder) and MidnightsFX.ValheimFortress.cfg, this will regenerate new configurations with the defaults.
	If you are trying to increase or lower the difficulty from this base point, it is recommended you start by decreasing/increasing the difficulty slope in small increments

Q. The skeltons are attacking the greydwarfs again
	A. The faction changes (and drop removal etc) are not persisted across game restarts. So if you save/quit during a challenge the remaining creatures will not act the same when you log back in, 
		and will revert back to their vanilla settings. Loosing stars, gaining their loot, loosing their connection to the shrine. It won't cause issues for your game. But you will still have to kill them normally, and won't get shrine rewards for it.
 
Q. My game freezes or becomes very slow for some period of time during waves or when creatures are spawning, what do I do?
	A. This mod is fairly intensive on the CPU, in order to perform all of the calculations that go into creating an interesting and varied wave- then actually spawning all of those creatures (and the potentially massive number of interactions with them)
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

## Other Mods
If you like this mod maybe you'll like my other work

[![Valheim Armory](https://i.imgur.com/GofTtar.png)](https://valheim.thunderstore.io/package/MidnightMods/ValheimArmory/)

## Credits
- A big thank you to Margmas, Venture, Redseiko and Probablykory for providing some examples and answers to my silly questions
- The valheim team for continuing to develop valheim, even after their initial roadmap!
	- The continued blogposts about upcoming Valheim content, which got my gears grinding to make this mod
- Unity Ultimate VFX for some or partial visual effects
- Traslations credits to: Azathoth

## Known issues
- Building pieces sometimes don't have destructable bits
- Building pieces don't have wear and tear
- Mobs can form a 'spawn tower' if they can't find someone to attack on spawn
- Automated turret likes to fire off into space instead of hitting its target occassionally (its aim isn't perfect, so sometimes this is intended)
	- If you can reliably reproduce this issue please report it on the Github or Discord
- Singleplayer logging out during a challenge will result in the challenge dissappearing (looking into improvements)
- Multiplayer having the region host change during a challenge can break the challenge (looking into solutions)

## Changelog
**0.9.14**
```
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

**0.5.0** - Initial beta release!
