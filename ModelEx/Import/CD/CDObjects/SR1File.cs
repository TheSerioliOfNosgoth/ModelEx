using System;
using System.IO;
using System.Collections.Generic;

namespace ModelEx
{
    public abstract class SR1Model : SRModel
    {
        #region Normals
        protected static Int32[,] s_aiNormals =
        {
            {0, 0, 4096},
			{-1930, -3344, -1365},
			{3861, 0, -1365},
			{-1930, 3344, -1365},
			{-353, -613, 4034},
			{-697, -1207, 3851},
			{-1019, -1765, 3552},
			{-1311, -2270, 3146},
			{-1563, -2707, 2645},
			{-1768, -3063, 2065},
			{-1920, -3326, 1423},
			{-2014, -3489, 738},
			{-2047, -3547, 30},
			{-2019, -3498, -677},
			{707, 0, 4034},
			{1394, 0, 3851},
			{2039, 0, 3552},
			{2622, 0, 3146},
			{3126, 0, 2645},
			{3536, 0, 2065},
			{3840, 0, 1423},
			{4028, 0, 738},
			{4095, 0, 30},
			{4039, 0, -677},
			{-353, 613, 4034},
			{-697, 1207, 3851},
			{-1019, 1765, 3552},
			{-1311, 2270, 3146},
			{-1563, 2707, 2645},
			{-1768, 3063, 2065},
			{-1920, 3326, 1423},
			{-2014, 3489, 738},
			{-2047, 3547, 30},
			{-2019, 3498, -677},
			{-1311, -3498, -1678},
			{-653, -3547, -1941},
			{24, -3489, -2145},
			{701, -3326, -2285},
			{1358, -3063, -2355},
			{1973, -2707, -2355},
			{2529, -2270, -2285},
			{3009, -1765, -2145},
			{3398, -1207, -1941},
			{3685, -613, -1678},
			{3685, 613, -1678},
			{3398, 1207, -1941},
			{3009, 1765, -2145},
			{2529, 2270, -2285},
			{1973, 2707, -2355},
			{1358, 3063, -2355},
			{701, 3326, -2285},
			{24, 3489, -2145},
			{-653, 3547, -1941},
			{-1311, 3498, -1678},
			{-2373, 2885, -1678},
			{-2745, 2339, -1941},
			{-3033, 1723, -2145},
			{-3231, 1055, -2285},
			{-3331, 355, -2355},
			{-3331, -355, -2355},
			{-3231, -1055, -2285},
			{-3033, -1723, -2145},
			{-2745, -2339, -1941},
			{-2373, -2885, -1678},
			{364, -631, 4030},
			{33, -1270, 3893},
			{1083, -664, 3893},
			{-273, -1899, 3618},
			{787, -1364, 3780},
			{1781, -712, 3618},
			{-544, -2497, 3200},
			{520, -2080, 3489},
			{1541, -1490, 3489},
			{2435, -777, 3200},
			{-767, -3043, 2631},
			{293, -2785, 2989},
			{1331, -2306, 3111},
			{2265, -1646, 2989},
			{3019, -857, 2631},
			{-939, -3504, 1901},
			{110, -3426, 2240},
			{1151, -3100, 2416},
			{2108, -2547, 2416},
			{2912, -1808, 2240},
			{3504, -938, 1901},
			{-1067, -3821, 1017},
			{-52, -3906, 1230},
			{966, -3739, 1364},
			{1922, -3330, 1409},
			{2755, -2706, 1364},
			{3409, -1907, 1230},
			{3843, -985, 1017},
			{-1174, -3923, 42},
			{-238, -4088, 51},
			{711, -4033, 58},
			{1622, -3760, 61},
			{2445, -3285, 61},
			{3137, -2632, 58},
			{3660, -1838, 51},
			{3985, -944, 42},
			{-1265, -3792, -889},
			{-457, -3928, -1064},
			{368, -3900, -1194},
			{1179, -3709, -1275},
			{1941, -3363, -1302},
			{2622, -2876, -1275},
			{3193, -2269, -1194},
			{3631, -1567, -1064},
			{3917, -800, -889},
			{364, 631, 4030},
			{1083, 664, 3893},
			{33, 1270, 3893},
			{1781, 712, 3618},
			{787, 1364, 3780},
			{-273, 1899, 3618},
			{2435, 777, 3200},
			{1541, 1490, 3489},
			{520, 2080, 3489},
			{-544, 2497, 3200},
			{3019, 857, 2631},
			{2265, 1646, 2989},
			{1331, 2306, 3111},
			{293, 2785, 2989},
			{-767, 3043, 2631},
			{3504, 938, 1901},
			{2912, 1808, 2240},
			{2108, 2547, 2416},
			{1151, 3100, 2416},
			{110, 3426, 2240},
			{-939, 3504, 1901},
			{3843, 985, 1017},
			{3409, 1907, 1230},
			{2755, 2706, 1364},
			{1922, 3330, 1409},
			{966, 3739, 1364},
			{-52, 3906, 1230},
			{-1067, 3821, 1017},
			{3985, 944, 42},
			{3660, 1838, 51},
			{3137, 2632, 58},
			{2445, 3285, 61},
			{1622, 3760, 61},
			{711, 4033, 58},
			{-238, 4088, 51},
			{-1174, 3923, 42},
			{3917, 800, -889},
			{3631, 1567, -1064},
			{3193, 2269, -1194},
			{2622, 2876, -1275},
			{1941, 3363, -1302},
			{1179, 3709, -1275},
			{368, 3900, -1194},
			{-457, 3928, -1064},
			{-1265, 3792, -889},
			{-729, 0, 4030},
			{-1117, 606, 3893},
			{-1117, -606, 3893},
			{-1507, 1186, 3618},
			{-1575, 0, 3780},
			{-1507, -1186, 3618},
			{-1890, 1719, 3200},
			{-2061, 589, 3489},
			{-2061, -589, 3489},
			{-1890, -1719, 3200},
			{-2252, 2186, 2631},
			{-2558, 1138, 2989},
			{-2663, 0, 3111},
			{-2558, -1138, 2989},
			{-2252, -2186, 2631},
			{-2565, 2565, 1901},
			{-3022, 1618, 2240},
			{-3260, 552, 2416},
			{-3260, -552, 2416},
			{-3022, -1618, 2240},
			{-2565, -2565, 1901},
			{-2775, 2835, 1017},
			{-3356, 1998, 1230},
			{-3721, 1032, 1364},
			{-3845, 0, 1409},
			{-3721, -1032, 1364},
			{-3356, -1998, 1230},
			{-2775, -2835, 1017},
			{-2810, 2979, 42},
			{-3421, 2250, 51},
			{-3848, 1400, 58},
			{-4067, 475, 61},
			{-4067, -475, 61},
			{-3848, -1400, 58},
			{-3421, -2250, 51},
			{-2810, -2979, 42},
			{-2652, 2992, -889},
			{-3173, 2360, -1064},
			{-3562, 1630, -1194},
			{-3802, 832, -1275},
			{-3883, 0, -1302},
			{-3802, -832, -1275},
			{-3562, -1630, -1194},
			{-3173, -2360, -1064},
			{-2652, -2992, -889},
			{-1778, -3080, -2031},
			{-2174, -2553, -2351},
			{-1124, -3159, -2351},
			{-2482, -1926, -2627},
			{-1519, -2632, -2745},
			{-427, -3112, -2627},
			{-2683, -1207, -2849},
			{-1812, -1959, -3107},
			{-790, -2548, -3107},
			{295, -2927, -2849},
			{-2758, -404, -3000},
			{-1968, -1132, -3408},
			{-1022, -1771, -3548},
			{3, -2270, -3408},
			{1028, -2591, -3000},
			{-2690, 470, -3052},
			{-1953, -147, -3596},
			{-1074, -755, -3879},
			{-117, -1308, -3879},
			{848, -1766, -3596},
			{1753, -2094, -3052},
			{-2472, 1388, -2955},
			{-1751, 963, -3575},
			{-917, 476, -3963},
			{-23, -40, -4095},
			{871, -555, -3963},
			{1710, -1034, -3575},
			{2438, -1447, -2955},
			{-2131, 2266, -2664},
			{-1403, 2070, -3243},
			{-599, 1763, -3647},
			{237, 1361, -3855},
			{1060, 886, -3855},
			{1827, 363, -3647},
			{2495, -179, -3243},
			{3028, -712, -2664},
			{-1729, 2987, -2203},
			{-1013, 2965, -2637},
			{-255, 2819, -2960},
			{513, 2555, -3159},
			{1261, 2184, -3227},
			{1956, 1722, -3159},
			{2569, 1188, -2960},
			{3075, 604, -2637},
			{3452, -4, -2203}
        };
        #endregion

        protected SR1Model(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt32 uVersion) :
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
            m_auColours = new UInt32[m_uVertexCount];
            ReadVertices(xReader);

            // Get the polygons
            m_axPolygons = new ExPolygon[m_uPolygonCount];
            m_axUVs = new ExUV[m_uIndexCount];
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

            // Before transformation, the world coords equal the local coords
            m_axPositions[v].worldPos = m_axPositions[v].localPos;
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

        protected virtual void ReadMaterial(BinaryReader xReader, int p)
        {
            int v1 = (p * 3) + 0;
            int v2 = (p * 3) + 1;
            int v3 = (p * 3) + 2;

            m_axPolygons[p].v1.UVID = v1;
            m_axPolygons[p].v2.UVID = v2;
            m_axPolygons[p].v3.UVID = v3;

            if (m_ePlatform != Platform.Dreamcast)
            {
                Byte v1U = xReader.ReadByte();
                Byte v1V = xReader.ReadByte();

                if (m_ePlatform == Platform.PSX)
                {
                    ushort paletteVal = xReader.ReadUInt16();
                    ushort rowVal = (ushort)((ushort)(paletteVal << 2) >> 8);
                    ushort colVal = (ushort)((ushort)(paletteVal << 11) >> 11);
                    m_axPolygons[p].paletteColumn = colVal;
                    m_axPolygons[p].paletteRow = rowVal;
                }
                else
                {
                    m_axPolygons[p].material.textureID = (UInt16)(xReader.ReadUInt16() & 0x07FF);
                }

                Byte v2U = xReader.ReadByte();
                Byte v2V = xReader.ReadByte();

                if (m_ePlatform == Platform.PSX)
                {
                    m_axPolygons[p].material.textureID = (UInt16)(((xReader.ReadUInt16() & 0x07FF) - 8) % 8);
                }
                else
                {
                    UInt16 usTemp = xReader.ReadUInt16();
                }

                Byte v3U = xReader.ReadByte();
                Byte v3V = xReader.ReadByte();

                m_axUVs[v1].u = ((float)v1U) / 255.0f;
                m_axUVs[v1].v = ((float)v1V) / 255.0f;
                m_axUVs[v2].u = ((float)v2U) / 255.0f;
                m_axUVs[v2].v = ((float)v2V) / 255.0f;
                m_axUVs[v3].u = ((float)v3U) / 255.0f;
                m_axUVs[v3].v = ((float)v3V) / 255.0f;

                float fCU = (m_axUVs[v1].u + m_axUVs[v2].u + m_axUVs[v3].u) / 3.0f;
                float fCV = (m_axUVs[v1].v + m_axUVs[v2].v + m_axUVs[v3].v) / 3.0f;
                float fSizeAdjust = 1.0f / 255.0f;      // 2.0f seems to work better for dreamcast
                float fOffsetAdjust = 0.5f / 255.0f;

                Utility.AdjustUVs(ref m_axUVs[v1], fCU, fCV, fSizeAdjust, fOffsetAdjust);
                Utility.AdjustUVs(ref m_axUVs[v2], fCU, fCV, fSizeAdjust, fOffsetAdjust);
                Utility.AdjustUVs(ref m_axUVs[v3], fCU, fCV, fSizeAdjust, fOffsetAdjust);
            }
            else
            {
                UInt16 v1U = xReader.ReadUInt16();
                UInt16 v1V = xReader.ReadUInt16();
                UInt16 v2U = xReader.ReadUInt16();
                UInt16 v2V = xReader.ReadUInt16();
                UInt16 v3U = xReader.ReadUInt16();
                UInt16 v3V = xReader.ReadUInt16();

                m_axUVs[v1].u = Utility.BizarreFloatToNormalFloat(v1U);
                m_axUVs[v1].v = Utility.BizarreFloatToNormalFloat(v1V);
                m_axUVs[v2].u = Utility.BizarreFloatToNormalFloat(v2U);
                m_axUVs[v2].v = Utility.BizarreFloatToNormalFloat(v2V);
                m_axUVs[v3].u = Utility.BizarreFloatToNormalFloat(v3U);
                m_axUVs[v3].v = Utility.BizarreFloatToNormalFloat(v3V);

                m_axPolygons[p].material.textureID = (UInt16)((xReader.ReadUInt16() & 0x07FF) - 1);
            }

            m_axPolygons[p].material.colour = 0xFFFFFFFF;

            return;
        }

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

    public class SR1File : SRFile
    {
        public const UInt32 BETA_VERSION = 0x3c204139;
        public const UInt32 RETAIL_VERSION = 0x3C20413B;

        #region Model classes

        protected class SR1ObjectModel : SR1Model
        {
            protected SR1ObjectModel(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt32 uVersion)
                : base (xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion)
            {
                xReader.BaseStream.Position = m_uModelData;
                m_uVertexCount              = xReader.ReadUInt32();
                m_uVertexStart              = m_uDataStart + xReader.ReadUInt32();
                m_xVertexScale.x            = 1.0f;
                m_xVertexScale.y            = 1.0f;
                m_xVertexScale.z            = 1.0f;
                xReader.BaseStream.Position += 0x08;
                m_uPolygonCount             = xReader.ReadUInt32();
                m_uPolygonStart             = m_uDataStart + xReader.ReadUInt32();
                m_uBoneCount                = xReader.ReadUInt32();
                m_uBoneStart                = m_uDataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position += 0x10;
                m_uMaterialStart            = m_uDataStart + xReader.ReadUInt32();
                m_uMaterialCount            = 0;
                m_uTreeCount                = 1;

                m_axTrees = new ExTree[m_uTreeCount];
            }

            public static SR1ObjectModel Load(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt16 usIndex, UInt32 uVersion)
            {
                xReader.BaseStream.Position = uModelData + (0x00000004 * usIndex);
                uModelData = uDataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position = uModelData;
                SR1ObjectModel xModel = new SR1ObjectModel(xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion);
                xModel.ReadData(xReader);
                return xModel;
            }

            protected override void ReadVertex(BinaryReader xReader, int v)
            {
                base.ReadVertex(xReader, v);

                m_axVertices[v].normalID = xReader.ReadUInt16();
            }

            protected override void ReadVertices(BinaryReader xReader)
            {
                base.ReadVertices(xReader);

                ReadArmature(xReader);
                ApplyArmature();
            }

            protected virtual void ReadArmature(BinaryReader xReader)
            {
                if (m_uBoneStart == 0 || m_uBoneCount == 0) return;

                xReader.BaseStream.Position = m_uBoneStart;
                m_axBones = new ExBone[m_uBoneCount];
                m_axBones = new ExBone[m_uBoneCount];
                for (UInt16 b = 0; b < m_uBoneCount; b++)
                {
                    // Get the bone data
                    xReader.BaseStream.Position += 8;
                    m_axBones[b].vFirst = xReader.ReadUInt16();
                    m_axBones[b].vLast = xReader.ReadUInt16();
                    m_axBones[b].localPos.x = (float)xReader.ReadInt16();
                    m_axBones[b].localPos.y = (float)xReader.ReadInt16();
                    m_axBones[b].localPos.z = (float)xReader.ReadInt16();
                    m_axBones[b].parentID1 = xReader.ReadUInt16();

                    // Combine this bone with it's ancestors is there are any
                    if ((m_axBones[b].vFirst != 0xFFFF) && (m_axBones[b].vLast != 0xFFFF))
                    {
                        for (UInt16 ancestorID = b; ancestorID != 0xFFFF; )
                        {
                            m_axBones[b].worldPos += m_axBones[ancestorID].localPos;
                            if (m_axBones[ancestorID].parentID1 == ancestorID) break;
                            ancestorID = m_axBones[ancestorID].parentID1;
                        }
                    }
                    xReader.BaseStream.Position += 4;
                }
                return;
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
                            m_axPositions[v].boneID = b;
                        }
                    }
                }
                return;
            }

            protected virtual void ReadPolygon(BinaryReader xReader, int p)
            {
                UInt32 uPolygonPosition = (UInt32)xReader.BaseStream.Position;

                m_axPolygons[p].v1 = m_axVertices[xReader.ReadUInt16()];
                m_axPolygons[p].v2 = m_axVertices[xReader.ReadUInt16()];
                m_axPolygons[p].v3 = m_axVertices[xReader.ReadUInt16()];

                m_axPolygons[p].material = new ExMaterial();
                m_axPolygons[p].material.visible = true;
                m_axPolygons[p].material.textureUsed = (Boolean)(((int)xReader.ReadUInt16() & 0x0200) != 0);

                if (m_axPolygons[p].material.textureUsed)
                {
                    // WIP
                    UInt32 uMaterialPosition = m_uDataStart + xReader.ReadUInt32();
                    if ((((uMaterialPosition - m_uMaterialStart) % 0x10) != 0) &&
                         ((uMaterialPosition - m_uMaterialStart) % 0x18) == 0)
                    {
                        m_ePlatform = Platform.Dreamcast;
                    }

                    xReader.BaseStream.Position = uMaterialPosition;
                    ReadMaterial(xReader, p);

                    if (m_ePlatform == Platform.Dreamcast)
                    {
                        xReader.BaseStream.Position += 0x06;
                    }
                    else
                    {
                        xReader.BaseStream.Position += 0x02;
                    }

                    m_axPolygons[p].material.colour = xReader.ReadUInt32();
                    m_axPolygons[p].material.colour |= 0xFF000000;

                }
                else
                {
                    m_axPolygons[p].material.colour = xReader.ReadUInt32() | 0xFF000000;
                }

                Utility.FlipRedAndBlue(ref m_axPolygons[p].material.colour);

                xReader.BaseStream.Position = uPolygonPosition + 0x0C;
            }

            protected override void ReadPolygons(BinaryReader xReader)
            {
                if (m_uPolygonStart == 0 || m_uPolygonCount == 0)
                {
                    return;
                }

                xReader.BaseStream.Position = m_uPolygonStart;

                ExMaterialList xMaterialsList = null;

                for (UInt16 p = 0; p < m_uPolygonCount; p++)
                {
                    ReadPolygon(xReader, p);

                    if (xMaterialsList == null)
                    {
                        xMaterialsList = new ExMaterialList(m_axPolygons[p].material);
                        m_xMaterialsList.Add(m_axPolygons[p].material);
                    }
                    else
                    {
                        ExMaterial newMaterial = xMaterialsList.AddToList(m_axPolygons[p].material);
                        if (m_axPolygons[p].material != newMaterial)
                        {
                            m_axPolygons[p].material = newMaterial;
                        }
                        else
                        {
                            m_xMaterialsList.Add(m_axPolygons[p].material);
                        }
                    }
                }

                m_uMaterialCount = (UInt32)m_xMaterialsList.Count;

                for (UInt32 t = 0; t < m_uTreeCount; t++)
                {
                    m_axTrees[t] = new ExTree();
                    m_axTrees[t].m_xMesh = new ExMesh();
                    m_axTrees[t].m_xMesh.m_uIndexCount = m_uIndexCount;
                    m_axTrees[t].m_xMesh.m_uPolygonCount = m_uPolygonCount;
                    m_axTrees[t].m_xMesh.m_axPolygons = m_axPolygons;
                    m_axTrees[t].m_xMesh.m_axVertices = m_axVertices;

                    // Make the vertices unique - Because I do the same thing in GenerateOutput
                    m_axTrees[t].m_xMesh.m_axVertices = new ExVertex[m_uIndexCount];
                    for (UInt16 poly = 0; poly < m_uPolygonCount; poly++)
                    {
                        m_axTrees[t].m_xMesh.m_axVertices[(3 * poly) + 0] = m_axPolygons[poly].v1;
                        m_axTrees[t].m_xMesh.m_axVertices[(3 * poly) + 1] = m_axPolygons[poly].v2;
                        m_axTrees[t].m_xMesh.m_axVertices[(3 * poly) + 2] = m_axPolygons[poly].v3;
                    }
                }
            }
        }

        protected class SR1UnitModel : SR1Model
        {
            protected UInt32 m_uBspTreeCount;
            protected UInt32 m_uBspTreeStart;
            protected Realm m_eRealm;
            protected UInt32 m_uSpectralVertexStart;
            protected UInt32 m_uSpectralColourStart;

            protected SR1UnitModel(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, Realm eRealm, UInt32 uVersion)
                : base (xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion)
            {
                m_eRealm                    = eRealm;
                xReader.BaseStream.Position = m_uModelData + 0x10;
                m_uVertexCount              = xReader.ReadUInt32();
                m_uPolygonCount             = xReader.ReadUInt32();
                xReader.BaseStream.Position += 0x04;
                m_uVertexStart              = m_uDataStart + xReader.ReadUInt32();
                m_uPolygonStart             = m_uDataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position += 0x10;
                m_uMaterialStart            = m_uDataStart + xReader.ReadUInt32();
                m_uMaterialCount            = 0;
                xReader.BaseStream.Position += 0x04;

                if (m_uVersion == BETA_VERSION)
                {
                    xReader.BaseStream.Position += 0x08;
                }

                m_uSpectralVertexStart      = m_uDataStart + xReader.ReadUInt32();
                m_uSpectralColourStart      = m_uDataStart + xReader.ReadUInt32();
                m_uBspTreeCount             = xReader.ReadUInt32();
                m_uBspTreeStart             = m_uDataStart + xReader.ReadUInt32();
                m_uTreeCount                = m_uBspTreeCount;

                m_axTrees = new ExTree[m_uTreeCount];
            }

            public static SR1UnitModel Load(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, Realm eRealm, UInt32 uVersion)
            {
                SR1UnitModel xModel = new SR1UnitModel(xReader, uDataStart, uModelData, strModelName, ePlatform, eRealm, uVersion);
                xModel.ReadData(xReader);
                return xModel;
            }

            protected override void ReadVertex(BinaryReader xReader, int v)
            {
                base.ReadVertex(xReader, v);

                m_axVertices[v].colourID = v;

                xReader.BaseStream.Position += 2;
                m_auColours[v] = xReader.ReadUInt32() | 0xFF000000;

                if (m_ePlatform != Platform.Dreamcast)
                {
                    Utility.FlipRedAndBlue(ref m_auColours[v]);
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
                        UInt32 uShiftColour = xReader.ReadUInt16();
                        UInt32 uAlpha = m_auColours[v] & 0xFF000000;
                        UInt32 uRed = ((uShiftColour >> 0) & 0x1F) << 0x13;
                        UInt32 uGreen = ((uShiftColour >> 5) & 0x1F) << 0x0B;
                        UInt32 uBlue = ((uShiftColour >> 10) & 0x1F) << 0x03;
                        m_auColours[v] = uAlpha | uRed | uGreen | uBlue;
                    }

                    // Spectral Verticices
                    xReader.BaseStream.Position = m_uSpectralVertexStart + 0x06;
                    int sVertex = xReader.ReadInt16();
                    xReader.BaseStream.Position = m_uSpectralVertexStart;
                    while (sVertex != 0xFFFF)
                    {
                        ExShiftVertex xShiftVertex;
                        xShiftVertex.basePos.x = (float)xReader.ReadInt16();
                        xShiftVertex.basePos.y = (float)xReader.ReadInt16();
                        xShiftVertex.basePos.z = (float)xReader.ReadInt16();
                        sVertex = xReader.ReadUInt16();

                        if (sVertex == 0xFFFF)
                        {
                            break;
                        }

                        xShiftVertex.offset.x = (float)xReader.ReadInt16();
                        xShiftVertex.offset.y = (float)xReader.ReadInt16();
                        xShiftVertex.offset.z = (float)xReader.ReadInt16();
                        m_axPositions[sVertex].localPos = xShiftVertex.offset + xShiftVertex.basePos;
                        m_axPositions[sVertex].worldPos = m_axPositions[sVertex].localPos;
                    }
                }
            }

            protected virtual void ReadPolygon(BinaryReader xReader, int p)
            {
                UInt32 uPolygonPosition = (UInt32)xReader.BaseStream.Position;

                m_axPolygons[p].v1 = m_axVertices[xReader.ReadUInt16()];
                m_axPolygons[p].v2 = m_axVertices[xReader.ReadUInt16()];
                m_axPolygons[p].v3 = m_axVertices[xReader.ReadUInt16()];
                m_axPolygons[p].material = new ExMaterial();

                m_axPolygons[p].material.textureUsed |= (Boolean)(((int)xReader.ReadUInt16() & 0x0004) == 0);
                xReader.BaseStream.Position += 0x02;
                UInt16 uMaterialOffset = xReader.ReadUInt16();
                m_axPolygons[p].material.textureUsed &= (Boolean)(uMaterialOffset != 0xFFFF);

                if (m_axPolygons[p].material.textureUsed)
                {
                    // WIP
                    UInt32 uMaterialPosition = uMaterialOffset + m_uMaterialStart;
                    if ((((uMaterialPosition - m_uMaterialStart) % 0x0C) != 0) &&
                         ((uMaterialPosition - m_uMaterialStart) % 0x14) == 0)
                    {
                        m_ePlatform = Platform.Dreamcast;
                    }

                    xReader.BaseStream.Position = uMaterialPosition;
                    ReadMaterial(xReader, p);
                }
                else
                {
                    m_axPolygons[p].material.textureUsed = false;
                    m_axPolygons[p].material.colour = 0xFFFFFFFF;
                }

                Utility.FlipRedAndBlue(ref m_axPolygons[p].material.colour);

                xReader.BaseStream.Position = uPolygonPosition + 0x0C;
            }

            protected override void ReadPolygons(BinaryReader xReader)
            {
                if (m_uPolygonStart == 0 || m_uPolygonCount == 0)
                {
                    return;
                }

                xReader.BaseStream.Position = m_uPolygonStart;

                for (UInt16 p = 0; p < m_uPolygonCount; p++)
                {
                    ReadPolygon(xReader, p);
                }

                MemoryStream xPolyStream = new MemoryStream((Int32)m_uVertexCount * 3);
                BinaryWriter xPolyWriter = new BinaryWriter(xPolyStream);
                BinaryReader xPolyReader = new BinaryReader(xPolyStream);

                List<ExMesh> xMeshes = new List<ExMesh>();
                List<Int64> xMeshPositions = new List<Int64>();

                for (UInt32 t = 0; t < m_uBspTreeCount; t++)
                {
                    xReader.BaseStream.Position = m_uBspTreeStart + (t * 0x24);
                    UInt32 uDataPos = m_uDataStart + xReader.ReadUInt32();
                    xReader.BaseStream.Position += 0x0C;
                    bool drawTester = ((xReader.ReadInt32() & 1) != 1);
                    xReader.BaseStream.Position += 0x06;
                    UInt16 usBspID = xReader.ReadUInt16();

                    m_axTrees[t] = ReadBSPTree(xReader, xPolyWriter, uDataPos, m_axTrees[t], xMeshes, xMeshPositions, 0);
                }

                ExMaterialList xMaterialsList = null;

                for (UInt16 p = 0; p < m_uPolygonCount; p++)
                {
                    if (xMaterialsList == null)
                    {
                        xMaterialsList = new ExMaterialList(m_axPolygons[p].material);
                        m_xMaterialsList.Add(m_axPolygons[p].material);
                    }
                    else
                    {
                        ExMaterial newMaterial = xMaterialsList.AddToList(m_axPolygons[p].material);
                        if (m_axPolygons[p].material != newMaterial)
                        {
                            m_axPolygons[p].material = newMaterial;
                        }
                        else
                        {
                            m_xMaterialsList.Add(m_axPolygons[p].material);
                        }
                    }
                }

                m_uMaterialCount = (UInt32)m_xMaterialsList.Count;

                xPolyReader.BaseStream.Position = 0;
                for (int m = 0; m < xMeshes.Count; m++)
                {
                    ExMesh xCurrentMesh = xMeshes[m];
                    Int64 iStartPosition = xPolyReader.BaseStream.Position;
                    Int64 iEndPosition = xMeshPositions[m];
                    Int64 iRange = iEndPosition - iStartPosition;
                    UInt32 uIndexCount = (UInt32)iRange / 4;

                    FinaliseMesh(xPolyReader, xCurrentMesh, uIndexCount);
                }
            }

            protected virtual ExTree ReadBSPTree(BinaryReader xReader, BinaryWriter xPolyWriter, UInt32 uDataPos, ExTree xParentTree, List<ExMesh> xMeshes, List<Int64> xMeshPositions, UInt32 uDepth)
            {
                if (uDataPos == 0)
                {
                    return null;
                }

                xReader.BaseStream.Position = uDataPos + 0x0E;
                bool isLeaf = ((xReader.ReadByte() & 0x02) == 0x02);
                Int32 iSubTreeCount = 2;

                ExTree xTree = null;
                ExMesh xMesh = null;

                UInt32 uMaxDepth = 0;

                if (uDepth <= uMaxDepth)
                {
                    xTree = new ExTree();
                    xMesh = new ExMesh();
                    xTree.m_xMesh = xMesh;

                    if (xParentTree != null)
                    {
                        xParentTree.Push(xTree);
                    }
                }
                else
                {
                    xTree = xParentTree;
                    xMesh = xParentTree.m_xMesh;
                }

                if (isLeaf)
                {
                    xTree.isLeaf = true;

                    xReader.BaseStream.Position = uDataPos + 0x08;
                    ReadBSPLeaf(xReader, xPolyWriter, xMesh);
                }
                else
                {
                    xReader.BaseStream.Position = uDataPos + 0x14;

                    UInt32[] auSubTreePositions = new UInt32[2];
                    for (Int32 s = 0; s < iSubTreeCount; s++)
                    {
                        auSubTreePositions[s] = xReader.ReadUInt32();
                    }

                    for (Int32 s = iSubTreeCount - 1; s >= 0; s--)
                    {
                        ReadBSPTree(xReader, xPolyWriter, auSubTreePositions[s], xTree, xMeshes, xMeshPositions, uDepth + 1);
                    }
                }

                if (uDepth <= uMaxDepth)
                {
                    if (xMesh != null && xMesh.m_uIndexCount > 0)
                    {
                        xMeshes.Add(xMesh);
                        xMeshPositions.Add(xPolyWriter.BaseStream.Position);
                    }
                }

                return xTree;
            }

            protected virtual void ReadBSPLeaf(BinaryReader xReader, BinaryWriter xPolyWriter, ExMesh xMesh)
            {
                UInt32 polygonPos = m_uDataStart + xReader.ReadUInt32();
                UInt32 polygonID = (polygonPos - m_uPolygonStart) / 0x0C;
                UInt16 polyCount = xReader.ReadUInt16();
                for (UInt16 p = 0; p < polyCount; p++)
                {
                    m_axPolygons[polygonID + p].material.visible = true;

                    xPolyWriter.Write(polygonID + p);

                    if (xMesh != null)
                    {
                        xMesh.m_uIndexCount += 3;
                    }
                }
            }

            protected virtual void FinaliseMesh(BinaryReader xPolyReader, ExMesh xMesh, UInt32 uIndexCount)
            {
                //xMesh.m_uIndexCount = uIndexCount;
                xMesh.m_uPolygonCount = xMesh.m_uIndexCount / 3;
                xMesh.m_axPolygons = new ExPolygon[xMesh.m_uPolygonCount];
                for (UInt32 p = 0; p < xMesh.m_uPolygonCount; p++)
                {
                    UInt32 polygonID = xPolyReader.ReadUInt32();
                    xMesh.m_axPolygons[p] = m_axPolygons[polygonID];
                }

                // Make the vertices unique - Because I do the same thing in GenerateOutput
                xMesh.m_axVertices = new ExVertex[xMesh.m_uIndexCount];
                for (UInt16 poly = 0; poly < xMesh.m_uPolygonCount; poly++)
                {
                    xMesh.m_axVertices[(3 * poly) + 0] = xMesh.m_axPolygons[poly].v1;
                    xMesh.m_axVertices[(3 * poly) + 1] = xMesh.m_axPolygons[poly].v2;
                    xMesh.m_axVertices[(3 * poly) + 2] = xMesh.m_axPolygons[poly].v3;
                }
            }
        }

        #endregion

        public SR1File(String strFileName)
            : base(strFileName, Game.SR1)
        {
        }

        protected override void ReadHeaderData(BinaryReader xReader)
        {
            m_uDataStart = 0;

            // Could use unit version number instead of thing below.
            // Check that's what SR2 does.
            //xReader.BaseStream.Position = m_uDataStart + 0xF0;
            //UInt32 unitVersionNumber = xReader.ReadUInt32();
            //if (unitVersionNumber != 0x3C20413B)

            // Moved to ResolvePointers due to not knowing how else to tell.
            //xReader.BaseStream.Position = 0x00000000;
            //if (xReader.ReadUInt32() == 0x00000000)
            //{
            //    m_eFileType = FileType.Unit;
            //}
            //else
            //{
            //    m_eFileType = FileType.Object;
            //}
        }

        protected override void ReadObjectData(BinaryReader xReader)
        {
            // Object name
            xReader.BaseStream.Position = m_uDataStart + 0x00000024;
            xReader.BaseStream.Position = m_uDataStart + xReader.ReadUInt32();
            String strModelName = new String(xReader.ReadChars(8));
            m_strModelName = Utility.CleanName(strModelName);

            // Texture type
            xReader.BaseStream.Position = m_uDataStart + 0x44;
            if (xReader.ReadUInt64() != 0xFFFFFFFFFFFFFFFF)
            {
                m_ePlatform = Platform.PSX;
            }
            else
            {
                m_ePlatform = Platform.PC;
            }

            // Model data
            xReader.BaseStream.Position = m_uDataStart + 0x00000008;
            m_usModelCount = xReader.ReadUInt16();
            m_usAnimCount = xReader.ReadUInt16();
            m_uModelStart = m_uDataStart + xReader.ReadUInt32();
            m_uAnimStart = m_uDataStart + xReader.ReadUInt32();

            m_axModels = new SR1Model[m_usModelCount];
            Platform ePlatform = m_ePlatform;
            for (UInt16 m = 0; m < m_usModelCount; m++)
            {
                m_axModels[m] = SR1ObjectModel.Load(xReader, m_uDataStart, m_uModelStart, m_strModelName, m_ePlatform, m, m_uVersion);
                if (m_axModels[m].Platform == Platform.Dreamcast)
                {
                    ePlatform = m_axModels[m].Platform;
                }
            }
            m_ePlatform = ePlatform;
        }

        protected override void ReadUnitData(BinaryReader xReader)
        {
            // Connected unit names
            xReader.BaseStream.Position = m_uDataStart;
            UInt32 m_uConnectionData = m_uDataStart + xReader.ReadUInt32();
            xReader.BaseStream.Position = m_uConnectionData + 0x30;
            xReader.BaseStream.Position = m_uDataStart + xReader.ReadUInt32();
            m_uConnectedUnitCount = xReader.ReadUInt32();
            m_astrConnectedUnit = new String[m_uConnectedUnitCount];
            for (int i = 0; i < m_uConnectedUnitCount; i++)
            {
                String strUnitName = new String(xReader.ReadChars(12));
                m_astrConnectedUnit[i] = Utility.CleanName(strUnitName);
                xReader.BaseStream.Position += 0x50;
            }

            // Instances
            xReader.BaseStream.Position = m_uDataStart + 0x78;
            m_uInstanceCount = xReader.ReadUInt32();
            m_uInstanceStart = m_uDataStart + xReader.ReadUInt32();
            m_astrInstances = new String[m_uInstanceCount];
            for (int i = 0; i < m_uInstanceCount; i++)
            {
                xReader.BaseStream.Position = m_uInstanceStart + 0x4C * i;
                String strInstanceName = new String(xReader.ReadChars(8));
                m_astrInstances[i] = Utility.CleanName(strInstanceName);
            }

            // Instance types
            xReader.BaseStream.Position = m_uDataStart + 0x8C;
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
            xReader.BaseStream.Position = m_uDataStart + 0x98;
            xReader.BaseStream.Position = m_uDataStart + xReader.ReadUInt32();
            String strModelName = new String(xReader.ReadChars(8));
            m_strModelName = Utility.CleanName(strModelName);

            // Texture type
            xReader.BaseStream.Position = m_uDataStart + 0x9C;
            if (xReader.ReadUInt64() != 0xFFFFFFFFFFFFFFFF)
            {
                m_ePlatform = Platform.PSX;
            }
            else
            {
                m_ePlatform = Platform.PC;
            }

            // Connected unit list. (unreferenced?)
            //xReader.BaseStream.Position = m_uDataStart + 0xC0;
            //m_uConnectedUnitsStart = m_uDataStart + xReader.ReadUInt32() + 0x08;
            //xReader.BaseStream.Position = m_uConnectedUnitsStart;
            //xReader.BaseStream.Position += 0x18;
            //String strUnitName0 = new String(xReader.ReadChars(16));
            //strUnitName0 = strUnitName0.Substring(0, strUnitName0.IndexOf(','));
            //xReader.BaseStream.Position += 0x18;
            //String strUnitName1 = new String(xReader.ReadChars(16));
            //strUnitName1 = strUnitName1.Substring(0, strUnitName1.IndexOf(','));
            //xReader.BaseStream.Position += 0x18;
            //String strUnitName2 = new String(xReader.ReadChars(16));
            //strUnitName2 = strUnitName2.Substring(0, strUnitName2.IndexOf(','));

            // Version number
            xReader.BaseStream.Position = m_uDataStart + 0xF0;
            m_uVersion = xReader.ReadUInt32();
            if (m_uVersion != RETAIL_VERSION && m_uVersion != BETA_VERSION)
            {
                throw new Exception("Wrong version number for level x");
            }

            // Model data
            xReader.BaseStream.Position = m_uDataStart;
            m_usModelCount = 2;
            m_uModelStart = m_uDataStart;
            m_axModels = new SR1Model[m_usModelCount];
            xReader.BaseStream.Position = m_uModelStart;
            UInt32 m_uModelData = m_uDataStart + xReader.ReadUInt32();

            // Material data
            m_axModels[0] = SR1UnitModel.Load(xReader, m_uDataStart, m_uModelData, m_strModelName, m_ePlatform, Realm.Material, m_uVersion);

            // Spectral data
            m_axModels[1] = SR1UnitModel.Load(xReader, m_uDataStart, m_uModelData, m_strModelName, m_ePlatform, Realm.Spectral, m_uVersion);

            //if (m_axModels[0].Platform == Platform.Dreamcast ||
            //    m_axModels[1].Platform == Platform.Dreamcast)
            //{
            //    m_ePlatform = Platform.Dreamcast;
            //}
        }

        protected override void ResolvePointers(BinaryReader xReader, BinaryWriter xWriter)
        {
            UInt32 uDataStart = ((xReader.ReadUInt32() >> 9) << 11) + 0x00000800;
            if (xReader.ReadUInt32() == 0x00000000)
            {
                m_eAsset = Asset.Unit;
            }
            else
            {
                m_eAsset = Asset.Object;
            }

            xReader.BaseStream.Position = uDataStart;
            xWriter.BaseStream.Position = 0;

            xReader.BaseStream.CopyTo(xWriter.BaseStream);
        }
    }
}