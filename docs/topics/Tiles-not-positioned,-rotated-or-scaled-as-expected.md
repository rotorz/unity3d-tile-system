Tiles do not appear to be positioned, rotated or scaled correctly when painted because an
offset has been specified or its origin (pivot point) is incorrect.


## Symptoms

Painted tiles are not aligned properly against tile system.



## Causes

Possible causes for this problem:

- An incorrect transform offset has been specified for brush.

- **Apply Prefab Transform** is selected for brush but transform of tile prefab is incorrect.

- Origin of tile prefab is incorrect.



## Diagnose

### Check offset of prefab

Prefab transform is used to offset tile when **Apply Prefab Transform** is selected for
brush. The position value of a prefab is often non-zero upon creation which may need to be
manually set to (0, 0, 0).

1. Find and select prefab asset using **Project** panel.

2. Check position using **Inspector**.

3. See [Fix offset of prefab](#fix-prefab-offset) if position is something other than
   (0, 0, 0).


### Check pivot point of prefab

1. Create a blank scene

2. Add instance of troublesome prefab to scene.

3. Ensure that position of prefab instance is set to (0, 0, 0) using **Inspector**.

4. See [Fix pivot point of prefab](#fix-prefab-pivot) if object does not appear at center
   of scene.



## Resolve

### Fix offset of prefab {#fix-prefab-offset}

1. Find and select prefab asset using **Project** window.

2. Set position of prefab to (0, 0, 0) using **Inspector** for central placement within
   tile cell.

3. Attempt to paint using brush to see if this resolves the problem.


### Fix pivot point of prefab {#fix-prefab-pivot}

Correct pivot point of game object by repositioning nested objects and/or altering mesh if
required. Another easier way to fine-tune tile object placement is to apply a prefab
offset as described in the next section.


### Fix placement by applying prefab offset

1. Create blank scene and add a tile system.

2. Paint instance of the troublesome tile.

3. Select root most object of tile which is connected to tile prefab.

4. Correct position/rotation/scale of tile using the regular Unity tools or inspector.

5. Select **![tool menu](../img/menu-button.png) | Use as Prefab Offset**.

6. Attempt to paint using brush to see if this resolves the problem.
