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
using Windows.UI.Xaml.Media.Imaging;
using Lumia.Imaging.Extras.Extensions;
using Lumia.Imaging.EditShowcase.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.Graphics.Imaging;
using Lumia.Imaging.EditShowcase.Utilities;

namespace Lumia.Imaging.EditShowcase.ViewModels
{
    public class ImageProcessorRenderer
    {
        private const ColorMode IntermediateColorMode = ColorMode.Bgra8888;
        private const ColorMode PreviewColorMode = ColorMode.Bgra8888;
        private RenderOption m_currentRenderOption;

        private const int ThumbnailWidth = 100;

        private readonly Task<List<IImageProcessor>> m_createImageProcessorsTask;
        private readonly object m_taskQueueLock;

        private IImageProvider m_source;

		private Size m_sourceSize;
        private Task m_lastQueuedTask;
        private Size m_requestedPreviewSize;
        
		private SwapChainPanelRenderer m_swapChainPanelRenderer;
        
        public ImageProcessorRenderer(Size previewSize)
        {
            m_taskQueueLock = new object();
            m_requestedPreviewSize = previewSize;

            // Begin initializing all the imageprocessors...
            m_createImageProcessorsTask = EffectFactory.CreateEffects();

            m_lastQueuedTask = InitializeAsync();
        }

        public Task<List<IImageProcessor>> GetImageProcessorsAsync()
        {
            return m_createImageProcessorsTask;
        }

        private async Task InitializeAsync()
        {
            // Load the default photo meanwhile.
            var source = new BufferProviderImageSource(PreloadedImages.Woman.AsBufferProvider());

            SourcePreviewBitmap = await LoadPhotoInternalAsync(source).ConfigureAwait(false);
            RaiseSourcePreviewAvailable();

            // Wait for the image processors.
            await GetImageProcessorsAsync().ConfigureAwait(false);

            // Now render the thumbnails showing the available imageprocessors.
            UpdateThumbnailsAsync();
        }

        public async Task UpdateThumbnailsAsync()
        {
#if DEBUG
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
			var fullSizedSourceBitmap = ((SoftwareBitmapImageSource)m_source).SoftwareBitmap;

			SoftwareBitmap thumbnailSizedSourceBitmap;

			// Some image processors can render at thumbnail resolution. Prepare a scaled down source for them.
			using (var thumbnailSourceRenderer = new SoftwareBitmapRenderer(m_source))
			{
				thumbnailSourceRenderer.Size = ThumbnailSize;
				thumbnailSourceRenderer.OutputOption = OutputOption.PreserveAspectRatio;
				thumbnailSizedSourceBitmap = await thumbnailSourceRenderer.RenderAsync().AsTask().ConfigureAwait(false);
			}

			var imageProcessors = await m_createImageProcessorsTask.ConfigureAwait(false);

            var batchTasks = Enumerable.Repeat((Task)Task.FromResult(false), 4).ToArray();

            var thumbnailRenderers = new ThumbnailRenderer[batchTasks.Length];
            for(int i = 0; i < thumbnailRenderers.Length; ++i)
            {
                thumbnailRenderers[i] = new ThumbnailRenderer(ThumbnailSize, OnThumbnailComplete);
            }

            for (int imageProcessorIndex = 0; imageProcessorIndex < imageProcessors.Count; imageProcessorIndex++)
            {
                var imageProcessor = imageProcessors[imageProcessorIndex];

                var sourceBitmap = imageProcessor.CanRenderAtPreviewSize
                    ? thumbnailSizedSourceBitmap
                    : thumbnailSizedSourceBitmap;// fullSizedSourceBitmap;

                var taskIndex = imageProcessorIndex % batchTasks.Length;

                batchTasks[taskIndex] = batchTasks[taskIndex].ContinueWith(_ =>
                    {
                        return thumbnailRenderers[taskIndex].RenderAsync(sourceBitmap, imageProcessor);

                    }, TaskContinuationOptions.ExecuteSynchronously);
            }

            await Task.WhenAll(batchTasks).ConfigureAwait(false);
        }

        private Size ThumbnailSize
        {
            get
            {
                var aspectRatio = m_sourceSize.Width / m_sourceSize.Height;
                var thumnailImageHeight = ThumbnailWidth / aspectRatio;
                return new Size(ThumbnailWidth, thumnailImageHeight);
            }
        }

        private Size NormalizeSourceSize(Size sourceSize)
        {
            const int smallerAxisFor5MPixels = 1728;
            double scaleFactor = smallerAxisFor5MPixels / Math.Min(sourceSize.Width, sourceSize.Height);
            return new Size((int)(sourceSize.Width * scaleFactor), (int)(sourceSize.Height * scaleFactor));
        }

		private static void OnThumbnailComplete(IImageProcessor processor, SoftwareBitmap softwareBitmap)
        {
            // Note: intentionally not awaited. We just deliver the thumbnail Bitmaps to the UI thread.
            TaskUtilities.RunOnDispatcherThreadAsync(() =>
            {
				WriteableBitmap writeableBitmap = new WriteableBitmap(softwareBitmap.PixelWidth, softwareBitmap.PixelHeight);
				softwareBitmap.CopyToBuffer(writeableBitmap.PixelBuffer);
				writeableBitmap.Invalidate();
                processor.ThumbnailImagSource = writeableBitmap;

                // The thumbnail image has been copied, so we can destroy the Bitmap.
                softwareBitmap.Dispose();
            });
        }

		public Task<RenderResult> RenderAsync(IImageProcessor imageProcessor, RenderOption renderOptions = RenderOption.None)
        {
            if (imageProcessor == null)
            {
                throw new ArgumentNullException("imageProcessor");
            }

            m_currentRenderOption = renderOptions;

            lock (m_taskQueueLock)
            {
                var renderTask = m_lastQueuedTask.ContinueWith(_ => RenderInternalAsync(m_source, m_sourceSize, imageProcessor, renderOptions)).Unwrap();
                m_lastQueuedTask = renderTask;
                return renderTask;
            }
        }

        private Task<RenderResult> RenderInternalAsync(IImageProvider source, Size sourceSize, IImageProcessor processor, RenderOption renderOptions)
        {
            if (source == null)
            {
                throw new NullReferenceException("source");
            }

            processor.OnBeforeRender();
            var renderAtSourceSize = (renderOptions & RenderOption.RenderAtSourceSize) == RenderOption.RenderAtSourceSize;

            var renderSize = processor.CanRenderAtPreviewSize && !renderAtSourceSize ? PreviewSize : sourceSize;

            Debug.WriteLine(string.Format("RenderEffectAsync {0} sourceSize = {1}, renderSize = {2}", processor.Name,
                sourceSize, renderSize));

            var stopwatch = Stopwatch.StartNew();

            var effect = processor.GetEffectAsync(source, sourceSize, renderSize);

            if (effect.IsSynchronous)
            {
                return FinishRender(effect.Result, stopwatch, renderOptions, renderSize);
            }
            else
            {
                return effect.Task.ContinueWith(effectTask => FinishRender(effectTask.Result, stopwatch, renderOptions, renderSize), TaskContinuationOptions.OnlyOnRanToCompletion).Unwrap();
            }
        }

        private Task<RenderResult> FinishRender(IImageProvider effect, Stopwatch setupStopwatch, RenderOption renderOptions, Size renderSize)
        {
            setupStopwatch.Stop();
            long setupTimeMillis = setupStopwatch.ElapsedMilliseconds;

            m_currentRenderOption = renderOptions;

            if ((renderOptions & RenderOption.RenderToJpeg) == RenderOption.RenderToJpeg)
            {
                return RenderJpegAsync(effect, renderSize, setupTimeMillis);
            }
            else
            {
                return RenderSwapChainPanelAsync(effect, renderSize, setupTimeMillis);
            }
        }

        private static async Task<RenderResult> RenderJpegAsync(IImageProvider effect, Size renderSize, long setupTimeMillis)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            IBuffer jpegBuffer;

            using (var jpegRenderer = new JpegRenderer(effect) { Size = renderSize, Quality = 1.0 })
            {
                jpegBuffer = await jpegRenderer.RenderAsync().AsTask().ConfigureAwait(false);
            }

            stopwatch.Stop();
            long renderTimeMillis = stopwatch.ElapsedMilliseconds;

            return new RenderResult(jpegBuffer, renderSize, setupTimeMillis, renderTimeMillis);
        }

        private volatile bool m_isRendering;
        //private SurfaceImageSource m_surfaceImageSource;

        public SwapChainPanel SwapChainPanel { get; set; }

        private async Task<RenderResult> RenderSwapChainPanelAsync(IImageProvider effect, Size renderSize, long setupTimeMillis)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();          

            if (m_swapChainPanelRenderer == null)
            {
                m_swapChainPanelRenderer = new SwapChainPanelRenderer(effect, SwapChainPanel);
            }
            m_swapChainPanelRenderer.RenderOptions = Lumia.Imaging.RenderOptions.Mixed;
            m_swapChainPanelRenderer.Source = effect;

            await m_swapChainPanelRenderer.RenderAsync().AsTask().ConfigureAwait(false);

            stopwatch.Stop();
            long renderTimeMillis = stopwatch.ElapsedMilliseconds;
            Debug.WriteLine("Finished rendering " + renderTimeMillis);
            m_isRendering = false;
            return new RenderResult(SwapChainPanel, renderSize, setupTimeMillis, renderTimeMillis);
        }

        public Task LoadPhotoAsync(IImageProvider imageProvider)
        {
            lock (m_taskQueueLock)
            {
                m_lastQueuedTask = m_lastQueuedTask.ContinueWith(async t =>
                {
                    SourcePreviewBitmap = await LoadPhotoInternalAsync(imageProvider).ConfigureAwait(false);

                    RaiseSourcePreviewAvailable();

                    // Note: Intentionally not awaited.
                    // ReSharper disable once CSharpWarnings::CS4014
                    UpdateThumbnailsAsync();
                });

                return m_lastQueuedTask;
            }
        }

        public SoftwareBitmap SourcePreviewBitmap { get; private set; }

        private void RaiseSourcePreviewAvailable()
        {
            var sourcePreviewAvailable = SourcePreviewAvailable;
            if (sourcePreviewAvailable != null)
            {
                sourcePreviewAvailable(this, EventArgs.Empty);
            }
        }

        private async Task<SoftwareBitmap> LoadPhotoInternalAsync(IImageProvider unnormalizedSource)
        {
            DisposableHelper.TryDisposeAndSetToNull(ref m_source);

            var info = await unnormalizedSource.GetInfoAsync().AsTask().ConfigureAwait(false);
            var unnormalizedSourceSize = info.ImageSize;

            if ((uint)unnormalizedSourceSize.Width == 0 || (uint)unnormalizedSourceSize.Height == 0)
            {
                throw new ArgumentException("Image source appears to be zero sized.");
            }

            // Normalize source bitmap to ~5 MP
            m_sourceSize = NormalizeSourceSize(unnormalizedSourceSize);

			using(var bitmapRenderer = new SoftwareBitmapRenderer(unnormalizedSource))
            {
                bitmapRenderer.Size = m_sourceSize;
                bitmapRenderer.OutputOption = OutputOption.PreserveAspectRatio;
                bitmapRenderer.RenderOptions = RenderOptions.Cpu;

                var normalizedSourceBitmap = await bitmapRenderer.RenderAsync().AsTask().ConfigureAwait(false);

                m_source = new SoftwareBitmapImageSource(normalizedSourceBitmap);
            }

            using(var bitmapRenderer = new SoftwareBitmapRenderer(m_source))
            {
                bitmapRenderer.Size = PreviewSize;
                bitmapRenderer.OutputOption = OutputOption.PreserveAspectRatio;
                bitmapRenderer.RenderOptions = RenderOptions.Cpu;
                
                return await bitmapRenderer.RenderAsync().AsTask().ConfigureAwait(false);
            }
		}

		public event EventHandler SourcePreviewAvailable;

        public Size PreviewSize
        {
            get
            {
                double aspectRatio = m_sourceSize.Width / m_sourceSize.Height;

                if ((int)m_requestedPreviewSize.Height == 0)
                {
                    if ((int)m_requestedPreviewSize.Width == 0)
                    {
                        return m_sourceSize;
                    }
                    else
                    {
                        var previewHeight = (int)(m_requestedPreviewSize.Width / aspectRatio);
                        return new Size(m_requestedPreviewSize.Width, previewHeight);
                    }
                }
                else
                {
                    if ((int)m_requestedPreviewSize.Width == 0)
                    {
                        var previewWidth = (int)(m_requestedPreviewSize.Height * aspectRatio);
                        return new Size(previewWidth, m_requestedPreviewSize.Height);
                    }
                    else
                    {
                        return m_sourceSize;
                    }
                }
            }
        }

    }
}
