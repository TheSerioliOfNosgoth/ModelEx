using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC
{
	public interface IModel
	{
		string Name { get; }
		string ModelTypePrefix { get; }
		Polygon[] Polygons { get; }
		Geometry Geometry { get; }
		Geometry ExtraGeometry { get; }
		Bone[] Bones { get; }
		Tree[] Groups { get; }
		Material[] Materials { get; }
		Platform Platform { get; }

		string GetTextureName(int materialIndex, ExportOptions options);
	}
}
