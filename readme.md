Lumia Imaging SDK projects
==========================

### Contents
- [1. Quick start](#quickstart)
- [2. Sample projects](#samples)
- [3. Lumia Imaging SDK Extras](#extras)
 - [3.1. Philosophy](#philosophy)
 - [3.2. Managed (C#/.NET)](#managed)
 - [3.2.1 Layer system](#layers)
 - [3.2.2 Image sources/effects](#sources)
 - [3.2.3 Utility code](#utility)
- [4. Reference](#reference)

## <a id="quickstart"></a>1. Quick start

Quick Start is a sample project accompanying the tutorial that helps to get your first app that utilizes the Lumia Imaging SDK up and running. This sample implements the following basic tasks: picking an image from the camera roll, applying an effect to it, and processing the filtered image to be rendered and saved as a full resolution JPEG.

<table>
 <tr>
  <th colspan="1" align="left">Quick Starts</th>
 </tr>
 <tr>
  <td><a href="Samples/QuickStart/cs">Quick Start (C#) for Windows 10 and Windows 10 Mobile</a></td>
 </tr>
 <tr>
  <td><a href="Samples/QuickStart/vb">Quick Start (Visual Basic) for Windows 10 and Windows 10 Mobile</a></td>
 </tr>
 <tr>
  <td><a href="Samples/QuickStart/js">Quick Start for Windows Phone 10 HTML5/JScript</a></td>
 </tr>
 <tr>
  <td><a href="Samples/QuickStartUniversal8.1">Quick Start for Windows 8.1 and Windows Phone 8.1</a></td>
  </tr>
</table>

## <a id="samples"></a>2. Sample projects

<table>
 <tr>
  <th colspan="2" align="left">Samples</th>
 </tr>
 <tr>
  <td><a href="Samples/EditShowcase">Edit Showcase</a></td>
  <td>Edit Showcase is an example app that demonstrates the use of effects on still images. The photo is processed with the predefined effects. In addition, custom controls are implemented for manipulating the effect properties in real time. The processed image can be saved in the JPEG format into the camera roll.</td>
 </tr>
 <tr>
  <td><a href="Samples/VideoEffectSample">Video Effect</a></td>
  <td>Video Effect sample is an example app that demonstrates the capabilities and performance of the Lumia Imaging SDK by allowing the user to preview and apply a number of real-time effects to camera preview. The effects are applied to the stream received from the camera and shown in the viewfinder. The effects can be changed using the buttons in the application bar. This example app supports recording and capturing of videos and photos.</td>
 </tr>
 <tr>
  <td><a href="Samples/LumiaImagingSDKWin2DDemo">Lumia Imaging SDK and Win2D Demo</a></td>
  <td>Lumia Imaging SDK and Win2D Demo is an example app that demonstrates the use of the Lumia Imaging SDK together with the Win2D API.</td>
 </tr>
 <tr>
  <td><a href="Samples/image-sequencer">Image Sequencer</a></td>
  <td>Image Sequencer is an example app that demonstrates the use of the Image Aligner and GIF Renderer APIs for creating Cinemagraph-style animations in the animated GIF format. There are also some example image sequences that can be used as a basis for the alignment and animation.</td>
  </tr>
 <tr>
  <td><a href="Samples/CustomEffectSample">Custom Effect Sample</a></td>
  <td>Custom Effect sample demonstrates  how to create Custom Effects to do image manipulation both on the CPU and the GPU.</td>
  </tr>
</table>

## <a id="extras"></a>3. Lumia Imaging SDK Extras

This repository contains extra functionality and sample code for the Lumia Imaging SDK. 

The code is provided under the MIT license, and can therefore be conveniently used and modified. 

Parts contained will typically target the latest release version of the Lumia Imaging SDK, unless otherwise marked.

### <a id="philosophy"></a>3.1 Philosophy

- The **projects** (.csproj, .vcxproj etc) collect various classes/features, but are mainly for sample and testing purposes. 
- This repository should be considered as a *set of individual classes*, not as a library.
- New revisions may not be compatible with old ones.
- Therefore, if you find something useful, it may be easier to *isolate the part you want* instead of taking a dependency on all the projects included.

### <a id="managed"></a>3.2 Managed (C#/.NET)

#### <a id="layers"></a>3.2.1 Layer system
    Managed/Lumia.Imaging.Extras.Layers/

Allows to describe image processing as a list of layers, like the familiar representation found in photo editing apps. 

After configuring the layers, an IImageProvider endpoint can be easily retrieved and rendered.

**Features**
- Layers and Adjustment Layers
- Layer styles (blend function, opacity, transparency mask, scaling/translation)
- Tuned for performance and low GC pressure in interactive scenarios.
- Flexible, construction of actual objects can be deferred etc.

#### <a id="sources"></a>3.2.2 Image sources/effects
    Managed/Lumia.Imaging.Extras.ImageProviders/

**NoiseImageSource**
A noise generator image source. Internally uses a ColorImageSource and a NoiseFilter.

**HighpassEffect**
A "highpass" effect, similar to familiar ones in photo editing apps. 

**DepthofField**
A set of high-level scenarios showing how to set up a "DoF" effect.

**HslAdjustmentEffect**
Example of an effect that does higher level HSL adjustments, similar to familiar ones in photo editing apps. 
Allows adjustments of saturation and lightness around Master, Red, Green, Blue, Cyan, Magenta and Yellow channels.

#### <a id="utility"></a>3.2.3 Utility code
    Managed/Lumia.Imaging.Extras.Utility/

**ImageProviderExtensions**
- **GetBitmapAsync** overloads that help to reuse bitmaps.
- **Rotate** method, making it convenient to rotate an image provider.

**BitmapExtensions**
- **CopyTo**/**ToWriteableBitmap**: Conversions to WriteableBitmap. These help in interactive scenarios, and can be useful for keeping work off the UI thread.

**BufferExtensions**
- **AsBufferTask**: Conversion from Task&lt;IBuffer&gt; to a Lumia Imaging SDK IBufferProvider that can be passed into a BufferProviderImageSource. This can be useful to avoid having to await the task-of-buffer before setting up an image source.

**MaybeTask&lt;T&gt;**
Value type that either holds a result **or** a task-of-result. This helps interactive app scenarios, keeping GC activity in check when dealing with mixed sync/async operations, as otherwise each new Task causes a heap allocation.

### <a id="reference"></a>4. Reference

[Lumia Imaging SDK](http://go.microsoft.com/fwlink/?LinkID=521939)
