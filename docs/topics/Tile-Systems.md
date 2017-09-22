A tile system is a grid which allows you to effortlessly paint both 2D and 3D tiles
alongside one another using brushes. Tiles are automatically aligned and oriented to their
tile system upon being painted.

Tiles can be painted using the provided [tools] or by custom runtime or editor scripts.
Such scripts typically interact with tile system components and brush assets allowing you
to procedurally generate maps or even implement your very own in-game level designer!

Each tile system is a game object with the the `TileSystem` component attached. With the
exception of tiles that are painted using purely procedural brushes, each tile is an
individual game object. Data is stored on a per tile basis which can be accessed using the
runtime API or stripped from builds when not needed at runtime (see [Stripping]).



[tools]: ./Tools.md
[Stripping]: ./Stripping.md
