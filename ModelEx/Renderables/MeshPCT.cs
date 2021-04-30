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

        public MeshPCT(IMeshParser<PositionColorTexturedVertex, short> meshParser)
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
            effect.DiffuseColor.Set(ColorToVector4(material.Diffuse));
            effect.AmbientColor.Set(ColorToVector4(material.Ambient));
            effect.SpecularColor.Set(ColorToVector4(material.Specular));
            effect.LightColor.Set(new Vector4(1, 1, 1, 1));
            effect.SpecularPower.Set(128);
            if (material.TextureFileName != null && material.TextureFileName != "")
            {
                effect.UseTexture.Set(true);
                effect.TextureVariable.SetResource(TextureManager.Instance.GetShaderResourceView(material.TextureFileName + SceneCDC.TextureExtension));
            }
            else
            {
                effect.UseTexture.Set(false);
            }
            effect.DepthBias.Set((material.IsDecal) ? 0.0f : 0.0f);

            effect.IsDecal = material.IsDecal;
            effect.BlendMode = material.BlendMode;

            effect.technique = effect.effect.GetTechniqueByName(technique);
        }

        public override void ApplyTransform(Matrix transform)
        {
            Matrix ViewPerspective = CameraManager.Instance.frameCamera.ViewPerspective;
            Matrix WorldViewPerspective = transform * ViewPerspective;
            Vector3 viewDir = CameraManager.Instance.frameCamera.eye - CameraManager.Instance.frameCamera.target;

            effect.World.SetMatrix(transform);
            //ewu.World.SetMatrix(Matrix.Scaling(-1, 1, 1) *  this.transform);
            effect.View.SetMatrix(CameraManager.Instance.frameCamera.View);
            effect.Projection.SetMatrix(CameraManager.Instance.frameCamera.Perspective);

            effect.CameraPosition.Set(CameraManager.Instance.frameCamera.eye);
            effect.LightDirection.Set(viewDir);
        }

        public override void Render(int indexCount, int startIndexLocation, int baseVertexLocation)
        {
            EffectTechniqueDescription techDesc = effect.technique.Description;
            for (int p = 0; p < techDesc.PassCount; p++)
            {
                effect.technique.GetPassByIndex(p).Apply(DeviceManager.Instance.context);
                effect.ApplyBlendState();
                DeviceManager.Instance.context.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
            }
        }
    }
}