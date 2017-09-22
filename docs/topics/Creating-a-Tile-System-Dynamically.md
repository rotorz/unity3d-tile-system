Tile systems can be created and used at runtime. This section demonstrates how to
implement this.

A tile system is essentially a game object with the `TileSystem` component attached. Once
initialized these can be used in the same way as usual. The following source listing
demonstrates how to create a tile system at runtime.


```csharp
using UnityEngine;
using Rotorz.Tile;

public class DynamicallyCreateTileSystem : MonoBehaviour
{
    // We will use this brush to paint a line of tiles
    public Brush testBrush;

    private void Start()
	{
        // Create game object for tile system
        var systemGO = new GameObject("Dynamic Tiles");
        // Attach tile system component
        var system = systemGO.AddComponent<TileSystem>();

        // Initialize tile system with 10 rows, 15 columns
        // where each tile consumes 1x1x1 in world space
        system.CreateSystem(1f, 1f, 1f, 10, 15);

        // Paint a horizontal line of tiles?
        if (testBrush != null) {
            for (int i = 0; i < 15; ++i) {
                testBrush.Paint(system, 5, i);
            }
        }
    }
}
```


To use the above script:

- Create a script called "DynamicallyCreateTileSystem.cs".

- Copy source listing from above.

- Attach script to a game object and specify a brush using the **Inspector** panel.
