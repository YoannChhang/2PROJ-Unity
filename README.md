# Multiplayer Tower Defense Game

## Project Overview

Our final project is a multiplayer tower defense game that can be played on an executable or web browser using any game engine such as Unity or Unreal Engine. The game is designed for both local and online multiplayer, allowing players to team up and collaborate in defeating waves of enemies while defending their own spawn point. The game features two distinct modes, the Multiplayer Green TD and the Multiplayer Green Circle TD, each with their own unique gameplay mechanics and challenges.

## Project Team

Our project is being developed by a team of 4 individuals, consisting of 3 developers and 1 designer. Our team members are:

- Hugues-Pacôme Stock, who has mostly dealt with the server and overall all multiplayer functionalities.
- Yoann Chhang, who has dealt with most in-game logic as well as graphics while implementing everything in a multiplayer aspect.
- Noah Louis, who has dealt with in-game as well and mostly dealt with enemies and in-game events.
- Noah Dijoux, who is the designer and has designed the game templates as well as user stories and backlog to help us in the project as well as steer us in the right direction.

We are a passionate and dedicated team that is committed to delivering a high-quality game that meets the expectations of our players. Each member of our team brings their own unique skills and experience to the project, and we work closely together to ensure that all aspects of the game are well-designed, well-implemented, and well-executed.

## Why We Chose This Subject

We chose to develop a multiplayer tower defense game for our final project because we wanted to challenge ourselves by creating a game that would be both entertaining and technically challenging to develop. We decided to use a game engine, such as Unity, because we felt that it would be easier to implement and develop a game using a pre-existing framework rather than creating our own engine from scratch. This allowed us to focus on the game's mechanics and design, rather than spending a lot of time on the technical aspects of game development.

Additionally, we were interested in using a game engine because it is a new technology that we had not previously worked with. We were excited to learn more about game engines and how they can be used to create engaging and immersive gaming experiences. We also felt that developing a tower defense game would be an interesting challenge because it requires a balance of strategy and action, making it both fun to play and challenging to design.

## Development Tools

Our game is being developed using Unity, with programming done in C#. We are using mostly Visual Studio for scripting and of course Unity for the UI and game making. We also use Github for project management and Discord & Teams to coordinate our collaboration. We are following agile development methodologies and are committed to delivering a high-quality game that meets the expectations of our players.

## Development Progress

Currently, we have made progress on several key features of the game:

- We implemented a basic multiplayer matchmaking system with lobbies through a host/client system using Unity's Netcode library.
- Lobbies have a player name system as well as a chat function.
- We created an easily scalable and changeable interface to navigate through menus and have started on the game's overall design.
- We created the map and set up tilemaps to easily create and generate new maps for new levels.
- We created a basic scalable wave function that spawns enemies, which are able to follow a path through the use of waypoints. Upon reaching the last waypoint of their path, they deal damage to the base.
- We are able to place down towers, and towers are able to detect and shoot at enemies within their detection area. They are also able to target first, last, close, strong and weak enemies depending on the player's choice.

The game itself is handled by a gameManager that will detect conditions to call the end of the game as well as pause the game.
The camera is also movable to see the map correctly.
One of the most important aspects of our development progress is that everything is scalable, meaning that we can easily implement new features and functionalities as we continue to develop the game. This allows us to adapt to changing requirements and ensures that the game remains engaging and challenging for players.

## Controls

- ESC: Pause the game
- E: Game won
- R: Game lost
- C: Toggle camera free movement
* Left click: Place tower

We are excited to continue developing our multiplayer tower defense game and are looking forward to complete it. We believe that our dedication to quality and attention to detail will help us to create a game that is both challenging and fun to play.
