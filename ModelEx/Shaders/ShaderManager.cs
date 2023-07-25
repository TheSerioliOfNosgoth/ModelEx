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
		public EffectWrapperLineCloud effectLineCloud;
		public EffectSprite effectSprite;

		public void Initialize()
		{
			transformEffectWireFrame = new EffectWrapperTransformEffectWireframe();
			effectPhongTexture = new EffectWrapperPhongTexture();
			effectGouraudTexture = new EffectWrapperGouraudTexture();
			effectMorphingUnit = new EffectMorphingUnit();
			effectLineCloud = new EffectWrapperLineCloud();
			effectSprite = new EffectSprite();

			transformEffectWireFrame.Initialize();
			effectPhongTexture.Initialize();
			effectGouraudTexture.Initialize();
			effectMorphingUnit.Initialize();
			effectLineCloud.Initialize();
			effectSprite.Initialize();
		}

		public void ShutDown()
		{
			transformEffectWireFrame?.Dispose();
			effectPhongTexture?.Dispose();
			effectGouraudTexture?.Dispose();
			effectMorphingUnit?.Dispose();
			effectLineCloud?.Dispose();
			effectSprite?.Dispose();
		}
	}
}