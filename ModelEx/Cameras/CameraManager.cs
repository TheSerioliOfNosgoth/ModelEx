using System;
using System.Collections.Generic;
using SlimDX;

namespace ModelEx
{
    public class CameraManager
    {
        #region Singleton Pattern
        private static CameraManager instance = null;
        public static CameraManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CameraManager();
                }
                return instance;
            }
        }
        #endregion

        private CameraManager()
        {
            frameCamera = new Camera();
            OrbitPanCamera ocp = new OrbitPanCamera();
            OrbitCamera oc = new OrbitCamera();
            EgoCamera ec = new EgoCamera();
            cameras.Add(ocp);
            cameras.Add(oc);
            cameras.Add(ec);

            currentIndex = 0;
            currentCamera = cameras[currentIndex];
        }

        List<DynamicCamera> cameras = new List<DynamicCamera>();

        public Camera frameCamera;
        public DynamicCamera currentCamera;
        int currentIndex;

        public Matrix ViewPerspective
        {
            get
            {
                return currentCamera.ViewPerspective;
            }
        }

        public void SetPerspective(float fov, float aspect, float znear, float zfar)
        {
            foreach (Camera camera in cameras)
            {
                camera.SetPerspective(fov, aspect, znear, zfar);
            }
        }

        public Matrix View
        {
            get
            {
                return currentCamera.View;
            }
        }

        public void SetView(Vector3 eye, Vector3 target)
        {
            foreach (Camera camera in cameras)
            {
                camera.SetView(eye, target, new Vector3(0, 1, 0));
            }
        }

        public string CycleCameras()
        {
            Camera previousCamera = currentCamera;

            int numCameras = cameras.Count;
            currentIndex = currentIndex + 1;
            if (currentIndex == numCameras)
                currentIndex = 0;
            currentCamera = cameras[currentIndex];

            if (previousCamera != null && currentCamera != null)
            {
                currentCamera.CopyFromOther(previousCamera);
            }

            return currentCamera.ToString();
        }

        public void UpdateFrameCamera()
        {
            currentCamera.Update();
            frameCamera.CopyFromOther(currentCamera);
        }
    }
}