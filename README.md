# Multiplayer Tower Defense Game

## Project Overview

Our final project is a multiplayer tower defense game that can be run on Mac and Windows. The game's local and online multiplayer modes let players cooperate to fend off waves of adversaries while defending their own spawn point. The game features different modes with different difficulties.


## Project Team

Our project is being developed by our 3 developers and 1 designer which are:

- Hugues-Pacôme Stock, who has mostly dealt with the server and overall all multiplayer functionalities.
- Yoann Chhang, who has dealt with most in-game logic as well as graphics while implementing everything in a multiplayer aspect.
- Noah Louis, who has dealt with in-game as well and mostly dealt with enemies and in-game events.
- Noah Dijoux, who is the designer and has designed the game templates as well as user stories and backlog to help us in the project as well as steer us in the right direction.

## Why We Chose This Subject

We decided to create a multiplayer tower defense game using a Game Engine because we wanted to be able to learn something new and be able to have a fully functional game in a short time. Instead of building our own game from scratch without an engine, we chose to use the Unity Game Engine since we thought it would be simpler to design and develop a game utilising a pre-existing framework. We were therefor able to spend less time on the low level technical parts of game development and more time on the general mechanics and even though we spent time learning Unity, we still accomplished more than what we would have without.

Additionally, we got the opportunity to learn how to use Unity which is a technology that we had not previously worked with. We were excited to learn more about game engines and how they can be used to create engaging and immersive gaming experiences since up to now we had only made games using libraries and not game engines.

## Development Tools

Our game is being developed using Unity, with programming done in C#. We are using mostly Visual Studio for scripting and of course Unity for the UI and game making. We also use Github for project management and Discord & Teams to coordinate our collaboration. We are also following agile development workflow in order to stay organised and deliver on time.

## Development Progress

Currently, we have made progress on several key features of the game:

- We implemented a basic multiplayer matchmaking system with lobbies through a host/client system using Unity's Netcode library.
Lobbies have a player name system as well as a chat function.
- We created an easily scalable and changeable interface to navigate through menus and have started on the game's overall design.
- We created the map and set up tilemaps to easily create and generate new maps for new levels.
- We created a basic scalable wave function that spawns enemies, which are able to follow a path through the use of waypoints. Upon reaching the last waypoint of their path, they deal damage to the base.
- We are able to place down towers, and towers are able to detect and shoot at enemies within their detection area. They are also able to target first, last, close, strong and weak enemies depending on the player's choice.

The game itself is handled by a gameManager that will detect conditions to call the end of the game as well as pause the game. The camera is also movable to see the map correctly. One of the most important aspects of our development progress is that everything is scalable, meaning that we can easily implement new features and functionalities as we continue to develop the game. This allows us to adapt to changing requirements and ensures that the game remains engaging and challenging for players.


## Controls

- ESC: Pause the game
- E: Game won
- R: Game lost
- C: Toggle camera free movement
* Left click: Place tower
