﻿using System;
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

        #region Constructor
        private CameraManager()
        {
            OrbitPanCamera ocp = new OrbitPanCamera();
            OrbitCamera oc = new OrbitCamera();
            EgoCamera ec = new EgoCamera();
            cameras.Add(ocp);
            cameras.Add(oc);
            cameras.Add(ec);

            currentIndex = 0;
            currentCamera = cameras[currentIndex];
        }
        #endregion

        List<Camera> cameras = new List<Camera>();

        public Camera currentCamera;
        int currentIndex;

        public Matrix ViewPerspective
        {
            get
            {
                if (currentCamera is EgoCamera)
                {
                    return ((EgoCamera)currentCamera).ViewPerspective;
                }
                else
                {
                    return currentCamera.ViewPerspective;
                }

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
                if (currentCamera is EgoCamera)
                {
                    return ((EgoCamera)currentCamera).View;
                }
                else
                {
                    return currentCamera.View;
                }
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
    }
}