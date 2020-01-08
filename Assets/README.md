# RLTK for Unity
This is a Unity port of [The Roguelike Toolkit (RLTK), implemented for Rust](https://github.com/thebracket/rltk_rs).

The samples demonstrate how to properly set up and write to a console:
##### Samples/Noise 
![](./Documentation/images~/noise.gif)

##### Samples/ShaderExample
![](./Documentation/images~/console_shader2.gif)

## How to get it
The recommended way to use this package is via the Unity Package Manager:
![](./Documentation/images~/upm.gif)

This will automatically install the package and all required dependencies, as well as letting you easily receive package updates via the package manager. When the package is intalled you can import the samples via the package manager ui.

## What does it do
Functionality is very limited right now - all you can do is write colored text to a console. 

In order to avoid rendering artifacts you must make sure to keep the console position locked to the pixel grid (origin works) and have the viewport and camera set up properly. The examples accomplish this using [LockCameraToConsole](./Runtime/RLTK/Monobehaviours/LockCameraToConsole.cs) in combination with unity's URP PixelPerfectCamera script.