# ModelEx
A tool for viewing and exporting the 3D models in Legacy of Kain: Soul Reaver 1 &amp; 2, Defiance, Gex 3: Deep Cover Gecko, Tomb Raider: Legend, and Tomb Raider: Anniversary

To use this tool, the model and texture files (*.drm, *.pcm) should be extracted from the bigfile using Soul Spiral of one of the various tools for unpacking games in Crystal Dynamics' CDC engine (aka Gex engine). Ideally they should be extracted with the directory structure intact.

Once the files are extracted, the models can be viewed in three ways:

1. As a *Scene* with all object placements displayed.
2. As a standalone *Object* with all alternate models visible.
3. As a *Debug Object* with with all alternate models visible as well as debug rendering options that can be accessed from the menu. Many are only relevent to SR1.

Each of the above is available to load from the File->Load memu option. Some models depend on seperate files for their textures and names of places objects. ModelEx will look for these either in expected location within the game's own directory structure or in the same folder as the model file. These locations can be selected from the combo box on the Load Resource dialog. This dialog also contains a checkbox to clear the loaded files (see below).

Scenes and Objects are cached as resources so that they can be rendered as instances of objects placed in other scenes. Anything loaded as either of these can be viewed in both of those modes.

Debug objects aren't cached because they need to be reloaded every time the render mode is changed. This can be very slow for older games such as Gex and Soul Reaver because of the complicated way the textures are stored.

You can switch between viewing Scenes, Objects, and Debug Objects from the Scene Mode menu.

Once opened, the models can be exported to the more common Collada (*.dae) format from the Fie->Export menu. Select Scene or Object to chose whether to export the currently selected Scene or the currently selected Object.

ModelEx supports three different camera modes:

1. *Ego* is a first person camera. Click the render window, then use WASD to move forward, backward, left and right, Q and E to move up and down, left-click and drag with the mouse to change the angle, and use the mouse wheel to set the movement speed. 
2. *Orbit* is an arcball camera that rotates around the model. Click the render window, then left-click and drag with the mouse to move the camera around, and use the mouse wheel to move closer/further from the target.
3. *Orbit Pan* is an arcball camera that rotates around the model. Click the render window, then left-click and drag with the mouse to move the camera around, and use the mouse wheel to move closer/further from the target. Right-click and drag with the mouse to pan the camera.
