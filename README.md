# SignalR Snake #

This README would normally document whatever steps are necessary to get your application up and running.

### What is this repository for? ###

* A simple game inspired by Slither.io made with MVC5 and SignalR. You play as a snake and so do other players,
 you eat food to gain score and can kill other players by crossing their path and blocking them.

### How do I get set up? ###

* Run it as you would with any other asp.net mvc 5 project. Connect, name your snake and play. The game is very performance heavy for the server as most
  calculations are on the server, only the angle deciding next move and drawing is handled by the client.