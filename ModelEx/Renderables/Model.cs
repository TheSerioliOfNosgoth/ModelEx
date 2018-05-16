using System;
using System.Collections.Generic;

namespace ModelEx
{
    public class Model : Renderable
    {
        public List<Material> Materials { get; } = new List<Material>();
        public List<Mesh> Meshes { get; } = new List<Mesh>();
        public List<SubMesh> SubMeshes { get; } = new List<SubMesh>();
        public Node Root { get; } = new Node();

        public override void Render()
        {
            if (SubMeshes.Count > 0)
            {
                RenderNode(Root, Transform);
            }
        }

        public void RenderNode(Node node, SlimDX.Matrix transform)
        {
            //node.Visible = false;
            if (node.Visible == false)
            {
                return;
            }

            SlimDX.Matrix localTransform = transform * node.Transform;

            foreach (int subMeshIndex in node.SubMeshIndices)
            {
                SubMesh subMesh = SubMeshes[subMeshIndex];
                Mesh mesh = Meshes[subMesh.MeshIndex];
                Material material = Materials[subMesh.MaterialIndex];
                mesh.ApplyMaterial(material);
                mesh.ApplyTransform(localTransform);
                mesh.ApplyBuffers();
                mesh.Render(subMesh.indexCount, subMesh.startIndexLocation, subMesh.baseVertexLocation);
            }

            foreach (Node child in node.Nodes)
            {
                RenderNode(child, localTransform);
            }
        }

        public Node FindNode(string name)
        {
            return FindNode(name, Root);
        }

        public Node FindNode(string name, Node node)
        {
            if (node.Name == name)
            {
                return node;
            }

            foreach (Node child in node.Nodes)
            {
                node = FindNode(name, child);
                if (node != null)
                {
                    return node;
                }
            }

            return null;
        }

        public override void Dispose()
        {
            foreach (Mesh mesh in Meshes)
            {
                mesh.Dispose();
            }
        }
    }
}