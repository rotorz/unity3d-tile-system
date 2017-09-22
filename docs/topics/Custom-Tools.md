You can implement your very own tools for interacting with tile systems which are shown
alongside the provided tools in the **Tools** palette. For consistency we recommend that
custom tools should only interact with the active tile system.

A custom tool can be defined by creating a new class extending `ToolBase` which can then
be registered using `ToolManager.RegisterTool`.

To improve the usability of your tool you should design an appropriate 22x22 icon graphic.
You might decide to create two versions of your icon to suite both the pro and non-pro
skins that are provided by Unity if you intend to distribute your custom tool.
