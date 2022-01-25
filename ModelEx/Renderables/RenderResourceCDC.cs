using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
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
		private SRFile _srFile;

		public const string TextureExtension = ".png";

		public RenderResourceCDC(SRFile srFile)
			: base(srFile.Name)
		{
			_srFile = srFile;
		}

		public void LoadModels(CDC.Objects.ExportOptions options)
		{
			foreach (Model model in Models)
			{
				model.Dispose();
			}

			for (int modelIndex = 0; modelIndex < _srFile.Models.Length; modelIndex++)
			{
				SRModelParser modelParser = new SRModelParser(_srFile.Name, _srFile);
				modelParser.BuildModel(this, modelIndex, options);
				if (_srFile.Asset == CDC.Asset.Unit)
				{
					Models.Add(modelParser.Model);
				}
				else
				{
					Models.Add(modelParser.Model);
				}
			}
		}

		public void LoadTextures(string fileName, CDC.Objects.ExportOptions options)
		{
			FileShaderResourceViewDictionary.Clear();

			Type currentFileType = _srFile.GetType();
			if (currentFileType == typeof(TRLFile))
			{
				String textureFileName = fileName;
				try
				{
					TRLPCTextureFile textureFile = new TRLPCTextureFile(textureFileName);

					SceneCDC.progressLevel = 0;
					SceneCDC.progressLevels = textureFile.TextureCount;
					SceneCDC.ProgressStage = "Loading Textures";

					for (int t = 0; t < textureFile.TextureCount; t++)
					{
						String textureName = CDC.Objects.Models.SRModel.GetPS2TextureName(_srFile.Name, (int)textureFile.TextureDefinitions[t].ID) + TextureExtension;

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
				String textureFileName = System.IO.Path.ChangeExtension(fileName, "vrm");
				try
				{
					SR2PCTextureFile textureFile = new SR2PCTextureFile(textureFileName);

					SceneCDC.progressLevel = 0;
					SceneCDC.progressLevels = textureFile.TextureCount;
					SceneCDC.ProgressStage = "Loading Textures";

					for (int t = 0; t < textureFile.TextureCount; t++)
					{
						String textureName = CDC.Objects.Models.SRModel.GetPS2TextureName(_srFile.Name, textureFile.TextureDefinitions[t].Flags1) + TextureExtension;

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
				if (_srFile.Platform == CDC.Platform.PC)
				{
					try
					{
						String textureFileName = GetTextureFileLocation(options, "textures.big", fileName);

						SR1PCTextureFile textureFile = new SR1PCTextureFile(textureFileName);

						SceneCDC.progressLevel = 0;
						SceneCDC.progressLevels = _srFile.GetNumMaterials();
						SceneCDC.ProgressStage = "Loading Textures";

						foreach (SRModel srModel in _srFile.Models)
						{
							foreach (CDC.Material material in srModel.Materials)
							{
								if (material.textureUsed)
								{
									System.IO.MemoryStream stream = textureFile.GetDataAsStream(material.textureID);
									if (stream != null)
									{
										String textureName = CDC.Objects.Models.SRModel.GetSoulReaverPCOrDreamcastTextureName(srModel.Name, material.textureID) + TextureExtension;
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
				else if (_srFile.Platform == CDC.Platform.Dreamcast)
				{
					try
					{
						String textureFileName = GetTextureFileLocation(options, "textures.vq", fileName);		

						SR1DCTextureFile textureFile = new SR1DCTextureFile(textureFileName);

						SceneCDC.progressLevel = 0;
						SceneCDC.progressLevels = _srFile.GetNumMaterials();
						SceneCDC.ProgressStage = "Loading Textures";

						foreach (SRModel srModel in _srFile.Models)
						{
							foreach (CDC.Material material in srModel.Materials)
							{
								if (material.textureUsed)
								{
									int textureID = material.textureID;
									System.IO.MemoryStream stream = textureFile.GetDataAsStream(textureID);
									if (stream != null)
									{
										String textureName = CDC.Objects.Models.SRModel.GetSoulReaverPCOrDreamcastTextureName(srModel.Name, material.textureID) + TextureExtension;

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
					String textureFileName = System.IO.Path.ChangeExtension(fileName, "crm");
					try
					{
						SR1PSTextureFile textureFile = new SR1PSTextureFile(textureFileName);

						SceneCDC.progressLevel = 0;
						SceneCDC.progressLevels = 100; // Arbitrarily large number.
						SceneCDC.ProgressStage = "Loading Textures";

						// Trick it into showing a progress bar.
						System.Threading.Thread.Sleep(100);
						SceneCDC.progressLevel = 100;

						UInt32 polygonCountAllModels = 0;
						foreach (SRModel srModel in _srFile.Models)
						{
							polygonCountAllModels += srModel.PolygonCount;
						}

						SR1PSTextureFile.SoulReaverPlaystationPolygonTextureData[] polygons =
							new SR1PSTextureFile.SoulReaverPlaystationPolygonTextureData[polygonCountAllModels];

						int polygonNum = 0;
						foreach (SRModel srModel in _srFile.Models)
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
						textureFile.BuildTexturesFromPolygonData(polygons, drawGreyscaleFirst, quantizeBounds, options);

						// For all models
						for (int t = 0; t < textureFile.TextureCount; t++)
						{
							String textureName = CDC.Objects.Models.SRModel.GetPlayStationTextureNameDefault(_srFile.Name, t) + TextureExtension;

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
							if (options.AlsoInferAlphaMaskingFromTexturePixels)
							{
								if (BitmapHasTransparentPixels(b))
								{
									for (int modelNum = 0; modelNum < _srFile.Models.Length; modelNum++)
									{
										if (t < _srFile.Models[modelNum].Materials.Length)
										{
											_srFile.Models[modelNum].Materials[t].UseAlphaMask = true;
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
						if (options.UseEachUniqueTextureCLUTVariation)
						{
							foreach (int textureID in textureFile.TexturesByCLUT.Keys)
							{
								Dictionary<ushort, Bitmap> textureCLUTCollection = textureFile.TexturesByCLUT[textureID];
								foreach (ushort clut in textureCLUTCollection.Keys)
								{
									String textureName = CDC.Objects.Models.SRModel.GetPlayStationTextureNameWithCLUT(_srFile.Name, textureID, clut) + TextureExtension;
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
					String textureFileName = System.IO.Path.ChangeExtension(fileName, "vrm");

					Gex3PSTextureFile textureFile = new Gex3PSTextureFile(textureFileName);

					UInt32 polygonCountAllModels = 0;
					foreach (SRModel srModel in _srFile.Models)
					{
						polygonCountAllModels += srModel.PolygonCount;
					}

					Gex3PSTextureFile.Gex3PlaystationPolygonTextureData[] polygons =
						new Gex3PSTextureFile.Gex3PlaystationPolygonTextureData[polygonCountAllModels];

					int polygonNum = 0;
					foreach (SRModel srModel in _srFile.Models)
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
					textureFile.BuildTexturesFromPolygonData(polygons, ((GexFile)_srFile).TPages, drawGreyscaleFirst, quantizeBounds, options);

					// For all models
					for (int t = 0; t < textureFile.TextureCount; t++)
					{
						String textureName = CDC.Objects.Models.SRModel.GetPlayStationTextureNameDefault(_srFile.Name, t) + TextureExtension;

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
						if (options.AlsoInferAlphaMaskingFromTexturePixels)
						{
							if (BitmapHasTransparentPixels(b))
							{
								for (int modelNum = 0; modelNum < _srFile.Models.Length; modelNum++)
								{
									if (t < _srFile.Models[modelNum].Materials.Length)
									{
										_srFile.Models[modelNum].Materials[t].UseAlphaMask = true;
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
					if (options.UseEachUniqueTextureCLUTVariation)
					{
						foreach (int textureID in textureFile.TexturesByCLUT.Keys)
						{
							Dictionary<ushort, Bitmap> textureCLUTCollection = textureFile.TexturesByCLUT[textureID];
							foreach (ushort clut in textureCLUTCollection.Keys)
							{
								String textureName = CDC.Objects.Models.SRModel.GetPlayStationTextureNameWithCLUT(_srFile.Name, textureID, clut) + TextureExtension;
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
			if (File.Exists(path))
			{
				try
				{
					File.Delete(path);
				}
				catch (Exception ex)
				{
					Console.WriteLine(string.Format("Couldn't delete existing file '{0}': {1}", path, ex.Message));
				}
			}
		}

		public void ExportToFile(string fileName, CDC.Objects.ExportOptions options)
		{
			string filePath = Path.GetFullPath(fileName);
			DeleteExistingFile(filePath);
			_srFile.ExportToFile(fileName, options);
			string baseExportDirectory = Path.GetDirectoryName(fileName);
			foreach (string textureFileName in _TexturesAsPNGs.Keys)
			{
				string texturePath = Path.Combine(baseExportDirectory, textureFileName);
				DeleteExistingFile(texturePath);
				_TexturesAsPNGs[textureFileName].Save(texturePath, ImageFormat.Png);
			}
		}

		protected string GetTextureFileLocation(CDC.Objects.ExportOptions options, string defaultTextureFileName, string modelFileName)
		{
			string result = "";
			List<string> possibleLocations = new List<string>();
			for (int i = 0; i < options.TextureFileLocations.Count; i++)
			{
				possibleLocations.Add(options.TextureFileLocations[i]);
			}

			List<string> searchDirectories = new List<string>();

			string rootDirectory = Path.GetDirectoryName(modelFileName);
			while (rootDirectory != null && rootDirectory != "")
			{
				string parentDirectory = Path.GetFileName(rootDirectory);
				rootDirectory = Path.GetDirectoryName(rootDirectory);
				if (parentDirectory == "kain2")
				{
					string outputDirectory = Path.Combine(rootDirectory, "output");
					searchDirectories.Add(outputDirectory);
					searchDirectories.Add(rootDirectory);
				}
			}

			searchDirectories.Add(Path.GetDirectoryName(modelFileName));
			searchDirectories.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

			for (int i = 0; i < searchDirectories.Count; i++)
			{
				string textureFileName = Path.Combine(searchDirectories[i], defaultTextureFileName);
				possibleLocations.Add(textureFileName);
			}

			for (int i = 0; i < possibleLocations.Count; i++)
			{
				if (File.Exists(possibleLocations[i]))
				{
					result = possibleLocations[i];
					Console.WriteLine(string.Format("Debug: using texture file '{0}'", result));
					break;
				}
			}
			return result;
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
