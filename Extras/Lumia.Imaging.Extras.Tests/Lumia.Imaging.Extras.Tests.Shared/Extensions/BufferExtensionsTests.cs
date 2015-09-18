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

using Lumia.Imaging.Extras.Extensions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Lumia.Imaging.Extras.Tests.Extensions
{
    [TestClass]
    public class BufferExtensionsTests
    {
        [TestMethod]
        public async Task TaskOfBufferAsBufferProvider()
        {
            var taskOfBuffer = Task.Run(() => Enumerable.Range(1, 10).Select(i => (byte)i).ToArray().AsBuffer());

            var bufferProvider = taskOfBuffer.AsBufferProvider();
            
            IBuffer buffer = await bufferProvider.GetAsync().AsTask().ConfigureAwait(false);

            CollectionAssert.AreEqual(Enumerable.Range(1, 10).Select(i => (byte)i).ToArray(), buffer.ToArray());
        }

        [TestMethod]
        public async Task AsyncOperationOfBufferAsBufferProvider()
        {
            var asyncOperationOfBuffer = AsyncInfo.Run(cancellationToken => Task.Run(() => Enumerable.Range(1, 10).Select(i => (byte)i).ToArray().AsBuffer(), cancellationToken));

            var bufferProvider = asyncOperationOfBuffer.AsBufferProvider();

            IBuffer buffer = await bufferProvider.GetAsync().AsTask().ConfigureAwait(false);

            CollectionAssert.AreEqual(Enumerable.Range(1, 10).Select(i => (byte)i).ToArray(), buffer.ToArray());
        }

    }
}
