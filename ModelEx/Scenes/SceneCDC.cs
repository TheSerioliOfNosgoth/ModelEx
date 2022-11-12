using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace ModelEx
{
	public class SceneCDC : Scene
	{
		// Make these private again when parsers don't need them.
		// Mesh loading is quick. No longer necessary to show progress inside DRM loading.
		public static string ProgressStage { get; set; } = "Done";
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

		public SceneCDC(CDC.DataFile dataFile, bool includeObjects)
			: base(includeObjects)
		{
			Name = dataFile.Name;

			for (int m = 0; m < dataFile.ModelCount; m++)
			{
				RenderInstance instance = new RenderInstance(dataFile.Name, m, new SlimDX.Vector3(), new SlimDX.Quaternion(), new SlimDX.Vector3(1.0f, 1.0f, 1.0f));
				instance.Name = dataFile.Models[m].Name + "-" + m.ToString();
				lock (_renderInstances)
				{
					_renderInstances.Add(instance);
				}
			}

			if (includeObjects && dataFile.Asset == CDC.Asset.Unit && dataFile.IntroCount > 0)
			{
				bool foundUprightSword = false;
				SlimDX.Vector3 saveSwdPos = new SlimDX.Vector3();
				foreach (CDC.Intro intro in dataFile.Intros)
				{
					if (intro.fileName == "saveswd" && Math.Abs(intro.rotation.x) < 3.0f)
					{
						saveSwdPos = new SlimDX.Vector3(
							0.01f * intro.position.x,
							0.01f * intro.position.z,
							0.01f * intro.position.y
						);

						foundUprightSword = true;
						break;
					}
				}

				foreach (CDC.Intro intro in dataFile.Intros)
				{
					SlimDX.Vector3 position = new SlimDX.Vector3(
						0.01f * intro.position.x,
						0.01f * intro.position.z,
						0.01f * intro.position.y
					);

					SlimDX.Vector3 scale = new SlimDX.Vector3(1.0f, 1.0f, 1.0f);

					SlimDX.Quaternion rotation;
					if (dataFile.Game == CDC.Game.Gex || dataFile.Game == CDC.Game.SR1)
					{
						rotation = SlimDX.Quaternion.RotationYawPitchRoll(
							-intro.rotation.z, // Yaw - Easy to spot from direction of raziel, enemies, flagall.
							-intro.rotation.y, // Pitch - Can be seen from the angle of hndtrch in huba6.
							-intro.rotation.x // Roll - Can be seen from the angle of stdorac in oracle3.
						);
					}
					else
					{
						SlimDX.Matrix.RotationX(-intro.rotation.x, out SlimDX.Matrix rotationMatrixX);
						SlimDX.Matrix.RotationY(-intro.rotation.z, out SlimDX.Matrix rotationMatrixY);
						SlimDX.Matrix.RotationZ(-intro.rotation.y, out SlimDX.Matrix rotationMatrixZ);
						SlimDX.Matrix rotationMatrix = rotationMatrixZ * rotationMatrixY * rotationMatrixX;
						SlimDX.Quaternion.RotationMatrix(ref rotationMatrix, out rotation);

						/*rotation = SlimDX.Quaternion.RotationYawPitchRoll(
							-intro.rotation.z,
							-intro.rotation.y,
							-intro.rotation.x
						);*/
					}

					if (intro.fileName == "saveswd" && Math.Abs(intro.rotation.x) >= 3.0f)
					{
						if (foundUprightSword)
						{
							position.X = saveSwdPos.X;
							position.Z = saveSwdPos.Z;
						}

						scale.Z = -1.0f;
					}

					RenderInstance instance = new RenderInstance(intro.fileName, intro.modelIndex, position, rotation, scale);
					instance.Name = intro.name;

					lock (_renderInstances)
					{
						_renderInstances.Add(instance);
					}
				}
			}
		}

		public SceneCDC(CDC.DataFile dataFile, RenderResource resource)
			: base(false)
		{
			Name = dataFile.Name;

			for (int m = 0; m < dataFile.ModelCount; m++)
			{
				RenderInstance instance = new RenderInstance(dataFile.Name, m, new SlimDX.Vector3(), new SlimDX.Quaternion(), new SlimDX.Vector3(1.0f, 1.0f, 1.0f), resource);
				instance.Name = dataFile.Models[m].Name + "-" + m.ToString();
				lock (_renderInstances)
				{
					_renderInstances.Add(instance);
				}
			}
		}

		public override void Render()
		{
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

			lock (_renderInstances)
			{
				foreach (RenderInstance instance in _renderInstances)
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

						RenderManager.Instance.DrawString(instance.Name, position2D, 16 * scale, new SlimDX.Color4(1.0f, 1.0f, 1.0f));
					}
				}
			}

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
	}
}
