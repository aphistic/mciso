using System;
using System.Collections.Generic;
using System.Text;

namespace mciso
{
    internal static class CommandConfig
    {
        public static string MapFile = "";
        public static string Output = "";
        public static RenderType Type = RenderType.Isometric;
        public static bool UseRenderLayers;
		public static int RenderInterval = 1;
        public static List<int> RenderLayers = new List<int>();
        public static string Directory = "";

        public static bool UseHighQuality;
        public static bool UseLowQuality;

        public enum RenderType
        {
            Unknown,
            Isometric,
            Overview
        }

        static CommandConfig()
        {
            ResetConfig();
        }
        public static void ResetConfig()
        {
            MapFile = "";
            Output = "";
            Type = RenderType.Isometric;
            UseRenderLayers = false;
			RenderInterval = 1;
            RenderLayers.Clear();
            Directory = "";

            UseHighQuality = false;
            UseLowQuality = false;
        }
        public static void LoadConfig(string[] args)
        {
            for (int idx = 0; idx < args.Length; idx++)
            {
                if (args[idx].StartsWith("-"))
                {
                    switch (args[idx].ToLower())
                    {
                        case "-map":
                            if (args.Length < idx + 1 || args[idx + 1].StartsWith("-"))
                                throw new ArgumentException("Please specify a map name to use.");

                            MapFile = args[++idx];
                            break;
                        case "-out":
                            if (args.Length < idx + 1 || args[idx + 1].StartsWith("-"))
                                throw new ArgumentException("Please specify a file to save to.");

                            Output = args[++idx];
                            break;
                        case "-lq":
                            UseLowQuality = true;
                            break;
                        case "-hq":
                            UseHighQuality = true;
                            break;
                        case "-type":
                            if (args.Length < idx + 1 || args[idx + 1].StartsWith("-"))
                                throw new ArgumentException("Please specify a render type.");

                            switch (args[++idx].ToLower())
                            {
                                case "iso":
                                    Type = RenderType.Isometric;
                                    break;
                                case "overview":
                                    Type = RenderType.Overview;
                                    break;
                                default:
                                    throw new ArgumentException(string.Format("Unknown render type: {0}", args[idx + 1]));
                            }
                            break;
                        case "-layers":
                            if (args.Length < idx + 1 || args[idx+1].StartsWith("-"))
                                throw new ArgumentException("Please specify the layers to render.");

                            UseRenderLayers = true;

                            string[] layers = args[++idx].Split(new [] {','});
                            for (int ldx = 0; ldx < layers.Length; ldx++)
                            {
                                // Split on dash to get ranges
                                string[] range = layers[ldx].Split(new[] {'-'});
                                if (range.Length > 1)
                                {
                                    // Have a range, try to parse the first number for the start
                                    int startRange;
                                    if (!int.TryParse(range[0], out startRange))
                                    {
                                        // Start range can't be parsed, something's messed up with this arg
                                        throw new ArgumentException(string.Format("Unable to parse layer parameter: {0}", range[0]));
                                    }

                                    // Now check if the end range has a slash (signifying an interval)
                                    string[] interval = range[1].Split(new[] {'/'});
                                    if (interval.Length > 1)
                                    {
                                        // First value should be the end range and the second should be the interval
                                        int endRange;
                                        if (!int.TryParse(interval[0], out endRange))
                                        {
                                            throw new ArgumentException(string.Format("Unable to parse layer parameter: {0}", interval[0]));
                                        }

                                        int rangeInterval;
                                        if (!int.TryParse(interval[1], out rangeInterval))
                                        {
                                            throw new ArgumentException(string.Format("Unable to parse layer parameter: /{0}", interval[1]));
                                        }

                                        for (int i = startRange; i <= endRange; i += rangeInterval)
                                        {
                                            RenderLayers.Add(i);
                                        }
                                    }
                                    else
                                    {
                                        int endRange;
                                        if (!int.TryParse(range[1], out endRange))
                                        {
                                            throw new ArgumentException(string.Format("Unable to parse layer parameter: {0}", range[1]));
                                        }

                                        for (int i = startRange; i <= endRange; i++)
                                        {
                                            RenderLayers.Add(i);    
                                        }
                                    }
                                }
                                else
                                {
									if (layers[ldx].StartsWith("/"))
									{
										int interval;
										if (!int.TryParse(layers[ldx].Substring(1), out interval))
										{
											throw new ArgumentException(string.Format("Unable to parse layer parameter: {0}", layers[idx]));
										}
										
										RenderInterval = interval;
									}
									else
									{
	                                    int layer;
	                                    if (!int.TryParse(layers[ldx], out layer))
	                                    {
	                                        throw new ArgumentException(string.Format("Unable to parse layer parameter: {0}", layers[ldx]));
	                                    }
	
	                                    RenderLayers.Add(layer);
									}
								}
							}
                            break;
                        case "-dir":
                            if (args.Length < idx + 1 || args[idx + 1].StartsWith("-"))
                                throw new ArgumentException("Please specify the directory to render to.");

                            Directory = args[++idx];
                            break;
                    }
                }
            }
        }

        public static string Usage()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Application Parameters:\n");
            sb.Append("-map\t\tThe Minecraft mclevel file to load and work with.\n\n");

            sb.Append("-out\t\tFilename to save the final image to.\n\n");

            sb.Append("-dir\t\tDirectory to save the collection of images to.\n\n");

            sb.Append("-type[=iso]\tThe type of images to render. Options are:\n");
            sb.Append("\t\t\tiso - Render the level in an isometric view.\n");
            sb.Append("\t\t\toverview - Render the level in a top-down overview.\n\n");

            sb.Append("-layers\t\tThe layers to render in the image. If you use\n");
            sb.Append("\t\tthis option you can also define -dir to store each\n");
            sb.Append("\t\tof the layers in its own image in the provided directory.\n");
            sb.Append("\t\tLayers can be defined in the following ways:\n");
            sb.Append("\t\t\t0,1,2 - Individually\n");
            sb.Append("\t\t\t0-10,20-30 - Ranges\n");
            sb.Append("\t\t\t/2 - Intervals (every 2, 3, etc)\n");
            sb.Append("\t\t\t0,1,2,20-30,50-100/2 - A combination of the options.\n");

            return sb.ToString();
        }

        public static void ValidateConfig()
        {
            if (MapFile.Length < 1)
            {
                throw new ArgumentException("Please supply a level to load.");
            }
            if (Type == RenderType.Overview && Directory.Length < 1)
            {
                throw new ArgumentException("When rendering a map overview, an output directory must be defined with the -dir option.");
            }
			if (Output.Length < 1 && Directory.Length < 1)
			{
				throw new ArgumentException("Some form of output must be specified.");
			}
            return;
        }
    }
}
