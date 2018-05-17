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

        public virtual Matrix View
        {
            get { return view; }
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

        public virtual Matrix Perspective
        {
            get { return perspective; }
        }

        public virtual Matrix ViewPerspective
        {
            get { return view * perspective; }
        }

        public void CopyFromOther(Camera camera)
        {
            SetView(camera.eye, camera.target, camera.up);
            perspective = camera.perspective;
        }

        public bool dragging = false;
        public int startX = 0;
        public int deltaX = 0;

        public int startY = 0;
        public int deltaY = 0;

        public virtual void MouseUp(object sender, MouseEventArgs e) { }
        public virtual void MouseDown(object sender, MouseEventArgs e) { }
        public virtual void MouseMove(object sender, MouseEventArgs e) { }
        public virtual void MouseWheel(object sender, MouseEventArgs e) { }

        public virtual void KeyPress(object sender, KeyPressEventArgs e) { }
        public virtual void KeyDown(object sender, KeyEventArgs e) { }
        public virtual void KeyUp(object sender, KeyEventArgs e) { }
    }
}