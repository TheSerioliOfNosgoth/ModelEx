using System;
using System.Collections.Generic;

namespace ModelEx
{
	public class VisibilityNode
	{
		public string Name = "";
		public Boolean Visible = true;
		public List<VisibilityNode> Nodes { get; } = new List<VisibilityNode>();
	}
}