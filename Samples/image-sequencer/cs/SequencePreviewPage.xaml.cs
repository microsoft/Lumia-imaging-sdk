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
using Lumia.Imaging.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace ImageSequencer
{

    public sealed partial class SequencePreviewPage : Page
    {

        private IReadOnlyList<IImageProvider> _unalignedImageProviders;
        private IReadOnlyList<IImageProvider> _alignedImageProviders;
        private IReadOnlyList<IImageProvider> _onScreenImageProviders;

        private int _unalignedImageWidth;
        private int _unalignedImageHeight;
        private int _alignedImageWidth;
        private int _alignedImageHeight;

        private WriteableBitmap _foregroundBitmap;
        private WriteableBitmap _backgroundBitmap;

        private RectangleGeometry _animatedArea;
        private bool _frameEnabled;
        private Point _dragStart;

        private int _animationIndex = 0;
        private DispatcherTimer _animationTimer;
        private volatile Task _renderTask;

        private NavigationHelper _navigationHelper;

        public SequencePreviewPage()
        {
            this.InitializeComponent();

            _animationTimer = new DispatcherTimer();
            _animationTimer.Tick += AnimationTimer_Tick;
            _animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);

            _navigationHelper = new NavigationHelper(this);

            // Hide page header on landscape orientation
            Window.Current.SizeChanged += ((s, e) => { Header.Visibility = (e.Size.Width > e.Size.Height) ? Visibility.Collapsed : Visibility.Visible; });
            Header.Visibility = Window.Current.Bounds.Width > Window.Current.Bounds.Height ? Visibility.Collapsed : Visibility.Visible;
        }

        private void AdjustPreviewImageSizeAccordingToLayout()
        {
            int imageWidth = _onScreenImageProviders == _unalignedImageProviders ? _unalignedImageWidth : _alignedImageWidth;
            int imageHeight = _onScreenImageProviders == _unalignedImageProviders ? _unalignedImageHeight : _alignedImageHeight;

            double aspectRatio = ((double)imageWidth) / imageHeight;

            double w = RootGrid.ActualWidth;
            double h = w / aspectRatio;

            if (h > RootGrid.ActualHeight)
            {
                w /= h / RootGrid.ActualHeight;
                h = RootGrid.ActualHeight;
            }

            double scale = w / ImageElementBackground.Width;

            ImageElement.Width = w;
            ImageElement.Height = h;
            ImageElementBackground.Width = w;
            ImageElementBackground.Height = h;

            if (_animatedArea == null && imageWidth != 0 && imageHeight != 0)
            {
                _animatedArea = new RectangleGeometry();
                AnimatedAreaIndicator.Width = w;
                AnimatedAreaIndicator.Height = h;
                Canvas.SetLeft(AnimatedAreaIndicator, 0);
                Canvas.SetTop(AnimatedAreaIndicator, 0);
                _animatedArea.Rect = new Rect(0, 0, w, h);
            }
            else if (_animatedArea != null)
            {
                if (scale > 0)
                {
                    Rect newAnimatedArea = new Rect(
                        _animatedArea.Rect.Left * scale,
                        _animatedArea.Rect.Top * scale,
                        _animatedArea.Rect.Width * scale,
                        _animatedArea.Rect.Height * scale);
                    if (newAnimatedArea.Height + newAnimatedArea.Top >= h)
                    {
                        newAnimatedArea.Height = h - newAnimatedArea.Top;
                    }

                    AnimatedAreaIndicator.Width = newAnimatedArea.Width;
                    AnimatedAreaIndicator.Height = newAnimatedArea.Height;
                    Canvas.SetLeft(AnimatedAreaIndicator, newAnimatedArea.Left);
                    Canvas.SetTop(AnimatedAreaIndicator, newAnimatedArea.Top);
                    _animatedArea.Rect = newAnimatedArea;
                }
            }
        }

        private void AdjustPreviewImageSizeAccordingToOrientation(object sender, SizeChangedEventArgs e)
        {
            AdjustPreviewImageSizeAccordingToLayout();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            ProgressBarHelper.ShowProgressBar("Processing sequence");

            // The image sources are provided as parameter
            List<IImageProvider> imageProviders = new List<IImageProvider>();

            if (e.Parameter is List<StorageFile>) // Using list of files as source for sequence
            {
                var files = e.Parameter as List<StorageFile>;
                foreach (StorageFile storageFile in files)
                {
                    imageProviders.Add(new StorageFileImageSource(storageFile));
                }
            }
            else if (e.Parameter is int) // Using bundled resources as source for sequence, list of files is created here
            {
                int sequenceId = (int)e.Parameter;
                int imageIndex = 0;
                List<StorageFile> files = new List<StorageFile>();
                try
                {
                    while (true)
                    {
                        var imageUri = new Uri("ms-appx:///Assets/sequence." + sequenceId + "." + imageIndex + ".jpg");
                        StorageFile file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(imageUri);
                        var stream = await file.OpenReadAsync(); // only for checking if file exists
                        stream.Dispose();
                        imageProviders.Add(new StorageFileImageSource(file));
                        imageIndex++;
                    }
                }
                catch (Exception)
                {
                    // Expected FileNotFoundException
                }
            }

            await PrepareImageSequence(imageProviders);

            Canvas.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            Canvas.ManipulationStarted += ImageElement_ManipulationStarted;
            Canvas.ManipulationDelta += ImageElement_ManipulationDelta;

            SetControlsEnabled(true);

            AdjustPreviewImageSizeAccordingToLayout();
            RootGrid.SizeChanged += AdjustPreviewImageSizeAccordingToOrientation;

            _animationTimer.Start();

            ProgressBarHelper.HideProgressBar();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _animationTimer.Stop();
        }

        public async Task PrepareImageSequence(List<IImageProvider> imageProviders)
        {
            _unalignedImageProviders = imageProviders;
            _onScreenImageProviders = _unalignedImageProviders;

            IAsyncImageResource asyncImageResource = (IAsyncImageResource)_unalignedImageProviders[0];
            var info = await asyncImageResource.LoadAsync();
            _unalignedImageWidth = (int)info.ImageSize.Width;
            _unalignedImageHeight = (int)info.ImageSize.Height;

            using (ImageAligner imageAligner = new ImageAligner())
            {
                imageAligner.Sources = _unalignedImageProviders;
                imageAligner.ReferenceSource = _unalignedImageProviders[0];

                try
                {
                    _alignedImageProviders = await imageAligner.AlignAsync();

                    asyncImageResource = (IAsyncImageResource)_unalignedImageProviders[0];
                    info = await asyncImageResource.LoadAsync();
                    _alignedImageWidth = (int)info.ImageSize.Width;
                    _alignedImageHeight = (int)info.ImageSize.Height;
                    await EnableAlign();
                }
                catch (Exception)
                {
                    // If align fails, fail silently but don't enable the align button on UI
                }
            }
        }

        private void AnimationTimer_Tick(object sender, object e)
        {
            Render(_onScreenImageProviders, _animationIndex);

            if (_animationIndex == (_onScreenImageProviders.Count() - 1))
            {
                _animationIndex = 0;
            }
            else
            {
                _animationIndex++;
            }
        }

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

        private async Task EnableAlign()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    alignButton.IsEnabled = true;
                });
        }

        private void ImageElement_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (_frameEnabled)
            {

                double x0 = Math.Min(e.Position.X, _dragStart.X);
                double x1 = Math.Max(e.Position.X, _dragStart.X);
                double y0 = Math.Min(e.Position.Y, _dragStart.Y);
                double y1 = Math.Max(e.Position.Y, _dragStart.Y);

                x0 = Math.Max(x0, 0);
                x1 = Math.Min(x1, ImageElementBackground.ActualWidth);
                y0 = Math.Max(y0, 0);
                y1 = Math.Min(y1, ImageElementBackground.ActualHeight);

                double width = x1 - x0;
                double height = y1 - y0;

                Rect rect = new Rect(x0, y0, width, height);
                Canvas.SetLeft(AnimatedAreaIndicator, rect.X);
                Canvas.SetTop(AnimatedAreaIndicator, rect.Y);
                AnimatedAreaIndicator.Width = rect.Width;
                AnimatedAreaIndicator.Height = rect.Height;

                _animatedArea.Rect = rect;

            }
        }

        private void ImageElement_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _dragStart = new Point(e.Position.X, e.Position.Y);
        }

        public void Play_Click(object sender, RoutedEventArgs e)
        {
            if (_animationTimer.IsEnabled)
            {
                Stop();
            }
            else
            {
                Play();
            }
        }

        private void Stop()
        {
            _animationTimer.Stop();
            playButton.Icon = new SymbolIcon(Symbol.Play);
        }

        private void Play()
        {
            _animationTimer.Start();
            playButton.Icon = new SymbolIcon(Symbol.Pause);
        }

        private void AlignButton_Click(object sender, RoutedEventArgs e)
        {
            bool isAligned = _onScreenImageProviders == _alignedImageProviders;

            if (!isAligned)
            {
                _onScreenImageProviders = _alignedImageProviders;
                BitmapIcon icon = new BitmapIcon();
                icon.UriSource = new Uri("ms-appx:/Assets/appbar.align.enabled.png");
                alignButton.Icon = icon;
            }
            else
            {
                _onScreenImageProviders = _unalignedImageProviders;
                BitmapIcon icon = new BitmapIcon();
                icon.UriSource = new Uri("ms-appx:/Assets/appbar.align.disabled.png");
                alignButton.Icon = icon;
            }

            Render(_onScreenImageProviders, _animationIndex, true);
            AdjustPreviewImageSizeAccordingToLayout();

            saveButton.IsEnabled = true;
        }

        private void FrameButton_Click(object sender, RoutedEventArgs e)
        {
            _frameEnabled = !_frameEnabled;
            AnimatedAreaIndicator.Visibility = _frameEnabled ? Visibility.Visible : Visibility.Collapsed;

            if (!_frameEnabled)
            {
                ImageElement.Clip = null;
                BitmapIcon icon = new BitmapIcon();
                icon.UriSource = new Uri("ms-appx:/Assets/appbar.frame.disabled.png");
                frameButton.Icon = icon;
            }
            else
            {
                ImageElement.Clip = _animatedArea;
                BitmapIcon icon = new BitmapIcon();
                icon.UriSource = new Uri("ms-appx:/Assets/appbar.frame.enabled.png");
                frameButton.Icon = icon;
            }

            Render(_onScreenImageProviders, _animationIndex, true);

            saveButton.IsEnabled = true;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            await _renderTask;

            SetControlsEnabled(false);

            ProgressBarHelper.ShowProgressBar("Saving");

            await Save();

            ProgressBarHelper.HideProgressBar();

            if (Frame.BackStackDepth == 2)
            {
                Frame.BackStack.RemoveAt(1);
            }

            _navigationHelper.GoBack();
        }

        private async Task Save()
        {
            if (_frameEnabled)
            {
                double _imageWidth = (_onScreenImageProviders == _unalignedImageProviders) ? _unalignedImageWidth : _alignedImageWidth;
                double _imageHeight = (_onScreenImageProviders == _unalignedImageProviders) ? _unalignedImageHeight : _alignedImageHeight;

                // Scale animated area coordinates from display coordinates to the match the original bitmap size
                double xScale = _imageWidth / ImageElementBackground.ActualWidth;
                double yScale = _imageHeight / ImageElementBackground.ActualHeight;
                Rect frame = new Rect(
                    _animatedArea.Rect.Left * xScale,
                    _animatedArea.Rect.Top * yScale,
                    _animatedArea.Rect.Width * xScale,
                    _animatedArea.Rect.Height * yScale);
                await GifExporter.Export(_onScreenImageProviders, frame);
            }
            else
            {
                await GifExporter.Export(_onScreenImageProviders, null);
            }
        }

        private void SetControlsEnabled(bool value)
        {
            playButton.IsEnabled = value;
            saveButton.IsEnabled = value;
            alignButton.IsEnabled = value && (_alignedImageProviders != null);
            frameButton.IsEnabled = value;
        }

        public void About_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AboutPage));
        }

    }
}
