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
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Lumia.Imaging.Extras.Extensions;
using Lumia.Imaging;

namespace Lumia.Imaging.EditShowcase
{
    internal class LookupImage : IDisposable
    {
        private IImageProvider m_landscapeSource;
        private IImageProvider m_portraitSource;
        private Task m_loadingTask;

        public LookupImage(string name)
        {
            m_loadingTask = Task.Run(async () =>
            {
                var file = await Package.Current.InstalledLocation.GetFileAsync(name).AsTask().ConfigureAwait(false);

                    using (var imageSource = new StorageFileImageSource(file))
                    {
                        var info = await imageSource.GetInfoAsync().AsTask().ConfigureAwait(false);
                        var landscapeBitmap = new Bitmap(info.ImageSize, ColorMode.Bgra8888);
                        var portraitBitmap = new Bitmap(new Size(info.ImageSize.Height, info.ImageSize.Width), ColorMode.Bgra8888);

                        using (var renderer = new BitmapRenderer(imageSource, landscapeBitmap, OutputOption.Stretch))
                        {
                            renderer.RenderOptions = RenderOptions.Cpu;
                            m_landscapeSource = new BitmapImageSource(await renderer.RenderAsync().AsTask().ConfigureAwait(false));
                        }

                        using (var renderer = new BitmapRenderer(m_landscapeSource.Rotate(90), portraitBitmap, OutputOption.Stretch))
                        {
                            renderer.RenderOptions = RenderOptions.Cpu;
                            m_portraitSource = new BitmapImageSource(await renderer.RenderAsync().AsTask().ConfigureAwait(false));
                        }
                    }
            });
        }

        public async Task<IImageProvider> GetAsync(Size imageSize)
        {
            if (m_loadingTask == null)
            {
                throw new ObjectDisposedException("LookupImage");
            }

            await m_loadingTask.ConfigureAwait(false);

            return imageSize.Width > imageSize.Height ? m_landscapeSource : m_portraitSource;
        }

        public void Dispose()
        {
            DisposeAndNull(ref m_landscapeSource);
            DisposeAndNull(ref m_portraitSource);
            m_loadingTask = null;
        }

        private void DisposeAndNull<T>(ref T obj)
            where T : class
        {
            var disposable = obj as IDisposable;

            if (disposable == null)
            {
                return;
            }

            disposable.Dispose();
            obj = null;
        }
    }
}
