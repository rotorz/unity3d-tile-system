By default tileset brushes use a material with a very simple opaque shader for improved
performance. A different shader can be selected when alpha transparency is desired.


## Symptoms

Transparency is not working for 2D tileset or autotile brushes.



## Causes

Atlas material is using a fully opaque shader.



## Resolve

1. Navigate to atlas material (see [Locating Atlas Material]).

2. Select a shader that supports transparency using **Inspector**.

   i.e. **Unlit | Transparent**.



[Locating Atlas Material]: ./Locating-Atlas-Material.md
