Brush asset files will be empty if Unity crashes before asset files are saved successfully.


## Symptoms

One or more brush asset files appear to be empty with no associated script or data.



## Causes

When assets are modified they are usually flagged as being dirty which indicates to Unity
that they will need to be saved. If Unity crashes before assets are saved successfully you
can end up with empty asset files. This issue is not unique to brush assets.



## Advice

Be sure to save assets reguarly using **File | Save Project**; doing so will help to avoid
this issue.
