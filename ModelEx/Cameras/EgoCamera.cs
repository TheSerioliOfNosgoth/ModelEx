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
		bool movingDown = false;
		bool movingUp = false;

		float pitchVal = 0.0f;
		float moveSpeed = 10.0f;

		public EgoCamera()
		{
			look = new Vector3(1, 0, 0);
			up = new Vector3(0, 1, 0);
			eye = new Vector3(0, 1, 0);
			target = eye + look;

			View = Matrix.LookAtLH(eye, target, up);
		}

		public void Yaw(int x)
		{
			Matrix rot = Matrix.RotationY(x / 100.0f);
			look = Vector3.TransformCoordinate(look, rot);

			target = eye + look;
			View = Matrix.LookAtLH(eye, target, up);
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
			View = Matrix.LookAtLH(eye, target, up);
		}

		public void Strafe(int val)
		{
			Vector3 axis = Vector3.Cross(look, up);
			// find the vector which is perpendicular to the current look direction, on the horizontal plane relative to the observer
			//Vector3 axis = Vector3.TransformCoordinate(look, Matrix.RotationY(0.5f * (float)Math.PI));
			// AMF - Added Deltatime
			//Matrix scale = Matrix.Scaling(0.1f, 0.1f, 0.1f);
			//axis = Vector3.TransformCoordinate(axis, scale);
			axis *= Timer.Instance.DeltaTime * moveSpeed;

			if (val > 0)
			{
				eye += axis;
			}
			else
			{
				eye -= axis;
			}

			target = eye + look;
			View = Matrix.LookAtLH(eye, target, up);
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
				eye += tempLook;
			}
			else
			{
				eye -= tempLook;
			}

			target = eye + look;
			View = Matrix.LookAtLH(eye, target, up);
		}

		public void MoveVertically(int val)
		{
			Vector3 vertical = up;
			// find the vector which is perpendicular to the current look direction, on the vertical plane relative to the observer
			//Vector3 vertical = Vector3.TransformCoordinate(look, Matrix.RotationZ(0.5f * (float)Math.PI));
			// AMF - Added Deltatime
			//Matrix scale = Matrix.Scaling(0.1f, 0.1f, 0.1f);
			//axis = Vector3.TransformCoordinate(axis, scale);
			vertical *= Timer.Instance.DeltaTime * moveSpeed;

			if (val > 0)
			{
				eye += vertical;
			}
			else
			{
				eye -= vertical;
			}

			target = eye + look;
			View = Matrix.LookAtLH(eye, target, up);
		}

		public override void SetView(Vector3 eye, Vector3 target, Vector3 up)
		{
			this.look = target - eye;
			this.eye = eye;
			this.target = target;
			this.up = up;
			View = Matrix.LookAtLH(eye, target, up);

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
			else if (e.KeyCode == System.Windows.Forms.Keys.Q)
			{
				movingDown = true;
			}
			else if (e.KeyCode == System.Windows.Forms.Keys.E)
			{
				movingUp = true;
			}
			else if (e.KeyCode == System.Windows.Forms.Keys.Add)
			{
				moveSpeed *= 10.0f;
				if (moveSpeed > 10000.0f)
				{
					moveSpeed = 10000.0f;
				}
			}
			else if (e.KeyCode == System.Windows.Forms.Keys.Subtract)
			{
				moveSpeed /= 10.0f;
				if (moveSpeed < 1.0f)
				{
					moveSpeed = 1.0f;
				}
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
			else if (e.KeyCode == System.Windows.Forms.Keys.Q)
			{
				movingDown = false;
			}
			else if (e.KeyCode == System.Windows.Forms.Keys.E)
			{
				movingUp = false;
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

			if (movingUp)
			{
				MoveVertically(1);
			}

			if (movingDown)
			{
				MoveVertically(-1);
			}
		}
	}
}