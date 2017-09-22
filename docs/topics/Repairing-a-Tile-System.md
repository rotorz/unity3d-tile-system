Tiles become broken when their data remains yet their game object counterpart is missing.
A common cause of this is where tile game objects are deleted instead of being erased.


## Prerequisite

Ensure that **Inspector** window is shown by selecting **Window | Inspector**.



## Steps

1. Select your tile system.


2. Click **Repair...** button in tile system inspector.

   There are two potential outcomes:

   - No broken tiles were found, thus no further steps remain.

   - Broken tile(s) found and a window similar to the following is displayed:

     ![](../img/ui/dialog-repair-tiles.png)


3. Choose from provided options:

   - **Repair** - Fix broken tiles by repainting with original brush (if still exists).

   - **Erase** - Erase broken tiles.

   - **Cancel** - Abort repair.
