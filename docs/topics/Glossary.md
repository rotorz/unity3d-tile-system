Alias Brush
: Brush that makes reference to another non-alias brush allowing some properties to be
  overridden and materials to be remapped.

Atlas
: 2D texture that contains artwork for multiple tiles.

Autotile Brush
: Autotile brushes require a special tileset which is automatically generated from the
  input autotile artwork. The aim of autotile brushes is to make it easier for artists to
  create 2D tiles that automatically orientate like walls, paths, etc.

Brush
: Tiles can be painted using specialized brushes. All brush types are derived from the
  base class **Rotorz.Tile.Brush**. Some brushes are stored as independent asset files
  whilst others are contained within composite assets like tilesets.

Build Prefab
: Optimized version of tile system can be built and then saved as a prefab.

Build Scene
: All tile systems in scene can be optimized where the optimized scene is saved to a
  separate file.

Cell
: A tile system is composed of cells which can either be empty or can contain a tile. The
  content of a cell can be accessed at runtime using **TileSystem.GetTile**.

Chunk
: Tile systems and their data structures are broken down into chunks to help reduce memory
  requirements. Procedural meshes are added on a per chunk basis procedural brushes are
  used.

Empty Brush
: A brush that paints tiles with no visual output. These are useful when designing
  oriented tiles that require gaps or when defining tiles of a purely logical nature. By
  default a master brush called "Empty Variation" is provided that can be used within
  oriented brushes.

Master Brush
: Brush that cannot be modified using the brush designer. Master brushes can be copied and
  aliases can be created using the brush designer. 'rotorz/unity3d-tile-system' includes
  some master brushes that can be used to create custom platform tiles simply by providing
  an alternative material.

Oriented Brush
: Brush which automatically picks from a number of user defined orientations and
  variations to automatically paint tiles that connect with one another.

Orientation
: Describes the context of a tile based upon its 8 surrounding tiles. Both oriented
  brushes and autotile brushes analyse the orientation of tiles to determine which ones
  should be painted.

Plop
: A "plop" is an object which does not contribute to the data structure of a tile system
  and is 'plopped' using the plop tool. Plops can be erased and cycled in a similar way to
  regular tiles.

Runtime API
: Programming interface that can be used by developers to interact with tile systems at
  runtime. For example, this could be used to create an in-game level designer.

Tile System
: Virtual grid that can contain zero or more tiles.

Tileset
: A tileset is associated with an atlas that contains a collection of tiles that can be
  painted by creating and using tileset brushes. Tilesets provide a small degree of
  indirection which makes it relitively easy to convert tileset brushes between procedural
  and non-procedural.

Tileset Brush
: Brush that can paint a procedural or non-procedural tile from a tileset.

Tool
: Used to paint with brushes and manipulate tiles.

Variation
: Multiple tile variations can be specified for each orientation of an oriented brush.
