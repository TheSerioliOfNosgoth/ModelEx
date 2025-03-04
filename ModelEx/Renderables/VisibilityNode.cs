using System;
using System.Collections.Generic;
using SlimDX;

namespace ModelEx
{
	public class VisibilityNode
	{
		public string Name = "";
		public Matrix Transform = Matrix.Identity;
		public bool Visible = true;
		public List<VisibilityNode> Nodes { get; } = new List<VisibilityNode>();

		protected BoundingSphere _boundingSphere;
		protected Model _model;

		public VisibilityNode()
		{
			SetBoundingSphere(_boundingSphere);
		}

		public VisibilityNode(BoundingSphere boundingSphere)
		{
			SetBoundingSphere(boundingSphere);
		}

		public void SetBoundingSphere(BoundingSphere boundingSphere)
		{
			_boundingSphere = boundingSphere;

			RenderResource resource = RenderManager.Instance.Resources[""];
			int modelIndex = (int)RenderResourceShapes.Shape.Sphere1;
			_model = resource.Models[modelIndex];
		}

		public void Render(Matrix transform)
		{
			float radius = _boundingSphere.Radius;

			if (!Visible || radius <= 0)
			{
				return;
			}

			Matrix scaleMatrix = Matrix.Scaling(radius, radius, radius);
			Matrix translateMatrix = Matrix.Translation(_boundingSphere.Center);
			Matrix localTransform = scaleMatrix * translateMatrix * Transform * transform;

			_model.Render(localTransform, null);
		}
	}
}