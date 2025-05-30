﻿using System;
using System.Collections.Generic;
using SlimDX;

namespace ModelEx
{
	public class Model : Renderable
	{
		public List<Material> Materials { get; } = new List<Material>();
		public List<Mesh> Meshes { get; } = new List<Mesh>();
		public List<SubMesh> SubMeshes { get; } = new List<SubMesh>();
		public ModelNode Root { get; } = new ModelNode();

		public Model(IModelParser modelParser)
		{
			Name = modelParser.ModelName;
			Materials.AddRange(modelParser.Materials);
			Meshes.AddRange(modelParser.Meshes);
			SubMeshes.AddRange(modelParser.SubMeshes);
			Root.Nodes.AddRange(modelParser.Groups);
			Root.Name = modelParser.ModelName;
		}

		public override void Render()
		{
			if (SubMeshes.Count > 0)
			{
				RenderNode(Root, Matrix.Identity, null, false);
				RenderNode(Root, Matrix.Identity, null, true);
			}
		}

		public void Render(Matrix transform, VisibilityNode visibilityNode)
		{
			if (SubMeshes.Count > 0)
			{
				RenderNode(Root, transform, visibilityNode, false);
				RenderNode(Root, transform, visibilityNode, true);
			}
		}

		public void RenderNode(ModelNode node, Matrix transform, VisibilityNode visibilityNode, bool isTransparent)
		{
			if (visibilityNode != null && !visibilityNode.Visible)
			{
				return;
			}

			Matrix localTransform;
			if (visibilityNode == null)
			{
				localTransform = node.Transform;
			}
			else
			{
				localTransform = visibilityNode.Transform;
			}
			localTransform *= transform;

			foreach (int subMeshIndex in node.SubMeshIndices)
			{
				SubMesh subMesh = SubMeshes[subMeshIndex];
				Mesh mesh = Meshes[subMesh.MeshIndex];
				Material material = Materials[subMesh.MaterialIndex];
				//if (material.Visible)
				//{
				if ((material.BlendMode == 0 && !isTransparent) || (material.BlendMode != 0 && isTransparent))
				{
					mesh.ApplyMaterial(material);
					mesh.ApplyTransform(localTransform);
					mesh.ApplyBuffers();
					mesh.Render(subMesh.indexCount, subMesh.startIndexLocation, subMesh.baseVertexLocation);
				}
				//}
			}

			int n = 0;
			foreach (ModelNode child in node.Nodes)
			{
				// Nodes are all in the same space as the instance of the model,
				// so use transform here rather than localTransform.
				// Relative positions might be needed as an option later on,
				// in which case, use localTransform.
				RenderNode(child, transform, visibilityNode?.Nodes[n], isTransparent);
				n++;
			}

			// visibilityNode.Transform is applied inside. Don't do it twice!
			visibilityNode?.Render(transform);
		}

		public ModelNode FindNode(string name)
		{
			return FindNode(name, Root);
		}

		public ModelNode FindNode(string name, ModelNode node)
		{
			if (node.Name == name)
			{
				return node;
			}

			foreach (ModelNode child in node.Nodes)
			{
				node = FindNode(name, child);
				if (node != null)
				{
					return node;
				}
			}

			return null;
		}

		public override BoundingSphere GetBoundingSphere()
		{
			BoundingSphere boundingSphere = new BoundingSphere();
			foreach (Mesh mesh in Meshes)
			{
				boundingSphere = BoundingSphere.Merge(boundingSphere, mesh.BoundingSphere);
			}

			return boundingSphere;
		}

		public override void Dispose()
		{
			foreach (Mesh mesh in Meshes)
			{
				mesh?.Dispose();
			}
		}
	}
}