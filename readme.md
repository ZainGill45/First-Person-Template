# Unity First Person Project Template

## Overview

This is a first-person player controller template for Unity, utilizing the Kinematic Character Controller (KCC) for robust and smooth movement. It includes features such as jumping, sprinting, crouching, camera handling, and input management. The template is designed to be modular and easily extendable, leveraging layered speed and FOV management for flexible gameplay mechanics.

## Features

* **Kinematic Character Controller (KCC):** Provides reliable and smooth character movement.
* **Player Input System:** Handles movement, mouse look, jumping, sprinting, crouching, and pausing.
* **Camera Handling:** Implements first-person camera movement with clamping, smooth transitions, and FOV adjustments.
* **Pause Management:** Ensures input handling stops when the game is paused.
* **Jump Mechanics:** Implements a physics-based jump system with configurable jump height.
* **Sprint and Crouch Mechanics:** Includes layered speed and FOV adjustments for smooth transitions.
* **Layered Speed and FOV Management:** Allows for dynamic adjustments to player speed and camera FOV through a layered system.
* **Utility Functions (ZUtils):** Provides useful extension methods and constants.

## Installation

1.  Clone the repository: `git clone <repository-url>`
2.  At the repository root, run the `git lfs install` command to initialize Git LFS hooks.
3.  Open the project in Unity (Unity 2021 or later recommended, specifically tested with a version similar to 2021 or later due to KCC usage).
4.  Ensure the Kinematic Character Controller (KCC) package is installed. This package is essential for the character controller to function.
5.  Ensure all required layers and input settings are correctly set up in Unity.

## Usage

### Player Input

* **Move:** WASD or Arrow Keys
* **Jump:** Space
* **Sprint:** Left Shift
* **Crouch:** Left Control
* **Pause:** Escape

### Player Movement

* Uses the Kinematic Character Controller (KCC) for precise and smooth movement.
* Supports grounded and air movement with configurable acceleration and deceleration.
* Includes an impulse system for applying external forces.

### Player Camera

* Mouse-controlled camera rotation with clamped vertical angles.
* Configurable field of view (FOV) settings and dynamic FOV adjustments.

### Code Structure

* **PlayerController.cs:** Manages character movement, camera rotation, and layered speed and FOV adjustments using KCC.
* **PlayerJump.cs:** Implements jump mechanics.
* **PlayerSprint.cs:** Implements sprint mechanics with layered speed and FOV adjustments.
* **PlayerCrouch.cs:** Implements crouch mechanics with layered speed and FOV adjustments and capsule collider resizing.
* **PlayerMeshUpdater.cs:** Updates the player mesh to match the KCC capsule collider.
* **InputManager.cs:** Manages user input, including movement, camera rotation, and action keys.
* **PauseManager.cs:** Manages game pause state.
* **ZUtils.cs:** Provides utility functions for movement smoothing and other general-purpose operations.

### Dependencies

* **Kinematic Character Controller (KCC):** For robust character movement.
* **PauseManager:** For pausing mechanics.
* **ZUtils:** Utility functions.

### Future Improvements

* Add footstep sounds.
* Add customizable key bindings through Unity's Input System.
* Add more player states.
* Add more complex movement options such as wall running.

### License

This project is open-source and available under the MIT License.
