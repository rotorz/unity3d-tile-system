Procedural meshes are generated on a per chunk basis but cannot be generated when the
maximum number of vertices has been exceeded.


## Symptoms

Procedural mesh is not updated and an error message is logged in **Console** window saying
"Chunk size of 'X' is too large for procedural mesh".



## Causes

There is a hard limit of 64K vertices per mesh which is exceeded when chunk size is too
large.



## Resolve

Reduce chunk size of affected tile system so that maximum vertex count is not exceeded.
The maximum chunk size for tile systems that contain procedural tiles has an area of
100x100 tiles (though not necessarily square).
