using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D11;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace ModelEx
{
	public abstract class MeshIndexed : Mesh
	{
		protected Buffer _indexBuffer;
		protected int _indexStride = 0;
		protected int _numIndices = 0;
		protected int _indexBufferSizeInBytes = 0;
		protected DataStream _indices;

		protected MeshIndexed(RenderResource resource)
			: base(resource)
		{
		}

		protected void SetIndexBuffer<V, I>(IMeshParserIndexed<V, I> indexParser) where I : struct
		{
			_numIndices = indexParser.IndexCount;
			_indexStride = Marshal.SizeOf(typeof(I));
			_indexBufferSizeInBytes = _numIndices * _indexStride;

			_indices = new DataStream(_indexBufferSizeInBytes, true, true);
			for (int i = 0; i < _numIndices; i++)
			{
				indexParser.FillIndex(i, out I index);
				_indices.Write(index);
			}

			_indices.Position = 0;

			_indexBuffer = new Buffer(
				DeviceManager.Instance.device,
				_indices,
				_indexBufferSizeInBytes,
				ResourceUsage.Default,
				BindFlags.IndexBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0);
		}

		public override void Dispose()
		{
			_indexBuffer?.Dispose();

			base.Dispose();
		}
	}
}