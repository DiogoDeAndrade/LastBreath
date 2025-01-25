# BUBBLE

Developed for the [Global Game Jam'25] at Lusofona University, with theme "Bubble".

## Game


## Dev stuff

### Initial ideas
  - Bubble Burocracy => "Papers please", but with bubbles. Decide on how to join bubbles together in groups, considering your own thoughts and ideals, etc.
  - Bubble City => A citybuilder on bubbles, player has to create buildings and manager resources, guaranteeing that the weight and balance is kept at an ideal level
  - Last Breath => Competitive multiplayer game, where the player has to help his underwater bubble city to grow, by finding resources and keeping his enemy from getting them.
  - Bubblerazzi => You're a papparazzi trying to snap photos of super stars (different glasses, airstyles, but they are all bubbles), before they pop or fly away.
    - Bonus points for example for getting "blonde female bubble with male spiky hair bubble", etc...
  - Bubble Parasite => You are a small bubble that can infect (walk on the surface) of large bubbles, consuming it. There are "protector" bubbles on the surface as well, that are trying to stop you from killing the host. You have to jump ship before the big bubble pops.

### Decisions (1h30)
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

### First prototype - movement (3h)

![Design](Screenshots/screen01.png)
![Design](Screenshots/screen02.png)

Players can now move around the map.

### Damage system (6h)

- Got the damage system and explosions working, with some bugs related to the input system and the debris (probably linked)

## Todo

- ~~Bug with input and respawns~~
- ~~Bug with explosion debris not showing up~~
- Critical damage
- Torpedo
- City requests
- City upgrades
- End conditions
- Main menu
- Character customization

## Art

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
