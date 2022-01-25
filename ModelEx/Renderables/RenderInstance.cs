using System;
using System.Collections.Generic;
using SlimDX;

namespace ModelEx
{
	public class RenderInstance : Renderable
	{
		protected string _resourceName = "";
		protected int _modelIndex = 0;

		public Model Model { get; protected set; }

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
		}

		public override void Render()
		{
			Model.Render(Transform);
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