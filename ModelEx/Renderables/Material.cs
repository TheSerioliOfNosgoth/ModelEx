﻿using System;
using System.Collections.Generic;

namespace ModelEx
{
	public class Material
	{
		public String Name = "";
		public bool Visible = true;
		public float DepthBias = 0.0f;
		public int BlendMode = 0;
		public System.Drawing.Color Diffuse = System.Drawing.Color.White;
		public System.Drawing.Color Ambient = System.Drawing.Color.White;
		public System.Drawing.Color Emissive = System.Drawing.Color.White;
		public System.Drawing.Color Reflective = System.Drawing.Color.White;
		public System.Drawing.Color Specular = System.Drawing.Color.White;
		public System.Drawing.Color Transparent = System.Drawing.Color.White;
		public float ColorFactor = 1.0f;
		public string TextureFileName = "";
	}
}