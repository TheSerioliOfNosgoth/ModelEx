using System;
using System.IO;
using SlimDX.D3DCompiler;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System.Runtime.InteropServices;

namespace ModelEx
{
	public class IncludeFX : Include
	{
		string _includeDirectory = "";

		public IncludeFX(string includeDirectory)
		{
			_includeDirectory = includeDirectory;
		}

		public void Close(Stream stream)
		{
			stream.Close();
			stream.Dispose();
		}
		public void Open(IncludeType type, string fileName, Stream parentStream, out Stream stream)
		{
			stream = new FileStream(Path.Combine(_includeDirectory, fileName), FileMode.Open);
		}
	}

	public abstract class Effect
	{
		public abstract void Initialize();
		public abstract void Dispose();

		public abstract void Apply(int pass);
	}
}