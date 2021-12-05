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
		protected List<Renderable> renderables;
		protected List<RenderInstance> renderInstances;
		public readonly ReadOnlyCollection<RenderInstance> RenderInstances;
		public Renderable CurrentObject { get { return renderInstances.Count > 0 ? renderInstances[0] : null; } }
		protected Dictionary<string, Bitmap> _TexturesAsPNGs;

		protected Scene()
		{
			renderables = new List<Renderable>();
			renderInstances = new List<RenderInstance>();
			RenderInstances = new ReadOnlyCollection<RenderInstance>(renderInstances);
			_TexturesAsPNGs = new Dictionary<string, Bitmap>();
		}

		public virtual void Dispose()
		{
			renderInstances.Clear();

			while (renderables.Count > 0)
			{
				renderables[0].Dispose();
				renderables.Remove(renderables[0]);
			}
		}

		public virtual void Render()
		{
			// handle attempts to render where the renderInstances collection is modified by another threads
			int retryCount = 0;
			int maxTries = 5;
			int retryDelay = 1000;

			while (retryCount < maxTries)
			{
				try
				{
					foreach (Renderable renderable in renderInstances)
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
