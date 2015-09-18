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

using Lumia.Imaging.Adjustments;
using System;
using Windows.Foundation;
using Windows.UI;
using Lumia.Imaging.Workers;

namespace Lumia.Imaging.Extras.ImageSources
{
    /// <summary>
    /// A procedural image source providing a noise distribution suitable for standard 5 MPixel images. 
    /// The noise is monochromatic and centered around gray (128,128,128).
    /// </summary>
    public class NoiseImageSource : IImageProvider2, IDisposable
    {
        private IImageProvider2 m_imageProvider;

        /// <summary>
        /// Constructs a noise image source.
        /// </summary>
        /// <param name="renderSize">The dimensions of the target image that this noise will be used for. The actual grain size is proportional to a standardized ~5 MP image with the same aspect ratio.</param>
        /// <param name="amplitude">Noise amplitude.</param>
        public NoiseImageSource(Size renderSize, int amplitude)
        {
            m_imageProvider = CreateSource(renderSize, amplitude);
        }

        ~NoiseImageSource()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            var disposable = m_imageProvider as IDisposable;

            if (disposable != null)
            {
                disposable.Dispose();
            }

            m_imageProvider = null;
        }

        private static IImageProvider2 CreateSource(Size renderSize, int grainSize)
        {
            if ((int)Math.Min(renderSize.Width, renderSize.Height) <= 0)
            {
                throw new ArgumentOutOfRangeException("renderSize");
            }

            const int smallerAxisFor5MPixels = 1728;

            double scaleFactor = smallerAxisFor5MPixels / Math.Min(renderSize.Width, renderSize.Height);
            var noiseImageSize = new Size(renderSize.Width * scaleFactor, renderSize.Height * scaleFactor);

            var limitedScaleFactor = Math.Max(1.0, scaleFactor);

            var colorImageSource = new ColorImageSource(noiseImageSize, Color.FromArgb(255, 128, 128, 128));

            var filterEffect = new GaussianNoiseEffect(colorImageSource, Math.Max(1, grainSize / limitedScaleFactor));

            return filterEffect;
        }

        public IAsyncOperation<ImageProviderInfo> GetInfoAsync()
        {
            return m_imageProvider.GetInfoAsync();
        }

        public bool Lock(RenderRequest renderRequest)
        {
            return m_imageProvider.Lock(renderRequest);
        }

        public IImageProvider2 Clone()
        {
            return m_imageProvider.Clone();
        }

        public IImageWorker CreateImageWorker(IImageWorkerRequest imageWorkerRequest)
        {
            return m_imageProvider.CreateImageWorker(imageWorkerRequest);
        }

        public IAsyncOperation<Bitmap> GetBitmapAsync(Bitmap bitmap, OutputOption outputOption)
        {
            return m_imageProvider.GetBitmapAsync(bitmap, outputOption);
        }

        public IAsyncAction PreloadAsync()
        {
            return m_imageProvider.PreloadAsync();
        }

        public RenderOptions SupportedRenderOptions
        {
            get { return m_imageProvider.SupportedRenderOptions; }
        }
    }

}
