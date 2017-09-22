The runtime API makes it easy to paint and erase tiles at runtime by reusing the brushes
that you have already defined.

>
> **Important** - Ensure that stripping preset of associated tile system is set to
> **Runtime Painting**, **No Stripping** or alternatively **Custom** with appropriate
> selection.
>



## Painting and erasing tiles

The following source code demonstrates how to paint and erase tiles at runtime. When
creating an in-game level designer it is likely that you would want to create some sort of
user interface for selecting brushes, etc.


```csharp
using UnityEngine;
using Rotorz.Tile;

public class RuntimePaintingExample : MonoBehaviour
{
    // Tile system to paint on
    public TileSystem tileSystem;
    // Brush to use with left mouse button
    public Brush brush;


    private void Update()
	{
        // Find mouse position in world space
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // Nearest point where ray intersects tile system
        TileIndex ti = this.tileSystem.ClosestTileIndexFromRay(ray);

        if (Input.GetMouseButtonDown(0)) {
            // Paint with left mouse button
            this.brush.Paint(tileSystem, ti);
            this.tileSystem.RefreshSurroundingTiles(ti);
        }
        else if (Input.GetMouseButtonDown(1)) {
            // Erase with right mouse button
            this.tileSystem.EraseTile(ti);
            this.tileSystem.RefreshSurroundingTiles(ti);
        }
    }
}
```


To use the above script:

1. Create a script called "RuntimePaintingExample.cs".

2. Copy source listing from above.

3. Attach script to a game object and specify the following properties using the
   **Inspector** panel:

   - The **Tile System** that you would like to paint on.

   - **Brush** to use when painting with left mouse button.

>
> **Note** - A slightly more advanced demonstration is provided with 'rotorz/unity3d-tile-system-examples'.
> See "hatguy_level_designer.unity".
>



## Improve performance using bulk edit mode

The performance of painting multiple tiles sequentually can be improved using bulk edit
mode (with `TileSystem.BeginBulkEdit` and `TileSystem.EndBulkEdit`):

- Tiles that are painted using orienting brushes (including autotile brushes) can change
  multiple tiles as their surrounding tiles are changes. During bulk edit mode the actual
  creation of tiles is suppressed until all changes have been made.

- Procedural tiles are presented using procedurally generated meshes which only need to be
  updated once regardless of how many tiles have been changed.


```csharp
private void PaintTwoTiles(int row, int column)
{
    this.tileSystem.BeginBulkEdit();
    {
        this.brush.Paint(tileSystem, row, column);
        this.tileSystem.RefreshSurroundingTiles(row, column);
        this.brush.Paint(tileSystem, row, column + 1);
        this.tileSystem.RefreshSurroundingTiles(row, column + 1);
    }
    this.tileSystem.EndBulkEdit();
}

private void PaintExampleTiles()
{
    this.tileSystem.BeginBulkEdit();
    {
        this.PaintTwoTiles(5, 5);
        this.PaintTwoTiles(6, 5);
    }
    this.tileSystem.EndBulkEdit();
}
```



## Improve performance with pooling

By default tiles are painted using the `DefaultRuntimeObjectFactory` which simply
instantiates tile prefabs to create tiles, and destroys tiles upon erasing them. This is
generally acceptable for in-game level designers or when generating tiles at the start of
a level, though you should consider a pooling solution if painting and erasing tiles
in-game.

A pooling system can be utilized by providing a custom `IObjectFactory` implementation.
You might opt to implement your very own specialized pooling solution, or simply to
integrate a third-party solution.

An open-source object factory called [RtsPoolManagerObjectFactory] has already been
implemented which provides support for [PoolManager by Path-o-logical Games].

>
> **Tip** - A pooling solution is only beneficial when using tile prefabs or prefab
> attachments. Tiles painted from a procedural tileset would otherwise not benefit from
> pooling.
>



[PoolManager by Path-o-logical Games]: http://poolmanager.path-o-logical.com
[RtsPoolManagerObjectFactory]: https://bitbucket.org/rotorz/rtspoolmanagerobjectfactory
