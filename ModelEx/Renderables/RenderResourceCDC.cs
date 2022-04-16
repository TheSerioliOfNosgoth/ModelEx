using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SRFile = CDC.Objects.SRFile;
using GexFile = CDC.Objects.GexFile;
using SR1File = CDC.Objects.SR1File;
using SR2File = CDC.Objects.SR2File;
using DefianceFile = CDC.Objects.DefianceFile;
using TRLFile = CDC.Objects.TRLFile;
using SRModel = CDC.Objects.Models.SRModel;
using Gex3PSTextureFile = BenLincoln.TheLostWorlds.CDTextures.Gex3PlaystationVRMTextureFile;
using SR1PCTextureFile = BenLincoln.TheLostWorlds.CDTextures.SoulReaverPCTextureFile;
using SR1PSTextureFile = BenLincoln.TheLostWorlds.CDTextures.SoulReaverPlaystationTextureFile;
using SR1DCTextureFile = BenLincoln.TheLostWorlds.CDTextures.SoulReaverDreamcastTextureFile;
using SR2PCTextureFile = BenLincoln.TheLostWorlds.CDTextures.SoulReaver2PCVRMTextureFile;
using TRLPCTextureFile = BenLincoln.TheLostWorlds.CDTextures.TombRaiderPCDRMTextureFile;

namespace ModelEx
{
	public class RenderResourceCDC : RenderResource
	{
		public SRFile File { get; private set; }
		public LoadRequestCDC LoadRequest { get; private set; }

		public const string TextureExtension = ".png";
		private CDC.Objects.ExportOptions ExportOptions;

		public RenderResourceCDC(SRFile srFile, LoadRequestCDC loadRequest)
			: base(srFile.Name)
		{
			File = srFile;
			LoadRequest = (LoadRequestCDC)loadRequest.Clone();
			ExportOptions = LoadRequest.ExportOptions;
		}

		public void LoadModels()
		{
			foreach (Model model in Models)
			{
				model.Dispose();
			}

			for (int modelIndex = 0; modelIndex < File.Models.Length; modelIndex++)
			{
				SRModelParser modelParser = new SRModelParser(File.Name, File);
				modelParser.BuildModel(this, modelIndex, ExportOptions);
				if (File.Asset == CDC.Asset.Unit)
				{
					Models.Add(modelParser.Model);
				}
				else
				{
					Models.Add(modelParser.Model);
				}
			}
		}

		public void LoadTextures(string fileName)
		{
			FileShaderResourceViewDictionary.Clear();

			Type currentFileType = File.GetType();
			if (currentFileType == typeof(TRLFile))
			{
				try
				{
					TRLPCTextureFile textureFile = new TRLPCTextureFile(fileName);

					SceneCDC.progressLevel = 0;
					SceneCDC.progressLevels = textureFile.TextureCount;
					SceneCDC.ProgressStage = "Loading Textures";

					for (int t = 0; t < textureFile.TextureCount; t++)
					{
						String textureName = SRModel.GetPS2TextureName(File.Name, (int)textureFile.TextureDefinitions[t].ID) + TextureExtension;

						System.IO.MemoryStream stream = textureFile.GetDataAsStream(t);
						//textureFile.ExportFile(t, "C:\\Users\\A\\Desktop\\Lara\\" + textureName);
						if (stream != null)
						{
							if (textureFile.TextureDefinitions[t].Format == BenLincoln.TheLostWorlds.CDTextures.DRMFormat.Uncompressed)
							{
								MemoryStream stream2 = textureFile.GetUncompressedDataAsStream2(t);
								AddTexture(stream2, textureName);
							}
							else
							{
								AddTexture(stream, textureName);
							}
							_TexturesAsPNGs.Add(textureName, textureFile.GetTextureAsBitmap(t));
						}

						SceneCDC.progressLevel++;
					}
				}
				catch (FileNotFoundException)
				{
					Console.WriteLine("Error: couldn't find a texture file");
				}
				catch (Exception ex)
				{
					Console.Write(ex.ToString());
				}
			}
			else if (currentFileType == typeof(SR2File) ||
					 currentFileType == typeof(DefianceFile))
			{
				try
				{
					SR2PCTextureFile textureFile = new SR2PCTextureFile(fileName);

					SceneCDC.progressLevel = 0;
					SceneCDC.progressLevels = textureFile.TextureCount;
					SceneCDC.ProgressStage = "Loading Textures";

					for (int t = 0; t < textureFile.TextureCount; t++)
					{
						String textureName = SRModel.GetPS2TextureName(File.Name, textureFile.TextureDefinitions[t].Flags1) + TextureExtension;

						System.IO.MemoryStream stream = textureFile.GetDataAsStream(t);
						//textureFile.ExportFile(t, "C:\\Users\\A\\Desktop\\" + textureName);
						if (stream != null)
						{
							AddTexture(stream, textureName);
							_TexturesAsPNGs.Add(textureName, textureFile.GetTextureAsBitmap(t));
						}

						SceneCDC.progressLevel++;
					}
				}
				catch (FileNotFoundException)
				{
					Console.WriteLine("Error: couldn't find a texture file");
				}
				catch (Exception ex)
				{
					Console.Write(ex.ToString());
				}
			}
			else if (currentFileType == typeof(SR1File))
			{
				if (File.Platform == CDC.Platform.PC)
				{
					try
					{
						SR1PCTextureFile textureFile = new SR1PCTextureFile(fileName);

						SceneCDC.progressLevel = 0;
						SceneCDC.progressLevels = File.GetNumMaterials();
						SceneCDC.ProgressStage = "Loading Textures";

						foreach (SRModel srModel in File.Models)
						{
							foreach (CDC.Material material in srModel.Materials)
							{
								if (material.textureUsed)
								{
									System.IO.MemoryStream stream = textureFile.GetDataAsStream(material.textureID);
									if (stream != null)
									{
										String textureName = SRModel.GetSoulReaverPCOrDreamcastTextureName(srModel.Name, material.textureID) + TextureExtension;
										AddTexture(stream, textureName);
										if (!_TexturesAsPNGs.ContainsKey(textureName))
										{
											_TexturesAsPNGs.Add(textureName, textureFile.GetTextureAsBitmap(material.textureID));
										}
									}
								}

								SceneCDC.progressLevel++;
							}
						}
					}
					catch (FileNotFoundException)
					{
						Console.WriteLine("Error: couldn't find a texture file");
					}
					catch (Exception ex)
					{
						Console.Write(ex.ToString());
					}
				}
				else if (File.Platform == CDC.Platform.Dreamcast)
				{
					try
					{
						SR1DCTextureFile textureFile = new SR1DCTextureFile(fileName);

						SceneCDC.progressLevel = 0;
						SceneCDC.progressLevels = File.GetNumMaterials();
						SceneCDC.ProgressStage = "Loading Textures";

						foreach (SRModel srModel in File.Models)
						{
							foreach (CDC.Material material in srModel.Materials)
							{
								if (material.textureUsed)
								{
									int textureID = material.textureID;
									System.IO.MemoryStream stream = textureFile.GetDataAsStream(textureID);
									if (stream != null)
									{
										String textureName = SRModel.GetSoulReaverPCOrDreamcastTextureName(srModel.Name, material.textureID) + TextureExtension;

										AddTexture(stream, textureName);

										if (!_TexturesAsPNGs.ContainsKey(textureName))
										{
											_TexturesAsPNGs.Add(textureName, textureFile.GetTextureAsBitmap(material.textureID));
										}
									}
								}

								SceneCDC.progressLevel++;
							}
						}
					}
					catch (FileNotFoundException)
					{
						Console.WriteLine("Error: couldn't find a texture file");
					}
					catch (Exception ex)
					{
						Console.Write(ex.ToString());
					}
				}
				else
				{
					try
					{
						SR1PSTextureFile textureFile = new SR1PSTextureFile(fileName);

						SceneCDC.progressLevel = 0;
						SceneCDC.progressLevels = 100; // Arbitrarily large number.
						SceneCDC.ProgressStage = "Loading Textures";

						// Trick it into showing a progress bar.
						System.Threading.Thread.Sleep(100);
						SceneCDC.progressLevel = 100;

						UInt32 polygonCountAllModels = 0;
						foreach (SRModel srModel in File.Models)
						{
							polygonCountAllModels += srModel.PolygonCount;
						}

						SR1PSTextureFile.SoulReaverPlaystationPolygonTextureData[] polygons =
							new SR1PSTextureFile.SoulReaverPlaystationPolygonTextureData[polygonCountAllModels];

						int polygonNum = 0;
						foreach (SRModel srModel in File.Models)
						{
							foreach (CDC.Polygon polygon in srModel.Polygons)
							{
								polygons[polygonNum].paletteColumn = polygon.paletteColumn;
								polygons[polygonNum].paletteRow = polygon.paletteRow;
								polygons[polygonNum].u = new int[3];
								polygons[polygonNum].v = new int[3];
								//polygons[polygonNum].materialColour = polygon.material.colour;
								polygons[polygonNum].materialColour = polygon.colour;

								polygons[polygonNum].u[0] = (int)(srModel.Geometry.UVs[polygon.v1.UVID].u * 255);
								polygons[polygonNum].v[0] = (int)(srModel.Geometry.UVs[polygon.v1.UVID].v * 255);
								polygons[polygonNum].u[1] = (int)(srModel.Geometry.UVs[polygon.v2.UVID].u * 255);
								polygons[polygonNum].v[1] = (int)(srModel.Geometry.UVs[polygon.v2.UVID].v * 255);
								polygons[polygonNum].u[2] = (int)(srModel.Geometry.UVs[polygon.v3.UVID].u * 255);
								polygons[polygonNum].v[2] = (int)(srModel.Geometry.UVs[polygon.v3.UVID].v * 255);

								polygons[polygonNum].textureID = polygon.material.textureID;
								polygons[polygonNum].CLUT = polygon.material.clutValue;

								polygons[polygonNum].textureUsed = polygon.material.textureUsed;
								polygons[polygonNum].visible = polygon.material.visible;
								//polygons[polygonNum].materialColour = polygon.material.colour;

								polygonNum++;
							}
						}
						bool drawGreyscaleFirst = false;
						bool quantizeBounds = true;
						textureFile.BuildTexturesFromPolygonData(polygons, drawGreyscaleFirst, quantizeBounds, ExportOptions);

						// For all models
						for (int t = 0; t < textureFile.TextureCount; t++)
						{
							String textureName = SRModel.GetPlayStationTextureNameDefault(File.Name, t) + TextureExtension;

							System.IO.MemoryStream stream = textureFile.GetDataAsStream(t);
							if (stream != null)
							{
								AddTexture(stream, textureName);
							}
							//string exportedTextureFileName = Path.ChangeExtension(textureName, "png");
							//_TexturesAsPNGs.Add(exportedTextureFileName, textureFile.GetTextureAsBitmap(t));
							Bitmap b = textureFile.GetTextureAsBitmap(t);
							// this is a hack that's being done here for now because I don't know for sure which of the flags/attributes controls
							// textures that should be alpha-masked. Alpha-masking EVERY texture is expensive.
							if (ExportOptions.AlsoInferAlphaMaskingFromTexturePixels)
							{
								if (BitmapHasTransparentPixels(b))
								{
									for (int modelNum = 0; modelNum < File.Models.Length; modelNum++)
									{
										if (t < File.Models[modelNum].Materials.Length)
										{
											File.Models[modelNum].Materials[t].UseAlphaMask = true;
										}
									}
								}
							}
							_TexturesAsPNGs.Add(textureName, b);

							// dump all textures as PNGs for debugging
							//Bitmap exportedTexture = textureFile.GetTextureAsBitmap(t);
							//string exportedTextureFileName = Path.ChangeExtension(textureName, "png");
							//exportedTexture.Save(exportedTextureFileName, ImageFormat.Png);
							//texNum = 0;
							//foreach (Bitmap tex in _Textures)
							//{
							//    tex.Save(@"C:\Debug\Tex-" + texNum + ".png", ImageFormat.Png);
							//    texNum++;
							//}

							SceneCDC.progressLevel++;
						}

						// for models that use index/CLUT textures, if the user has enabled this option
						if (ExportOptions.UseEachUniqueTextureCLUTVariation)
						{
							foreach (int textureID in textureFile.TexturesByCLUT.Keys)
							{
								Dictionary<ushort, Bitmap> textureCLUTCollection = textureFile.TexturesByCLUT[textureID];
								foreach (ushort clut in textureCLUTCollection.Keys)
								{
									String textureName = SRModel.GetPlayStationTextureNameWithCLUT(File.Name, textureID, clut) + TextureExtension;
									System.IO.MemoryStream stream = textureFile.GetTextureWithCLUTAsStream(textureID, clut);
									if (stream != null)
									{
										AddTexture(stream, textureName);
									}
									Bitmap b = textureFile.TexturesByCLUT[textureID][clut];
									_TexturesAsPNGs.Add(textureName, b);

									SceneCDC.progressLevel++;
								}
							}
						}
					}
					catch (FileNotFoundException)
					{
						Console.WriteLine("Error: couldn't find a texture file");
					}
					catch (Exception ex)
					{
						Console.Write(ex.ToString());
					}
				}
			}
			else
			{
				try
				{
					Gex3PSTextureFile textureFile = new Gex3PSTextureFile(fileName);

					UInt32 polygonCountAllModels = 0;
					foreach (SRModel srModel in File.Models)
					{
						polygonCountAllModels += srModel.PolygonCount;
					}

					Gex3PSTextureFile.Gex3PlaystationPolygonTextureData[] polygons =
						new Gex3PSTextureFile.Gex3PlaystationPolygonTextureData[polygonCountAllModels];

					int polygonNum = 0;
					foreach (SRModel srModel in File.Models)
					{
						foreach (CDC.Polygon polygon in srModel.Polygons)
						{
							polygons[polygonNum].paletteColumn = polygon.paletteColumn;
							polygons[polygonNum].paletteRow = polygon.paletteRow;
							polygons[polygonNum].u = new int[3];
							polygons[polygonNum].v = new int[3];
							//polygons[polygonNum].materialColour = polygon.material.colour;
							polygons[polygonNum].materialColour = polygon.colour;

							polygons[polygonNum].u[0] = (int)(srModel.Geometry.UVs[polygon.v1.UVID].u * 255);
							polygons[polygonNum].v[0] = (int)(srModel.Geometry.UVs[polygon.v1.UVID].v * 255);
							polygons[polygonNum].u[1] = (int)(srModel.Geometry.UVs[polygon.v2.UVID].u * 255);
							polygons[polygonNum].v[1] = (int)(srModel.Geometry.UVs[polygon.v2.UVID].v * 255);
							polygons[polygonNum].u[2] = (int)(srModel.Geometry.UVs[polygon.v3.UVID].u * 255);
							polygons[polygonNum].v[2] = (int)(srModel.Geometry.UVs[polygon.v3.UVID].v * 255);

							polygons[polygonNum].textureID = polygon.material.textureID;
							polygons[polygonNum].CLUT = polygon.material.clutValue;

							polygons[polygonNum].textureUsed = polygon.material.textureUsed;
							polygons[polygonNum].visible = polygon.material.visible;
							//polygons[polygonNum].materialColour = polygon.material.colour;

							polygonNum++;
						}
					}

					bool drawGreyscaleFirst = false;
					bool quantizeBounds = true;
					textureFile.BuildTexturesFromPolygonData(polygons, ((GexFile)File).TPages, drawGreyscaleFirst, quantizeBounds, ExportOptions);

					// For all models
					for (int t = 0; t < textureFile.TextureCount; t++)
					{
						String textureName = SRModel.GetPlayStationTextureNameDefault(File.Name, t) + TextureExtension;

						System.IO.MemoryStream stream = textureFile.GetDataAsStream(t);
						if (stream != null)
						{
							AddTexture(stream, textureName);
						}
						//string exportedTextureFileName = Path.ChangeExtension(textureName, "png");
						//_TexturesAsPNGs.Add(exportedTextureFileName, textureFile.GetTextureAsBitmap(t));
						Bitmap b = textureFile.GetTextureAsBitmap(t);
						// this is a hack that's being done here for now because I don't know for sure which of the flags/attributes controls
						// textures that should be alpha-masked. Alpha-masking EVERY texture is expensive.
						if (ExportOptions.AlsoInferAlphaMaskingFromTexturePixels)
						{
							if (BitmapHasTransparentPixels(b))
							{
								for (int modelNum = 0; modelNum < File.Models.Length; modelNum++)
								{
									if (t < File.Models[modelNum].Materials.Length)
									{
										File.Models[modelNum].Materials[t].UseAlphaMask = true;
									}
								}
							}
						}
						_TexturesAsPNGs.Add(textureName, b);

						// dump all textures as PNGs for debugging
						//Bitmap exportedTexture = textureFile.GetTextureAsBitmap(t);
						//string exportedTextureFileName = Path.ChangeExtension(textureName, "png");
						//exportedTexture.Save(exportedTextureFileName, ImageFormat.Png);
						//texNum = 0;
						//foreach (Bitmap tex in _Textures)
						//{
						//    tex.Save(@"C:\Debug\Tex-" + texNum + ".png", ImageFormat.Png);
						//    texNum++;
						//}
					}

					// for models that use index/CLUT textures, if the user has enabled this option
					if (ExportOptions.UseEachUniqueTextureCLUTVariation)
					{
						foreach (int textureID in textureFile.TexturesByCLUT.Keys)
						{
							Dictionary<ushort, Bitmap> textureCLUTCollection = textureFile.TexturesByCLUT[textureID];
							foreach (ushort clut in textureCLUTCollection.Keys)
							{
								String textureName = SRModel.GetPlayStationTextureNameWithCLUT(File.Name, textureID, clut) + TextureExtension;
								System.IO.MemoryStream stream = textureFile.GetTextureWithCLUTAsStream(textureID, clut);
								if (stream != null)
								{
									AddTexture(stream, textureName);
								}
								Bitmap b = textureFile.TexturesByCLUT[textureID][clut];
								_TexturesAsPNGs.Add(textureName, b);
							}
						}
					}
				}
				catch (FileNotFoundException)
				{
					Console.WriteLine("Error: couldn't find a texture file");
				}
				catch (Exception ex)
				{
					Console.Write(ex.ToString());
				}
			}
		}
		
		// make sure that overwriting exported files isn't silently failing
		protected void DeleteExistingFile(string path)
		{
			if (System.IO.File.Exists(path))
			{
				try
				{
					System.IO.File.Delete(path);
				}
				catch (Exception ex)
				{
					Console.WriteLine(string.Format("Couldn't delete existing file '{0}': {1}", path, ex.Message));
				}
			}
		}

		public void ExportToFile(string fileName)
		{
			string filePath = Path.GetFullPath(fileName);
			DeleteExistingFile(filePath);
			File.ExportToFile(fileName, ExportOptions);
			string baseExportDirectory = Path.GetDirectoryName(fileName);
			foreach (string textureFileName in _TexturesAsPNGs.Keys)
			{
				string texturePath = Path.Combine(baseExportDirectory, textureFileName);
				DeleteExistingFile(texturePath);
				_TexturesAsPNGs[textureFileName].Save(texturePath, ImageFormat.Png);
			}
		}

		protected static bool BitmapHasTransparentPixels(Bitmap b)
		{
			for (int y = 0; y < b.Height; y++)
			{
				for (int x = 0; x < b.Width; x++)
				{
					Color c = b.GetPixel(x, y);
					if (c.A == 0)
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
