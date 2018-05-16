using System;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D11;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace ModelEx
{
    public abstract class Mesh
    {
        protected Buffer vertexBuffer;
        protected DataStream vertices;

        protected int vertexStride;
        protected int numVertices;

        protected Buffer indexBuffer;
        protected int indexStride = 0;
        protected int numIndices = 0;
        protected int indexBufferSizeInBytes = 0;
        protected DataStream indices;

        public String Name = "";

        protected Vector4 ColorToVector4(System.Drawing.Color color)
        {
            return new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
        }

        protected Color4 ColorToColor4(System.Drawing.Color color)
        {
            return new Color4(1, color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
        }

        protected void SetIndexBuffer<I>(IIndexParser<I> indexParser) where I : struct
        {
            numIndices = indexParser.IndexCount;
            indexStride = Marshal.SizeOf(typeof(I));
            indexBufferSizeInBytes = numIndices * indexStride;

            indices = new DataStream(indexBufferSizeInBytes, true, true);
            for (int i = 0; i < numIndices; i++)
            {
                indexParser.FillIndex(i, out I index);
                indices.Write(index);
            }

            indices.Position = 0;

            indexBuffer = new Buffer(
                DeviceManager.Instance.device,
                indices,
                indexBufferSizeInBytes,
                ResourceUsage.Default,
                BindFlags.IndexBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None,
                0);
        }

        protected void SetVertexBuffer<V>(IVertexParser<V> vertexParser) where V : struct
        {
            numVertices = vertexParser.VertexCount;
            vertexStride = Marshal.SizeOf(typeof(V));
            int SizeOfVertexBufferInBytes = numVertices * vertexStride;

            vertices = new DataStream(SizeOfVertexBufferInBytes, true, true);
            for (int i = 0; i < numVertices; i++)
            {
                vertexParser.FillVertex(i, out V vertex);
                vertices.Write(vertex);
            }

            vertices.Position = 0;

            vertexBuffer = new Buffer(DeviceManager.Instance.device,
                vertices,
                SizeOfVertexBufferInBytes,
                ResourceUsage.Default,
                BindFlags.VertexBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None,
                0);
        }

        public abstract void ApplyBuffers();

        public abstract void ApplyMaterial(Material material);

        public abstract void ApplyTransform(Matrix transform);

        public abstract void Render(int indexCount, int startIndexLocation, int baseVertexLocation);

        public void Dispose()
        {
            indexBuffer?.Dispose();
            vertexBuffer?.Dispose();
        }
    }
}