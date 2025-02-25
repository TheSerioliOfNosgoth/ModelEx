using System;
using System.Collections.Generic;
using SlimDX;

namespace ModelEx
{
	public class VisibilityNode
	{
		public string Name = "";
		public Matrix Transform = Matrix.Identity;
		public bool Visible = true;
		public List<VisibilityNode> Nodes { get; } = new List<VisibilityNode>();
	}
}