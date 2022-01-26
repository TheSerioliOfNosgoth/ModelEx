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
		protected List<RenderInstance> renderInstances;
		public readonly ReadOnlyCollection<RenderInstance> RenderInstances;
		public Renderable CurrentObject { get { return renderInstances.Count > 0 ? renderInstances[0] : null; } }

		protected Scene()
		{
			renderInstances = new List<RenderInstance>();
			RenderInstances = new ReadOnlyCollection<RenderInstance>(renderInstances);
		}

		public virtual void Dispose()
		{
			renderInstances.Clear();
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
	}
}
