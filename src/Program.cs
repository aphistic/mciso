using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text;
using LibMinecraft;

namespace mciso
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CommandConfig.LoadConfig(args);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                Console.WriteLine(CommandConfig.Usage());
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unknown exception occurred: {0}", ex.Message);
                Console.WriteLine(ex.StackTrace);
                return;
            }

            try
            {
                CommandConfig.ValidateConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                Console.WriteLine(CommandConfig.Usage());
                return;
            }

            if (!File.Exists(CommandConfig.MapFile))
            {
                Console.WriteLine("Unable to load level: {0}", CommandConfig.MapFile);
                return;
            }

            McLevel mcLevel = new McLevel(CommandConfig.MapFile);
            mcLevel.LoadFile();

            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            if (CommandConfig.Type == CommandConfig.RenderType.Overview)
            {
                string tileset = string.Format("images.overview{0}.png", CommandConfig.UseHighQuality
                                                                             ? "hq"
                                                                             : CommandConfig.UseLowQuality ? "lq" : "");
                Stream tilesetStream = currentAssembly.GetManifestResourceStream(tileset);
                if (tilesetStream == null)
                {
                    Console.WriteLine("Unable to load overview tileset.");
                    return;
                }
                Image imgMini = Image.FromStream(tilesetStream);
                List<Bitmap> imagesMini = new List<Bitmap>();
                int miniWidth = imgMini.Width;
                int miniHeight = imgMini.Height;
                for (int idx = 0; idx < miniWidth/miniHeight; idx++)
                {
                    Bitmap bmpMini = new Bitmap(miniHeight, miniHeight);
                    using (Graphics gMini = Graphics.FromImage(bmpMini))
                    {
                        gMini.DrawImage(imgMini, new Rectangle(0, 0, miniHeight, miniHeight),
                                        new Rectangle(idx*miniHeight, 0, miniHeight, miniHeight),
                                        GraphicsUnit.Pixel);
                    }
                    imagesMini.Add(bmpMini);
                }

                List<int> layers;
                if (CommandConfig.UseRenderLayers)
                {
	                if (CommandConfig.RenderLayers.Count < 1 &&
					    CommandConfig.RenderInterval > 0)
					{
						// Use an interval to render.
						layers = new List<int>();
						for (int l = 0; l <= mcLevel.Map.Height; l += CommandConfig.RenderInterval)
						{
							layers.Add(l);
						}
					}
					else
					{
	                	layers = new List<int>(CommandConfig.RenderLayers);
					}
                }
                else
                {
                    layers = new List<int>();
                    for (int l = 0; l <= mcLevel.Map.Height; l++)
                    {
                        layers.Add(l);
                    }
                }

                for (int layerIdx = 0; layerIdx < layers.Count; layerIdx++)
                {
                    int y = layers[layerIdx];
                    if (y < mcLevel.Map.Height)
                    {
                        using (Bitmap bmpLayer = new Bitmap(mcLevel.Map.Width, mcLevel.Map.Length))
                        {
                            using (Graphics gLayer = Graphics.FromImage(bmpLayer))
                            {
                                for (int z = 0; z < mcLevel.Map.Length; z++)
                                {
                                    for (int x = 0; x < mcLevel.Map.Width; x++)
                                    {
                                        int block = mcLevel.Map.Blocks[x, y, z];
                                        if (block > 0 && block < imagesMini.Count)
                                        {
                                            gLayer.DrawImageUnscaled(imagesMini[block], x, z, miniHeight, miniHeight);
                                        }
                                    }
                                }
                            }
							if (!Directory.Exists(CommandConfig.Directory))
							{
								Directory.CreateDirectory(CommandConfig.Directory);
							}
                            bmpLayer.Save(Path.Combine(CommandConfig.Directory, string.Format("{0}.png", y)),
                                          ImageFormat.Png);
                        }
                    }
                }
                for (int idx = 0; idx < imagesMini.Count; idx++)
                {
                    imagesMini[idx].Dispose();
                }

                Console.WriteLine("Finished writing layers.");
            }
            else if (CommandConfig.Type == CommandConfig.RenderType.Isometric)
            {
                string tileset = string.Format("images.isometric{0}.png", CommandConfig.UseHighQuality
                                                                             ? "hq"
                                                                             : CommandConfig.UseLowQuality ? "lq" : "");
                Stream tilesetStream = currentAssembly.GetManifestResourceStream(tileset);
                if (tilesetStream == null)
                {
                    Console.WriteLine("Unable to load isometric tileset.");
                    return;
                }
                Image imgOrig = Image.FromStream(tilesetStream);
                List<Bitmap> images = new List<Bitmap>();
                int imgWidth = imgOrig.Width;
                int imgHeight = imgOrig.Height;
                for (int idx = 0; idx < imgWidth/imgHeight; idx++)
                {
                    Bitmap bmpBlock = new Bitmap(imgHeight, imgHeight);
                    using (Graphics gBlock = Graphics.FromImage(bmpBlock))
                    {
                        gBlock.DrawImage(imgOrig, new Rectangle(0, 0, imgHeight, imgHeight),
                                         new Rectangle(idx*imgHeight, 0, imgHeight, imgHeight),
                                         GraphicsUnit.Pixel);
                    }
                    images.Add(bmpBlock);
                }

                int bmpWidth = imgHeight*mcLevel.Map.Width;
                int bmpHeight = (int) (bmpWidth*.75);
                using (Bitmap bmp = new Bitmap(bmpWidth + (imgHeight*3), bmpHeight + (imgHeight*3)))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {

                        List<int> layers;
                        if (CommandConfig.UseRenderLayers)
                        {
							if (CommandConfig.RenderLayers.Count < 1 &&
							    CommandConfig.RenderInterval > 0)
							{
								// Use an interval to render.
								layers = new List<int>();
								for (int l = 0; l <= mcLevel.Map.Height; l += CommandConfig.RenderInterval)
								{
									layers.Add(l);
								}
							}
							else
							{
                            	layers = new List<int>(CommandConfig.RenderLayers);
							}
                        }
                        else
                        {
                            layers = new List<int>();
                            for (int l = 0; l <= mcLevel.Map.Height; l++)
                            {
                                layers.Add(l);
                            }
                        }

                        for (int layerIdx = 0; layerIdx < layers.Count; layerIdx++)
                        {
                            int y = layers[layerIdx];
                            if (y < mcLevel.Map.Height)
                            {
                                for (int x = 0; x < mcLevel.Map.Length; x++)
                                {
                                    for (int z = 0; z < mcLevel.Map.Width; z++)
                                    {
                                        int block = mcLevel.Map.Blocks[x, y, z];
                                        if (block > 0 && block < images.Count)
                                        {
                                            try
                                            {
                                                g.DrawImageUnscaled(images[block],
                                                                    (x*(imgHeight/2)) + (bmpWidth/2) - (z*(imgHeight/2)),
                                                                    (z*(imgHeight/4)) + (x*(imgHeight/4)) -
                                                                    (y*(imgHeight/2)) + (bmpHeight/4));
                                            }
                                            catch (Exception)
                                            {
                                                // More than likely a new type of block was found that hasn't been
                                                // added to the tileset yet.
                                                // Console.WriteLine("Block failed: ({0}, {1}, {2})", x, y, z);

                                            }
                                        }
                                    }
                                }
                            }
                            if (CommandConfig.Directory.Length > 0)
                            {
								if (!Directory.Exists(CommandConfig.Directory))
								{
									Directory.CreateDirectory(CommandConfig.Directory);
								}
                                bmp.Save(Path.Combine(CommandConfig.Directory, string.Format("{0}.png", y)), ImageFormat.Png);
                            }
                        }

						if (CommandConfig.Output.Length > 0)
						{
                        	bmp.Save(CommandConfig.Output, ImageFormat.Png);
							Console.WriteLine("Saved image to {0}", CommandConfig.Output);
						}
						
						if (CommandConfig.Directory.Length > 0)
						{
							Console.WriteLine("Saved layers to {0}", CommandConfig.Directory);
						}
                    }
                }
                for (int idx = 0; idx < images.Count; idx++)
                {
                    images[idx].Dispose();
                }
				
            }
        }
    }
}
