using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace ModelEx
{
	public class RenderResource
	{
		public string Name { get; private set; } = "";

		public List<Model> Models = new List<Model>();
		public Dictionary<string, Texture2D> FileTextureDictionary = new Dictionary<string, Texture2D>();
		public Dictionary<string, ShaderResourceView> FileShaderResourceViewDictionary = new Dictionary<string, ShaderResourceView>();
		protected Dictionary<string, Bitmap> _TexturesAsPNGs = new Dictionary<string, Bitmap>();

		public RenderResource(string name)
		{
			Name = name;
		}

		public void Dispose()
		{
		}

		public void AddTexture(Stream stream, string fileName)
		{
			try
			{
				if (fileName.Contains("\\"))
				{
					fileName = System.IO.Path.GetFileName(fileName);
				}

				if (!FileTextureDictionary.ContainsKey(fileName))
				{
					Texture2D texture;
					long oldPosition = stream.Position;
					stream.Position = 0;
					texture = Texture2D.FromStream(DeviceManager.Instance.device, stream, (int)stream.Length);
					stream.Position = oldPosition;

					FileTextureDictionary.Add(fileName, texture);

					if (!FileShaderResourceViewDictionary.ContainsKey(fileName))
					{
						ShaderResourceView textureView = new ShaderResourceView(DeviceManager.Instance.device, texture);
						FileShaderResourceViewDictionary.Add(fileName, textureView);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		public ShaderResourceView GetShaderResourceView(string fileName)
		{
			if (fileName.Contains("\\"))
			{
				fileName = System.IO.Path.GetFileName(fileName);
			}

			if (FileShaderResourceViewDictionary.ContainsKey(fileName))
			{
				return FileShaderResourceViewDictionary[fileName];
			}

			return null;
		}
	}
}
