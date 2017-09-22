Default orientation is assumed when actual orientation of tile is not defined within brush
(or next best was not matched). The default orientation is also used to generate preview
for brush.


| Fallback Orientation     | Description                                                |
|--------------------------|:-----------------------------------------------------------|
| **Next Best** (default)  | Attempt to find next best orientation that is available before assuming the default orientation.  |
| **Use Default**          | Use default orientation when no strong matches are found.  |
| **Use Default Strict**   | Use default orientation when no exact matches are found.   |
