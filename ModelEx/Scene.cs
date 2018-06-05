using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ModelEx
{
    public class Scene
    {
        #region Singleton Pattern
        private static Scene instance = null;
        public static Scene Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Scene();
                }
                return instance;
            }
        }
        #endregion

        #region Constructor
        private Scene()
        {
            renderObjects = new List<Renderable>();
            RenderObjects = new ReadOnlyCollection<Renderable>(renderObjects);
        }
        #endregion

        private List<Renderable> renderObjects;
        public readonly ReadOnlyCollection<Renderable> RenderObjects;

        public void AddRenderObject(Renderable renderObject)
        {
            lock (renderObjects)
            {
                renderObjects.Add(renderObject);
            }
        }

        public void RemoveRenderObject(Renderable renderObject)
        {
            lock (renderObjects)
            {
                if (renderObjects.Contains(renderObject))
                {
                    renderObject.Dispose();
                    renderObjects.Remove(renderObject);
                }
            }
        }

        public void ShutDown()
        {
            lock (renderObjects)
            {
                while (renderObjects.Count > 0)
                {
                    Renderable renderObject = renderObjects[0];
                    renderObject.Dispose();
                    renderObjects.Remove(renderObject);
                }
            }
        }

        public void Render()
        {
            lock (renderObjects)
            {
                foreach (Renderable renderable in renderObjects)
                {
                    renderable.Render();
                }
            }
        }
    }
}
