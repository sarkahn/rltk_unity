# RLTK for Unity
This is a Unity port of [The Roguelike Toolkit (RLTK), implemented for Rust](https://github.com/thebracket/rltk_rs).

![](./Assets/Documentation/images~/splash.png)
 
The samples demonstrate how to properly set up and write to a console:
##### Samples/Noise 

![](./Assets/Documentation/images~/noise.gif)

##### Samples/ShaderExample

![](./Assets/Documentation/images~/console_shader2.gif)


## The Console

There are three primary console types, `SimpleConsole`, `NativeConsole` and `SimpleConsoleProxy`:

##### [SimpleConsole](Assets/Runtime/RLTK/Consoles/SimpleConsole.cs)
  * Provides a straightforward API for writing text to a console.
  * **NOT** a MonoBehaviour, you must construct and use it from code.
  * Must manually call `Draw()` and `Update()` every frame to render it.
  * Must call manually `Dispose()` before it goes out of scope to free internal [unmanaged memory](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html). 

##### [NativeConsole](Assets/Runtime/RLTK/Consoles/NativeConsole.cs)
  * Derived from `SimpleConsole`, provides a few more advanced functions for those familiar with Unity's [job system](https://docs.unity3d.com/2019.3/Documentation/Manual/JobSystem.html).
  * Same rules for `Draw()` and `Dispose()`.

##### [SimpleConsoleProxy](Assets/Runtime/RLTK/MonoBehaviours/SimpleConsoleProxy.cs)
  * A MonoBehaviour wrapped around a `SimpleConsole`. Allows you to easily reference and write to a console from other MonoBehaviours.
  * Console properties can be tweaked from the inspector.
  * No need to call `Dispose()` or `Draw()`. 
  * Can be set up automatically using the menu option `GameObject/RLTK/Initialize Simple Console`.

## How to get it
The recommended way to use this package is via the Unity Package Manager. At the top left 
of the package manager, click the "+" button and choose "Add package from git url...". 
Then paste in `https://github.com/sarkahn/rltk_unity.git#upm`
and you should be good to go.

![](./Assets/Documentation/images~/upm.gif)

This will automatically install the package and all required dependencies. You can import 
the built in samples from the package manager UI.

If you need to update RLTK you can remove and re-install it via the package manager or delete 
this section from the "Packages/manifest.json" file in your project root folder:

![](Assets/Documentation/images~/manifest.png)

That will cause the package manager to automatically update to the latest version.

## What does it do
Functionality is very limited right now - all you can do is write colored text to a console. This project is not made with Unity beginners in mind nor those without some programming experience.

## How to use it

For examples of how to use the different consoles check the [samples](https://github.com/sarkahn/rltk_unity/tree/master/Assets/Samples) and [tests](https://github.com/sarkahn/rltk_unity/tree/master/Assets/Tests/Editor).

In order to avoid rendering artifacts you must make sure to keep the console position locked to the pixel grid (origin works) and have the viewport and camera set up properly. The examples accomplish this using the [LockCameraToConsole](Assets/Runtime/RLTK/Monobehaviours/LockCameraToConsole.cs) component or with [RenderUtility.AdjustCameraToConsole](Assets/Runtime/RLTK/Rendering/RenderUtility.cs#L112). Again, see the examples for expected usage.