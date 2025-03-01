# Unity First Person Project Template

## Overview
This is a first-person player controller template for Unity, designed for smooth movement, camera handling, and input management. It includes features such as jumping, sprinting, and pausing the game. The template is structured to be modular and easily extendable.

## Features
- **Player Input System**: Handles movement, mouse look, jumping, sprinting, and pausing.
- **Character Controller Movement**: Implements acceleration, deceleration, and air control for smooth gameplay.
- **Camera Handling**: Implements first-person camera movement with clamping and smooth transitions.
- **Pause Management**: Ensures input handling stops when the game is paused.
- **Jump Mechanics**: Implements a physics-based jump system with configurable jump height.

## Installation
1. Clone the repository: `git clone <repository-url>`
2. At the repository root run the `git lfs install` command to initilize git lfs hooks
3. Open the project in Unity (version 6000.0.36f1 or later recommended)
4. Ensure all required layers and input settings are correctly set up in Unity

## Usage
### Player Input
- **Move:** `WASD` or Arrow Keys
- **Jump:** `Space`
- **Sprint:** `Left Shift`
- **Pause:** `Escape`

### Player Movement
- Uses Unity's `CharacterController` for smooth movement.
- Supports grounded and air movement.
- Includes an impulse system for external force application.

### Player Camera
- Mouse-controlled camera rotation.
- Clamped vertical angles to prevent unnatural rotations.
- Field of view (FOV) settings included.

## Code Structure

### `PlayerInput.cs`
Manages user input, including movement, camera rotation, and action keys.

### `PlayerMovement.cs`
Handles character movement, acceleration, deceleration, and air physics.

### `PlayerCamera.cs`
Manages first-person camera rotation, smoothing, and field of view.

### `PlayerJump.cs`
Implements jumping mechanics, calculating jump force based on gravity.

## Dependencies
- **UnityEngine.CharacterController**: For movement handling.
- **PauseManager**: Used for pausing mechanics (ensure it's implemented in your project).
- **ZUtils**: Utility functions for movement smoothing (ensure it's included in your project).

## Future Improvements
- Implement crouching mechanics.
- Add footstep sounds.
- Add customizable key bindings through Unity's new Input System.

## License
This project is open-source and available under the MIT License.
