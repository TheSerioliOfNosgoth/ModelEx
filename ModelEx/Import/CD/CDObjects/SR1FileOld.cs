using System;
using System.IO;
using System.Collections.Generic;

namespace ModelEx3
{
    public enum Platform
    {
        None,
        PC,
        PSX,
        Dreamcast
    }
    public enum FileType
    {
        Object,
        Area
    }
    public enum Realm
    {
        Material,
        Spectral
    }
    public struct ExVector
    {
        public Int16 x, y, z;
        ExVector(Int16 x, Int16 y, Int16 z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public static ExVector operator +(ExVector v1, ExVector v2)
        {
            return new ExVector(
                (Int16)(v1.x + v2.x),
                (Int16)(v1.y + v2.y),
                (Int16)(v1.z + v2.z)
            );
        }
    }
    public struct ExNormal
    {
        public Int32 x, y, z;
    }
    public struct ExBone
    {
        public UInt16 vFirst, vLast;    // The ID of first and last effected vertex 
        public ExVector localPos;       // Local bone coordinates
        public ExVector worldPos;       // World bone coordinated
        public UInt16 parentID;         // ID of parent bone
    }
    public struct ExVertex
    {
        public UInt16 index;            // Index in the file
        public ExVector localPos;       // Local vertex coordinates
        public ExVector worldPos;       // World vertex coordinates
        public UInt16 normalID;         // Index of the vertex normal
        public ExNormal normal;         // Normal for the vertex
        public UInt32 colour;           // Colour of the vertex
        public UInt16 boneID;           // Index of the bone effecting this vertex
        public Byte u, v;               // Texture coordinates
        public float fU, fV;            // Adjusted texture coordinates
    }
    public struct ExShiftVertex
    {
        public UInt16 index;            // Index in the file
        public ExVector basePos;        // Base vertex coordinates
        public ExVector offset;         // Offset from base coordinates
    }
    public struct ExPolygon
    {
        public Boolean isVisible;
        public ExMaterial material;     // The material used
        public ExVertex v1, v2, v3;     // Vertices for the polygon
        public int paletteRow;          // The row of the pallete to use (PS1)
        public int paletteColumn;       // The column of the pallet to use (PS1)
    }
    public class ExMaterial
    {
        public UInt16 ID;               // The ID of the material
        public Boolean textureUsed;     // Flag specifying if a texture is used
        public UInt16 textureID;        // ID of the texture file
        public UInt32 colour;           // Diffuse colour
        public String textureName;      // Name of the texture file
    }
    public class ExMaterialList
    {
        private ExMaterialList _next;
        public ExMaterialList next
        {
            get { return _next; }
        }
        public ExMaterial material;
        public ExMaterialList(ExMaterial material)
        {
            this.material = material;
            _next = null;
        }
        // Tries to add the material to the list
        public ExMaterial AddToList(ExMaterial material)
        {
            // Check if the material is already in the list
            if ((material.textureID == this.material.textureID) &&
                (material.colour == this.material.colour) &&
                (material.textureUsed == this.material.textureUsed))
                return this.material;
            // Check the rest of the list
            if (next != null)
            {
                return next.AddToList(material);
            }
            // Add the material to the list
            _next = new ExMaterialList(material);
            return material;
        }
    }
    public class ExBSPTree
    {
        public UInt32 dataPos;
        public Boolean isLeaf;
        public ExBSPTree leftChild;
        public ExBSPTree rightChild;
    }
    public class ExBSPTreeStack
    {
        private class Node
        {
            public ExBSPTree tree;
            public Node lastNode;
        }
        private Node currentNode;
        public void Push(ExBSPTree tree)
        {
            Node lastNode = currentNode;
            currentNode = new Node();
            currentNode.tree = tree;
            currentNode.lastNode = lastNode;
            return;
        }
        public ExBSPTree Pop()
        {
            if (currentNode == null) return null;
            ExBSPTree tree = currentNode.tree;
            currentNode = currentNode.lastNode;
            return tree;
        }
        public ExBSPTree Top
        {
            get
            {
                if (currentNode == null) return null;
                return currentNode.tree;
            }
        }
    }

    public class ExModel
    {
        #region Normals
        private static Int32[,] s_aiNormals =
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
        public FileType m_eFileType;
        public Platform m_ePlatform;
        public Realm m_eRealm;
        public String m_strModelName;
        public UInt32 m_uDataStart;
        public UInt32 m_uModelData;
        public UInt32 m_uVertexCount;
        public UInt32 m_uVertexStart;
        public UInt32 m_uSpectralVertexStart;
        public UInt32 m_uSpectralColourStart;
        public UInt32 m_uPolygonCount;
        public UInt32 m_uPolygonStart;
        public UInt32 m_uBoneCount;
        public UInt32 m_uBoneStart;
        public UInt16 m_uMaterialCount;
        public UInt32 m_uMaterialStart;
        public UInt32 m_uBspTreeCount;
        public UInt32 m_uBspTreeStart;
        public ExBone[] m_axBones;
        public UInt16[] m_ausIndices;
        public UInt32 m_uIndexCount { get { return 3 * m_uPolygonCount; } }
        public ExVertex[] m_axVertices;
        public ExPolygon[] m_axPolygons;
        public ExMaterial[] m_axMaterials;
        private ExMaterialList m_xMaterialsList;

        public ExModel(String strModelName, UInt32 dataStart, UInt32 modelData, Platform ePlatform, FileType eFileType, Realm eRealm, BinaryReader reader)
        {
            m_strModelName = strModelName;
            m_ePlatform = ePlatform;
            m_eFileType = eFileType;
            m_eRealm = eRealm;
            m_uDataStart = dataStart;
            m_uModelData = modelData;

            switch (eFileType)
            {
                case FileType.Object:
                    MapObjectData(reader);
                    break;
                case FileType.Area:
                    MapAreaData(reader);
                    break;
            }

            // Get the vertices
            ReadVertices(reader);

            if (eFileType == FileType.Object)
            {
                // Get the armature
                ReadArmature(reader);
                // Apply the armature to the vertices
                ApplyArmature();
            }

            // Get the polygons
            ReadPolygons(reader);

            // Genetate the vertices, indices and materials to be output
            GenerateOutputData();
        }

        void MapObjectData(BinaryReader reader)
        {
            reader.BaseStream.Position = m_uModelData;
            m_uVertexCount = reader.ReadUInt32();
            m_uVertexStart = m_uDataStart + reader.ReadUInt32();
            reader.BaseStream.Position += 0x00000008;
            m_uPolygonCount = reader.ReadUInt32();
            m_uPolygonStart = m_uDataStart + reader.ReadUInt32();
            m_uBoneCount = reader.ReadUInt32();
            m_uBoneStart = m_uDataStart + reader.ReadUInt32();
            m_uMaterialCount = 0;
        }

        void MapAreaData(BinaryReader reader)
        {
            reader.BaseStream.Position = m_uModelData + 0x10;
            m_uVertexCount = reader.ReadUInt32();
            m_uPolygonCount = reader.ReadUInt32();
            reader.BaseStream.Position += 0x04;
            m_uVertexStart = m_uDataStart + reader.ReadUInt32();
            m_uPolygonStart = m_uDataStart + reader.ReadUInt32();
            m_uBoneCount = 0;
            m_uBoneStart = 0;
            reader.BaseStream.Position += 0x10;
            m_uMaterialStart = m_uDataStart + reader.ReadUInt32();
            m_uMaterialCount = 0;
            reader.BaseStream.Position += 0x04;
            m_uSpectralVertexStart = m_uDataStart + reader.ReadUInt32();
            m_uSpectralColourStart = m_uDataStart + reader.ReadUInt32();
            m_uBspTreeCount = reader.ReadUInt32();
            m_uBspTreeStart = m_uDataStart + reader.ReadUInt32();
        }

        void GenerateOutputData()
        {
            // Make the vertices unique and generate new index array
            m_axVertices = new ExVertex[m_uIndexCount];
            m_ausIndices = new UInt16[m_uIndexCount];
            for (UInt16 p = 0; p < m_uPolygonCount; p++)
            {
                m_axVertices[(3 * p) + 0] = m_axPolygons[p].v1;
                m_axVertices[(3 * p) + 1] = m_axPolygons[p].v2;
                m_axVertices[(3 * p) + 2] = m_axPolygons[p].v3;
                m_ausIndices[(3 * p) + 0] = (UInt16)((3 * p) + 0);
                m_ausIndices[(3 * p) + 1] = (UInt16)((3 * p) + 1);
                m_ausIndices[(3 * p) + 2] = (UInt16)((3 * p) + 2);
            }

            // Build the materials array
            m_axMaterials = new ExMaterial[m_uMaterialCount];
            UInt16 mNew = 0;

            // Get the untextured materials
            ExMaterialList matList = m_xMaterialsList;
            while (matList != null)
            {
                if (!matList.material.textureUsed)
                {
                    m_axMaterials[mNew] = matList.material;
                    m_axMaterials[mNew].ID = mNew;
                    m_axMaterials[mNew].textureName = "";
                    mNew++;
                }
                matList = matList.next;
            }

            // Get the textured materials
            matList = m_xMaterialsList;
            while (matList != null)
            {
                if (matList.material.textureUsed)
                {
                    m_axMaterials[mNew] = matList.material;
                    m_axMaterials[mNew].ID = mNew;
                    if (m_ePlatform == Platform.PSX)
                    {
                        m_axMaterials[mNew].textureName =
                            m_strModelName.TrimEnd(new char[] { '_' }).ToLower() + "-" +
                            m_axMaterials[mNew].textureID.ToString("0000") +
                            ".png";
                    }
                    else
                    {
                        m_axMaterials[mNew].textureName =
                            "Texture-" +
                            m_axMaterials[mNew].textureID.ToString("00000") +
                            ".png";
                    }
                    mNew++;
                }
                matList = matList.next;
            }
            return;
        }

        void ReadVertices(BinaryReader reader)
        {
            if (m_uVertexStart == 0 || m_uVertexCount == 0) return;

            reader.BaseStream.Position = m_uVertexStart;
            m_axVertices = new ExVertex[m_uVertexCount];
            for (UInt16 v = 0; v < m_uVertexCount; v++)
            {
                // Read the local coordinates
                m_axVertices[v].localPos.x = reader.ReadInt16();
                m_axVertices[v].localPos.y = reader.ReadInt16();
                m_axVertices[v].localPos.z = reader.ReadInt16();

                // If it's an object get the normals
                if (m_eFileType == FileType.Object)
                {
                    m_axVertices[v].normalID = reader.ReadUInt16();
                    m_axVertices[v].normal.x = s_aiNormals[m_axVertices[v].normalID, 0];
                    m_axVertices[v].normal.y = s_aiNormals[m_axVertices[v].normalID, 1];
                    m_axVertices[v].normal.z = s_aiNormals[m_axVertices[v].normalID, 2];
                }
                // If it's an area get the vertex colours
                if (m_eFileType == FileType.Area)
                {
                    reader.BaseStream.Position += 2;
                    m_axVertices[v].colour = reader.ReadUInt32() | 0xFF000000;

                    if (m_ePlatform != Platform.Dreamcast)
                    {
                        FlipRedAndBlue(ref m_axVertices[v].colour);
                    }
                }

                // Before transformation, the world coords equal the local coords
                m_axVertices[v].worldPos = m_axVertices[v].localPos;

                // The vertex may need to know it's own ID
                m_axVertices[v].index = v;
            }

            if (m_eFileType == FileType.Area && m_eRealm == Realm.Spectral)
            {
                // Spectral Colours
                reader.BaseStream.Position = m_uSpectralColourStart;
                for (int v = 0; v < m_uVertexCount; v++)
                {
                    UInt32 uShiftColour = reader.ReadUInt16();
                    UInt32 uAlpha = m_axVertices[v].colour & 0xFF000000;
                    UInt32 uRed = ((uShiftColour >> 0) & 0x1F) << 0x13;
                    UInt32 uGreen = ((uShiftColour >> 5) & 0x1F) << 0x0B;
                    UInt32 uBlue = ((uShiftColour >> 10) & 0x1F) << 0x03;
                    m_axVertices[v].colour = uAlpha | uRed | uGreen | uBlue;
                }

                // Spectral Verticices
                reader.BaseStream.Position = m_uSpectralVertexStart + 0x06;
                int sVertex = reader.ReadInt16();
                reader.BaseStream.Position = m_uSpectralVertexStart;
                while (sVertex != 0xFFFF)
                {
                    ExShiftVertex xShiftVertex;
                    xShiftVertex.basePos.x = reader.ReadInt16();
                    xShiftVertex.basePos.y = reader.ReadInt16();
                    xShiftVertex.basePos.z = reader.ReadInt16();
                    sVertex = reader.ReadUInt16();

                    if (sVertex == 0xFFFF)
                    {
                        break;
                    }

                    xShiftVertex.offset.x = reader.ReadInt16();
                    xShiftVertex.offset.y = reader.ReadInt16();
                    xShiftVertex.offset.z = reader.ReadInt16();
                    m_axVertices[sVertex].localPos = xShiftVertex.offset + xShiftVertex.basePos;
                    m_axVertices[sVertex].worldPos = m_axVertices[sVertex].localPos;
                }
            }

            return;
        }

        void ReadArmature(BinaryReader reader)
        {
            if (m_uBoneStart == 0 || m_uBoneCount == 0) return;

            reader.BaseStream.Position = m_uBoneStart;
            m_axBones = new ExBone[m_uBoneCount];
            m_axBones = new ExBone[m_uBoneCount];
            for (UInt16 b = 0; b < m_uBoneCount; b++)
            {
                // Get the bone data
                reader.BaseStream.Position += 8;
                m_axBones[b].vFirst = reader.ReadUInt16();
                m_axBones[b].vLast = reader.ReadUInt16();
                m_axBones[b].localPos.x = reader.ReadInt16();
                m_axBones[b].localPos.y = reader.ReadInt16();
                m_axBones[b].localPos.z = reader.ReadInt16();
                m_axBones[b].parentID = reader.ReadUInt16();

                // Combine this bone with it's ancestors is there are any
                if ((m_axBones[b].vFirst != 0xFFFF) && (m_axBones[b].vLast != 0xFFFF))
                {
                    for (UInt16 ancestorID = b; ancestorID != 0xFFFF; )
                    {
                        m_axBones[b].worldPos += m_axBones[ancestorID].localPos;
                        if (m_axBones[ancestorID].parentID == ancestorID) break;
                        ancestorID = m_axBones[ancestorID].parentID;
                    }
                }
                reader.BaseStream.Position += 4;
            }
            return;
        }

        void ApplyArmature()
        {
            if ((m_uVertexStart == 0 || m_uVertexCount == 0) ||
                (m_uBoneStart == 0 || m_uBoneCount == 0)) return;

            for (UInt16 b = 0; b < m_uBoneCount; b++)
            {
                if ((m_axBones[b].vFirst != 0xFFFF) && (m_axBones[b].vLast != 0xFFFF))
                {
                    for (UInt16 v = m_axBones[b].vFirst; v <= m_axBones[b].vLast; v++)
                    {
                        m_axVertices[v].worldPos += m_axBones[b].worldPos;
                        m_axVertices[v].boneID = b;
                    }
                }
            }
            return;
        }

        void ReadPolygons(BinaryReader reader)
        {
            m_axPolygons = new ExPolygon[m_uPolygonCount];
            if (m_eFileType == FileType.Area) ReadBSPTree(reader);
            for (UInt16 p = 0; p < m_uPolygonCount; p++)
            {
                reader.BaseStream.Position = m_uPolygonStart + (p * 12);

                // Copy vertices to the polygon
                m_axPolygons[p].v1 = m_axVertices[reader.ReadUInt16()];
                m_axPolygons[p].v2 = m_axVertices[reader.ReadUInt16()];
                m_axPolygons[p].v3 = m_axVertices[reader.ReadUInt16()];

                m_axPolygons[p].material = new ExMaterial();

                if (m_eFileType == FileType.Object)
                {
                    // Get flag to say if a texture is used
                    m_axPolygons[p].material.textureUsed = (Boolean)(((int)reader.ReadUInt16() & 0x0200) != 0);

                    // Get the texture data if present
                    if (m_axPolygons[p].material.textureUsed)
                    {
                        reader.BaseStream.Position = m_uDataStart + reader.ReadInt32();
                        ReadTextureData(reader, ref m_axPolygons[p]);
                        reader.BaseStream.Position += 2;
                    }
                    else
                    {
                        reader.BaseStream.Position = m_uPolygonStart + (12 * p) + 8;
                    }
                    m_axPolygons[p].material.colour = reader.ReadUInt32() | 0xFF000000;
                }
                if (m_eFileType == FileType.Area)
                {
                    // Get flag to say if a texture is used.  This needs work...  I improved it though :)
                    m_axPolygons[p].material.textureUsed = (Boolean)(((int)reader.ReadUInt16() & 0x0004) == 0);

                    // Get the texture data if present
                    reader.BaseStream.Position += 2;
                    UInt16 materialOffset = reader.ReadUInt16();
                    if (materialOffset != 0xFFFF &&
                        m_axPolygons[p].material.textureUsed &&
                        m_axPolygons[p].isVisible)
                    {
                        reader.BaseStream.Position = m_uMaterialStart + materialOffset;
                        ReadTextureData(reader, ref m_axPolygons[p]);
                        m_axPolygons[p].material.colour = 0xFFFFFFFF;
                    }
                    else
                    {
                        m_axPolygons[p].material.textureUsed = false;
                        m_axPolygons[p].material.colour = 0x00000000;
                        m_axPolygons[p].v1.colour = 0x00000000;
                        m_axPolygons[p].v2.colour = 0x00000000;
                        m_axPolygons[p].v3.colour = 0x00000000;
                    }
                }
                FlipRedAndBlue(ref m_axPolygons[p].material.colour);

                // Add the material to the list
                if (m_xMaterialsList == null)
                {
                    m_xMaterialsList = new ExMaterialList(m_axPolygons[p].material);
                    m_uMaterialCount++;
                }
                else
                {
                    ExMaterial newMaterial = m_xMaterialsList.AddToList(m_axPolygons[p].material);
                    if (m_axPolygons[p].material != newMaterial)
                    {
                        m_axPolygons[p].material = newMaterial;
                    }
                    else
                    {
                        m_uMaterialCount++;
                    }
                }
            }
        }

        void ReadTextureData(BinaryReader reader, ref ExPolygon polygon)
        {
            if (m_ePlatform != Platform.Dreamcast)
            {
                polygon.v1.u = reader.ReadByte();
                polygon.v1.v = reader.ReadByte();
                if (m_ePlatform == Platform.PSX)
                {
                    ushort paletteVal = reader.ReadUInt16();
                    ushort rowVal = (ushort)((ushort)(paletteVal << 2) >> 8);
                    ushort colVal = (ushort)((ushort)(paletteVal << 11) >> 11);
                    polygon.paletteColumn = colVal;
                    polygon.paletteRow = rowVal;
                }
                else
                {
                    UInt16 usTemp = reader.ReadUInt16();
                    reader.BaseStream.Position -= 2;
                    polygon.material.textureID = (UInt16)(reader.ReadUInt16() & 0x07FF);
                }
                polygon.v2.u = reader.ReadByte();
                polygon.v2.v = reader.ReadByte();
                if (m_ePlatform == Platform.PSX)
                {
                    polygon.material.textureID = (UInt16)(((reader.ReadUInt16() & 0x07FF) - 8) % 8);
                }
                else
                {
                    UInt16 usTemp = reader.ReadUInt16();
                }
                polygon.v3.u = reader.ReadByte();
                polygon.v3.v = reader.ReadByte();

                polygon.v1.fU = ((float)polygon.v1.u) / 255.0f;
                polygon.v1.fV = ((float)polygon.v1.v) / 255.0f;
                polygon.v2.fU = ((float)polygon.v2.u) / 255.0f;
                polygon.v2.fV = ((float)polygon.v2.v) / 255.0f;
                polygon.v3.fU = ((float)polygon.v3.u) / 255.0f;
                polygon.v3.fV = ((float)polygon.v3.v) / 255.0f;

                float fSizeAdjust = 1.0f / 255.0f;      // 2.0f seems to work better for dreamcast
                float fOffsetAdjust = 0.5f / 255.0f;
                float fCU = (polygon.v1.fU + polygon.v2.fU + polygon.v3.fU) / 3.0f;
                float fCV = (polygon.v1.fV + polygon.v2.fV + polygon.v3.fV) / 3.0f;
                AdjustUVs(ref polygon.v1, fCU, fCV, fSizeAdjust, fOffsetAdjust);
                AdjustUVs(ref polygon.v2, fCU, fCV, fSizeAdjust, fOffsetAdjust);
                AdjustUVs(ref polygon.v3, fCU, fCV, fSizeAdjust, fOffsetAdjust);
            }
            else
            {
                UInt16 int1 = reader.ReadUInt16();
                UInt16 int2 = reader.ReadUInt16();
                UInt16 int3 = reader.ReadUInt16();
                UInt16 int4 = reader.ReadUInt16();
                UInt16 int5 = reader.ReadUInt16();
                UInt16 int6 = reader.ReadUInt16();
                //polygon.v1.u = int2;
                //polygon.v1.v = int1;
                //polygon.v2.u = int4;
                //polygon.v2.v = int3;
                //polygon.v3.u = int6;
                //polygon.v3.v = int5;
                polygon.v1.fU = BizarreFloatToNormalFloat(int2);
                polygon.v1.fV = BizarreFloatToNormalFloat(int1);
                polygon.v2.fU = BizarreFloatToNormalFloat(int4);
                polygon.v2.fV = BizarreFloatToNormalFloat(int3);
                polygon.v3.fU = BizarreFloatToNormalFloat(int6);
                polygon.v3.fV = BizarreFloatToNormalFloat(int5);
                polygon.material.textureID = (UInt16)((reader.ReadUInt16() & 0x07FF) - 1);
            }

            return;
        }

        void ReadBSPTree(BinaryReader reader)
        {
            Boolean drawTester;
            UInt16 bspID = 0;
            ExBSPTree currentTree;
            ExBSPTree[] bspTrees = new ExBSPTree[m_uBspTreeCount];
            ExBSPTreeStack stack = new ExBSPTreeStack();
            for (UInt16 b = 0; b < m_uBspTreeCount; b++)
            {
                reader.BaseStream.Position = m_uBspTreeStart + (b * 0x24);
                bspTrees[b] = new ExBSPTree();
                bspTrees[b].dataPos = m_uDataStart + reader.ReadUInt32();

                reader.BaseStream.Position += 0x0E;
                drawTester = ((reader.ReadInt16() & 1) != 1);

                reader.BaseStream.Position += 0x06;
                bspID = reader.ReadUInt16();
                stack.Push(bspTrees[b]);
                currentTree = stack.Top;

                while (currentTree != null)
                {
                    reader.BaseStream.Position = currentTree.dataPos + 0x0E;
                    currentTree.isLeaf = ((reader.ReadByte() & 0x02) == 0x02);
                    if (currentTree.isLeaf)
                    {
                        // Handle Leaf here
                        reader.BaseStream.Position = currentTree.dataPos + 0x08;
                        UInt32 polygonPos = m_uDataStart + reader.ReadUInt32();
                        UInt32 polygonID = (polygonPos - m_uPolygonStart) / 0x0C;
                        UInt16 polyCount = reader.ReadUInt16();
                        for (UInt16 p = 0; p < polyCount; p++)
                        {
                            // 0 = dome, 2 = firelamps, 3 = barriers,
                            // 4 = centre floor, 5 = outer floor,
                            // 6 = collision around coffins,
                            // 7 = corridor, 8 = coffins and small dome,
                            // 9 = stairs, 
                            /*if (bspID == 0 || bspID == 2 || bspID == 5 ||
                                bspID == 7 || bspID == 8 || bspID == 9 ||
                                bspID == 4)*/
                            if (drawTester)
                            {
                                m_axPolygons[polygonID + p].isVisible = true;
                            }
                        }

                        // Finished with right child, now handle left.
                        currentTree = stack.Pop();
                        continue;
                    }
                    reader.BaseStream.Position = currentTree.dataPos + 0x14;
                    UInt32 leftPos = reader.ReadUInt32();
                    if (leftPos != 0)
                    {
                        currentTree.leftChild = new ExBSPTree();
                        currentTree.leftChild.dataPos = m_uDataStart + leftPos;
                        stack.Push(currentTree.leftChild);
                    }
                    UInt32 rightPos = reader.ReadUInt32();
                    if (rightPos != 0)
                    {
                        currentTree.rightChild = new ExBSPTree();
                        currentTree.rightChild.dataPos = m_uDataStart + rightPos;
                        currentTree = currentTree.rightChild;
                    }
                }
            }
            return;
        }

        static void FlipRedAndBlue(ref UInt32 colour)
        {
            UInt32 tempColour = colour;
            colour =
                (tempColour & 0xFF000000) |
                ((tempColour << 16) & 0x00FF0000) |
                (tempColour & 0x0000FF00) |
                ((tempColour >> 16) & 0x000000FF);
            return;
        }

        static float BizarreFloatToNormalFloat(UInt16 usBizarreFloat)
        {
            // Converts the 16-bit floating point values used in the DC version of Soul Reaver to normal 32-bit floats
            ushort usExponent;
            int iUnbiasedExponent;
            ushort usSignificand;
            //bool bPositive = true; // AMF I took this out
            //ushort usSignCheck = usBizarreFloat;
            //usSignCheck = usSignCheck >> 15;

            usExponent = usBizarreFloat;
            usExponent = (ushort)(usExponent << 1);
            usExponent = (ushort)(usExponent >> 8);
            iUnbiasedExponent = usExponent - 127;
            usSignificand = usBizarreFloat;
            usSignificand = (ushort)(usSignificand << 9);
            usSignificand = (ushort)(usSignificand >> 9);
            float fFraction = 1f;
            for (int i = 0; i < 7; i++)
            {
                byte cCurrent = (byte)usSignificand;
                cCurrent = (byte)(cCurrent << (i + 1));
                cCurrent = (byte)(cCurrent >> 7);
                fFraction += (float)((float)cCurrent * Math.Pow(2, 0 - (1 + i)));
            }
            float fCalcValue = (float)(fFraction * Math.Pow(2, (double)iUnbiasedExponent));
            //if (!bPositive)
            //{
            //    calcValue *= -1f;
            //}
            return fCalcValue;
        }

        static float ClampToRange(float fValue, float fMin, float fMax)
        {
            if (fValue < fMin)
            {
                return fMin;
            }
            if (fValue > fMax)
            {
                return fMax;
            }
            return fValue;
        }

        static void AdjustUVs(ref ExVertex xVertex, float fCentreU, float fCentreV, float fSizeAdjust, float fOffsetAdjust)
        {
            if (fCentreU < xVertex.fU)
            {
                xVertex.fU = Math.Max(fCentreU, xVertex.fU - fSizeAdjust);
            }
            if (fCentreU > xVertex.fU)
            {
                xVertex.fU = Math.Min(fCentreU, xVertex.fU + fSizeAdjust);
            }
            xVertex.fU += fOffsetAdjust;
            xVertex.fU = ClampToRange(xVertex.fU, 0.0f, 255.0f);
            if (fCentreV < xVertex.fV)
            {
                xVertex.fV = Math.Max(fCentreV, xVertex.fV - fSizeAdjust);
            }
            if (fCentreV > xVertex.fV)
            {
                xVertex.fV = Math.Min(fCentreV, xVertex.fV + fSizeAdjust);
            }
            xVertex.fV += fOffsetAdjust;
            xVertex.fV = ClampToRange(xVertex.fV, 0.0f, 255.0f);
        }
    }

    public abstract class SR1Model
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
        protected String            m_strModelName;
        protected Platform          m_ePlatform;
        protected UInt32            m_uDataStart;
        protected UInt32            m_uModelData;
        protected UInt32            m_uVertexCount;
        protected UInt32            m_uVertexStart;
        protected UInt32            m_uPolygonCount;
        protected UInt32            m_uPolygonStart;
        protected UInt16            m_uMaterialCount;
        protected UInt32            m_uMaterialStart;
        protected UInt16[]          m_ausIndices;
        protected UInt32            m_uIndexCount { get { return 3 * m_uPolygonCount; } }
        protected ExVertex[]        m_axVertices;
        protected ExPolygon[]       m_axPolygons;
        protected ExMaterial[]      m_axMaterials;
        protected ExMaterialList    m_xMaterialsList;

        protected SR1Model(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform)
        {
            m_strModelName  = strModelName;
            m_ePlatform     = ePlatform;
            m_uDataStart    = uDataStart;
            m_uModelData    = uModelData;
            m_uVertexCount  = 0;
            m_uVertexStart  = 0;
            m_uPolygonCount = 0;
            m_uPolygonStart = 0;
        }

        protected virtual void ReadData(BinaryReader xReader)
        {
            // Get the vertices
            m_axVertices = new ExVertex[m_uVertexCount];
            ReadVertices(xReader);

            // Get the polygons
            m_axPolygons = new ExPolygon[m_uPolygonCount];
            ReadPolygons(xReader);
        }

        protected virtual void ReadVertex(BinaryReader xReader, ref ExVertex xVertex)
        {
            // Read the local coordinates
            xVertex.localPos.x = xReader.ReadInt16();
            xVertex.localPos.y = xReader.ReadInt16();
            xVertex.localPos.z = xReader.ReadInt16();

            // Before transformation, the world coords equal the local coords
            xVertex.worldPos = xVertex.localPos;
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
                m_axVertices[v].index = v;
                ReadVertex(xReader, ref m_axVertices[v]);
            }

            return;
        }

        protected virtual void ReadPolygon(BinaryReader xReader, ref ExPolygon xPolygon)
        {
            xPolygon.v1 = m_axVertices[xReader.ReadUInt16()];
            xPolygon.v2 = m_axVertices[xReader.ReadUInt16()];
            xPolygon.v3 = m_axVertices[xReader.ReadUInt16()];
            xPolygon.material = new ExMaterial();
        }

        protected virtual void ReadPolygons(BinaryReader xReader)
        {
            for (UInt16 p = 0; p < m_uPolygonCount; p++)
            {
                xReader.BaseStream.Position = m_uPolygonStart + (p * 12);

                ReadPolygon(xReader, ref m_axPolygons[p]);

                if (m_xMaterialsList == null)
                {
                    m_xMaterialsList = new ExMaterialList(m_axPolygons[p].material);
                    m_uMaterialCount++;
                }
                else
                {
                    ExMaterial newMaterial = m_xMaterialsList.AddToList(m_axPolygons[p].material);
                    if (m_axPolygons[p].material != newMaterial)
                    {
                        m_axPolygons[p].material = newMaterial;
                    }
                    else
                    {
                        m_uMaterialCount++;
                    }
                }
            }
        }

        protected virtual void ReadTextureData(BinaryReader reader, ref ExPolygon polygon)
        {
            if (m_ePlatform != Platform.Dreamcast)
            {
                polygon.v1.u = reader.ReadByte();
                polygon.v1.v = reader.ReadByte();
                if (m_ePlatform == Platform.PSX)
                {
                    ushort paletteVal = reader.ReadUInt16();
                    ushort rowVal = (ushort)((ushort)(paletteVal << 2) >> 8);
                    ushort colVal = (ushort)((ushort)(paletteVal << 11) >> 11);
                    polygon.paletteColumn = colVal;
                    polygon.paletteRow = rowVal;
                }
                else
                {
                    UInt16 usTemp = reader.ReadUInt16();
                    reader.BaseStream.Position -= 2;
                    polygon.material.textureID = (UInt16)(reader.ReadUInt16() & 0x07FF);
                }
                polygon.v2.u = reader.ReadByte();
                polygon.v2.v = reader.ReadByte();
                if (m_ePlatform == Platform.PSX)
                {
                    polygon.material.textureID = (UInt16)(((reader.ReadUInt16() & 0x07FF) - 8) % 8);
                }
                else
                {
                    UInt16 usTemp = reader.ReadUInt16();
                }
                polygon.v3.u = reader.ReadByte();
                polygon.v3.v = reader.ReadByte();

                polygon.v1.fU = ((float)polygon.v1.u) / 255.0f;
                polygon.v1.fV = ((float)polygon.v1.v) / 255.0f;
                polygon.v2.fU = ((float)polygon.v2.u) / 255.0f;
                polygon.v2.fV = ((float)polygon.v2.v) / 255.0f;
                polygon.v3.fU = ((float)polygon.v3.u) / 255.0f;
                polygon.v3.fV = ((float)polygon.v3.v) / 255.0f;

                float fSizeAdjust = 1.0f / 255.0f;      // 2.0f seems to work better for dreamcast
                float fOffsetAdjust = 0.5f / 255.0f;
                float fCU = (polygon.v1.fU + polygon.v2.fU + polygon.v3.fU) / 3.0f;
                float fCV = (polygon.v1.fV + polygon.v2.fV + polygon.v3.fV) / 3.0f;
                AdjustUVs(ref polygon.v1, fCU, fCV, fSizeAdjust, fOffsetAdjust);
                AdjustUVs(ref polygon.v2, fCU, fCV, fSizeAdjust, fOffsetAdjust);
                AdjustUVs(ref polygon.v3, fCU, fCV, fSizeAdjust, fOffsetAdjust);
            }
            else
            {
                UInt16 int1 = reader.ReadUInt16();
                UInt16 int2 = reader.ReadUInt16();
                UInt16 int3 = reader.ReadUInt16();
                UInt16 int4 = reader.ReadUInt16();
                UInt16 int5 = reader.ReadUInt16();
                UInt16 int6 = reader.ReadUInt16();
                //polygon.v1.u = int2;
                //polygon.v1.v = int1;
                //polygon.v2.u = int4;
                //polygon.v2.v = int3;
                //polygon.v3.u = int6;
                //polygon.v3.v = int5;
                polygon.v1.fU = BizarreFloatToNormalFloat(int2);
                polygon.v1.fV = BizarreFloatToNormalFloat(int1);
                polygon.v2.fU = BizarreFloatToNormalFloat(int4);
                polygon.v2.fV = BizarreFloatToNormalFloat(int3);
                polygon.v3.fU = BizarreFloatToNormalFloat(int6);
                polygon.v3.fV = BizarreFloatToNormalFloat(int5);
                polygon.material.textureID = (UInt16)((reader.ReadUInt16() & 0x07FF) - 1);
            }

            return;
        }

        protected virtual void GenerateOutputData()
        {
            // Make the vertices unique and generate new index array
            m_axVertices = new ExVertex[m_uIndexCount];
            m_ausIndices = new UInt16[m_uIndexCount];
            for (UInt16 p = 0; p < m_uPolygonCount; p++)
            {
                m_axVertices[(3 * p) + 0] = m_axPolygons[p].v1;
                m_axVertices[(3 * p) + 1] = m_axPolygons[p].v2;
                m_axVertices[(3 * p) + 2] = m_axPolygons[p].v3;
                m_ausIndices[(3 * p) + 0] = (UInt16)((3 * p) + 0);
                m_ausIndices[(3 * p) + 1] = (UInt16)((3 * p) + 1);
                m_ausIndices[(3 * p) + 2] = (UInt16)((3 * p) + 2);
            }

            // Build the materials array
            m_axMaterials = new ExMaterial[m_uMaterialCount];
            UInt16 mNew = 0;

            // Get the untextured materials
            ExMaterialList matList = m_xMaterialsList;
            while (matList != null)
            {
                if (!matList.material.textureUsed)
                {
                    m_axMaterials[mNew] = matList.material;
                    m_axMaterials[mNew].ID = mNew;
                    m_axMaterials[mNew].textureName = "";
                    mNew++;
                }
                matList = matList.next;
            }

            // Get the textured materials
            matList = m_xMaterialsList;
            while (matList != null)
            {
                if (matList.material.textureUsed)
                {
                    m_axMaterials[mNew] = matList.material;
                    m_axMaterials[mNew].ID = mNew;
                    if (m_ePlatform == Platform.PSX)
                    {
                        m_axMaterials[mNew].textureName =
                            m_strModelName.TrimEnd(new char[] { '_' }).ToLower() + "-" +
                            m_axMaterials[mNew].textureID.ToString("0000") +
                            ".png";
                    }
                    else
                    {
                        m_axMaterials[mNew].textureName =
                            "Texture-" +
                            m_axMaterials[mNew].textureID.ToString("00000") +
                            ".png";
                    }
                    mNew++;
                }
                matList = matList.next;
            }
            return;
        }

        #region Utility

        protected static void FlipRedAndBlue(ref UInt32 colour)
        {
            UInt32 tempColour = colour;
            colour =
                (tempColour & 0xFF000000) |
                ((tempColour << 16) & 0x00FF0000) |
                (tempColour & 0x0000FF00) |
                ((tempColour >> 16) & 0x000000FF);
            return;
        }

        protected static float BizarreFloatToNormalFloat(UInt16 usBizarreFloat)
        {
            // Converts the 16-bit floating point values used in the DC version of Soul Reaver to normal 32-bit floats
            ushort usExponent;
            int iUnbiasedExponent;
            ushort usSignificand;
            //bool bPositive = true; // AMF I took this out
            //ushort usSignCheck = usBizarreFloat;
            //usSignCheck = usSignCheck >> 15;

            usExponent = usBizarreFloat;
            usExponent = (ushort)(usExponent << 1);
            usExponent = (ushort)(usExponent >> 8);
            iUnbiasedExponent = usExponent - 127;
            usSignificand = usBizarreFloat;
            usSignificand = (ushort)(usSignificand << 9);
            usSignificand = (ushort)(usSignificand >> 9);
            float fFraction = 1f;
            for (int i = 0; i < 7; i++)
            {
                byte cCurrent = (byte)usSignificand;
                cCurrent = (byte)(cCurrent << (i + 1));
                cCurrent = (byte)(cCurrent >> 7);
                fFraction += (float)((float)cCurrent * Math.Pow(2, 0 - (1 + i)));
            }
            float fCalcValue = (float)(fFraction * Math.Pow(2, (double)iUnbiasedExponent));
            //if (!bPositive)
            //{
            //    calcValue *= -1f;
            //}
            return fCalcValue;
        }

        protected static float ClampToRange(float fValue, float fMin, float fMax)
        {
            if (fValue < fMin)
            {
                return fMin;
            }
            if (fValue > fMax)
            {
                return fMax;
            }
            return fValue;
        }

        protected static void AdjustUVs(ref ExVertex xVertex, float fCentreU, float fCentreV, float fSizeAdjust, float fOffsetAdjust)
        {
            if (fCentreU < xVertex.fU)
            {
                xVertex.fU = Math.Max(fCentreU, xVertex.fU - fSizeAdjust);
            }
            if (fCentreU > xVertex.fU)
            {
                xVertex.fU = Math.Min(fCentreU, xVertex.fU + fSizeAdjust);
            }
            xVertex.fU = ClampToRange(xVertex.fU + fOffsetAdjust, 0.0f, 255.0f);

            if (fCentreV < xVertex.fV)
            {
                xVertex.fV = Math.Max(fCentreV, xVertex.fV - fSizeAdjust);
            }
            if (fCentreV > xVertex.fV)
            {
                xVertex.fV = Math.Min(fCentreV, xVertex.fV + fSizeAdjust);
            }
            xVertex.fV = ClampToRange(xVertex.fV + fOffsetAdjust, 0.0f, 255.0f);
        }

        #endregion
    }

    public class SR1File
    {
        #region Model classes

        protected class SR1ObjectModel : SR1Model
        {
            protected UInt32    m_uBoneCount;
            protected UInt32    m_uBoneStart;
            protected ExBone[]  m_axBones;

            protected SR1ObjectModel(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform)
                : base (xReader, uDataStart, uModelData, strModelName, ePlatform)
            {
                xReader.BaseStream.Position = m_uModelData;
                m_uVertexCount              = xReader.ReadUInt32();
                m_uVertexStart              = m_uDataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position += 0x08;
                m_uPolygonCount             = xReader.ReadUInt32();
                m_uPolygonStart             = m_uDataStart + xReader.ReadUInt32();
                m_uBoneCount                = xReader.ReadUInt32();
                m_uBoneStart                = m_uDataStart + xReader.ReadUInt32();
                m_uMaterialCount            = 0;
            }

            public static SR1ObjectModel Load(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform)
            {
                SR1ObjectModel xModel = new SR1ObjectModel(xReader, uDataStart, uModelData, strModelName, ePlatform);
                return xModel;
            }

            protected override void ReadVertex(BinaryReader xReader, ref ExVertex xVertex)
            {
                base.ReadVertex(xReader, ref xVertex);

                xVertex.normalID = xReader.ReadUInt16();
                xVertex.normal.x = s_aiNormals[xVertex.normalID, 0];
                xVertex.normal.y = s_aiNormals[xVertex.normalID, 1];
                xVertex.normal.z = s_aiNormals[xVertex.normalID, 2];
            }

            protected override void ReadVertices(BinaryReader xReader)
            {
                base.ReadVertices(xReader);
                ReadArmature(xReader);
                ApplyArmature();
            }

            protected virtual void ReadArmature(BinaryReader reader)
            {
                if (m_uBoneStart == 0 || m_uBoneCount == 0) return;

                reader.BaseStream.Position = m_uBoneStart;
                m_axBones = new ExBone[m_uBoneCount];
                m_axBones = new ExBone[m_uBoneCount];
                for (UInt16 b = 0; b < m_uBoneCount; b++)
                {
                    // Get the bone data
                    reader.BaseStream.Position += 8;
                    m_axBones[b].vFirst = reader.ReadUInt16();
                    m_axBones[b].vLast = reader.ReadUInt16();
                    m_axBones[b].localPos.x = reader.ReadInt16();
                    m_axBones[b].localPos.y = reader.ReadInt16();
                    m_axBones[b].localPos.z = reader.ReadInt16();
                    m_axBones[b].parentID = reader.ReadUInt16();

                    // Combine this bone with it's ancestors is there are any
                    if ((m_axBones[b].vFirst != 0xFFFF) && (m_axBones[b].vLast != 0xFFFF))
                    {
                        for (UInt16 ancestorID = b; ancestorID != 0xFFFF; )
                        {
                            m_axBones[b].worldPos += m_axBones[ancestorID].localPos;
                            if (m_axBones[ancestorID].parentID == ancestorID) break;
                            ancestorID = m_axBones[ancestorID].parentID;
                        }
                    }
                    reader.BaseStream.Position += 4;
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
                            m_axVertices[v].worldPos += m_axBones[b].worldPos;
                            m_axVertices[v].boneID = b;
                        }
                    }
                }
                return;
            }

            protected override void ReadPolygon(BinaryReader xReader, ref ExPolygon xPolygon)
            {
                base.ReadPolygon(xReader, ref xPolygon);

                xPolygon.material.textureUsed = (Boolean)(((int)xReader.ReadUInt16() & 0x0200) != 0);

                if (xPolygon.material.textureUsed)
                {
                    xReader.BaseStream.Position = m_uDataStart + xReader.ReadInt32();
                    ReadTextureData(xReader, ref xPolygon);
                    xReader.BaseStream.Position += 2;
                }
                else
                {
                    xReader.BaseStream.Position += 6;
                }

                xPolygon.material.colour = xReader.ReadUInt32() | 0xFF000000;
                FlipRedAndBlue(ref xPolygon.material.colour);
            }
        }

        protected class SR1AreaModel : SR1Model
        {
            protected UInt32    m_uBspTreeCount;
            protected UInt32    m_uBspTreeStart;
            protected Realm     m_eRealm;
            protected UInt32    m_uSpectralVertexStart;
            protected UInt32    m_uSpectralColourStart;

            protected SR1AreaModel(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, Realm eRealm)
                : base (xReader, uDataStart, uModelData, strModelName, ePlatform)
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
                m_uSpectralVertexStart      = m_uDataStart + xReader.ReadUInt32();
                m_uSpectralColourStart      = m_uDataStart + xReader.ReadUInt32();
                m_uBspTreeCount             = xReader.ReadUInt32();
                m_uBspTreeStart             = m_uDataStart + xReader.ReadUInt32();
            }

            public static SR1AreaModel Load(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, Realm eRealm)
            {
                SR1AreaModel xModel = new SR1AreaModel(xReader, uDataStart, uModelData, strModelName, ePlatform, eRealm);
                return xModel;
            }

            protected override void ReadVertex(BinaryReader xReader, ref ExVertex xVertex)
            {
                base.ReadVertex(xReader, ref xVertex);

                xReader.BaseStream.Position += 2;
                xVertex.colour = xReader.ReadUInt32() | 0xFF000000;

                if (m_ePlatform != Platform.Dreamcast)
                {
                    FlipRedAndBlue(ref xVertex.colour);
                }

            }

            protected override void ReadVertices(BinaryReader xReader)
            {
                base.ReadVertices(xReader);
                ReadSpectralData(xReader);
            }

            protected virtual void ReadSpectralData(BinaryReader reader)
            {
                if (m_eRealm == Realm.Spectral)
                {
                    // Spectral Colours
                    reader.BaseStream.Position = m_uSpectralColourStart;
                    for (int v = 0; v < m_uVertexCount; v++)
                    {
                        UInt32 uShiftColour = reader.ReadUInt16();
                        UInt32 uAlpha = m_axVertices[v].colour & 0xFF000000;
                        UInt32 uRed = ((uShiftColour >> 0) & 0x1F) << 0x13;
                        UInt32 uGreen = ((uShiftColour >> 5) & 0x1F) << 0x0B;
                        UInt32 uBlue = ((uShiftColour >> 10) & 0x1F) << 0x03;
                        m_axVertices[v].colour = uAlpha | uRed | uGreen | uBlue;
                    }

                    // Spectral Verticices
                    reader.BaseStream.Position = m_uSpectralVertexStart + 0x06;
                    int sVertex = reader.ReadInt16();
                    reader.BaseStream.Position = m_uSpectralVertexStart;
                    while (sVertex != 0xFFFF)
                    {
                        ExShiftVertex xShiftVertex;
                        xShiftVertex.basePos.x = reader.ReadInt16();
                        xShiftVertex.basePos.y = reader.ReadInt16();
                        xShiftVertex.basePos.z = reader.ReadInt16();
                        sVertex = reader.ReadUInt16();

                        if (sVertex == 0xFFFF)
                        {
                            break;
                        }

                        xShiftVertex.offset.x = reader.ReadInt16();
                        xShiftVertex.offset.y = reader.ReadInt16();
                        xShiftVertex.offset.z = reader.ReadInt16();
                        m_axVertices[sVertex].localPos = xShiftVertex.offset + xShiftVertex.basePos;
                        m_axVertices[sVertex].worldPos = m_axVertices[sVertex].localPos;
                    }
                }
            }

            protected override void ReadPolygon(BinaryReader xReader, ref ExPolygon xPolygon)
            {
                base.ReadPolygon(xReader, ref xPolygon);

                xPolygon.material.textureUsed = (Boolean)(((int)xReader.ReadUInt16() & 0x0004) == 0);

                xReader.BaseStream.Position += 2;
                UInt16 materialOffset = xReader.ReadUInt16();
                if (materialOffset != 0xFFFF &&
                    xPolygon.material.textureUsed &&
                    xPolygon.isVisible)
                {
                    xReader.BaseStream.Position = m_uMaterialStart + materialOffset;
                    ReadTextureData(xReader, ref xPolygon);
                    xPolygon.material.colour = 0xFFFFFFFF;
                }
                else
                {
                    xPolygon.material.textureUsed = false;
                    xPolygon.material.colour = 0x00000000;
                    xPolygon.v1.colour = 0x00000000;
                    xPolygon.v2.colour = 0x00000000;
                    xPolygon.v3.colour = 0x00000000;
                }
                FlipRedAndBlue(ref xPolygon.material.colour);
            }

            protected override void ReadPolygons(BinaryReader xReader)
            {
                ReadBSPTree(xReader);
                base.ReadPolygons(xReader);
            }

            protected virtual void ReadBSPTree(BinaryReader reader)
            {
                Boolean drawTester;
                UInt16 bspID = 0;
                ExBSPTree currentTree;
                ExBSPTree[] bspTrees = new ExBSPTree[m_uBspTreeCount];
                ExBSPTreeStack stack = new ExBSPTreeStack();
                for (UInt16 b = 0; b < m_uBspTreeCount; b++)
                {
                    reader.BaseStream.Position = m_uBspTreeStart + (b * 0x24);
                    bspTrees[b] = new ExBSPTree();
                    bspTrees[b].dataPos = m_uDataStart + reader.ReadUInt32();

                    reader.BaseStream.Position += 0x0E;
                    drawTester = ((reader.ReadInt16() & 1) != 1);

                    reader.BaseStream.Position += 0x06;
                    bspID = reader.ReadUInt16();
                    stack.Push(bspTrees[b]);
                    currentTree = stack.Top;

                    while (currentTree != null)
                    {
                        reader.BaseStream.Position = currentTree.dataPos + 0x0E;
                        currentTree.isLeaf = ((reader.ReadByte() & 0x02) == 0x02);
                        if (currentTree.isLeaf)
                        {
                            // Handle Leaf here
                            reader.BaseStream.Position = currentTree.dataPos + 0x08;
                            UInt32 polygonPos = m_uDataStart + reader.ReadUInt32();
                            UInt32 polygonID = (polygonPos - m_uPolygonStart) / 0x0C;
                            UInt16 polyCount = reader.ReadUInt16();
                            for (UInt16 p = 0; p < polyCount; p++)
                            {
                                // 0 = dome, 2 = firelamps, 3 = barriers,
                                // 4 = centre floor, 5 = outer floor,
                                // 6 = collision around coffins,
                                // 7 = corridor, 8 = coffins and small dome,
                                // 9 = stairs, 
                                /*if (bspID == 0 || bspID == 2 || bspID == 5 ||
                                    bspID == 7 || bspID == 8 || bspID == 9 ||
                                    bspID == 4)*/
                                if (drawTester)
                                {
                                    m_axPolygons[polygonID + p].isVisible = true;
                                }
                            }

                            // Finished with right child, now handle left.
                            currentTree = stack.Pop();
                            continue;
                        }
                        reader.BaseStream.Position = currentTree.dataPos + 0x14;
                        UInt32 leftPos = reader.ReadUInt32();
                        if (leftPos != 0)
                        {
                            currentTree.leftChild = new ExBSPTree();
                            currentTree.leftChild.dataPos = m_uDataStart + leftPos;
                            stack.Push(currentTree.leftChild);
                        }
                        UInt32 rightPos = reader.ReadUInt32();
                        if (rightPos != 0)
                        {
                            currentTree.rightChild = new ExBSPTree();
                            currentTree.rightChild.dataPos = m_uDataStart + rightPos;
                            currentTree = currentTree.rightChild;
                        }
                    }
                }
                return;
            }
        }

        #endregion

        public String       m_strModelName;
        public UInt32       m_uDataStart;
        public UInt16       m_usModelCount;
        public UInt16       m_usAnimCount;
        public UInt32       m_uModelStart;
        public ExModel[]    m_axModels;
        public UInt32       m_uAnimStart;
        public UInt32       m_uInstanceCount;
        public UInt32       m_uInstanceStart;
        public String[]     m_astrInstanceNames;
        public UInt32       m_uInstanceTypesStart;
        public String[]     m_axInstanceTypeNames;
        public FileType     m_eFileType;
        public Platform     m_ePlatform;

        public SR1File(string fileName)
        {
            FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(file);

            // Get start of usefull data
            m_uDataStart = ((reader.ReadUInt32() >> 9) << 11) + 0x00000800;

            #region Model Type
            if (reader.ReadUInt32() == 0x00000000)
            {
                m_eFileType = FileType.Area;
            }
            else
            {
                m_eFileType = FileType.Object;
            }
            #endregion

            #region Load Data
            if (m_eFileType == FileType.Object)
            {
                ReadObjectFile(reader);
            }
            else
            {
                ReadAreaFile(reader);
            }
            #endregion

            // Close the file
            reader.Close();
            file.Close();
            reader = null;
            file = null;
        }

        private void ReadObjectFile(BinaryReader xReader)
        {
            // Object name
            xReader.BaseStream.Position = m_uDataStart + 0x00000024;
            xReader.BaseStream.Position = m_uDataStart + xReader.ReadUInt32();
            m_strModelName = new String(xReader.ReadChars(8));

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
            // xReader.BaseStream.Position += 0x00000002;
            m_uModelStart = m_uDataStart + xReader.ReadUInt32();
            m_uAnimStart = m_uDataStart + xReader.ReadUInt32();

            m_axModels = new ExModel[m_usModelCount];
            for (int m = 0; m < m_usModelCount; m++)
            {
                xReader.BaseStream.Position = m_uModelStart + (0x00000004 * m);
                UInt32 UModelData = m_uDataStart + xReader.ReadUInt32();
                m_axModels[m] = new ExModel(m_strModelName, m_uDataStart, UModelData, m_ePlatform, m_eFileType, Realm.Material, xReader);
            }
        }

        private void ReadAreaFile(BinaryReader xReader)
        {
            // Instance names
            xReader.BaseStream.Position = m_uDataStart + 0x78;
            m_uInstanceCount = xReader.ReadUInt32();
            m_uInstanceStart = m_uDataStart + xReader.ReadUInt32();
            m_astrInstanceNames = new String[m_uInstanceCount];
            for (int i = 0; i < m_uInstanceCount; i++)
            {
                xReader.BaseStream.Position = m_uInstanceStart + 0x4C * i;
                m_astrInstanceNames[i] = new String(xReader.ReadChars(8));
            }

            // Instance list
            xReader.BaseStream.Position = m_uDataStart + 0x8C;
            m_uInstanceTypesStart = m_uDataStart + xReader.ReadUInt32();
            xReader.BaseStream.Position = m_uInstanceTypesStart;
            List<String> xInstanceList = new List<String>();
            while (xReader.ReadByte() != 0xFF)
            {
                xReader.BaseStream.Position--;
                xInstanceList.Add(new String(xReader.ReadChars(8)));
                xReader.BaseStream.Position += 0x08;
            }
            m_axInstanceTypeNames = xInstanceList.ToArray();

            // Area name
            xReader.BaseStream.Position = m_uDataStart + 0x98;
            xReader.BaseStream.Position = m_uDataStart + xReader.ReadUInt32();
            String strModelName = new String(xReader.ReadChars(8));
            try
            {
                m_strModelName = strModelName.Substring(0, strModelName.IndexOf('\0'));
            }
            catch
            {
                m_strModelName = strModelName;
            }

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

            // Version number
            xReader.BaseStream.Position = m_uDataStart + 0xF0;
            UInt32 areaVersionNumber = xReader.ReadUInt32();
            if (areaVersionNumber != 0x3C20413B)
            {
                // Beta gets here...
                // throw new Exception("Wrong version number for level x");
            }

            // Model data
            xReader.BaseStream.Position = m_uDataStart;
            m_usModelCount = 2;
            m_uModelStart = m_uDataStart;
            m_axModels = new ExModel[m_usModelCount];
            xReader.BaseStream.Position = m_uModelStart;
            UInt32 m_uModelData = m_uDataStart + xReader.ReadUInt32();

            // Material data
            m_axModels[0] = new ExModel(m_strModelName, m_uDataStart, m_uModelData, m_ePlatform, m_eFileType, Realm.Material, xReader);

            // Spectral data
            m_axModels[1] = new ExModel(m_strModelName, m_uDataStart, m_uModelData, m_ePlatform, m_eFileType, Realm.Spectral, xReader);
        }
    }
}