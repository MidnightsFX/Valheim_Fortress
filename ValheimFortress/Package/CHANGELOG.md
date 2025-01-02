  **0.31.1**
---
```
- Changes ballista targeting to allow shooting as long as they would hit a creature/character even if its not the primary target.
	- Fixes a bug where the ballista would not shoot at a target if it was not the primary target.
	- Removes ballistas psychic avoidance of shooting the player (you can now be hit in the crossfire)
```

  **0.31.0**
---
```
- Adds an admin function to all shrines of challenge and shrines of the arena
	- Admin mode currently supports setting a shrine specific filter for what levels can be selected at that shrine
- Adds level names, these are internal, non-unique and used to filter what levels can be selected at a shrine
```

  **0.30.5**
---
```
- Improves shrine reconnection scenarios that involve zero remaining enemies or no found enemies
```

  **0.30.4**
---
```
- Bog Witch update
- Updated to Jotunn 2.21.2
- Fixes for the new game version, this is not backwards compatible.
```

  **0.30.3**
---
```
- Fix for enemies not being reconnected to a shrine in some area load/unload scenarios
- Fixed enemies being counted twice in specific scenarios
- Improves portal stability when the area is loaded and unloaded
```

  **0.30.2**
---
```
- Fixed rewards docs
- Fix for Wildshrines reference going missing
- Fix for wavestalling caused by no enemies being part of a reloaded location
```

  **0.30.1**
---
```
- Fixes Documentation for new level reward limiting
- Fixes reward limiting not working for level 0
- Reduces the size of the UI panel when no modifiers are enabled
- Fixes location of the rewards estimator
```

  **0.30.0**
---
```
- Adds support to limit rewards available for a specific level
- Adds support to gate rewards by arbitrary global keys
- Adds support for making longer or shorter wave definitions
- Massively reduced the shiny-ness of the shrine of the arena
- Increased the likelyhood that a turret would take a shot if it might be able to hit its target
- Fix a ghosting bug with the spawner coroutine, ghosts no longer pop in and out after aborting a challenge
- Fixed a bug that would redirect spawns from one shrine to another
- Fixed a bug where portals would spawn in trees
```

  **0.23.3**
---
```
- Multiplayer diminishing returns always provide a small bonus instead of a the potential of a penalty.
```

  **0.23.2**
---
```
- Rewards multiplier only applies to multiplayer scenarios for diminishing returns. Single challenges are always still 100% rewards.
```

  **0.23.1**
---
```
- Added multiplayer multipliers for rewards, default 100% per player.
```

  **0.23.0**
---
```
- Added live building cost reloading for structures added by this mod. Cost and refund changes now apply immediately.
```

  **0.22.6**
---
```
- Updated localization to be very literal about what kind of tribute is required for shrines by default
- Increased allowable tribute sizes for wildshrines
```

  **0.22.5**
---
```
- Disabled Leech by default for swamp waves
- Fixed reward spawning with multiple stacks spawning more than intended
```

  **0.22.4**
---
```
- Changed rewards to drop as stacks
- Improved Turret enemy detection to ignore enemies they cant see, but are in range
- Added a hybrid ZDO turret sync to fix turret turning/look direction being out of sync for the chunk owner vs others
- Updated default rewards with Ashlands rewards
- Updated default levels with Ashlands levels
- Added safety checks to ensure that Challenges do not attempt to spawn waves they do not have.
```

  **0.22.3**
---
```
- Fixed non-CPU addressable meshes for physics calculations
```

  **0.22.2**
---
```
- Disabled network synchronization of turret targets
- Encouraged turrets to think for themselves
- Turrets prefer closer targets
- Exposed configuration for toggling on/off the shot safety checks
- Reduced turret collider size to help prevent it from shooting itself/neighbors
- Added target cacheing with variable refresh settings
- Improved checks for targets to only be active when characters besides the player are around
```

  **0.22.1**
---
```
- Desyncronizes turret scanning and rotation
- Variable turret scanning frequency
```

  **0.22.0**
---
```
- Made the automated turrets damage configurable
- Reduced the default distance between wildshrines shrines to slightly increase spawns
- Updated to Jotunn 2.20.1
- Removed colored rugs
- Removed colors glass
- Added configuration for damage of the automated ballista
- Added configuration for range of the automated ballista
- Added configuration for shot cooldown for the automated ballista
- Added a safety check for the ballista so it will only fire at its target when it can actually hit it
```

  **0.21.1**
---
```
- Fixed localization reference for blackmetal
- Fixed wildshrine enablement configuration referencing the gameobject
- Fixed Shrine reconnection error caused by having multiple cohorts of the same creature in a wave spawn definition
- Fixed a bug that could prevent shrine progression if the spawned counter was reset
- Fixed a bug that would cause infinite spawns from wildshrines
```

  **0.21.0**
---
```
- Recompiled for Ashlands!
	- Jotun 2.20.0 update. Fixes mis-aligned build tabs
- All shrine types now support resuming in-progress challenges (this fixes issues with rapid area unloading, eg die->teleport, server restart)
- Disabled automated ballista shooting passive creatures. This is configurable, but off by default.
- Disabled colored glass by default (colored glass may be removed from this mod in the future)
```

  **0.20.5**
  ---
```
- Fix for reference to missing dynamic linked library (dll)
- Updated Jotunn to support Ashlands
- Removed soft dependency listed for sears catalog
```

  **0.20.4**
  ---
```
- Fixed an error where wildshrines could try to sync a null znet on client connect
```

  **0.20.3**
  ---
```
- Fixed an error where connecting to a world with a VF shrine within loading range would trigger a data sync of a null object
```

  **0.20.2**
  ---
  ```
- Fixed issue which would throw an error when starting a challenge at the Shrine of Challenge & Shrine of the Arena (error would not cause issues)
- Deprecated and split out MaxCreaturesPerWave configuration to allow seperate configuration for the Challenge and Arena shrines (wildshrines already have seperate configs)
- Increased the max stars that are allowed through the shrine to 10
- Fixed some switched sychronization RPC channels
```

**0.20.1**
---
```
- Removed initial synchronization of current-run wave definitions on on client load from shrines
	- Fixes a possible NPE resulting in sychronizing invalid data (since shrines wave definitions do not persist between world initialization)
```

**0.20.0**
---
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

**0.9.20**
---
```
- Fixes a freeze that may occur when shrine spawn radius is 100% invalid
```

**0.9.19**
---
```
- Made spawn point determination async. This may delay the time it takes to start a wave a little bit but will support a much larger number of attempts to spawn a portal
- For non-gladiator mode, portal generation still moves in segments roughly 10% of the maximum distance at a time.
- Fixes freezes that may occur related to being unable to determine a spawn location around the shrine in a short period of time (freeze when you click 'To Valhalla')
```

**0.9.18**
---
```
- Fix for Gladiator mode not skipping portal generation
- Reducing the maximum number of portal generation attempts to help prevent primary threadlock
```

**0.9.17**
---
```
- Updating Jotunn minimum version
- Updating BepInEx minimum version
- Changing inclusion of the yaml.net lib to be repacked instead of merged to avoid issues with thunderstores new assembly scanner
```

**0.9.16**
---
```
- Fixes a NPE error that could occur when unloading and reloading distant portals
- Fixed a consistency issue with creatures that are destroyed by the shrine not always staying dead, which could occur on reloading
- Added more flavor text variety between waves
```

**0.9.15**
---
```
- Disabled some extra debug logging lines
- Fixed a misspelling in template example/definition for rewards
- Fixes TeleportCreatureThreshold to be respected and configurable
```

**0.9.14**
---
```
- Fixes an issue where the shrine UI would not function normally after being placed in the current play session, but would work fine after a reload
- Fixes an error related to the cancel UI not being available
- Changes how creatures are destroyed to prevent client desynchronization on challenge forfeit
- Adds spanish translation!
```

**0.9.13**
---
```
- Fixes for portals reappearing after world/region reload
- Optimized vfx textures a little
```

**0.9.12**
---
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
---
```
- Improvements to portal cleanup, now with more clients getting portals cleaned up!
```

**0.9.10**
---
```
- Fix for potentially unresponsive UI that primarily occurs after the shrine is built
- Fixes for portals not being visible to everyone in multiplayer
- Fixes for shrine announcements not being visible to every player in multiplayer
- Fixes for multiplayer area sychronization issues
- Interact to reset if the shrine gets stuck at (0) creatures
- Cleanup methods for removing orphaned portals
```

**0.9.9**
---
```
- Fixes a potential phase-skip issue
- Improves the wave-generations abilities to reduce the wave size to smaller quantities
- Reduced the minimum wave reduction size, so you can now face smaller waves with more powerful enemies, if you so choose
- Added spawning jitter for each phase, adding a delay when spawning large amounts of enemies
- Randomized the orientation of spawn portals
- Added more wave pause flavortext
```

**0.9.8**
---
```
- fix for rare NPE exceptions with multiple players loading the shrine at different points
```

**0.9.7**
---
```
- bugfix for a case where parts of the UI might not regenerate after being re-opened
```

**0.9.6**
---
```
- Improved support for toggling various shrine settings on/off during gameplay
- Improved support for estimating values
- Added a system to send flares & teleport remaining creatures to shrine, with configuration options- for when you just can't find those remaining enemies
- Fixed situations where the UI could generate without its button clicks being wired up
```

**0.9.5**
---
```
- Optimized the shrine menu calculations for how many rewards will be recieved
- Optimized reward spawning code to distribute it over many updates
- Provided configuration to increase/decrease how fast rewards spawn
```

**0.9.4**
---
```
- More improvements to consistency of server sync'd configurations (building settings are not applied during without a restart)
- Added configuration to tune rewards value increase per level
- Added configuration to tune rewards base increase
- Added configuration to enable/disable displaying of an estimate for the amount of rewards you will recieve
- Updated default values of rewards to scale considerably higher
- Fix for boss waves not spawning their bosses
```

**0.9.3**
---
```
- Fix rewards/main config server sync interchange
```

**0.9.2**
---
```
- Fixed server file sync and config file location for linux servers
- Added configuration for the max number of creatures in a wave, the generator will attempt to reduce creatures to this point (by upgrading their stars)
- Added a configuration option for the max stars (0-15), more than 2 stars will have no effect if you do not have CLLC
- Added filesync support for the primary config file
	- Recipes (like whats required to build something) are not hot-reloaded.
```

**0.9.1**
---
```
- Fix for potential error from generating a wave and adding a duplicate creature
- Added max_creatures_per_wave as a configuration option, reducing this will reduce the number of creatures that spawn at once
	- Reducing this will result in more creatures being upgraded to higher stars, the overall difficulty remains largely the same
- Reduced the default spawn radius for shrine portals to be 100
```

**0.9.0**
---
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
---
```
- !!CHANGED CONFIG LOCATION!! now MidnightsFX.ValheimFortress.cfg
- Fixed boss loot enable/disable, now correctly will allow boss loot
- Removed more informational logging
- Fixed some configs that were not Admin only (server enforced)
- More wave generation tuning, mountains, plains, and mistlands should no longer feel impossible on their first levels
- Added an enabled/disabled config for all spawnable creatures, if you really hate fighting something you can just disable it now
```

**0.8.2**
---
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
---
```
- Optimized download size, removed potential duplicate embedded libraries
```

**0.8.0**
---
```
- Added Dynamic yaml configuration for Rewards and available creature spawns
	- Additional rewards can be added through new entries, existing rewards can be modified and disabled in the same way
	- Monsters can be added as possible spawns, configured for which biome they spawn from and how much their spawncosts are, or disabled entirely.
```

** 0.7.3**
---
```
- Fixed level 5 having an infinite loading loop crash to using the wrong level data
```

** 0.7.2**
---
```
- Disable map drawing overlay due to potential errors
- Enable max level configuration
	- Max level is currently set to 30, 5 levels for each biome
- Made all of the shrine modifiers enable/disable-able
```

**0.7.1**
---
```
- Added spawn portals at the remote locations where enemies will spawn
- Fixed the readme formatting :)
```

**0.7.0**
---
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
---
```
- Fixed Shrine of Challenge interaction with custom hammer mods
- Added a custom ballista that is more expensive and does not require ammo (and does not shoot at friendlies!)
- Added configuration values for Shrine base difficulty, ramp per level, difficulty slope and the maximum points that any wave can have
```

**0.6.0**
---
```
- Updated recipe parsing to support recovery of crafting componets for structures
- Fix monster types fighting each other, all spawned enemies are now 'boss' type, and will not be fought by anything else
- Localization of the rewards selector
- Hugin tutorial for the shrine of challenge
- Shrine of challenge now has more collision with the world around it
- Added stone stakes! Filling the gap between Corewood & Dverger stakes, they are lower damage, but take longer to destroy
```

**0.5.2**
---
```
- Localization works in almost every spot now (rewards selector text being the exception)
- Added a small example of what its like using this mod
```

**0.5.1**
---
```
- Fixes for the UI immediately closing when being opened with a key that is also used for other keybinds
- Fixes for the UI being unopenable by players that are not currently in control of the region
- Fixes for the spawn-in portal being enabled at the wrong times
- Reduction in the amount of log-spam that debug mode has when spawning creatures.
```

**0.5.0**
---
```
- Initial beta release!
```
