﻿using System;
using System.Collections.Generic;
using System.IO;

namespace CDC
{
	public abstract class SR2Model : Model
	{
		#region Normals
		protected static Int32[,] _normals =
		{
			{0, 0, 1166016512},
			{-990822400, -984547328, -995450880},
			{1165053952, 0, -995450880},
			{-990822400, 1162936320, -995450880},
			{-1011843072, -1004978176, 1165762560},
			{-1003601920, -996745216, 1165012992},
			{-998326272, -992174080, 1163788288},
			{-995893248, -988946432, 1162125312},
			{-993828864, -987156480, 1160073216},
			{-992149504, -985698304, 1157697536},
			{-990904320, -984621056, 1152507904},
			{-990134272, -983953408, 1144553472},
			{-989863936, -983715840, 1106247680},
			{-990093312, -983916544, -1003929600},
			{1144045568, 0, 1165762560},
			{1152270336, 0, 1165012992},
			{1157554176, 0, 1163788288},
			{1159979008, 0, 1162125312},
			{1162043392, 0, 1160073216},
			{1163722752, 0, 1157697536},
			{1164967936, 0, 1152507904},
			{1165737984, 0, 1144553472},
			{1166012416, 0, 1106247680},
			{1165783040, 0, -1003929600},
			{-1011843072, 1142505472, 1165762560},
			{-1003601920, 1150738432, 1165012992},
			{-998326272, 1155309568, 1163788288},
			{-995893248, 1158537216, 1162125312},
			{-993828864, 1160327168, 1160073216},
			{-992149504, 1161785344, 1157697536},
			{-990904320, 1162862592, 1152507904},
			{-990134272, 1163530240, 1144553472},
			{-989863936, 1163767808, 1106247680},
			{-990093312, 1163567104, -1003929600},
			{-995893248, -983916544, -992886784},
			{-1004322816, -983715840, -990732288},
			{1103101952, -983953408, -989458432},
			{1143947264, -984621056, -988884992},
			{1151975424, -985698304, -988598272},
			{1157013504, -987156480, -988598272},
			{1159598080, -988946432, -988884992},
			{1161564160, -992174080, -989458432},
			{1163157504, -996745216, -990732288},
			{1164333056, -1004978176, -992886784},
			{1164333056, 1142505472, -992886784},
			{1163157504, 1150738432, -990732288},
			{1161564160, 1155309568, -989458432},
			{1159598080, 1158537216, -988884992},
			{1157013504, 1160327168, -988598272},
			{1151975424, 1161785344, -988598272},
			{1143947264, 1162862592, -988884992},
			{1103101952, 1163530240, -989458432},
			{-1004322816, 1163767808, -990732288},
			{-995893248, 1163567104, -992886784},
			{-988524544, 1161056256, -992886784},
			{-987000832, 1158819840, -990732288},
			{-985821184, 1154965504, -989458432},
			{-985010176, 1149493248, -988884992},
			{-984600576, 1135706112, -988598272},
			{-984600576, -1011777536, -988598272},
			{-985010176, -997990400, -988884992},
			{-985821184, -992518144, -989458432},
			{-987000832, -988663808, -990732288},
			{-988524544, -986427392, -992886784},
			{1136001024, -1004683264, 1165746176},
			{1107558400, -996229120, 1165185024},
			{1149722624, -1004142592, 1165185024},
			{-1014464512, -991076352, 1164058624},
			{1145356288, -995459072, 1164722176},
			{1155440640, -1003356160, 1164058624},
			{-1006108672, -988016640, 1162346496},
			{1140981760, -989724672, 1163530240},
			{1153474560, -994426880, 1163530240},
			{1159213056, -1002291200, 1162346496},
			{-1002455040, -985780224, 1160015872},
			{1133674496, -986836992, 1161482240},
			{1151754240, -988798976, 1161981952},
			{1158516736, -993148928, 1161482240},
			{1161605120, -1000980480, 1160015872},
			{-999636992, -983891968, 1156423680},
			{1121714176, -984211456, 1158414336},
			{1150279680, -985546752, 1159135232},
			{1157873664, -987811840, 1159135232},
			{1161166848, -991821824, 1158414336},
			{1163591680, -999653376, 1156423680},
			{-997892096, -982593536, 1149124608},
			{-1034944512, -982245376, 1150926848},
			{1148289024, -982929408, 1152024576},
			{1156595712, -984604672, 1152393216},
			{1160523776, -987160576, 1152024576},
			{1163202560, -991010816, 1150926848},
			{1164980224, -998883328, 1149124608},
			{-997015552, -982175744, 1109917696},
			{-1016201216, -981499904, 1112276992},
			{1144111104, -981725184, 1114112000},
			{1154138112, -982843392, 1114898432},
			{1159254016, -984788992, 1114898432},
			{1162088448, -987463680, 1114112000},
			{1164230656, -991576064, 1112276992},
			{1165561856, -999555072, 1109917696},
			{-996270080, -982712320, -1000456192},
			{-1008435200, -982155264, -997916672},
			{1136132096, -982269952, -996851712},
			{1150509056, -983052288, -996188160},
			{1156751360, -984469504, -995966976},
			{1159979008, -986464256, -996188160},
			{1162317824, -988950528, -996851712},
			{1164111872, -993796096, -997916672},
			{1165283328, -1001914368, -1000456192},
			{1136001024, 1142800384, 1165746176},
			{1149722624, 1143341056, 1165185024},
			{1107558400, 1151254528, 1165185024},
			{1155440640, 1144127488, 1164058624},
			{1145356288, 1152024576, 1164722176},
			{-1014464512, 1156407296, 1164058624},
			{1159213056, 1145192448, 1162346496},
			{1153474560, 1153056768, 1163530240},
			{1140981760, 1157758976, 1163530240},
			{-1006108672, 1159467008, 1162346496},
			{1161605120, 1146503168, 1160015872},
			{1158516736, 1154334720, 1161482240},
			{1151754240, 1158684672, 1161981952},
			{1133674496, 1160646656, 1161482240},
			{-1002455040, 1161703424, 1160015872},
			{1163591680, 1147830272, 1156423680},
			{1161166848, 1155661824, 1158414336},
			{1157873664, 1159671808, 1159135232},
			{1150279680, 1161936896, 1159135232},
			{1121714176, 1163272192, 1158414336},
			{-999636992, 1163591680, 1156423680},
			{1164980224, 1148600320, 1149124608},
			{1163202560, 1156472832, 1150926848},
			{1160523776, 1160323072, 1152024576},
			{1156595712, 1162878976, 1152393216},
			{1148289024, 1164554240, 1152024576},
			{-1034944512, 1165238272, 1150926848},
			{-997892096, 1164890112, 1149124608},
			{1165561856, 1147928576, 1109917696},
			{1164230656, 1155907584, 1112276992},
			{1162088448, 1160019968, 1114112000},
			{1159254016, 1162694656, 1114898432},
			{1154138112, 1164640256, 1114898432},
			{1144111104, 1165758464, 1114112000},
			{-1016201216, 1165983744, 1112276992},
			{-997015552, 1165307904, 1109917696},
			{1165283328, 1145569280, -1000456192},
			{1164111872, 1153687552, -997916672},
			{1162317824, 1158533120, -996851712},
			{1159979008, 1161019392, -996188160},
			{1156751360, 1163014144, -995966976},
			{1150509056, 1164431360, -996188160},
			{1136132096, 1165213696, -996851712},
			{-1008435200, 1165328384, -997916672},
			{-996270080, 1164771328, -1000456192},
			{-1003077632, 0, 1165746176},
			{-997482496, 1142390784, 1165185024},
			{-997482496, -1005092864, 1165185024},
			{-994287616, 1150566400, 1164058624},
			{-993730560, 0, 1164722176},
			{-994287616, -996917248, 1164058624},
			{-991150080, 1154932736, 1162346496},
			{-989802496, 1142112256, 1163530240},
			{-989802496, -1005371392, 1163530240},
			{-991150080, -992550912, 1162346496},
			{-989020160, 1158193152, 1160015872},
			{-987766784, 1150173184, 1161482240},
			{-987336704, 0, 1161981952},
			{-987766784, -997310464, 1161482240},
			{-989020160, -989290496, 1160015872},
			{-987738112, 1159745536, 1156423680},
			{-985866240, 1154105344, 1158414336},
			{-984891392, 1141506048, 1159135232},
			{-984891392, -1005977600, 1159135232},
			{-985866240, -993378304, 1158414336},
			{-987738112, -987738112, 1156423680},
			{-986877952, 1160851456, 1149124608},
			{-984498176, 1157218304, 1150926848},
			{-983003136, 1149304832, 1152024576},
			{-982495232, 0, 1152393216},
			{-983003136, -998178816, 1152024576},
			{-984498176, -990265344, 1150926848},
			{-986877952, -986632192, 1149124608},
			{-986734592, 1161441280, 1109917696},
			{-984231936, 1158455296, 1112276992},
			{-982482944, 1152319488, 1114112000},
			{-981585920, 1139638272, 1114898432},
			{-981585920, -1007845376, 1114898432},
			{-982482944, -995164160, 1114112000},
			{-984231936, -989028352, 1112276992},
			{-986734592, -986042368, 1109917696},
			{-987381760, 1161494528, -1000456192},
			{-985247744, 1158905856, -997916672},
			{-983654400, 1154203648, -996851712},
			{-982671360, 1146093568, -996188160},
			{-982339584, 0, -995966976},
			{-982671360, -1001390080, -996188160},
			{-983654400, -993280000, -996851712},
			{-985247744, -988577792, -997916672},
			{-987381760, -985989120, -1000456192},
			{-992067584, -985628672, -989995008},
			{-989339648, -987787264, -988614656},
			{-997425152, -985305088, -988614656},
			{-988078080, -990855168, -987484160},
			{-994189312, -987463680, -987000832},
			{-1009418240, -985497600, -987484160},
			{-987254784, -996745216, -986574848},
			{-991789056, -990584832, -985518080},
			{-1002078208, -987807744, -985518080},
			{1133740032, -986255360, -986574848},
			{-986947584, -1010171904, -985956352},
			{-990511104, -997359616, -984285184},
			{-998277120, -992124928, -983711744},
			{1077936128, -988946432, -984285184},
			{1149272064, -987631616, -985956352},
			{-987226112, 1139474432, -985743360},
			{-990633984, -1022164992, -983515136},
			{-997834752, -1002651648, -982355968},
			{-1024851968, -995917824, -982355968},
			{1146355712, -992165888, -983515136},
			{1155211264, -989667328, -985743360},
			{-988119040, 1152221184, -986140672},
			{-992288768, 1148239872, -983601152},
			{-999997440, 1139671040, -982011904},
			{-1044905984, -1038090240, -981471232},
			{1146732544, -1005928448, -982011904},
			{1154859008, -998162432, -983601152},
			{1159225344, -994779136, -986140672},
			{-989515776, 1158520832, -987332608},
			{-995139584, 1157718016, -984961024},
			{-1005207552, 1155293184, -983306240},
			{1131216896, 1152000000, -982454272},
			{1149534208, 1146978304, -982454272},
			{1155817472, 1135968256, -983306240},
			{1159458816, -1020067840, -984961024},
			{1161641984, -1003356160, -987332608},
			{-992468992, 1161474048, -989220864},
			{-998424576, 1161383936, -987443200},
			{-1015087104, 1160785920, -986120192},
			{1140867072, 1159704576, -985305088},
			{1151180800, 1158184960, -985026560},
			{1156874240, 1154957312, -985305088},
			{1159761920, 1150582784, -986120192},
			{1161834496, 1142358016, -987443200},
			{1163378688, -1065353216, -989220864},
			{1158926336, 1158926336, 1158926336},
			{1158926336, 1158926336, -988557312},
			{1158926336, -988557312, 1158926336},
			{1158926336, -988557312, -988557312},
			{-988557312, 1158926336, 1158926336},
			{-988557312, 1158926336, -988557312},
			{-988557312, -988557312, 1158926336},
			{-988557312, -988557312, -988557312}
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
		protected uint _normalCount;
		protected uint _normalStart;
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

		protected class SR2TriangleList
		{
			public Material material;
			public UInt32 polygonCount;
			public UInt32 polygonStart;
			public UInt16 groupID;
			public UInt32 next;
		}

		protected SR2Model(BinaryReader reader, DataFile dataFile, UInt32 dataStart, UInt32 modelData, String modelName, Platform ePlatform, UInt32 version)
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
			_normalCount = 0;
			_normalStart = 0;
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
			ReadVertices(reader, options);

			// Get the normals
			_geometry.VertexNormals = new Vector[_normals.Length / 3];
			_geometry.PolygonNormals = new Vector[_normalCount];
			ReadNormals(reader, options);

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

		protected virtual void ReadVertex(BinaryReader reader, int v, ExportOptions options)
		{
			_geometry.Vertices[v].positionID = v;

			_geometry.PositionsRaw[v].x = (float)reader.ReadInt16();
			_geometry.PositionsRaw[v].y = (float)reader.ReadInt16();
			_geometry.PositionsRaw[v].z = (float)reader.ReadInt16();
			reader.BaseStream.Position += 0x02;
		}

		protected virtual void ReadVertices(BinaryReader reader, ExportOptions options)
		{
			if (_vertexStart == 0 || _vertexCount == 0)
			{
				return;
			}

			reader.BaseStream.Position = _vertexStart;

			for (int v = 0; v < _vertexCount; v++)
			{
				ReadVertex(reader, v, options);
			}
		}

		protected virtual void ReadNormal(BinaryReader reader, int n, ExportOptions options)
		{
			_geometry.PolygonNormals[n].x = reader.ReadInt16() / 4096.0f;
			_geometry.PolygonNormals[n].y = reader.ReadInt16() / 4096.0f;
			_geometry.PolygonNormals[n].z = reader.ReadInt16() / 4096.0f;
		}

		protected virtual void ReadNormals(BinaryReader reader, ExportOptions options)
		{
			for (int n = 0; n < _geometry.VertexNormals.Length; n++)
			{
				// Are these wrong? Different on PC and PS2?
				_geometry.VertexNormals[n].x = (_normals[n, 0] / 4096.0f);
				_geometry.VertexNormals[n].y = (_normals[n, 1] / 4096.0f);
				_geometry.VertexNormals[n].z = (_normals[n, 2] / 4096.0f);
			}

			if (_normalStart == 0 || _normalCount == 0)
			{
				return;
			}

			reader.BaseStream.Position = _normalStart;

			for (int v = 0; v < _normalCount; v++)
			{
				ReadNormal(reader, v, options);
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
