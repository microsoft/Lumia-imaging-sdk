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

namespace Lumia.Imaging.Extras.Layers.Internal
{
    internal static class LayerSource
    {
        private const TaskContinuationOptions ContinuationOptions =
            TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion;

        private static IImageProvider Bind(IImageProvider currentImage, IImageProvider previousImage)
        {
            var imageConsumer = currentImage as IImageConsumer;
            if (imageConsumer != null)
            {
                imageConsumer.Source = previousImage;
            }

            return currentImage;
        }

        internal static MaybeTask<IImageProvider> Bind(MaybeTask<IImageProvider> currentImage, MaybeTask<IImageProvider> previousImage)
        {
            // Note: the code below is written with static lambdas that don't close over any variables. This avoids heap allocations.

            if (currentImage.IsSynchronous)
            {
                if (previousImage.IsSynchronous)
                {
                    return new MaybeTask<IImageProvider>(Bind(currentImage.Result, previousImage.Result));
                }
                else // previousImage.IsAsynchronous
                {
                    return new MaybeTask<IImageProvider>(previousImage.Task.ContinueWith(BindPreviousAsyncToCurrentSync, currentImage.Result, ContinuationOptions));
                }
            }
            else // currentImage.IsAsynchronous
            {
                if (previousImage.IsSynchronous)
                {
                    return new MaybeTask<IImageProvider>(currentImage.Task.ContinueWith(BindPreviousSyncToCurrentAsync, previousImage.Result, ContinuationOptions));
                }
                else // previousImage.IsAsynchronous
                {
                    return new MaybeTask<IImageProvider>(currentImage.Task.ContinueWith(BindPreviousAsyncToCurrentAsync, previousImage.Task, ContinuationOptions).Unwrap());
                }
            }
        }

        private static readonly Func<Task<IImageProvider>, object, IImageProvider> BindPreviousAsyncToCurrentSync = 
            (previousImageTask, state) => Bind((IImageProvider)state, previousImageTask.Result);

        private static readonly Func<Task<IImageProvider>, object, IImageProvider> BindPreviousSyncToCurrentAsync = 
            (currentImageTask, state) => Bind(currentImageTask.Result, (IImageProvider)state);

        private static readonly Func<Task<IImageProvider>, object, Task<IImageProvider>> BindPreviousAsyncToCurrentAsync = 
            (currentImageTask, previousImageTaskObj) => ((Task<IImageProvider>)previousImageTaskObj).ContinueWith(
                (previousImageTask, state) => Bind((IImageProvider) state, previousImageTask.Result), 
                currentImageTask.Result, 
                ContinuationOptions);

    }
}
