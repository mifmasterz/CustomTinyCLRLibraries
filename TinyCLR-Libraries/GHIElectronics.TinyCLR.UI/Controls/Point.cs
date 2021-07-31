
using System;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>
    /// The Point object represents a location in a two-dimensional coordinate system.
    /// </summary>
    [Serializable]
    public struct Point {
        /// <summary>
        /// The horizontal coordinate of the point.
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        public int X;
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// The vertical coordinate of the point.
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        public int Y;
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Creates a new Point.
        /// </summary>
        /// <param name="x">X-axis position.</param>
        /// <param name="y">Y-axis position.</param>
        public Point(int x, int y) {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Formats the point as a string for debugging.
        /// </summary>
        /// <returns>The point as a string. E.g. [100, 100]</returns>
        public override string ToString() => "[" + this.X + ", " + this.Y + "]";
    }
}
