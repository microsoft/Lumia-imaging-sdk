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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ImageSequencer
{

    public sealed partial class SequenceCapturePage : Page
    {

        private List<IRandomAccessStream> _capturedSequence;
        private MediaCapture _mediaCapture;
        private LowLagPhotoSequenceCapture _lowLagPhotoSequenceCapture;
        private Task _saveTask;
        private Boolean _recording = false;
        private List<StorageFile> _files = new List<StorageFile>();
        private int _fileIndex = 1;
        private NavigationHelper _navigationHelper;
        private const int AMOUNT_OF_FRAMES_IN_SEQUENCE = 20;

        public SequenceCapturePage()
        {
            this.InitializeComponent();

            _navigationHelper = new NavigationHelper(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;

            InitializeMediaCapture();

            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                Windows.Phone.UI.Input.HardwareButtons.CameraPressed += HardwareButtons_CameraPressed;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;

            base.OnNavigatedFrom(e);
            _mediaCapture.Dispose();

            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                Windows.Phone.UI.Input.HardwareButtons.CameraPressed -= HardwareButtons_CameraPressed;
            }

        }

        private async void InitializeMediaCapture()
        {
            _capturedSequence = new List<IRandomAccessStream>();

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

            captureElement.Source = _mediaCapture;
            await _mediaCapture.StartPreviewAsync();

            var format = ImageEncodingProperties.CreateJpeg();
            format.Width = 640;
            format.Height = 480;

            _lowLagPhotoSequenceCapture = await _mediaCapture.PrepareLowLagPhotoSequenceCaptureAsync(format);            
            _lowLagPhotoSequenceCapture.PhotoCaptured += OnPhotoCaptured;
        }

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

        private async void ShowPreviewPage()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    Frame.Navigate(typeof(SequencePreviewPage), _files);
                }
            );
        }

        private async Task Save(IRandomAccessStream frame, int i)
        {
            var filename = "ImageSequencer." + i + ".jpg";
            var folder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            var storageFile = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            var stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite);
            await RandomAccessStream.CopyAndCloseAsync(frame, stream);
            _files.Add(storageFile);
        }

        public void CaptureElement_Tapped(object sender, RoutedEventArgs e)
        {
            StartStopCapture();
        }

        public void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            StartStopCapture();
        }
        void HardwareButtons_CameraPressed(object sender, Windows.Phone.UI.Input.CameraEventArgs e)
        {
            StartStopCapture();
        }

        private async void StartStopCapture()
        {
            if (!_recording)
            {
                _recording = true;
                ProgressBarHelper.ShowProgressBar("Capturing");
                await _lowLagPhotoSequenceCapture.StartAsync();
                CaptureButton.Icon = new SymbolIcon(Symbol.Stop);
            }
            else
            {
                StopSequenceCapture();
            }
        }

        public async void StopSequenceCapture()
        {
            _recording = false;
            await _lowLagPhotoSequenceCapture.FinishAsync();
            ProgressBarHelper.HideProgressBar();
            ShowPreviewPage();
        }

        public void About_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AboutPage));
        }

    }
}
