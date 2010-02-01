mciso
Written by: Erik Davidson
---
A C# utility to render isometric and top-down overviews of Minecraft levels.

Application usage:

To execute the utility you must have either the .Net framework installed (for Windows) or 
mono installed (for Linux or OS X). You can execute it using the following:

mono mciso.exe [parameters]

The available parameters are as follows:
-map		The Minecraft mclevel file to load and work with. This is required!

-out		Filename to save the final image to. Optional if you use the -dir
		parameter.		

-dir		Directory to save the collection of images to.

-type[=iso]	The type of images to render. Options are:
			iso - Render the level in an isometric view.
			overview - Render the level in a top-down overview.

-layers		The layers to render in the image. If you use
		this option you can also define -dir to store each
		of the layers in its own image in the provided directory.
		Layers can be defined in the following ways:
			0,1,2 - Individually
			0-10,20-30 - Ranges
			/2 - Intervals (every 2, 3, etc)
			0,1,2,20-30,50-100/2 - A combination of the options.

As an example of how the layers are read, the last example listed above (0,1,2,20-30,50-100/2)
would render layers 0, 1, 2, 20 through 30 and then every other layer from 50 to 100.

Heavily inspired by Isocraft by pubby8 (http://www.minecraftforum.net/viewtopic.php?id=4231)
Credit for some of the initial tilesets goes to pubby8 as well.