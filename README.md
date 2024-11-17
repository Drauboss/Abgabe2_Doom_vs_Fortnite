# Abgabe2_Doom_vs_Fortnite

## Gameplay Recoording
[![Doom](https://img.youtube.com/vi/AI96NwbTwk4/0.jpg)](https://www.youtube.com/watch?v=AI96NwbTwk4)

## Edited Task:
- Game Mechanics: High Mobility
- Single or Multiplayer: Animated Bots

## Used Assets
- **Weapon Fire Effect:** [Stylized M4 Assault Rifle with Scope](https://assetstore.unity.com/packages/3d/props/guns/stylized-m4-assault-rifle-with-scope-complete-kit-with-gunshot-v-178197)
- **Camera FOV Effect:** [DOTween (HOTween v2)](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676)
- **Remaining Assets:** [Polygon Sampler Pack](https://assetstore.unity.com/packages/3d/environments/polygon-sampler-pack-207048)

## Used Animations and Enemy Character Models
- [Mixamo](https://www.mixamo.com/)

## Controls
- **W, A, S, D:** Movement
- **Mouse:** Camera Control
- **Spacebar:** Jump / Double Jump
- **Left Mouse Button:** Shoot
- **Right Mouse Button:** Swing (red dot indicates swing point). While swinging, the rope can be shortened by pressing the spacebar. The rope can be lengthened by pressing the down arrow / S key. Forces can be applied in the respective direction for swinging using W/A/D.
- **Q:** Grappling Gun (when pressed, a rope is shot at the swing point and the player is pulled to the point).
- **Shift:** Sprint
- **R:** Climb (when pressed, the player climbs up and down by pressing the arrow keys).
- **E:** Dash (the player moves in the direction they are looking).

## Enemies
The zombies move towards the player and attack when they are near the player. The player can weaken them by shooting and can kill them with enough hits. The zombies are equipped with a NavMeshAgent to move towards the player. Additionally, the zombies have walking, running, attacking, and dying animations.

## Player
The player has health and loses it through zombie attacks. The player also has a kill counter. The player's gun shoots projectiles that cause damage upon contact with the zombies.

## Game World
- **Purple round platforms:** Trampolines that catapult the player into the air.
- **Green floating boxes:** For practicing swinging.
- **Blue towers:** For trying out climbing.
- **Ground floor of the building:** A shooting range to practice shooting. Hits are displayed on the dummies.