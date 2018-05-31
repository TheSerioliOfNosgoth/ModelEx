using System;
using System.IO;
using System.Collections.Generic;

namespace ModelEx
{
    public abstract class SR2Model : SRModel
    {
        #region Normals
        protected static Int32[,] s_aiNormals =
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

        protected class SR2TriangleList
        {
            public ExMaterial m_xMaterial;
            public UInt32 m_uPolygonCount;
            public UInt32 m_uPolygonStart;
            public UInt16 m_usGroupID;
            public UInt32 m_uNext;
        }

        protected SR2Model(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt32 uVersion) :
            base(xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion)
        {
        }

        protected virtual void ReadData(BinaryReader xReader)
        {
            // Get the normals
            m_axNormals = new ExVector[s_aiNormals.Length / 3];
            for (int n = 0; n < m_axNormals.Length; n++)
            {
                m_axNormals[n].x = s_aiNormals[n, 0];
                m_axNormals[n].y = s_aiNormals[n, 1];
                m_axNormals[n].z = s_aiNormals[n, 2];
            }

            // Get the vertices
            m_axVertices = new ExVertex[m_uVertexCount];
            m_axPositions = new ExPosition[m_uVertexCount];
            m_axPositionsAlt = new ExVector[m_uVertexCount];
            m_auColours = new UInt32[m_uVertexCount];
            m_auColoursAlt = new UInt32[m_uVertexCount];
            m_axUVs = new ExUV[m_uVertexCount];
            ReadVertices(xReader);

            // Get the polygons
            m_axPolygons = new ExPolygon[m_uPolygonCount];
            ReadPolygons(xReader);

            // Generate the output
            GenerateOutput();
        }

        protected virtual void ReadVertex(BinaryReader xReader, int v)
        {
            m_axVertices[v].positionID = v;

            // Read the local coordinates
            m_axPositions[v].localPos.x = (float)xReader.ReadInt16();
            m_axPositions[v].localPos.y = (float)xReader.ReadInt16();
            m_axPositions[v].localPos.z = (float)xReader.ReadInt16();
            xReader.BaseStream.Position += 0x02;

            // Before transformation, the world coords equal the local coords
            m_axPositions[v].worldPos = m_axPositions[v].localPos;
            m_axPositionsAlt[v] = m_axPositions[v].localPos;
        }

        protected virtual void ReadVertices(BinaryReader xReader)
        {
            if (m_uVertexStart == 0 || m_uVertexCount == 0)
            {
                return;
            }

            xReader.BaseStream.Position = m_uVertexStart;

            for (UInt16 v = 0; v < m_uVertexCount; v++)
            {
                ReadVertex(xReader, v);
            }

            return;
        }

        protected abstract void ReadPolygons(BinaryReader xReader);

        protected virtual void GenerateOutput()
        {
            // Make the vertices unique
            m_axVertices = new ExVertex[m_uIndexCount];
            for (UInt32 p = 0; p < m_uPolygonCount; p++)
            {
                m_axVertices[(3 * p) + 0] = m_axPolygons[p].v1;
                m_axVertices[(3 * p) + 1] = m_axPolygons[p].v2;
                m_axVertices[(3 * p) + 2] = m_axPolygons[p].v3;
            }

            // Build the materials array
            m_axMaterials = new ExMaterial[m_uMaterialCount];
            UInt16 mNew = 0;

            foreach (ExMaterial xMaterial in m_xMaterialsList)
            {
                m_axMaterials[mNew] = xMaterial;
                m_axMaterials[mNew].ID = mNew;
                mNew++;
            }

            return;
        }
    }

    public class SR2File : SRFile
    {
        #region Model classes

        protected class SR2ObjectModel : SR2Model
        {
            protected UInt32 m_uColourStart;

            protected SR2ObjectModel(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt32 uVersion)
                : base (xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion)
            {
                xReader.BaseStream.Position = m_uModelData + 0x04;
                UInt32 uBoneCount1          = xReader.ReadUInt32();
                UInt32 uBoneCount2          = xReader.ReadUInt32();
                m_uBoneCount                = uBoneCount1 + uBoneCount2;
                m_uBoneStart                = xReader.ReadUInt32();
                m_xScale.x            = xReader.ReadSingle();
                m_xScale.y            = xReader.ReadSingle();
                m_xScale.z            = xReader.ReadSingle();
                xReader.BaseStream.Position += 0x04;
                m_uVertexCount              = xReader.ReadUInt32();
                m_uVertexStart              = m_uDataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position += 0x08;
                m_uPolygonCount             = 0; // xReader.ReadUInt32();
                m_uPolygonStart             = 0; // m_uDataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position += 0x18;
                m_uColourStart              = xReader.ReadUInt32();
                xReader.BaseStream.Position += 0x0C;
                m_uMaterialStart            = m_uDataStart + xReader.ReadUInt32();
                m_uMaterialCount            = 0;
                m_uTreeCount                = 1;

                m_axTrees = new ExTree[m_uTreeCount];
            }

            public static SR2ObjectModel Load(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt16 usIndex, UInt32 uVersion)
            {
                xReader.BaseStream.Position = uModelData + (0x00000004 * usIndex);
                uModelData = uDataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position = uModelData;
                SR2ObjectModel xModel = new SR2ObjectModel(xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion);
                xModel.ReadData(xReader);
                return xModel;
            }

            protected override void ReadVertex(BinaryReader xReader, int v)
            {
                base.ReadVertex(xReader, v);

                m_axVertices[v].normalID = xReader.ReadUInt16();
                xReader.BaseStream.Position += 0x02;

                m_axVertices[v].UVID = v;

                UInt16 vU = xReader.ReadUInt16();
                UInt16 vV = xReader.ReadUInt16();

                m_axUVs[v].u = Utility.BizarreFloatToNormalFloat(vU);
                m_axUVs[v].v = Utility.BizarreFloatToNormalFloat(vV);
            }

            protected override void ReadVertices(BinaryReader xReader)
            {
                base.ReadVertices(xReader);

                xReader.BaseStream.Position = m_uColourStart;
                for (UInt16 v = 0; v < m_uVertexCount; v++)
                {
                    m_auColours[v] = xReader.ReadUInt32();
                }

                ReadArmature(xReader);
                ApplyArmature();
            }

            protected virtual void ReadArmature(BinaryReader xReader)
            {
                if (m_uBoneStart == 0 || m_uBoneCount == 0) return;

                xReader.BaseStream.Position = m_uBoneStart;
                m_axBones = new ExBone[m_uBoneCount];
                for (UInt16 b = 0; b < m_uBoneCount; b++)
                {
                    // Get the bone data
                    m_axBones[b].localPos.x = xReader.ReadSingle();
                    m_axBones[b].localPos.y = xReader.ReadSingle();
                    m_axBones[b].localPos.z = xReader.ReadSingle();

                    float unknown = xReader.ReadSingle();
                    m_axBones[b].flags = xReader.ReadUInt32();

                    m_axBones[b].vFirst = xReader.ReadUInt16();
                    m_axBones[b].vLast = xReader.ReadUInt16();

                    m_axBones[b].parentID1 = xReader.ReadUInt16();
                    m_axBones[b].parentID2 = xReader.ReadUInt16();

                    //if (parent1 != 0xFFFF && parent2 != 0xFFFF &&
                    //    parent2 != 0)
                    if (m_axBones[b].flags == 8)
                    {
                        m_axBones[b].parentID1 = m_axBones[b].parentID2;
                    }

                    xReader.BaseStream.Position += 0x04;
                }

                for (UInt16 b = 0; b < m_uBoneCount; b++)
                {
                    // Combine this bone with it's ancestors if there are any
                    if ((m_axBones[b].vFirst != 0xFFFF) && (m_axBones[b].vLast != 0xFFFF))
                    {
                        //for (UInt16 ancestorID = b; ancestorID != 0xFFFF; )
                        //{
                        //    m_axBones[b].worldPos += m_axBones[ancestorID].localPos;
                        //    if (m_axBones[ancestorID].parentID1 == ancestorID) break;
                        //    ancestorID = m_axBones[ancestorID].parentID1;
                        //}

                        m_axBones[b].worldPos = CombineParent(b);
                    }
                }
                return;
            }

            protected ExVector CombineParent(UInt16 bone)
            {
                if (bone == 0xFFFF)
                {
                    return new ExVector(0.0f, 0.0f, 0.0f);
                }

                ExVector vector1 = CombineParent(m_axBones[bone].parentID1);
                ExVector vector2 = CombineParent(m_axBones[bone].parentID2);
                ExVector vector3 = m_axBones[bone].localPos;
                vector3 += vector1;
                //vector3 += vector2;
                return vector3;
            }

            protected virtual void ApplyArmature()
            {
                if ((m_uVertexStart == 0 || m_uVertexCount == 0) ||
                    (m_uBoneStart == 0 || m_uBoneCount == 0)) return;

                for (UInt16 b = 0; b < m_uBoneCount; b++)
                {
                    if ((m_axBones[b].vFirst != 0xFFFF) && (m_axBones[b].vLast != 0xFFFF))
                    {
                        for (UInt16 v = m_axBones[b].vFirst; v <= m_axBones[b].vLast; v++)
                        {
                            m_axPositions[v].worldPos += m_axBones[b].worldPos;
                            m_axPositionsAlt[v] += m_axBones[b].worldPos;
                            m_axPositions[v].boneID = b;
                        }
                    }
                }
                return;
            }

            protected override void ReadPolygons(BinaryReader xReader)
            {
                if (m_uMaterialStart == 0)
                {
                    return;
                }

                List<SR2TriangleList> xTriangleListList = new List<SR2TriangleList>();
                UInt32 uMaterialPosition = m_uMaterialStart;
                m_uTreeCount = 0;
                while (uMaterialPosition != 0)
                {
                    xReader.BaseStream.Position = uMaterialPosition;
                    SR2TriangleList xTriangleList = new SR2TriangleList();

                    if (ReadTriangleList(xReader, ref xTriangleList)/* && xTriangleList.m_usGroupID == 0*/)
                    {
                        xTriangleListList.Add(xTriangleList);
                        m_uPolygonCount += xTriangleList.m_uPolygonCount;

                        if ((UInt32)xTriangleList.m_usGroupID > m_uTreeCount)
                        {
                            m_uTreeCount = xTriangleList.m_usGroupID;
                        }
                    }

                    m_xMaterialsList.Add(xTriangleList.m_xMaterial);

                    uMaterialPosition = xTriangleList.m_uNext;
                }

                m_uMaterialCount = (UInt32)m_xMaterialsList.Count;

                m_uTreeCount++;
                m_axTrees = new ExTree[m_uTreeCount];
                for (UInt32 t = 0; t < m_uTreeCount; t++)
                {
                    m_axTrees[t] = new ExTree();
                    m_axTrees[t].mesh = new ExMesh();

                    foreach (SR2TriangleList xTriangleList in xTriangleListList)
                    {
                        if (t == (UInt32)xTriangleList.m_usGroupID)
                        {
                            m_axTrees[t].mesh.polygonCount += xTriangleList.m_uPolygonCount;
                        }
                    }

                    m_axTrees[t].mesh.indexCount = m_axTrees[t].mesh.polygonCount * 3;
                    m_axTrees[t].mesh.polygons = new ExPolygon[m_axTrees[t].mesh.polygonCount];
                    m_axTrees[t].mesh.vertices = new ExVertex[m_axTrees[t].mesh.indexCount];
                }

                for (UInt32 t = 0; t < m_uTreeCount; t++)
                {
                    UInt32 tp = 0;
                    foreach (SR2TriangleList xTriangleList in xTriangleListList)
                    {
                        if (t != (UInt32)xTriangleList.m_usGroupID)
                        {
                            continue;
                        }

                        xReader.BaseStream.Position = xTriangleList.m_uPolygonStart;
                        for (int pl = 0; pl < xTriangleList.m_uPolygonCount; pl++)
                        {
                            m_axTrees[t].mesh.polygons[tp].v1 = m_axVertices[xReader.ReadUInt16()];
                            m_axTrees[t].mesh.polygons[tp].v2 = m_axVertices[xReader.ReadUInt16()];
                            m_axTrees[t].mesh.polygons[tp].v3 = m_axVertices[xReader.ReadUInt16()];
                            m_axTrees[t].mesh.polygons[tp].material = xTriangleList.m_xMaterial;
                            tp++;
                        }
                    }

                    // Make the vertices unique - Because I do the same thing in GenerateOutput
                    for (UInt16 poly = 0; poly < m_axTrees[t].mesh.polygonCount; poly++)
                    {
                        m_axTrees[t].mesh.vertices[(3 * poly) + 0] = m_axTrees[t].mesh.polygons[poly].v1;
                        m_axTrees[t].mesh.vertices[(3 * poly) + 1] = m_axTrees[t].mesh.polygons[poly].v2;
                        m_axTrees[t].mesh.vertices[(3 * poly) + 2] = m_axTrees[t].mesh.polygons[poly].v3;
                    }
                }

                m_axPolygons = new ExPolygon[m_uPolygonCount];
                UInt32 p = 0;
                foreach (SR2TriangleList xTriangleList in xTriangleListList)
                {
                    xReader.BaseStream.Position = xTriangleList.m_uPolygonStart;
                    for (int pl = 0; pl < xTriangleList.m_uPolygonCount; pl++)
                    {
                        m_axPolygons[p].v1 = m_axVertices[xReader.ReadUInt16()];
                        m_axPolygons[p].v2 = m_axVertices[xReader.ReadUInt16()];
                        m_axPolygons[p].v3 = m_axVertices[xReader.ReadUInt16()];
                        m_axPolygons[p].material = xTriangleList.m_xMaterial;
                        p++;
                    }
                }
            }

            protected virtual bool ReadTriangleList(BinaryReader xReader, ref SR2TriangleList xTriangleList)
            {
                xTriangleList.m_uPolygonCount = (UInt32)xReader.ReadUInt16() / 3;
                xTriangleList.m_usGroupID = xReader.ReadUInt16(); // Used by MON_SetAccessories and INSTANCE_UnhideAllDrawGroups
                xTriangleList.m_uPolygonStart = (UInt32)(xReader.BaseStream.Position) + 0x0C;
                UInt16 xWord0 = xReader.ReadUInt16();
                UInt16 xWord1 = xReader.ReadUInt16();
                UInt32 xDWord0 = xReader.ReadUInt32();
                xTriangleList.m_xMaterial = new ExMaterial();
                xTriangleList.m_xMaterial.visible = ((xWord1 & 0x0800) == 0);
                xTriangleList.m_xMaterial.textureID = (UInt16)(xWord0 & 0x0FFF);
                xTriangleList.m_xMaterial.colour = 0xFFFFFFFF;
                if (xTriangleList.m_xMaterial.textureID > 0)
                {
                    xTriangleList.m_xMaterial.textureUsed = true;
                }
                else
                {
                    xTriangleList.m_xMaterial.textureUsed = false;
                    //xMaterial.colour = 0x00000000;
                }
                xTriangleList.m_uNext = xReader.ReadUInt32();

                return (xTriangleList.m_xMaterial.visible);
            }
        }

        protected class SR2UnitModel : SR2Model
        {
            protected UInt32 m_uOctTreeCount;
            protected UInt32 m_uOctTreeStart;
            protected Realm m_eRealm;
            protected UInt32 m_uSpectralVertexStart;
            protected UInt32 m_uSpectralColourStart;

            protected SR2UnitModel(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, Realm eRealm, UInt32 uVersion)
                : base (xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion)
            {
                m_eRealm                    = eRealm;
                // xReader.BaseStream.Position += 0x04;
                // m_uInstanceCount = xReader.ReadUInt32();
                // m_uInstanceStart = m_uDataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position = m_uModelData + 0x0C;

                m_uVertexCount              = xReader.ReadUInt32();
                m_uPolygonCount             = 0; // xReader.ReadUInt32(); // Length = 0x14
                xReader.BaseStream.Position += 0x08;
                m_uVertexStart              = m_uDataStart + xReader.ReadUInt32();
                m_uPolygonStart             = 0; // m_uDataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position += 0x14;
                xReader.BaseStream.Position += 0x04;
                m_uSpectralVertexStart      = m_uDataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position += 0x04;
                m_uSpectralColourStart      = m_uDataStart + xReader.ReadUInt32();
                m_uMaterialStart            = 0;
                m_uMaterialCount            = 0;
                m_uOctTreeCount             = xReader.ReadUInt32();
                m_uOctTreeStart             = m_uDataStart + xReader.ReadUInt32();
                m_uTreeCount                = m_uOctTreeCount;

                //m_uVertexCount = xReader.ReadUInt32();
                //m_uPolygonCount = 0; // xReader.ReadUInt32(); // Length = 0x14
                //xReader.BaseStream.Position += 0x08;
                //m_uVertexStart = m_uDataStart + xReader.ReadUInt32();
                //m_uPolygonStart = 0; // m_uDataStart + xReader.ReadUInt32();
                //xReader.BaseStream.Position += 0x14;
                //m_uMaterialStart = m_uDataStart + xReader.ReadUInt32(); // Vertex colours
                //m_uMaterialStart = m_uDataStart + xReader.ReadUInt32(); // Vertex colours
                //m_uMaterialStart = m_uDataStart + xReader.ReadUInt32(); // Vertex colours
                //m_uMaterialStart = 0;
                //m_uMaterialCount = 0;
                //m_uSpectralColourStart = m_uDataStart + xReader.ReadUInt32();
                //m_uSpectralVertexStart = 0; // m_uDataStart + xReader.ReadUInt32();
                //m_uOctTreeCount = xReader.ReadUInt32();
                //m_uOctTreeStart = m_uDataStart + xReader.ReadUInt32();
                //m_uTreeCount = m_uOctTreeCount;

                m_axTrees = new ExTree[m_uTreeCount];
            }

            public static SR2UnitModel Load(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, Realm eRealm, UInt32 uVersion)
            {
                SR2UnitModel xModel = new SR2UnitModel(xReader, uDataStart, uModelData, strModelName, ePlatform, eRealm, uVersion);
                xModel.ReadData(xReader);
                return xModel;
            }

            protected override void ReadVertex(BinaryReader xReader, int v)
            {
                base.ReadVertex(xReader, v);

                m_axVertices[v].colourID = v;

                m_auColours[v] = xReader.ReadUInt32();

                m_axVertices[v].UVID = v;

                UInt16 vU = xReader.ReadUInt16();
                UInt16 vV = xReader.ReadUInt16();

                // This is the broken one in pillars4c.
                // Search for in Cheat Engine 03 3E 17 3F DF F1 F6 D4 D3 C7 00 00 36 4B 39 FF
                if (v == 13370)
                {
                    m_axUVs[v].u = Utility.BizarreFloatToNormalFloat(vU); // 0x3E03 = 0.127929688 with Ben's formula
                    m_axUVs[v].v = Utility.BizarreFloatToNormalFloat(vV); // 0x3F17 = 0.58984375 with Ben's formula
                    //m_axUVs[v].u = 0.0f;
                    //m_axUVs[v].v = 0.0f;
                }
                else
                {
                    m_axUVs[v].u = Utility.BizarreFloatToNormalFloat(vU);
                    m_axUVs[v].v = Utility.BizarreFloatToNormalFloat(vV);
                }
            }

            protected override void ReadVertices(BinaryReader xReader)
            {
                base.ReadVertices(xReader);

                ReadSpectralData(xReader);
            }

            protected virtual void ReadSpectralData(BinaryReader xReader)
            {
                if (m_eRealm == Realm.Spectral &&
                    m_uSpectralVertexStart != 0 && m_uSpectralColourStart != 0)
                {
                    // Spectral Colours
                    xReader.BaseStream.Position = m_uSpectralColourStart;
                    for (int v = 0; v < m_uVertexCount; v++)
                    {
                        UInt32 uShiftColour = xReader.ReadUInt32();
                        UInt32 uAlpha = m_auColours[v] & 0xFF000000;
                        UInt32 uRGB = uShiftColour & 0x00FFFFFF;
                        m_auColours[v] = uAlpha | uRGB;
                    }

                    // Spectral vertices
                    xReader.BaseStream.Position = m_uModelData + 0x2C;
                    UInt32 uCurrentIndexPosition = xReader.ReadUInt32();
                    UInt32 uCurrentSpectralVertex = m_uSpectralVertexStart;
                    while (true)
                    {
                        xReader.BaseStream.Position = uCurrentIndexPosition;
                        Int32 iVertex = xReader.ReadInt32();
                        uCurrentIndexPosition = (UInt32)xReader.BaseStream.Position;

                        if(iVertex == -1)
                        {
                            break;
                        }

                        xReader.BaseStream.Position = uCurrentSpectralVertex;
                        ExShiftVertex xShiftVertex;
                        xShiftVertex.basePos.x = (float)xReader.ReadInt16();
                        xShiftVertex.basePos.y = (float)xReader.ReadInt16();
                        xShiftVertex.basePos.z = (float)xReader.ReadInt16();
                        uCurrentSpectralVertex = (UInt32)xReader.BaseStream.Position;

                        m_axPositions[iVertex].localPos = xShiftVertex.basePos;
                        m_axPositions[iVertex].worldPos = m_axPositions[iVertex].localPos;
                        m_axPositionsAlt[iVertex] = m_axPositions[iVertex].localPos;
                    }

                    //// Spectral Verticices
                    //xReader.BaseStream.Position = m_uSpectralVertexStart + 0x06;
                    //int sVertex = xReader.ReadInt16();
                    //xReader.BaseStream.Position = m_uSpectralVertexStart;
                    //while (sVertex != 0xFFFF)
                    //{
                    //    ExShiftVertex xShiftVertex;
                    //    xShiftVertex.basePos.x = (float)xReader.ReadInt16();
                    //    xShiftVertex.basePos.y = (float)xReader.ReadInt16();
                    //    xShiftVertex.basePos.z = (float)xReader.ReadInt16();
                    //    sVertex = xReader.ReadUInt16();

                    //    if (sVertex == 0xFFFF)
                    //    {
                    //        break;
                    //    }

                    //    xShiftVertex.offset.x = (float)xReader.ReadInt16();
                    //    xShiftVertex.offset.y = (float)xReader.ReadInt16();
                    //    xShiftVertex.offset.z = (float)xReader.ReadInt16();
                    //    m_axPositions[sVertex].localPos = xShiftVertex.offset + xShiftVertex.basePos;
                    //    m_axPositions[sVertex].worldPos = m_axPositions[sVertex].localPos;
                    //}
                }
            }

            protected override void ReadPolygons(BinaryReader xReader)
            {
                ExMaterial xMaterial = new ExMaterial();
                xMaterial.textureID = 0;
                xMaterial.colour = 0xFFFFFFFF;
                m_xMaterialsList.Add(xMaterial);

                MemoryStream xPolyStream = new MemoryStream((Int32)m_uVertexCount * 3);
                BinaryWriter xPolyWriter = new BinaryWriter(xPolyStream);
                BinaryReader xPolyReader = new BinaryReader(xPolyStream);

                MemoryStream xTextureStream = new MemoryStream((Int32)m_uVertexCount * 3);
                BinaryWriter xTextureWriter = new BinaryWriter(xTextureStream);
                BinaryReader xTextureReader = new BinaryReader(xTextureStream);

                List<ExMesh> xMeshes = new List<ExMesh>();
                List<Int64> xMeshPositions = new List<Int64>();

                for (UInt32 t = 0; t < m_uOctTreeCount; t++)
                {
                    xReader.BaseStream.Position = m_uOctTreeStart + (t * 0x60);

                    xReader.BaseStream.Position += 0x2C;
                    bool drawTester = ((xReader.ReadUInt32() & 1) != 1);
                    UInt32 uOctID = xReader.ReadUInt32();
                    xReader.BaseStream.Position += 0x10;
                    UInt32 uDataPos = xReader.ReadUInt32();
                    xReader.BaseStream.Position += 0x08;
                    // In each terrain group, vertices start from part way through the array.
                    UInt32 uStartIndex = xReader.ReadUInt32();

                    m_axTrees[t] = ReadOctTree(xReader, xPolyWriter, xTextureWriter, uDataPos, m_axTrees[t], xMeshes, xMeshPositions, 0, (UInt16)uStartIndex);
                }

                m_uPolygonCount = (UInt32)xPolyReader.BaseStream.Position / 6;
                m_axPolygons = new ExPolygon[m_uPolygonCount];
                UInt32 uPolygon = 0;

                xPolyReader.BaseStream.Position = 0;
                xTextureReader.BaseStream.Position = 0;
                for (int m = 0; m < xMeshes.Count; m++)
                {
                    ExMesh xCurrentMesh = xMeshes[m];
                    Int64 iStartPosition = xPolyReader.BaseStream.Position;
                    Int64 iEndPosition = xMeshPositions[m];
                    Int64 iRange = iEndPosition - iStartPosition;
                    while (iRange % 6 > 0) iRange++;
                    UInt32 uIndexCount = (UInt32)iRange / 2;

                    FinaliseMesh(xPolyReader, xTextureReader, xCurrentMesh, xMaterial, uIndexCount, ref uPolygon);
                }
                m_uMaterialCount = (UInt32)m_xMaterialsList.Count;

                return;
            }

            protected virtual ExTree ReadOctTree(BinaryReader xReader, BinaryWriter xPolyWriter, BinaryWriter xTextureWriter, UInt32 uDataPos, ExTree xParentTree, List<ExMesh> xMeshes, List<Int64> xMeshPositions, UInt32 uDepth, UInt16 uStartIndex)
            {
                if (uDataPos == 0)
                {
                    return null;
                }

                xReader.BaseStream.Position = uDataPos + 0x34;
                Int32 iSubTreeCount = xReader.ReadInt32();

                ExTree xTree = null;
                ExMesh xMesh = null;

                UInt32 uMaxDepth = 0;

                if (uDepth <= uMaxDepth)
                {
                    xTree = new ExTree();
                    xMesh = new ExMesh();
                    xTree.mesh = xMesh;

                    if (xParentTree != null)
                    {
                        xParentTree.Push(xTree);
                    }
                }
                else
                {
                    xTree = xParentTree;
                    xMesh = xParentTree.mesh;
                }

                if (iSubTreeCount == 0)
                {
                    xTree.isLeaf = true;

                    xReader.BaseStream.Position = uDataPos + 0x30;
                    ReadOctLeaf(xReader, xPolyWriter, xTextureWriter, xMesh, uStartIndex);
                }
                else
                {
                    UInt32[] auSubTreePositions = new UInt32[iSubTreeCount];
                    for (Int32 s = 0; s < iSubTreeCount; s++)
                    {
                        auSubTreePositions[s] = xReader.ReadUInt32();
                    }

                    for (Int32 s = iSubTreeCount - 1; s >= 0; s--)
                    {
                        ReadOctTree(xReader, xPolyWriter, xTextureWriter, auSubTreePositions[s], xTree, xMeshes, xMeshPositions, uDepth + 1, uStartIndex);
                    }
                }

                if (uDepth <= uMaxDepth)
                {
                    if (xMesh != null && xMesh.indexCount > 0)
                    {
                        xMeshes.Add(xMesh);
                        xMeshPositions.Add(xPolyWriter.BaseStream.Position);
                    }
                }

                return xTree;
            }

            protected virtual void ReadOctLeaf(BinaryReader xReader, BinaryWriter xPolyWriter, BinaryWriter xTextureWriter, ExMesh xMesh, UInt16 uStartIndex)
            {
                UInt32 uLeafData = m_uDataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position = uLeafData;

                UInt32 uNextStrip = (UInt32)xReader.BaseStream.Position;
                while (true)
                {
                    bool bShouldWrite = true; // For debug.

                    UInt32 uLength = xReader.ReadUInt32();
                    if (uLength == 0 || uLength == 0xFFFFFFFF)
                    {
                        break;
                    }

                    uNextStrip += uLength;

                    UInt32 uIndexCount = xReader.ReadUInt32();
                    if (uIndexCount == 0)
                    {
                        continue;
                    }

                    UInt16[] axStripIndices = new UInt16[uIndexCount];
                    for (UInt32 i = 0; i < uIndexCount; i++)
                    {
                        axStripIndices[i] = (UInt16)(uStartIndex + xReader.ReadUInt16());
                    }

                    if (xReader.BaseStream.Position % 4 != 0)
                    {
                        xReader.BaseStream.Position += 0x02;
                    }

                    while (true)
                    {                        
                        // 0xFFFF wrong?  Try uTestNextStrip
                        UInt32 uIndexCount2 = xReader.ReadUInt32();
                        if (/*(uIndexCount2 & 0x0000FFFF) == 0x0000FFFF || (uIndexCount2 & 0xFFFF0000) == 0xFFFF0000 ||*/ uIndexCount2 == 0)
                        {
                            //if (xReader.BaseStream.Position % 4 != 0)
                            //{
                            //    xReader.BaseStream.Position += 0x02;
                            //}
                            break;
                        }

                        xReader.BaseStream.Position += 0x04;
                        UInt32 uTextureID = xReader.ReadUInt32();
                        xReader.BaseStream.Position += 0x04;

                        UInt32 uTestNextStrip = xReader.ReadUInt32();

                        UInt16[] axStripIndices2 = new UInt16[uIndexCount2];
                        for (UInt32 i = 0; i < uIndexCount2; i++)
                        {
                            axStripIndices2[i] = xReader.ReadUInt16();
                        }

                        if (bShouldWrite)
                        {
                            for (UInt16 i = 0; i < uIndexCount2; i++)
                            {
                                xPolyWriter.Write(axStripIndices[axStripIndices2[i]]);
                                if (xPolyWriter.BaseStream.Position % 6 == 0)
                                {
                                    xTextureWriter.Write(uTextureID);
                                }
                            }

                            if (xMesh != null)
                            {
                                xMesh.indexCount += uIndexCount2;
                            }
                        }

                        xReader.BaseStream.Position = uTestNextStrip;
                    }

                    xReader.BaseStream.Position = uNextStrip;
                }

                // Was this a special second set of polys?  Animated ones?
                while (true)
                {
                    bool bShouldWrite = true; // For debug.

                    UInt32 uIndexCount = xReader.ReadUInt32();
                    if (uIndexCount == 0)
                    {
                        break;
                    }

                    xReader.BaseStream.Position += 0x04;
                    UInt32 uTextureID = xReader.ReadUInt32();

                    xReader.BaseStream.Position += 0x04;
                    uNextStrip = xReader.ReadUInt32();

                    UInt16[] axStripIndices = new UInt16[uIndexCount];
                    for (UInt32 i = 0; i < uIndexCount; i++)
                    {
                        axStripIndices[i] = (UInt16)(uStartIndex + xReader.ReadUInt16());
                    }

                    if (bShouldWrite)
                    {
                        for (UInt16 i = 0; i < uIndexCount; i++)
                        {
                            xPolyWriter.Write(axStripIndices[i]);
                            if (xPolyWriter.BaseStream.Position % 6 == 0)
                            {
                                xTextureWriter.Write(uTextureID);
                            }
                        }

                        if (xMesh != null)
                        {
                            xMesh.indexCount += uIndexCount;
                        }
                    }

                    xReader.BaseStream.Position = uNextStrip;
                }
            }

            protected virtual void FinaliseMesh(BinaryReader xPolyReader, BinaryReader xTextureReader, ExMesh xMesh, ExMaterial xMaterial, UInt32 uIndexCount, ref UInt32 uPolygon)
            {
                //uIndexCount &= 0x0000FFFF;
                //uIndexCount /= 3;
                //uIndexCount *= 3;

                //xMesh.m_uIndexCount = uIndexCount;
                xMesh.polygonCount = xMesh.indexCount / 3;
                xMesh.polygons = new ExPolygon[xMesh.polygonCount];
                for (UInt32 p = 0; p < xMesh.polygonCount; p++)
                {
                    UInt16 uV1 = xPolyReader.ReadUInt16();
                    UInt16 uV2 = xPolyReader.ReadUInt16();
                    UInt16 uV3 = xPolyReader.ReadUInt16();

                    xMesh.polygons[p].v1 = m_axVertices[uV1];
                    xMesh.polygons[p].v2 = m_axVertices[uV2];
                    xMesh.polygons[p].v3 = m_axVertices[uV3];

                    xMaterial = new ExMaterial();

                    xMaterial.visible = true;

                    UInt16 xWord1 = xTextureReader.ReadUInt16();
                    UInt16 xWord2 = xTextureReader.ReadUInt16();
                    xMaterial.textureID = (UInt16)(xWord1 & 0x0FFF);
                    xMaterial.colour = 0xFFFFFFFF;
                    if (xMaterial.textureID > 0 && xMaterial.visible)
                    {
                        xMaterial.textureUsed = true;
                    }
                    else
                    {
                        xMaterial.textureUsed = false;
                    }

                    bool bFoundMaterial = false;
                    for (int m = 0; m < m_xMaterialsList.Count; m++)
                    {
                        if (m_xMaterialsList[m].colour == xMaterial.colour &&
                            m_xMaterialsList[m].textureID == xMaterial.textureID &&
                            m_xMaterialsList[m].textureUsed == xMaterial.textureUsed)
                        {
                            bFoundMaterial = true;
                            xMaterial = m_xMaterialsList[m];
                            break;
                        }
                    }

                    if (!bFoundMaterial)
                    {
                        m_xMaterialsList.Add(xMaterial);
                    }

                    xMesh.polygons[p].material = xMaterial;
                }

                // Make the vertices unique - Because I do the same thing in GenerateOutput
                xMesh.vertices = new ExVertex[xMesh.indexCount];
                for (UInt32 poly = 0; poly < xMesh.polygonCount; poly++)
                {
                    m_axPolygons[uPolygon++] = xMesh.polygons[poly];
                    xMesh.vertices[(3 * poly) + 0] = xMesh.polygons[poly].v1;
                    xMesh.vertices[(3 * poly) + 1] = xMesh.polygons[poly].v2;
                    xMesh.vertices[(3 * poly) + 2] = xMesh.polygons[poly].v3;
                }
            }
        }

        #endregion

        public SR2File(String strFileName) 
            : base(strFileName, Game.SR2)
        {
        }

        protected override void ReadHeaderData(BinaryReader xReader)
        {
            m_uDataStart = 0;

            xReader.BaseStream.Position = 0x00000080;
            if (xReader.ReadUInt32() == 0x04C2041D)
            {
                m_eAsset = Asset.Unit;
            }
            else
            {
                m_eAsset = Asset.Object;
            }
        }

        protected override void ReadObjectData(BinaryReader xReader)
        {
            // Object name
            xReader.BaseStream.Position = m_uDataStart + 0x00000024;
            xReader.BaseStream.Position = m_uDataStart + xReader.ReadUInt32();
            String strModelName = new String(xReader.ReadChars(8));
            m_strModelName = Utility.CleanName(strModelName);

            // Texture type
            //xReader.BaseStream.Position = m_uDataStart + 0x44;
            //if (xReader.ReadUInt64() != 0xFFFFFFFFFFFFFFFF)
            //{
            //    m_ePlatform = Platform.PSX;
            //}
            //else
            //{
            m_ePlatform = Platform.PC;
            //}

            // Model data
            xReader.BaseStream.Position = m_uDataStart + 0x0000000C;
            m_usModelCount = 1; //xReader.ReadUInt16();
            m_usAnimCount = 0; //xReader.ReadUInt16();
            m_uModelStart = m_uDataStart + xReader.ReadUInt32();
            m_uAnimStart = 0; //m_uDataStart + xReader.ReadUInt32();

            m_axModels = new SR2Model[m_usModelCount];
            for (UInt16 m = 0; m < m_usModelCount; m++)
            {
                m_axModels[m] = SR2ObjectModel.Load(xReader, m_uDataStart, m_uModelStart, m_strModelName, m_ePlatform, m, m_uVersion);
            }
        }

        protected override void ReadUnitData(BinaryReader xReader)
        {
            // Connected unit names
            xReader.BaseStream.Position = m_uDataStart;
            UInt32 m_uConnectionData = m_uDataStart + xReader.ReadUInt32();
            xReader.BaseStream.Position = m_uConnectionData + 0x24;
            m_uConnectedUnitCount = xReader.ReadUInt32();
            xReader.BaseStream.Position = m_uDataStart + xReader.ReadUInt32();
            m_astrConnectedUnit = new String[m_uConnectedUnitCount];
            for (int i = 0; i < m_uConnectedUnitCount; i++)
            {
                String strUnitName = new String(xReader.ReadChars(12));
                m_astrConnectedUnit[i] = Utility.CleanName(strUnitName);
                xReader.BaseStream.Position += 0x84;
            }

            // Instances
            xReader.BaseStream.Position = m_uDataStart + 0x44;
            m_uInstanceCount = xReader.ReadUInt32();
            m_uInstanceStart = m_uDataStart + xReader.ReadUInt32();
            m_astrInstanceNames = new String[m_uInstanceCount];
            for (int i = 0; i < m_uInstanceCount; i++)
            {
                xReader.BaseStream.Position = m_uInstanceStart + 0x60 * i;
                String strInstanceName = new String(xReader.ReadChars(8));
                m_astrInstanceNames[i] = Utility.CleanName(strInstanceName);
            }

            // Instance types
            xReader.BaseStream.Position = m_uDataStart + 0x4C;
            m_uInstanceTypesStart = m_uDataStart + xReader.ReadUInt32();
            xReader.BaseStream.Position = m_uInstanceTypesStart;
            List<String> xInstanceList = new List<String>();
            while (xReader.ReadByte() != 0xFF)
            {
                xReader.BaseStream.Position--;
                String strInstanceTypeName = new String(xReader.ReadChars(8));
                xInstanceList.Add(Utility.CleanName(strInstanceTypeName));
                xReader.BaseStream.Position += 0x08;
            }
            m_axInstanceTypeNames = xInstanceList.ToArray();

            // Unit name
            xReader.BaseStream.Position = m_uDataStart + 0x50;
            xReader.BaseStream.Position = m_uDataStart + xReader.ReadUInt32();
            String strModelName = new String(xReader.ReadChars(10)); // Need to check
            m_strModelName = Utility.CleanName(strModelName);

            // Texture type
            //xReader.BaseStream.Position = m_uDataStart + 0x9C;
            //if (xReader.ReadUInt64() != 0xFFFFFFFFFFFFFFFF)
            //{
            //    m_ePlatform = Platform.PSX;
            //}
            //else
            //{
                m_ePlatform = Platform.PC;
            //}

            // Model data
            xReader.BaseStream.Position = m_uDataStart;
            m_usModelCount = 2;
            m_uModelStart = m_uDataStart;
            m_axModels = new SR2Model[m_usModelCount];
            xReader.BaseStream.Position = m_uModelStart;
            UInt32 m_uModelData = m_uDataStart + xReader.ReadUInt32();

            // Material data
            m_axModels[0] = SR2UnitModel.Load(xReader, m_uDataStart, m_uModelData, m_strModelName, m_ePlatform, Realm.Material, m_uVersion);

            // Spectral data
            m_axModels[1] = SR2UnitModel.Load(xReader, m_uDataStart, m_uModelData, m_strModelName, m_ePlatform, Realm.Spectral, m_uVersion);

            //if (m_axModels[0].Platform == Platform.Dreamcast ||
            //    m_axModels[1].Platform == Platform.Dreamcast)
            //{
            //    m_ePlatform = Platform.Dreamcast;
            //}
        }

        protected override void ResolvePointers(BinaryReader xReader, BinaryWriter xWriter)
        {
            xReader.BaseStream.Position = 0;
            xWriter.BaseStream.Position = 0;

            xReader.BaseStream.Position += 0x04;
            UInt32 uRegionCount = xReader.ReadUInt32();

            UInt32 uTotal = 0;
            UInt32[] auRegionSizes = new UInt32[uRegionCount];
            UInt32[] auRegionPositions = new UInt32[uRegionCount];
            for (UInt32 r = 0; r < uRegionCount; r++)
            {
                auRegionSizes[r] = xReader.ReadUInt32();
                auRegionPositions[r] = uTotal;
                uTotal += auRegionSizes[r];
                xReader.BaseStream.Position += 0x08;
            }

            UInt32 uRegionDataSize = uRegionCount * 0x0C;
            UInt32 uPointerData = (uRegionDataSize & 0x00000003) + ((uRegionDataSize + 0x17) & 0xFFFFFFF0);
            for (UInt32 r = 0; r < uRegionCount; r++)
            {
                xReader.BaseStream.Position = uPointerData;
                UInt32 uPointerCount = xReader.ReadUInt32();
                UInt32 uPointerDataSize = uPointerCount * 0x04;
                UInt32 uObjectData = uPointerData + ((uPointerDataSize + 0x13) & 0xFFFFFFF0);
                UInt32 uObjectDataSize = (auRegionSizes[r] + 0x0F) & 0xFFFFFFF0;

                xReader.BaseStream.Position = uObjectData;
                xWriter.BaseStream.Position = auRegionPositions[r];
                Byte[] auObjectData = xReader.ReadBytes((Int32)auRegionSizes[r]);
                xWriter.Write(auObjectData);

                xReader.BaseStream.Position = uPointerData + 0x04;
                UInt32[] auAddresses = new UInt32[uPointerCount];
                for (UInt32 p = 0; p < uPointerCount; p++)
                {
                    auAddresses[p] = xReader.ReadUInt32();
                }

                UInt32[] auValues = new UInt32[uPointerCount];
                for (UInt32 p = 0; p < uPointerCount; p++)
                {
                    xReader.BaseStream.Position = uObjectData + auAddresses[p];
                    UInt32 uValue1 = xReader.ReadUInt32();
                    UInt32 uValue2 = uValue1 & 0x003FFFFF;
                    UInt32 uValue3 = uValue1 >> 0x16;

                    auAddresses[p] += auRegionPositions[r];
                    auValues[p] = auRegionPositions[uValue3] + uValue2;

                    xWriter.BaseStream.Position = auAddresses[p];
                    xWriter.Write(auValues[p]);
                }

                uPointerData = uObjectData + uObjectDataSize;
            }
        }
    }
}