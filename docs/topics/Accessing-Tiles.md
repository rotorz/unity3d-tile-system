Each non-empty tile is represented by a data object which describes its state and
references the attached game object when applicable. Tiles can be accessed via the tile
system component or on a per-chunk basis.


## Accessing a single tile

Individual tiles can be accessed simply by specifying the row and column of the tile that
you would like to lookup:


```csharp
// Access tile on row 4 column 5
TileData tile = tileSystem.GetTile(4, 5);
if (tile != null) {
    // Tile exists!
}
```


### Enumerating tiles by row and column

Tiles can be enumerated by row and column transparently of the underlying data structure:


```csharp
// Loop through all tiles in tile system
for (int row = 0; row < tileSystem.RowCount; ++row) {
    for (int column = 0; column < tileSystem.ColumnCount; ++column) {
        TileData tile = tileSystem.GetTile(row, column);

        // Skip empty tile
        if (tile == null) {
            continue;
        }

        // Do something with tile!
    }
}
```


### Enumerating tiles by chunk (more efficient)

Tiles are stored in a chunked data structure which helps to avoid wasting memory when
large areas of a tile system are empty. This trait can also be useful when enumerating all
non-empty tiles because you can easily skip empty chunks when enumerating them.

>
> **Note** - Row and column indices are not known when enumerating tiles in this manor.
>

```csharp
// Loop through non-empty tiles in tile system
foreach (Chunk chunk in tileSystem.Chunks) {
    // Skip missing chunk!
    if (chunk == null) {
        continue;
    }

    foreach (TileData tile in chunk.tiles) {
        // Skip empty tiles in chunk. When accessing tiles
        // manually we must also check to see if non-null
        // tile is empty.
        if (tile == null || tile.Empty) {
            continue;
        }

        // Do something with tile!
    }
}
```
