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
		public SortedList<string, Texture2D> FileTextureDictionary = new SortedList<string, Texture2D>();
		public SortedList<string, ShaderResourceView> FileShaderResourceViewDictionary = new SortedList<string, ShaderResourceView>();
		protected SortedList<string, Bitmap> _TexturesAsPNGs = new SortedList<string, Bitmap>();

		public RenderResource(string name)
		{
			Name = name;
		}

		public void Dispose()
		{
			while (Models.Count > 0)
			{
				Model model = Models[0];
				model.Dispose();
				Models.RemoveAt(0);
			}

			while (FileShaderResourceViewDictionary.Count > 0)
			{
				string key = FileShaderResourceViewDictionary.Keys[0];
				ShaderResourceView fsResourceView = FileShaderResourceViewDictionary[key];
				fsResourceView.Dispose();
				FileShaderResourceViewDictionary.Remove(key);
			}

			while (FileTextureDictionary.Count > 0)
			{
				string key = FileTextureDictionary.Keys[0];
				Texture2D texture = FileTextureDictionary[key];
				texture.Dispose();
				FileTextureDictionary.Remove(key);
			}

			while (_TexturesAsPNGs.Count > 0)
			{
				string key = _TexturesAsPNGs.Keys[0];
				Bitmap bitmap = _TexturesAsPNGs[key];
				bitmap.Dispose();
				_TexturesAsPNGs.Remove(key);
			}
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
