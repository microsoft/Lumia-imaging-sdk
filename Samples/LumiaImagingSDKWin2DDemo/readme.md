Lumia Imaging SDK Win2D Demo
---------------------

This sample demonstrates  how to use **Lumia Imaging Effects** together with **Win2D Effects**. The sample covers two scenarios. First it shows how to interop between Lumia Imaging effects and Win2D effects using SoftwareBitmap. 
Second scenario demonstrates how to use a IDirect3DSurface to interop between the to  SDK´s.  

Specifically, this sample will cover how to:

1. **Create BasicVideoEffect** That uses IDirectD3Surface to interop between Lumia Imaging **GrayscaleEffects** and **Win2D DisplacemantmapEffect**.
2. **Use Lumia Imaging SDK to add SepiaEffect** and then use Win2D to draw Text, lines, rectangles and circles. Interop is done with SoftwareBitmap. Switch between Front and back camera.



## Related topics

**Samples**


[UniversalCameraSample](https://github.com/Microsoft/Windows-universal-samples/tree/master/universalcamerasample)

[How to preview video from a webcam](https://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh868171.aspx)

[Media capture using capture device](https://code.msdn.microsoft.com/windowsapps/Media-Capture-Sample-adf87622)

**Reference**

[Lumia Imaging SDK](https://dev.windows.com/en-us/featured/lumia)

[Windows.Media.Capture.MediaCapture namespace](https://msdn.microsoft.com/en-us/library/windows/apps/windows.media.devices.aspx)

[Windows.Media.Capture.MediaCaptureInitializationSettings constructor](https://msdn.microsoft.com/en-us/library/windows/apps/windows.media.capture.mediacaptureinitializationsettings.mediacaptureinitializationsettings.aspx) 

[Windows.Media.Capture.MediaCaptureInitilizationSettings.VideoDeviceId property](https://msdn.microsoft.com/en-us/library/windows/apps/windows.media.capture.mediacaptureinitializationsettings.videodeviceid.aspx)


## System requirements

**Hardware:** Camera

**Client:** Windows 10 

**Server:** Windows 10 

**Phone:**  Windows 10 Technical Preview

## Build the sample

1.  Start Visual Studio 2015 and select **File** \> **Open** \> **Project/Solution**.
2.  Press Ctrl+Shift+B, or select **Build** \> **Build Solution**.

## Run the sample

The next steps depend on whether you just want to deploy the sample or you want to both deploy and run it.

**Deploying the sample:**

1.  Select **Build** \> **Deploy Solution**.

**Deploying and running the sample:**

1.  To debug the sample and then run it, press F5 or select **Debug** \> **Start Debugging**. To run the sample without debugging, press Ctrl+F5 or select **Debug** \> **Start Without Debugging**.


License
-------
See the **license.txt** file delivered with this project

