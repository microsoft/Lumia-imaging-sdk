/*
 * Copyright (c) 2014 Microsoft Mobile
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using ImageSequencer.Common;
using Lumia.Imaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace ImageSequencer
{

    public sealed partial class VideoPreviewCapturePage : Page
    {

        private List<StorageFile> _files;
        private CameraPreviewImageSource _cameraPreviewImageSource;
        private WriteableBitmapRenderer _writeableBitmapRenderer;
        private JpegRenderer _jpegRenderer;
        private WriteableBitmap _writeableBitmap;
        private NavigationHelper _navigationHelper;
        private Task _renderTask;
        private bool _stop;
        private bool _capturing; 
        private bool _rendering;
        private bool _initialized;
        private int _sequenceIndex;

        public VideoPreviewCapturePage()
        {
            this.InitializeComponent();

            _navigationHelper = new NavigationHelper(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            _rendering = false;
            _stop = false;
            _files = new List<StorageFile>();
            _sequenceIndex = 1;
            InitializeAsync();

            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                Windows.Phone.UI.Input.HardwareButtons.CameraPressed += HardwareButtons_CameraPressed;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
            
            base.OnNavigatedFrom(e);

            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                Windows.Phone.UI.Input.HardwareButtons.CameraPressed -= HardwareButtons_CameraPressed;
            }
        }

        public async void InitializeAsync()
        {
            // Create a camera preview image source (from Imaging SDK)
            _cameraPreviewImageSource = new CameraPreviewImageSource();
            await _cameraPreviewImageSource.InitializeAsync(string.Empty);
            var properties = await _cameraPreviewImageSource.StartPreviewAsync();

            // Create a preview bitmap with the correct aspect ratio 
            var width = 640.0;
            var height = (width / properties.Width) * properties.Height;
            _writeableBitmap = new WriteableBitmap((int)width, (int)height);

            captureElement.Source = _writeableBitmap;

            _writeableBitmapRenderer = new WriteableBitmapRenderer();
            _jpegRenderer = new JpegRenderer();

            // Attach preview frame delegate
            _cameraPreviewImageSource.PreviewFrameAvailable += OnPreviewFrameAvailable;

            _initialized = true;
        }

        private void OnPreviewFrameAvailable(IAsyncImageResource asyncImageResource)
        {
            _renderTask = Render();
        }

        

        private async Task Render()
        {
            if (!_rendering && !_stop)
            {
                _rendering = true;

                // Render camera preview frame to screen
                _writeableBitmapRenderer.Source = _cameraPreviewImageSource;
                _writeableBitmapRenderer.WriteableBitmap = _writeableBitmap;
                await _writeableBitmapRenderer.RenderAsync();

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    CoreDispatcherPriority.High, () =>
                    {
                        _writeableBitmap.Invalidate();
                    });

                // Write camera preview frame to file if capturing
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

        private async Task Save(IBuffer frame, int i)
        {
            var filename = "ImageSequencer." + i + ".jpg";            
            var folder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            var storageFile = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteBufferAsync(storageFile, frame);
            _files.Add(storageFile);
        }

        private async void Current_VisibilityChanged(object sender, Windows.UI.Core.VisibilityChangedEventArgs e)
        {
            try
            {
                if (e.Visible)
                {
                    await ResumePreviewAsync();
                }
                else
                {
                    await PausePreviewAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public async Task PausePreviewAsync()
        {
            if (_initialized)
            {
                await _cameraPreviewImageSource.StopPreviewAsync();
            }
        }

        public async Task ResumePreviewAsync()
        {
            if (_initialized)
            {
                await _cameraPreviewImageSource.InitializeAsync(string.Empty);
                await _cameraPreviewImageSource.StartPreviewAsync();
            }
        }

        public void CaptureElement_Tapped(object sender, RoutedEventArgs e)
        {
            StartStopCapture();
        }

        void HardwareButtons_CameraPressed(object sender, Windows.Phone.UI.Input.CameraEventArgs e)
        {
            StartStopCapture();
        }

        private void StartStopCapture()
        {
            if (!_capturing)
            {
                _capturing = true;
                ProgressBarHelper.ShowProgressBar("Capturing");
                CaptureButton.Icon = new SymbolIcon(Symbol.Stop);
            }
            else
            {
                StopCapture();
            }
        }

        public void StopCapture()
        {
            _stop = true;
            ProgressBarHelper.HideProgressBar();
            ShowPreview();
        }

        private async void ShowPreview()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    Frame.Navigate(typeof(SequencePreviewPage), _files);
                }
            );
        }

        public void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            StartStopCapture();
        }

        public void About_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AboutPage));
        }

    }
}
