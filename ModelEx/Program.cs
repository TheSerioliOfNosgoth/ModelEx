using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SlimDX.Windows;

namespace ModelEx
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			bool guiMode = true;
			if (args.Length > 0)
			{
				guiMode = false;
			}

			if (guiMode)
			{
				MainWindow form = new MainWindow();
				MessagePump.Run(form, () => { });
			}
			else
			{
				string inputFilePath = "";
				try
				{
					string outputFilePath = "";
					string mode = "sr1";
					CDC.ExportOptions options = new CDC.ExportOptions();
					CDC.Platform platform = CDC.Platform.None;

					ArrayList arguments = new ArrayList();
					for (int i = 0; i < args.Length; i++)
					{
						// if the argument is one with a second component, catch that now
						string nextArg = "";
						switch (args[i])
						{
							case "--input":
								nextArg = args[i + 1];
								inputFilePath = Path.GetFullPath(nextArg);
								i++;
								break;
							case "--output":
								nextArg = args[i + 1];
								outputFilePath = Path.GetFullPath(nextArg);
								i++;
								break;
							case "--texture-file":
								nextArg = args[i + 1];
								options.TextureFileLocations.Add(Path.GetFullPath(nextArg));
								i++;
								break;
							case "--polygon-colour":
								nextArg = args[i + 1];
								string[] nextArgArr = nextArg.Split(new string[] { "," }, StringSplitOptions.None);
								bool validColour = true;
								if (nextArgArr.Length != 4)
								{
									validColour = false;
								}
								else
								{
									float alpha = 0.0f;
									float red = 0.0f;
									float green = 0.0f;
									float blue = 0.0f;
									try
									{
										alpha = float.Parse(nextArgArr[0].Trim());
										red = float.Parse(nextArgArr[1].Trim());
										green = float.Parse(nextArgArr[2].Trim());
										blue = float.Parse(nextArgArr[3].Trim());
										options.PolygonColourAlpha = alpha;
										options.PolygonColourRed = red;
										options.PolygonColourGreen = green;
										options.PolygonColourBlue = blue;
									}
									catch (Exception ex)
									{
										validColour = false;
									}
								}
								if (!validColour)
								{
									Console.WriteLine(string.Format("Couldn't parse '{0}' as a floating-point ARGB colour (e.g. \"0.7, 0.0, 1.0, 0.0\" for translucent green)", nextArg));
									Environment.Exit(1);
								}
								i++;
								break;
							case "--platform":
								nextArg = args[i + 1].ToLower();
								switch (nextArg)
								{
									case "auto":
									case "autodetect":
										platform = CDC.Platform.None;
										break;
									case "playstation":
									case "psx":
										platform = CDC.Platform.PSX;
										break;
									case "playstation2":
									case "ps2":
										platform = CDC.Platform.PlayStation2;
										break;
									case "pc":
									case "windows":
										platform = CDC.Platform.PC;
										break;
									case "dc":
									case "dreamcast":
										platform = CDC.Platform.Dreamcast;
										break;
									case "xbox":
										platform = CDC.Platform.Xbox;
										break;
								}
								i++;
								break;
							case "--rendermode":
								nextArg = args[i + 1].ToLower();
								switch (nextArg)
								{
									case "standard":
										options.RenderMode = CDC.RenderMode.Standard;
										break;
									case "wireframe":
										options.RenderMode = CDC.RenderMode.Wireframe;
										break;
									case "notextures":
										options.RenderMode = CDC.RenderMode.NoTextures;
										break;
									case "polyflags1":
										options.RenderMode = CDC.RenderMode.DebugPolygonFlags1;
										break;
									case "polyflags2":
										options.RenderMode = CDC.RenderMode.DebugPolygonFlags2;
										break;
									case "polyflags3":
										options.RenderMode = CDC.RenderMode.DebugPolygonFlags3;
										break;
									case "polyflagshash":
										options.RenderMode = CDC.RenderMode.DebugPolygonFlagsHash;
										break;
									case "textureattr1":
										options.RenderMode = CDC.RenderMode.DebugTextureAttributes1;
										break;
									case "textureattr2":
										options.RenderMode = CDC.RenderMode.DebugTextureAttributes2;
										break;
									case "textureattr3":
										options.RenderMode = CDC.RenderMode.DebugTextureAttributes3;
										break;
									case "textureattr4":
										options.RenderMode = CDC.RenderMode.DebugTextureAttributes4;
										break;
									case "textureattr5":
										options.RenderMode = CDC.RenderMode.DebugTextureAttributes5;
										break;
									case "textureattr6":
										options.RenderMode = CDC.RenderMode.DebugTextureAttributes6;
										break;
									case "textureattrhash":
										options.RenderMode = CDC.RenderMode.DebugTextureAttributesHash;
										break;
									case "textureattra1":
										options.RenderMode = CDC.RenderMode.DebugTextureAttributesA1;
										break;
									case "textureattra2":
										options.RenderMode = CDC.RenderMode.DebugTextureAttributesA2;
										break;
									case "textureattra3":
										options.RenderMode = CDC.RenderMode.DebugTextureAttributesA3;
										break;
									case "textureattra4":
										options.RenderMode = CDC.RenderMode.DebugTextureAttributesA4;
										break;
									case "textureattra5":
										options.RenderMode = CDC.RenderMode.DebugTextureAttributesA5;
										break;
									case "textureattra6":
										options.RenderMode = CDC.RenderMode.DebugTextureAttributesA6;
										break;
									case "textureattrahash":
										options.RenderMode = CDC.RenderMode.DebugTextureAttributesAHash;
										break;
									case "clut1":
										options.RenderMode = CDC.RenderMode.DebugTexturePage1;
										break;
									case "clut2":
										options.RenderMode = CDC.RenderMode.DebugTexturePage2;
										break;
									case "clut3":
										options.RenderMode = CDC.RenderMode.DebugTexturePage3;
										break;
									case "clut4":
										options.RenderMode = CDC.RenderMode.DebugTexturePage4;
										break;
									case "clut5":
										options.RenderMode = CDC.RenderMode.DebugTexturePage5;
										break;
									case "clut6":
										options.RenderMode = CDC.RenderMode.DebugTexturePage6;
										break;
									case "cluthash":
										options.RenderMode = CDC.RenderMode.DebugTexturePageHash;
										break;
									case "clutnrcb1":
										options.RenderMode = CDC.RenderMode.DebugCLUTNonRowColBits1;
										break;
									case "clutnrcb2":
										options.RenderMode = CDC.RenderMode.DebugCLUTNonRowColBits2;
										break;
									case "clutnrcbhash":
										options.RenderMode = CDC.RenderMode.DebugCLUTNonRowColBitsHash;
										break;
									case "texturepage1":
										options.RenderMode = CDC.RenderMode.DebugTexturePage1;
										break;
									case "texturepage2":
										options.RenderMode = CDC.RenderMode.DebugTexturePage2;
										break;
									case "texturepage3":
										options.RenderMode = CDC.RenderMode.DebugTexturePage3;
										break;
									case "texturepage4":
										options.RenderMode = CDC.RenderMode.DebugTexturePage4;
										break;
									case "texturepage5":
										options.RenderMode = CDC.RenderMode.DebugTexturePage5;
										break;
									case "texturepage6":
										options.RenderMode = CDC.RenderMode.DebugTexturePage6;
										break;
									case "texturepagehash":
										options.RenderMode = CDC.RenderMode.DebugTexturePageHash;
										break;
									case "texturepageu28hash":
										options.RenderMode = CDC.RenderMode.DebugTexturePageUpper28BitsHash;
										break;
									case "texturepageu5hash":
										options.RenderMode = CDC.RenderMode.DebugTexturePageUpper5BitsHash;
										break;
									case "bsptreenum":
										options.RenderMode = CDC.RenderMode.DebugBSPRootTreeNumber;
										break;
									case "bsptreenodeid":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeNodeID;
										break;
									case "bsproot1":
										options.RenderMode = CDC.RenderMode.DebugBSPRootTreeFlags1;
										break;
									case "bsproot2":
										options.RenderMode = CDC.RenderMode.DebugBSPRootTreeFlags2;
										break;
									case "bsproot3":
										options.RenderMode = CDC.RenderMode.DebugBSPRootTreeFlags3;
										break;
									case "bsproot4":
										options.RenderMode = CDC.RenderMode.DebugBSPRootTreeFlags4;
										break;
									case "bsproot5":
										options.RenderMode = CDC.RenderMode.DebugBSPRootTreeFlags5;
										break;
									case "bsproot6":
										options.RenderMode = CDC.RenderMode.DebugBSPRootTreeFlags6;
										break;
									case "bsproothash":
										options.RenderMode = CDC.RenderMode.DebugBSPRootTreeFlagsHash;
										break;
									case "bspnode1":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeImmediateParentFlags1;
										break;
									case "bspnode2":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeImmediateParentFlags2;
										break;
									case "bspnode3":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeImmediateParentFlags3;
										break;
									case "bspnode4":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeImmediateParentFlags4;
										break;
									case "bspnode5":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeImmediateParentFlags5;
										break;
									case "bspnode6":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeImmediateParentFlags6;
										break;
									case "bspnodehash":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeImmediateParentFlagsHash;
										break;
									case "bspor1":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeAllParentFlagsORd1;
										break;
									case "bspor2":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeAllParentFlagsORd2;
										break;
									case "bspor3":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeAllParentFlagsORd3;
										break;
									case "bspor4":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeAllParentFlagsORd4;
										break;
									case "bspor5":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeAllParentFlagsORd5;
										break;
									case "bspor6":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeAllParentFlagsORd6;
										break;
									case "bsporhash":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeAllParentFlagsORdHash;
										break;
									case "bspleaf1":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeLeafFlags1;
										break;
									case "bspleaf2":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeLeafFlags2;
										break;
									case "bspleaf3":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeLeafFlags3;
										break;
									case "bspleaf4":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeLeafFlags4;
										break;
									case "bspleaf5":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeLeafFlags5;
										break;
									case "bspleaf6":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeLeafFlags6;
										break;
									case "bspleafhash":
										options.RenderMode = CDC.RenderMode.DebugBSPTreeLeafFlagsHash;
										break;
									case "boneidhash":
										options.RenderMode = CDC.RenderMode.DebugBoneIDHash;
										break;
									case "sortpushhash":
										options.RenderMode = CDC.RenderMode.DebugSortPushHash;
										break;
									case "sortpushflags1":
										options.RenderMode = CDC.RenderMode.DebugSortPushFlags1;
										break;
									case "sortpushflags2":
										options.RenderMode = CDC.RenderMode.DebugSortPushFlags2;
										break;
									case "sortpushflags3":
										options.RenderMode = CDC.RenderMode.DebugSortPushFlags3;
										break;
									case "polyalpha":
										options.RenderMode = CDC.RenderMode.PolygonAlpha;
										break;
									case "polyopacity":
										options.RenderMode = CDC.RenderMode.PolygonOpacity;
										break;
									case "vertexalpha":
										options.RenderMode = CDC.RenderMode.AverageVertexAlpha;
										break;
								}
								i++;
								break;
							default:
								arguments.Add(args[i]);
								break;
						}
					}

					ArrayList positionalArgs = new ArrayList();

					foreach (string arg in arguments)
					{
						bool handled = false;
						if ((arg == "-?") || (arg == "-h") || (arg == "--help"))
						{
							Console.WriteLine("Run ModelEx with no arguments to access the GUI mode. Use --input INPUT_FILE and --output OUTPUT_FILE to convert in batch mode");
							Environment.Exit(0);
						}
						if (arg == "--sr1")
						{
							mode = "sr1";
							handled = true;
						}
						if (arg == "--sr2")
						{
							mode = "sr2";
							handled = true;
						}
						if (arg == "--defiance")
						{
							mode = "defiance";
							handled = true;
						}
						if (arg == "--trl")
						{
							mode = "trl";
							handled = true;
						}
						if (arg == "--discard-portal-polygons")
						{
							options.DiscardPortalPolygons = true;
							handled = true;
						}
						if (arg == "--discard-meshes-without-nonzero-flags")
						{
							options.DiscardMeshesWithNoNonZeroFlags = true;
							handled = true;
						}
						if (arg == "--discard-non-visible")
						{
							options.DiscardNonVisible = true;
							handled = true;
						}
						if (arg == "--spectral")
						{
							options.ExportSpectral = true;
							handled = true;
						}
						if (arg == "--unhide-invisible-textures")
						{
							options.UnhideCompletelyTransparentTextures = true;
							handled = true;
						}
						if (arg == "--missing-palettes-in-greyscale")
						{
							options.AlwaysUseGreyscaleForMissingPalettes = true;
							handled = true;
						}
						if (arg == "--double-sided")
						{
							options.ExportDoubleSidedMaterials = true;
							handled = true;
						}
						if (arg == "--bsp-or-tree-root")
						{
							options.BSPRenderingIncludeRootTreeFlagsWhenORing = true;
							handled = true;
						}
						if (arg == "--bsp-or-leaf")
						{
							options.BSPRenderingIncludeLeafFlagsWhenORing = true;
							handled = true;
						}
						if (arg == "--interpolate-poly-colours")
						{
							options.InterpolatePolygonColoursWhenColouringBasedOnVertices = true;
							handled = true;
						}
						if (arg == "--all-cluts")
						{
							options.UseEachUniqueTextureCLUTVariation = true;
							handled = true;
						}
						if (arg == "--materials-for-all-flags")
						{
							options.DistinctMaterialsForAllFlags = true;
							handled = true;
						}
						if (arg == "--ignore-backfacing-flag-for-terrain")
						{
							options.IgnoreBackfacingFlagForTerrain = true;
							handled = true;
						}
						if (arg == "--infer-alphamask")
						{
							options.AlsoInferAlphaMaskingFromTexturePixels = true;
							handled = true;
						}
						if (arg == "--adjust-uvs")
						{
							options.AdjustUVs = true;
							handled = true;
						}
						if (arg == "--set-polygon-colour")
						{
							options.SetAllPolygonColoursToValue = true;
							handled = true;
						}
						if (arg == "--ignore-vertex-colours")
						{
							options.IgnoreVertexColours = true;
							handled = true;
						}
						if (!handled)
						{
							positionalArgs.Add(arg);
						}
					}

					if (inputFilePath == "")
					{
						Console.WriteLine("--input path is required");
						Environment.Exit(1);
					}

					if (outputFilePath == "")
					{
						Console.WriteLine("--output path is required");
						Environment.Exit(1);
					}

					RenderControl sceneView = new RenderControl();
					sceneView.Initialize();

					LoadRequestCDC loadRequest = new LoadRequestCDC();

					Thread loadingThread = new Thread((() =>
					{
						RenderManager.Instance.UnloadResources();

						loadRequest.DataFile = inputFilePath;
						loadRequest.ExportOptions = options;
						if (mode == "gex") loadRequest.GameType = CDC.Game.Gex;
						else if (mode == "sr1") loadRequest.GameType = CDC.Game.SR1;
						else if (mode == "sr2") loadRequest.GameType = CDC.Game.SR2;
						else if (mode == "defiance") loadRequest.GameType = CDC.Game.Defiance;
						else if (mode == "trl") loadRequest.GameType = CDC.Game.TRL;

						if (loadRequest.GameType == CDC.Game.SR1)
						{
							CDC.SR1File sr1File = new CDC.SR1File(inputFilePath, platform, options);

							if (sr1File.Platform == CDC.Platform.PC)
							{
								loadRequest.TextureFile = CDC.Utility.GetTextureFileLocation(options, "textures.big", inputFilePath);
							}
							else if (sr1File.Platform == CDC.Platform.Dreamcast)
							{
								loadRequest.TextureFile = CDC.Utility.GetTextureFileLocation(options, "textures.vq", inputFilePath);
							}
							else
							{
								loadRequest.TextureFile = Path.ChangeExtension(inputFilePath, "crm");
							}
						}
						else if (loadRequest.GameType == CDC.Game.Gex ||
							loadRequest.GameType == CDC.Game.SR2 || loadRequest.GameType == CDC.Game.Defiance)
						{
							loadRequest.TextureFile = Path.ChangeExtension(inputFilePath, "vrm");
						}
						else
						{
							loadRequest.TextureFile = inputFilePath;
						}

						if (loadRequest.GameType == CDC.Game.TRL || loadRequest.GameType == CDC.Game.TRA)
						{
							//
						}
						else
						{
							loadRequest.ObjectListFile = inputFilePath;
						}

						LoadResourceFlags loadResourceFlags = LoadResourceFlags.ReloadScene | LoadResourceFlags.LoadDebugResource;
						RenderManager.Instance.LoadResourceCDC(loadRequest, loadResourceFlags);
					}));

					loadingThread.Name = "LoadingThread";
					loadingThread.SetApartmentState(ApartmentState.STA);
					loadingThread.Start();

					do
					{
						lock (SceneCDC.ProgressStage)
						{
							Console.WriteLine(string.Format("{0}: {1}%", SceneCDC.ProgressStage, (float)(SceneCDC.ProgressPercent) / 100.0));
						}
						Thread.Sleep(1000);
					}
					while (loadingThread.IsAlive);
					Console.WriteLine("Done loading");

					RenderManager.Instance.ExportResourceCDC(loadRequest.ResourceName, outputFilePath);

					Console.WriteLine("Done exporting");

					sceneView.ShutDown();
					loadingThread.Abort();
					sceneView.Dispose();
					Console.WriteLine("Done");
				}
				catch (Exception ex)
				{
					Console.WriteLine(string.Format("Error processing '{0}': {1}", inputFilePath, ex.Message));
					Console.WriteLine(ex.StackTrace);
					Environment.Exit(1);
				}
			}
			return;
		}
	}
}