using System;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace ModelEx
{
    public class OrbitPanCamera : DynamicCamera
    {
        float maxZoom = 3.0f;

        public OrbitPanCamera()
        {
            eye = new Vector3(4, 2, 0);
            target = new Vector3(0, 0, 0);
            up = new Vector3(0, 1, 0);

            view = Matrix.LookAtLH(eye, target, up);
            perspective = Matrix.PerspectiveFovLH((float)Math.PI / 4, 1.0f, 0.1f, 1000.0f);
        }

        public void RotateY(int value)
        {
            float rotY = (value / 100.0f);
            Vector3 eyeLocal = eye - target;

            Matrix rotMat = Matrix.RotationY(rotY);
            eyeLocal = Vector3.TransformCoordinate(eyeLocal, rotMat);
            eye = eyeLocal + target;

            SetView(eye, target, up);
        }

        public void RotateOrtho(int value)
        {
            Vector3 viewDir = target - eye;
            Vector3 orhto = Vector3.Cross(viewDir, up);

            float rotOrtho = (value / 100.0f);
            Matrix rotOrthoMat = Matrix.RotationAxis(orhto, rotOrtho);

            Vector3 eyeLocal = eye - target;
            eyeLocal = Vector3.TransformCoordinate(eyeLocal, rotOrthoMat);
            Vector3 newEye = eyeLocal + target;
            Vector3 newViewDir = target - newEye;
            float cosAngle = Vector3.Dot(newViewDir, up) / (newViewDir.Length() * up.Length());
            if (cosAngle < 0.999f && cosAngle > -0.999f)
            {
                eye = eyeLocal + target;
                SetView(eye, target, up);
            }
        }

        public void PanX(int value)
        {
            float scaleFactor = 0.0f;
            if (value > 1)
            {
                scaleFactor = -0.05f;
            }
            else if (value < -1)
            {
                scaleFactor = 0.05f;
            }

            Vector3 viewDir = target - eye;
            Vector3 orhto = Vector3.Cross(viewDir, up);
            orhto.Normalize();
            scaleFactor = scaleFactor * (float)Math.Sqrt(viewDir.Length()) * 0.5f;
            Matrix scaling = Matrix.Scaling(scaleFactor, scaleFactor, scaleFactor);
            orhto = Vector3.TransformCoordinate(orhto, scaling);

            target = target + orhto;
            eye = eye + orhto;
            SetView(eye, target, up);
        }

        public void PanY(int value)
        {
            float scaleFactor = 0.00f;
            if (value > 1)
            {
                scaleFactor = -0.05f;
            }
            else if (value < -1)
            {
                scaleFactor = 0.05f;
            }

            Vector3 viewDir = target - eye;
            scaleFactor = scaleFactor * (float)Math.Sqrt(viewDir.Length()) * 0.5f;
            viewDir.Y = 0.0f;
            viewDir.Normalize();
            Matrix scaling = Matrix.Scaling(scaleFactor, scaleFactor, scaleFactor);
            viewDir = Vector3.TransformCoordinate(viewDir, scaling);

            target = target + viewDir;
            eye = eye + viewDir;
            SetView(eye, target, up);
        }

        public void Zoom(int value)
        {
            Vector3 viewDir = eye - target;

            float scaleFactor = 1.0f;
            if (value > 0)
            {
                scaleFactor = 1.1f;
            }
            else
            {
                if (viewDir.Length() > maxZoom)
                    scaleFactor = 0.9f;
            }

            Matrix scale = Matrix.Scaling(scaleFactor, scaleFactor, scaleFactor);
            viewDir.Normalize();
            viewDir = Vector3.TransformCoordinate(viewDir, scale);
            if (value > 0)
            {
                eye = eye + viewDir;
            }
            else
            {
                eye = eye - viewDir;
            }

            SetView(eye, target, up);
        }

        public override void MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            dragging = false;
        }

        public override void MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            dragging = true;
            startX = e.X;
            startY = e.Y;
        }

        public override void MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (dragging)
            {
                int currentX = e.X;
                deltaX = startX - currentX;
                startX = currentX;

                int currentY = e.Y;
                deltaY = startY - currentY;
                startY = currentY;

                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    this.RotateY(-deltaX);
                    this.RotateOrtho(deltaY);
                }
                else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    this.PanX(deltaX);
                    this.PanY(deltaY);
                }
            }
        }

        public override void MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int delta = e.Delta;

            // AMF - reversed delta
            this.Zoom(-delta);
        }

        public override void KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        public override void KeyDown(object sender, KeyEventArgs e)
        {

        }

        public override void KeyUp(object sender, KeyEventArgs e)
        {

        }

        public override void Update()
        {
        }
    }
}