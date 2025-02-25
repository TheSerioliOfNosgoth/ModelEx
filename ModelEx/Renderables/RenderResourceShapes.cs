using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelEx
{
	public class RenderResourceShapes : RenderResource
	{
		public enum Shape
		{
			Cube,
			Octahedron,
			Sphere0,
			Sphere1,
			Sphere2
		}

		public RenderResourceShapes()
			: base("")
		{
		}

		public void LoadModels()
		{
			if (Models.Count > 0)
			{
				return;
			}

			ModelParser cubeParser = new ModelParser("cube");
			cubeParser.BuildModel(this, RenderResourceShapes.Shape.Cube);
			Models.Add(cubeParser.Model);

			ModelParser octaParser = new ModelParser("octahedron");
			octaParser.BuildModel(this, RenderResourceShapes.Shape.Octahedron);
			Models.Add(octaParser.Model);

			ModelParser sphereParser0 = new ModelParser("sphere0");
			sphereParser0.BuildModel(this, RenderResourceShapes.Shape.Sphere0);
			Models.Add(sphereParser0.Model);

			ModelParser sphereParser1 = new ModelParser("sphere1");
			sphereParser1.BuildModel(this, RenderResourceShapes.Shape.Sphere1);
			Models.Add(sphereParser1.Model);

			ModelParser sphereParser2 = new ModelParser("sphere2");
			sphereParser2.BuildModel(this, RenderResourceShapes.Shape.Sphere2);
			Models.Add(sphereParser2.Model);
		}
	}
}
