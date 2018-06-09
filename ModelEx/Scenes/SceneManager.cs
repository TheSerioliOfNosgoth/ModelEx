using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ModelEx
{
    public class SceneManager
    {
        private static SceneManager instance = null;
        public static SceneManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SceneManager();
                }
                return instance;
            }
        }

        private List<Scene> scenes;
        public readonly ReadOnlyCollection<Scene> Scenes;
        public Scene CurrentScene { get; private set; }
        public Renderable CurrentObject { get { return CurrentScene?.RenderObjects[0]; } }

        private SceneManager()
        {
            scenes = new List<Scene>();
            Scenes = new ReadOnlyCollection<Scene>(scenes);
        }

        public void AddScene(Scene scene)
        {
            scenes.Add(scene);
            CurrentScene = scene;
        }

        public void RemoveScene(Scene scene)
        {
            int index = scenes.IndexOf(scene);
            scenes.Remove(scene);

            if (scenes.Count <= 0)
            {
                CurrentScene = null;
            }
            else if (index >= 0 && index < scenes.Count)
            {
                CurrentScene = scenes[index];
            }
            else
            {
                CurrentScene = scenes[scenes.Count - 1];
            }

            scene.Dispose();
        }

        public void ShutDown()
        {
            CurrentScene = null;

            while (scenes.Count > 0)
            {
                scenes[0].Dispose();
                scenes.Remove(scenes[0]);
            }
        }

        public void Render()
        {
            CurrentScene?.Render();
        }
    }
}
