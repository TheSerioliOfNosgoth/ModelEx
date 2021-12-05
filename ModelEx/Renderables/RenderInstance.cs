using System;
using System.Collections.Generic;
using SlimDX;

namespace ModelEx
{
	public class RenderInstance : Renderable
	{
		public readonly Model Model;

		public RenderInstance(Model model)
		{
			Model = model;
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
			//model.Dispose();
		}
	}
}