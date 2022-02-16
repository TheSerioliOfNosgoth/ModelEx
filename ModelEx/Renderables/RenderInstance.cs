using System;
using System.Collections.Generic;
using SlimDX;

namespace ModelEx
{
	public class RenderInstance : Renderable
	{
		protected string _resourceName = "";
		protected int _modelIndex = 0;
		protected Vector3 _position = new Vector3();

		public readonly VisibilityNode Root = new VisibilityNode();

		protected Model Model { get; set; }

		public RenderInstance(string resourceName, int modelIndex, Vector3 position)
		{
			_resourceName = resourceName;
			_modelIndex = modelIndex;
			_position = position;

			UpdateModel();
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

		public void UpdateModel()
		{
			RenderResource resource;
			if (RenderManager.Instance.Resources.ContainsKey(_resourceName))
			{
				resource = RenderManager.Instance.Resources[_resourceName];
			}
			else
			{
				resource = RenderManager.Instance.Resources[""];
			}

			if (Model != resource.Models[_modelIndex])
			{
				Model = resource.Models[_modelIndex];

				Root.Name = Model.Root.Name;
				foreach (ModelNode modelNode in Model.Root.Nodes)
				{
					VisibilityNode visibilityNode = new VisibilityNode();
					visibilityNode.Name = modelNode.Name;
					Root.Nodes.Add(visibilityNode);
				}

				float height = (resource.Name == "") ? GetBoundingSphere().Radius : 0.0f;
				Transform = Matrix.Translation(
					_position.X,
					_position.Y + height,
					_position.Z
				);
			}
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