A regular brush can be turned into a master brush by moving it's associated asset into a
"TileBrushes/Master" folder somewhere within your project.


## Prerequisite

Whilst it can be useful to turn oriented brushes into highly reusable master brushes, it
is rarely useful to apply this notion to alias or tileset brushes.



## Steps

1. Locate brush asset within your project.

   Your brush will most likely be located somewhere in the
   **Assets/Plugins/PackageData/@rotorz/unity3d-tile-system/Brushes** folder.

   >
   > **Tip** - Quickly locate brush asset by right-clicking brush in **Brush** palette and
   > then selecting **Reveal Asset**
   >


2. Create a folder called "Master" inside "TileBrushes" folder using the **Project** window.

   For example:

   - **Assets/Plugins/PackageData/@rotorz/unity3d-tile-system/Brushes/Master**
   - **Assets/YourFolder/Brushes/Master**


3. Drag brush asset into "Master" folder.

   >
   > **Caution** - You may encounter problems if you attempt to move your brush asset
   > outside of the Unity interface (i.e. using Explorer or Finder).
   >


4. Select menu command **![tool menu](../img/menu-button.png) | Rescan Brushes**.



## Result

Your brush should then be listed as a master brush.
