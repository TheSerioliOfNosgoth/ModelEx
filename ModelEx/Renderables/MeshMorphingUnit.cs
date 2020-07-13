using System.Collections.Generic;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System.Runtime.InteropServices;

namespace ModelEx
{
    public class MeshMorphingUnit : Mesh
    {
        protected EffectMorphingUnit effect = ShaderManager.Instance.effectMorphingUnit;
        protected string technique = "";

        public static float RealmBlend = 0.0f;

        public MeshMorphingUnit(IMeshParser<Position2Color2TexturedVertex, short> meshParser)
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
            if (effect.DiffuseColor != null)
            {
                effect.DiffuseColor.Set(ColorToVector4(material.Diffuse));
            }
            if (effect.AmbientColor != null)
            {
                effect.AmbientColor.Set(ColorToVector4(material.Ambient));
            }
            if (effect.SpecularColor != null)
            {
                effect.SpecularColor.Set(ColorToVector4(material.Specular));
            }
            if (effect.LightColor != null)
            {
                effect.LightColor.Set(new Vector4(1, 1, 1, 1));
            }
            if (effect.SpecularPower != null)
            {
                effect.SpecularPower.Set(128);
            }
            if (material.TextureFileName != null && material.TextureFileName != "" && effect.UseTexture != null && effect.TextureVariable != null)
            {
                effect.UseTexture.Set(true);
                effect.TextureVariable.SetResource(TextureManager.Instance.GetShaderResourceView(material.TextureFileName + SceneCDC.TextureExtension));
            }
            else
            {
                if (effect.UseTexture != null)
                {
                    effect.UseTexture.Set(false);
                }
            }

            if (effect.effect != null)
            {
                effect.technique = effect.effect.GetTechniqueByName(technique);
            }
        }

        public override void ApplyTransform(Matrix transform)
        {
            Matrix ViewPerspective = CameraManager.Instance.frameCamera.ViewPerspective;
            Matrix WorldViewPerspective = transform * ViewPerspective;
            Vector3 viewDir = CameraManager.Instance.frameCamera.eye - CameraManager.Instance.frameCamera.target;

            if (effect.World != null && effect.View != null && effect.Projection != null && effect.CameraPosition != null && effect.LightDirection != null && effect.RealmBlend != null)
            {
                effect.World.SetMatrix(transform);
                //ewu.World.SetMatrix(Matrix.Scaling(-1, 1, 1) *  this.transform);
                effect.View.SetMatrix(CameraManager.Instance.frameCamera.View);
                effect.Projection.SetMatrix(CameraManager.Instance.frameCamera.Perspective);

                effect.CameraPosition.Set(CameraManager.Instance.frameCamera.eye);
                effect.LightDirection.Set(viewDir);

                effect.RealmBlend.Set(RealmBlend);
            }
        }

        public override void Render(int indexCount, int startIndexLocation, int baseVertexLocation)
        {
            if (effect.technique != null)
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
}