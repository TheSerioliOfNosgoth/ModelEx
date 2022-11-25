using System;
using System.Collections.Generic;
using System.Drawing;
using Tree = CDC.Tree;

namespace ModelEx
{
	class SRModelParser :
		IModelParser
	{
		string _objectName;
		CDC.DataFile _dataFile;
		CDC.IModel _cdcModel;
		public Model Model;
		public string ModelName { get; private set; }
		public List<Material> Materials { get; } = new List<Material>();
		public List<Mesh> Meshes { get; } = new List<Mesh>();
		public List<SubMesh> SubMeshes { get; } = new List<SubMesh>();
		public List<ModelNode> Groups { get; } = new List<ModelNode>();

		public SRModelParser(string objectName, CDC.DataFile dataFile)
		{
			_objectName = objectName;
			_dataFile = dataFile;
		}

		public void BuildModel(RenderResource resource, int modelIndex, CDC.ExportOptions options)
		{
			_cdcModel = _dataFile.Models[modelIndex];
			string modelName = _cdcModel.Name;

			#region Materials

			for (int materialIndex = 0; materialIndex < _cdcModel.Materials.Length; materialIndex++)
			{
				Material material = new Material();
				material.Visible = _cdcModel.Materials[materialIndex].visible;
				// Breaks early SR1 builds.
				//material.BlendMode = _cdcModel.Materials[materialIndex].blendMode;
				//int sortPush = unchecked((sbyte)_cdcModel.Materials[materialIndex].sortPush);
				//sortPush = 128 - sortPush;
				//material.DepthBias = (1.0f / 100000.0f) * sortPush;
				// Maybe use a hack for warpgates WARPGATE_DrawWarpGateRim indicates tree 3 should have lower priority.
				if (_dataFile.Asset == CDC.Asset.Unit && modelIndex > 0 && modelIndex <= _dataFile.PortalCount)
				{
					material.BlendMode = 1;
				}

				Color colorDiffuse = Color.FromArgb((int)unchecked(_cdcModel.Materials[materialIndex].colour));
				material.Diffuse = colorDiffuse;
				material.TextureFileName = _cdcModel.GetTextureName(materialIndex, options);
				Materials.Add(material);
			}

			#endregion

			#region Groups
			for (int groupIndex = 0; groupIndex < _cdcModel.Groups.Length; groupIndex++)
			{
				Tree srGroup = _cdcModel.Groups[groupIndex];
				string groupName = modelName + "-group-" + groupIndex.ToString();
				if (srGroup != null && srGroup.mesh != null &&
					srGroup.mesh.indexCount > 0 && srGroup.mesh.polygonCount > 0)
				{
					ModelNode group = new ModelNode();
					SRMeshParser meshParser = new SRMeshParser(_objectName, _dataFile);
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

					SlimDX.Vector3 offset = new SlimDX.Vector3()
					{
						X = 0.01f * srGroup.globalOffset.x,
						Y = 0.01f * srGroup.globalOffset.z,
						Z = 0.01f * srGroup.globalOffset.y,
					};

					SlimDX.Matrix.Translation(ref offset, out SlimDX.Matrix transform);
					group.Transform = transform;

					Groups.Add(group);
				}
			}
			#endregion

			ModelName = modelName;
			Model = new Model(this);
		}
	}
}
