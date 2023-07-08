using System;
using System.Windows.Forms;
using SlimDX;

namespace ModelEx
{
	public class Camera
	{
		public Vector3 eye;
		public Vector3 target;
		public Vector3 up;

		public Matrix View { get; protected set; } = Matrix.Identity;

		public Matrix Perspective
		{
			get { return CameraManager.Instance.Perspective; }
		}

		public Matrix ViewPerspective
		{
			get { return View * Perspective; }
		}

		public virtual void SetView(Vector3 eye, Vector3 target, Vector3 up)
		{
			this.eye = eye;
			this.target = target;
			this.up = up;
			View = Matrix.LookAtLH(eye, target, up);
		}

		public void CopyFromOther(Camera camera)
		{
			SetView(camera.eye, camera.target, camera.up);
		}
	}
}