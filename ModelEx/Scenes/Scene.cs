using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace ModelEx
{
	public abstract class Scene
	{
		private List<Renderable> renderObjects;
		public readonly ReadOnlyCollection<Renderable> RenderObjects;
		public Renderable CurrentObject { get { return RenderObjects.Count > 0 ? renderObjects[0] : null; } }
		protected Dictionary<string, Bitmap> _TexturesAsPNGs;

		protected Scene()
		{
			renderObjects = new List<Renderable>();
			RenderObjects = new ReadOnlyCollection<Renderable>(renderObjects);
			_TexturesAsPNGs = new Dictionary<string, Bitmap>();
		}

		public virtual void Dispose()
		{
			while (renderObjects.Count > 0)
			{
				renderObjects[0].Dispose();
				renderObjects.Remove(renderObjects[0]);
			}
		}

		protected void AddRenderObject(Renderable renderObject)
		{
			renderObjects.Add(renderObject);
		}

		protected void RemoveRenderObject(Renderable renderObject)
		{
			if (renderObjects.Contains(renderObject))
			{
				renderObject.Dispose();
				renderObjects.Remove(renderObject);
			}
		}

		public virtual void Render()
		{
			// handle attempts to render where the renderObjects collection is modified by another threads
			int retryCount = 0;
			int maxTries = 5;
			int retryDelay = 1000;

			while (retryCount < maxTries)
			{
				try
				{
					foreach (Renderable renderable in renderObjects)
					{
						renderable.Render();
					}
					retryCount = maxTries + 1;
				}
				catch (Exception ex)
				{
					retryCount++;
					string message = string.Format("retrying in {0} milliseconds", retryDelay);
					if (retryCount >= maxTries)
					{
						message = string.Format("giving up after {0} attempts", maxTries);
					}
					Console.WriteLine(string.Format("Exception thrown while rendering objects: {0}, {1}", ex.Message, message));
					Thread.Sleep(retryDelay);
				}
			}
		}

		public abstract void ImportFromFile(string fileName, CDC.Objects.ExportOptions options);

		public abstract void ExportToFile(string fileName, CDC.Objects.ExportOptions options);
	}
}
