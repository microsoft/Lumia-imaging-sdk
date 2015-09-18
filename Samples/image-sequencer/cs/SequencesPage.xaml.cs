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
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace ImageSequencer
{

    public sealed partial class SequencesPage : Page
    {

        public ObservableCollection<String> Sequences { get; private set; }
        public ObservableCollection<GifThumbnail> Gifs { get; private set; }

        public SequencesPage()
        {            
            this.InitializeComponent();
            Gifs = new System.Collections.ObjectModel.ObservableCollection<GifThumbnail>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {            
            FindAnimGIFsFromPicturesLibraryAsync();

            Sequences = new ObservableCollection<String>()
            {
                "ms-appx:///Assets/sequence.1.0.jpg",
                "ms-appx:///Assets/sequence.2.0.jpg"
            };

            DataContext = this;
        }

        private async void FindAnimGIFsFromPicturesLibraryAsync()
        {
            Gifs.Clear();

            var files = await KnownFolders.SavedPictures.GetFilesAsync();
            foreach (StorageFile storageFile in files)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(
                    storageFile.Name,
                    "Sequence\\d+\\.gif",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    var stream = await storageFile.OpenReadAsync();
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(stream);
                    GifThumbnail gifThumbnail = new GifThumbnail();
                    gifThumbnail.BitmapImage = bitmapImage;
                    gifThumbnail.FileName = storageFile.Name;
                    Gifs.Add(gifThumbnail);
                }
            }

            noGifFilesTextBlock.Visibility = (Gifs.Count() == 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        public class GifThumbnail
        {
            public BitmapImage BitmapImage { get; set; }
            public String FileName { get; set; }
        }

        public void Example_Click(object sender, RoutedEventArgs e)
        {
            int sequenceId = Sequences.IndexOf((String)(((Button)sender).DataContext)) + 1;
            Frame.Navigate(typeof(SequencePreviewPage), sequenceId);
        }

        public void Thumbnail_Click(object sender, RoutedEventArgs e)
        {
            GifThumbnail thumbnail = (sender as Button).DataContext as GifThumbnail;
            Frame.Navigate(typeof(GifPage), thumbnail.FileName);
        }

        public async void Capture_Click(object sender, RoutedEventArgs e)
        {
            MediaCapture mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync();
            if (mediaCapture.VideoDeviceController.LowLagPhotoSequence.Supported)
            {
                mediaCapture.Dispose();
                Frame.Navigate(typeof(SequenceCapturePage));
            }
            else
            {
                mediaCapture.Dispose();
                Frame.Navigate(typeof(VideoPreviewCapturePage));
            }
        }

        public void About_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AboutPage));
        }
    }
}
