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
        public EffectWrapperPhongBlinn effectPhongBlinn;
        public EffectWrapperPhongTexture effectPhongTexture;
        public EffectWrapperGouraudTexture effectGouraudTexture;

        public void Initialize()
        {
            transformEffectWireFrame = new EffectWrapperTransformEffectWireframe();
            effectPhongBlinn = new EffectWrapperPhongBlinn();
            effectPhongTexture = new EffectWrapperPhongTexture();
            effectGouraudTexture = new EffectWrapperGouraudTexture();
        }

        public void ShutDown()
        {
            transformEffectWireFrame?.Dispose();
            effectPhongBlinn?.Dispose();
            effectPhongTexture?.Dispose();
            effectGouraudTexture?.Dispose();
        }

        public void LoadShaders()
        {
            transformEffectWireFrame.Load();
            effectPhongBlinn.Load();
            effectPhongTexture.Load();
            effectGouraudTexture.Load();
        }
    }
}