Volumetric Light Beam Kit


This tool allows you easily to create cylinders ready to use with our volumetric light shaders to create light beam like effects. Instead of going through a 3d modelling software such as Maya or 3ds max to create your mesh, you can do it directly in Unity. It comes with an easy to use cylinder creation tool, shaders and a few textures.

To create a beam go to GameObject/Create Other/Light Beam in the Unity menu. This will spawn a beam in front of you in the scene. Unity will automatically select it and enter modification mode. When this mode is active you can freely modify your beam by either using the handles in the scene or the properties in the inspector. When you are finished modifying your beam press the “Done” button in the inspector. This exits the modification mode and the beam is now a normal mesh in the editor. What also happened when you created the beam from the menu, a mesh asset and a material was created in the root asset folder. If we wouldn’t do this the beam would be saved in the scene and the save times would increase, especially if there are many beams in the scene. A common practice is to move the files to a good location right away. If it belongs to a lamp, move it to the lamp’s location. If it is a scene specific asset, move it to the scene location. You can do whatever you want but it’s nice with a tidy project :).

When you exited modification mode the inspector changed to two buttons; Modify and New Lightbeam Mesh. Modify enters the modification mode again and the New Light Beam button creates a new mesh and saves it where the original is located. A dialog appears that asks whether you want to use the same material as the current lightbeam or duplicate it.

The shader uses two channels in the base texture to achieve the light beam effect. These two channels work like alpha textures and combined they shape the light beam. The mask in the red channel is always facing towards the camera; this is the main mask and the shape of the beam is dictated by this channel. The green channel is masking the beam when the viewer is looking towards the light beam from a top down angle. The Width and Tweak parameters are there to tweak the light beam when a different mask texture is used and shouldn’t need to be touched otherwise.

The detail textures works like a normal texture with alpha and uses the beam’s original UVs. This texture can be used to create dust, fog, light rays and other stuff to make the beams look a certain way. The UVs can be animated with the Detail Animation parameter, the X value animates the U (horizontal) and Y animates V (vertical). This is the speed at which the UVs are offset. The tiling and offset parameters used by Unity for textures can be used with this one.

Note: To use the soft versions of the shaders you need a pro license, this is because a depthbuffer need to be written and it's impossible with standard license.

Video: http://www.youtube.com/watch?v=bxTa6oCV6LQ