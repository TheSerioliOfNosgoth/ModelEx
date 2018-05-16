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

        public MeshPCT(IMeshParser<PositionColorTexturedVertex, short> meshParser)
        {
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
            if (material.TextureFileName != null)
            {
                effect.TextureVariable.SetResource(TextureManager.Instance.GetShaderResourceView(material.TextureFileName + ".dds"));
            }

            effect.technique = effect.effect.GetTechniqueByName("Render");
        }

        public override void ApplyTransform(Matrix transform)
        {
            Matrix ViewPerspective = CameraManager.Instance.ViewPerspective;
            Matrix WorldViewPerspective = transform * ViewPerspective;
            Vector3 viewDir = CameraManager.Instance.currentCamera.eye - CameraManager.Instance.currentCamera.target;

            effect.World.SetMatrix(transform);
            //ewu.World.SetMatrix(Matrix.Scaling(-1, 1, 1) *  this.transform);
            effect.View.SetMatrix(CameraManager.Instance.View);
            effect.Projection.SetMatrix(CameraManager.Instance.currentCamera.Perspective);

            effect.CameraPosition.Set(CameraManager.Instance.currentCamera.eye);
            effect.LightDirection.Set(viewDir);
        }

        public override void Render(int indexCount, int startIndexLocation, int baseVertexLocation)
        {
            EffectTechniqueDescription techDesc = effect.technique.Description;
            for (int p = 0; p < techDesc.PassCount; p++)
            {
                effect.technique.GetPassByIndex(p).Apply(DeviceManager.Instance.context);
                DeviceManager.Instance.context.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
            }
        }
    }
}