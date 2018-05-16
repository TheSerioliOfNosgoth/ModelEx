using SlimDX;

namespace ModelEx
{
    public abstract class Renderable
    {
        public abstract void Render();
        public abstract void Dispose();

        public virtual string Name { get; set; }
        public virtual Matrix Transform { get; set; } = Matrix.Identity;
    }
}
