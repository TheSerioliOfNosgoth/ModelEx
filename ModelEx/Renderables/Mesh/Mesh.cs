using System;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D11;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace ModelEx
{
	public abstract class Mesh
	{
		protected RenderResource _renderResource;

		protected Buffer _vertexBuffer;
		protected DataStream _vertices;

		protected int _vertexStride = 0;
		protected int _numVertices = 0;

		public BoundingBox BoundingBox { get; protected set; }
		public BoundingSphere BoundingSphere { get; protected set; }

		public String Name { get; protected set; } = "";

		protected Mesh(RenderResource resource)
		{
			_renderResource = resource;
		}

		protected Vector4 ColorToVector4(System.Drawing.Color color)
		{
			return new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
		}

		protected Color4 ColorToColor4(System.Drawing.Color color)
		{
			return new Color4(1, color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
		}

		protected void SetVertexBuffer<V>(IMeshParser<V> vertexParser) where V : struct
		{
			_numVertices = vertexParser.VertexCount;
			_vertexStride = Marshal.SizeOf(typeof(V));
			int SizeOfVertexBufferInBytes = _numVertices * _vertexStride;

			_vertices = new DataStream(SizeOfVertexBufferInBytes, true, true);
			for (int i = 0; i < _numVertices; i++)
			{
				vertexParser.FillVertex(i, out V vertex);
				_vertices.Write(vertex);
			}

			_vertices.Position = 0;

			BoundingBox = BoundingBox.FromPoints(_vertices, _numVertices, _vertexStride);
			BoundingSphere = BoundingSphere.FromBox(BoundingBox);

			_vertexBuffer = new Buffer(DeviceManager.Instance.device,
				_vertices,
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

		public virtual void Dispose()
		{
			_vertexBuffer?.Dispose();
		}
	}
}