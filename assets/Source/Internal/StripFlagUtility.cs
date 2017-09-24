// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile.Internal
{
    public static class StripFlagUtility
    {
        /// <summary>
        /// Get bitmask representation of stripping options from preset.
        /// </summary>
        /// <param name="preset">Stripping preset.</param>
        /// <returns>
        /// Bitmask representation of preset.
        /// </returns>
        public static int GetPresetOptions(StrippingPreset preset)
        {
            switch (preset) {
                case StrippingPreset.StripRuntime:
                    return (StripFlag.STRIP_TILE_SYSTEM | StripFlag.STRIP_CHUNK_MAP | StripFlag.STRIP_TILE_DATA | StripFlag.STRIP_BRUSH_REFS | StripFlag.STRIP_EMPTY_CHUNKS | StripFlag.STRIP_COMBINED_EMPTY | StripFlag.STRIP_PLOP_COMPONENTS);

                case StrippingPreset.KeepSystemComponent:
                    return (StripFlag.STRIP_CHUNK_MAP | StripFlag.STRIP_TILE_DATA | StripFlag.STRIP_BRUSH_REFS | StripFlag.STRIP_EMPTY_CHUNKS | StripFlag.STRIP_COMBINED_EMPTY | StripFlag.STRIP_PLOP_COMPONENTS);

                case StrippingPreset.NoStripping:
                    return 0;

                case StrippingPreset.RuntimeAccess:
                    return (StripFlag.STRIP_BRUSH_REFS | StripFlag.STRIP_EMPTY_CHUNKS | StripFlag.STRIP_COMBINED_EMPTY);

                case StrippingPreset.RuntimePainting:
                    return (StripFlag.STRIP_EMPTY_CHUNKS | StripFlag.STRIP_COMBINED_EMPTY);

                case StrippingPreset.StripEverything:
                    return 0xFFFF;

                default:
                    return 0x0000;
            }
        }

        /// <summary>
        /// Pre-filter stripping options to ensure that required dependencies are present.
        /// </summary>
        /// <param name="options">Bitmask of stripping options.</param>
        /// <returns>
        /// Filtered bitmask of stripping options.
        /// </returns>
        public static int PreFilterStrippingOptions(int options)
        {
            // Note: Changes should also be reflected for each stripping property.

            if ((options & (StripFlag.STRIP_TILE_SYSTEM | StripFlag.STRIP_CHUNKS)) != 0) {
                options |= StripFlag.STRIP_CHUNK_MAP;
            }
            if ((options & StripFlag.STRIP_CHUNK_MAP) != 0) {
                options |= StripFlag.STRIP_TILE_DATA;
            }
            if ((options & StripFlag.STRIP_TILE_DATA) != 0) {
                options |= StripFlag.STRIP_BRUSH_REFS;
            }
            if ((options & StripFlag.STRIP_CHUNKS) != 0) {
                options |= StripFlag.STRIP_EMPTY_CHUNKS;
            }
            if ((options & StripFlag.STRIP_EMPTY_OBJECTS) != 0) {
                options |= StripFlag.STRIP_COMBINED_EMPTY;
            }
            return options;
        }
    }
}
