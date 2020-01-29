# Change Log

## [0.2.3] - 2020-01-14
* Simplified RenderUtility API a little
 
## [0.2.2] - 2020-01-14
* Updated readme
* Moved RenderUtility to Rendering folder

## [0.2.1] - 2020-01-14
Rendering changes, removed some cruft
* Moved most rendering related code to RenderUtility
* Removed PostProcessing stuff
* Added optional "pixel data" to mesh for proper scanline /pixel effects
* Updated to Unity 2019.3.0f6

## [0.2.0] - 2020-01-14
* Split console job-related functions into a derived "NativeConsole", "SimpleConsole" provides basic single-threaded behaviour. See the [Readme](README.md) for details.
* Added more console tests to demonstrate usage.
* Added a "ManualDraw" sample to show how to render a non-monobehaviour console.
* Fleshed out readme a bit more.

## [0.1.3] - 2020-01-08
* Changed package name to just "RLKT" so it doesn't look so horrible in the packages folder.

## [0.1.2] - 2020-01-08
* Added editor create menu
* Fixed material resource name bug.

## [0.1.1] - 2020-01-08
* Some shader tweaking, added a splash screen example.

## [0.1.0] - 2020-01-07
* Reworked project layout to be in line with Unity's [recommended package layout](https://docs.unity3d.com/Manual/cus-layout.html).
* Added Travis content integration to automate creating a UPM-ready package branch using [unity-package-manager-ci](https://github.com/TrismegistusDevelopment/unity-package-manager-ci).
* Update [the readme](./README.md) with shiny images.