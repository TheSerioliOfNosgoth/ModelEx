using System;
using System.IO;
using System.Collections.Generic;

namespace ModelEx
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
        Unit
    }
    public enum Realm
    {
        Material,
        Spectral
    }

    public struct ExVector
    {
        public float x, y, z;
        public ExVector(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public static ExVector operator +(ExVector v1, ExVector v2)
        {
            return new ExVector(
                v1.x + v2.x,
                v1.y + v2.y,
                v1.z + v2.z
            );
        }
        public static ExVector operator *(ExVector v, float f)
        {
            return new ExVector(
                v.x *= f,
                v.y *= f,
                v.z *= f
            );
        }
    }
    public struct ExBone
    {
        public UInt16 vFirst, vLast;    // The ID of first and last effected vertex 
        public ExVector localPos;       // Local bone coordinates
        public ExVector worldPos;       // World bone coordinated
        public UInt16 parentID1;        // ID of parent bone 1
        public UInt16 parentID2;        // ID of parent bone 2
        public UInt32 flags;            // Flags including which parent to use.
    }
    public struct ExPosition
    {
        public ExVector localPos;       // Local vertex coordinates
        public ExVector worldPos;       // World vertex coordinates
        public UInt16 boneID;           // Index of the bone effecting this vertex
    }
    public struct ExNormal
    {
        public Int32 x, y, z;
    }
    public struct ExUV
    {
        public float u, v;
    }
    public struct ExVertex
    {
        public int positionID;       // Index of the vertex position
        public int normalID;         // Index of the vertex normal
        public int colourID;         // Index of the vertex colour
        public int UVID;             // Index of the vertex UV
    }
    public struct ExShiftVertex
    {
        public UInt16 index;            // Index in the file
        public ExVector basePos;        // Base vertex coordinates
        public ExVector offset;         // Offset from base coordinates
    }
    public struct ExPolygon
    {
        public ExMaterial material;     // The material used
        public ExVertex v1, v2, v3;     // Vertices for the polygon
        public int paletteRow;          // The row of the pallete to use (PS1)
        public int paletteColumn;       // The column of the pallet to use (PS1)
    }
    public class ExMaterial
    {
        public UInt16 ID;               // The ID of the material
        public Boolean visible;         // Flag specifying if this material is visible
        public Boolean textureUsed;     // Flag specifying if this material has a texture
        public UInt16 textureID;        // ID of the texture file
        public UInt32 colour;           // Diffuse colour
    }
    public class ExMaterialList
    {
        protected ExMaterialList _next;
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
        public virtual ExMaterial AddToList(ExMaterial material)
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
    public class ExTreeStack
    {
        private class Node
        {
            public ExTree tree;
            public Node lastNode;
        }
        private Node firstNode;
        private Node currentNode;
        private UInt32 nodeCount;
        public void Push(ExTree tree)
        {
            Node lastNode = currentNode;
            currentNode = new Node();
            currentNode.tree = tree;
            currentNode.lastNode = lastNode;
            if (nodeCount == 0)
            {
                firstNode = currentNode;
            }
            nodeCount++;
            return;
        }
        public ExTree Pop()
        {
            if (currentNode == null) return null;
            ExTree tree = currentNode.tree;
            currentNode = currentNode.lastNode;
            nodeCount--;
            if (nodeCount == 0)
            {
                firstNode = null;
            }
            return tree;
        }
        public ExTree Top
        {
            get
            {
                if (currentNode == null) return null;
                return currentNode.tree;
            }
        }
        public ExTree Start
        {
            get
            {
                if (firstNode == null) return null;
                return firstNode.tree;
            }
        }
        public UInt32 Count
        {
            get { return nodeCount; }
        }
        public ExTree GetNode(UInt32 uIndex)
        {
            Node xCurrentNode = currentNode;
            for (UInt32 i = 0; i <= uIndex; i++)
            {
                if (xCurrentNode == null)
                {
                    return null;
                }
                if (i == uIndex)
                {
                    return xCurrentNode.tree;
                }
                xCurrentNode = xCurrentNode.lastNode;
            }

            return null;
        }
    }
    public class ExTree : ExTreeStack
    {
        public UInt32 tempID;
        public UInt32 dataPos;
        public Boolean isLeaf;
        public ExMesh m_xMesh;
    }
    public class ExMesh
    {
        public UInt32 m_uPolygonCount;
        public UInt32 m_uIndexCount;
        public ExVertex[] m_axVertices;
        public ExPolygon[] m_axPolygons;
    }

    public class Utility
    {
        public static String CleanName(String name)
        {
            if (name == null)
            {
                return "";
            }

            int index = name.IndexOfAny(new char[] { ',', '\0' });
            if (index >= 0)
            {
                name = name.Substring(0, index);
            }

            return name.Trim();
        }

        public static void FlipRedAndBlue(ref UInt32 colour)
        {
            UInt32 tempColour = colour;
            colour =
                (tempColour & 0xFF000000) |
                ((tempColour << 16) & 0x00FF0000) |
                (tempColour & 0x0000FF00) |
                ((tempColour >> 16) & 0x000000FF);
            return;
        }

        public static float BizarreFloatToNormalFloat(UInt16 usBizarreFloat)
        {
            //UInt32 usSign = (UInt32)usBizarreFloat & 0x00008000;
            //usSign >>= 15;
            //UInt32 usExponent = (UInt32)usBizarreFloat & 0x00007C00;
            //usExponent >>= 10;
            //usExponent -= 15;
            //UInt32 usSignificand = (UInt32)usBizarreFloat & 0x000003FF;
            //float fFraction = 1f;
            //for (int i = 0; i < 10; i++)
            //{
            //    UInt32 usCurrent = usSignificand;
            //    usCurrent = (byte)(usCurrent << (i + 1));
            //    usCurrent = (byte)(usCurrent >> 10);
            //    fFraction += (float)((float)usCurrent * Math.Pow(2, 0 - (1 + i)));
            //}
            //float fCalcValue = (float)(fFraction * Math.Pow(2, (double)usExponent));
            //return fCalcValue;

            // Converts the 16-bit floating point values used in the DC version of Soul Reaver to normal 32-bit floats
            ushort usExponent;
            int iUnbiasedExponent;
            ushort usSignificand;
            bool bPositive = true;
            ushort usSignCheck = usBizarreFloat;
            usSignCheck = (ushort)(usSignCheck >> 15);
            if (usSignCheck != 0)
            {
                bPositive = false;
            }

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
            if (!bPositive)
            {
                fCalcValue *= -1f;
            }
            return fCalcValue;
        }

        public static float ClampToRange(float fValue, float fMin, float fMax)
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

        public static void AdjustUVs(ref ExUV xUV, float fCentreU, float fCentreV, float fSizeAdjust, float fOffsetAdjust)
        {
            if (fCentreU < xUV.u)
            {
                xUV.u = Math.Max(fCentreU, xUV.u - fSizeAdjust);
            }
            if (fCentreU > xUV.u)
            {
                xUV.u = Math.Min(fCentreU, xUV.u + fSizeAdjust);
            }
            xUV.u = ClampToRange(xUV.u + fOffsetAdjust, 0.0f, 255.0f);

            if (fCentreV < xUV.v)
            {
                xUV.v = Math.Max(fCentreV, xUV.v - fSizeAdjust);
            }
            if (fCentreV > xUV.v)
            {
                xUV.v = Math.Min(fCentreV, xUV.v + fSizeAdjust);
            }
            xUV.v = ClampToRange(xUV.v + fOffsetAdjust, 0.0f, 255.0f);
        }
    }

    public abstract class SRModel
    {
        protected String m_strModelName;
        protected UInt32 m_uVersion;
        protected Platform m_ePlatform;
        protected UInt32 m_uDataStart;
        protected UInt32 m_uModelData;
        protected UInt32 m_uVertexCount;
        protected UInt32 m_uVertexStart;
        protected UInt32 m_uPolygonCount;
        protected UInt32 m_uPolygonStart;
        protected UInt32 m_uBoneCount;
        protected UInt32 m_uBoneStart;
        protected UInt32 m_uTreeCount;
        protected UInt32 m_uMaterialCount;
        protected UInt32 m_uMaterialStart;
        protected UInt32 m_uIndexCount { get { return 3 * m_uPolygonCount; } }
        protected ExVector m_xVertexScale;
        protected ExVertex[] m_axVertices;
        protected ExPosition[] m_axPositions;
        protected ExVector[] m_axNormals;
        protected UInt32[] m_auColours;
        protected ExUV[] m_axUVs;
        protected ExPolygon[] m_axPolygons;
        protected ExBone[]  m_axBones;
        protected ExTree[] m_axTrees;
        protected ExMaterial[] m_axMaterials;
        protected List<ExMaterial> m_xMaterialsList;

        public String Name { get { return m_strModelName; } }
        public UInt32 PolygonCount { get { return m_uPolygonCount; } }
        public ExPolygon[] Polygons { get { return m_axPolygons; } }
        public UInt32 IndexCount { get { return m_uIndexCount; } }
        public ExVertex[] Vertices { get { return m_axVertices; } }
        public ExPosition[] Positions { get { return m_axPositions; } }
        public ExVector[] Normals { get { return m_axNormals; } }
        public UInt32[] Colours { get { return m_auColours; } }
        public ExUV[] UVs { get { return m_axUVs; } }
        public ExBone[] Bones { get { return m_axBones; } }
        public UInt32 MeshCount { get { return m_uTreeCount; } }
        public ExTree[] Groups { get { return m_axTrees; } }
        public UInt32 MaterialCount { get { return m_uMaterialCount; } }
        public ExMaterial[] Materials { get { return m_axMaterials; } }
        public Platform Platform { get { return m_ePlatform; } }

        protected SRModel(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt32 uVersion)
        {
            m_strModelName = strModelName;
            m_ePlatform = ePlatform;
            m_uVersion = uVersion;
            m_uDataStart = uDataStart;
            m_uModelData = uModelData;
            m_uVertexCount = 0;
            m_uVertexStart = 0;
            m_uPolygonCount = 0;
            m_uPolygonStart = 0;
            m_xVertexScale.x = 1.0f;
            m_xVertexScale.y = 1.0f;
            m_xVertexScale.z = 1.0f;
            m_xMaterialsList = new List<ExMaterial>();
        }
    }

    public abstract class SRFile
    {
        public String m_strModelName;
        public UInt32 m_uVersion;
        public UInt32 m_uDataStart;
        public UInt16 m_usModelCount;
        public UInt16 m_usAnimCount;
        public UInt32 m_uModelStart;
        public SRModel[] m_axModels;
        public UInt32 m_uAnimStart;
        public UInt32 m_uInstanceCount;
        public UInt32 m_uInstanceStart;
        public String[] m_astrInstances;
        public UInt32 m_uInstanceTypesStart;
        public String[] m_axInstanceTypeNames;
        public UInt32 m_uConnectedUnitCount;
        public UInt32 m_uConnectedUnitsStart;
        public String[] m_astrConnectedUnit;
        public FileType m_eFileType;
        public Platform m_ePlatform;

        public static StreamWriter m_xLogFile = null;

        protected SRFile(String strFileName)
        {
            FileStream xFile = new FileStream(strFileName, FileMode.Open, FileAccess.Read);
            BinaryReader xReader = new BinaryReader(xFile);
            MemoryStream xStream = new MemoryStream((int)xFile.Length);
            BinaryWriter xWriter = new BinaryWriter(xStream);

            //String strDebugFileName = Path.GetDirectoryName(strFileName) + "\\" + Path.GetFileNameWithoutExtension(strFileName) + "-Debug.txt";
            //m_xLogFile = File.CreateText(strDebugFileName);

            ResolvePointers(xReader, xWriter);
            xReader.Close();
            xReader = new BinaryReader(xStream);

            ReadHeaderData(xReader);

            if (m_eFileType == FileType.Object)
            {
                ReadObjectData(xReader);
            }
            else
            {
                ReadUnitData(xReader);
            }

            xReader.Close();

            if (m_xLogFile != null)
            {
                m_xLogFile.Close();
                m_xLogFile = null;
            }
        }

        protected abstract void ReadHeaderData(BinaryReader xReader);

        protected abstract void ReadObjectData(BinaryReader xReader);

        protected abstract void ReadUnitData(BinaryReader xReader);

        protected abstract void ResolvePointers(BinaryReader xReader, BinaryWriter xWriter);
    }
}
