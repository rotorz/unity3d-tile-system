Reference of options that apply to runtime.


## Erase Empty Chunks

When ticked, hints that empty chunks should be erased when they become empty when erasing
tiles.

>
> **Note** - The value of this option is also respected when editing tile system at design
> time.
>



## Apply Basic Stripping

Indicates whether a basic level of stripping should be applied when tile system receives
the `Awake` message.



## Update Procedural at Start

When ticked, procedural tiles will be updated when the tile system receives its first
`Awake` message.



## Mark Procedural Dynamic

Indicates whether procedurally generated meshes are likely to be updated throughout play
where tiles are painted and/or erased frequently to improve performance (this may use more
memory).

It is useful to mark procedural meshes as dynamic when painting or erasing tiles as part
of game-play or a custom in-game level designer.

Avoid setting if procedural meshes are only updated at start of level when loading or
generating map.



## Add Procedural Normals

When ticked, indicates that normals should be added to procedural meshes which allows you
to use shaders that require normals. This is required if you would like procedural tiles
to respond to lighting.



## Procedural Sorting Layer

The sorting layer that should be used for procedural tileset meshes.



## Procedural Order in Layer

Order in sorting layer to use for procedural tileset meshes.
