//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using Lumia.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Lumia.Imaging.Adjustments;
using System.Linq;
using Lumia.Imaging.Transforms;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.Foundation.Collections;
using Windows.Media;
using System.Collections.ObjectModel;
using Lumia.Imaging.Artistic;
using Lumia.Imaging.Compositing;
using Windows.Foundation.Metadata;
using Lumia.Imaging.VideoEffectSample.Utilities;
using Windows.Graphics.Display;
using Windows.Devices.Sensors;
using Windows.System.Display;
using Windows.Phone.UI.Input;
using Windows.Devices.Enumeration;
using Windows.Storage.FileProperties;
using Windows.Graphics.Imaging;

namespace Lumia.Imaging.VideoEffectSample.ViewModels
{

    // Camera functionallity is inspired from  https://github.com/Microsoft/Windows-universal-samples/tree/master/universalcamerasample
   
    public class MainViewModel : ViewModelBase
    {
#region Fields
        private string m_currentState;
        private bool m_isRecording;
        private bool m_isRecordingEnabled;
        private bool m_isCaptureEnabled;
        private PropertySet m_configurationPropertySet;
        public CaptureElement m_PreviewVideoElement;
        private MediaCapture m_mediaCapture;
        protected ImageSource m_cameraImageSource;
        private string m_cameraSettings;
        private bool m_isEditing;
        private bool m_isStaticEffectVisible;
        private VideoEffectsViewModel m_currentEditor;
        private VideoEffectsViewModel m_currentStaticEditor;
        private double m_ImageWidth;
        private double m_ImageHeight;
        private Windows.Devices.Enumeration.Panel m_desiredCameraPanel;
        private bool m_isToggelingCamera;

        // Receive notifications about rotation of the device and UI and apply any necessary rotation to the preview stream and UI controls       
        private readonly DisplayInformation m_displayInformation = DisplayInformation.GetForCurrentView();
        private readonly SimpleOrientationSensor m_orientationSensor = SimpleOrientationSensor.GetDefault();
        private SimpleOrientation m_deviceOrientation = SimpleOrientation.NotRotated;
        private DisplayOrientations m_displayOrientation = DisplayOrientations.Portrait;

        // Rotation metadata to apply to the preview stream and recorded videos (MF_MT_VIDEO_ROTATION)
        // Reference: http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh868174.aspx
        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");

        // Prevent the screen from sleeping while the camera is running
        private readonly DisplayRequest _displayRequest = new DisplayRequest();
        private bool m_mirroringPreview;
        private bool m_externalCamera;
        private bool m_hasFrontAndBackCamera;
        
        private IImageProvider m_imageProviderEffect;
        private IImageProvider m_imageConsumerEffect;
        private ContrastEffect m_contrastEffect;
        private BrightnessEffect m_brightnessEffect;       
        private HueSaturationEffect m_hueSaturationEffect;
        private AntiqueEffect m_antiqueEffect;
        private BlendEffect m_blendEffect;
        private SepiaEffect m_sepiaEffect;
        private MirrorEffect m_mirrorEffect;
        private GrayscaleEffect m_grayscaleEffect;

        public DelegateCommand<FrameworkElement> PreviewVideoElementLoadedCommand { get; set; }
        public DelegateCommand StartStopRecordCommand { get; set; }
        public DelegateCommand CaptureCommand { get; set; }
        public DelegateCommand ToggleCameraCommand { get; set; }
        public ObservableCollection<VideoEffectsViewModel> Effects { get; private set; }
        public ObservableCollection<VideoEffectsViewModel> NonEditableEffects { get; private set; }

#endregion

        public MainViewModel()
        {
            PreviewVideoElementLoadedCommand = new DelegateCommand<FrameworkElement>(OnFrameworkElementLoaded);
            m_configurationPropertySet = new PropertySet();

            Application.Current.Suspending += OnApplicationSuspending;
            Application.Current.Resuming += OnApplicationResuming;
            SystemMediaTransportControls.GetForCurrentView().PropertyChanged += OnMediaPropertyChanged;

            StartStopRecordCommand = new DelegateCommand(OnStartStopRecord, OnCanExecutePlayRecord);
            CaptureCommand = new DelegateCommand(OnCaptureImage, CanExecuteCaptureImage);
            ToggleCameraCommand = new DelegateCommand(OnToggleCamera, CanExecuteToggleCamera);

            Effects = CreateEditableEffects();
            NonEditableEffects = CreateNonEditableEffects();

            m_imageConsumerEffect = m_contrastEffect;
            m_imageProviderEffect = m_brightnessEffect;

            m_desiredCameraPanel = Windows.Devices.Enumeration.Panel.Back;

            UpdateConfiguration();

            CurrentState = "Normal";
            CurrentEditor = Effects.FirstOrDefault();

            IsRecordingEnabled = true;
            m_externalCamera = false;
            m_isToggelingCamera = false;
        }

#region Properties

        public ImageSource CameraImageSource
        {
            get { return m_cameraImageSource; }
            set { SetProperty(ref m_cameraImageSource, value); }
        }

        public string CameraSettings
        {
            get { return m_cameraSettings; }
            set { SetProperty(ref m_cameraSettings, value); }
        }
        public Uri VideoRecordingIcon
        {
            get
            {
                if (m_isRecording)
                {
                    return new Uri("ms-appx:///Icons/stop.png");
                }

                return new Uri("ms-appx:///Icons/feature.video.png");
            }
        }

        public double ImageWidth
        {
            get { return m_ImageWidth; }
            set { SetProperty(ref m_ImageWidth, value); }
        }

        public double ImageHeight
        {
            get { return m_ImageHeight; }
            set { SetProperty(ref m_ImageHeight, value); }
        }
        public string CurrentState
        {
            get { return m_currentState; }
            set { SetProperty(ref m_currentState, value); }
        }

        public VideoEffectsViewModel CurrentEditor
        {
            get { return m_currentEditor; }
            set
            {
                SetProperty(ref m_currentEditor, value);
            }
        }

        public VideoEffectsViewModel CurrentStaticEditor
        {
            get { return m_currentStaticEditor; }
            set
            {
                SetProperty(ref m_currentStaticEditor, value);
                UpdateConfiguration();
            }
        }       
        public bool IsEditing
        {
            get { return m_isEditing; }
            set
            {
                if (m_isEditing == value)
                    return;

                IsStaticEffectVisible = false;
                SetProperty(ref m_isEditing, value);
                CurrentState = m_isEditing ? "Editing" : "Normal";

                if (m_isEditing)
                {
                    CurrentStaticEditor = null;
                    UpdateConfiguration();
                }
            }
        }
        public bool IsStaticEffectVisible
        {
            get { return m_isStaticEffectVisible; }
            set
            {
                if (m_isStaticEffectVisible == value)
                    return;

                IsEditing = false;
                SetProperty(ref m_isStaticEffectVisible, value);
                CurrentState = m_isStaticEffectVisible ? "StaticEditing" : "EndStaticEditing";

                if (m_isStaticEffectVisible)
                {
                    UpdateConfiguration();
                }
            }
        }
        public string RecordingStateText
        {
            get { return m_isRecording ? "Stop" : "Start"; }
        }

        public bool IsRecordingEnabled
        {
            get { return m_isRecordingEnabled; }
            set
            {
                SetProperty(ref m_isRecordingEnabled, value);
                UpdateButtonState();
            }
        }
        public bool IsRecording
        {
            get { return m_isRecording; }
            set
            {
                SetProperty(ref m_isRecording, value);
                CurrentState = m_isRecording ? "Recording" : "RecordingStopped";
                OnPropertyChanged(() => RecordingStateText);
                OnPropertyChanged(() => VideoRecordingIcon);

                UpdateButtonState();
            }
        }

        private bool IsCaptureEnabled
        {
            get { return m_isCaptureEnabled; }
            set
            {
                m_isCaptureEnabled = value;
                UpdateButtonState();
            }
        }

#endregion

        private bool CanExecuteToggleCamera()
        {
            return !m_isToggelingCamera && m_hasFrontAndBackCamera;
        }

        private async void OnToggleCamera()
        {
            m_isToggelingCamera = true;
            UpdateButtonState();
            await StopPreviewAsync();
            if (m_desiredCameraPanel == Windows.Devices.Enumeration.Panel.Back)
            {
                m_desiredCameraPanel = Windows.Devices.Enumeration.Panel.Front;
            }
            else
            {
                m_desiredCameraPanel = Windows.Devices.Enumeration.Panel.Back;
            }
            await StartPreviewAsync();
            m_isToggelingCamera = false;
            UpdateButtonState();
        }

        private async void OnCaptureImage()
        {
            await CaptureImageAsync();
        }

        private bool CanExecuteCaptureImage()
        {
            return IsCaptureEnabled && !m_isToggelingCamera;
        }

        private async void OnStopPreview()
        {
            await StopPreviewAsync();
        }

        async void OnApplicationResuming(object sender, object e)
        {
            await StartPreviewAsync();
        }

        private async Task StopPreviewAsync()
        {
            await CloseAsync();
        }

        public bool OnCanExecutePlayRecord()
        {
            return IsRecordingEnabled && !m_isToggelingCamera;
        }       

        private ObservableCollection<VideoEffectsViewModel> CreateNonEditableEffects()
        {
            var effects = new ObservableCollection<VideoEffectsViewModel>();

            m_antiqueEffect = new AntiqueEffect();
            effects.Add(new VideoEffectsViewModel(m_antiqueEffect, "antique_thumbnail.jpg") { Name = "Antique" });

            m_sepiaEffect = new SepiaEffect();
            effects.Add(new VideoEffectsViewModel(m_sepiaEffect, "sepia_thumbnail.jpg") { Name = "Sepia" });

            m_blendEffect = new BlendEffect() { GlobalAlpha = 0.5 };

            effects.Add(new VideoEffectsViewModel(m_blendEffect, "blend_brokenglass_thumbnail.jpg", "BrokenGlas.png") { Name = "Blend 1" });

            var effect = new BlendEffect() { BlendFunction = BlendFunction.Overlay, GlobalAlpha = 0.5 };

            effects.Add(new VideoEffectsViewModel(effect, "blend_raindrops_thumbnail.jpg", "raindrops.jpg") { Name = "Blend 2" });

            m_mirrorEffect = new MirrorEffect();
            effects.Add(new VideoEffectsViewModel(m_mirrorEffect, "mirror_thumbnail.jpg") { Name = "Mirror" });

            m_grayscaleEffect = new GrayscaleEffect();
            effects.Add(new VideoEffectsViewModel(m_grayscaleEffect, "grayscale_thumbnail.jpg") { Name = "Grayscale" });

            return effects;
        }

        private ObservableCollection<VideoEffectsViewModel> CreateEditableEffects()
        {
            var effects = new ObservableCollection<VideoEffectsViewModel>();

            m_contrastEffect = new ContrastEffect();
            effects.Add(new VideoEffectsViewModel(m_contrastEffect, "contrast.png") { Name = "Contrast", MaxValue = 1.0, MinValue = -1.0 });
            
            m_hueSaturationEffect = new HueSaturationEffect(m_contrastEffect);

            effects.Add(new VideoEffectsViewModel(m_hueSaturationEffect, "saturation.png") { Name = "Hue", MaxValue = 1.0, MinValue = -1.0, PropertyName = "Hue" });
            effects.Add(new VideoEffectsViewModel(m_hueSaturationEffect, "saturation.png") { Name = "Saturation", MaxValue = 1.0, MinValue = -1.0, PropertyName = "Saturation" });

            m_brightnessEffect = new BrightnessEffect(m_hueSaturationEffect);
            effects.Add(new VideoEffectsViewModel(m_brightnessEffect, "brightness.png") { Name = "Brightness", MaxValue = 1.0, MinValue = -1.0 });

            return effects;
        }
            
        private async void OnMediaPropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            if (args.Property == SystemMediaTransportControlsProperty.SoundLevel)
            {
                await TaskUtilities.RunOnDispatcherThreadAsync(async () =>
                {
                    if (sender.SoundLevel != Windows.Media.SoundLevel.Muted)
                    {
                         await StartPreviewAsync();
                    }
                    else
                    {
                        await CloseAsync();
                    }
                });
            }
        }
       
        private async System.Threading.Tasks.Task StartRecordAsync()
        {
            try
            {
                IsRecording = true;
                //if camera does not support lowlag record and lowlag photo at the same time enable TakePhoto button after recording
                IsCaptureEnabled = m_mediaCapture.MediaCaptureSettings.ConcurrentRecordAndPhotoSupported;
                
                string fileName = "LumiaImagingVideo.mp4";

                var recordStorageFile = await KnownFolders.PicturesLibrary.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.GenerateUniqueName);

                MediaEncodingProfile recordProfile = MediaEncodingProfile.CreateMp4(Windows.Media.MediaProperties.VideoEncodingQuality.Auto);

                // Calculate rotation angle, taking mirroring into account if necessary
                var rotationAngle = 360 - ConvertDeviceOrientationToDegrees(GetCameraOrientation());
                recordProfile.Video.Properties.Add(RotationKey, PropertyValue.CreateInt32(rotationAngle));

                Debug.WriteLine("Starting recording...");

                await m_mediaCapture.StartRecordToStorageFileAsync(recordProfile, recordStorageFile);
            }
            catch (Exception exception)
            {
                Debug.Assert(true, string.Format("Failed to start record video. {0}", exception.Message));
                IsRecording = false;
            }
        }

        private async void OnStartStopRecord()
        {   
            if (!IsRecording)
            {
                await StartRecordAsync();
            }
            else
            {
                await StopRecordAsync();
            }
        }

        private async System.Threading.Tasks.Task StopRecordAsync()
        {
            try
            {
                await m_mediaCapture.StopRecordAsync();
            }
            catch(Exception)
            { }

            IsRecording = false;
            IsCaptureEnabled = true;           
        }
     
        protected virtual async Task CloseAsync()
        {
            _displayRequest.RequestRelease();

            if (m_mediaCapture == null)
                return;

            UnregisterEventHandlers();
            m_mediaCapture.RecordLimitationExceeded -= OnMediaCaptureRecordLimitationExceeded;
            m_mediaCapture.Failed -= OnMediaCaptureFailed;

            try
            {
                await m_mediaCapture.StopPreviewAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (m_isRecording)
            {
                await m_mediaCapture.StopRecordAsync();
            }

            m_mediaCapture.Dispose();
            m_mediaCapture = null;

        }
        async void OnApplicationSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            if (m_mediaCapture != null)
              await CloseAsync();

            deferral.Complete();
        }      

        private async void OnFrameworkElementLoaded(FrameworkElement frameworkElement)
        {
            m_PreviewVideoElement = (CaptureElement)frameworkElement;
            await StartPreviewAsync();
        }
        private async Task StartPreviewAsync()
        {
            try
            {
                m_mediaCapture = new MediaCapture();

                m_mediaCapture.Failed += OnMediaCaptureFailed;           

                _displayRequest.RequestActive();

                // Populate orientation variables with the current state
                m_displayOrientation = m_displayInformation.CurrentOrientation;

                RegisterEventHandlers();

                m_mediaCapture.RecordLimitationExceeded += OnMediaCaptureRecordLimitationExceeded;

                var devInfo = await FindCameraDeviceByPanelAsync(m_desiredCameraPanel);
                var id = devInfo != null ? devInfo.Id : string.Empty;

                var settings = new MediaCaptureInitializationSettings();
                settings.VideoDeviceId = id;
                settings.MediaCategory = MediaCategory.Communications;

                await m_mediaCapture.InitializeAsync(settings);

                m_mediaCapture.SetEncoderProperty(MediaStreamType.VideoPreview, new Guid("9C27891A-ED7A-40e1-88E8-B22727A024EE"), PropertyValue.CreateUInt32(1));

                var resolutionMax = GetHighestResolution();

                ImageWidth = resolutionMax.Width;
                ImageHeight = resolutionMax.Height;

                await m_mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, resolutionMax);

                m_PreviewVideoElement.Source = m_mediaCapture;
                await AddEffectsAsync();

                await m_mediaCapture.StartPreviewAsync();

                await SetPreviewRotationAsync();

                IsCaptureEnabled = true;

                UpdateButtonState();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void OnMediaCaptureFailed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            try
            {
                await CloseAsync();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void UpdateConfiguration()
        {
            m_configurationPropertySet.Clear();

            if (CurrentStaticEditor != null)
            {
                m_configurationPropertySet.Add(new KeyValuePair<string, object>("Effect", CurrentStaticEditor.Effect));
            }
            else
            {
                var effectList = new EffectList(Effects.Select(evm => evm.Effect).Distinct());
                m_configurationPropertySet.Add(new KeyValuePair<string, object>("Effect", effectList));
            }
        }
        private async Task AddEffectsAsync()
        {
            UpdateConfiguration();

            var videoEffectDefinition = new Windows.Media.Effects.VideoEffectDefinition("Lumia.Imaging.VideoEffect", m_configurationPropertySet);

            await m_mediaCapture.ClearEffectsAsync(MediaStreamType.VideoPreview);
            await m_mediaCapture.ClearEffectsAsync(MediaStreamType.VideoRecord);
            await m_mediaCapture.ClearEffectsAsync(MediaStreamType.Photo);

            await m_mediaCapture.AddVideoEffectAsync(videoEffectDefinition, MediaStreamType.VideoPreview);

            switch (m_mediaCapture.MediaCaptureSettings.VideoDeviceCharacteristic)
            {
                case VideoDeviceCharacteristic.AllStreamsIndependent:
                    await m_mediaCapture.AddVideoEffectAsync(videoEffectDefinition, MediaStreamType.VideoRecord);
                    await m_mediaCapture.AddVideoEffectAsync(videoEffectDefinition, MediaStreamType.Photo);
                    break;
                case VideoDeviceCharacteristic.PreviewPhotoStreamsIdentical:
                case VideoDeviceCharacteristic.RecordPhotoStreamsIdentical:
                    await m_mediaCapture.AddVideoEffectAsync(videoEffectDefinition, MediaStreamType.VideoRecord);
                    break;
                case VideoDeviceCharacteristic.PreviewRecordStreamsIdentical:
                    await m_mediaCapture.AddVideoEffectAsync(videoEffectDefinition, MediaStreamType.Photo);
                    break;

            }

        }

        private void UpdateButtonState()
        {
            if (CaptureCommand == null)
                return;

            CaptureCommand.NotifyCanExecuteChanged();
            StartStopRecordCommand.NotifyCanExecuteChanged();
            ToggleCameraCommand.NotifyCanExecuteChanged();
        }

        private async Task CaptureImageAsync()
        {
            IsCaptureEnabled = false;

            try
            {
                //if camera does not support record and Takephoto at the same time disable Record button when taking photo
                IsRecordingEnabled = m_mediaCapture.MediaCaptureSettings.ConcurrentRecordAndPhotoSupported;

                ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();

                var stream = new InMemoryRandomAccessStream();

                await m_mediaCapture.CapturePhotoToStreamAsync(imageProperties, stream);
                

                await SaveToPicturesLibraryAsync(stream);

                IsRecordingEnabled = true;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Failed to capture image. {0}", ex.Message));
            }

            IsCaptureEnabled = true;
        }      

        public async Task SaveToPicturesLibraryAsync(InMemoryRandomAccessStream stream)
        {
            try
            {
                var rotationAngle = ConvertDeviceOrientationToDegrees(GetCameraOrientation());
                IsCaptureEnabled = false;
                var file = await KnownFolders.PicturesLibrary.CreateFileAsync("LumiaImagingCapturedImage.jpg", CreationCollisionOption.GenerateUniqueName);

                stream.Seek(0);
                var imageSource = new RandomAccessStreamImageSource(stream);
                using (var jpegRenderer = new JpegRenderer(imageSource))
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    if (rotationAngle != 0)
                    {
                        jpegRenderer.Source = new RotationEffect(imageSource, rotationAngle - 180); //Andjust to Lumia.Imaging Rotation
                    }
                    // Jpeg renderer gives the raw buffer containing the filtered image.
                    IBuffer jpegBuffer = await jpegRenderer.RenderAsync();
                    await fileStream.WriteAsync(jpegBuffer);
                    await fileStream.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            IsCaptureEnabled = true;
        }

        private async Task<DeviceInformation> FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel desiredPanel)
        {
            // Get available devices for capturing pictures
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            m_hasFrontAndBackCamera = allVideoDevices.Any(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front) &&
                allVideoDevices.Any(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Back);

            // Get the desired camera by panel
            DeviceInformation desiredDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredPanel);

            // If there is no device mounted on the desired panel, use the first device found
            desiredDevice = desiredDevice ?? allVideoDevices.FirstOrDefault();
            if (desiredDevice != null)
            {
                m_desiredCameraPanel = desiredPanel;
                m_mirroringPreview = desiredPanel == Windows.Devices.Enumeration.Panel.Front;
                if (desiredDevice.EnclosureLocation == null || desiredDevice.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Unknown)
                {
                    // No information on the location of the camera, assume it's an external camera, not integrated on the device
                    m_externalCamera = true;
                }
                else
                {
                    m_externalCamera = false;
                }
            }

            return desiredDevice;
        }

        private static int ConvertDeviceOrientationToDegrees(SimpleOrientation orientation)
        {
            switch (orientation)
            {
                case SimpleOrientation.Rotated90DegreesCounterclockwise:
                    return 90;
                case SimpleOrientation.Rotated180DegreesCounterclockwise:
                    return 180;
                case SimpleOrientation.Rotated270DegreesCounterclockwise:
                    return 270;
                case SimpleOrientation.NotRotated:
                default:
                    return 0;
            }
        }

        private async void OnDisplayInformationOrientationChanged(DisplayInformation sender, object args)
        {
            m_displayOrientation = sender.CurrentOrientation;

            await SetPreviewRotationAsync();
        }

        private async Task SetPreviewRotationAsync()
        {
            // Only need to update the orientation if the camera is mounted on the device
            if (m_externalCamera) return;

            // Calculate which way and how far to rotate the preview
            int rotationDegrees = ConvertDisplayOrientationToDegrees(m_displayOrientation);

            // The rotation direction needs to be inverted if the preview is being mirrored
            if (m_mirroringPreview)
            {
                rotationDegrees = (360 - rotationDegrees) % 360;
            }

            // Add rotation metadata to the preview stream to make sure the aspect ratio / dimensions match when rendering and getting preview frames
            var props = m_mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
            props.Properties.Add(RotationKey, rotationDegrees);
            await m_mediaCapture.SetEncodingPropertiesAsync(MediaStreamType.VideoPreview, props, null);
        }
        private SimpleOrientation GetCameraOrientation()
        {
            if (m_externalCamera)
            {
                // Cameras that are not attached to the device do not rotate along with it, so apply no rotation
                return SimpleOrientation.NotRotated;
            }

            var result = m_deviceOrientation;

            // Account for the fact that, on portrait-first devices, the camera sensor is mounted at a 90 degree offset to the native orientation
            if (m_displayInformation.NativeOrientation == DisplayOrientations.Portrait)
            {
                switch (result)
                {
                    case SimpleOrientation.Rotated90DegreesCounterclockwise:
                        result = SimpleOrientation.NotRotated;
                        break;
                    case SimpleOrientation.Rotated180DegreesCounterclockwise:
                        result = SimpleOrientation.Rotated90DegreesCounterclockwise;
                        break;
                    case SimpleOrientation.Rotated270DegreesCounterclockwise:
                        result = SimpleOrientation.Rotated180DegreesCounterclockwise;
                        break;
                    case SimpleOrientation.NotRotated:
                        result = SimpleOrientation.Rotated270DegreesCounterclockwise;
                        break;
                }
            }

            // If the preview is being mirrored for a front-facing camera, then the rotation should be inverted
            if (m_mirroringPreview)
            {
                // This only affects the 90 and 270 degree cases, because rotating 0 and 180 degrees is the same clockwise and counter-clockwise
                switch (result)
                {
                    case SimpleOrientation.Rotated90DegreesCounterclockwise:
                        return SimpleOrientation.Rotated270DegreesCounterclockwise;
                    case SimpleOrientation.Rotated270DegreesCounterclockwise:
                        return SimpleOrientation.Rotated90DegreesCounterclockwise;
                }
            }

            return result;
        }
        private static int ConvertDisplayOrientationToDegrees(DisplayOrientations orientation)
        {
            switch (orientation)
            {
                case DisplayOrientations.Portrait:
                    return 90;
                case DisplayOrientations.LandscapeFlipped:
                    return 180;
                case DisplayOrientations.PortraitFlipped:
                    return 270;
                case DisplayOrientations.Landscape:
                default:
                    return 0;
            }
        }
        private void RegisterEventHandlers()
        {
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.CameraPressed += OnHardwareButtonsCameraPressed;
            }
                        
            if (m_orientationSensor != null)
            {
                m_orientationSensor.OrientationChanged += OnOrientationSensorOrientationChanged;
            }

            m_displayInformation.OrientationChanged += OnDisplayInformationOrientationChanged;
        }

        private void OnOrientationSensorOrientationChanged(SimpleOrientationSensor sender, SimpleOrientationSensorOrientationChangedEventArgs args)
        {
            if (args.Orientation != SimpleOrientation.Faceup && args.Orientation != SimpleOrientation.Facedown)
            {
                // Only update the current orientation if the device is not parallel to the ground. This allows users to take pictures of documents (FaceUp)
                // or the ceiling (FaceDown) in portrait or landscape, by first holding the device in the desired orientation, and then pointing the camera
                // either up or down, at the desired subject.
                //Note: This assumes that the camera is either facing the same way as the screen, or the opposite way. For devices with cameras mounted
                //      on other panels, this logic should be adjusted.
                m_deviceOrientation = args.Orientation;
            }
        }

        private async void OnHardwareButtonsCameraPressed(object sender, CameraEventArgs e)
        {
            await CaptureImageAsync();
        }
        private void UnregisterEventHandlers()
        {
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.CameraPressed -= OnHardwareButtonsCameraPressed;
            }

            if (m_orientationSensor != null)
            {
                m_orientationSensor.OrientationChanged -= OnOrientationSensorOrientationChanged;
            }

            m_displayInformation.OrientationChanged -= OnDisplayInformationOrientationChanged;
        }      

        private VideoEncodingProperties GetHighestResolution()
        {
            VideoEncodingProperties resolutionMax = null;
            int maxWidth = 0;
            int maxHeight = 0;
            float maxFramerate = 0;

            var resolutions = m_mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview);

            for (var i = 0; i < resolutions.Count; i++)
            {
                VideoEncodingProperties res = (VideoEncodingProperties)resolutions[i];

                var frameRate = ((float)res.FrameRate.Numerator) / res.FrameRate.Denominator;
                if (frameRate > 30.01f)
                    continue;
                if (res.Width >= maxWidth && frameRate >= maxFramerate)
                {
                    if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons") && res.Width > 1280 )
                         continue;
                    
                    if (res.Height > maxHeight)
                    {
                        maxWidth = (int)res.Width;
                        maxHeight = (int)res.Height;
                        maxFramerate = frameRate;
                        resolutionMax = res;
                    }
                }
            }

            return resolutionMax;
        }


        async void OnMediaCaptureRecordLimitationExceeded(MediaCapture sender)
        {
            await TaskUtilities.RunOnDispatcherThreadAsync(async () =>
            {
                await StopRecordAsync();
                IsRecording = false;
            });
        }

        /*    protected async override void OnNavigatingTo()
            {          
             SystemMediaTransportControls.GetForCurrentView().PropertyChanged += OnMediaPropertyChanged;     
                await StartPreviewAsync();
            }
            protected async override void OnNavigatingFrom()
            {           
             SystemMediaTransportControls.GetForCurrentView().PropertyChanged -= OnMediaPropertyChanged;    
                await StopPreviewAsync();
            }
            */

    }
}

