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
using Lumia.Imaging.Artistic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace QuickStart
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        
        // Here we will apply Cartoon Effect to an image.
        private CartoonEffect _cartoonEffect;

        // The following  WriteableBitmap contains 
        // The filtered and thumbnail image.
        private WriteableBitmap _cartoonImageBitmap;
        private WriteableBitmap _thumbnailImageBitmap;

        public MainPage()
        {
            InitializeComponent();

            double scaleFactor = 1.0;
#if WINDOWS_PHONE_APP
            scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
#endif
            _cartoonImageBitmap = new WriteableBitmap((int)(Window.Current.Bounds.Width * scaleFactor), (int)(Window.Current.Bounds.Height * scaleFactor));
            _thumbnailImageBitmap = new WriteableBitmap((int)OriginalImage.Width, (int)OriginalImage.Height);
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
                SuggestedFileName = string.Format("CartoonImage_{0}", DateTime.Now.ToString("yyyyMMddHHmmss"))
            };

            savePicker.FileTypeChoices.Add("JPG File", new List<string> { ".jpg" });

            SaveImage(savePicker);
        }

        private async Task<bool> ApplyFilterAsync(StorageFile file)
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

                // A cartoon effect is initialized with selected image stream as source.
                var imageStream = new RandomAccessStreamImageSource(fileStream);
                _cartoonEffect = new CartoonEffect(imageStream);

                // Render the image to a WriteableBitmap.
                var renderer = new WriteableBitmapRenderer(_cartoonEffect, _cartoonImageBitmap);
                _cartoonImageBitmap = await renderer.RenderAsync();
                _cartoonImageBitmap.Invalidate();

                // Set the rendered image as source for the cartoon image control.
                CartoonImage.Source = _cartoonImageBitmap;
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
            if (_cartoonEffect == null)
            {
                return false;
            }

            string errorMessage = null;

            try
            {
                using (var jpegRenderer = new JpegRenderer(_cartoonEffect))
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
    }
}
