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
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace ImageSequencer
{

    public sealed partial class GifPage : Page
    {
        private NavigationHelper _navigationHelper;
        private List<WriteableBitmap> _frames;
        private int _currentFrameIndex = 0;
        private String filename;
        private DispatcherTimer timer;

        public GifPage()
        {
            this.InitializeComponent();
            _navigationHelper = new NavigationHelper(this);
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += Timer_Tick;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _frames = new List<WriteableBitmap>();
            filename = e.Parameter as String;
            FileNameTextBlock.Text = filename;
            ProgressBarHelper.ShowProgressBar("Loading GIF");
            await LoadImage(filename);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            timer.Stop();
            ProgressBarHelper.HideProgressBar();
            base.OnNavigatedFrom(e);
        }

        private void StartAnimation()
        {
            DeleteButton.IsEnabled = true;
            timer.Start();
            ProgressBarHelper.HideProgressBar();
        }

        private void Timer_Tick(object sender, object e)
        {
            GifImage.Source = _frames[_currentFrameIndex];

            _currentFrameIndex++;
            if (_currentFrameIndex >= _frames.Count())
            {
                _currentFrameIndex = 0;
            }
        }

        private async Task LoadImage(String filename)
        {
            var storageFile = await KnownFolders.SavedPictures.GetFileAsync(filename);

            var storageFileImageSource = new StorageFileImageSource(storageFile);
            var asyncImageResource = storageFileImageSource as IAsyncImageResource;
            var imageResource = await asyncImageResource.LoadAsync();

            for (uint frameIndex = 0; frameIndex < imageResource.FrameCount; frameIndex++)
            {
                imageResource.FrameIndex = frameIndex;

                var writeableBitmap = new WriteableBitmap((int)imageResource.ImageSize.Width, (int)imageResource.ImageSize.Height);
                using (var writeableBitmapRenderer = new WriteableBitmapRenderer(storageFileImageSource, writeableBitmap))
                {
                    writeableBitmapRenderer.RenderOptions = RenderOptions.Cpu;
                    await writeableBitmapRenderer.RenderAsync();
                    _frames.Add(writeableBitmap);
                }
            }

            StartAnimation();
        }

        public async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var storageFile = await KnownFolders.SavedPictures.GetFileAsync(filename);
            await storageFile.DeleteAsync();
            _navigationHelper.GoBack();
        }

        public void About_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AboutPage));
        }
    }
}
