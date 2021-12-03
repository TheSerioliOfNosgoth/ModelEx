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

		public Matrix view = Matrix.Identity;
		public Matrix perspective = Matrix.Identity;
		public Matrix viewPerspective = Matrix.Identity;

		public Matrix View
		{
			get { return view; }
		}

		public Matrix Perspective
		{
			get { return perspective; }
		}

		public Matrix ViewPerspective
		{
			get { return view * perspective; }
		}

		public void SetPerspective(float fov, float aspect, float znear, float zfar)
		{
			perspective = Matrix.PerspectiveFovLH(fov, aspect, znear, zfar);
		}

		public virtual void SetView(Vector3 eye, Vector3 target, Vector3 up)
		{
			this.eye = eye;
			this.target = target;
			this.up = up;
			view = Matrix.LookAtLH(eye, target, up);
		}

		public void CopyFromOther(Camera camera)
		{
			SetView(camera.eye, camera.target, camera.up);
			perspective = camera.perspective;
		}
	}
}