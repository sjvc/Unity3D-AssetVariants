What is the HD/SD problem with 2D sprites?
------------------
There are a lot of devices, with different resolutions, screen densities and amount of RAM. So, which resolution should you use in your game for your assets? If you make them hi-res, they will look awesome!, but in low end devices it will be a waste of memory. And, on the other hand, if we make them low-res, we will need less memory, but they will look so bad...

An easy way to deal with it, is to include different versions of our sprites in our game and, when the game is loaded, we can decide which sprite version should be used. 
But unfortunately Unity3D does not have implemented (yet) an easy way to do it. So... this is a problem.

Wait... Unity3D does not have a solution for this problem?
-------------
Well, using [AssetBundle variants](https://unity3d.com/es/learn/tutorials/topics/scripting/assetbundles-and-assetbundle-manager) you can achieve it. But, I feel [we need a more specific feature](http://forum.unity3d.com/threads/official-how-did-asset-bundle-variant-fail-to-satisfy-your-hd-sd-use-case.375716/) for solving this problem.

The good news is that they are [working on a solution](http://forum.unity3d.com/threads/sd-hd-sprites.408565/) that they may release in the future.

How I solved this problem?
---------------
This is not a final solution for this problem, but a temporary workaround until they release an official solution. The good news is that it may work for you.

In Unity3D, when a scene is loaded, Unity3D automatically loads all the sprites that are referenced in the game objects in such scene. So what I did is an editor script that duplicates a scene, loops through all the game objects, and replaces the sprite references in their sprite renderers. If the game object has animations, and any animation has a sprite reference, the script also creates a new animator controller referencing new animations that reference different sprites. 
So, for example, if you created a scene using HD sprites, you can use my script to generate a new scene that uses SD sprites.

Also, I included a method for duplicating a prefab, replacing its sprites in its sprite renderer and animations.

Ok, it may work. Let's do it:
---------------
Let's say you created your scene using HD sprites, and you want to create a new scene with SD assets:

1.  Copy AssetVariants.cs and AssetVariantsMenu.cs to Assets/Editor folder (create it if not exists).
2.  Create a new folder called Variant-sd next to the HD texture files, and add the SD versions inside.
3.  Set Pixels Per Unit, in Import Settings, for your SD textures. If Pixels Per Unit of your HD texture is 100, and SD texture is half size, then it should be 50.
4.  Do right click on your scene asset, in project window, and select "Create Scene Variants".
5.  Now, the folder Assets/Variant-sd is automatically created, and all the generated files are inside. Open the scene inside to see your SD sprites in it.
6.  Also, you can do right click on your prefabs, and select "Create prefab variants", and files will be created inside Assets/Variant-sd folder.

You should know:
---------------
*  Edit the file AssetVariantsMenu.cs so you can decide which variants will be generated.
*  Delete the folder Assets/Variant-{variant_name} before running the scene generation again.
*  This code only replaces sprite references in sprite renderers inside game objects and animations. If there are other references to sprites, they won't be replaced.
*  When you generate a prefab variant, references of such prefab won't be replaced in the scene. So if you instantiate prefabs in your scene at runtime, you will be still instantiating the original prefabs, not the variants. Despite that, you may find this feature useful.
