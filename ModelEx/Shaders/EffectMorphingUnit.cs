using System;
using SlimDX.D3DCompiler;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System.Runtime.InteropServices;

namespace ModelEx
{
    public class EffectMorphingUnit : Effect
    {
        public ShaderBytecode effectByteCode;
        public SlimDX.Direct3D11.Effect effect;
        public ShaderSignature inputSignature;
        public EffectTechnique technique;
        public EffectPass pass;
        public InputLayout layout;

        public EffectMatrixVariable World;
        public EffectMatrixVariable View;
        public EffectMatrixVariable Projection;
        public EffectVectorVariable CameraPosition;
        public EffectVectorVariable LightDirection;

        // Colors
        public EffectVectorVariable AmbientColor;
        public EffectVectorVariable DiffuseColor;
        public EffectVectorVariable SpecularColor;
        public EffectVectorVariable LightColor;

        public EffectScalarVariable SpecularPower;

        // Texture
        public EffectScalarVariable UseTexture;
        public EffectResourceVariable TextureVariable;

        // Realm
        public EffectScalarVariable RealmBlend;

        InputElement[] elements = new[] { 
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0,  0),
                new InputElement("POSITION", 1, Format.R32G32B32_Float, 12, 0),
                new InputElement("COLOR",    0, Format.R32G32B32_Float, 24, 0),
                new InputElement("COLOR",    1, Format.R32G32B32_Float, 36, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float,    48, 0) 
            };

        public override void Initialize()
        {
            base.Initialize();

            try
            {
                using (ShaderBytecode effectByteCode = ShaderBytecode.CompileFromFile(
                    "Shaders/MorphingUnit.fx",
                    "Render",
                    "fx_5_0",
                    ShaderFlags.EnableStrictness,
                    EffectFlags.None))
                {
                    effect = new SlimDX.Direct3D11.Effect(DeviceManager.Instance.device, effectByteCode);
                    technique = effect.GetTechniqueByIndex(0);
                    pass = technique.GetPassByIndex(0);
                    inputSignature = pass.Description.Signature;
                }

                layout = new InputLayout(DeviceManager.Instance.device, inputSignature, elements);

                //WorldViewProjection = effect.GetVariableByName("matWorldViewProj").AsMatrix();
                World = effect.GetVariableByName("World").AsMatrix();
                View = effect.GetVariableByName("View").AsMatrix();
                Projection = effect.GetVariableByName("Projection").AsMatrix();

                //LightDir = effect.GetVariableByName("vecLightDir").AsVector();
                CameraPosition = effect.GetVariableByName("CameraPosition").AsVector();
                LightDirection = effect.GetVariableByName("LightDirection").AsVector();

                DiffuseColor = effect.GetVariableByName("DiffuseColor").AsVector();
                AmbientColor = effect.GetVariableByName("AmbientColor").AsVector();
                SpecularColor = effect.GetVariableByName("SpecularColor").AsVector();
                LightColor = effect.GetVariableByName("LightColor").AsVector();

                SpecularPower = effect.GetVariableByName("SpecularPower").AsScalar();

                UseTexture = effect.GetVariableByName("UseTexture").AsScalar();
                TextureVariable = effect.GetVariableByName("Texture").AsResource();

                RealmBlend = effect.GetVariableByName("RealmBlend").AsScalar();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public override void Dispose()
        {
            effect?.Dispose();
            layout?.Dispose();
        }
    }
}
