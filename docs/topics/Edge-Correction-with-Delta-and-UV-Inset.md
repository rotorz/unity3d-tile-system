One of the most common ways to reduce bleeding at the edges of tiles is to inset tile UVs
by a small amount; half a texel tends to be quite effective.

This technique works quite well for some scenarios. When tiles are viewed too closely you
will notice that the outer pixels of each tile appear smaller than the other pixels. This
is because the UVs have been inset by a small amount. It is likely that you will see tile
borders when tiles are viewed closely with bilinear or trilinear filtering.

>
> **Tip** - Where possible you should use the border correction method. Whilst it can take
> longer to produce the artwork, the results are generally better.
>
