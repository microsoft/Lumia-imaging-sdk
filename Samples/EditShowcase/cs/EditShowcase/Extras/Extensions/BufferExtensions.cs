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
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace Lumia.Imaging.Extras.Extensions
{
    public static class BufferExtensions
    {
        internal class AsyncOperationBufferProvider : IBufferProvider
        {
            private readonly Task<IBuffer> m_bufferTask;

            public AsyncOperationBufferProvider(Task<IBuffer> bufferTask)
            {
                m_bufferTask = bufferTask;
            }

            public IAsyncOperation<IBuffer> GetAsync()
            {
                return m_bufferTask.AsAsyncOperation();
            }
        }

        /// <summary>
        /// Adapts the Task&lt;IBuffer&gt; to work as an IBufferProvider suitable for BufferProviderImageSource.
        /// </summary>
        /// <param name="bufferTask">An asynchronous task that will result in an IBuffer containing an image.</param>
        /// <returns>An IBufferProvider.</returns>
        public static IBufferProvider AsBufferProvider(this Task<IBuffer> bufferTask)
        {
            return new AsyncOperationBufferProvider(bufferTask);
        }
        
        /// <summary>
        /// Adapts the IAsyncOperation&lt;IBuffer&gt; to work as an IBufferProvider suitable for BufferProviderImageSource.
        /// </summary>
        /// <param name="bufferAsyncOperation">An asynchronous operation that will result in an IBuffer containing an image.</param>
        /// <returns>An IBufferProvider.</returns>
        public static IBufferProvider AsBufferProvider(this IAsyncOperation<IBuffer> bufferAsyncOperation)
        {
            return new AsyncOperationBufferProvider(bufferAsyncOperation.AsTask());
        }
    }
}
