using System;
using System.Collections.Generic;
using SlimDX;

namespace ModelEx
{
	public class CameraManager
	{
		public Camera FrameCamera { get; }
		public DynamicCamera CurrentCamera { get; set; }

		public Matrix Perspective { get; private set; } = Matrix.Identity;

		private static CameraManager instance;
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
		}

		public void ResetPosition()
		{
			Scene scene = (Scene)RenderManager.Instance.CameraTarget;
			if (scene != null)
			{
				scene.Cameras.ResetPositions();
			}
		}

		public void SetPerspective(float fov, float aspect, float znear, float zfar)
		{
			Perspective = Matrix.PerspectiveFovLH(fov, aspect, znear, zfar);
		}

		public void UpdateFrameCamera()
		{
			if (CurrentCamera != null)
			{
				CurrentCamera.Update();
				FrameCamera.CopyFromOther(CurrentCamera);
			}
		}
	}
}