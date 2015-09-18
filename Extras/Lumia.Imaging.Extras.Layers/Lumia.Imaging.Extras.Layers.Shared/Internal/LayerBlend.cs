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

using Lumia.Imaging.Compositing;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Lumia.Imaging.Extras.Layers.Internal
{
    internal static class LayerBlend
    {
        private const TaskContinuationOptions ContinuationOptions =
            TaskContinuationOptions.OnlyOnRanToCompletion |
            TaskContinuationOptions.ExecuteSynchronously;

        private class BlendParams
        {
            public MaybeTask<IImageProvider> PreviousImage;
            public MaybeTask<IImageProvider> Image;
            public MaybeTask<IImageProvider> Mask;
            public LayerStyle Style;
        }

        internal static MaybeTask<IImageProvider> Apply(LayerContext context, MaybeTask<IImageProvider> image)
        {
            var layer = context.CurrentLayer;

            var mask = new MaybeTask<IImageProvider>();

            if (layer.Style.MaskResolver != null)
            {
                mask = layer.Style.MaskResolver(context);                
            }

            const double opacityEpsilon = 0.01;
            bool isOpaque = context.CurrentLayerIndex == 0 ||
                ((layer is AdjustmentLayer) &&
                layer.Style.BlendFunction == BlendFunction.Normal &&
                Math.Abs(layer.Style.Opacity - 1.0) <= opacityEpsilon);

            if (isOpaque)
            {
                return new MaybeTask<IImageProvider>();
            }

            // This whole unfolded mess may look silly, but allows us to do the bare minimum. 
            // It defers resolving task-ness until later, so there are less objects allocated on average.
            // Using async/await causes quite a few extra objects on the heap.
            // We also try to avoid Task.FromResult which is at least one extra Task on the heap.
            // Task.WhenAll is not efficient in .NET 4.0, and it's unknown how that translates to Silverlight for WP 8.0.

            var blendParams = new BlendParams
            {
                PreviousImage = context.PreviousImage,
                Image = image,
                Mask = mask,
                Style = layer.Style
            };

            if (blendParams.PreviousImage.IsSynchronous)
            {
                if (blendParams.Image.IsSynchronous)
                {
                    if (blendParams.Mask.IsSynchronous || blendParams.Mask.IsEmpty)
                    {
                        return new MaybeTask<IImageProvider>(CreateBlendEffect(blendParams));
                    }
                    else
                    {
                        return new MaybeTask<IImageProvider>(
                            mask.Task.ContinueWith((maskTask, state) =>
                            {
                                var bp = (BlendParams)state;
                                bp.Mask = new MaybeTask<IImageProvider>(maskTask.Result);

                                return CreateBlendEffect(bp);

                            }, blendParams, ContinuationOptions));
                    }
                }
                else // image.IsAsynchronous
                {
                    if (blendParams.Mask.IsSynchronous || blendParams.Mask.IsEmpty)
                    {
                        return new MaybeTask<IImageProvider>(
                            image.Task.ContinueWith((imageTask, state) =>
                            {
                                var bp = (BlendParams)state;
                                bp.Image = new MaybeTask<IImageProvider>(imageTask.Result);

                                return CreateBlendEffect(bp);

                            }, blendParams, ContinuationOptions));
                    }
                    else // mask.IsAsynchronous
                    {
                        return new MaybeTask<IImageProvider>(
                            image.Task.ContinueWith((imageTask, state) =>
                            {
                                var bp = (BlendParams)state;
                                bp.Image = new MaybeTask<IImageProvider>(imageTask.Result);

                                return bp.Mask.Task.ContinueWith((maskTask, state2) =>
                                {
                                    var bp2 = (BlendParams)state2;
                                    bp2.Mask = new MaybeTask<IImageProvider>(maskTask.Result);

                                    return CreateBlendEffect(bp2);

                                }, bp, ContinuationOptions);

                            }, blendParams, ContinuationOptions).Unwrap());
                    }
                }
            }
            else // previousImage.IsAsynchronous
            {
                if (blendParams.Image.IsSynchronous)
                {
                    if (blendParams.Mask.IsSynchronous || blendParams.Mask.IsEmpty)
                    {
                        return new MaybeTask<IImageProvider>(
                            blendParams.PreviousImage.Task.ContinueWith((previousImageTask, state) =>
                            {
                                var bp = (BlendParams)state;
                                bp.PreviousImage = new MaybeTask<IImageProvider>(previousImageTask.Result);

                                return CreateBlendEffect(bp);

                            }, blendParams, ContinuationOptions));
                    }
                    else // mask.IsAsynchronous
                    {
                        return new MaybeTask<IImageProvider>(
                            blendParams.PreviousImage.Task.ContinueWith((previousImageTask, state2) =>
                            {
                                var blendParams2 = (BlendParams)state2;
                                blendParams2.PreviousImage = new MaybeTask<IImageProvider>(previousImageTask.Result);

                                return blendParams2.Mask.Task.ContinueWith((maskTask, state3) =>
                                {
                                    var blendParams3 = (BlendParams)state3;
                                    blendParams3.Mask = new MaybeTask<IImageProvider>(maskTask.Result);

                                    return CreateBlendEffect(blendParams3);

                                }, blendParams2, ContinuationOptions);

                            }, blendParams, ContinuationOptions).Unwrap());
                    }
                }
                else // image.IsAsynchronous
                {
                    if (mask.IsSynchronous || mask.IsEmpty)
                    {
                        return new MaybeTask<IImageProvider>(
                            blendParams.PreviousImage.Task.ContinueWith((previousImageTask, state) =>
                            {
                                var bp = (BlendParams)state;
                                bp.PreviousImage = new MaybeTask<IImageProvider>(previousImageTask.Result);

                                return bp.Image.Task.ContinueWith((imageTask, state2) =>
                                {
                                    var bp2 = (BlendParams)state2;
                                    bp2.Image = new MaybeTask<IImageProvider>(imageTask.Result);

                                    return CreateBlendEffect(bp2);

                                }, bp, ContinuationOptions);

                            }, blendParams, ContinuationOptions).Unwrap());
                    }
                    else // mask.IsAsynchronous
                    {
                        return new MaybeTask<IImageProvider>(
                            blendParams.PreviousImage.Task.ContinueWith((previousImageTask, state) =>
                            {
                                var bp = (BlendParams)state;
                                bp.PreviousImage = new MaybeTask<IImageProvider>(previousImageTask.Result);

                                return bp.Image.Task.ContinueWith((imageTask, state2) =>
                                {
                                    var bp2 = (BlendParams)state2;
                                    bp2.Image = new MaybeTask<IImageProvider>(imageTask.Result);

                                    return bp2.Mask.Task.ContinueWith((maskTask, state3) =>
                                    {
                                        var bp3 = (BlendParams)state3;
                                        bp3.Mask = new MaybeTask<IImageProvider>(maskTask.Result);

                                        return CreateBlendEffect(bp3);

                                    }, bp2, ContinuationOptions);

                                }, bp, ContinuationOptions);

                            }, blendParams, ContinuationOptions).Unwrap().Unwrap());
                    }
                }
            }

            throw new NotImplementedException();
        }

        private static IImageProvider CreateBlendEffect(BlendParams blendParams)
        {
            Debug.Assert(blendParams.Image.IsSynchronous);
            Debug.Assert(blendParams.PreviousImage.IsSynchronous);
            Debug.Assert(blendParams.Mask.IsSynchronous || blendParams.Mask.IsEmpty);

            var blendEffect = new BlendEffect(blendParams.PreviousImage.Result, blendParams.Image.Result, blendParams.Style.BlendFunction, blendParams.Style.Opacity);

            if (!blendParams.Mask.IsEmpty)
            {
                blendEffect.MaskSource = blendParams.Mask.Result;
            }

            if (blendParams.Style.TargetArea.HasValue)
            {
                blendEffect.TargetArea = blendParams.Style.TargetArea.Value;
            }

            return blendEffect;
        }
    }
}
