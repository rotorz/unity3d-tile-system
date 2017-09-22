Answers to frequently asked questions!


## Is this product compatible with iOS and Android?

Yes, absolutely. Munchy Bunny! (available from app store) does in fact use this system.

Of course, the usual mobile development rules still apply, however. Where possible use the
built version of your scene when deploying to mobile devices and keep the number of unique
materials to a minimum to keep the number of draw calls as low as possible. Where possible
use texture atlases (combine textures).



## Can pooling be used to improve performance at runtime?

A custom pooling solution can be integrated into the painting/erasing of tiles (or
attachments) that are instantiated from prefabs by providing your own `IObjectFactory`
implementation. Effectively this allows you to integrate an existing solution or to
implement your very own.

An object factory adding support for [PoolManager by Path-o-logical Games] is already
available in the form of the open-source script [RtsPoolManagerObjectFactory].



## Can I interact with tile systems using PlayMaker?

A selection of PlayMaker actions are available from the following open-source repository
[RtsPlayMakerActions].



## Can I hide the default grid?

The grid shown in scene views can cause confusion when working with tile systems.
Fortunately this can be hidden via the **Gizmos** drop-down menu in the scene view:

![Hide placement grid toggle.](./img/faq/hide-grid.png)



## Can I hide colliders when painting?

Yes along with any other gizmos that get in the way of your creativity. Deselect
**BoxCollider** (along with any other undesirable gizmos) from the **Gizmos** drop-down
menu.



## How are the class libraries (DLL) used when building my project?

This product includes two DLL modules. One of these provides functionality for the Unity
editor whilst the other provides components necessary to maintain a working tile system at
runtime.

The editor DLL is not included in builds, but the other DLL is. If there is no requirement
to maintain functional tile systems at runtime then ensure that "Strip System Components"
is ticked when building your system.

The runtime DLL can be excluded from your project build manually by dragging the library
into an "Editor" folder. It is essential that the DLL is relocated using the Unity editor
to avoid breaking existing systems.

A number of warning messages will be presented in the editor console window at runtime for
scenes that contain runtime components. See [Stripping].



[Stripping]: ./guide/Stripping.md

[PoolManager by Path-o-logical Games]: http://poolmanager.path-o-logical.com/
[RtsPlayMakerActions]: https://bitbucket.org/rotorz/rtsplaymakeractions
[RtsPoolManagerObjectFactory]: https://bitbucket.org/rotorz/rtspoolmanagerobjectfactory
