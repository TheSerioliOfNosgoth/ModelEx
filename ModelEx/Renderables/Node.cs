using System;
using System.Collections.Generic;

namespace ModelEx
{
    public class Node
    {
        public string Name = "";
        public SlimDX.Matrix Transform = SlimDX.Matrix.Identity;
        public List<Node> Nodes { get; } = new List<Node>();
        public List<int> SubMeshIndices { get; } = new List<int>();
        public Boolean Visible = true;
    }
}