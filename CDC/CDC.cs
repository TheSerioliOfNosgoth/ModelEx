using System;
using System.Collections.Generic;

namespace CDC
{
    public enum Platform
    {
        None,
        PC,
        PSX,
        Dreamcast
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
        public int paletteRow;        // The row of the pallete to use (PS1)
        public int paletteColumn;     // The column of the pallet to use (PS1)
    }

    public struct TreePolygon
    {
        public UInt32 textureID;
        public UInt16 v1, v2, v3;
        public bool useExtraGeometry;
    }

    public class Material
    {
        public UInt16 ID;               // The ID of the material
        public Boolean visible;         // Flag specifying if this material is visible
        public Boolean textureUsed;     // Flag specifying if this material has a texture
        public UInt16 textureID;        // ID of the texture file
        public UInt32 colour;           // Diffuse colour
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
    }

    public class Mesh
    {
        public UInt32 polygonCount;
        public UInt32 indexCount;
        public UInt32 startIndex;
        public Vertex[] vertices;
        public Polygon[] polygons;
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
