# BUBBLE

Developed for the [Global Game Jam'25] at Lusofona University, with theme "Bubble".

## Game


## Dev Log

### Initial ideas (Fri 17:00)
  - Bubble Burocracy => "Papers please", but with bubbles. Decide on how to join bubbles together in groups, considering your own thoughts and ideals, etc.
  - Bubble City => A citybuilder on bubbles, player has to create buildings and manager resources, guaranteeing that the weight and balance is kept at an ideal level
  - Last Breath => Competitive multiplayer game, where the player has to help his underwater bubble city to grow, by finding resources and keeping his enemy from getting them.
  - Bubblerazzi => You're a papparazzi trying to snap photos of super stars (different glasses, airstyles, but they are all bubbles), before they pop or fly away.
    - Bonus points for example for getting "blonde female bubble with male spiky hair bubble", etc...
  - Bubble Parasite => You are a small bubble that can infect (walk on the surface) of large bubbles, consuming it. There are "protector" bubbles on the surface as well, that are trying to stop you from killing the host. You have to jump ship before the big bubble pops.

### Decisions (Fri 18:30)
  - "Last Breath"
  - Multiplayer, couch competitive
  - Players control a submarine that has to fetch resources for their homebase
  - Players can either just "follow orders" as fast as possible, or be aggressive
  - They can fire torpedos, and they can hit the submarines by ramming
    - Damage is proportional to combined speeds
    - Hits on the cockpit are double damage
  - Players can restore torpedos and health on their base
  - After a bit of progress (resources gathered), the base upgrades (the shield bubble grows, there are turrets that target the submarines)

![Design](Screenshots/design01.png)

### First prototype - movement (Fri 19:30)

![DevShot](Screenshots/screen01.png)
![DevShot](Screenshots/screen02.png)

Players can now move around the map.

### Damage system (Fri 22:30)

- Got the damage system and explosions working, with some bugs related to the input system and the debris (probably linked)

### Restart on Saturday (Sat 10:00)

![DevShot](Screenshots/screen03.png)

- Fixed the bugs that took me so long yesterday in a few minutes!

### Torpedos (Sat 12:30)

![DevShot](Screenshots/screen04.png)

- Torpedos done, including a homing one which is pretty cool, some fun to be had already

### Reload/Repair (Sat 13:30)

![DevShot](Screenshots/screen05.png)

- Reload and repair at cities done

### Resource spawning (Sat 15:00)

![DevShot](Screenshots/screen06.png)

- Resource system in place (spawns)

### Resource gathering (Sat 16:40)

![DevShot](Screenshots/screen07.png)

- Resources can now be gathered and there's an inventory system in place

### City requests (Sat 18:10)

![DevShot](Screenshots/screen08.png)

- Cities now request resources and we have to go get them

### Main game loop working (Sat 20:00)

- The game loop is fully working, going to finish it off, instead of adding more stuff

### Resource modifiers (Sat 22:15)

![DevShot](Screenshots/screen09.png)

- Added resource modifiers - when you carry the resources, you are subject to some effects
- Added new resource types

### Back to work (Sun 09:45)

- Going back to work, to finish and polish existing stuff, we're feature locked

### Big city lights (Sun 10:45)

![DevShot](Screenshots/screen10.png)

- Got city graphics in, and a city revive mechanics (I know, I said feature lock...)

### End conditions (Sun 11:15)

- End conditions are now checked, game now can go back to main menu (if there is one), winner is announced

## Todo

- ~~Bug with input and respawns~~
- ~~Bug with explosion debris not showing up~~
- ~~Critical damage~~
- ~~Torpedo~~
- ~~City Reload~~
- ~~City Repair~~
- ~~Resource spawns~~
- ~~Resource gathering~~
- ~~City requests~~
- ~~Resource modifiers~~
- ~~More resource types~~
- ~~City graphics~~
- Screen shake
- ~~End conditions~~
- Main menu
- Character customization
- SoundFX
- New submarine 
- Powerups
- City upgrades

## Art

- Font [Optimus](https://www.dafont.com/pt/optimus.font) by [Pixel Sagas](https://www.dafont.com/pt/pixel-sagas.d32), free for non-commercial use.
- Everything else done by [Diogo de Andrade], licensed through the [CC0] license.

## Sound

- Everything else done by [Diogo de Andrade], licensed through the [CC0] license.

## Code

- Some code was adapted/refactored from [Okapi Kit], [MIT] license.
- Uses [Unity Common], [MIT] license.
- All remaining game source code by Diogo de Andrade is licensed under the [MIT] license.

## Metadata

- Autor: [Diogo de Andrade]

[Diogo de Andrade]:https://github.com/DiogoDeAndrade
[CC0]:https://creativecommons.org/publicdomain/zero/1.0/
[CC-BY 3.0]:https://creativecommons.org/licenses/by/3.0/
[CC-BY-NC 3.0]:https://creativecommons.org/licenses/by-nc/3.0/
[CC-BY-SA 4.0]:http://creativecommons.org/licenses/by-sa/4.0/
[CC-BY 4.0]:https://creativecommons.org/licenses/by/4.0/
[OkapiKit]:https://github.com/VideojogosLusofona/OkapiKit
[Unity Common]:https://github.com/DiogoDeAndrade/UnityCommon
[Global Game Jam'25]:https://globalgamejam.org/
[MIT]:LICENSE
