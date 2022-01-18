using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Text;
using GexFile = CDC.Objects.GexFile;
using SRFile = CDC.Objects.SRFile;
using SR1File = CDC.Objects.SR1File;
using SR2File = CDC.Objects.SR2File;
using DefianceFile = CDC.Objects.DefianceFile;
using TRLFile = CDC.Objects.TRLFile;
using SRModel = CDC.Objects.Models.SRModel;
using GexModel = CDC.Objects.Models.GexModel;
using SR1Model = CDC.Objects.Models.SR1Model;
using SR2Model = CDC.Objects.Models.SR2Model;
using DefianceModel = CDC.Objects.Models.DefianceModel;
using Tree = CDC.Tree;
using Gex3PSTextureFile = BenLincoln.TheLostWorlds.CDTextures.Gex3PlaystationVRMTextureFile;
using SR1PCTextureFile = BenLincoln.TheLostWorlds.CDTextures.SoulReaverPCTextureFile;
using SR1PSTextureFile = BenLincoln.TheLostWorlds.CDTextures.SoulReaverPlaystationTextureFile;
using SR1DCTextureFile = BenLincoln.TheLostWorlds.CDTextures.SoulReaverDreamcastTextureFile;
using SR2PCTextureFile = BenLincoln.TheLostWorlds.CDTextures.SoulReaver2PCVRMTextureFile;
using TRLPCTextureFile = BenLincoln.TheLostWorlds.CDTextures.TombRaiderPCDRMTextureFile;

using SpriteTextRenderer;

namespace ModelEx
{
	public class SceneCDC : Scene
	{
		public const string TextureExtension = ".png";

		class ModelParser :
			IModelParser
		{
			public Model Model;
			public string ModelName { get; private set; }
			public List<Material> Materials { get; } = new List<Material>();
			public List<Mesh> Meshes { get; } = new List<Mesh>();
			public List<SubMesh> SubMeshes { get; } = new List<SubMesh>();
			public List<Node> Groups { get; } = new List<Node>();

			public ModelParser(string modelName)
			{
				ModelName = modelName;
			}

			public void BuildModel()
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

				Node group = new Node();
				group.Name = "group";

				MeshParser meshParser = new MeshParser(ModelName);
				meshParser.BuildMesh();
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

		class MeshParser :
			IMeshParser<PositionColorTexturedVertex, short>
		{
			struct BasicVertex
			{
				public float X;
				public float Y;
				public float Z;
			}

			List<BasicVertex> _vertexList = new List<BasicVertex>();
			List<int> _indexList = new List<int>();
			public List<SubMesh> SubMeshes { get; } = new List<SubMesh>();
			public Mesh Mesh;
			public string MeshName { get; private set; }
			public string Technique { get; private set; }
			public int VertexCount { get { return _vertexList.Count; } }
			public int IndexCount { get { return _indexList.Count; } }

			public MeshParser(string meshName)
			{
				MeshName = meshName;
			}

			public void BuildMesh()
			{
				float v = 1.2f;
				float h = 1.0f;

				BasicVertex[] vertices =
				{
					new BasicVertex { X =  0, Y =  v, Z =  0 },
					new BasicVertex { X = -h, Y =  0, Z =  h },
					new BasicVertex { X = -h, Y =  0, Z = -h },
					new BasicVertex { X =  h, Y =  0, Z = -h },
					new BasicVertex { X =  h, Y =  0, Z =  h },
					new BasicVertex { X =  0, Y = -v, Z =  0 }
				};

				_vertexList.AddRange(vertices);

				int[] indices = {
					0, 1, 2,
					5, 3, 2,
					0, 3, 4,
					5, 1, 4,
					5, 2, 1,
					0, 2, 3,
					5, 4, 3,
					0, 4, 1
				};

				_indexList.AddRange(indices);

				Technique = "DefaultRender";

				Mesh = new MeshPCT(this);

				SubMesh subMeshA = new SubMesh
				{
					Name = MeshName + "-0",
					MaterialIndex = 0,
					indexCount = 12,
					startIndexLocation = 0,
					baseVertexLocation = 0
				};

				SubMeshes.Add(subMeshA);

				SubMesh subMeshB = new SubMesh
				{
					Name = MeshName + "-1",
					MaterialIndex = 1,
					indexCount = 12,
					startIndexLocation = 12,
					baseVertexLocation = 0
				};

				SubMeshes.Add(subMeshB);
			}

			public void FillVertex(int v, out PositionColorTexturedVertex vertex)
			{
				vertex.Position = new SlimDX.Vector3()
				{
					X = _vertexList[v].X,
					Y = _vertexList[v].Y,
					Z = _vertexList[v].Z
				};

				vertex.Color = new SlimDX.Color3()
				{
					//Alpha = 1.0f,
					Red = 1.0f,
					Green = 1.0f,
					Blue = 1.0f
				};

				vertex.TextureCoordinates = new SlimDX.Vector2()
				{
					X = 0.0f,
					Y = 0.0f
				};
			}

			public void FillIndex(int i, out short index)
			{
				index = (short)_indexList[i];
			}
		}

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

			public void BuildModel(int modelIndex, CDC.Objects.ExportOptions options)
			{
				_srModel = _srFile.Models[modelIndex];
				String modelName = _objectName + "-" + modelIndex.ToString();

				#region Materials
				ProgressStage = "Model " + modelIndex.ToString() + " - Creating Materials";
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
						progressLevel += _srModel.IndexCount / _srModel.Groups.Length;
					}
				}
				#endregion

				#region Groups
				for (int groupIndex = 0; groupIndex < _srModel.Groups.Length; groupIndex++)
				{
					ProgressStage = "Model " + modelIndex.ToString() + " - Creating Group " + groupIndex.ToString();
					Thread.Sleep(100);

					Tree srGroup = _srModel.Groups[groupIndex];
					String groupName = String.Format("{0}-{1}-group-{2}", _objectName, modelIndex, groupIndex);
					if (srGroup != null && srGroup.mesh != null &&
						srGroup.mesh.indexCount > 0 && srGroup.mesh.polygonCount > 0)
					{
						Node group = new Node();
						SRMeshParser meshParser = new SRMeshParser(_objectName, _srFile);
						meshParser.BuildMesh(modelIndex, groupIndex, 0);
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

		class SRMeshParser :
			IMeshParser<PositionNormalTexturedVertex, short>,
			IMeshParser<PositionColorTexturedVertex, short>,
			IMeshParser<Position2Color2TexturedVertex, short>
		{
			string _objectName;
			SRFile _srFile;
			SRModel _srModel;
			Tree _srGroup;
			List<int> _vertexList = new List<int>();
			List<int> _indexList = new List<int>();
			public List<SubMesh> SubMeshes { get; } = new List<SubMesh>();
			public Mesh Mesh;

			public SRMeshParser(string objectName, SRFile srFile)
			{
				_objectName = objectName;
				_srFile = srFile;
			}

			public void BuildMesh(int modelIndex, int groupIndex, int meshIndex)
			{
				_srModel = _srFile.Models[modelIndex];
				_srGroup = _srModel.Groups[groupIndex];
				String modelName = String.Format("{0}-{1}", _objectName, modelIndex);
				String groupName = String.Format("{0}-{1}-group-{2}", _objectName, modelIndex, groupIndex);
				String meshName = String.Format("{0}-{1}-group-{2}-mesh-{3}", _objectName, modelIndex, groupIndex, meshIndex);

				int startIndexLocation = 0;
				for (int materialIndex = 0; materialIndex < _srModel.MaterialCount; materialIndex++)
				{
					int indexCount = 0;
					int totalIndexCount = (int)_srGroup.mesh.indexCount;
					for (int v = 0; v < totalIndexCount; v++)
					{
						if (_srGroup.mesh.polygons[v / 3].material.ID == materialIndex)
						{
							_vertexList.Add(v);
							_indexList.Add(_indexList.Count - startIndexLocation);
							indexCount++;
						}
					}

					if (indexCount > 0)
					{
						String subMeshName = String.Format("{0}-{1}-group-{2}-submesh-{3}", _objectName, modelIndex, groupIndex, materialIndex);
						SubMesh subMesh = new SubMesh
						{
							Name = subMeshName,
							MaterialIndex = materialIndex,
							indexCount = indexCount,
							startIndexLocation = startIndexLocation,
							baseVertexLocation = startIndexLocation
						};
						SubMeshes.Add(subMesh);

						startIndexLocation += indexCount;
					}
				}

				if (SubMeshes.Count > 0)
				{
					MeshName = meshName;
					Technique = "DefaultRender";
					if (_srFile.Asset == CDC.Asset.Unit)
					{
						//Mesh = new MeshPCT(this);
						Mesh = new MeshMorphingUnit(this);
					}
					else
					{
						Mesh = new MeshPNT(this);
					}
				}
			}

			public string MeshName
			{
				get;
				private set;
			}

			public string Technique
			{
				get;
				private set;
			}

			public int VertexCount { get { return _vertexList.Count; } }

			public void FillVertex(int v, out PositionNormalTexturedVertex vertex)
			{
				ref CDC.Vertex exVertex = ref _srGroup.mesh.vertices[_vertexList[v]];
				CDC.Geometry exGeometry = exVertex.isExtraGeometry ? _srModel.ExtraGeometry : _srModel.Geometry;

				vertex.Position = new SlimDX.Vector3()
				{
					X = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].x,
					Y = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].z,
					Z = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].y
				};

				vertex.Normal = new SlimDX.Vector3()
				{
					X = exGeometry.Normals[exVertex.normalID].x,
					Y = exGeometry.Normals[exVertex.normalID].z,
					Z = exGeometry.Normals[exVertex.normalID].y
				};
				vertex.Normal.Normalize();

				vertex.TextureCoordinates = new SlimDX.Vector2()
				{
					X = exGeometry.UVs[exVertex.UVID].u,
					Y = exGeometry.UVs[exVertex.UVID].v
				};
			}

			public void FillVertex(int v, out PositionColorTexturedVertex vertex)
			{
				ref CDC.Vertex exVertex = ref _srGroup.mesh.vertices[_vertexList[v]];
				CDC.Geometry exGeometry = exVertex.isExtraGeometry ? _srModel.ExtraGeometry : _srModel.Geometry;

				vertex.Position = new SlimDX.Vector3()
				{
					X = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].x,
					Y = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].z,
					Z = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].y
				};

				vertex.Color = new SlimDX.Color3()
				{
					//Alpha = ((_srModel.Colours[vertex.colourID] & 0xFF000000) >> 24) / 255.0f,
					Red = ((exGeometry.Colours[exVertex.colourID] & 0x00FF0000) >> 16) / 255.0f,
					Green = ((exGeometry.Colours[exVertex.colourID] & 0x0000FF00) >> 8) / 255.0f,
					Blue = ((exGeometry.Colours[exVertex.colourID] & 0x000000FF) >> 0) / 255.0f
				};

				vertex.TextureCoordinates = new SlimDX.Vector2()
				{
					X = exGeometry.UVs[exVertex.UVID].u,
					Y = exGeometry.UVs[exVertex.UVID].v
				};
			}

			public void FillVertex(int v, out Position2Color2TexturedVertex vertex)
			{
				ref CDC.Vertex exVertex = ref _srGroup.mesh.vertices[_vertexList[v]];
				CDC.Geometry exGeometry = exVertex.isExtraGeometry ? _srModel.ExtraGeometry : _srModel.Geometry;

				vertex.Position0 = new SlimDX.Vector3()
				{
					X = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].x,
					Y = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].z,
					Z = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].y
				};

				vertex.Position1 = new SlimDX.Vector3()
				{
					X = 0.01f * exGeometry.PositionsAltPhys[exVertex.positionID].x,
					Y = 0.01f * exGeometry.PositionsAltPhys[exVertex.positionID].z,
					Z = 0.01f * exGeometry.PositionsAltPhys[exVertex.positionID].y
				};

				vertex.Color0 = new SlimDX.Color3()
				{
					//Alpha = ((_srModel.VertexData.Colours[vertex.colourID] & 0xFF000000) >> 24) / 255.0f,
					Red = ((exGeometry.Colours[exVertex.colourID] & 0x00FF0000) >> 16) / 255.0f,
					Green = ((exGeometry.Colours[exVertex.colourID] & 0x0000FF00) >> 8) / 255.0f,
					Blue = ((exGeometry.Colours[exVertex.colourID] & 0x000000FF) >> 0) / 255.0f
				};

				vertex.Color1 = new SlimDX.Color3()
				{
					//Alpha = ((_srModel.ColoursAlt[vertex.colourID] & 0xFF000000) >> 24) / 255.0f,
					Red = ((exGeometry.ColoursAlt[exVertex.colourID] & 0x00FF0000) >> 16) / 255.0f,
					Green = ((exGeometry.ColoursAlt[exVertex.colourID] & 0x0000FF00) >> 8) / 255.0f,
					Blue = ((exGeometry.ColoursAlt[exVertex.colourID] & 0x000000FF) >> 0) / 255.0f
				};

				vertex.TextureCoordinates = new SlimDX.Vector2()
				{
					X = exGeometry.UVs[exVertex.UVID].u,
					Y = exGeometry.UVs[exVertex.UVID].v
				};
			}

			public int IndexCount { get { return _vertexList.Count; } }

			public void FillIndex(int i, out short index)
			{
				index = (short)_indexList[i];
			}
		}

		private static long progressLevels = 0;
		private static long progressLevel = 0;

		public static int ProgressPercent
		{
			get
			{
				if (progressLevels <= 0 || progressLevel <= 0)
				{
					return 0;
				}

				return (int)((100 * progressLevel) / progressLevels);
			}
		}

		public static string ProgressStage { get; private set; } = "Done";

		CDC.Game _game = CDC.Game.SR1;
		List<SRFile> _objectFiles = new List<SRFile>();

		SpriteRenderer _spriteRenderer;
		TextBlockRenderer _textBlockRenderer;

		public SceneCDC(CDC.Game game)
			: base()
		{
			_game = game;
			_spriteRenderer = new SpriteRenderer();
			_textBlockRenderer = new TextBlockRenderer(_spriteRenderer, "Arial", SlimDX.DirectWrite.FontWeight.Bold, SlimDX.DirectWrite.FontStyle.Normal, SlimDX.DirectWrite.FontStretch.Normal, 16);
		}

		public override void Dispose()
		{
			base.Dispose();

			_textBlockRenderer.Dispose();
			_spriteRenderer.Dispose();
		}

		public override void Render()
		{
			_spriteRenderer.RefreshViewport();

			SlimDX.Direct3D11.DepthStencilState oldDSState = DeviceManager.Instance.context.OutputMerger.DepthStencilState;
			SlimDX.Direct3D11.BlendState oldBlendState = DeviceManager.Instance.context.OutputMerger.BlendState;
			SlimDX.Direct3D11.RasterizerState oldRasterizerState = DeviceManager.Instance.context.Rasterizer.State;
			SlimDX.Direct3D11.VertexShader oldVertexShader = DeviceManager.Instance.context.VertexShader.Get();
			SlimDX.Direct3D11.Buffer[] oldVSCBuffers = DeviceManager.Instance.context.VertexShader.GetConstantBuffers(0, 10);
			SlimDX.Direct3D11.PixelShader oldPixelShader = DeviceManager.Instance.context.PixelShader.Get();
			SlimDX.Direct3D11.Buffer[] oldPSCBuffers = DeviceManager.Instance.context.PixelShader.GetConstantBuffers(0, 10);
			SlimDX.Direct3D11.ShaderResourceView[] oldShaderResources = DeviceManager.Instance.context.PixelShader.GetShaderResources(0, 10);
			SlimDX.Direct3D11.GeometryShader oldGeometryShader = DeviceManager.Instance.context.GeometryShader.Get();

			base.Render();

			DeviceManager.Instance.context.OutputMerger.DepthStencilState = oldDSState;
			DeviceManager.Instance.context.OutputMerger.BlendState = oldBlendState;
			DeviceManager.Instance.context.Rasterizer.State = oldRasterizerState;
			DeviceManager.Instance.context.VertexShader.Set(oldVertexShader);
			DeviceManager.Instance.context.VertexShader.SetConstantBuffers(oldVSCBuffers, 0, 10);
			DeviceManager.Instance.context.PixelShader.Set(oldPixelShader);
			DeviceManager.Instance.context.PixelShader.SetConstantBuffers(oldPSCBuffers, 0, 10);
			DeviceManager.Instance.context.PixelShader.SetShaderResources(oldShaderResources, 0, 10);
			DeviceManager.Instance.context.GeometryShader.Set(oldGeometryShader);

			System.Threading.Monitor.Enter(DeviceManager.Instance.device);
			foreach (RenderInstance instance in renderInstances)
			{
				if (instance.Name == null)
				{
					continue;
				}

				SlimDX.Matrix world = instance.Transform;
				SlimDX.Matrix view = CameraManager.Instance.frameCamera.View;
				SlimDX.Matrix projection = CameraManager.Instance.frameCamera.Perspective;

				//SlimDX.Matrix viewProj = view * projection;
				SlimDX.Matrix worldViewProjection = world * view * projection;
				SlimDX.Direct3D11.Viewport vp = DeviceManager.Instance.context.Rasterizer.GetViewports()[0];
				SlimDX.Vector3 position3D = SlimDX.Vector3.Project(SlimDX.Vector3.Zero, vp.X, vp.Y, vp.Width, vp.Height, vp.MinZ, vp.MaxZ, worldViewProjection);
				SlimDX.Vector2 position2D = new SlimDX.Vector2(position3D.X, position3D.Y);

				if (position3D.Z < vp.MaxZ)
				{
					SlimDX.Vector3 objPos = SlimDX.Vector3.Zero;
					objPos = SlimDX.Vector3.TransformCoordinate(objPos, instance.Transform);
					SlimDX.Vector3 camPos = CameraManager.Instance.frameCamera.eye;
					SlimDX.Vector3 objOffset = objPos - camPos;
					float scale = Math.Min(2.0f, 5.0f / (float)Math.Sqrt(Math.Max(1.0f, objOffset.Length())));

					_textBlockRenderer.DrawString(instance.Name, position2D, 16 * scale, new SlimDX.Color4(1.0f, 1.0f, 1.0f), CoordinateType.Absolute);
				}
			}
			_spriteRenderer.Flush();
			System.Threading.Monitor.Exit(DeviceManager.Instance.device);

			DeviceManager.Instance.context.OutputMerger.DepthStencilState = oldDSState;
			DeviceManager.Instance.context.OutputMerger.BlendState = oldBlendState;
			DeviceManager.Instance.context.Rasterizer.State = oldRasterizerState;
			DeviceManager.Instance.context.VertexShader.Set(oldVertexShader);
			DeviceManager.Instance.context.VertexShader.SetConstantBuffers(oldVSCBuffers, 0, 10);
			DeviceManager.Instance.context.PixelShader.Set(oldPixelShader);
			DeviceManager.Instance.context.PixelShader.SetConstantBuffers(oldPSCBuffers, 0, 10);
			DeviceManager.Instance.context.PixelShader.SetShaderResources(oldShaderResources, 0, 10);
			DeviceManager.Instance.context.GeometryShader.Set(oldGeometryShader);
		}

		//public static string GetTextureNameDefault(string objectName, int textureID)
		//{
		//    String textureName = string.Format("{0}_{1:X4}", objectName.TrimEnd(new char[] { '_' }).ToLower(), textureID);
		//    return textureName;
		//}

		//public static string GetPlayStationTextureNameDefault(string objectName, int textureID)
		//{
		//    return GetTextureNameDefault(objectName, textureID);
		//}

		//public static string GetPlayStationTextureNameWithCLUT(string objectName, int textureID, ushort clut)
		//{
		//    String textureName = string.Format("{0}_{1:X4}_{2:X4}", objectName.TrimEnd(new char[] { '_' }).ToLower(), textureID, clut);
		//    return textureName;
		//}

		//public static string GetSoulReaverPCOrDreamcastTextureName(string objectName, int textureID)
		//{
		//    return GetTextureNameDefault(objectName, textureID);
		//}

		//public static string GetPS2TextureName(string objectName, int textureID)
		//{
		//    return GetTextureNameDefault(objectName, textureID);
		//}

		//protected static String GetTextureName(SRModel srModel, int materialIndex, CDC.Objects.ExportOptions options)
		//{
		//    CDC.Material material = srModel.Materials[materialIndex];
		//    String textureName = "";
		//    if (material.textureUsed)
		//    {
		//        if (srModel is SR1Model)
		//        {
		//            if (srModel.Platform == CDC.Platform.PSX)
		//            {
		//                if (options.UseEachUniqueTextureCLUTVariation)
		//                {
		//                    textureName = GetPlayStationTextureNameWithCLUT(srModel.Name, material.textureID, material.clutValue);
		//                }
		//                else
		//                {
		//                    textureName = GetPlayStationTextureNameDefault(srModel.Name, material.textureID);
		//                }
		//            }
		//            else
		//            {
		//                textureName = GetSoulReaverPCOrDreamcastTextureName(srModel.Name, material.textureID);
		//            }
		//        }
		//        else if (srModel is SR2Model ||
		//            srModel is DefianceModel)
		//        {
		//            textureName = GetPS2TextureName(srModel.Name, material.textureID);
		//        }
		//    }

		//    return textureName;
		//}

		protected static bool BitmapHasTransparentPixels(Bitmap b)
		{
			for (int y = 0; y < b.Height; y++)
			{
				for (int x = 0; x < b.Width; x++)
				{
					Color c = b.GetPixel(x, y);
					if (c.A == 0)
					{
						return true;
					}
				}
			}

			return false;
		}

		protected string GetTextureFileLocation(CDC.Objects.ExportOptions options, string defaultTextureFileName, string modelFileName)
		{
			string result = "";
			List<string> possibleLocations = new List<string>();
			for (int i = 0; i < options.TextureFileLocations.Count; i++)
			{
				possibleLocations.Add(options.TextureFileLocations[i]);
			}

			List<string> searchDirectories = new List<string>();

			string rootDirectory = Path.GetDirectoryName(modelFileName);
			while (rootDirectory != null && rootDirectory != "")
			{
				string parentDirectory = Path.GetFileName(rootDirectory);
				rootDirectory = Path.GetDirectoryName(rootDirectory);
				if (parentDirectory == "kain2")
				{
					string outputDirectory = Path.Combine(rootDirectory, "output");
					searchDirectories.Add(outputDirectory);
					searchDirectories.Add(rootDirectory);
				}
			}

			searchDirectories.Add(Path.GetDirectoryName(modelFileName));
			searchDirectories.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

			for (int i = 0; i < searchDirectories.Count; i++)
			{
				string textureFileName = Path.Combine(searchDirectories[i], defaultTextureFileName);
				possibleLocations.Add(textureFileName);
			}

			for (int i = 0; i < possibleLocations.Count; i++)
			{
				if (File.Exists(possibleLocations[i]))
				{
					result = possibleLocations[i];
					Console.WriteLine(string.Format("Debug: using texture file '{0}'", result));
					break;
				}
			}
			return result;
		}

		public override void ImportFromFile(string fileName, CDC.Objects.ExportOptions options)
		{
			ImportFromFile(fileName, options, -1);
		}

		public void ImportFromFile(string fileName, CDC.Objects.ExportOptions options, int childIndex)
		{
			progressLevel = 0;
			progressLevels = 0;
			ProgressStage = "Reading file";

			SRFile srFile = null;

			#region TryLoad
			if (_game == CDC.Game.Gex)
			{
				try
				{
					GexFile gexFile = new GexFile(fileName, options);
					if (gexFile.Asset == CDC.Asset.Unit && childIndex >= 0)
					{
						srFile = gexFile.Objects[childIndex];
					}
					else
					{
						srFile = gexFile;
					}
				}
				catch
				{
					return;
				}
			}
			else if (_game == CDC.Game.SR1)
			{
				try
				{
					srFile = new SR1File(fileName, options);
				}
				catch
				{
					return;
				}
			}
			else if (_game == CDC.Game.SR2)
			{
				try
				{
					srFile = new SR2File(fileName, options);
				}
				catch
				{
					return;
				}
			}
			else if (_game == CDC.Game.Defiance)
			{
				try
				{
					srFile = new DefianceFile(fileName, options);
				}
				catch
				{
					return;
				}
			}
			else
			{
				try
				{
					srFile = new TRLFile(fileName, options);
				}
				catch
				{
					return;
				}
			}
			#endregion

			#region Progress Levels
			for (int modelIndex = 0; modelIndex < srFile.Models.Length; modelIndex++)
			{
				SRModel srModel = srFile.Models[modelIndex];

				for (int materialIndex = 0; materialIndex < srModel.MaterialCount; materialIndex++)
				{
					progressLevels += 1;
				}

				for (int groupIndex = 0; groupIndex < srModel.Groups.Length; groupIndex++)
				{
					if (srModel.Groups[groupIndex] != null && srModel.Groups[groupIndex].mesh != null)
					{
						for (int materialIndex = 0; materialIndex < srModel.MaterialCount; materialIndex++)
						{
							int vertexCount = (int)srModel.Groups[groupIndex].mesh.indexCount;
							progressLevels += 1;
						}
					}
				}
			}
			#endregion

			for (int modelIndex = 0; modelIndex < srFile.Models.Length; modelIndex++)
			{
				SRModelParser modelParser = new SRModelParser(srFile.Name, srFile);
				modelParser.BuildModel(modelIndex, options);
				if (srFile.Asset == CDC.Asset.Unit)
				{
					Physical physical = new Physical(modelParser.Model);
					renderables.Add(physical.Model);
					renderInstances.Add(physical);
				}
				else
				{
					Physical physical = new Physical(modelParser.Model);
					renderables.Add(physical.Model);
					renderInstances.Add(physical);
				}
			}

			// Use Octahedron to show positions of stuff.
			ModelParser octaParser = new ModelParser("octahedron");
			octaParser.BuildModel();
			renderables.Add(octaParser.Model);

			if (srFile.Asset == CDC.Asset.Unit && srFile.IntroCount > 0)
			{
				foreach (CDC.Intro intro in srFile.Intros)
				{
					Marker marker = new Marker(octaParser.Model);
					float height = marker.GetBoundingSphere().Radius;
					marker.Name = intro.name;
					marker.Transform = SlimDX.Matrix.Translation(
						0.01f * intro.position.x,
						0.01f * intro.position.z + height,
						0.01f * intro.position.y
					);
					renderInstances.Add(marker);
				}
			}

			_objectFiles.Add(srFile);

			if (options.TextureLoadRequired())
			{
				ProgressStage = "Loading Textures";
				// reset the instance so that textures with identical names are not retained from previous models
				TextureManager.Instance.ResetInstance();
				//Thread.Sleep(1000);
				#region Textures
				Type currentFileType = srFile.GetType();
				if (currentFileType == typeof(TRLFile))
				{
					String textureFileName = fileName;
					try
					{
						TRLPCTextureFile textureFile = new TRLPCTextureFile(textureFileName);
						for (int t = 0; t < textureFile.TextureCount; t++)
						{
							String textureName = CDC.Objects.Models.SRModel.GetPS2TextureName(srFile.Name, (int)textureFile.TextureDefinitions[t].ID) + TextureExtension;

							System.IO.MemoryStream stream = textureFile.GetDataAsStream(t);
							//textureFile.ExportFile(t, "C:\\Users\\A\\Desktop\\Lara\\" + textureName);
							if (stream != null)
							{
								if (textureFile.TextureDefinitions[t].Format == BenLincoln.TheLostWorlds.CDTextures.DRMFormat.Uncompressed)
								{
									MemoryStream stream2 = textureFile.GetUncompressedDataAsStream2(t);
									TextureManager.Instance.AddTexture(stream2, textureName);
								}
								else
								{
									TextureManager.Instance.AddTexture(stream, textureName);
								}
								_TexturesAsPNGs.Add(textureName, textureFile.GetTextureAsBitmap(t));
							}
						}
					}
					catch (Exception ex)
					{
						Console.Write(ex.ToString());
					}
				}
				else if (currentFileType == typeof(SR2File) ||
						 currentFileType == typeof(DefianceFile))
				{
					String textureFileName = System.IO.Path.ChangeExtension(fileName, "vrm");
					try
					{
						SR2PCTextureFile textureFile = new SR2PCTextureFile(textureFileName);
						for (int t = 0; t < textureFile.TextureCount; t++)
						{
							String textureName = CDC.Objects.Models.SRModel.GetPS2TextureName(srFile.Name, textureFile.TextureDefinitions[t].Flags1) + TextureExtension;

							System.IO.MemoryStream stream = textureFile.GetDataAsStream(t);
							//textureFile.ExportFile(t, "C:\\Users\\A\\Desktop\\" + textureName);
							if (stream != null)
							{
								TextureManager.Instance.AddTexture(stream, textureName);
								_TexturesAsPNGs.Add(textureName, textureFile.GetTextureAsBitmap(t));
							}
						}
					}
					catch (Exception ex)
					{
						Console.Write(ex.ToString());
					}
				}
				else if (currentFileType == typeof(SR1File))
				{
					if (srFile.Platform == CDC.Platform.PC)
					{
						String textureFileName = GetTextureFileLocation(options, "textures.big", fileName);
						bool gotTextureFile = false;
						if (textureFileName != "")
						{
							gotTextureFile = true;
						}
						if (gotTextureFile)
						{
							try
							{
								SR1PCTextureFile textureFile = new SR1PCTextureFile(textureFileName);
								foreach (SRModel srModel in srFile.Models)
								{
									foreach (CDC.Material material in srModel.Materials)
									{
										if (material.textureUsed)
										{
											System.IO.MemoryStream stream = textureFile.GetDataAsStream(material.textureID);
											if (stream != null)
											{
												String textureName = CDC.Objects.Models.SRModel.GetSoulReaverPCOrDreamcastTextureName(srModel.Name, material.textureID) + TextureExtension;
												TextureManager.Instance.AddTexture(stream, textureName);
												if (!_TexturesAsPNGs.ContainsKey(textureName))
												{
													_TexturesAsPNGs.Add(textureName, textureFile.GetTextureAsBitmap(material.textureID));
												}
											}
										}
									}
								}
							}
							catch (Exception ex)
							{
								Console.Write(ex.ToString());
							}
						}
						else
						{
							Console.WriteLine("Error: couldn't find a texture file");
						}
					}
					else if (srFile.Platform == CDC.Platform.Dreamcast)
					{
						String textureFileName = GetTextureFileLocation(options, "textures.vq", fileName);
						bool gotTextureFile = false;
						if (textureFileName != "")
						{
							gotTextureFile = true;
						}
						if (gotTextureFile)
						{
							try
							{
								SR1DCTextureFile textureFile = new SR1DCTextureFile(textureFileName);
								foreach (SRModel srModel in srFile.Models)
								{
									foreach (CDC.Material material in srModel.Materials)
									{
										if (material.textureUsed)
										{
											int textureID = material.textureID;
											System.IO.MemoryStream stream = textureFile.GetDataAsStream(textureID);
											if (stream != null)
											{
												String textureName = CDC.Objects.Models.SRModel.GetSoulReaverPCOrDreamcastTextureName(srModel.Name, material.textureID) + TextureExtension;

												TextureManager.Instance.AddTexture(stream, textureName);

												if (!_TexturesAsPNGs.ContainsKey(textureName))
												{
													_TexturesAsPNGs.Add(textureName, textureFile.GetTextureAsBitmap(material.textureID));
												}
											}
										}
									}
								}
							}
							catch (Exception ex)
							{
								Console.Write(ex.ToString());
							}
						}
						else
						{
							Console.WriteLine("Error: couldn't find a texture file");
						}
					}
					else
					{
						String textureFileName = System.IO.Path.ChangeExtension(fileName, "crm");
						try
						{
							SR1PSTextureFile textureFile = new SR1PSTextureFile(textureFileName);

							UInt32 polygonCountAllModels = 0;
							foreach (SRModel srModel in srFile.Models)
							{
								polygonCountAllModels += srModel.PolygonCount;
							}

							SR1PSTextureFile.SoulReaverPlaystationPolygonTextureData[] polygons =
								new SR1PSTextureFile.SoulReaverPlaystationPolygonTextureData[polygonCountAllModels];

							int polygonNum = 0;
							foreach (SRModel srModel in srFile.Models)
							{
								foreach (CDC.Polygon polygon in srModel.Polygons)
								{
									polygons[polygonNum].paletteColumn = polygon.paletteColumn;
									polygons[polygonNum].paletteRow = polygon.paletteRow;
									polygons[polygonNum].u = new int[3];
									polygons[polygonNum].v = new int[3];
									//polygons[polygonNum].materialColour = polygon.material.colour;
									polygons[polygonNum].materialColour = polygon.colour;

									polygons[polygonNum].u[0] = (int)(srModel.Geometry.UVs[polygon.v1.UVID].u * 255);
									polygons[polygonNum].v[0] = (int)(srModel.Geometry.UVs[polygon.v1.UVID].v * 255);
									polygons[polygonNum].u[1] = (int)(srModel.Geometry.UVs[polygon.v2.UVID].u * 255);
									polygons[polygonNum].v[1] = (int)(srModel.Geometry.UVs[polygon.v2.UVID].v * 255);
									polygons[polygonNum].u[2] = (int)(srModel.Geometry.UVs[polygon.v3.UVID].u * 255);
									polygons[polygonNum].v[2] = (int)(srModel.Geometry.UVs[polygon.v3.UVID].v * 255);

									polygons[polygonNum].textureID = polygon.material.textureID;
									polygons[polygonNum].CLUT = polygon.material.clutValue;

									polygons[polygonNum].textureUsed = polygon.material.textureUsed;
									polygons[polygonNum].visible = polygon.material.visible;
									//polygons[polygonNum].materialColour = polygon.material.colour;

									polygonNum++;
								}
							}
							bool drawGreyscaleFirst = false;
							bool quantizeBounds = true;
							textureFile.BuildTexturesFromPolygonData(polygons, drawGreyscaleFirst, quantizeBounds, options);

							// For all models
							for (int t = 0; t < textureFile.TextureCount; t++)
							{
								String textureName = CDC.Objects.Models.SRModel.GetPlayStationTextureNameDefault(srFile.Name, t) + TextureExtension;

								System.IO.MemoryStream stream = textureFile.GetDataAsStream(t);
								if (stream != null)
								{
									TextureManager.Instance.AddTexture(stream, textureName);
								}
								//string exportedTextureFileName = Path.ChangeExtension(textureName, "png");
								//_TexturesAsPNGs.Add(exportedTextureFileName, textureFile.GetTextureAsBitmap(t));
								Bitmap b = textureFile.GetTextureAsBitmap(t);
								// this is a hack that's being done here for now because I don't know for sure which of the flags/attributes controls
								// textures that should be alpha-masked. Alpha-masking EVERY texture is expensive.
								if (options.AlsoInferAlphaMaskingFromTexturePixels)
								{
									if (BitmapHasTransparentPixels(b))
									{
										for (int modelNum = 0; modelNum < srFile.Models.Length; modelNum++)
										{
											if (t < srFile.Models[modelNum].Materials.Length)
											{
												srFile.Models[modelNum].Materials[t].UseAlphaMask = true;
											}
										}
									}
								}
								_TexturesAsPNGs.Add(textureName, b);

								// dump all textures as PNGs for debugging
								//Bitmap exportedTexture = textureFile.GetTextureAsBitmap(t);
								//string exportedTextureFileName = Path.ChangeExtension(textureName, "png");
								//exportedTexture.Save(exportedTextureFileName, ImageFormat.Png);
								//texNum = 0;
								//foreach (Bitmap tex in _Textures)
								//{
								//    tex.Save(@"C:\Debug\Tex-" + texNum + ".png", ImageFormat.Png);
								//    texNum++;
								//}
							}

							// for models that use index/CLUT textures, if the user has enabled this option
							if (options.UseEachUniqueTextureCLUTVariation)
							{
								foreach (int textureID in textureFile.TexturesByCLUT.Keys)
								{
									Dictionary<ushort, Bitmap> textureCLUTCollection = textureFile.TexturesByCLUT[textureID];
									foreach (ushort clut in textureCLUTCollection.Keys)
									{
										String textureName = CDC.Objects.Models.SRModel.GetPlayStationTextureNameWithCLUT(srFile.Name, textureID, clut) + TextureExtension;
										System.IO.MemoryStream stream = textureFile.GetTextureWithCLUTAsStream(textureID, clut);
										if (stream != null)
										{
											TextureManager.Instance.AddTexture(stream, textureName);
										}
										Bitmap b = textureFile.TexturesByCLUT[textureID][clut];
										_TexturesAsPNGs.Add(textureName, b);
									}
								}
							}
						}
						catch (Exception ex)
						{
							Console.Write(ex.ToString());
						}
					}
				}
				else
				{
					String textureFileName = System.IO.Path.ChangeExtension(fileName, "vrm");
					try
					{
						Gex3PSTextureFile textureFile = new Gex3PSTextureFile(textureFileName);

						UInt32 polygonCountAllModels = 0;
						foreach (SRModel srModel in srFile.Models)
						{
							polygonCountAllModels += srModel.PolygonCount;
						}

						Gex3PSTextureFile.Gex3PlaystationPolygonTextureData[] polygons =
							new Gex3PSTextureFile.Gex3PlaystationPolygonTextureData[polygonCountAllModels];

						int polygonNum = 0;
						foreach (SRModel srModel in srFile.Models)
						{
							foreach (CDC.Polygon polygon in srModel.Polygons)
							{
								polygons[polygonNum].paletteColumn = polygon.paletteColumn;
								polygons[polygonNum].paletteRow = polygon.paletteRow;
								polygons[polygonNum].u = new int[3];
								polygons[polygonNum].v = new int[3];
								//polygons[polygonNum].materialColour = polygon.material.colour;
								polygons[polygonNum].materialColour = polygon.colour;

								polygons[polygonNum].u[0] = (int)(srModel.Geometry.UVs[polygon.v1.UVID].u * 255);
								polygons[polygonNum].v[0] = (int)(srModel.Geometry.UVs[polygon.v1.UVID].v * 255);
								polygons[polygonNum].u[1] = (int)(srModel.Geometry.UVs[polygon.v2.UVID].u * 255);
								polygons[polygonNum].v[1] = (int)(srModel.Geometry.UVs[polygon.v2.UVID].v * 255);
								polygons[polygonNum].u[2] = (int)(srModel.Geometry.UVs[polygon.v3.UVID].u * 255);
								polygons[polygonNum].v[2] = (int)(srModel.Geometry.UVs[polygon.v3.UVID].v * 255);

								polygons[polygonNum].textureID = polygon.material.textureID;
								polygons[polygonNum].CLUT = polygon.material.clutValue;

								polygons[polygonNum].textureUsed = polygon.material.textureUsed;
								polygons[polygonNum].visible = polygon.material.visible;
								//polygons[polygonNum].materialColour = polygon.material.colour;

								polygonNum++;
							}
						}
						bool drawGreyscaleFirst = false;
						bool quantizeBounds = true;
						textureFile.BuildTexturesFromPolygonData(polygons, ((GexFile)srFile).TPages, drawGreyscaleFirst, quantizeBounds, options);

						// For all models
						for (int t = 0; t < textureFile.TextureCount; t++)
						{
							String textureName = CDC.Objects.Models.SRModel.GetPlayStationTextureNameDefault(srFile.Name, t) + TextureExtension;

							System.IO.MemoryStream stream = textureFile.GetDataAsStream(t);
							if (stream != null)
							{
								TextureManager.Instance.AddTexture(stream, textureName);
							}
							//string exportedTextureFileName = Path.ChangeExtension(textureName, "png");
							//_TexturesAsPNGs.Add(exportedTextureFileName, textureFile.GetTextureAsBitmap(t));
							Bitmap b = textureFile.GetTextureAsBitmap(t);
							// this is a hack that's being done here for now because I don't know for sure which of the flags/attributes controls
							// textures that should be alpha-masked. Alpha-masking EVERY texture is expensive.
							if (options.AlsoInferAlphaMaskingFromTexturePixels)
							{
								if (BitmapHasTransparentPixels(b))
								{
									for (int modelNum = 0; modelNum < srFile.Models.Length; modelNum++)
									{
										if (t < srFile.Models[modelNum].Materials.Length)
										{
											srFile.Models[modelNum].Materials[t].UseAlphaMask = true;
										}
									}
								}
							}
							_TexturesAsPNGs.Add(textureName, b);

							// dump all textures as PNGs for debugging
							//Bitmap exportedTexture = textureFile.GetTextureAsBitmap(t);
							//string exportedTextureFileName = Path.ChangeExtension(textureName, "png");
							//exportedTexture.Save(exportedTextureFileName, ImageFormat.Png);
							//texNum = 0;
							//foreach (Bitmap tex in _Textures)
							//{
							//    tex.Save(@"C:\Debug\Tex-" + texNum + ".png", ImageFormat.Png);
							//    texNum++;
							//}
						}

						// for models that use index/CLUT textures, if the user has enabled this option
						if (options.UseEachUniqueTextureCLUTVariation)
						{
							foreach (int textureID in textureFile.TexturesByCLUT.Keys)
							{
								Dictionary<ushort, Bitmap> textureCLUTCollection = textureFile.TexturesByCLUT[textureID];
								foreach (ushort clut in textureCLUTCollection.Keys)
								{
									String textureName = CDC.Objects.Models.SRModel.GetPlayStationTextureNameWithCLUT(srFile.Name, textureID, clut) + TextureExtension;
									System.IO.MemoryStream stream = textureFile.GetTextureWithCLUTAsStream(textureID, clut);
									if (stream != null)
									{
										TextureManager.Instance.AddTexture(stream, textureName);
									}
									Bitmap b = textureFile.TexturesByCLUT[textureID][clut];
									_TexturesAsPNGs.Add(textureName, b);
								}
							}
						}
					}
					catch (Exception ex)
					{
						Console.Write(ex.ToString());
					}
				}
				#endregion
			}

			progressLevel = progressLevels;
			ProgressStage = "Done";
			//Thread.Sleep(1000);
		}

		// make sure that overwriting exported files isn't silently failing
		protected void DeleteExistingFile(string path)
		{
			if (File.Exists(path))
			{
				try
				{
					File.Delete(path);
				}
				catch (Exception ex)
				{
					Console.WriteLine(string.Format("Couldn't delete existing file '{0}': {1}", path, ex.Message));
				}
			}
		}

		public override void ExportToFile(string fileName, CDC.Objects.ExportOptions options)
		{
			if ((_objectFiles != null) && (_objectFiles.Count > 0) && (_objectFiles[0] != null))
			{
				string filePath = Path.GetFullPath(fileName);
				DeleteExistingFile(filePath);
				_objectFiles[0].ExportToFile(fileName, options);
				string baseExportDirectory = Path.GetDirectoryName(fileName);
				foreach (string textureFileName in _TexturesAsPNGs.Keys)
				{
					string texturePath = Path.Combine(baseExportDirectory, textureFileName);
					DeleteExistingFile(texturePath);
					_TexturesAsPNGs[textureFileName].Save(texturePath, ImageFormat.Png);
				}
			}
		}
	}
}
