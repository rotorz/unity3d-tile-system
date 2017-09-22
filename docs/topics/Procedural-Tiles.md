Procedural tiles are presented using meshes that are procedurally generated on a per chunk
basis. These can be painted and erased incredibly quickly and generally incur fewer draw
calls than non-procedurally painted tiles.

![Tiles painted using procedural brush rendered using single draw call.](../img/tileset/procedural-tiles.jpg)

Whilst game objects are not created for each tile, it is possible to attach game objects
by specifying an attachment prefab, see [Attach Prefab]. Colliders can also be added very
easily, see [Add Collider].



[Add Collider]: ./Tileset-Brush-Properties.md
[Attach Prefab]: ./Tileset-Brush-Properties.md
