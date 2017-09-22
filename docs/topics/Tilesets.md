Tilesets make it easier for you to define brushes that allow you to paint 2D tiles onto
your tile systems. You can define a tileset from an atlas texture which includes the
artwork for a number of uniformly sized tiles.

Each tileset is represented with an asset which:

- Associates the tileset with its atlas texture and material.

- Defines the way in which tiles are packed.

- Contains tileset brushes that are used when painting tiles.

- Manages pre-generated mesh assets for non-procedural tiles.

Once you have defined your tileset you can create a [tileset brush] for each of the tiles
that you would like to be able to paint with. Tileset brushes can then be used with the
provided editor tools or dynamically using the [Runtime API].

>
> **Remember** - If you previously used version 1.x of this extension then you may have
> used atlas brushes for painting 2D tiles. These have been replaced with tilesets brushes
> which can be used procedurally or non-procedurally. Previously atlas brushes were only
> able to paint tiles non-procedurally.
>



[Runtime API]: ./Runtime-Scripting.md
[tileset brush]: ./Tileset-Brushes.md
