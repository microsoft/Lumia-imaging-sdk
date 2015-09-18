
Lumia Imaging SDK Extras
========================

This repository contains extra functionality and sample code for the Lumia Imaging SDK. 

The code is provided under the MIT license, and can therefore be conveniently used and modified. 

Parts contained will typically target the latest release version of the Lumia Imaging SDK, unless otherwise marked.

###Philosophy

- The **projects** (.csproj, .vcxproj etc) collect various classes/features, but are mainly for sample and testing purposes. 
- This repository should be considered as a *set of individual classes*, not as a library.
- New revisions may not be compatible with old ones.
- Therefore, if you find something useful, it may be easier to *isolate the part you want* instead of taking a dependency on all the projects included.

###Release notes

- Updated to match Lumia Imaging SDK 3.0.
- DotVisualizerExtensions removed. See functionality added to renderer classes in the SDK.
- Native custom filter/effect helpers removed. See CustomEffectSample for how to create native effects.
- Lumia.Imaging.Extras.Sample removed. See new samples.

##Layer system
    Lumia.Imaging.Extras.Layers/

Allows to describe image processing as a list of layers, like the familiar representation found in photo editing apps. 

After configuring the layers, an IImageProvider endpoint can be easily retrieved and rendered.

###Features
- Layers and Adjustment Layers
- Layer styles (blend function, opacity, transparency mask, scaling/translation)
- Tuned for performance and low GC pressure in interactive scenarios.
- Flexible, construction of actual objects can be deferred etc.

##Image sources/effects
    Lumia.Imaging.Extras.ImageProviders/

####NoiseImageSource
A noise generator image source. Internally uses a ColorImageSource and a NoiseFilter.

####HighpassEffect 
A "highpass" effect, similar to familiar ones in photo editing apps. 

####DepthofField
A set of high-level scenarios showing how to set up a "DoF" effect.

####HslAdjustmentEffect
Example of an effect that does higher level HSL adjustments, similar to familiar ones in photo editing apps. 
Allows adjustments of saturation and lightness around Master, Red, Green, Blue, Cyan, Magenta and Yellow channels.

##Utility code
    Lumia.Imaging.Extras.Utility/

####ImageProviderExtensions
- **GetBitmapAsync** overloads that help to reuse bitmaps.
- **Rotate** method, making it convenient to rotate an image provider.

####BitmapExtensions
- **CopyTo**/**ToWriteableBitmap**: Conversions to WriteableBitmap. These help in interactive scenarios, and can be useful for keeping work off the UI thread.

####BufferExtensions
- **AsBufferTask**: Conversion from Task&lt;IBuffer&gt; to a Lumia Imaging SDK IBufferProvider that can be passed into a BufferProviderImageSource. This can be useful to avoid having to await the task-of-buffer before setting up an image source.

####MaybeTask&lt;T&gt;
Value type that either holds a result **or** a task-of-result. This helps interactive app scenarios, keeping GC activity in check when dealing with mixed sync/async operations, as otherwise each new Task causes a heap allocation.

