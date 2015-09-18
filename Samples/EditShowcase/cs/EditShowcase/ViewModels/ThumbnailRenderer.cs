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
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Lumia.Imaging.EditShowcase.Interfaces;

namespace Lumia.Imaging.EditShowcase.ViewModels
{

	public class ThumbnailRenderer
    {
        private readonly Size m_thumbnailSize;
        private readonly Action<IImageProcessor, SoftwareBitmap> m_thumbnailCompleteAction;
        private SoftwareBitmapRenderer m_bitmapRenderer;

        public ThumbnailRenderer(Size thumbnailSize, Action<IImageProcessor, SoftwareBitmap> thumbnailCompleteAction)
        {
            m_thumbnailSize = thumbnailSize;
            m_thumbnailCompleteAction = thumbnailCompleteAction;
            m_bitmapRenderer = new SoftwareBitmapRenderer();
            m_bitmapRenderer.Size = thumbnailSize;
        }

		public Task RenderAsync(SoftwareBitmap sourceBitmap, IImageProcessor processor)
        {
			var sourceBitmapSize = new Size(sourceBitmap.PixelWidth, sourceBitmap.PixelHeight);

			Debug.WriteLine(string.Format("RenderThumbnailAsync {0} sourceSize = {1} thumbnailSize = {2}", processor.Name, sourceBitmapSize, m_thumbnailSize));

            var effect = processor.GetEffectAsync(new SoftwareBitmapImageSource(sourceBitmap), sourceBitmapSize, m_thumbnailSize);

            // Avoid creating a Task object on the heap if not necessary.
            if (effect.IsSynchronous)
            {
                return FinishRenderAsync(effect.Result, processor);
            }
            else
            {
                return effect.Task.ContinueWith(effectTask => FinishRenderAsync(effectTask.Result, processor), TaskContinuationOptions.OnlyOnRanToCompletion);
            }
        }

        private async Task FinishRenderAsync(IImageProvider effect, IImageProcessor processor)
        {         
            m_bitmapRenderer.Source = effect;
       
            var thumbnailTargetBitmap = await m_bitmapRenderer.RenderAsync().AsTask().ConfigureAwait(false);
            
            m_thumbnailCompleteAction(processor, thumbnailTargetBitmap);
        }
    }

}
