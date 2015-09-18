
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
using System;
using VideoEffectDemo;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Media.Capture;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace LumiaImagingSDKWin2DDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoEffectView : Page
    {
        private MediaCapture m_mediaCapture;
        public VideoEffectView()
        {
            this.InitializeComponent();

            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (s, a) =>
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                    a.Handled = true;
                }
            };

            if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
            {
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += (s, a) =>
                {   
                    if (Frame.CanGoBack)
                    {
                        Frame.GoBack();
                        a.Handled = true;
                    }
                };
            }

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private async void OnUnloaded(object sender, RoutedEventArgs e)
        {
            await m_mediaCapture.StopPreviewAsync();
            m_mediaCapture.Failed -= mediaCapture_Failed;
        }

        private void mediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            var action = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
              //  progressText.Text = "MediaCapture failed: " + errorEventArgs.Message;
            });
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            m_mediaCapture = new MediaCapture();
            m_mediaCapture.Failed += mediaCapture_Failed;

            var settings = new MediaCaptureInitializationSettings()
            {
                StreamingCaptureMode = StreamingCaptureMode.Video
            };

            try
            {
                await m_mediaCapture.InitializeAsync(settings);
                
                await m_mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, GetHighestResolution());
            }
            catch (Exception)
            {               
                return;
            }

            await m_mediaCapture.AddVideoEffectAsync( new VideoEffectDefinition(typeof(ExampleVideoEffect).FullName, new PropertySet()), MediaStreamType.VideoPreview);

            m_previewVideoElement.Source = m_mediaCapture;
            await m_mediaCapture.StartPreviewAsync();
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
                    if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons") && res.Width > 1280)
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



    }
}
