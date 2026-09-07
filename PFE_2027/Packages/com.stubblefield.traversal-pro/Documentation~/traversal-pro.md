# Traversal Pro Documentation

<!-- TOC -->
* [Traversal Pro Documentation](#traversal-pro-documentation)
  * [Philosophy](#philosophy-)
  * [Setup](#setup)
  * [Component Reference](#component-reference)
    * [1. Character Motor (ICharacterMotor)](#1-character-motor-icharactermotor)
    * [2. Character Run](#2-character-run)
    * [3. Jump (IJump)](#3-jump-ijump)
    * [4. Air Control](#4-air-control)
    * [5. Free Fall (IFreeFall)](#5-free-fall-ifreefall)
    * [6. Ground Snap](#6-ground-snap)
    * [7. View Control](#7-view-control-)
    * [8. View Rotate With Ground](#8-view-rotate-with-ground)
    * [9. Sprint Camera Effect](#9-sprint-camera-effect)
    * [10. Cinemachine Damping Modifier](#10-cinemachine-damping-modifier)
    * [11. Velocity Yaw Animation](#11-velocity-yaw-animation)
    * [12. Yaw Constraint](#12-yaw-constraint)
    * [13. Acceleration Roll Animation](#13-acceleration-roll-animation)
    * [14. Character Animator](#14-character-animator)
    * [15. Gravity Lift](#15-gravity-lift)
<!-- TOC -->

## Philosophy 
The overall philosophy of Traversal Pro is to use only physics forces to simulate character movement and to simplify and modularize character controller setup as much as possible. Traversal Pro is implemented as a collection of modular components that each try to provide just one feature. Most of the components provided can simply be removed if the feature they provide is not desired.

## Setup
To set up up Traversal Pro, install it from the Unity Package Manager, then right click in the Hierarchy window and choose Traversal Pro / Third Person Player or First Person Player. This will create a working player character in the scene. When you click play mouse, keyboard, and gamepad controls will control the character on screen.

## Component Reference

### 1. Character Motor (ICharacterMotor)
The CharacterMotor component is the heart of Traversal Pro. Most other Traversal Pro components require it (or rather the interface it implements: ICharacterMotor). It detects the ground under the character and applies forces to move the character along the ground. It requires a CapsuleCollider component and a Rigidbody component attached to the same GameObject. In Traversal Pro, the character's Rigidbody should not rotate. Character rotation happens in the view and mesh GameObjects instead. The CharacterMotor component (or ICharacterMotor interface) requires that another component assigns a value to the LocalVelocityGoal property. The CharacterMotor will then apply forces to the character's Rigidbody to try to reach this velocity goal. Note that when assigning a value to LocalVelocityGoal, you should generally not be multiplying it by Time.deltaTime. It is also important that the MoveInput, MaxLocalSpeed, Acceleration, and MaxAcceleration properties are assigned values by another component. These allow the CharacterMotor and other scripts which depend on it to know how the character is intended to move. By default, the CharacterRun component handles this.

Note that the MoveInput and LocalVelocityGoal values are vectors which should generally be in the same world space direction, but the MoveInput property is meant to represent a movement vector read from player input (such as the WASD keys or gamepad joystick) and have a magnitude between 0 and 1 and be rotated by the player's view. So, for example, if the Player is looking East and moves left with their input device, then MoveInput and LocalVelocityGoal should both be pointing North. The MoveInput property makes it easy for other components to get player input without having to do the math to rotate it by the player's camera orientation. In the case of non-player-characters, MoveInput should still get appropriate values assigned to it pointing in the direction it wants to move.

### 2. Character Run
The CharacterRun component is the default component provided by TraversalPro that defines character movement behavior. It defines run speed, sprint speed, acceleration, and deceleration values, reads player input, and then assigns a value to the attached CharacterMotor's LocalVelocityGoal property. This is probably the first script you'd want to replace with your own script to define custom player movement. To do so, simply create a new MonoBehaviour, get a reference to a CharacterMotor (or ICharacterMotor) component and set its MoveInput and LocalVelocityGoal properties every frame or whenever they change. Be sure to also set its MaxLocalSpeed, Acceleration, and MaxAcceleration values in Start or whenever they change. Setting them every frame is ok too. 

### 3. Jump (IJump)
The Jump component defines jump height, grace times (also called "coyote time"), cooldown time, reads player input, and applies forces to the Character's Rigidbody. It depends on an attached ICharacterMotor component to know when the character is grounded. If you want to implement your own jumping behavior, such as double jumping, you can simply write a new MonoBehaviour script that reads input and applies forces to the character. You can reference the ICharacterMotor component to read if the character is grounded, but you arent' required to reference it nor inform it of forces you apply to the character.

### 4. Air Control
The Air Control component applies horizontal forces to the character during free fall, allowing the player to have some movement control during free fall. This component reads the MoveInput property from the attached ICharacterMotor component to know which direction and how strongly to apply forces to the character. This component is completely optional and can simply be removed if not desired. 

### 5. Free Fall (IFreeFall)
Applies extra gravity and drag forces to the attached Rigidbody. You can define a terminal speed and this component will apply the appropriate force to the character so that the terminal speed is naturally reached during free fall. Note that drag forces are always applied, not just during free fall and they are applied in all directions, not just upward when falling downward. If using this component, the drag value on the attached Rigidbody should be set to 0. If you do not want drag forces applied by this component, you can set the terminal speed to a high number such as one million. The gravity scale value is a multiplier for the gravity value found in the project settings. This component applies additional forces to the attached Rigidbody to simulate the desired gravity. For example, if the project gravity is (0, -9.81, 0) and the gravity scale is set to 2, then this component would simply apply (0, -9.81, 0) units of additional acceleration during FixedUpdate. Then, later in the frame, Unity will apply the project's (0, -9.81, 0) units of acceleration so that the character's Rigidbody would have received a total of (0, -19.62, 0) units of acceleration for that physics frame. Note that this component can be attached to any Rigidbody, not just ones with an ICharacterMotor attached.

### 6. Ground Snap
Applies downward acceleration to the character when close to the ground to prevent the character from briefly free falling when running over changing slopes, such as when running up or down ramps or stairs. This component tries to detect when it should and shouldn't apply its downward acceleration so that it doesn't fight other components for control of the character. It primarily checks if the character is in free fall and is close to the ground and applies its acceleration then. So, for example, if you have a jetpack component that is accelerating the character up, the Ground Snap component will detect that the character is not in a standard free fall and so will not apply downward acceleration. Traversal Pro is designed to exert as little control on the character as possible so it's easy to write custom components to add forces to the character. Still, if you find this component is preventing some other custom components from controlling the character, you can remove it, reduce the acceleration it applies, or even dynamically disable it when those other components are trying to control the character.

### 7. View Control 
Reads player input and smoothly rotates the attached Transform accordingly. Cinemachine cameras should generally have the Transform this is attached to as their target so the player's camera will try to rotate to match this Transform's rotation. The rotation of this Transform is considered the character's view direction. Other components, such as CharacterRun, reference this Transform so they know which direction the player is looking. Note that it is generally the Transform this component is attached to that gets referenced as the character's view, not this component itself.

### 8. View Rotate With Ground
Reads an ICharacterMotor component to get the ground's angular velocity around the Y axis and applies that to the attached ViewControl component. This has the effect of making the player's camera rotate with the ground when the ground rotates. This is enabled by default, but this can cause motion sickness in some players so this component can simply be disabled or removed entirely.

### 9. Sprint Camera Effect
Smoothly modifies the field-of-view of an attached Cinemachine camera when the character starts or stops sprinting.

### 10. Cinemachine Damping Modifier
This component smoothly modifies the damping value on a Cinemachine component based on the velocity of an ICharacterMotor. This component helps a Cinemachine camera with damping be able to track a character during free fall or when moving quickly.

### 11. Velocity Yaw Animation
Smoothly rotates the attached Transform around the Y axis to match a character's movement direction. This works well for third person controllers where the character mesh should be facing in the movement direction rather than the camera's forward direction.

### 12. Yaw Constraint
Smoothly rotates the attached Transform around the Y axis to match a character's view direction. This works well for first or third person controllers where the character mesh should be facing the camera's forward direction rather than the movement direction.

### 13. Acceleration Roll Animation
Applies a rotation around the local forward axis of a character mesh based on its Rigidbody's acceleration. This simulates a character leaning into a turn when moving quickly. 

### 14. Character Animator
Reads various data from several Traversal Pro components and sets values on the attached Unity Animator component. This component is provided mostly as an example for how you might set animation values. It will most likely need to be replaced by a custom script as you customize your characters with your own scripts for your game.

### 15. Gravity Lift
This component should be generally attached to a trigger Capsule Collider in the environment and not a character. It applies forces to any dynamic Rigidbodies that enter it to move them along its capsule. This creates a fun physics mechanic where a character can run into it and be launched into the air. 