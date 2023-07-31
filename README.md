# ModelEx
A tool for viewing and exporting the 3D models in Legacy of Kain: Soul Reaver 1 &amp; 2, Defiance, Gex 3: Deep Cover Gecko, Tomb Raider: Legend, and Tomb Raider: Anniversary

To use this tool, the model and texture files (*.drm, *.pcm) should be extracted from the bigfile using Soul Spiral of one of the various tools for unpacking games in Crystal Dynamics' CDC engine (aka Gex engine). Ideally they should be extracted with the directory structure intact.

Once the files are extracted, the models can be viewed in three ways:

1. As a *Scene* with all object placements displayed.
2. As a standalone *Object* with all alternate models visible.
3. As a *Debug Object* with with all alternate models visible as well as debug rendering options that can be accessed from the menu.

Scenes and Objects are cached as resources so that they can be rendered as instances of objects placed in other scenes. Anything loaded as either of these can be viewed in both of those modes.

Debug objects aren't cached because they need to be reloaded every time the render mode is changed. This can be very slow for older games such as Gex and Soul Reaver because of the complicated way the textures are stored.
