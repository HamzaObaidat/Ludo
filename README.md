# Ludo Game with Unity and Addressables

This project implements a simple Ludo game scene in Unity with the following features:
- A UI with Roll and Reset buttons
- A Ludo board layout with a single game piece (chip)
- Die rolling animation and random number generation using an online service
- Chip movement based on die roll results
- Integration of Unity's Addressables system for loading assets

## Project Structure

- `Assets/`
  - `Scripts/`
    - `LudoGameManager.cs`: Main game logic script
    - `APIManager.cs`: Manages API calls for random number generation
  - `Scenes/`
    - `LudoGame.unity`: Main game scene
  - `Prefabs/`
    - `Dice.prefab`: Prefab for the dice UI element
    - `Chip.prefab`: Prefab for the game piece
  - `Sprites/`
    - `DiceSprites/`: Contains sprites for dice faces
    - `ChipSprite.png`: Sprite for the game piece
  - `AddressableAssetsData/`: Contains Addressables configuration

## Setup Instructions

1. Clone the repository or download the project files.
2. Open the project in Unity (version 2020.3 or later recommended).
3. Ensure the Addressables package is installed (Window > Package Manager).
4. Open the `LudoGame` scene in the `Scenes` folder.
5. In the Hierarchy window, select the `GameManager` GameObject and ensure all references in the Inspector are correctly assigned:
   - Roll Button
   - Reset Button
   - Dice Image
   - Chip Transform
   - Board Positions (array of Transform components representing board spaces)
6. Ensure there's an empty GameObject in the scene with the `APIManager` script attached.
7. Build and run the project on your desired platform (Android or iOS recommended).

## Addressables Setup

1. In the Unity Editor, go to Window > Asset Management > Addressables > Groups.
2. Create two new groups: "DiceSprites" and "ChipSprite".
3. Drag the dice face sprites into the "DiceSprites" group.
4. Drag the chip sprite into the "ChipSprite" group.
5. Ensure the address for each asset matches the names used in the `LudoGameManager` script.

## Android/iOS Compatibility

To ensure compatibility with Android or iOS:
1. Switch the build platform to Android or iOS in File > Build Settings.
2. For Android, ensure you have the Android SDK and JDK installed.
3. For iOS, you'll need to build on a Mac with Xcode installed.
4. Configure any platform-specific settings in Player Settings.
5. For the random number API, ensure you have internet permissions set in your app manifest.

## Notes

- The random number generation uses the Random.org API through the `APIManager` singleton. You may need to replace the API URL in `APIManager.cs` with a different service if you encounter rate limiting issues.
- The project uses Addressables for efficient asset loading. Make sure to build your Addressables before building the final app.
- Extend the `GetDiceSprite` method in `LudoGameManager.cs` to return the correct dice face sprite based on the roll result.
- The `APIManager` is implemented as a singleton to ensure there's only one instance managing API calls throughout the game.