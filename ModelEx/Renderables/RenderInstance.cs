using System;
using System.Collections.Generic;
using SlimDX;

namespace ModelEx
{
	public class RenderInstance : Renderable
	{
		protected string _resourceName = "";
		protected int _modelIndex = 0;

		public readonly VisibilityNode Root = new VisibilityNode();

		protected Model Model { get; set; }

		public RenderInstance(string resourceName, int modelIndex)
		{
			_resourceName = resourceName;
			_modelIndex = modelIndex;

			RenderResource resource;
			if (RenderManager.Instance.Resources.ContainsKey(resourceName))
			{
				resource = RenderManager.Instance.Resources[resourceName];
			}
			else
			{
				resource = RenderManager.Instance.Resources[""];
			}

			Model = resource.Models[modelIndex];

			Root.Name = Model.Root.Name;
			foreach (ModelNode modelNode in Model.Root.Nodes)
			{
				VisibilityNode visibilityNode = new VisibilityNode();
				visibilityNode.Name = modelNode.Name;
				Root.Nodes.Add(visibilityNode);
			}
		}

		public VisibilityNode FindNode(string name)
		{
			return FindNode(name, Root);
		}

		public VisibilityNode FindNode(string name, VisibilityNode visibilityNode)
		{
			if (visibilityNode.Name == name)
			{
				return visibilityNode;
			}

			foreach (VisibilityNode child in visibilityNode.Nodes)
			{
				visibilityNode = FindNode(name, child);
				if (visibilityNode != null)
				{
					return visibilityNode;
				}
			}

			return null;
		}

		public override void Render()
		{
			Model.Render(Transform, Root);
		}

		public override BoundingSphere GetBoundingSphere()
		{
			return Model.GetBoundingSphere();
		}

		public override void Dispose()
		{
		}
	}
}