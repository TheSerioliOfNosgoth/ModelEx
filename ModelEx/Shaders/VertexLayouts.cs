﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SlimDX;

namespace ModelEx
{
    public interface IVertexParser<V>
    {
        int VertexCount { get; }
        void FillVertex(int v, out V vertex);
    }
    public interface IIndexParser<I>
    {
        int IndexCount { get; }
        void FillIndex(int i, out I index);
    }
    public interface IMeshParser<V, I> : IVertexParser<V>, IIndexParser<I>
    {
        string MeshName { get; }
        string Technique { get; }
    }
    public interface IModelParser
    {

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PositionColoredVertex
    {
        public Vector3 Position;
        public int Color;

        public PositionColoredVertex(Vector3 position, int color)
        {
            this.Position = position;
            this.Color = color;
        }

        public PositionColoredVertex(float x, float y, float z, int color)
        {
            this.Position = new Vector3(x, y, z);
            this.Color = color;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PositionColoredNormalVertex
    {
        public Vector3 Position;
        public int Color;
        public Vector3 Normal;

        public PositionColoredNormalVertex(Vector3 position, int color, Vector3 normal)
        {
            this.Position = position;
            this.Color = color;
            this.Normal = normal;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PositionNormalVertex
    {
        public Vector3 Position;
        public Vector3 Normal;

        public PositionNormalVertex(Vector3 position, Vector3 normal)
        {
            this.Position = position;
            this.Normal = normal;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PositionNormalTexturedVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinates;

        public PositionNormalTexturedVertex(Vector3 position, Vector3 normal, Vector2 textureCoordinates)
        {
            this.Position = position;
            this.Normal = normal;
            this.TextureCoordinates = textureCoordinates;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PositionColorTexturedVertex
    {
        public Vector3 Position;
        public Color3 Color;
        public Vector2 TextureCoordinates;

        public PositionColorTexturedVertex(Vector3 position, Color3 color, Vector2 textureCoordinates)
        {
            this.Position = position;
            this.Color = color;
            this.TextureCoordinates = textureCoordinates;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PositionTexturedVertex
    {
        public Vector3 Position;
        public Vector2 TextureCoordinates;


        public PositionTexturedVertex(Vector3 position, Vector2 textureCoordinates)
        {
            this.Position = position;
            TextureCoordinates = textureCoordinates;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PositionTextured3DVertex
    {
        public Vector3 Position;
        public Vector3 TextureCoordinates;


        public PositionTextured3DVertex(Vector3 position, Vector3 textureCoordinates)
        {
            this.Position = position;
            TextureCoordinates = textureCoordinates;
        }
    }

}
