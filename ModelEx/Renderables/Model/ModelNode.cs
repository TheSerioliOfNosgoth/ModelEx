using System;
using System.Collections.Generic;

namespace ModelEx
{
	public class ModelNode
	{
		public string Name = "";
		public SlimDX.Matrix Transform = SlimDX.Matrix.Identity;
		public Boolean Visible = true;
		public List<ModelNode> Nodes { get; } = new List<ModelNode>();
		public List<int> SubMeshIndices { get; } = new List<int>();
	}
}