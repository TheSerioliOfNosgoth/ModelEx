using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;
using System.Runtime.InteropServices;

namespace SpriteTextRenderer
{
    internal class SpriteVertexLayout
    {
        internal static InputElement[] Description = {
            new InputElement("TEXCOORD", 0, SlimDX.DXGI.Format.R32G32_Float, 0, 0),
            new InputElement("TEXCOORDSIZE", 0, SlimDX.DXGI.Format.R32G32_Float, 8, 0),
            new InputElement("POSITION", 0, SlimDX.DXGI.Format.R32G32_Float, 16, 0),
            new InputElement("SIZE", 0, SlimDX.DXGI.Format.R32G32_Float, 24, 0),
            new InputElement("COLOR", 0, SlimDX.DXGI.Format.B8G8R8A8_UNorm, 32, 0)};

        internal struct Struct
        {
            internal Vector2 TexCoord;
            internal Vector2 TexCoordSize;
            internal Vector2 Position;
            internal Vector2 Size;
            internal int Color;

            internal static int SizeInBytes { get { return Marshal.SizeOf(typeof(Struct)); } }
        }
    }

    /// <summary>
    /// This structure holds data for sprites with a specific texture
    /// </summary>
    internal class SpriteSegment
    {
        internal ShaderResourceView Texture;
        internal List<SpriteVertexLayout.Struct> Sprites = new List<SpriteVertexLayout.Struct>();
    }

    internal class CharTableDescription
    {
        internal Texture2D Texture = null;
        internal ShaderResourceView SRV;
        internal CharDescription[] Chars = new CharDescription[256];
    }

    internal class CharDescription
    {
        /// <summary>
        /// Size of the char excluding overhangs
        /// </summary>
        internal Vector2 CharSize;
        internal float OverhangLeft, OverhangRight, OverhangTop, OverhangBottom;

        internal Vector2 TexCoordsStart;
        internal Vector2 TexCoordsSize;

        internal CharTableDescription TableDescription;

        internal StringMetrics ToStringMetrics(Vector2 position, float scalX, float scalY)
        {
            return new StringMetrics
            {
                TopLeft = position,
                Size = new Vector2(CharSize.X * scalX, CharSize.Y * scalY),
                OverhangTop = Math.Abs(scalY) * OverhangTop,
                OverhangBottom = Math.Abs(scalY) * OverhangBottom,
                OverhangLeft = Math.Abs(scalX) * OverhangLeft,
                OverhangRight = Math.Abs(scalX) * OverhangRight,
            };
        }
    }

    /// <summary>
    /// Defines, in which area a specific text is rendered
    /// </summary>
    /// <remarks>
    /// <para>The textblock is the area filled with actual characters without any overhang.</para>
    /// <para>Overhangs enlarge the textblock rectangle. I.e. if OverhangLeft is 10, then there are parts of the text that are rendered 10 units left of the actual text block.
    /// However, these parts do not count as real text.
    /// If an overhang is negative, there is no letter, which actually reaches the textblock edge. Thus, the textblock is rendered on a smaller area.</para>
    /// <para>The full rect is the actual rendering space. I.e. the textblock with overhangs.</para>
    /// </remarks>
    /// <example>
    /// <para>Consider the following example. The string "Example Text" has been drawn at position (20, 40).</para>
    /// <img src="../Blocks.jpg" alt="Block structure"/>
    /// <para>The light red block is the text block. This is the layout rectangle. Text blocks containing one line
    /// usually have the same height - the line height. Therefore, text blocks can easily be concatenated without
    /// worrying about layout.</para>
    /// <para>The dark red block is the actual FullRect. This is the rectangle that exactly fits the rendered text.
    /// The difference between text block and full rect is described by the overhangs. If an overhang is positive,
    /// then the full rect is bigger than the textblock (as for the right side). If it is negative, the full rect
    /// is smaller (as for the other sides).</para>
    /// <para>
    /// Here are the actual values for the example:
    /// <ul>
    /// <li>TopLeft: (20, 40)</li>
    /// <li>Size: (449.17, 117.9)</li>
    /// <li>OverhangLeft: -14.48</li>
    /// <li>OverhangRight: 12.30</li>
    /// <li>OverhangTop: -15.06</li>
    /// <li>OverhangBottom: -4.54</li>
    /// </ul>
    /// </para>
    /// </example>
    public class StringMetrics
    {
        /// <summary>
        /// Top left corner of the textblock.
        /// </summary>
        public Vector2 TopLeft { get; set; }

        /// <summary>
        /// Size of the textblock.
        /// </summary>
        public Vector2 Size { get; set; }

        /// <summary>
        /// Returns the bottom right corner of the textblock
        /// </summary>
        public Vector2 BottomRight
        {
            get { return TopLeft + Size; }
        }

        /// <summary>
        /// The space that is added to the textblock by overhangs on the left side.
        /// </summary>
        public float OverhangLeft { get; set; }

        /// <summary>
        /// The space that is added to the textblock by overhangs on the right side.
        /// </summary>
        public float OverhangRight { get; set; }

        /// <summary>
        /// The space that is added above the textblock by overhangs.
        /// </summary>
        public float OverhangTop { get; set; }

        /// <summary>
        /// The space that is added below the textblock by overhangs.
        /// </summary>
        public float OverhangBottom { get; set; }

        /// <summary>
        /// The top left corner of the full rect.
        /// </summary>
        public Vector2 FullRectTopLeft
        {
            get
            {
                return new Vector2(
                    TopLeft.X - OverhangLeft * (Size.X < 0 ? -1 : 1),
                    TopLeft.Y - OverhangTop * (Size.Y < 0 ? -1 : 1));
            }
        }

        /// <summary>
        /// The size of the full rect.
        /// </summary>
        public Vector2 FullRectSize
        {
            get
            {
                return new Vector2(
                    Size.X + (OverhangLeft + OverhangRight) * (Size.X < 0 ? -1 : 1),
                    Size.Y + (OverhangTop + OverhangBottom) * (Size.Y < 0 ? -1 : 1));
            }
        }

        /// <summary>
        /// Merges this instance of StringMetrics with another instance. 
        /// The textblock and overhangs of this instance will be increased to cover both instances.
        /// </summary>
        /// <param name="second">The second StringMetrics instance. This object will not be changed.</param>
        /// <exception cref="System.ArgumentException">Thrown when one instance has flipped axes and the other does not.</exception>
        public void Merge(StringMetrics second)
        {
            //if current instance has no values yet, take the values of the second instance
            if (Size.X == 0 && Size.Y == 0)
            {
                TopLeft = second.TopLeft;
                Size = second.Size;
                OverhangLeft = second.OverhangLeft;
                OverhangRight = second.OverhangRight;
                OverhangTop = second.OverhangTop;
                OverhangBottom = second.OverhangBottom;
                return;
            }
            //if second instance is not visible, do nothing
            if (second.FullRectSize.X == 0 && second.FullRectSize.Y == 0)
                return;

            //Flipped y axis means that positive y points upwards
            //Flipped x axis means that positive x points to the right
            bool xAxisFlipped = Size.X < 0;
            bool yAxisFlipped = Size.Y < 0;
            //Check, if axes of both instances point in the same direction
            if(this.Size.X * second.Size.X < 0)
                throw new ArgumentException("The x-axis of the current instance is " + 
                    (xAxisFlipped ? "" : "not ") + "flipped. The x-axis of the second instance has to point in the same direction");
            if(this.Size.Y * second.Size.Y < 0)
                throw new ArgumentException("The y-axis of the current instance is " + 
                    (yAxisFlipped ? "" : "not ") + "flipped. The y-axis of the second instance has to point in the same direction");

            //Update flipped info if it cannot be obtained from the current instance
            if (Size.X == 0)
                xAxisFlipped = second.Size.X < 0;
            if (Size.Y == 0)
                yAxisFlipped = second.Size.Y < 0;

            //Find the functions to determine the topmost of two values and so on
            Func<float, float, float> findTopMost, findBottomMost;
            Func<float, float, float> findLeftMost, findRightMost;
            if (yAxisFlipped)
            {
                findTopMost = Math.Max;
                findBottomMost = Math.Min;
            }
            else
            {
                findTopMost = Math.Min;
                findBottomMost = Math.Max;
            }

            if (xAxisFlipped)
            {
                findLeftMost = Math.Max;
                findRightMost = Math.Min;
            }
            else
            {
                findLeftMost = Math.Min;
                findRightMost = Math.Max;
            }

            //Find new textblock
            float top = findTopMost(this.TopLeft.Y, second.TopLeft.Y);
            float bottom = findBottomMost(this.TopLeft.Y + this.Size.Y, second.TopLeft.Y + second.Size.Y);
            float left = findLeftMost(this.TopLeft.X, second.TopLeft.X);
            float right = findRightMost(this.TopLeft.X + this.Size.X, second.TopLeft.X + second.Size.X);

            //Find new overhangs
            float topOverhangPos = findTopMost(this.FullRectTopLeft.Y, second.FullRectTopLeft.Y);
            float bottomOverhangPos = findBottomMost(this.FullRectTopLeft.Y + this.FullRectSize.Y, second.FullRectTopLeft.Y + second.FullRectSize.Y);
            float leftOverhangPos = findLeftMost(this.FullRectTopLeft.X, second.FullRectTopLeft.X);
            float rightOverhangPos = findRightMost(this.FullRectTopLeft.X + this.FullRectSize.X, second.FullRectTopLeft.X + second.FullRectSize.X);

            TopLeft = new Vector2(left, top);
            Size = new Vector2(right - left, bottom - top);
            OverhangLeft = (left - leftOverhangPos) * (xAxisFlipped ? -1 : 1);
            OverhangRight = (rightOverhangPos - right) * (xAxisFlipped ? -1 : 1);
            OverhangTop = (top - topOverhangPos) * (yAxisFlipped ? -1 : 1);
            OverhangBottom = (bottomOverhangPos - bottom) * (yAxisFlipped ? -1 : 1);
        }
    }
}
