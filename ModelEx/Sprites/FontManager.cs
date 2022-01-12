using System;

namespace ModelEx
{
	public class FontManager
	{
		private static FontManager instance = null;
		public static FontManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new FontManager();
				}
				return instance;
			}
		}

		private FontManager()
		{
		}

		public SlimDX.Direct3D10_1.Device1 D3DDevice10;
		public SlimDX.DirectWrite.Factory WriteFactory;
		public SlimDX.Direct2D.Factory D2DFactory;

		public void Initialize()
		{
			D3DDevice10 = new SlimDX.Direct3D10_1.Device1(SlimDX.Direct3D10.DeviceCreationFlags.BgraSupport, SlimDX.Direct3D10_1.FeatureLevel.Level_10_0);
			WriteFactory = new SlimDX.DirectWrite.Factory(SlimDX.DirectWrite.FactoryType.Shared);
			D2DFactory = new SlimDX.Direct2D.Factory(SlimDX.Direct2D.FactoryType.SingleThreaded);
		}

		public void ShutDown()
		{
			D3DDevice10.Dispose();
			WriteFactory.Dispose();
			D2DFactory.Dispose();
		}
	}
}