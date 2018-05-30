using System;

namespace ModelEx
{
    public class ShaderManager
    {
        #region Singleton Pattern
        private static ShaderManager instance = null;
        public static ShaderManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ShaderManager();
                }
                return instance;
            }
        }
        #endregion

        #region Constructor
        private ShaderManager() { }
        #endregion

        public EffectWrapperTransformEffectWireframe transformEffectWireFrame;
        public EffectWrapperPhongTexture effectPhongTexture;
        public EffectWrapperGouraudTexture effectGouraudTexture;
        public EffectMorphingUnit effectMorphingUnit;

        public void Initialize()
        {
            transformEffectWireFrame = new EffectWrapperTransformEffectWireframe();
            effectPhongTexture = new EffectWrapperPhongTexture();
            effectGouraudTexture = new EffectWrapperGouraudTexture();
            effectMorphingUnit = new EffectMorphingUnit();
        }

        public void ShutDown()
        {
            transformEffectWireFrame?.Dispose();
            effectPhongTexture?.Dispose();
            effectGouraudTexture?.Dispose();
            effectMorphingUnit?.Dispose();
        }

        public void LoadShaders()
        {
            transformEffectWireFrame.Load();
            effectPhongTexture.Load();
            effectGouraudTexture.Load();
            effectMorphingUnit.Load();
        }
    }
}