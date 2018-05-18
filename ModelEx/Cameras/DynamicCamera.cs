using System;
using System.Windows.Forms;
using SlimDX;

namespace ModelEx
{
    public abstract class DynamicCamera : Camera
    {
        protected bool dragging = false;
        protected int startX = 0;
        protected int deltaX = 0;

        protected int startY = 0;
        protected int deltaY = 0;

        public abstract void MouseUp(object sender, MouseEventArgs e);
        public abstract void MouseDown(object sender, MouseEventArgs e);
        public abstract void MouseMove(object sender, MouseEventArgs e);
        public abstract void MouseWheel(object sender, MouseEventArgs e);

        public abstract void KeyPress(object sender, KeyPressEventArgs e);
        public abstract void KeyDown(object sender, KeyEventArgs e);
        public abstract void KeyUp(object sender, KeyEventArgs e);

        public abstract void Update();
    }
}