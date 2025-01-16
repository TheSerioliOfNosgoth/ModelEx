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
			Sphere
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

			ModelParser sphereParser = new ModelParser("sphere");
			sphereParser.BuildModel(this, RenderResourceShapes.Shape.Sphere);
			Models.Add(sphereParser.Model);
		}
	}
}
