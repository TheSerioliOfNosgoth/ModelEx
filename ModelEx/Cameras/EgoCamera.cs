using System;
using System.Collections.Generic;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace ModelEx
{
    public class EgoCamera : DynamicCamera
    {
        Vector3 look;

        bool strafingLeft = false;
        bool strafingRight = false;
        bool movingForward = false;
        bool movingBack = false;

        float pitchVal = 0.0f;
        float moveSpeed = 10.0f;

        public EgoCamera()
        {
            look = new Vector3(1, 0, 0);
            up = new Vector3(0, 1, 0);
            eye = new Vector3(0, 1, 0);
            target = eye + look;

            view = Matrix.LookAtLH(eye, target, up);
            perspective = Matrix.PerspectiveFovLH((float)Math.PI / 4, 1.0f, 0.1f, 1000.0f);
        }

        public void Yaw(int x)
        {
            Matrix rot = Matrix.RotationY(x / 100.0f);
            look = Vector3.TransformCoordinate(look, rot);

            target = eye + look;
            view = Matrix.LookAtLH(eye, target, up);
        }

        public void Pitch(int y)
        {
            Vector3 axis = Vector3.Cross(up, look);
            float rotation = y / 100.0f;
            pitchVal = pitchVal + rotation;

            float halfPi = (float)Math.PI / 2.0f;

            if (pitchVal < -halfPi)
            {
                pitchVal = -halfPi;
                rotation = 0;
            }
            if (pitchVal > halfPi)
            {
                pitchVal = halfPi;
                rotation = 0;
            }

            Matrix rot = Matrix.RotationAxis(axis, rotation);

            look = Vector3.TransformCoordinate(look, rot);

            look.Normalize();

            target = eye + look;
            view = Matrix.LookAtLH(eye, target, up);
        }

        public void Strafe(int val)
        {
            Vector3 axis = Vector3.Cross(look, up);
            // AMF - Added Deltatime
            //Matrix scale = Matrix.Scaling(0.1f, 0.1f, 0.1f);
            //axis = Vector3.TransformCoordinate(axis, scale);
            axis *= Timer.Instance.DeltaTime * moveSpeed;

            if (val > 0)
            {
                eye = eye + axis;
            }
            else
            {
                eye = eye - axis;
            }

            target = eye + look;
            view = Matrix.LookAtLH(eye, target, up);
        }

        public void Move(int val)
        {
            Vector3 tempLook = look;
            // AMF - Added Deltatime
            //Matrix scale = Matrix.Scaling(0.1f, 0.1f, 0.1f);
            //tempLook = Vector3.TransformCoordinate(tempLook, scale);
            tempLook *= Timer.Instance.DeltaTime * moveSpeed;

            if (val > 0)
            {
                eye = eye + tempLook;
            }
            else
            {
                eye = eye - tempLook;
            }

            target = eye + look;
            view = Matrix.LookAtLH(eye, target, up);
        }

        public override void SetView(Vector3 eye, Vector3 target, Vector3 up)
        {
            this.look = target - eye;
            this.eye = eye;
            this.target = target;
            this.up = up;
            view = Matrix.LookAtLH(eye, target, up);

            Vector3 dir = look;
            dir.Normalize();
            pitchVal = (float)System.Math.Asin(dir.Y);
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
                    // AMF - reversed deltaY
                    Pitch(-deltaY);
                    Yaw(-deltaX);
                }
            }
        }

        public override void MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
        }

        public override void KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
        }

        public override void KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.W)
            {
                movingForward = true;
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.S)
            {
                movingBack = true;
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.A)
            {
                strafingLeft = true;
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.D)
            {
                strafingRight = true;
            }
        }

        public override void KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.W)
            {
                movingForward = false;
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.S)
            {
                movingBack = false;
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.A)
            {
                strafingLeft = false;
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.D)
            {
                strafingRight = false;
            }
        }

        public override void Update()
        {
            if (strafingLeft)
            {
                Strafe(1);
            }

            if (strafingRight)
            {
                Strafe(-1);
            }

            if (movingForward)
            {
                Move(1);
            }

            if (movingBack)
            {
                Move(-1);
            }
        }
    }
}