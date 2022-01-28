using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using SlimDX;

namespace ModelEx
{
	public abstract class Scene : Renderable
	{
		protected bool _includeObjects;
		protected List<RenderInstance> _renderInstances;

		public readonly ReadOnlyCollection<RenderInstance> RenderInstances;
		public Renderable CurrentObject { get { return _renderInstances.Count > 0 ? _renderInstances[0] : null; } }

		protected Scene(bool includeObjects)
		{
			_includeObjects = includeObjects;
			_renderInstances = new List<RenderInstance>();
			RenderInstances = new ReadOnlyCollection<RenderInstance>(_renderInstances);
		}

		public override void Dispose()
		{
			_renderInstances.Clear();
		}

		public override void Render()
		{
			// handle attempts to render where the renderInstances collection is modified by another threads
			int retryCount = 0;
			int maxTries = 5;
			int retryDelay = 1000;

			while (retryCount < maxTries)
			{
				try
				{
					foreach (Renderable renderable in _renderInstances)
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

		public override BoundingSphere GetBoundingSphere()
		{
			BoundingSphere boundingSphere = new BoundingSphere();
			if (CurrentObject != null)
			{
				boundingSphere = CurrentObject.GetBoundingSphere();
			}

			return boundingSphere;
		}
	}
}
