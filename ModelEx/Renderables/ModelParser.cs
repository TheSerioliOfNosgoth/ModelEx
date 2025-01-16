using System;
using System.Collections.Generic;
using System.Drawing;

namespace ModelEx
{
	class ModelParser :
		IModelParser
	{
		public Model Model;
		public string ModelName { get; private set; }
		public List<Material> Materials { get; } = new List<Material>();
		public List<Mesh> Meshes { get; } = new List<Mesh>();
		public List<SubMesh> SubMeshes { get; } = new List<SubMesh>();
		public List<ModelNode> Groups { get; } = new List<ModelNode>();

		public ModelParser(string modelName)
		{
			ModelName = modelName;
		}

		public void BuildModel(RenderResource resource, RenderResourceShapes.Shape shape)
		{
			Material materialA = new Material();
			materialA.Visible = true;
			Color colorDiffuseA = Color.FromArgb(unchecked((int)0xFF0000FF));
			materialA.Diffuse = colorDiffuseA;
			materialA.TextureFileName = "";
			Materials.Add(materialA);
			Material materialB = new Material();
			materialB.Visible = true;
			Color colorDiffuseB = Color.FromArgb(unchecked((int)0xFF00FF00));
			materialB.Diffuse = colorDiffuseB;
			materialB.TextureFileName = "";
			Materials.Add(materialB);
			Material materialC = new Material();
			materialC.Visible = true;
			Color colorDiffuseC = Color.FromArgb(unchecked((int)0xFFFF0000));
			materialC.Diffuse = colorDiffuseC;
			materialC.TextureFileName = "";
			Materials.Add(materialC);

			ModelNode group = new ModelNode();
			group.Name = "group";

			MeshParser meshParser = new MeshParser(ModelName);

			switch (shape)
			{
				case RenderResourceShapes.Shape.Cube:
					meshParser.BuildCube(resource);
					break;
				case RenderResourceShapes.Shape.Octahedron:
					meshParser.BuildOctahedron(resource);
					break;
				case RenderResourceShapes.Shape.Sphere:
					meshParser.BuildSphere(resource);
					break;
			}

			foreach (SubMesh subMesh in meshParser.SubMeshes)
			{
				// If the mesh parser knew the total submeshes for the model,
				// then this could be done inside BuildMesh.
				subMesh.MeshIndex = Meshes.Count;
				group.SubMeshIndices.Add(SubMeshes.Count);
				SubMeshes.Add(subMesh);
			}

			Meshes.Add(meshParser.Mesh);
			Groups.Add(group);
			Model = new Model(this);
		}
	}
}
