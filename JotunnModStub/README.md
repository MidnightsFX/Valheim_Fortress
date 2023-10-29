# ValheimFortress
!!BETA!!

What does Beta mean for this project? It should be functional, and likely mostly stable. But may have bugs, and some significant features may change as it gets developed.

There are likely to be some bugs. They might not be bugs that I have seen already!
Please report any bugs you find on the [Github tracker](https://github.com/MidnightsFX/Valheim_Fortress/issues)

---
## What is Valheim Fortress
When the Valheim Devs released the blog ["Fearsome Foes!"](https://www.valheimgame.com/news/development-blog-fearsome-foes) they talked about a concept called "Fortress Time!".
They did not go into details about what this idea was, but I was very excited! However, what that ended up being, while exciting was not what I was expecting.

I wanted a system that would encourage building a massive fortress, defending it, and reap rewards for doing so!

So now you know half of what this mod means to be. The other half is primarily cosmetic, and ease of use. 
That being, more colorways for buildable pieces along with new variants with the goal of helping ensure large-scale base defense does not become too tedius.


## Features
Wave survival through the Shrine of Challenge. Cosmetic building variants, and functional building variants.

### The shrine of challenge

| Name | Description | Icon |
| ----------- | ----------- | ----------- | ----------- |
| Shrine of Challenge | Summon enemies, kill them, and be rewarded! | ![Antler Bow Icon](https://i.imgur.com/mEcWfTp.png) |

This is what the vast majority of the code from this mod supports. A building which allows you to call dangerous enemies to attack you, in exchange for a reward.
Do you like fighting? Well you can now fight to get more resources!

The shrine of challenge is highly configurable. However it only updates configuration values at startup. This is something I am considering changing, but it massively simplifies this mod.


### Cosmetics
The secondary goal of this mod is to help in providing cosmetics and functional base building pieces!

Cosmetics
* Rug color variants
* Crystal wall variants

## Future Features / Incomplete things
There are a number of things that are not yet complete and these are the things I plan on working towards.

* Shrine of Challenge
	* I plan on changing the default spawn style to be portals in a large radius around the shrine (outside of the players build radius etc) to encourage building and defending a base.
	* The current spawn style will live on in a "gladiator style" spawn, with longer durations between waves and spawning from the shrine area regardless of player built structures.
	* Overhaul the level system, making it dynamic and removing level restrictions, meaning there will be a relatively massive number of potential levels
	* Adding challenge modifiers, get extra loot for 2x enemies, stronger enemies, multiple bosses etc
	* Add a Hugin tutorial for the player upon building the Shrine
	* Change the boss level gating to require hanging up a boss head, like the power alters
	* Add ward support checking to ensure that the player interacting with the shrine is allowed
	* Localization support for all text from the Shrine of challenge

* Potentially a new ballista that is much more expensive, does not require refeuling but shoots slower
* A stake variant that is more expensive but does not get completely destroyed (stops doing damage before it gets destroyed, allows creatures to walk over it)
* Whitemarble cosmetic variant of blackmarble
* Torch color variants
* Lampost color variants
* Darkwood variant to aesthetically fit other tar coated wood pieces

## Credits
- A big thank you to Margmas & Venture for providing some examples and answers to my silly questions
- The valheim team for continuing to develop valheim, even after their initial roadmap!
	- The continued blogposts about upcoming Valheim content, which got my gears grinding to make this mod

## Known issues
- Localization does not work yet
- No Hugin tutorial for the Shrine of Challenge
- Sanity checking pieces recipes does not happen
- Building pieces sometimes don't have destructable bits
- Building pieces don't have wear and tear

## Changelog
