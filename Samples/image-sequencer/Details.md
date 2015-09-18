## Architecture overview

The application consists of six views:

-   Most of the application functionality is in the **SequencePreviewPage** class. It displays the animation preview and also handles the image stabilization using the Lumia Imaging SDK **ImageAligner**. To export the previewed animation to the GIF file, it utilizes GifExporter, which in turn uses **GifEncoder** from the Lumia Imaging SDK.
-   **SequencesPage** is the opening view of the application in which example image sequences and saved GIF files are displayed.
-   **GifPage** displays a saved GIF animation.
-   **AboutPage** contains the application description and version info and is available in the menu of each view.
-   **SequenceCapturePage** and **VideoPreviewPage** both implement the sequence capture functionality. **SequenceCapturePage** uses the **LowLagPhotoSequenceCapture** of the WinRT MediaCapture API, while **VideoPreviewCapturePage** is a fallback solution for devices where sequence capture is not supported by the camera hardware. It implements the sequence capture by capturing video preview images in short intervals.

## Preview page

**Initialization and image stabilization**

The animation preview is implemented in the **SequencePreviewPage** class. When the **SequencePreviewPage OnNavigatedTo** method gets called, it receives a list of image StorageFiles as a parameter. In the case of the example sequence, an identifier is received. The ID is a file name suffix for the image resource files stored in the project Assets/Sequences directory. The file name syntax is sequence.[id].[sequence index].jpg. A list of StorageFileImageSources is then created out of the provided files so they can be used by the Lumia Imaging SDK APIs.

The list of StorageFileImageSources gets passed to the **PrepareImageSequence** method. The **PrepareImageSequence** method prepares the image sequence playback by creating an aligned copy of the image sequence by using the Image Aligner API. The application can then display either unaligned or aligned bitmaps in real time by rendering images from either the list of unaligned image providers, or the list of aligned image providers.

C#  
```
public async Task PrepareImageSequence(List<IImageProvider> imageProviders)
{

    _unalignedImageProviders = imageProviders;
    _onScreenImageProviders = _unalignedImageProviders;

    // ...

    using (ImageAligner imageAligner = new ImageAligner())
    {
        imageAligner.Sources = _unalignedImageProviders;
        imageAligner.ReferenceSource = _unalignedImageProviders[0];
        try
        {
            _alignedImageProviders = await imageAligner.AlignAsync();

            // ...
        }
        catch (Exception e)
        {
            // If align fails, fail silently but don't enable the align button on UI.
        }
    };
}
```

### Animation

**DispatcherTimer** is used for timing the animation. The timer is created in the **SequencePreviewPage** constructor and is controlled in **Play** and **Stop** methods. When the timer is running, it calls the **AnimationTimer\_Tick** method at 100 millisecond intervals. The **AnimationTimer\_Tick** method calls the **Render** method to render the current frame and advances the index of the displayed frame, wrapping it when necessary.

C#  
```
private void AnimationTimer_Tick(object sender, EventArgs eventArgs)
{
    Render(_animationIndex);

    if (_animationIndex == (_onScreenImageProviders.Count() - 1))
    {
        _animationIndex = 0;
    }
    else
    {
        _animationIndex++;
    }
}
```

In the **Render** method, the **IImageProvider** is rendered to the on-screen bitmap.

C#  
```
private void Render(IReadOnlyList<IImageProvider> imageProviders, int animationIndex, bool renderBackground = false)
{
    if (_renderTask == null || _renderTask.IsCompleted)
    {
        _renderTask = DoRender(imageProviders, animationIndex, renderBackground);
    }
}

private async Task DoRender(IReadOnlyList<IImageProvider> imageProviders, int animationIndex, bool renderBackground = false)
{
    if (_onScreenImageProviders[animationIndex] != null)
    {
        int imageWidth = imageProviders == _unalignedImageProviders ? _unalignedImageWidth : _alignedImageWidth;
        int imageHeight = imageProviders == _unalignedImageProviders ? _unalignedImageHeight : _alignedImageHeight;
        if (_foregroundBitmap == null || _foregroundBitmap.PixelWidth != imageWidth || _foregroundBitmap.PixelHeight != imageHeight)
            _foregroundBitmap = new WriteableBitmap(imageWidth, imageHeight);
        using (WriteableBitmapRenderer writeableBitmapRenderer = new WriteableBitmapRenderer(imageProviders[animationIndex], _foregroundBitmap))
        {
            _foregroundBitmap = await writeableBitmapRenderer.RenderAsync();
        }
        if (renderBackground)
        {
            if (_backgroundBitmap == null || _backgroundBitmap.PixelWidth != imageWidth || _backgroundBitmap.PixelHeight != imageHeight)
                _backgroundBitmap = new WriteableBitmap(imageWidth, imageHeight);
            using (WriteableBitmapRenderer writeableBitmapRenderer = new WriteableBitmapRenderer(imageProviders[0], _backgroundBitmap))
            {
                _backgroundBitmap = await writeableBitmapRenderer.RenderAsync();
            }
        }
        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                _foregroundBitmap.Invalidate();
                ImageElement.Source = _foregroundBitmap;
                if (renderBackground)
                {
                    _backgroundBitmap.Invalidate();
                    ImageElementBackground.Source = _backgroundBitmap;
                }
            });
    }
}
```

### Setting the animation boundaries

The animated area selection works by having the animation in two layers: background and foreground. In the preview page, this works by having two overlapping Image elements, **ImageElementBackground** and **ImageElement**. **ImageElementBackground** displays the first frame of the sequence. **ImageElement** displays the current frame, cropped to the selected area by using the Clip property of **ImageElement**. During animation playback, only **ImageElement** is refreshed.

![](../Images/samples/layers.png)

The user can adjust the cropped area by dragging the canvas. The drag events are handled in the **ImageElement\_ManipulationDelta** method. The animated area is indicated by drawing a white rectangle on top of the image elements.

## Exporting animated GIF files

When user taps the save icon, the previewed animation is exported as an animated GIF file. The GIF export happens in the **GifExporter** class. It takes a list of IImageProviders as parameter, as well as an optional Rect representing the animated area.

The file is saved to the Pictures library. This requires the Pictures library capability to be set in the package manifest. The saved files are available for viewing and sharing in the Photos app, though the Photos image viewer does not play back the animation. The saved files are also accessible through the device file system. Note that you cannot use the MediaLibrary API to store the GIF files to the camera roll because it converts the GIF files to still JPEG images.

C#  
```
public static async Task Export(IReadOnlyList<IImageProvider> images, Rect? animatedArea)
{
    // The list of aligned images may contain Null items if some images couldn't be aligned.
    List<IImageProvider> sanitizedImages = new List<IImageProvider>();
    foreach (IImageProvider image in images) {
        if (image != null) {
            sanitizedImages.Add(image);
        }
    }

    ImageProviderInfo info = await sanitizedImages[0].GetInfoAsync();
    int w = (int)info.ImageSize.Width;
    int h = (int)info.ImageSize.Height;

    IReadOnlyList<IImageProvider> gifRendererSources;

    if (animatedArea.HasValue)
    {
        // Ensure the animated area dimensions are smaller than the image dimensions.
        double rectW = animatedArea.Value.Width;
        double rectH = animatedArea.Value.Height;
        if ((animatedArea.Value.Width + animatedArea.Value.Left) >= w)
        {
            rectW = w - animatedArea.Value.Left - 1;
        }
        if ((animatedArea.Value.Top + animatedArea.Value.Height) >= h)
        {
            rectH = h - animatedArea.Value.Top - 1;
        }
        Rect rect = new Rect(animatedArea.Value.Left, animatedArea.Value.Top, rectW, rectH);
        gifRendererSources = CreateFramedAnimation(sanitizedImages, rect, w, h);
    }
    else {
        gifRendererSources = sanitizedImages;
    }

    using (GifRenderer gifRenderer = new GifRenderer())
    {
        gifRenderer.Duration = 100;
        gifRenderer.NumberOfAnimationLoops = 10000;
        gifRenderer.Sources = gifRendererSources;
        var buffer = await gifRenderer.RenderAsync();
        var filename = "Sequence" + (await GetFileNameRunningNumber()) + ".gif";
        var storageFile = await KnownFolders.SavedPictures.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
        using (var stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
        {
             await stream.WriteAsync(buffer);
        }
    }
 }
```

If the animated area is defined, **GifExporter** creates a new list of IImageProviders that contain images in which only the animated area changes. This is implemented by using **CropFilter** to crop the animated area, and then using **BlendFilter** to blend it on top of the static background.

C#  
```
private static IReadOnlyList<IImageProvider> CreateFramedAnimation(IReadOnlyList<IImageProvider> images, Rect animationBounds, int w, int h)
{
    List<IImageProvider> framedAnimation = new List<IImageProvider>();
    foreach (IImageProvider frame in images)
    {
        FilterEffect cropFilterEffect = new FilterEffect(frame);
        cropFilterEffect.Filters = new List<IFilter>() { new CropFilter(animationBounds) };
        FilterEffect blendFilterEffect = new FilterEffect(images[0]);
        BlendFilter blendFilter = new BlendFilter();
        blendFilter.ForegroundSource = cropFilterEffect;
        blendFilter.Level = 1.0;
        blendFilter.BlendFunction = BlendFunction.Normal;
        blendFilter.TargetArea = new Rect(
            animationBounds.Left / w,
            animationBounds.Top / h,
            animationBounds.Width / w,
            animationBounds.Height / h
        );
        blendFilterEffect.Filters = new List<IFilter>() { blendFilter };
        framedAnimation.Add(blendFilterEffect);
     }
     return framedAnimation;
 }
```

## Capturing image sequences

### Initialization

Sequence capture is implemented in the **SequenceCapturePage** class. A **MediaCapture** object for controlling the phone back camera is initialized in the **InitializeMediaCapture** method, called in the **OnNavigatedTo** method of the page.

C#  
```
// ...

_mediaCapture = new MediaCapture();
var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
var backCamera = devices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Back);

await _mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
{
    StreamingCaptureMode = StreamingCaptureMode.Video,
    PhotoCaptureSource = PhotoCaptureSource.Auto,
    AudioDeviceId = string.Empty,
    VideoDeviceId = backCamera.Id
});

// ...
```

The **SequenceCapturePage** XAML tree contains a **CaptureElement** that is used to display the camera viewfinder image, so you must set the previously initialized **MediaCapture** object as the source of **CaptureElement**, and start the preview:

C#  
```
// ...

captureElement.Source = _mediaCapture;
await _mediaCapture.StartPreviewAsync();

// ...
```

The **MediaCapture.PrepareLowLagPhotoSequenceCaptureAsync** method is then used to initialize the photo sequence capture. It returns a **LowLagPhotoSequenceCapture** object, which can be used to control the sequence capture.

C#  
```
// ...

var format = ImageEncodingProperties.CreateJpeg();
format.Width = 640;
format.Height = 480;

_variablePhotoSequenceCapture = await _mediaCapture.PrepareLowLagPhotoSequenceCaptureAsync(format);

// ...
```

A handler is added for the PhotoCaptured event that is triggered on each captured image. Whenever a shot is taken, it's saved to a temporary folder to be used later. In Image Sequencer, the number of captured images is limited to 20 due to memory usage concerns on low-end devices.

C#  
```
// ...

_variablePhotoSequenceCapture.PhotoCaptured += OnPhotoCaptured;

// ...

public void OnPhotoCaptured(LowLagPhotoSequenceCapture s, PhotoCapturedEventArgs e)
{
     if (_fileIndex < AMOUNT_OF_FRAMES_IN_SEQUENCE)
     {
         if (_saveTask == null)
         {
             _saveTask = Save(e.Frame, _fileIndex++);
         }
         else
         {
             _saveTask = _saveTask.ContinueWith(t => Save(e.Frame, _fileIndex++));
         }
     }
     else
     {
         StopSequenceCapture();
     }
}

private async Task Save(IRandomAccessStream frame, int i) {
     var filename = "ImageSequencer." + i + ".jpg";
     var folder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
     var storageFile = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
     var stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite);
     await RandomAccessStream.CopyAndCloseAsync(frame, stream);
     _files.Add(storageFile);
}
```

The photo sequence capture can be started and stopped using the **StartAsync** and **StopAsync** methods of **LowLagPhotoSequenceCapture**. Use **FinishAsync** to release the resources used by the photo sequence operation. These methods are used in the event handlers of the hardware camera button and the on-screen capture button.

### Using video preview images for capturing image sequences

Not all devices support photo sequence capture. You can use the **MediaCapture.VideoDeviceController.LowLagPhotoSequence.Supported** property to verify whether sequence capture is supported by the device running your application. For unsupported devices, you can implement a fall-back by using video preview to capture sequences of images. This can be done easily using **CameraPreviewImageSource** in the Lumia Imaging SDK.

The **CameraPreviewImageSource** is initialized similarly to the **LowLagPhotoSequenceCapture**:

C#  
```
public async void InitializeAsync()
{
    // Create a camera preview image source (from Imaging SDK).
    _cameraPreviewImageSource = new CameraPreviewImageSource();
    await _cameraPreviewImageSource.InitializeAsync(string.Empty);
    var properties = await _cameraPreviewImageSource.StartPreviewAsync();

    // Create a preview bitmap with the correct aspect ratio.
    var width = 640.0;
    var height = (width / properties.Width) * properties.Height;
    _writeableBitmap = new WriteableBitmap((int)width, (int)height);

    captureElement.Source = _writeableBitmap;

    _writeableBitmapRenderer = new WriteableBitmapRenderer();
    _jpegRenderer = new JpegRenderer();

    // Attach preview frame delegate.
    _cameraPreviewImageSource.PreviewFrameAvailable += OnPreviewFrameAvailable;
}
```

Instead of using **CaptureElement** to display the viewfinder, a regular image control is used to display the images provided by the **CameraPreviewImageSource**. If sequence capture is ongoing, the image is saved to JPG file using the Lumia Imaging SDK **JpegRenderer**.

C#  
```
private void OnPreviewFrameAvailable(IImageSize args)
{
    _renderTask = Render();
}

private async Task Render()
{

    if (!_rendering && !_stop)
    {
        _rendering = true;

        // Render the camera preview frame to the screen.
        _writeableBitmapRenderer.Source = _cameraPreviewImageSource;
        _writeableBitmapRenderer.WriteableBitmap = _writeableBitmap;
        await _writeableBitmapRenderer.RenderAsync();
        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
            CoreDispatcherPriority.High, () =>
            {
                _writeableBitmap.Invalidate();
            });

        // Write the camera preview frame to a file if capturing.
        if (_capturing)
        {
            if (_sequenceIndex < 20)
            {
                _jpegRenderer.Source = _cameraPreviewImageSource;
                IBuffer jpg = await _jpegRenderer.RenderAsync();
                await Save(jpg, _sequenceIndex++);
            }
            else
            {
                StartStopCapture();
            }
        }
        _rendering = false;
    }

    if (_stop)
    {
        _capturing = false;
        _cameraPreviewImageSource.Dispose();
        _writeableBitmapRenderer.Dispose();
        _jpegRenderer.Dispose();
    }
}
```

## Viewing animated GIF files

No standard XAML control exists for displaying animated GIFs, so you have to to decode the GIF frames manually by using BitmapDecoder. This is done in the **GifPage** class.

**BitmapDecoder** provides us access to the animation frames in a GIF file. The GIFs generated by the Lumia Imaging SDK uses partial GIF encoding, meaning that the animation frames contain only pixels different from the corresponding pixels in the first frame. Hence, in addition to decoding the frames, you must overlay each frame on top of the first frame to complete the image. This is done by examining the RGBA values of the pixels. If the value of the alpha channel is zero, you must replace the pixel using the corresponding pixel in the first frame.

**Note**: The overlay procedure is not needed for animated GIFs not using partial encoding.

C#  
```
private async Task LoadImage(String filename)
{
    var storageFile = await KnownFolders.SavedPictures.GetFileAsync(filename);
    using (var res = await storageFile.OpenAsync(FileAccessMode.Read))
    {
        var bitmapDecoder = await BitmapDecoder.CreateAsync(BitmapDecoder.GifDecoderId, res);
        byte[] firstFrame = null;
        for (uint frameIndex = 0; frameIndex < bitmapDecoder.FrameCount; frameIndex++)
        {
            var frame = await bitmapDecoder.GetFrameAsync(frameIndex);
            var writeableBitmap = new WriteableBitmap((int)bitmapDecoder.OrientedPixelWidth, (int)bitmapDecoder.OrientedPixelHeight);
            var pixelData = await frame.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Straight,
                new BitmapTransform(),
                ExifOrientationMode.RespectExifOrientation,
                ColorManagementMode.DoNotColorManage);
            var bytes = pixelData.DetachPixelData();
            if (firstFrame == null)
            {
                firstFrame = bytes;
            }
            else
            {
                // Blend the frame on top of the first frame.
                for (uint i = 0; i < bytes.Count(); i += 4)
                {
                    int alpha = bytes[i + 3];
                    if (alpha == 0 && firstFrame != null)
                    {
                        Array.Copy(firstFrame, i, bytes, i, 4);
                    }
                }
            }
            using (var stream = writeableBitmap.PixelBuffer.AsStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            _frames.Add(writeableBitmap);
        }
    }
    StartAnimation();
}
```
