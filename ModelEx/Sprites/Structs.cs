using System;
using SlimDX;
using SlimDX.Direct3D11;

namespace SpriteTextRenderer
{
	internal class CharTableDescription
	{
		internal Texture2D Texture = null;
		internal ShaderResourceView SRV;
		internal CharDescription[] Chars = new CharDescription[256];
	}

	internal class CharDescription
	{
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

	public class StringMetrics
	{
		public Vector2 TopLeft { get; set; }

		public Vector2 Size { get; set; }

		public Vector2 BottomRight
		{
			get { return TopLeft + Size; }
		}

		public float OverhangLeft { get; set; }

		public float OverhangRight { get; set; }

		public float OverhangTop { get; set; }

		public float OverhangBottom { get; set; }

		public Vector2 FullRectTopLeft
		{
			get
			{
				return new Vector2(
					TopLeft.X - OverhangLeft * (Size.X < 0 ? -1 : 1),
					TopLeft.Y - OverhangTop * (Size.Y < 0 ? -1 : 1));
			}
		}

		public Vector2 FullRectSize
		{
			get
			{
				return new Vector2(
					Size.X + (OverhangLeft + OverhangRight) * (Size.X < 0 ? -1 : 1),
					Size.Y + (OverhangTop + OverhangBottom) * (Size.Y < 0 ? -1 : 1));
			}
		}

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
			if (this.Size.X * second.Size.X < 0)
				throw new ArgumentException("The x-axis of the current instance is " +
					(xAxisFlipped ? "" : "not ") + "flipped. The x-axis of the second instance has to point in the same direction");
			if (this.Size.Y * second.Size.Y < 0)
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
