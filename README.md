# RLTK for Unity
This is a Unity port of [The Roguelike Toolkit (RLTK), implemented for Rust](https://github.com/thebracket/rltk_rs).

The samples demonstrate how to properly set up and write to a console:
##### Samples/Noise 

![](./Assets/Documentation/images~/noise.gif)

##### Samples/ShaderExample

![](./Assets/Documentation/images~/console_shader2.gif)

## How to get it
The recommended way to use this package is via the Unity Package Manager. At the top left 
of the package manager, click the "+" button and choose "Add package from git url...". 
Then paste in `https://github.com/sarkahn/rltk_unity#upm`
and you should be good to go.

![](./Assets/Documentation/images~/upm.gif)

This will automatically install the package and all required dependencies. You can import 
the built in samples from the package manager UI.

If you need to update RLTK you can remove and re-install it via the package manager or delete 
this section from the "Packages/manifest.json" file in your project root folder:

![](Assets/Documentation/images~/manifest.png)

That will cause the package manager to automatically update to the latest version.

## What does it do
Functionality is very limited right now - all you can do is write colored text to a console. 

In order to avoid rendering artifacts you must make sure to keep the console position locked to the pixel grid (origin works) and have the viewport and camera set up properly. The examples accomplish this using [LockCameraToConsole](./Runtime/RLTK/Monobehaviours/LockCameraToConsole.cs) in combination with unity's URP PixelPerfectCamera script.