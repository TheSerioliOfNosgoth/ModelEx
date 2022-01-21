using System.Collections.Generic;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System.Runtime.InteropServices;

namespace ModelEx
{
	public class MeshPCT : Mesh
	{
		protected EffectWrapperGouraudTexture effect = ShaderManager.Instance.effectGouraudTexture;
		protected string technique = "";

		public MeshPCT(RenderResource resource, IMeshParser<PositionColorTexturedVertex, short> meshParser)
			: base(resource)
		{
			Name = meshParser.MeshName;
			technique = meshParser.Technique;
			SetIndexBuffer(meshParser);
			SetVertexBuffer(meshParser);
		}

		public override void ApplyBuffers()
		{
			DeviceManager.Instance.context.InputAssembler.InputLayout = effect.layout;
			DeviceManager.Instance.context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
			DeviceManager.Instance.context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, vertexStride, 0));
			DeviceManager.Instance.context.InputAssembler.SetIndexBuffer(indexBuffer, Format.R16_UInt, 0);
		}

		public override void ApplyMaterial(Material material)
		{
			effect.Constants.DiffuseColor = ColorToVector4(material.Diffuse);
			effect.Constants.AmbientColor = ColorToVector4(material.Ambient);
			effect.Constants.SpecularColor = ColorToVector4(material.Specular);
			effect.Constants.LightColor = new Vector4(1, 1, 1, 1);
			effect.Constants.SpecularPower = 128;

			if (material.TextureFileName != null && material.TextureFileName != "")
			{
				effect.Constants.UseTexture = true;
				effect.Texture = renderResource.GetShaderResourceView(material.TextureFileName + RenderResourceCDC.TextureExtension);
			}
			else
			{
				effect.Constants.UseTexture = false;
			}

			effect.Constants.DepthBias = material.DepthBias;

			effect.BlendMode = material.BlendMode;
			effect.Constants.VertexColorFactor = 1.0f; // 2.0f for Gex
		}

		public override void ApplyTransform(Matrix transform)
		{
			Matrix ViewPerspective = CameraManager.Instance.frameCamera.ViewPerspective;
			Matrix WorldViewPerspective = transform * ViewPerspective;
			Vector3 viewDir = CameraManager.Instance.frameCamera.eye - CameraManager.Instance.frameCamera.target;

			effect.Constants.World = Matrix.Transpose(transform);
			//effect.Constants.World = Matrix.Scaling(-1, 1, 1) * transform;
			effect.Constants.View = Matrix.Transpose(CameraManager.Instance.frameCamera.View);
			effect.Constants.Projection = Matrix.Transpose(CameraManager.Instance.frameCamera.Perspective);

			effect.Constants.CameraPosition = CameraManager.Instance.frameCamera.eye;
			effect.Constants.LightDirection = viewDir;

			effect.Constants.RealmBlend = 0.0f;
		}

		public override void Render(int indexCount, int startIndexLocation, int baseVertexLocation)
		{
			effect.Apply(1);
			DeviceManager.Instance.context.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
		}
	}
}