Each non-procedural tileset brush is accompanied by a pre-generated mesh asset. These mesh
assets can be removed using the cleanup command when they are not referenced by any
tileset brushes.


## Prerequisite

If you have switched tileset brushes from non-procedural to procedural then any tiles that
were painted prior will be affected when the 'unused' mesh assets are removed. Such tiles
should be refreshed so that they are replaced with procedural tiles instead.



## Steps

1. Select tileset using **Brush** palette and select menu command
   **![tileset context menu](../img/context-button.png) | Show in Designer...**.
		

2. Select menu command **![tileset menu](../img/gear-button.png) | Cleanup Meshes...**.

   ![Cleanup tileset meshes menu item.](../img/tileset/cleanup-tileset-meshes-1.png)

   The following confirmation window should appear:

   ![Cleanup tileset mesh assets confirmation window.](../img/tileset/cleanup-tileset-meshes-2.png)


3. Click **Cleanup**.
