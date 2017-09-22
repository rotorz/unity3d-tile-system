Unfortunately your hardware and the Unity software can only handle so much! There are
several tips that can help to overcome some of these limitations.


## Symptoms

Unity editor begins to perform slowly when scene contains a lot of tiles (amongst other
game objects).



## Resolve

### Tip #1 - Simplify brush prefabs

Ensure that the prefabs associated with brushes contain the bare minimal number of
components and colliders because these can have a severe impact upon performance (both in
editor and at runtime).

- Remove collider components and manually create and position larger colliders using game
  objects to reduce overall number of colliders. For example, instead of having 10 box
  colliders (one per tile) in a line for a platform, use 1.

- Minimize number of script components that are attached to tiles to the bare minimum.


### Tip #2 - Use multiple smaller tile systems

Compose scenes using multiple smaller tile systems instead of one large one. This can help
to improve performance in editor.


### Tip #3 - Use fewer (but larger) tiles

Try to reduce the number of tiles by creating larger tiles with larger (combined) meshes,
simpler colliders and fewer overall components. The tile system would thus contain less
tiles (whilst being the same physical size) meaning that there are less game objects in
the Unity editor. This can give give a massive performance boost.


### Tip #4 - Compose scene from multiple smaller optimized tile systems

1. Create each tile system in a separate scene with appropriate chunk sizes.

2. Ensure that appropriate stripping and build options are specified. It is important to
   ensure that static tiles are merged.

3. Build optimized version of each tile system as prefabs.

4. Compose final scene using the optimized prefabs to overcome some of these limitations.

>
> **Important** - Ensure that brushes are marked as "static" in order to benefit from
> these optimizations.
>



## Advice

If you are experiencing slow-down in the editor due to large quantities of tiles, you will
almost certainly benefit at runtime from optimized tile systems!
