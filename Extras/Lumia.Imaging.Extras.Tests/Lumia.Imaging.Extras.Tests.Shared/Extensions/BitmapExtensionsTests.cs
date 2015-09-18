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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Lumia.Imaging.Extras.Tests.Extensions
{
    [TestClass]
    public class BitmapExtensionsTests
    {
        [TestMethod]
        public async Task TaskOfBitmapAsBitmapProvider()
        {
            var bitmap = new Bitmap(new Size(10, 10), ColorMode.Bgra8888);

            Task<Bitmap> taskOfBitmap = Task.Run(() => bitmap);

            var bitmapProvider = taskOfBitmap.AsBitmapProvider();

            var returnedBitmap = await bitmapProvider.GetAsync().AsTask().ConfigureAwait(false);

            Assert.AreSame(bitmap, returnedBitmap);
        }

        [TestMethod]
        public async Task AsyncOperationOfBitmapAsBitmapProvider()
        {
            var bitmap = new Bitmap(new Size(10, 10), ColorMode.Bgra8888);

            IAsyncOperation<Bitmap> asyncOperationOfBitmap = AsyncInfo.Run(cancellationToken => Task.Run(() => bitmap, cancellationToken));

            var bitmapProvider = asyncOperationOfBitmap.AsBitmapProvider();

            var returnedBitmap = await bitmapProvider.GetAsync().AsTask().ConfigureAwait(false);

            Assert.AreSame(bitmap, returnedBitmap);
        }

        [TestMethod]
        public async Task TaskOfIReadableBitmapAsBitmapProvider()
        {
            var bitmap = new Bitmap(new Size(10, 10), ColorMode.Bgra8888);

            Task<IReadableBitmap> taskOfBitmap = Task.Run(() => (IReadableBitmap)bitmap);

            var bitmapProvider = taskOfBitmap.AsBitmapProvider();

            var returnedBitmap = await bitmapProvider.GetAsync().AsTask().ConfigureAwait(false);

            Assert.AreSame(bitmap, returnedBitmap);
        }

        [TestMethod]
        public async Task AsyncOperationOfIReadableBitmapAsBitmapProvider()
        {
            var bitmap = new Bitmap(new Size(10, 10), ColorMode.Bgra8888);

            IAsyncOperation<IReadableBitmap> taskOfBitmap = AsyncInfo.Run(cancellationToken => Task.Run(() => (IReadableBitmap)bitmap, cancellationToken));

            var bitmapProvider = taskOfBitmap.AsBitmapProvider();

            var returnedBitmap = await bitmapProvider.GetAsync().AsTask().ConfigureAwait(false);

            Assert.AreSame(bitmap, returnedBitmap);
        }

    }
}
