# The Roguelike Toolkit (RLTK) for Unity

![](Documentation~/images~/splash.png)

This is a framework built specifically for creating roguelikes and rendering ascii efficiently and without artifacts inside Unity. While the API is made to be as simple and straightforward as possible, internally it utilizes Unity's Job system and Burst compiler to achieve great performance and avoid any memory allocations whenever possible.

This code is based off [The Roguelike Toolkit (RLTK), implemented for Rust](https://github.com/thebracket/rltk_rs).

## How to Install

RLTK is designed as a package. You can install it via the Package Manager: 

__"Add package from git url..."__ -> `https://github.com/sarkahn/rltk_unity.git`

## The Samples

The samples demonstrate how to properly set up and write to a console. They can be imported from the package manager gui.

##### Samples/Noise

![](Documentation~/images~/noise.gif)

##### Samples/ShaderExample

![](Documentation~/images~/console_shader2.gif)


## The Consoles

There are three primary console types, `SimpleConsole`, `NativeConsole` and `SimpleConsoleProxy`:

##### [SimpleConsole](Runtime/Consoles/SimpleConsole.cs) - Usage is demonstrated in the "ManualDraw" sample.
  * Provides a straightforward API for writing text to a console.
  * **NOT** a MonoBehaviour, you must construct and use it directly from code.
  * Must manually call `Draw()` and `Update()` every frame to render it.
  * Must call manually `Dispose()` before it goes out of scope to free internal [unmanaged memory](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html).



##### [NativeConsole](Runtime/Consoles/NativeConsole.cs) - Usage is demonstrated in the "Noise" sample.
  * Derived from `SimpleConsole`, provides a few more advanced functions for those familiar with Unity's [job system](https://docs.unity3d.com/2019.3/Documentation/Manual/JobSystem.html).
  * Same rules for `Draw()` and `Dispose()`.

##### [SimpleConsoleProxy](Runtime/MonoBehaviours/SimpleConsoleProxy.cs) - Usage is demonstrated in the "Hello World" sample.
  * A MonoBehaviour wrapped around a `SimpleConsole`. Allows you to easily reference and write to a console from other MonoBehaviours.
  * Console properties can be tweaked from the inspector.
  * No need to call `Dispose()` or `Draw()`.
  * Can be set up automatically using the menu option `GameObject/RLTK/Initialize Simple Console`.



**IMPORTANT**: In order to avoid rendering artifacts you must make sure to keep the console position locked to the pixel grid (origin works) and have the viewport and camera set up properly. An easy way to automatically set up the camera is directly from code with a single function call to [RenderUtility.AdjustCameraToConsole](Runtime/Rendering/RenderUtility.cs#L112).


If you're using SimpleConsoleProxy you can use the [LockCameraToConsole](Runtime/Monobehaviours/LockCameraToConsole.cs) MonoBehaviour instead.

## What does it do
* You can write to a console with the `Set` and `Print` functions, or retrieve the native tiles from the console for use directly inside jobs. Consoles are fast enough to clear and draw a large number of tiles every frame, the way you would in a traditional roguelike.
* There are field of view functions in the `FOV` class.

## How to use it

For examples of how to use the different parts of the framework check the [samples](https://github.com/sarkahn/rltk_unity/tree/master/Assets/Samples) and [tests](https://github.com/sarkahn/rltk_unity/tree/master/Assets/Tests/Editor).

Along with RLTK I am [developing a Roguelike that uses RLTK as a backend](https://github.com/sarkahn/rltk_unity_roguelike), based on the excellent [Roguelike Tutorial in Rust](https://bfnightly.bracketproductions.com/rustbook/chapter_1.html). It's being developed using Unity's ECS framework and should be of interest to anyone who would want to know how to actually make a game using RLTK.

---
RLTK for Unity will always be free and the code will always be open source. With that being said I put quite a lot of work into it. If you find it useful, please consider donating. Any amount you can spare would really help me out a great deal - thank you!

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=Y54CX7AXFKQXG)