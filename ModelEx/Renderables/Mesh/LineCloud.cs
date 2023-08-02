using System.Collections.Generic;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System.Runtime.InteropServices;

namespace ModelEx
{
	public class LineCloud : Mesh
	{
		protected EffectWrapperLineCloud _effect = ShaderManager.Instance.effectLineCloud;
		protected string _technique = "";

		public LineCloud(RenderResource resource, IMeshParser<PositionNormalVertex> meshParser)
			: base(resource)
		{
			Name = meshParser.MeshName;
			_technique = meshParser.Technique;
			SetVertexBuffer(meshParser);
		}

		public override void ApplyBuffers()
		{
			DeviceManager.Instance.context.InputAssembler.InputLayout = _effect.layout;
			DeviceManager.Instance.context.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
			DeviceManager.Instance.context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBuffer, _vertexStride, 0));
		}

		public override void ApplyMaterial(Material material)
		{
			_effect.Constants.DiffuseColor = ColorToVector4(material.Diffuse);
			_effect.Constants.AmbientColor = ColorToVector4(material.Ambient);
			_effect.Constants.SpecularColor = ColorToVector4(material.Specular);
			_effect.Constants.LightColor = new Vector4(1, 1, 1, 1);
			_effect.Constants.SpecularPower = 128;

			if (material.TextureFileName != null && material.TextureFileName != "")
			{
				_effect.Constants.UseTexture = true;
				_effect.Texture = _renderResource.GetShaderResourceView(material.TextureFileName + RenderResourceCDC.TextureExtension);
			}
			else
			{
				_effect.Constants.UseTexture = false;
			}

			_effect.Constants.DepthBias = material.DepthBias;

			_effect.BlendMode = material.BlendMode;
		}

		public override void ApplyTransform(Matrix transform)
		{
			Matrix ViewPerspective = CameraManager.Instance.FrameCamera.ViewPerspective;
			Matrix WorldViewPerspective = transform * ViewPerspective;
			Vector3 viewDir = CameraManager.Instance.FrameCamera.eye - CameraManager.Instance.FrameCamera.target;

			_effect.Constants.World = Matrix.Transpose(transform);
			//effect.Constants.World = Matrix.Scaling(-1, 1, 1) * transform;
			_effect.Constants.View = Matrix.Transpose(CameraManager.Instance.FrameCamera.View);
			_effect.Constants.Projection = Matrix.Transpose(CameraManager.Instance.FrameCamera.Perspective);

			_effect.Constants.CameraPosition = CameraManager.Instance.FrameCamera.eye;
			_effect.Constants.LightDirection = viewDir;

			_effect.Constants.RealmBlend = 0.0f;
		}

		public override void Render(int indexCount, int startIndexLocation, int baseVertexLocation)
		{
			_effect.Apply(1);
			DeviceManager.Instance.context.Draw(indexCount, baseVertexLocation);
		}
	}
}