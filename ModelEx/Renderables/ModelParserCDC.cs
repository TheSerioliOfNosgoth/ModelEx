using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using SRFile = CDC.Objects.SRFile;
using SRModel = CDC.Objects.Models.SRModel;
using Tree = CDC.Tree;

namespace ModelEx
{
	class SRModelParser :
		IModelParser
	{
		string _objectName;
		SRFile _srFile;
		SRModel _srModel;
		public Model Model;
		public string ModelName { get; private set; }
		public List<Material> Materials { get; } = new List<Material>();
		public List<Mesh> Meshes { get; } = new List<Mesh>();
		public List<SubMesh> SubMeshes { get; } = new List<SubMesh>();
		public List<Node> Groups { get; } = new List<Node>();

		public SRModelParser(string objectName, SRFile srFile)
		{
			_objectName = objectName;
			_srFile = srFile;
		}

		public void BuildModel(RenderResource resource, int modelIndex, CDC.Objects.ExportOptions options)
		{
			_srModel = _srFile.Models[modelIndex];
			String modelName = _objectName + "-" + modelIndex.ToString();

			#region Materials
			SceneCDC.ProgressStage = "Model " + modelIndex.ToString() + " - Creating Materials";
			Thread.Sleep(500);
			for (int materialIndex = 0; materialIndex < _srModel.MaterialCount; materialIndex++)
			{
				Material material = new Material();
				material.Visible = _srModel.Materials[materialIndex].visible;
				// Breaks early SR1 builds.
				//material.BlendMode = _srModel.Materials[materialIndex].blendMode;
				//int sortPush = unchecked((sbyte)_srModel.Materials[materialIndex].sortPush);
				//sortPush = 128 - sortPush;
				//material.DepthBias = (1.0f / 100000.0f) * sortPush;
				// Maybe use a hack for warpgates WARPGATE_DrawWarpGateRim indicates tree 3 should have lower priority.
				Color colorDiffuse = Color.FromArgb((int)unchecked(_srModel.Materials[materialIndex].colour));
				material.Diffuse = colorDiffuse;
				material.TextureFileName = CDC.Objects.Models.SRModel.GetTextureName(_srModel, materialIndex, options);
				Materials.Add(material);

				if (_srModel.Groups.Length > 0)
				{
					SceneCDC.progressLevel += _srModel.IndexCount / _srModel.Groups.Length;
				}
			}
			#endregion

			#region Groups
			for (int groupIndex = 0; groupIndex < _srModel.Groups.Length; groupIndex++)
			{
				SceneCDC.ProgressStage = "Model " + modelIndex.ToString() + " - Creating Group " + groupIndex.ToString();
				Thread.Sleep(100);

				Tree srGroup = _srModel.Groups[groupIndex];
				String groupName = String.Format("{0}-{1}-group-{2}", _objectName, modelIndex, groupIndex);
				if (srGroup != null && srGroup.mesh != null &&
					srGroup.mesh.indexCount > 0 && srGroup.mesh.polygonCount > 0)
				{
					Node group = new Node();
					SRMeshParser meshParser = new SRMeshParser(_objectName, _srFile);
					meshParser.BuildMesh(resource, modelIndex, groupIndex, 0);
					foreach (SubMesh subMesh in meshParser.SubMeshes)
					{
						// If the mesh parser knew the total submeshes for the model,
						// then this could be done inside BuildMesh.
						subMesh.MeshIndex = Meshes.Count;
						group.SubMeshIndices.Add(SubMeshes.Count);
						SubMeshes.Add(subMesh);
					}
					Meshes.Add(meshParser.Mesh);
					group.Name = groupName;
					Groups.Add(group);
				}
			}
			#endregion

			ModelName = modelName;
			Model = new Model(this);
		}
	}
}
