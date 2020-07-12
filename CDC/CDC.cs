using System;
using System.Collections.Generic;

namespace CDC
{
    public enum Platform
    {
        None,
        PC,
        PSX,
        PlayStation2,
        Dreamcast,
        Xbox
    }

    public enum Game
    {
        SR1,
        SR2,
        Defiance
    }

    public enum Asset
    {
        Object,
        Unit
    }

    public struct Vector
    {
        public float x, y, z;
        public Vector(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public static Vector operator *(Vector v1, Vector v2)
        {
            return new Vector(
                v1.x * v2.x,
                v1.y * v2.y,
                v1.z * v2.z
            );
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            return new Vector(
                v1.x + v2.x,
                v1.y + v2.y,
                v1.z + v2.z
            );
        }

        public static Vector operator *(Vector v, float f)
        {
            return new Vector(
                v.x *= f,
                v.y *= f,
                v.z *= f
            );
        }
    }

    public struct Bone
    {
        public UInt16 vFirst, vLast;    // The ID of first and last effected vertex 
        public Vector localPos;       // Local bone coordinates
        public Vector worldPos;       // World bone coordinated
        public UInt16 parentID1;        // ID of parent bone 1
        public UInt16 parentID2;        // ID of parent bone 2
        public UInt32 flags;            // Flags including which parent to use.
    }

    public struct Normal
    {
        public Int32 x, y, z;
    }

    public struct UV
    {
        public float u, v;
    }

    public struct Vertex
    {
        public int positionID;          // Index of the vertex position
        public int normalID;            // Index of the vertex normal
        public int colourID;            // Index of the vertex colour
        public int UVID;                // Index of the vertex UV
        public int boneID;              // Index of the vertex bone influence
        public ushort RGB15;
        public ushort code;
        public Boolean isExtraGeometry; // Is part of the extra geometry (Should be in the polygon or mesh, but need to refactor)
    }

    public struct ShiftVertex
    {
        public UInt16 index;          // Index in the file
        public Vector basePos;        // Base vertex coordinates
        public Vector offset;         // Offset from base coordinates
    }

    public struct Polygon
    {
        public Material material;     // The material used
        public Vertex v1, v2, v3;     // Vertices for the polygon
        public ushort CLUT;
        public int paletteRow;          // The row of the pallete to use (PS1)
        public int paletteColumn;       // The column of the pallet to use (PS1)
        public int normal;
        //public byte sr1Flags;           // flags value from Soul Reaver specifically
        //public UInt16 sr1TextureFT3Attributes;
        public UInt32 colour;           // Diffuse colour
        public uint RootBSPTreeNumber;
        public string BSPNodeID;
    }

    public struct TreePolygon
    {
        public UInt32 textureID;
        public UInt16 v1, v2, v3;
        public bool useExtraGeometry;
    }

    public class Material
    {
        public const float OPACITY_TRANSLUCENT = 0.6f;         // 0.3
        public const float OPACITY_BARELY_VISIBLE = 0.4f;      // 0.25f

        public UInt16 ID;               // The ID of the material
        public Boolean visible;         // Flag specifying if this material is visible
        public Boolean textureUsed;     // Flag specifying if this material has a texture
        public UInt16 textureID;        // ID of the texture file
        public UInt16 texturePage;      // raw "tpage" value from the DRM
        public UInt32 colour;           // Diffuse colour
        public float opacity;
        public float emissivity;
        public bool UseAlphaMask;
        public byte polygonFlags;
        public byte sortPush;
        public UInt16 textureAttributes;
        public UInt16 textureAttributesA;
        public UInt16 clutValue;     // Colour lookup table row/column
        public uint RootBSPTreeNumber;
        public UInt16 BSPTreeRootFlags;
        public UInt16 BSPTreeParentNodeFlags;
        public UInt16 BSPTreeAllParentNodeFlagsORd;
        public UInt16 BSPTreeLeafFlags;

        // bitflag masks
        public byte polygonFlagsUsedMask;
        public byte sortPushUsedMask;
        public UInt16 texturePageUsedMask;
        public UInt32 colourUsedMask;
        public UInt16 textureAttributesUsedMask;
        public UInt16 textureAttributesAUsedMask;
        public UInt16 clutValueUsedMask;
        public UInt16 BSPTreeRootFlagsUsedMask;
        public UInt16 BSPTreeParentNodeFlagsUsedMask;
        public UInt16 BSPTreeAllParentNodeFlagsORdUsedMask;
        public UInt16 BSPTreeLeafFlagsUsedMask;

        // values for matching whether or not a material is the same or not
        public byte polygonFlagsEffective
        {
            get
            {
                return (byte)(polygonFlagsUsedMask & polygonFlags);
            }
        }

        public byte sortPushEffective
        {
            get
            {
                return (byte)(sortPushUsedMask & sortPush);
            }
        }

        public UInt16 texturePageEffective
        {
            get
            {
                return (UInt16)(texturePageUsedMask & texturePage);
            }
        }

        public UInt32 colourEffective
        {
            get
            {
                return (UInt32)(colourUsedMask & colour);
            }
        }

        public UInt16 textureAttributesEffective
        {
            get
            {
                return (UInt16)(textureAttributesUsedMask & textureAttributes);
            }
        }

        public UInt16 textureAttributesAEffective
        {
            get
            {
                return (UInt16)(textureAttributesAUsedMask & textureAttributesA);
            }
        }

        public UInt16 clutValueEffective
        {
            get
            {
                return (UInt16)(clutValueUsedMask & clutValue);
            }
        }

        public UInt16 BSPTreeRootFlagsEffective
        {
            get
            {
                return (UInt16)(BSPTreeRootFlagsUsedMask & BSPTreeRootFlags);
            }
        }

        public UInt16 BSPTreeParentNodeFlagsEffective
        {
            get
            {
                return (UInt16)(BSPTreeParentNodeFlagsUsedMask & BSPTreeParentNodeFlags);
            }
        }

        public UInt16 BSPTreeAllParentNodeFlagsORdEffective
        {
            get
            {
                return (UInt16)(BSPTreeAllParentNodeFlagsORdUsedMask & BSPTreeAllParentNodeFlagsORd);
            }
        }

        public UInt16 BSPTreeLeafFlagsEffective
        {
            get
            {
                return (UInt16)(BSPTreeLeafFlagsUsedMask & BSPTreeLeafFlags);
            }
        }

        public Material()
        {
            opacity = 1.0f;
            emissivity = 0.0f;
            UseAlphaMask = false;
            polygonFlags = 0;
            sortPush = 0;
            textureAttributes = 0;
            textureAttributesA = 0;
            texturePage = 0;
            textureID = 0;
            RootBSPTreeNumber = 0;
            BSPTreeRootFlags = 0;
            BSPTreeParentNodeFlags = 0;
            BSPTreeAllParentNodeFlagsORd = 0;
            BSPTreeLeafFlags = 0;

            polygonFlagsUsedMask = 0xFF;
            sortPushUsedMask = 0xFF;
            texturePageUsedMask = 0xFFFF;
            colourUsedMask = 0xFFFFFFFF;
            textureAttributesUsedMask = 0xFFFF;
            textureAttributesAUsedMask = 0xFFFF;
            clutValueUsedMask = 0xFFFF;
            BSPTreeRootFlagsUsedMask = 0xFFFF;
            BSPTreeParentNodeFlagsUsedMask = 0xFFFF;
            BSPTreeAllParentNodeFlagsORdUsedMask = 0xFFFF;
            BSPTreeLeafFlagsUsedMask = 0xFFFF;
        }

        public Material Clone()
        {
            Material clone = new Material();
            clone.ID = ID;
            clone.visible = visible;
            clone.textureUsed = textureUsed;
            clone.texturePage = texturePage;
            clone.textureID = textureID;
            clone.textureAttributesA = textureAttributesA;
            clone.colour = colour;
            clone.opacity = opacity;
            clone.UseAlphaMask = UseAlphaMask;
            clone.emissivity = emissivity;
            clone.polygonFlags = polygonFlags;
            clone.sortPush = sortPush;
            clone.textureAttributes = textureAttributes;
            clone.RootBSPTreeNumber = RootBSPTreeNumber;
            clone.BSPTreeRootFlags = BSPTreeRootFlags;
            clone.BSPTreeParentNodeFlags = BSPTreeParentNodeFlags;
            clone.BSPTreeAllParentNodeFlagsORd = BSPTreeAllParentNodeFlagsORd;
            clone.BSPTreeLeafFlags = BSPTreeLeafFlags;
            clone.polygonFlagsUsedMask = polygonFlagsUsedMask;
            clone.sortPushUsedMask = sortPushUsedMask;
            clone.texturePageUsedMask = texturePageUsedMask;
            clone.colourUsedMask = colourUsedMask;
            clone.textureAttributesUsedMask = textureAttributesUsedMask;
            clone.textureAttributesAUsedMask = textureAttributesAUsedMask;
            clone.clutValueUsedMask = clutValueUsedMask;
            clone.BSPTreeRootFlagsUsedMask = BSPTreeRootFlagsUsedMask;
            clone.BSPTreeParentNodeFlagsUsedMask = BSPTreeParentNodeFlagsUsedMask;
            clone.BSPTreeAllParentNodeFlagsORdUsedMask = BSPTreeAllParentNodeFlagsORdUsedMask;
            clone.BSPTreeLeafFlagsUsedMask = BSPTreeLeafFlagsUsedMask;
            return clone;
        }
    }

    public class MaterialList
    {
        protected MaterialList _next;
        public MaterialList next
        {
            get { return _next; }
        }
        public Material material;
        public MaterialList(Material material)
        {
            this.material = material;
            _next = null;
        }
        // Tries to add the material to the list
        public virtual Material AddToList(Material material)
        {
            // Check if the material is already in the list
            if (//(material.RootBSPTreeNumber == this.material.RootBSPTreeNumber) &&
                (material.textureID == this.material.textureID) &&
                (material.texturePageEffective == this.material.texturePageEffective) &&
                (material.clutValueEffective == this.material.clutValueEffective) &&
                (material.colourEffective == this.material.colourEffective) &&
                (material.textureUsed == this.material.textureUsed) &&
                (material.opacity.Equals(this.material.opacity)) &&
                (material.emissivity.Equals(this.material.emissivity)) &&
                (material.polygonFlagsEffective == this.material.polygonFlagsEffective) &&
                (material.sortPushEffective == this.material.sortPushEffective) &&
                (material.textureAttributesEffective == this.material.textureAttributesEffective) &&
                (material.textureAttributesAEffective == this.material.textureAttributesAEffective) &&
                (material.BSPTreeRootFlagsEffective == this.material.BSPTreeRootFlagsEffective) &&
                (material.BSPTreeParentNodeFlagsEffective == this.material.BSPTreeParentNodeFlagsEffective) &&
                (material.BSPTreeLeafFlagsEffective == this.material.BSPTreeLeafFlagsEffective) &&
                (material.UseAlphaMask == this.material.UseAlphaMask)
                )
            {
                return this.material;
            }
            //// Check if the material is already in the list
            //if ((material.textureID == this.material.textureID) &&
            //    (material.clutValue == this.material.clutValue) &&
            //    (material.texturePage == this.material.texturePage) &&
            //    (material.colour == this.material.colour) &&
            //    (material.textureUsed == this.material.textureUsed) &&
            //    (material.opacity.Equals(this.material.opacity)) &&
            //    (material.emissivity.Equals(this.material.emissivity)) &&
            //    (material.polygonFlags == this.material.polygonFlags) &&
            //    (material.sortPush == this.material.sortPush) &&
            //    (material.textureAttributes == this.material.textureAttributes) &&
            //    (material.BSPTreeRootFlags == this.material.BSPTreeRootFlags) &&
            //    (material.BSPTreeParentNodeFlags == this.material.BSPTreeParentNodeFlags) &&
            //    (material.BSPTreeAllParentNodeFlagsORd == this.material.BSPTreeAllParentNodeFlagsORd) &&
            //    (material.BSPTreeLeafFlags == this.material.BSPTreeLeafFlags) &&
            //    (material.UseAlphaMask == this.material.UseAlphaMask)
            //    )
            //{
            //    return this.material;
            //}
            // Check the rest of the list
            if (next != null)
            {
                return next.AddToList(material);
            }
            // Add the material to the list
            _next = new MaterialList(material);
            return material;
        }
    }

    public class TreeStack
    {
        private class Node
        {
            public Tree tree;
            public Node lastNode;
        }
        private Node firstNode;
        private Node currentNode;
        private UInt32 nodeCount;
        public void Push(Tree tree)
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

        public Tree Pop()
        {
            if (currentNode == null) return null;
            Tree tree = currentNode.tree;
            currentNode = currentNode.lastNode;
            nodeCount--;
            if (nodeCount == 0)
            {
                firstNode = null;
            }
            return tree;
        }

        public Tree Top
        {
            get
            {
                if (currentNode == null) return null;
                return currentNode.tree;
            }
        }

        public Tree Start
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

        public Tree GetNode(UInt32 uIndex)
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

    public class Tree : TreeStack
    {
        public UInt32 tempID;
        public UInt32 dataPos;
        public Boolean isLeaf;
        public Mesh mesh;
        public UInt16 sr1Flags;
    }

    public class Mesh
    {
        public UInt32 polygonCount;
        public UInt32 indexCount;
        public UInt32 startIndex;
        public Vertex[] vertices;
        public Polygon[] polygons;
        public UInt16 sr1BSPTreeFlags;
        public List<UInt16> sr1BSPNodeFlags;
        public List<UInt16> sr1BSPLeafFlags;

        public Mesh()
        {
            sr1BSPNodeFlags = new List<ushort>();
            sr1BSPLeafFlags = new List<ushort>();
        }
    }

    public class Geometry
    {
        public Vertex[] Vertices;
        public Vector[] PositionsRaw;
        public Vector[] PositionsPhys;
        public Vector[] PositionsAltPhys;
        public Vector[] Normals;
        public UInt32[] Colours;
        public UInt32[] ColoursAlt;
        public UV[] UVs;
    }
}
