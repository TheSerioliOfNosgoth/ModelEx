using System;

namespace ModelEx
{
	public class ShaderManager
	{
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

		private ShaderManager()
		{
		}

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

		public void InitShaders()
		{
			transformEffectWireFrame.Initialize();
			effectPhongTexture.Initialize();
			effectGouraudTexture.Initialize();
			effectMorphingUnit.Initialize();
		}
	}
}