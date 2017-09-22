Actor passes through tiles painted using the smooth platform brush because inner tiles do
not have colliders by default.


## Symptoms

Collisions are not detected between objects and inner tiles (those that are surrounded by
8 other tiles).



## Causes

The smooth platform brush does not include a box collider on inner tiles by default
because they are usually not needed for 2.5D platform type games. Omission of these
colliders improves performance both in editor and in game.



## Resolve

### Resolution #1 - Better when possible:

Manually add minimum number of colliders using empty game objects instead of using
colliders in brushes. The fewer colliders the better!


### Resolution #2 - Less efficient but often easier:

1. Find and select the following prefab in **Project** panel:
   "Rotorz/Tile System/TileBrushes/Blocks/Smooth Block/centre_inner.prefab".

2. Select menu **Component | Physics | Box Collider**.

3. Ensure that centre of collider is set to (0, 0, 0) and that size of collider is set to
   (1, 1, 1) using **Inspector**.
