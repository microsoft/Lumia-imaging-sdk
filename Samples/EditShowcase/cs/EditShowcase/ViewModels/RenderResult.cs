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
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace Lumia.Imaging.EditShowcase.ViewModels
{
    public struct RenderResult
    {
#if WINDOWS_UWP
		public RenderResult(SoftwareBitmap softwareBitmap, Size size, long setupTimeMillis, long renderTimeMillis)
		{
			SoftwareBitmap = softwareBitmap;
			Bitmap = null;
			Buffer = null;
			SwapChainPanel = null;
			Size = size;
			SetupTimeMillis = setupTimeMillis;
			RenderTimeMillis = renderTimeMillis;
		}

#endif
		public RenderResult(Bitmap bitmap, Size size, long setupTimeMillis, long renderTimeMillis)
        {
#if WINDOWS_UWP
			SoftwareBitmap = null;
#endif
			Bitmap = bitmap;
            Buffer = null;
            SwapChainPanel = null;
            Size = size;
            SetupTimeMillis = setupTimeMillis;
            RenderTimeMillis = renderTimeMillis;
        }

        public RenderResult(IBuffer buffer, Size size, long setupTimeMillis, long renderTimeMillis)
        {
#if WINDOWS_UWP
			SoftwareBitmap = null;
#endif
			Bitmap = null;
            Buffer = buffer;
            SwapChainPanel = null;
            Size = size;
            SetupTimeMillis = setupTimeMillis;
            RenderTimeMillis = renderTimeMillis;
        }

        public RenderResult(SwapChainPanel swapChainPanel, Size size, long setupTimeMillis, long renderTimeMillis)
        {
#if WINDOWS_UWP
			SoftwareBitmap = null;
#endif
			Bitmap = null;
            Buffer = null;
            SwapChainPanel = swapChainPanel;
            Size = size;
            SetupTimeMillis = setupTimeMillis;
            RenderTimeMillis = renderTimeMillis;
        }

#if WINDOWS_UWP
		public readonly SoftwareBitmap SoftwareBitmap;
#endif
		public readonly Bitmap Bitmap;
        public readonly IBuffer Buffer;
        public readonly SwapChainPanel SwapChainPanel;
        public readonly Size Size;
        public readonly long SetupTimeMillis;
        public readonly long RenderTimeMillis;
    }
}