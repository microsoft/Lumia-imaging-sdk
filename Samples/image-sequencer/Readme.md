Image Sequencer
===============

Image Sequencer is an example application demonstrating the use of Lumia Imaging SDKs Image Aligner and Gif Renderer APIs for creating cinemagraph-style animations in animated GIF format. The application has a set of hard coded image sequences to be used for basis of the alignment and animation. User can manipulate the animation by limiting the animated area to a small rectangular section, and by stabilizing the images in order to eliminate camera shake. Animations with still backgrounds and minor repeated movement are commonly called cinemagraphs.

Developed with Microsoft Visual Studio Express 2015 

Compatible with:

 * Windows 10

Instructions
------------

Make sure you have the following installed:

 * Windows 10
 * Visual Studio Express 2015 
 * Nuget 3.1.6 or later

To build and run the sample in emulator

1. Open the SLN file:
   File > Open Project, select the solution (.sln postfix) file for the target
   
2. Select the target 'Emulator' and platform 'x86'.
3. Press F5 to build the project and run it.


If the project does not compile on the first attempt it's possible that you
did not have the required packages yet. With Nuget 3.1.6 or later the missing
packages are fetched automatically when build process is invoked, so try
building again. If some packages cannot be found there should be an
error stating this in the Output panel in Visual Studio Express.



About the implementation
------------------------

| Folder | Description |
| ------ | ----------- |
| / | Contains the project file, the license information and this file (README.md) |
| ImageSequencer | Root folder for the implementation files.  |
| ImageSequencer/Assets | Graphic assets like icons and tiles. |
| ImageSequencer/Resources | Localized resources. |
| ImageSequencer/Properties | Application property files. |
| ImageSequencer/Toolkit.Content | Graphics assets for Windows Phone toolkit. |

Important classes:

| Class | Description |
| ----- | ----------- |
| MainPage | Page for displaying the preview animation. |
| ImagePicker | Page for displaying hard coded image sequences and saved GIF files in a grid. |
| GifExporter | Utility class for exporting GIF animations. |


Known issues
------------

 * Application tombstoning is not supported.

## Reference

[Lumia Imaging SDK](http://go.microsoft.com/fwlink/?LinkID=521939)