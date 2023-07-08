using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using ModelEx.Cameras;
using SlimDX;

namespace ModelEx
{
	public abstract class Scene : Renderable
	{
		protected bool _includeObjects;
		protected List<RenderInstance> _renderInstances;

		public readonly ReadOnlyCollection<RenderInstance> RenderInstances;
		public Renderable CurrentObject { get { return _renderInstances.Count > 0 ? _renderInstances[0] : null; } }
		//public CameraSet Cameras { get; protected set; }

		protected Scene(bool includeObjects)
		{
			_includeObjects = includeObjects;
			_renderInstances = new List<RenderInstance>();
			RenderInstances = new ReadOnlyCollection<RenderInstance>(_renderInstances);
			//Cameras = new CameraSet(this);
		}

		public override void Dispose()
		{
			lock (_renderInstances)
			{
				_renderInstances.Clear();
			}
		}

		public void UpdateModels()
		{
			lock (_renderInstances)
			{
				foreach (Renderable renderable in _renderInstances)
				{
					if (renderable is RenderInstance)
					{
						RenderInstance renderInstance = (RenderInstance)renderable;
						renderInstance.UpdateModel();
					}
				}
			}
		}

		public override void Render()
		{
			lock (_renderInstances)
			{
				foreach (Renderable renderable in _renderInstances)
				{
					renderable.Render();
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
