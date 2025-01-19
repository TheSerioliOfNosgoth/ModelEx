using System;
using System.Collections.Generic;
using SlimDX;

namespace ModelEx
{
	public class DynamicInstance : RenderInstance
	{
		protected string _resourceName = "";
		protected string _modelName = "";

		public DynamicInstance(string resourceName, string modelName, Vector3 position, Quaternion rotation, Vector3 scale)
			: base(position, rotation, scale)
		{
			_resourceName = resourceName;
			_modelName = modelName;

			UpdateModel();
		}

		public DynamicInstance(string resourceName, string modelName, Vector3 position, Quaternion rotation, Vector3 scale, RenderResource resource)
			: base(position, rotation, scale)
		{
			_resourceName = resourceName;
			_modelName = modelName;

			UpdateModel(resource);
		}

		public void UpdateModel()
		{
			RenderResource resource = null;

			if (_resourceName != "" && RenderManager.Instance.Resources.ContainsKey(_resourceName))
			{
				resource = RenderManager.Instance.Resources[_resourceName];
			}

			UpdateModel(resource);
		}

		public void UpdateModel(RenderResource resource, string modelName = null)
		{
			if (modelName == null)
			{
				modelName = _modelName;
			}
			if (resource == null)
			{
				resource = RenderManager.Instance.Resources[""];
				modelName = "octahedron";
			}

			Model newModel = resource.Models.Find(x => x.Name == modelName);

			if (Model != newModel)
			{
				Root.Nodes.Clear();

				Model = newModel;

				Root.Name = Model.Root.Name;
				Root.Visible = Model.Root.Visible;
				Root.Transform = Model.Root.Transform;
				foreach (ModelNode modelNode in Model.Root.Nodes)
				{
					VisibilityNode visibilityNode = new VisibilityNode();
					visibilityNode.Name = modelNode.Name;
					visibilityNode.Visible = modelNode.Visible;
					visibilityNode.Transform = modelNode.Transform;
					Root.Nodes.Add(visibilityNode);
				}

				float height = (resource.Name == "") ? GetBoundingSphere().Radius : 0.0f;

				Matrix.RotationQuaternion(ref _rotation, out Matrix rotationMatrix);
				Matrix scaleMatrix = Matrix.Scaling(_scale);
				Transform = scaleMatrix * rotationMatrix * Matrix.Translation(
					_position.X,
					_position.Y + height,
					_position.Z
				);
			}
		}
	}
}