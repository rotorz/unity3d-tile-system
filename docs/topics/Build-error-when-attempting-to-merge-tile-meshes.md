Merged meshes must not exceed the maximum number of vertices per mesh. The chunk size or
combine method should be configured in a way that is appropriate for your requirements.


## Symptoms

The following error message is logged to the console window when attempting to build one
or more tile systems:

```
count <= std::numeric_limits<UInt16>::max();
```



## Causes

This error occurs when the number of vertices in the combined mesh exceeds the maximum
number of vertices permitted in a mesh (64K).



## Resolve

Reduce the size of the combined meshes.


### Deselect **Combine into submeshes**:

When using multiple materials you can sometimes get a little extra mileage before
encountering the 64k vertex limit by combining tiles into a separate mesh for each
material. Though if you continue to encounter the 64k vertex limit you will need to reduce
the size of your chunks.


### Reduce chunk size:

The following advise is based upon the selected **Combine Method**.


**By Chunk**
: Specify a smaller chunk size using the "Custom Chunk in Tiles" combine method instead.

**By Tile System**
: Consider using one of the other combine methods instead.

**Custom Chunk In Tiles**
: Specify smaller chunk size.


You may want to consider the following if the above advice does not help you to resolve
your problem:

- Avoid selecting "Static" property for some brushes so that not all tiles are combined.

- Utilize the static batching feature of Unity Pro instead.

- Reduce the number of vertices in your tile meshes.
