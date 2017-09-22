Reference of options allowing you to customize the way in which your tile system is built.


## Combine Method

Runtime performance can often be improved by reducing the overall number of draw calls.
The number of draw calls can be reduced by combining multiple tiles that share the same
material.

The performance of combined tiles can be further optimized by ensuring that a worthwhile
amount of geometry is combined (whilst avoiding excessive numbers of vertices and
triangles). This can be achieved by specifying appropriate chunk sizes when combining
tiles. The chunk size will vary depending upon the complexity of your tiles along with
your target platforms.


None
: Do not attempt to combine tile meshes.

By Chunk (default)
: Combine tiles into one mesh per chunk; with sub-mesh per material.

By Tile System
: Combine tiles into one large chunk; with sub-mesh per material.

Custom Chunk In Tiles
: Allows you to specify a different chunk size for tile combiner.



## Combine into Submeshes

When tiles are merged with multiple materials you have the choice between combining them
into submeshes or into separate meshes:


Yes (default) - combine into submeshes (per chunk)
: A submesh is created for each material which according to the Unity documentation
  ([Mesh.SetTriangles]) are simply represented as separate triangle lists.

No - combine into one mesh per material (per chunk)
: A separate mesh is created for each material which in some scenarios adds a little
  further mileage before the upper limit of 64k vertices is encountered.


In both circumstances you will incur one draw call per material per chunk for simple
single pass shaders.

>
> **Tip** - In the event that your combined chunks exceed the maximum limit of 64k vertices
> you can reduce the chunk size. Refer to **Combine Method** for further information.
>



## Vertex Snap Threshold

Vertex snapping is performed on tiles that were painted using a **smooth** brush. The
maximum snapping distance can be altered if needed. No vertex snapping or smoothing is
performed on tiles when snapping threshold is set to zero.

*Default Value: 0.001*



## Static Snapping

Select this property when vertex snapping should also be performed on **static** tiles, or
tiles that were painted using a static brush.



## Generate Second UVs

Select this property when a second set of UV coordinates should be generated for
**static** tiles. This can be useful when creating lightmaps.

>
> **Note** - Utilizes the built-in secondary UV generation functionality.
>
> See [Unwrapping.GenerateSecondaryUVSet].
>



## Pre-generate Procedural

Causes all tiles that are painted using procedural brushes to be pre-generated. Selecting
this option allows tile data and brush references to be stripped. Procedurally generated
meshes will be saved into the optimized scene which will naturally increase its file size.

Procedural tiles will be generated at runtime if this option is not specified.



## Reduce Box Colliders

Searches for adjacent box colliders which were painted using a static brush and then
attempts to combine them to reduce the overall number of colliders. Box colliders can only
be combined when their corner points connect.


Snap Threshold
: Threshold which must be satisfied to determine whether two corner points occupy the same
  space.

Keep Separate
: Colliders can be kept separate by tag and/or layer allowing collision detection logic to
  differentiate between colliders by tag and/or layer.

  Set to **Nothing** to reduce colliders regardless of tag and layer. This can lead to
  fewer colliders but may affect runtime behavior.

Include tiles flagged as solid
: Tiles which are flagged as "solid" can be promoted into box colliders allowing them to
  be reduced with actual colliders. This helps to reduce the number of colliders in your
  scene since colliders would otherwise form around a gap.

  The "Grass Platform" demonstration brush is an example of a brush which benefits from
  this.



[Mesh.SetTriangles]: http://docs.unity3d.com/Documentation/ScriptReference/Mesh.SetTriangles.html
[Unwrapping.GenerateSecondaryUVSet]: http://docs.unity3d.com/Documentation/ScriptReference/Unwrapping.GenerateSecondaryUVSet.html
