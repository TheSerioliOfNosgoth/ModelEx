using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimDX;

namespace SpriteTextRenderer
{
    /// <summary>
    /// Collection of helper methods for the SpriteRenderer and TextRenderer
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Returns a rectangle from the provided vector parameters.
        /// </summary>
        /// <param name="Position">Position of the rectangle's top left corner</param>
        /// <param name="Size">Size of the rectangle</param>
        /// <returns>The constructed rectangle</returns>
        public static RectangleF RectFromVectors(Vector2 Position, Vector2 Size)
        {
            return new RectangleF(Position.X, Position.Y, Size.X, Size.Y);
        }
    }
}
