using System;
using System.Collections.Generic;
using SlimDX;

namespace ModelEx
{
	public class ModelNode
	{
		public string Name = "";
		public Matrix Transform = Matrix.Identity;
		public bool Visible = true;
		public List<ModelNode> Nodes { get; } = new List<ModelNode>();
		public List<int> SubMeshIndices { get; } = new List<int>();
	}
}