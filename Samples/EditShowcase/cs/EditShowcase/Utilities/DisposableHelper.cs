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

namespace Lumia.Imaging.EditShowcase.Utilities
{
    public static class DisposableHelper
    {
        public static void TryDisposeAndSetToNull<T>(ref T obj) where T : class
        {
            var disposable = obj as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }

            obj = null;
        }

        public static void TryDisposeAndSetToNull<T>(ref Task<T> objTask) where T : class
        {
            if (objTask == null)
            {
                return;
            }

            var disposable = objTask.Result as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }

            objTask = null;
        }

        public static void TryDisposeAndSetToNull<T>(ref T[] objs) where T : class
        {
            if (objs == null)
            {
                return;
            }

            foreach (var obj in objs)
            {
                var disposable = obj as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }

            objs = null;
        }
    }
}