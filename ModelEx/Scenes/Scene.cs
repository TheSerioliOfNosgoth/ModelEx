using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ModelEx
{
    public abstract class Scene
    {
        private List<Renderable> renderObjects;
        public readonly ReadOnlyCollection<Renderable> RenderObjects;
        public Renderable CurrentObject { get { return RenderObjects.Count > 0 ? renderObjects[0] : null; } }

        protected Scene()
        {
            renderObjects = new List<Renderable>();
            RenderObjects = new ReadOnlyCollection<Renderable>(renderObjects);
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
            foreach (Renderable renderable in renderObjects)
            {
                renderable.Render();
            }
        }

        public abstract void ImportFromFile(string fileName);

        public abstract void ExportToFile(string fileName, string fileFormat = "collada");
    }
}
