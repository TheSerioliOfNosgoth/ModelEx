using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using GexFile = CDC.Objects.GexFile;
using SRFile = CDC.Objects.SRFile;
using SR1File = CDC.Objects.SR1File;
using SR2File = CDC.Objects.SR2File;
using DefianceFile = CDC.Objects.DefianceFile;
using TRLFile = CDC.Objects.TRLFile;
using SRModel = CDC.Objects.Models.SRModel;

using SpriteTextRenderer;

namespace ModelEx
{
	public class SceneCDC : Scene
	{
		// Make these private again when parsers don't need them.
		// Mesh loading is quick. No longer necessary to show progress inside DRM loading.
		public static long progressLevels = 0;
		public static long progressLevel = 0;

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

		// Make these private again when parsers don't need them.
		// Mesh loading is quick. No longer necessary to show progress inside DRM loading.
		public static string ProgressStage { get; set; } = "Done";

		CDC.Game _game = CDC.Game.SR1;
		List<SRFile> _objectFiles = new List<SRFile>();

		RenderResource _markerResource = new RenderResource();
		List<RenderResource> _renderResources = new List<RenderResource>();

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

		public override void ImportFromFile(string fileName, CDC.Objects.ExportOptions options, bool isReload = false)
		{
			ImportFromFile(fileName, options, isReload, -1);
		}

		public void ImportFromFile(string fileName, CDC.Objects.ExportOptions options, bool isReload, int childIndex)
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

			RenderResourceCDC renderResource = null;

			if (_renderResources.Count > 0)
			{
				renderResource = (RenderResourceCDC)_renderResources[0];

				if (isReload)
				{
					renderResource.Dispose();
					renderResource = null;
					_renderResources.Remove(renderResource);
				}
			}
			
			if (renderResource == null)
			{
				renderResource = new RenderResourceCDC(srFile);
			}

			renderResource.LoadModels(options);
			foreach (Model model in renderResource.Models)
			{
				Physical physical = new Physical(model);
				renderables.Add(physical.Model);
				renderInstances.Add(physical);
			}

			// Use Octahedron to show positions of stuff.
			ModelParser octaParser = new ModelParser("octahedron");
			octaParser.BuildModel(_markerResource);
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
			_renderResources.Add(renderResource);

			if (options.TextureLoadRequired())
			{
				ProgressStage = "Loading Textures";
				// reset the instance so that textures with identical names are not retained from previous models
				//TextureManager.Instance.ResetInstance();
				//Thread.Sleep(1000);
				#region Textures
				#endregion
			}

			progressLevel = progressLevels;
			ProgressStage = "Done";
			//Thread.Sleep(1000);
		}

		public override void ExportToFile(string fileName, CDC.Objects.ExportOptions options)
		{
		}
	}
}
