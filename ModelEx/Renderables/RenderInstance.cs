using System;
using System.Collections.Generic;
using SlimDX;

namespace ModelEx
{
	public class RenderInstance : Renderable
	{
		protected readonly RenderResource _renderResource;
		protected int _modelIndex = 0;

		public Model Model { get; protected set; }

		public RenderInstance(RenderResource renderResource, int modelIndex)
		{
			_renderResource = renderResource;
			_modelIndex = modelIndex;

			Model = renderResource.Models[modelIndex];
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