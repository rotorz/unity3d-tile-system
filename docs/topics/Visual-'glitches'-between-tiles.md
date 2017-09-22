Visual anomalies may be encountered when creating tiles or textures. There are a number of
causes of this.


## Diagnose

### Texture Tiling

Ensure that tiling of texture is seamless using graphics software.


### UV Coordinates

Ensure that UV coordinates are accurate enough for smooth tiling. Zoom into vertices and
align them to your texture as accurately as possible.

>
> **Tip** - Make use of the snapping functionality of your modelling software. Some
> modelling packages allow selected vertices to be snapped together on a single axis
> (using scale in Blender, for example).
>


### Edge Bleeding

There are a number of factors that can contribute to edge bleeding:

- **Texture filtering** - Bilinear and trilinear filtering causes texture pixels to be
  blended with other adjacent pixels. Edge bleeding occurs when pixels are incorrectly
  blended across UV seams.

- **Mip-mapping** - Real-time applications make use of a rendering feature called
  mip-mapping which helps to improve the performance of rendering textured meshes by
  storing multiple versions of each texture at reduced qualities. Where possible the lower
  quality versions are used to improve performance. The process of generating reduced
  texture sizes causes blending to occur across UV seams.

- **Lossy texture compression** - Compression artefacts exist within compressed versions
  of textures. Whilst the effects of this are often not prominent, they can lead to edge
  bleeding in atlas textures where such artefacts cross UV seams.

- **Use of Non-Power-Of-Two (NPOT) textures** - It seems that NPOT textures are internally
  resized into power-of-two textures. The process of upscaling/downscaling the texture
  causes pixels to be blended and thus leading to artefacts when using atlas textures.

In most scenarios the effect of edge bleeding can be counteracted by adding suitable
borders around each UV island in your textures. Refer to [Edge Correction] for further
information regarding edge correction with respect to 2D tilesets.


### Smooth Brushes and Building

- When appropriate enable brush smoothing. This will smooth normals across tiles (provided
  that topology of tile meshes allow for this).

- Build tile system to benefit from snapped vertices and any smoothed joins.


### Vertex Threshold (When Building)

Ensure that snapping vertex threshold is sufficient for the scale that you are working
with. The vertex threshold can be altered using the inspector panel by selecting the tile
system. Please note that changes will only become apparent when tile system is rebuilt.



[Edge Correction]: ./Edge-Correction.md
