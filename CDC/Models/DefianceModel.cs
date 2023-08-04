﻿using System;
using System.Collections.Generic;
using System.IO;

namespace CDC
{
	public abstract class DefianceModel : Model
	{
		#region Normals
		protected static float[,] _normals =
		{
			{0.000000f, 0.000000f, 1.000000f},
			{-0.471191f, -0.816406f, -0.333252f},
			{0.942627f, 0.000000f, -0.333252f},
			{-0.471191f, 0.816406f, -0.333252f},
			{-0.086182f, -0.149658f, 0.984863f},
			{-0.170166f, -0.294678f, 0.940186f},
			{-0.248779f, -0.430908f, 0.867188f},
			{-0.320068f, -0.554199f, 0.768066f},
			{-0.381592f, -0.660889f, 0.645752f},
			{-0.431641f, -0.747803f, 0.504150f},
			{-0.468750f, -0.812012f, 0.347412f},
			{-0.491699f, -0.851807f, 0.180176f},
			{-0.499756f, -0.865967f, 0.007324f},
			{-0.492920f, -0.854004f, -0.165283f},
			{0.172607f, 0.000000f, 0.984863f},
			{0.340332f, 0.000000f, 0.940186f},
			{0.497803f, 0.000000f, 0.867188f},
			{0.640137f, 0.000000f, 0.768066f},
			{0.763184f, 0.000000f, 0.645752f},
			{0.863281f, 0.000000f, 0.504150f},
			{0.937500f, 0.000000f, 0.347412f},
			{0.983398f, 0.000000f, 0.180176f},
			{0.999756f, 0.000000f, 0.007324f},
			{0.986084f, 0.000000f, -0.165283f},
			{-0.086182f, 0.149658f, 0.984863f},
			{-0.170166f, 0.294678f, 0.940186f},
			{-0.248779f, 0.430908f, 0.867188f},
			{-0.320068f, 0.554199f, 0.768066f},
			{-0.381592f, 0.660889f, 0.645752f},
			{-0.431641f, 0.747803f, 0.504150f},
			{-0.468750f, 0.812012f, 0.347412f},
			{-0.491699f, 0.851807f, 0.180176f},
			{-0.499756f, 0.865967f, 0.007324f},
			{-0.492920f, 0.854004f, -0.165283f},
			{-0.320068f, -0.854004f, -0.409668f},
			{-0.159424f, -0.865967f, -0.473877f},
			{0.005859f, -0.851807f, -0.523682f},
			{0.171143f, -0.812012f, -0.557861f},
			{0.331543f, -0.747803f, -0.574951f},
			{0.481689f, -0.660889f, -0.574951f},
			{0.617432f, -0.554199f, -0.557861f},
			{0.734619f, -0.430908f, -0.523682f},
			{0.829590f, -0.294678f, -0.473877f},
			{0.899658f, -0.149658f, -0.409668f},
			{0.899658f, 0.149658f, -0.409668f},
			{0.829590f, 0.294678f, -0.473877f},
			{0.734619f, 0.430908f, -0.523682f},
			{0.617432f, 0.554199f, -0.557861f},
			{0.481689f, 0.660889f, -0.574951f},
			{0.331543f, 0.747803f, -0.574951f},
			{0.171143f, 0.812012f, -0.557861f},
			{0.005859f, 0.851807f, -0.523682f},
			{-0.159424f, 0.865967f, -0.473877f},
			{-0.320068f, 0.854004f, -0.409668f},
			{-0.579346f, 0.704346f, -0.409668f},
			{-0.670166f, 0.571045f, -0.473877f},
			{-0.740479f, 0.420654f, -0.523682f},
			{-0.788818f, 0.257568f, -0.557861f},
			{-0.813232f, 0.086670f, -0.574951f},
			{-0.813232f, -0.086670f, -0.574951f},
			{-0.788818f, -0.257568f, -0.557861f},
			{-0.740479f, -0.420654f, -0.523682f},
			{-0.670166f, -0.571045f, -0.473877f},
			{-0.579346f, -0.704346f, -0.409668f},
			{0.088867f, -0.154053f, 0.983887f},
			{0.008057f, -0.310059f, 0.950439f},
			{0.264404f, -0.162109f, 0.950439f},
			{-0.066650f, -0.463623f, 0.883301f},
			{0.192139f, -0.333008f, 0.922852f},
			{0.434814f, -0.173828f, 0.883301f},
			{-0.132813f, -0.609619f, 0.781250f},
			{0.126953f, -0.507813f, 0.851807f},
			{0.376221f, -0.363770f, 0.851807f},
			{0.594482f, -0.189697f, 0.781250f},
			{-0.187256f, -0.742920f, 0.642334f},
			{0.071533f, -0.679932f, 0.729736f},
			{0.324951f, -0.562988f, 0.759521f},
			{0.552979f, -0.401855f, 0.729736f},
			{0.737061f, -0.209229f, 0.642334f},
			{-0.229248f, -0.855469f, 0.464111f},
			{0.026855f, -0.836426f, 0.546875f},
			{0.281006f, -0.756836f, 0.589844f},
			{0.514648f, -0.621826f, 0.589844f},
			{0.710938f, -0.441406f, 0.546875f},
			{0.855469f, -0.229004f, 0.464111f},
			{-0.260498f, -0.932861f, 0.248291f},
			{-0.012695f, -0.953613f, 0.300293f},
			{0.235840f, -0.912842f, 0.333008f},
			{0.469238f, -0.812988f, 0.343994f},
			{0.672607f, -0.660645f, 0.333008f},
			{0.832275f, -0.465576f, 0.300293f},
			{0.938232f, -0.240479f, 0.248291f},
			{-0.286621f, -0.957764f, 0.010254f},
			{-0.058105f, -0.998047f, 0.012451f},
			{0.173584f, -0.984619f, 0.014160f},
			{0.395996f, -0.917969f, 0.014893f},
			{0.596924f, -0.802002f, 0.014893f},
			{0.765869f, -0.642578f, 0.014160f},
			{0.893555f, -0.448730f, 0.012451f},
			{0.972900f, -0.230469f, 0.010254f},
			{-0.308838f, -0.925781f, -0.217041f},
			{-0.111572f, -0.958984f, -0.259766f},
			{0.089844f, -0.952148f, -0.291504f},
			{0.287842f, -0.905518f, -0.311279f},
			{0.473877f, -0.821045f, -0.317871f},
			{0.640137f, -0.702148f, -0.311279f},
			{0.779541f, -0.553955f, -0.291504f},
			{0.886475f, -0.382568f, -0.259766f},
			{0.956299f, -0.195313f, -0.217041f},
			{0.088867f, 0.154053f, 0.983887f},
			{0.264404f, 0.162109f, 0.950439f},
			{0.008057f, 0.310059f, 0.950439f},
			{0.434814f, 0.173828f, 0.883301f},
			{0.192139f, 0.333008f, 0.922852f},
			{-0.066650f, 0.463623f, 0.883301f},
			{0.594482f, 0.189697f, 0.781250f},
			{0.376221f, 0.363770f, 0.851807f},
			{0.126953f, 0.507813f, 0.851807f},
			{-0.132813f, 0.609619f, 0.781250f},
			{0.737061f, 0.209229f, 0.642334f},
			{0.552979f, 0.401855f, 0.729736f},
			{0.324951f, 0.562988f, 0.759521f},
			{0.071533f, 0.679932f, 0.729736f},
			{-0.187256f, 0.742920f, 0.642334f},
			{0.855469f, 0.229004f, 0.464111f},
			{0.710938f, 0.441406f, 0.546875f},
			{0.514648f, 0.621826f, 0.589844f},
			{0.281006f, 0.756836f, 0.589844f},
			{0.026855f, 0.836426f, 0.546875f},
			{-0.229248f, 0.855469f, 0.464111f},
			{0.938232f, 0.240479f, 0.248291f},
			{0.832275f, 0.465576f, 0.300293f},
			{0.672607f, 0.660645f, 0.333008f},
			{0.469238f, 0.812988f, 0.343994f},
			{0.235840f, 0.912842f, 0.333008f},
			{-0.012695f, 0.953613f, 0.300293f},
			{-0.260498f, 0.932861f, 0.248291f},
			{0.972900f, 0.230469f, 0.010254f},
			{0.893555f, 0.448730f, 0.012451f},
			{0.765869f, 0.642578f, 0.014160f},
			{0.596924f, 0.802002f, 0.014893f},
			{0.395996f, 0.917969f, 0.014893f},
			{0.173584f, 0.984619f, 0.014160f},
			{-0.058105f, 0.998047f, 0.012451f},
			{-0.286621f, 0.957764f, 0.010254f},
			{0.956299f, 0.195313f, -0.217041f},
			{0.886475f, 0.382568f, -0.259766f},
			{0.779541f, 0.553955f, -0.291504f},
			{0.640137f, 0.702148f, -0.311279f},
			{0.473877f, 0.821045f, -0.317871f},
			{0.287842f, 0.905518f, -0.311279f},
			{0.089844f, 0.952148f, -0.291504f},
			{-0.111572f, 0.958984f, -0.259766f},
			{-0.308838f, 0.925781f, -0.217041f},
			{-0.177979f, 0.000000f, 0.983887f},
			{-0.272705f, 0.147949f, 0.950439f},
			{-0.272705f, -0.147949f, 0.950439f},
			{-0.367920f, 0.289551f, 0.883301f},
			{-0.384521f, 0.000000f, 0.922852f},
			{-0.367920f, -0.289551f, 0.883301f},
			{-0.461426f, 0.419678f, 0.781250f},
			{-0.503174f, 0.143799f, 0.851807f},
			{-0.503174f, -0.143799f, 0.851807f},
			{-0.461426f, -0.419678f, 0.781250f},
			{-0.549805f, 0.533691f, 0.642334f},
			{-0.624512f, 0.277832f, 0.729736f},
			{-0.650146f, 0.000000f, 0.759521f},
			{-0.624512f, -0.277832f, 0.729736f},
			{-0.549805f, -0.533691f, 0.642334f},
			{-0.626221f, 0.626221f, 0.464111f},
			{-0.737793f, 0.395020f, 0.546875f},
			{-0.795898f, 0.134766f, 0.589844f},
			{-0.795898f, -0.134766f, 0.589844f},
			{-0.737793f, -0.395020f, 0.546875f},
			{-0.626221f, -0.626221f, 0.464111f},
			{-0.677490f, 0.692139f, 0.248291f},
			{-0.819336f, 0.487793f, 0.300293f},
			{-0.908447f, 0.251953f, 0.333008f},
			{-0.938721f, 0.000000f, 0.343994f},
			{-0.908447f, -0.251953f, 0.333008f},
			{-0.819336f, -0.487793f, 0.300293f},
			{-0.677490f, -0.692139f, 0.248291f},
			{-0.686035f, 0.727295f, 0.010254f},
			{-0.835205f, 0.549316f, 0.012451f},
			{-0.939453f, 0.341797f, 0.014160f},
			{-0.992920f, 0.115967f, 0.014893f},
			{-0.992920f, -0.115967f, 0.014893f},
			{-0.939453f, -0.341797f, 0.014160f},
			{-0.835205f, -0.549316f, 0.012451f},
			{-0.686035f, -0.727295f, 0.010254f},
			{-0.647461f, 0.730469f, -0.217041f},
			{-0.774658f, 0.576172f, -0.259766f},
			{-0.869629f, 0.397949f, -0.291504f},
			{-0.928223f, 0.203125f, -0.311279f},
			{-0.947998f, 0.000000f, -0.317871f},
			{-0.928223f, -0.203125f, -0.311279f},
			{-0.869629f, -0.397949f, -0.291504f},
			{-0.774658f, -0.576172f, -0.259766f},
			{-0.647461f, -0.730469f, -0.217041f},
			{-0.434082f, -0.751953f, -0.495850f},
			{-0.530762f, -0.623291f, -0.573975f},
			{-0.274414f, -0.771240f, -0.573975f},
			{-0.605957f, -0.470215f, -0.641357f},
			{-0.370850f, -0.642578f, -0.670166f},
			{-0.104248f, -0.759766f, -0.641357f},
			{-0.655029f, -0.294678f, -0.695557f},
			{-0.442383f, -0.478271f, -0.758545f},
			{-0.192871f, -0.622070f, -0.758545f},
			{0.072021f, -0.714600f, -0.695557f},
			{-0.673340f, -0.098633f, -0.732422f},
			{-0.480469f, -0.276367f, -0.832031f},
			{-0.249512f, -0.432373f, -0.866211f},
			{0.000732f, -0.554199f, -0.832031f},
			{0.250977f, -0.632568f, -0.732422f},
			{-0.656738f, 0.114746f, -0.745117f},
			{-0.476807f, -0.035889f, -0.877930f},
			{-0.262207f, -0.184326f, -0.947021f},
			{-0.028564f, -0.319336f, -0.947021f},
			{0.207031f, -0.431152f, -0.877930f},
			{0.427979f, -0.511230f, -0.745117f},
			{-0.603516f, 0.338867f, -0.721436f},
			{-0.427490f, 0.235107f, -0.872803f},
			{-0.223877f, 0.116211f, -0.967529f},
			{-0.005615f, -0.009766f, -0.999756f},
			{0.212646f, -0.135498f, -0.967529f},
			{0.417480f, -0.252441f, -0.872803f},
			{0.595215f, -0.353271f, -0.721436f},
			{-0.520264f, 0.553223f, -0.650391f},
			{-0.342529f, 0.505371f, -0.791748f},
			{-0.146240f, 0.430420f, -0.890381f},
			{0.057861f, 0.332275f, -0.941162f},
			{0.258789f, 0.216309f, -0.941162f},
			{0.446045f, 0.088623f, -0.890381f},
			{0.609131f, -0.043701f, -0.791748f},
			{0.739258f, -0.173828f, -0.650391f},
			{-0.422119f, 0.729248f, -0.537842f},
			{-0.247314f, 0.723877f, -0.643799f},
			{-0.062256f, 0.688232f, -0.722656f},
			{0.125244f, 0.623779f, -0.771240f},
			{0.307861f, 0.533203f, -0.787842f},
			{0.477539f, 0.420410f, -0.771240f},
			{0.627197f, 0.290039f, -0.722656f},
			{0.750732f, 0.147461f, -0.643799f},
			{0.842773f, -0.000977f, -0.537842f},
			{0.577393f, 0.577393f, 0.577393f},
			{0.577393f, 0.577393f, -0.577393f},
			{0.577393f, -0.577393f, 0.577393f},
			{0.577393f, -0.577393f, -0.577393f},
			{-0.577393f, 0.577393f, 0.577393f},
			{-0.577393f, 0.577393f, -0.577393f},
			{-0.577393f, -0.577393f, 0.577393f},
			{-0.577393f, -0.577393f, -0.577393f},
		};
		#endregion

		protected DataFile _dataFile;
		protected string _name;
		protected string _modelTypePrefix;
		protected uint _version;
		protected Platform _platform;
		protected uint _dataStart;
		protected uint _modelData;
		protected uint _vertexCount;
		protected uint _vertexStart;
		protected uint _extraVertexCount;
		protected uint _extraVertexStart;
		protected uint _normalCount;
		protected uint _normalStart;
		protected uint _extraNormalCount;
		protected uint _extraNormalStart;
		protected uint _polygonCount;
		protected uint _polygonStart;
		protected uint _boneCount;
		protected uint _boneStart;
		protected uint _groupCount;
		protected uint _materialCount;
		protected uint _materialStart;
		protected uint _indexCount { get { return 3 * _polygonCount; } }
		// Vertices are scaled before any bones are applied.
		// Scaling afterwards will break the characters.
		protected Vector _vertexScale;
		protected Geometry _geometry;
		protected Geometry _extraGeometry;
		protected Polygon[] _polygons;
		protected Bone[] _bones;
		protected Tree[] _trees;
		protected Material[] _materials;
		protected List<Material> _materialsList;

		public override string Name { get { return _name; } }
		public override string ModelTypePrefix { get { return _modelTypePrefix; } }
		public override Polygon[] Polygons { get { return _polygons; } }
		public override Geometry Geometry { get { return _geometry; } }
		public override Geometry ExtraGeometry { get { return _extraGeometry; } }
		public override Bone[] Bones { get { return _bones; } }
		public override Tree[] Groups { get { return _trees; } }
		public override Material[] Materials { get { return _materials; } }
		public override Platform Platform { get { return _platform; } }

		protected class DefianceTriangleList
		{
			public Material material;
			public UInt32 polygonCount;
			public UInt32 polygonStart;
			public UInt16 groupID;
			public UInt32 next;
		}

		protected DefianceModel(BinaryReader reader, DataFile dataFile, UInt32 dataStart, UInt32 modelData, String modelName, Platform ePlatform, UInt32 version)
		{
			_dataFile = dataFile;
			_name = modelName;
			_modelTypePrefix = "";
			_platform = ePlatform;
			_version = version;
			_dataStart = dataStart;
			_modelData = modelData;
			_vertexCount = 0;
			_vertexStart = 0;
			_extraVertexCount = 0;
			_extraVertexStart = 0;
			_normalCount = 0;
			_normalStart = 0;
			_extraNormalCount = 0;
			_extraNormalStart = 0;
			_polygonCount = 0;
			_polygonStart = 0;
			_vertexScale.x = 1.0f;
			_vertexScale.y = 1.0f;
			_vertexScale.z = 1.0f;
			_geometry = new Geometry();
			_extraGeometry = new Geometry();
			_materialsList = new List<Material>();
		}

		public virtual void ReadData(BinaryReader reader, ExportOptions options)
		{
			// Get the vertices
			_geometry.Vertices = new Vertex[_vertexCount];
			_geometry.PositionsRaw = new Vector[_vertexCount];
			_geometry.PositionsPhys = new Vector[_vertexCount];
			_geometry.PositionsAltPhys = new Vector[_vertexCount];
			_geometry.Colours = new UInt32[_vertexCount];
			_geometry.ColoursAlt = new UInt32[_vertexCount];
			_geometry.UVs = new UV[_vertexCount];
			ReadTypeAVertices(reader, options);

			// Get the extra vertices
			_extraGeometry.Vertices = new Vertex[_extraVertexCount];
			_extraGeometry.PositionsRaw = new Vector[_extraVertexCount];
			_extraGeometry.PositionsPhys = new Vector[_extraVertexCount];
			_extraGeometry.PositionsAltPhys = new Vector[_extraVertexCount];
			_extraGeometry.Colours = new UInt32[_extraVertexCount];
			_extraGeometry.ColoursAlt = new UInt32[_extraVertexCount];
			_extraGeometry.UVs = new UV[_extraVertexCount];
			ReadTypeBVertices(reader, options);

			// Get the normals
			_geometry.VertexNormals = new Vector[_normals.Length / 3];
			_geometry.PolygonNormals = new Vector[_normalCount];
			ReadTypeANormals(reader, options);

			// Get the extra normals
			_extraGeometry.VertexNormals = new Vector[_normals.Length / 3];
			_extraGeometry.PolygonNormals = new Vector[_extraNormalCount];
			ReadTypeBNormals(reader, options);

			// Get the polygons
			_polygons = new Polygon[_polygonCount];
			ReadPolygons(reader, options);

			for (uint p = 0; p < _polygonCount; p++)
			{
				HandleDebugRendering((int)p, options);
			}

			// Generate the output
			GenerateOutput();
		}

		protected virtual void ReadTypeAVertex(BinaryReader reader, int v, ExportOptions options)
		{
			_geometry.Vertices[v].positionID = v;

			_geometry.PositionsRaw[v].x = (float)reader.ReadInt16();
			_geometry.PositionsRaw[v].y = (float)reader.ReadInt16();
			_geometry.PositionsRaw[v].z = (float)reader.ReadInt16();
			reader.BaseStream.Position += 0x02;
		}

		protected virtual void ReadTypeAVertices(BinaryReader reader, ExportOptions options)
		{
			if (_vertexStart == 0 || _vertexCount == 0)
			{
				return;
			}

			reader.BaseStream.Position = _vertexStart;

			for (int v = 0; v < _vertexCount; v++)
			{
				ReadTypeAVertex(reader, v, options);
			}
		}

		protected virtual void ReadTypeBVertex(BinaryReader reader, int v, ExportOptions options)
		{
			_extraGeometry.Vertices[v].positionID = v;

			_extraGeometry.PositionsRaw[v].x = (float)reader.ReadInt16();
			_extraGeometry.PositionsRaw[v].y = (float)reader.ReadInt16();
			_extraGeometry.PositionsRaw[v].z = (float)reader.ReadInt16();
			reader.BaseStream.Position += 0x02;
		}

		protected virtual void ReadTypeBVertices(BinaryReader reader, ExportOptions options)
		{
			if (_extraVertexStart == 0 || _extraVertexCount == 0)
			{
				return;
			}

			reader.BaseStream.Position = _extraVertexStart;

			for (int v = 0; v < _extraVertexCount; v++)
			{
				ReadTypeBVertex(reader, v, options);
			}
		}

		protected virtual void ReadTypeANormal(BinaryReader reader, int n, ExportOptions options)
		{
			_geometry.PolygonNormals[n].x = reader.ReadInt16();
			_geometry.PolygonNormals[n].y = reader.ReadInt16();
			_geometry.PolygonNormals[n].z = reader.ReadInt16();
		}

		protected virtual void ReadTypeANormals(BinaryReader reader, ExportOptions options)
		{
			for (int n = 0; n < _geometry.VertexNormals.Length; n++)
			{
				_geometry.VertexNormals[n].x = _normals[n, 0];
				_geometry.VertexNormals[n].y = _normals[n, 1];
				_geometry.VertexNormals[n].z = _normals[n, 2];
			}

			if (_normalStart == 0 || _normalCount == 0)
			{
				return;
			}

			reader.BaseStream.Position = _normalStart;

			for (int v = 0; v < _normalCount; v++)
			{
				ReadTypeANormal(reader, v, options);
			}
		}

		protected virtual void ReadTypeBNormal(BinaryReader reader, int n, ExportOptions options)
		{
			_extraGeometry.PolygonNormals[n].x = reader.ReadInt16();
			_extraGeometry.PolygonNormals[n].y = reader.ReadInt16();
			_extraGeometry.PolygonNormals[n].z = reader.ReadInt16();
		}

		protected virtual void ReadTypeBNormals(BinaryReader reader, ExportOptions options)
		{
			for (int n = 0; n < _extraGeometry.VertexNormals.Length; n++)
			{
				_extraGeometry.VertexNormals[n].x = _normals[n, 0];
				_extraGeometry.VertexNormals[n].y = _normals[n, 1];
				_extraGeometry.VertexNormals[n].z = _normals[n, 2];
			}

			if (_normalStart == 0 || _normalCount == 0)
			{
				return;
			}

			reader.BaseStream.Position = _normalStart;

			for (int v = 0; v < _normalCount; v++)
			{
				ReadTypeBNormal(reader, v, options);
			}
		}

		protected abstract void ReadPolygons(BinaryReader reader, ExportOptions options);

		protected virtual void GenerateOutput()
		{
			// Make the vertices unique
			_geometry.Vertices = new Vertex[_indexCount];
			for (uint p = 0; p < _polygonCount; p++)
			{
				_geometry.Vertices[(3 * p) + 0] = _polygons[p].v1;
				_geometry.Vertices[(3 * p) + 1] = _polygons[p].v2;
				_geometry.Vertices[(3 * p) + 2] = _polygons[p].v3;
			}

			// Build the materials array
			_materials = new Material[_materialCount];
			UInt16 mNew = 0;

			foreach (Material xMaterial in _materialsList)
			{
				_materials[mNew] = xMaterial;
				_materials[mNew].ID = mNew;
				mNew++;
			}

			return;
		}

		public override string GetTextureName(int materialIndex, ExportOptions options)
		{
			string textureName = "";
			if (materialIndex >= 0 && materialIndex < _materials.Length)
			{
				Material material = _materials[materialIndex];
				if (material.textureUsed)
				{
					textureName = Utility.GetPS2TextureName(_dataFile.Name, material.textureID);
				}
			}

			return textureName;
		}
	}
}
