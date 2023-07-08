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

		CameraMode mode;

		public CameraMode Mode
		{
			get
			{
				return mode;
			}
			set
			{
				mode = value;
				CurrentCamera = cameras[(int)mode];
			}
		}

		List<DynamicCamera> cameras = new List<DynamicCamera>();

		public Camera FrameCamera { get; set; }
		public DynamicCamera CurrentCamera { get; set; }

		public Matrix Perspective { get; private set; } = Matrix.Identity;

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
			FrameCamera = new Camera();
			EgoCamera ec = new EgoCamera();
			OrbitCamera oc = new OrbitCamera();
			OrbitPanCamera ocp = new OrbitPanCamera();
			cameras.Add(ec);
			cameras.Add(oc);
			cameras.Add(ocp);

			mode = 0;
			CurrentCamera = cameras[(int)mode];
		}

		public void Reset()
		{
			Vector3 eye = new Vector3(0.0f, 0.0f, 0.0f);
			Vector3 target = new Vector3(0.0f, 0.0f, 1.0f);

			Renderable cameraTarget = RenderManager.Instance.GetCameraTarget();
			if (cameraTarget != null)
			{
				BoundingSphere boundingSphere = cameraTarget.GetBoundingSphere();
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
			Perspective = Matrix.PerspectiveFovLH(fov, aspect, znear, zfar);
		}

		public void SetView(Vector3 eye, Vector3 target)
		{
			foreach (Camera camera in cameras)
			{
				camera.SetView(eye, target, new Vector3(0, 1, 0));
			}
		}

		public void UpdateFrameCamera()
		{
			CurrentCamera.Update();
			FrameCamera.CopyFromOther(CurrentCamera);
		}
	}
}