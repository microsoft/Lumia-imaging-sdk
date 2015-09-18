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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Lumia.Imaging;
using Windows.Graphics.Display;
using Lumia.Imaging.Artistic;
using Lumia.Imaging.Adjustments;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace QuickStart
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private GrayscaleEffect _grayscaleEffect;
        private BrightnessEffect _brightnessEffect;
        SwapChainPanelRenderer m_renderer;

        // The following  WriteableBitmap contains 
        // The filtered and thumbnail image.
        private WriteableBitmap _writeableBitmap;
        private WriteableBitmap _thumbnailImageBitmap;

        public MainPage()
        {
            InitializeComponent();

            double scaleFactor = 1.0;
            scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;

            _writeableBitmap = new WriteableBitmap((int)(Window.Current.Bounds.Width * scaleFactor), (int)(Window.Current.Bounds.Height * scaleFactor));
            _thumbnailImageBitmap = new WriteableBitmap((int)OriginalImage.Width, (int)OriginalImage.Height);
            _grayscaleEffect = new GrayscaleEffect();
            _brightnessEffect = new BrightnessEffect(_grayscaleEffect);
            SwapChainPanelTarget.Loaded += SwapChainPanelTarget_Loaded;

        }

        private async void SwapChainPanelTarget_Loaded(object sender, RoutedEventArgs e)
        {

            if (SwapChainPanelTarget.ActualHeight > 0 && SwapChainPanelTarget.ActualWidth > 0)
            {
                if (m_renderer == null)
                {
                    m_renderer = new SwapChainPanelRenderer(_brightnessEffect, SwapChainPanelTarget);
                    await LoadDefaultImageAsync();
                }
            }

            SwapChainPanelTarget.SizeChanged += async (s, args) =>
            {
                await m_renderer.RenderAsync();
            };
        }

        private async Task LoadDefaultImageAsync()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new System.Uri("ms-appx:///Assets/defaultImage.jpg"));
            await ApplyEffectAsync(file);
        }

        private void PickImage_Click(object sender, RoutedEventArgs e)
        {
            SaveButton.IsEnabled = false;

            var openPicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                ViewMode = PickerViewMode.Thumbnail
            };

            // Filter to include a sample subset of file types.
            openPicker.FileTypeFilter.Clear();
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".jpg");

            PickImage(openPicker);
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            SaveButton.IsEnabled = false;

            var savePicker = new FileSavePicker()
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = string.Format("QuickstartImage_{0}", DateTime.Now.ToString("yyyyMMddHHmmss"))
            };

            savePicker.FileTypeChoices.Add("JPG File", new List<string> { ".jpg" });

            SaveImage(savePicker);
        }

        private async Task<bool> ApplyEffectAsync(StorageFile file)
        {
            // Open a stream for the selected file.
            IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);

            string errorMessage = null;

            try
            {
                // Show thumbnail of original image.
                _thumbnailImageBitmap.SetSource(fileStream);
                OriginalImage.Source = _thumbnailImageBitmap;

                // Rewind stream to start.                     
                fileStream.Seek(0);

                // Set the imageSource on the effect and render            
                ((IImageConsumer)_grayscaleEffect).Source = new Lumia.Imaging.RandomAccessStreamImageSource(fileStream);
                await m_renderer.RenderAsync();

            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                var dialog = new MessageDialog(errorMessage);
                await dialog.ShowAsync();
                return false;
            }

            return true;
        }

        private async Task<bool> SaveImageAsync(StorageFile file)
        {
            if (_grayscaleEffect == null)
            {
                return false;
            }

            string errorMessage = null;

            try
            {
                using (var jpegRenderer = new JpegRenderer(_grayscaleEffect))
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    // Jpeg renderer gives the raw buffer containing the filtered image.
                    IBuffer jpegBuffer = await jpegRenderer.RenderAsync();
                    await stream.WriteAsync(jpegBuffer);
                    await stream.FlushAsync();
                }
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                var dialog = new MessageDialog(errorMessage);
                await dialog.ShowAsync();
                return false;
            }
            return true;
        }
        private async void PickImage(FileOpenPicker openPicker)
        {
            // Open the file picker.
            StorageFile file = await openPicker.PickSingleFileAsync();

            // file is null if user cancels the file picker.
            if (file != null)
            {
                if (!(await ApplyEffectAsync(file)))
                    return;

                SaveButton.IsEnabled = true;
            }
        }

        private async void SaveImage(FileSavePicker savePicker)
        {
            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                await SaveImageAsync(file);
            }

            SaveButton.IsEnabled = true;
        }

        private async void OnBrightnessChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            _brightnessEffect.Level = e.NewValue;
            if (((IImageConsumer)_grayscaleEffect).Source != null)
            {
                await m_renderer.RenderAsync();
            }
        }
    }
}
