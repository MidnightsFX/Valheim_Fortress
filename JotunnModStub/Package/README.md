# ValheimFortress
**!!BETA!!**

What does Beta mean for this project? It should be functional, and probably mostly stable. But may have bugs, and some significant features may change as it gets developed.

Got a bug to report or just want to chat about the mod? Drop by the discord or github.
|||||
|--|--|--|--|
| Discord | [![discord logo](https://i.imgur.com/uE6umQE.png)](https://discord.gg/Dmr9PQTy9m) | Github | [![github logo](https://i.imgur.com/lvbP5OF.png)](https://github.com/MidnightsFX/Valheim_Fortress) |
 

---
## What is Valheim Fortress
When the Valheim Devs released the blog ["Fearsome Foes!"](https://www.valheimgame.com/news/development-blog-fearsome-foes) they talked about a concept called "Fortress Time!".
They did not go into details about what this idea was, but I was very excited! However, what that ended up being, while exciting was not what I was expecting.

I wanted a system that would encourage building a massive fortress, defending it, and reaping rewards for doing so!

So now you know half of what this mod means to be. The other half is primarily cosmetic, and ease of use.
That being, more colorways for buildable pieces along with new variants with the goal of helping ensure large-scale base defense does not become too tedious.


## Features
Wave survival through the Shrine of Challenge. Cosmetic building variants, and functional building variants.

### The shrine of challenge

![Shrine of Challenge Icon](https://i.imgur.com/mEcWfTp.png)

This is what the vast majority of the code from this mod supports. A building which allows you to call dangerous enemies to attack you, in exchange for a reward.
Do you like fighting? Well you can now fight to get more resources!

The goal with the shrine of challenge is to provide a variable but high level of difficulty raid, which the player(s) can invoke in exchange for a promised reward.

The shrine will gradually unlock more levels and rewards as the server defeats the various bosses.

The shrine of challenge is highly configurable. However it only updates configuration values at startup. This is something I am considering changing, but it massively simplifies this mod.


### Cosmetics
The secondary goal of this mod is to help in providing cosmetics and functional base building pieces!

These pieces likely need better color balancing, and might have other oddities at this point. I've got a slew of cosmetics that I would like to add. It will take time though.

<details>
<summary>Rugs</summary>

|Name|Icon|
|--|--|
|Green Circle Rug| ![Green circle rug](https://i.imgur.com/59WStBA.png)|
|Red Circle Rug| ![Red circle rug](https://i.imgur.com/lpGMbPz.png)|
|Yellow Circle Rug| ![Yellow circle rug](https://i.imgur.com/BDuAfcO.png)|

</details>

<details>
<summary>Crystal Walls</summary>

|Name|Icon|
|--|--|
|Green Crystal wall| ![Green crystal wall](https://i.imgur.com/7uC5Td1.png)|
|Blue Crystal wall| ![Blue crystal wall](https://i.imgur.com/2XjuTHZ.png)|
|Red Crystal wall| ![Red crystal wall](https://i.imgur.com/ESTRoai.png)|
|Yellow Crystal wall| ![Yellow crystal wall](https://i.imgur.com/28FaMnE.png)|

</details>

## Future Features / Incomplete things
There are a number of things that are not yet complete and these are the things I plan on working towards.

* Shrine of Challenge
	* Overhaul the level system, making it dynamic and removing level restrictions, meaning there will be a relatively massive number of potential levels
	* Adding challenge modifiers, get extra loot for 2x enemies, stronger enemies, multiple bosses etc
	* Add a Hugin tutorial for the player upon building the Shrine
	* Change the boss level gating to require hanging up a boss head, like the power alters
	* Add ward support checking to ensure that the player interacting with the shrine is allowed
	* Localization support for all text from the Shrine of challenge

* Defensive structures
	* Potentially a new ballista that is much more expensive, does not require refeuling but shoots slower
	* A stake variant that is more expensive but does not get completely destroyed (stops doing damage before it gets destroyed, allows creatures to walk over it)

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

## Known issues
- Localization does not work yet
- No Hugin tutorial for the Shrine of Challenge
- Sanity checking pieces recipes does not happen
- Building pieces sometimes don't have destructable bits
- Building pieces don't have wear and tear

## Changelog

**0.5.1**
```
- Fixes for the UI immediately closing when being opened with a key that is also used for other keybinds
- Fixes for the UI being unopenable by players that are not currently in control of the region
- Fixes for the spawn-in portal being enabled at the wrong times
- Reduction in the amount of log-spam that debug mode has when spawning creatures.
```

**0.5.0** - Initial beta release!
