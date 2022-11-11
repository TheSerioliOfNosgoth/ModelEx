using System;
using System.Collections.Generic;
using System.Drawing;
using CDCFile = CDC.Objects.CDCFile;
using CDCModel = CDC.Objects.Models.Model;
using Tree = CDC.Tree;

namespace ModelEx
{
	class SRModelParser :
		IModelParser
	{
		string _objectName;
		CDCFile _cdcFile;
		CDCModel _cdcModel;
		public Model Model;
		public string ModelName { get; private set; }
		public List<Material> Materials { get; } = new List<Material>();
		public List<Mesh> Meshes { get; } = new List<Mesh>();
		public List<SubMesh> SubMeshes { get; } = new List<SubMesh>();
		public List<ModelNode> Groups { get; } = new List<ModelNode>();

		public SRModelParser(string objectName, CDCFile cdcFile)
		{
			_objectName = objectName;
			_cdcFile = cdcFile;
		}

		public void BuildModel(RenderResource resource, int modelIndex, CDC.Objects.ExportOptions options)
		{
			_cdcModel = _cdcFile.Models[modelIndex];
			String modelName = _objectName + "-" + modelIndex.ToString();

			#region Materials

			for (int materialIndex = 0; materialIndex < _cdcModel.MaterialCount; materialIndex++)
			{
				Material material = new Material();
				material.Visible = _cdcModel.Materials[materialIndex].visible;
				// Breaks early SR1 builds.
				//material.BlendMode = _srModel.Materials[materialIndex].blendMode;
				//int sortPush = unchecked((sbyte)_srModel.Materials[materialIndex].sortPush);
				//sortPush = 128 - sortPush;
				//material.DepthBias = (1.0f / 100000.0f) * sortPush;
				// Maybe use a hack for warpgates WARPGATE_DrawWarpGateRim indicates tree 3 should have lower priority.
				Color colorDiffuse = Color.FromArgb((int)unchecked(_cdcModel.Materials[materialIndex].colour));
				material.Diffuse = colorDiffuse;
				material.TextureFileName = CDC.Objects.Models.Model.GetTextureName((CDC.Objects.Models.Model)_cdcModel, materialIndex, options);
				Materials.Add(material);
			}

			#endregion

			#region Groups
			for (int groupIndex = 0; groupIndex < _cdcModel.Groups.Length; groupIndex++)
			{
				Tree srGroup = _cdcModel.Groups[groupIndex];
				String groupName = String.Format("{0}-{1}-group-{2}", _objectName, modelIndex, groupIndex);
				if (srGroup != null && srGroup.mesh != null &&
					srGroup.mesh.indexCount > 0 && srGroup.mesh.polygonCount > 0)
				{
					ModelNode group = new ModelNode();
					SRMeshParser meshParser = new SRMeshParser(_objectName, _cdcFile);
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
