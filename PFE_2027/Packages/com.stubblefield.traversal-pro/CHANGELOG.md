# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

<!-- TOC -->
* [Changelog](#changelog)
  * [[0.1.14] - 2026-04-19](#0114---2026-04-19)
    * [Changed](#changed)
  * [[0.1.13] - 2025-06-29](#0113---2025-06-29)
    * [Changed](#changed-1)
  * [[0.1.12] - 2025-03-11](#0112---2025-03-11)
    * [Added](#added)
    * [Fixed](#fixed)
  * [[0.1.11] - 2025-02-16](#0111---2025-02-16)
    * [Added](#added-1)
    * [Changed](#changed-2)
    * [Deprecated](#deprecated)
    * [Fixed](#fixed-)
  * [[0.1.0] - 2025-02-07](#010---2025-02-07)
<!-- TOC -->

## [0.1.14] - 2026-04-19

### Changed
- Changed minimum Unity version to 6000.3
- Changed object id and instance id references to EntityId for Unity 6

## [0.1.13] - 2025-06-29

### Changed
- Changed minimum Unity version to 2022.3

## [0.1.12] - 2025-03-11

### Added
- TerrainCollisionCorrection.cs

### Fixed
- TerrainCollisionCorrection.cs is a component that applies a workaround fix for a Unity bug in the TerrainCollider. If that bug gets fixed by Unity, then this component is no longer necessary. The bug is that Rigidbodies will sometimes not get contacts with the terrain collider and will briefly fall through then pop up back to the surface.
- CharacterMotor.overhangTolerance is not clamped to [0, CapsuleCollider.radius]. This fixes a bug where setting the value to high would cause the player to always be ungrounded.
- CharacterMotor.steepSlopeThresholdDegrees is now clamped to [0, 90]. This fixes a bug where if it was set to greater than 90, then the player would struggle on slopes.
- Bug where player would jitter when standing still on steep slopes.

## [0.1.11] - 2025-02-16

### Added
- CharacterAnimator.cs now has an AnimationCurve called "runSpeedToValue" which gives greater control over the run animation speed based on character movement speed. This helps to prevent animation feet sliding.
- The original UnityRobot animation files from the Unity Starter Assets package are now included and used in the Animator controller asset for the Unity Robot.
- A new Animator Controller asset called "UnityRobotAnimator.controller" has been created with better transitions between animations and which uses the landing walk and run animations provided in the Unity Starter Assets package.
- VelocityYawAnimation "speedToSmoothTime" field. This is an AnimationCurve which gives greater control of the character mesh yaw speed at different movement speeds.
- CharacterAnimator "OnLand" method
- CharacterAnimator "OnFootStep" method

### Changed
- Renamed grid materials in Playground sample.
- The Cinemachine cameras for both third and first person players have reduced damping, making them a bit snappier.
- In CharacterRun.cs, the default acceleration value has been reduced from 35 to 25 and the default deceleration value reduced from 50 to 35.
- In the Playground scene, the green building's gravity lift is stronger. This allows the player to more easily run into it from the 2nd level and get to the other side without falling.
- Character.prefab now uses a blue material.
- CharacterRun.cs now updates with FixedUpdate rather than Update and uses DefaultExecutionOrder to update before CharacterMotor.cs

### Deprecated
- Previous animation clips have now been moved to an "Animation_Deprecated" folder and will be removed in a later version.
- CharacterAnimator "InvokeFootSteppedEvent" method
- CharacterAnimator "InvokeLandedEvent" method
- VelocityYawAnimation "smoothTime" field

### Fixed 
- Bug where a Null Reference Exception would occur when selecting the "Character" option under the Traversal Pro menu in the Hierarchy.
- Mouse now works in Playground sample pause menu.
- Bug where AccelerationRollAnimation.cs would apply jerky roll rotations to the character mesh when it was on a moving platform.
- Bug where PauseMenu.cs in Playground scene was throwing an exception when trying to switch action maps while the GameObject the PlayerInput component was on was deactivated.

## [0.1.0] - 2025-02-07
Initial release.