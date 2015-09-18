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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;

namespace Lumia.Imaging.Extras.Extensions
{
    public static class BitmapExtensions
    {
        internal class AsyncOperationBitmapProvider : IReadableBitmapProvider
        {
            private readonly IAsyncOperation<IReadableBitmap> m_bitmapAsyncOperation;

            public AsyncOperationBitmapProvider(IAsyncOperation<Bitmap> bitmapAsyncOperation)
            {
                m_bitmapAsyncOperation = bitmapAsyncOperation
                    .AsTask()
                    .ContinueWith(t => (IReadableBitmap)t.Result, TaskContinuationOptions.OnlyOnRanToCompletion|TaskContinuationOptions.ExecuteSynchronously)
                    .AsAsyncOperation();
            }

            public AsyncOperationBitmapProvider(IAsyncOperation<IReadableBitmap> bitmapAsyncOperation)
            {
                m_bitmapAsyncOperation = bitmapAsyncOperation;
            }

            public IAsyncOperation<IReadableBitmap> GetAsync()
            {
                return m_bitmapAsyncOperation;
            }
        }

        /// <summary>
        /// Adapts the Task&lt;IReadableBitmap&gt; to work as an IReadableBitmapProvider suitable for BitmapProviderImageSource.
        /// </summary>
        /// <param name="bitmapTask">An asynchronous task that will result in an IReadableBitmap containing an image.</param>
        /// <returns>An IReadableBitmapProvider.</returns>
        public static IReadableBitmapProvider AsBitmapProvider(this Task<IReadableBitmap> bitmapTask)
        {
            return new AsyncOperationBitmapProvider(bitmapTask.AsAsyncOperation());
        }

        /// <summary>
        /// Adapts the IAsyncOperation&lt;IReadableBitmap&gt; to work as an IReadableBitmapProvider suitable for BitmapProviderImageSource.
        /// </summary>
        /// <param name="bitmapAsyncOperation">An asynchronous operation that will result in an IReadableBitmap containing an image.</param>
        /// <returns>An IReadableBitmapProvider.</returns>
        public static IReadableBitmapProvider AsBitmapProvider(this IAsyncOperation<IReadableBitmap> bitmapAsyncOperation)
        {
            return new AsyncOperationBitmapProvider(bitmapAsyncOperation);
        }

        /// <summary>
        /// Adapts the Task&lt;Bitmap&gt; to work as an IReadableBitmapProvider suitable for BitmapProviderImageSource.
        /// </summary>
        /// <param name="bitmapTask">An asynchronous task that will result in a Bitmap containing an image.</param>
        /// <returns>An IReadableBitmapProvider.</returns>
        public static IReadableBitmapProvider AsBitmapProvider(this Task<Bitmap> bitmapTask)
        {
            return new AsyncOperationBitmapProvider(bitmapTask.AsAsyncOperation());
        }

        /// <summary>
        /// Adapts the IAsyncOperation&lt;Bitmap&gt; to work as an IReadableBitmapProvider suitable for BitmapProviderImageSource.
        /// </summary>
        /// <param name="bitmapAsyncOperation">An asynchronous operation that will result in a Bitmap containing an image.</param>
        /// <returns>An IReadableBitmapProvider.</returns>
        public static IReadableBitmapProvider AsBitmapProvider(this IAsyncOperation<Bitmap> bitmapAsyncOperation)
        {
            return new AsyncOperationBitmapProvider(bitmapAsyncOperation);
        }

        /// <summary>
        /// Copies the bitmap contents into an existing WriteableBitmap. This method can only be called in the CoreDispatcher synchronization context.
        /// </summary>
        /// <param name="bitmap">Source Bitmap. Must have ColorMode.Bgra8888.</param>
        /// <param name="writeableBitmap">Target WriteableBitmap. Must be the same dimensions as the source Bitmap.</param>
        /// <returns></returns>
        /// <remarks>
        /// This special case operation can be useful when the time spent in the dispatcher thread should be kept to a minimum, at the cost of the memory consumed by two copies of the image.
        /// <para>
        /// For example: a number of thumbnails must be generated and displayed in a XAML UI. BitmapRenderers are to generate the Bitmaps, doing all the actual rendering in the ThreadPool.
        /// The final conversions from Bitmaps to WriteableBitmaps are done using CoreDispatcher.RunAsync and calling this method.
        /// </para>
        /// </remarks>
        public static void CopyTo(this Bitmap bitmap, WriteableBitmap writeableBitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            if (writeableBitmap == null)
                throw new ArgumentNullException("writeableBitmap");

            var firstBuffer = bitmap.Buffers[0];

            var bitmapWidth = (int)bitmap.Dimensions.Width;
            var bitmapHeight = (int)bitmap.Dimensions.Height;

            if (bitmapWidth != writeableBitmap.PixelWidth ||
                bitmapHeight != writeableBitmap.PixelHeight)
            {
                throw new ArgumentException("The WriteableBitmap must have the same dimensions as the Bitmap.");
            }

            if (bitmap.ColorMode != ColorMode.Bgra8888)
            {
                throw new ArgumentException("The source Bitmap must have ColorMode.Bgra8888.");
            }

            var bitmapWidthBytes = (uint)(4 * bitmapWidth);

            var sourcePixels = firstBuffer.Buffer;

            var writeableBitmapBuffer = writeableBitmap.PixelBuffer;
            uint pitch = firstBuffer.Pitch;

            if (pitch == bitmapWidthBytes)
            {
                sourcePixels.CopyTo(0U, writeableBitmapBuffer, 0U, (uint) (bitmapWidthBytes*bitmapHeight));
            }
            else
            {
                for (int y = 0; y < bitmapHeight; ++y)
                {
                    sourcePixels.CopyTo((uint) (y*pitch), writeableBitmapBuffer, (uint) (y*bitmapWidthBytes), bitmapWidthBytes);
                }
            }
            writeableBitmap.Invalidate();
        }


        /// <summary>
        /// Copies the bitmap into a new WriteableBitmap. This method can only be called in the CoreDispatcher synchronization context.
        /// </summary>
        /// <param name="bitmap">Source bitmap. Must have ColorMode.Bgra8888.</param>
        /// <returns></returns>
        /// <remarks>
        /// This special case operation can be useful when the time spent in the dispatcher thread should be kept to a minimum, at the cost of the memory consumed by two copies of the image.
        /// <para>
        /// For example: a number of thumbnails must be generated and displayed in a XAML UI. BitmapRenderers are to generate the Bitmaps, doing all the actual rendering in the ThreadPool.
        /// The final conversions from Bitmaps to WriteableBitmaps are done using CoreDispatcher.RunAsync and calling this method.
        /// </para>
        /// </remarks>
        public static WriteableBitmap ToWriteableBitmap(this Bitmap bitmap)
        {
            var firstBuffer = bitmap.Buffers[0];

            var sourcePixels = firstBuffer.Buffer;

            var bitmapWidth = (int)bitmap.Dimensions.Width;
            var bitmapHeight = (int)bitmap.Dimensions.Height;

            var writeableBitmap = new WriteableBitmap(bitmapWidth, bitmapHeight);

            if (bitmap.ColorMode != ColorMode.Bgra8888)
            {
                throw new ArgumentException("The source Bitmap must have ColorMode.Bgra8888.");
            }

            var bitmapWidthBytes = (uint)(4 * bitmapWidth);

            var writeableBitmapBuffer = writeableBitmap.PixelBuffer;
            uint pitch = firstBuffer.Pitch;

            if (pitch == bitmapWidthBytes)
            {
                sourcePixels.CopyTo(0U, writeableBitmapBuffer, 0U, (uint)(bitmapWidthBytes*bitmapHeight));                
            }
            else
            {
                for (int y = 0; y < bitmapHeight; ++y)
                {
                    sourcePixels.CopyTo((uint)(y * pitch), writeableBitmapBuffer, (uint)(y * bitmapWidthBytes), bitmapWidthBytes);
                }
                
            }


            writeableBitmap.Invalidate();

            return writeableBitmap;
        }

    }
}
