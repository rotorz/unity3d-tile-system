Certain runtime functionality becomes unavailable when stripping is applied to a tile
system. The level of capabilities lost depends upon the level of stripping that has been
applied.

[Stripping Presets] were added to help make the consequences of the stripping process
easier to understand. If you attempt to access tile data when tile data has been stripped,
you will experience a number of errors including `NullReferenceException` exceptions.

>
> **Important** - Ensure that tile system component is not stripped when some runtime
> aspects are required because it is always required when using runtime API. See
> [Stripping] for more information.
>



[Stripping]: ./Stripping.md
