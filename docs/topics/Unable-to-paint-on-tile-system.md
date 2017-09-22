Unable to paint or erase tiles from the active tile system.


## Symptoms

When tile system is selected it is not possible to paint tiles.



## Causes

Possible causes for this problem:

- Tile system has been locked.

- Tool option **Paint Around Existing Tiles** is selected.

- Tile system is not active or selected.

- Paint tool is not selected.

- Tile system is missing the `TileSystem` component.

- Tile system has been optimized.

- Tile system must be upgraded to work with newer version of 'rotorz/unity3d-tile-system'.



## Resolve

Below are some troubleshooting steps to help determine the cause of the problem.

Make sure that your tile system is selected in **Hierarchy** window or **Scene** palette.
Then try to select the **Paint** tool.


### Check if tile system has been locked

1. Select root game object of tile system.

   If the message "Tile system is locked. Select 'Toggle Lock' from context menu to unlock inspector."
   is shown then the tile system has been locked.
   
   Resolution:
   
   - Select **Toggle Lock** from context menu of tile system component inside inspector
     window.

   - or, Right-click tile system using scene palette and select **Lock** from context menu.


### Check if **Paint Around Existing Tiles** is selected
				
If this tool option is selected then it is not possible to paint over existing tiles.


### Check if tile system has been optimized

1. Select root game object of tile system.

If the message "Tile system has been built and can no longer be edited." is shown then it
is no longer possible to edit this tile system.


### Make sure that tile system component is present

1. Select root game object of tile system.

2. Check if **Tile System** component is listed in **Inspector**.

If tile system component is not listed then it is likely that the tile system has been
optimized and can no longer be edited. Ensure that you are attempting to paint on the
original non-optimized tile system.


### Make sure that tile system component is not marked as missing

1. Select root game object of tile system.

2. Check tile system component in **Inspector** to verify whether component is valid.
   Component is not valid if:

   - It is missing the specialized inspector user interface.
   
   - The **Script** property is blank or marked as missing.

   If `TileSystem` script is not attached to component, ensure that 'rotorz/unity3d-tile-system'
   has been properly imported into your project and that there are no errors in the Unity
   console window.

3. Assign the `TileSystem` script to the **Script** property of the tile system component.
