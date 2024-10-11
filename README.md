# Ludo Game Unity Project

## Overview

This project is a simple implementation of a Ludo-style board game using Unity. It features a single game piece (chip) moving on a board based on dice rolls, with the dice values fetched from an online random number generator.

## Features

- Simple UI with Roll and Reset buttons
- Ludo board game layout with a single game piece
- Dice rolling animation
- Integration with an online random number service for fair dice rolls
- Unity Addressables system for efficient asset loading
- Sound effects for enhanced user experience

## Requirements

- Unity 2022.3.30f1 LTS or later
- Addressables package

## Setup

1. Clone this repository to your local machine.
2. Open the project in Unity.
3. Ensure the Addressables package is installed (Window > Package Manager).
4. Open the main scene located in `Assets/Scenes/MainScene.unity`.

## How to Play

1. Click the "Roll" button to roll the die.
2. Wait for the die animation and the fetching of the random number.
3. Click on the chip to move it.
4. Use the "Reset" button to return the chip to its starting position.

## Project Structure

- `Assets/Scripts/`
  - `GameManager.cs`: Main game logic
  - `ApiManager.cs`: Handles API calls for random number generation
  - `AudioManager.cs`: Manages sound effects
- `Assets/Scenes/`
  - `MainScene.unity`: The main (and only) scene of the game

## Implementation Details

- The game uses Unity's Addressables system to load die and chip sprites efficiently.
- Random numbers for dice rolls are fetched from [Random.org](https://www.random.org/) API.


## Future Improvements

- Implement full Ludo ruleset with multiple pieces per player
- Create more visually appealing UI and game board


---