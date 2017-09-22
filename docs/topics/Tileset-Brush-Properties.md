Tileset brushes have special properties which allow you to attach game objects and define
whether they are procedural.


## Procedural

Tileset brushes will typically inherit the procedural property of their tilesets. This
property allows you to mark brushes as procedural or non-procedural on a per brush basis.

This is useful for scenarios where the majority of a tileset is to be used procedurally
with the exception of certain select tiles. It is also possible to create both a
procedural and non-procedural brush for the same tile.


**Inherit**
: Use to inherit value of [Default Procedural Setting]
  property from tileset.

**Yes**
: Mark as procedural.

**No**
: Mark as non-procedural.


When this property is changed any tiles previously painted using this brush must be
refreshed before changes become apparent. Additional mesh assets will be generated when
needed for non-procedural brushes.

See [Procedural and Non-Procedural Tiles] for further information.



## Attach Prefab

Additional game objects can be attached to painted tiles by specifying a prefab to attach.
This allows you to add components and scripts to each painted tile.



## Add Collider

When selected, box colliders are automatically added to each painted tile. Avoid adding
colliders unless absolutely necessary because exessive numbers of colliders will incur
greater overhead.

This property is only able to add box colliders to tiles. Other types of colliders can be
specified by attaching a prefab instead.



[Default Procedural Setting]: ./Default-Tileset-Procedural-Setting.md
[Procedural and Non-Procedural Tiles]: ./Procedural-and-Non-Procedural-Tiles.md
