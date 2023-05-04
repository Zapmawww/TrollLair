# Unity Project
## Generator
The generator uses celullar automata (CA) to generate a binary terrain map (land/weall), then water is added with another CA progress. Finally, all the objects and agents are generated with rules and randomness.

*Example parameter for terrain generating: Iterations, Cave density, Troll Density, etc..*

*Example rule for agent generating: trolls cannot spawn on water.*

(See CAgen.cs for detail.)

## Map
**Some code are modified from http://www.roguebasin.com/ and stackoverflow**

The map is defined to store terrain and  positions for object and agent. 

*Parameters: width, height*

(See GameMap.cs for detail.)


## Agent
**Some code are modified from NPBehave**

There are 3 type of agents in the game:
* Troll:
(See Troll.cs for detail.)
        
        Flocking with others, attack thief if found, evade from thief while engaged without chief.
* Troll Chief
(See TrollChief.cs for detail.)

        Wondering around, seeking for thiefs, kill thief if touched.
* Thief
(See Thief.cs for detail.)

        Looking for gems and chests, using torch to increase max speed, touching for killing trolls, but the max speed decreases after killing.

## URL for Video
The [Video Documentation](https://www.bilibili.com/video/BV1Fk4y1E7T6/)
## Unity Version
2022.1.20f1
