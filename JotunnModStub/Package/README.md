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

## Configuration
This mod is HIGHLY configurable. All buildings have configurable crafting recipes and many aspects about the shrine of challenge can be configured to your liking.

This mod uses almost exclusively server sided configuration. Which means that these values will not be updated while the game is playing. 
This helps me reduce complexity, in the future I will consider server-synched configs, but right now that is not included in this mod.

This mod does support [BepinEx in-game Configuration](https://valheim.thunderstore.io/package/Azumatt/Official_BepInEx_ConfigurationManager/)
Configurations can also be edited in the config file within your Bepinex folder `BepinEx\Config\com.midnightsfx.ValheimFortress.cfg`

<details>
<summary>Example in-game configuration</summary>

Basic configuration view in-game.
![basic configs](https://i.imgur.com/6zebaBk.png)

</details>

### How to adjust the difficulty
The Shrine of challenge provides a number of key configuration values which can be used to adjust the difficulty level in many different ways.

|Name|Default|What it Does|
|--|--|--|
|level_base_challenge_points | 100 | The base level of points all waves have, this primarily determines early level difficulty. |
|base_challenge_points_increase | 10 | The number of points added each level eg: (level * 10) |
|challenge_slope | 0.2 | The logarithmic slope adjustment, tuning this higher will result in quicker difficulty scale-ups, while lower will result in a slower difficulty curve |
|max_challenge_points | 3000 | This is a cap on how many points a wave can generate with, it primarily limits the max wave sizes (its intentionally relatively low, feel free to tune it upwards. Who needs to keep their base in one piece anyways?) |
|creature_star_chance | 0.15 | percentage 0.001-1.0, chance that a creature will spawn as a 1+ star variant, some creatures always spawn as multi-star, others always never spawn at a higher star rate. |

<details>
<summary>Summary of the difficulty equation</summary>

And now you want to know how these values are actually used to compute the challenge points right?
look at this summary below
```
slope = Log((10 + level) * (1 + challenge_slope)))

base_challenge_points_increase * level
-------------------------------------- = total_increase_points
			slope

allocated_challenge_points = total_increase_points + base_challenge_points

if (allocated_challenge_points > max_challenge_points) { allocated_challenge_points = max_challenge_points; }
```

</details>


## Future Features / Incomplete things
There are a number of things that I plan on adding in the future. Here is the current list.

* Shrine of Challenge
	* Add ward support checking to ensure that the player interacting with the shrine is allowed

* cosmetics
	* Whitemarble cosmetic variant of blackmarble
	* Torch color variants
	* Lampost color variants
	* Darkwood variant to aesthetically fit other tar coated wood pieces

* Consider server-synced configuration

## Other Mods
If you like this mod maybe you'll like my other work

[![Valheim Armory](https://i.imgur.com/GofTtar.png)](https://valheim.thunderstore.io/package/MidnightMods/ValheimArmory/)

## Credits
- A big thank you to Margmas, Venture, Redseiko and Probablykory for providing some examples and answers to my silly questions
- The valheim team for continuing to develop valheim, even after their initial roadmap!
	- The continued blogposts about upcoming Valheim content, which got my gears grinding to make this mod

## Known issues
- Building pieces sometimes don't have destructable bits
- Building pieces don't have wear and tear
- Mobs can form a 'spawn tower' if they can't find someone to attack on spawn
- Automated turret likes to fire off into space instead of hitting its target occassionally (its aim isn't perfect, this is intentional)

## Changelog
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
