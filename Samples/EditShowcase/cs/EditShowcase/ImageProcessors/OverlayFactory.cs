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

using Lumia.Imaging;
using Lumia.Imaging.Transforms;
using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Lumia.Imaging.EditShowcase.ImageProcessors
{
    /// <summary>
    /// Defines an external overlay image in terms of three URIs (for use in landscape, portrait and square scenarios).
    /// </summary>
    public class OverlayFactory
    {
        public OverlayFactory(Uri landscapeUri, Uri portraitUri, Uri squareUri)
        {
            LandscapeUri = landscapeUri;
            PortraitUri = portraitUri;
            SquareUri = squareUri;
            ShapeTolerance = 0.2;
            HorizontalAlignment = HorizontalAlignment.None;
            VerticalAlignment = HorizontalAlignment.None;
        }

        public OverlayFactory(string landscapeUri, string portraitUri, string squareUri) :
            this(new Uri(landscapeUri), new Uri(portraitUri), new Uri(squareUri))
        {
        }

        public Uri LandscapeUri { get; set; }

        public Uri PortraitUri { get; set; }

        public Uri SquareUri { get; set; }

        public double ShapeTolerance { get; set; }

        public HorizontalAlignment HorizontalAlignment { get; set; }

        public HorizontalAlignment VerticalAlignment { get; set; }

        public bool CheckAspectRatios { get; set; }

        private Tuple<Uri, int> GetUriAndRotation(Size backgroundSize)
        {
            double aspectRatio = backgroundSize.Width / backgroundSize.Height;

            var uri = SquareUri;

            double divergenceFromSquare = aspectRatio - 1.0;

            int rotation = 0;

            if (Math.Abs(divergenceFromSquare) > ShapeTolerance)
            {
                if (divergenceFromSquare > 0.0)
                {
                    uri = LandscapeUri;
                }
                else
                {
                    if (PortraitUri != null)
                    {
                        uri = PortraitUri;
                    }
                    else
                    {
                        uri = LandscapeUri;
                        rotation = 90;
                    }
                }
            }

            return Tuple.Create(uri, rotation);
        }

        /// <summary>
        /// Creates an image source of the overlay, specifying the size of the background image it will be used on. The image source will be sized and cropped correctly.
        /// </summary>
        /// <param name="backgroundSize">The size of the background image.</param>
        /// <returns>The constructed overlay image source.</returns>
        public async Task<IImageProvider> CreateAsync(Size backgroundSize)
        {
            var uriAndRotation = GetUriAndRotation(backgroundSize);
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uriAndRotation.Item1).AsTask().ConfigureAwait(false);
            var overlayImageSource = new StorageFileImageSource(file);

            var overlayImageInfo = await overlayImageSource.GetInfoAsync().AsTask().ConfigureAwait(false);
            var overlayImageAspectRatio = overlayImageInfo.ImageSize.Width / overlayImageInfo.ImageSize.Height;

            int overlayImageScaledWidth = (int)overlayImageInfo.ImageSize.Width;
            int overlayImageScaledHeight = (int)overlayImageInfo.ImageSize.Height;

            if ((int)backgroundSize.Width > (int)backgroundSize.Height)
            {
                overlayImageScaledHeight = (int)(backgroundSize.Width / overlayImageAspectRatio);
                overlayImageScaledWidth = (int)(backgroundSize.Width);
            }
            else if ((int)backgroundSize.Width < (int)backgroundSize.Height)
            {
                overlayImageScaledWidth = (int)(backgroundSize.Height * overlayImageAspectRatio);
                overlayImageScaledHeight = (int)(backgroundSize.Height);
            }

            var renderer = new BitmapRenderer(overlayImageSource) { Size = new Size(overlayImageScaledWidth, overlayImageScaledHeight) };
            var overlayBitmap = await renderer.RenderAsync().AsTask().ConfigureAwait(false);
            var overlayBitmapImageSource = new BitmapImageSource(overlayBitmap);

            return Crop(overlayBitmapImageSource, overlayImageInfo, (int)overlayImageInfo.ImageSize.Width, (int)overlayImageInfo.ImageSize.Height);
        }

        private IImageProvider Crop(BitmapImageSource overlayBitmapImageSource, ImageProviderInfo originalOverlayImageProviderInfo, int backgroundWidth, int backgroundHeight)
        {
            IImageProvider imageProvider;

            int overlayWidth = (int)overlayBitmapImageSource.Bitmap.Dimensions.Width;
            int overlayHeight = (int)overlayBitmapImageSource.Bitmap.Dimensions.Height;

            if (HorizontalAlignment != HorizontalAlignment.None)
            {
                int cropLeft = 0;
                int cropTop = 0;
                int cropWidth = Math.Min(overlayWidth, (int)originalOverlayImageProviderInfo.ImageSize.Width);
                int cropHeight = Math.Min(overlayHeight, (int)originalOverlayImageProviderInfo.ImageSize.Height);

                if ((HorizontalAlignment == HorizontalAlignment.Center) &&
                    (overlayWidth < (int)originalOverlayImageProviderInfo.ImageSize.Width))
                {
                    cropLeft = Math.Abs(overlayWidth / 2 - backgroundWidth / 2);
                    cropWidth -= cropLeft * 2;
                }

                if ((VerticalAlignment == HorizontalAlignment.Center) &&
                    (overlayHeight < (int)originalOverlayImageProviderInfo.ImageSize.Height))
                {
                    cropTop = Math.Abs(overlayHeight / 2 - backgroundHeight / 2);
                    cropHeight -= cropTop * 2;
                }

                imageProvider = new CropEffect(new Rect(cropLeft, cropTop, cropWidth, cropHeight));
            }
            else
            {
                imageProvider = overlayBitmapImageSource;
            }

            return imageProvider;
        }
    }
}
