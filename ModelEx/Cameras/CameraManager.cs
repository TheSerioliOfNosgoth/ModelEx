using System;
using System.Collections.Generic;
using SlimDX;

namespace ModelEx
{
    public class CameraManager
    {
        public enum CameraMode : int
        {
            Ego = 0,
            Orbit = 1,
            OrbitPan = 2
        }

        List<DynamicCamera> cameras = new List<DynamicCamera>();

        public Camera frameCamera;
        public DynamicCamera currentCamera;
        CameraMode cameraMode;

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

        private CameraManager()
        {
            frameCamera = new Camera();
            EgoCamera ec = new EgoCamera();
            OrbitCamera oc = new OrbitCamera();
            OrbitPanCamera ocp = new OrbitPanCamera();
            cameras.Add(ec);
            cameras.Add(oc);
            cameras.Add(ocp);

            cameraMode = 0;
            currentCamera = cameras[(int)cameraMode];
        }

        public void Reset()
        {
            Vector3 eye = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 target = new Vector3(0.0f, 0.0f, 1.0f);

            Renderable currentObject = SceneManager.Instance.CurrentObject;
            if (currentObject != null && currentObject.GetType() == typeof(Physical))
            {
                BoundingSphere boundingSphere = currentObject.GetBoundingSphere();
                if (boundingSphere != null)
                {
                    target = boundingSphere.Center;
                    eye = target - new Vector3(0.0f, 0.0f, boundingSphere.Radius * 2.5f);
                }
            }

            SetView(eye, target);
        }

        public void SetPerspective(float fov, float aspect, float znear, float zfar)
        {
            foreach (Camera camera in cameras)
            {
                camera.SetPerspective(fov, aspect, znear, zfar);
            }
        }

        public void SetView(Vector3 eye, Vector3 target)
        {
            foreach (Camera camera in cameras)
            {
                camera.SetView(eye, target, new Vector3(0, 1, 0));
            }
        }

        public void SetCamera(CameraMode mode)
        {
            cameraMode = mode;
            currentCamera = cameras[(int)cameraMode];
        }

        public void UpdateFrameCamera()
        {
            currentCamera.Update();
            frameCamera.CopyFromOther(currentCamera);
        }
    }
}