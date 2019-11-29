# Histogrammer

*Histogrammer* is a tool for Unity3D that helps you pinpoint redundant data in your project.

Nobody likes a messy project. You know how it goes. You work on your project for a long time and things just stack up. Hundreds of prefabs, many more scripts, and you just start to lose a bit of oversight.

Another issue is that sometimes you just expose too much in the inspector. Every time you look at a prefab you see too many options you can customize. Most stay the same, and half of them you don't even know what they do anymore.

This is where *Histogrammer* comes in. With this tool you can more easily pinpoint data in your project that isn't actually being used all that often, or just straight up is the same value across all prefabs.

<p align="center">
  <img src="https://github.com/Thundernerd/Unity3D-Histogrammer/blob/master/Documentation%7E/histogrammer_1.png">
</p>

## Usage

*These steps assume that Unity has been opened and Histogrammer has been added to the project.*

1. Open *Histogrammer* by selecting *Window/TNRD/Histogrammer* from the top menu

2. Select a script using the script field or drag-and-drop one from your project browser

3. Select the field you wish to inspect from the popup browser next to the *Field* label

4. Press the search button and wait for *Histogrammer* to complete scanning your project

#### Expanding results

To show the detailed results of one of the values shown in the Results section. Click on one of the bars as shown below.

|     |     |
| --- | --- |
| ![](https://github.com/Thundernerd/Unity3D-Histogrammer/blob/master/Documentation%7E/histogrammer_2.png) | ![](https://github.com/Thundernerd/Unity3D-Histogrammer/blob/master/Documentation%7E/histogrammer_3.png) |

#### Pinging objects

Instead of looking for the objects in your project browser manually. You can click on one of the lines in the detailed results views to highlight the object in the project browser.
