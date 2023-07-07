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

	public enum Game : int
	{
		Gex,
		SR1,
		SR2,
		Defiance,
		TRL,
		TRA
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

	public struct Vector4
	{
		public float x, y, z, w;
		public Vector4(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}
		public static Vector4 operator *(Vector4 v1, Vector4 v2)
		{
			return new Vector4(
				v1.x * v2.x,
				v1.y * v2.y,
				v1.z * v2.z,
				v1.w * v2.w
			);
		}

		public static Vector4 operator +(Vector4 v1, Vector4 v2)
		{
			return new Vector4(
				v1.x + v2.x,
				v1.y + v2.y,
				v1.z + v2.z,
				v1.w + v2.w
			);
		}

		public static Vector4 operator *(Vector4 v, float f)
		{
			return new Vector4(
				v.x *= f,
				v.y *= f,
				v.z *= f,
				v.w *= f
			);
		}
	}

	public struct Matrix
	{
		public Vector4 v0, v1, v2, v3;

		public Matrix(Vector4 v0, Vector4 v1, Vector4 v2, Vector4 v3)
		{
			this.v0 = v0;
			this.v1 = v1;
			this.v2 = v2;
			this.v3 = v3;
		}
	}

	public struct Bone
	{
		public ushort vFirst, vLast; // The ID of first and last effected vertex 
		public Vector localPos;      // Local bone coordinates
		public Vector worldPos;      // World bone coordinated
		public ushort parentID1;     // ID of parent bone 1
		public ushort parentID2;     // ID of parent bone 2
		public uint flags;           // Flags including which parent to use.
		public float weight;
	}

	public struct Normal
	{
		public int x, y, z;
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
		public ushort index;          // Index in the file
		public Vector basePos;        // Base vertex coordinates
		public Vector offset;         // Offset from base coordinates
	}

	public struct Polygon
	{
		public Material material;       // The material used
		public uint materialOffset;
		public Vertex v1, v2, v3;       // Vertices for the polygon
		public int normal;
		public short rootBSPTreeID;
		public string BSPNodeID;
	}

	public struct TreePolygon
	{
		public uint textureID;
		public uint vbBaseOffset;

		public ushort v1, v2, v3;
		public bool useExtraGeometry;
	}

	public struct Intro
	{
		public string name;
		public string fileName;
		public int introNum;
		public int uniqueID;
		public int objectID;
		public Vector position;
		public Vector rotation;
		public int modelIndex;
		public int monsterAge;
        public override string ToString()
        {
			return name;
        }
    }

	public struct BGInstance
	{
		public ushort id;
		public uint bgObject;
		public Matrix matrix;
		public int modelIndex;
		public string name;
	}

	public struct Portal
	{
		public string toLevelName;
		public int mSignalID;
		public Vector min;
		public Vector max;
		public Vector[] t1;
		public Vector[] t2;
		public Vector[] quad;
	}

	public struct MonsterAttributes
	{
		public int numSubAttributes;
		public MonsterSubAttributes[] subAttributes;
	}

	public struct MonsterSubAttributes
	{
		public uint dataStart;
		public int modelNum;
	}

	public class Material
	{
		public const float OPACITY_TRANSLUCENT = 0.6f;    // 0.3
		public const float OPACITY_BARELY_VISIBLE = 0.4f; // 0.25f

		public ushort ID;                 // The ID of the material
		public Boolean visible;           // Flag specifying if this material is visible
		public int blendMode;             // The type of operation used to handle transparency
		public Boolean textureUsed;       // Flag specifying if this material has a texture
		public ushort textureID;          // ID of the texture file
		public ushort texturePage;        // raw "tpage" value from the DRM
		public uint colour;               // Diffuse colour
		public Boolean isTranslucent;
		public Boolean isEmissive;
		public float opacity;
		public float emissivity;
		public bool UseAlphaMask;
		public byte polygonFlags;
		public byte sortPush;
		public ushort textureAttributes;
		public ushort textureAttributesA;
		public ushort clutValue;          // Colour lookup table row/column
		public short BSPRootTreeID;
		public ushort BSPTreeRootFlags;
		public ushort BSPTreeParentNodeFlags;
		public ushort BSPTreeAllParentNodeFlagsORd;
		public ushort BSPTreeLeafFlags;

		// bitflag masks
		public byte polygonFlagsUsedMask;
		public byte sortPushUsedMask;
		public ushort texturePageUsedMask;
		public uint colourUsedMask;
		public ushort textureAttributesUsedMask;
		public ushort textureAttributesAUsedMask;
		public ushort clutValueUsedMask;
		public ushort BSPTreeRootFlagsUsedMask;
		public ushort BSPTreeParentNodeFlagsUsedMask;
		public ushort BSPTreeAllParentNodeFlagsORdUsedMask;
		public ushort BSPTreeLeafFlagsUsedMask;

		// values for matching whether or not a material is the same or not
		public byte polygonFlagsEffective { get { return (byte)(polygonFlagsUsedMask & polygonFlags); } }

		public byte sortPushEffective { get { return (byte)(sortPushUsedMask & sortPush); } }

		public ushort texturePageEffective { get { return (ushort)(texturePageUsedMask & texturePage); } }

		public uint colourEffective { get { return (uint)(colourUsedMask & colour); } }

		public ushort textureAttributesEffective { get { return (ushort)(textureAttributesUsedMask & textureAttributes); } }

		public ushort textureAttributesAEffective { get { return (ushort)(textureAttributesAUsedMask & textureAttributesA); } }

		public ushort clutValueEffective { get { return (ushort)(clutValueUsedMask & clutValue); } }

		public ushort BSPTreeRootFlagsEffective { get { return (ushort)(BSPTreeRootFlagsUsedMask & BSPTreeRootFlags); } }

		public ushort BSPTreeParentNodeFlagsEffective { get { return (ushort)(BSPTreeParentNodeFlagsUsedMask & BSPTreeParentNodeFlags); } }

		public ushort BSPTreeAllParentNodeFlagsORdEffective { get { return (ushort)(BSPTreeAllParentNodeFlagsORdUsedMask & BSPTreeAllParentNodeFlagsORd); } }

		public ushort BSPTreeLeafFlagsEffective { get { return (ushort)(BSPTreeLeafFlagsUsedMask & BSPTreeLeafFlags); } }

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
			BSPRootTreeID = 0;
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
			clone.blendMode = blendMode;
			clone.textureUsed = textureUsed;
			clone.textureID = textureID;
			clone.texturePage = texturePage;
			clone.colour = colour;
			clone.isTranslucent = isTranslucent;
			clone.isEmissive = isEmissive;
			clone.opacity = opacity;
			clone.emissivity = emissivity;
			clone.UseAlphaMask = UseAlphaMask;
			clone.polygonFlags = polygonFlags;
			clone.sortPush = sortPush;
			clone.textureAttributes = textureAttributes;
			clone.textureAttributesA = textureAttributesA;
			clone.clutValue = clutValue;
			clone.BSPRootTreeID = BSPRootTreeID;
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
			if (material.ID == this.material.ID &&
				material.visible == this.material.visible &&
				material.blendMode == this.material.blendMode &&
				material.textureUsed == this.material.textureUsed &&
				material.textureID == this.material.textureID &&
				material.texturePage == this.material.texturePage &&
				material.colour == this.material.colour &&
				material.isTranslucent == this.material.isTranslucent &&
				material.isEmissive == this.material.isEmissive &&
				material.opacity == this.material.opacity &&
				material.emissivity == this.material.emissivity &&
				material.UseAlphaMask == this.material.UseAlphaMask &&
				material.polygonFlags == this.material.polygonFlags &&
				material.sortPush == this.material.sortPush &&
				material.textureAttributes == this.material.textureAttributes &&
				material.textureAttributesA == this.material.textureAttributesA &&
				material.clutValue == this.material.clutValue &&
				material.BSPRootTreeID == this.material.BSPRootTreeID &&
				material.BSPTreeRootFlags == this.material.BSPTreeRootFlags &&
				material.BSPTreeParentNodeFlags == this.material.BSPTreeParentNodeFlags &&
				material.BSPTreeAllParentNodeFlagsORd == this.material.BSPTreeAllParentNodeFlagsORd &&
				material.BSPTreeLeafFlags == this.material.BSPTreeLeafFlags &&
				material.polygonFlagsUsedMask == this.material.polygonFlagsUsedMask &&
				material.sortPushUsedMask == this.material.sortPushUsedMask &&
				material.texturePageUsedMask == this.material.texturePageUsedMask &&
				material.colourUsedMask == this.material.colourUsedMask &&
				material.textureAttributesUsedMask == this.material.textureAttributesUsedMask &&
				material.textureAttributesAUsedMask == this.material.textureAttributesAUsedMask &&
				material.clutValueUsedMask == this.material.clutValueUsedMask &&
				material.BSPTreeRootFlagsUsedMask == this.material.BSPTreeRootFlagsUsedMask &&
				material.BSPTreeParentNodeFlagsUsedMask == this.material.BSPTreeParentNodeFlagsUsedMask &&
				material.BSPTreeAllParentNodeFlagsORdUsedMask == this.material.BSPTreeAllParentNodeFlagsORdUsedMask &&
				material.BSPTreeLeafFlagsUsedMask == this.material.BSPTreeLeafFlagsUsedMask)
			{
				return this.material;
			}

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
		private uint nodeCount;
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

		public uint Count
		{
			get { return nodeCount; }
		}

		public Tree GetNode(uint uIndex)
		{
			Node xCurrentNode = currentNode;
			for (uint i = 0; i <= uIndex; i++)
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
		public uint tempID;
		public uint dataPos;
		public Boolean isLeaf;
		public Mesh mesh;
		public ushort sr1Flags;
		public Vector globalOffset = new Vector();
	}

	public class Mesh
	{
		public uint polygonCount;
		public uint indexCount;
		public uint startIndex;
		public Vertex[] vertices;
		public Polygon[] polygons;
		public ushort sr1BSPTreeFlags;
		public List<ushort> sr1BSPNodeFlags;
		public List<ushort> sr1BSPLeafFlags;

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
		public uint[] Colours;
		public uint[] ColoursAlt;
		public UV[] UVs;
	}
}
