using System;
using SlimDX.D3DCompiler;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System.Runtime.InteropServices;

namespace ModelEx
{
    public class EffectWrapperPhongBlinn
    {
        public ShaderBytecode effectByteCode;
        public Effect effect;
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

        //public EffectVectorVariable diffuse;
        ////public EffectVectorVariable emissive;
        //public EffectVectorVariable ambient;
        ////public EffectVectorVariable specular;

        InputElement[] elements = new[] { 
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0)
            };

        public void Load()
        {
            try
            {
                using (ShaderBytecode effectByteCode = ShaderBytecode.CompileFromFile(
                    "Shaders/PhongBlinn.fx",
                    "Render",
                    "fx_5_0",
                    ShaderFlags.EnableStrictness,
                    EffectFlags.None))
                {
                    effect = new Effect(DeviceManager.Instance.device, effectByteCode);
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

                //diffuse = effect.GetVariableByName("materialDiffuse").AsVector();
                //emissive = effect.GetVariableByName("materialEmissive").AsVector();
                //ambient = effect.GetVariableByName("materialAmbient").AsVector();
                //specular = effect.GetVariableByName("materialSpecular").AsVector();
                //dirLightColor = effect.GetVariableByName("dirLightColor").AsVector();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void Dispose()
        {
            effect?.Dispose();
            layout?.Dispose();
        }
    }
}