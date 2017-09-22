Tilesets define whether their brushes will be primarily procedural or non-procedural. The
value of this setting is usually inherited by brushes, though can be overridden on a per
brush basis where necessary.

The value of this property should reflect the primary nature of the tileset because the
property can be conveniently overridden on a per-brush basis for special cases (see
[Procedural]).

>
> **Important** - When the value of this property is changed, any tile systems containing
> tiles that were painted using affected brushes must be refreshed. See [Refreshing all Painted].
>



## Converting to Procedural

The conversion of brushes from non-procedural to procedural is relatively fast. Existing
tile systems should be refreshed to ensure that any tiles that were painted using the
affected brushes are updated.

Mesh assets that were previously generated for non-procedural tiles are not removed
automatically to avoid breaking tiles that were painted prior to this change. The unwanted
mesh assets can be cleaned up, see [Cleanup Unused Meshes].



## Converting to Non-Procedural

The conversion process may take a moment to generate missing mesh assets. You may notice
that tiles previously painted using the affected brushes have disappeared; this can be
corrected by refreshing all affected tile systems.



[Cleanup Unused Meshes]: ./Cleanup-Unused-Meshes.md
[Procedural]: ./Tileset-Brush-Properties.md
[Refreshing all Painted]: ./Refreshing-all-Painted-Tiles.md
