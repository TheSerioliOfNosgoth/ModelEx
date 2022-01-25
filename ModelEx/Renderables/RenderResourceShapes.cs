using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelEx
{
	public class RenderResourceShapes : RenderResource
	{
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

			ModelParser octaParser = new ModelParser("octahedron");
			octaParser.BuildModel(this);

			Models.Add(octaParser.Model);
		}
	}
}
