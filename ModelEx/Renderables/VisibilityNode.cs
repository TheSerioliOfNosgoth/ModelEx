using System;
using System.Collections.Generic;

namespace ModelEx
{
	public class VisibilityNode
	{
		public string Name = "";
		public SlimDX.Matrix Transform = SlimDX.Matrix.Identity;
		public Boolean Visible = true;
		public List<VisibilityNode> Nodes { get; } = new List<VisibilityNode>();
	}
}