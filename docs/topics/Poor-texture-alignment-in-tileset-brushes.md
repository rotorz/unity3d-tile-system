Atlas tile textures do not align properly when painted onto a tile system. There are a
number of causes for such issues, this troubleshooting guide covers the most common
reported difficulties.


## Symptoms

- Pixels from adjacent tiles bleeding through onto painted tile.

- Visual seam between tiles.

- Artwork of multiple tiles appear on painted tile.

- Fraction of tile artwork appears on painted tile.

- Tile texture appears blurred and stretched.



## Causes

Possible causes for this problem:

- Incorrect or inconsistent tile sizes in texture itself.

- Tile texture does not support seamless tiling.

- Side effect called "Edge Bleeding" which has several contributing factors including:

  - Mip-mapping

  - Bilinear / trilinear texture filtering

  - Lossy texture compression

  - Use of Non-Power-Of-Two (NPOT) texture sizes



## Diagnose

### Ensure that tile sizes are consistent in atlas texture
				
Many graphics applications are able to display a grid with user defined cell sizes.

1. Open atlas texture using such software.

2. Enable grid where cell size matches that of your intended tile size.

3. Ensure that each tile fits perfectly within grid.


### Where applicable make sure that tiles tile seamlessly

1. Open atlas texture using a graphics package.

2. Copy one of the troublesome tiles and paste into a blank image.

3. Offset tile image by half of its size so that you can see how it joins with itself.

4. If tile doesn't tile against itself then this must be corrected.


### Try to disable mip-mapping for atlas texture

If creating an entirely two-dimensional game using an orthographic camera projection then
it is unlikely that you will benefit from mip-mapping.


### Make sure that texture is square and a power-of-two

1. Load atlas texture into graphics software.

2. Review image dimensions:

   If image is not perfectly square or does not have power-of-two dimensions then texture
   filtering may be the cause of your problem.
