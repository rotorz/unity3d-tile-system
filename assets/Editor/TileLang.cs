// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.Localization;
using Rotorz.Tile.Editor.Internal;
using System.Globalization;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Language domain for tile system package.
    /// </summary>
    [DiscoverablePackageLanguage]
    public sealed class TileLang : PackageLanguage<TileLang>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TileLang"/> class.
        /// </summary>
        public TileLang()
            : base("@rotorz/unity3d-tile-system", CultureInfo.GetCultureInfo("en-US"))
        {
        }


        /// <inheritdoc/>
        public override void Reload()
        {
            base.Reload();
            UnityIntegrationUtility.Regenerate();
        }


        public static string FormatActionWithShortcut(string actionText, string shortcutKeys)
        {
            return string.Format(
                /* Format string to annotate action name with shortcut keys.
                   i.e. 'Goto Target Brush (F3)'
                   0: action text
                   1: shortcut keys; for instance, "B" */
                TileLang.ParticularText("Format|ActionWithShortcut", "{0} ({1})"),
                actionText, shortcutKeys
            );
        }

        public static string FormatPixelMetric(int quantity)
        {
            return string.Format(
                /* Format quantity of pixels.
                   i.e. '42 pixels'
                   0: quantity of pixels */
                TileLang.ParticularText("Format|PixelMetric", "{0} pixels"),
                quantity
            );
        }

        public static string FormatPixelFractionMetric(float fraction)
        {
            return string.Format(
                /* Format fraction of a pixel.
                   i.e. '25% of 1 pixel'
                   0: number typically between 0 and 100 */
                TileLang.ParticularText("Format|PixelFractionMetric", "{0}% of 1 pixel"),
                fraction
            );
        }

        public static string FormatYesNoStatus(bool status)
        {
            return status
                ? TileLang.ParticularText("Status", "Yes")
                : TileLang.ParticularText("Status", "No");
        }

        public static string FormatDragObjectTitle(string objectName, string objectType)
        {
            return string.Format(
                /* Format title string of object that is being dragged.
                   i.e. 'Grass Brush (OrientedBrush)'
                   0: name of the object
                   1: type of the object */
                TileLang.ParticularText("Format|DragObjectTitle", "{0} ({1})"),
                objectName, objectType
            );
        }
    }
}
