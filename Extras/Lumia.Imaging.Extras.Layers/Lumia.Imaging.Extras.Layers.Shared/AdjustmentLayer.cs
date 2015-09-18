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
using Windows.Foundation;

namespace Lumia.Imaging.Extras.Layers
{
    /// <summary>
    /// Represents an adjustment layer containing an effect or a list of filters that will be applied to the image.
    /// </summary>
    public sealed class AdjustmentLayer : Layer
    {
        private const TaskContinuationOptions ContinuationOptions =
            TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion;

        /// <summary>
        /// Creates an adjustment layer containing the specified effect.
        /// </summary>
        /// <param name="layerStyle">Layer style to use for this layer.</param>
        /// <param name="effect">The effect to apply in the adjustment layer.</param>
        /// <returns>The adjustment layer.</returns>
        public AdjustmentLayer(LayerStyle layerStyle, IImageProvider effect) :
            this(layerStyle, context => effect)
        {
        }

        /// <summary>
        /// Creates an adjustment layer containing the specified effect.
        /// </summary>
        /// <param name="layerStyle">Layer style to use for this layer.</param>
        /// <param name="effectTask">An already started task that asynchronously results in the effect to apply in the adjustment layer.</param>
        /// <returns>The adjustment layer.</returns>
        public AdjustmentLayer(LayerStyle layerStyle, Task<IImageProvider> effectTask) :
            base(layerStyle, context => new MaybeTask<IImageProvider>(effectTask), Size.Empty)
        {
        }

        /// <summary>
        /// Creates an adjustment layer containing the specified effect.
        /// </summary>
        /// <param name="layerStyle">Layer style to use for this layer.</param>
        /// <param name="effectTask">An already started task that asynchronously results in the effect to apply in the adjustment layer.</param>
        /// <returns>The adjustment layer.</returns>
        public AdjustmentLayer(LayerStyle layerStyle, MaybeTask<IImageProvider> effectTask) :
            base(layerStyle, effectTask, Size.Empty)
        {
        }

        /// <summary>
        /// Creates an adjustment layer containing the specified effect.
        /// </summary>
        /// <param name="layerStyle">Layer style to use for this layer.</param>
        /// <param name="effectResolver">A lazy resolve func that returns the effect <see cref="IImageProvider" /> based on context sensitive values such as source image size.</param>
        /// <returns>The adjustment layer.</returns>
        public AdjustmentLayer(LayerStyle layerStyle, Func<LayerContext, IImageProvider> effectResolver) :
            base(layerStyle, context => new MaybeTask<IImageProvider>(effectResolver(context)), Size.Empty)
        {
        }

        /// <summary>
        /// Creates an adjustment layer containing the specified effect.
        /// </summary>
        /// <param name="layerStyle">Layer style to use for this layer.</param>
        /// <param name="effectResolver">A lazy resolve func that asynchronously returns the effect <see cref="IImageProvider" /> based on context sensitive values such as source image size.</param>
        /// <returns>The adjustment layer.</returns>
        public AdjustmentLayer(LayerStyle layerStyle, Func<LayerContext, Task<IImageProvider>> effectResolver) :
            base(layerStyle, context => new MaybeTask<IImageProvider>(effectResolver(context)), Size.Empty)
        {
        }

        /// <summary>
        /// Creates an adjustment layer containing the specified effect.
        /// </summary>
        /// <param name="layerStyle">Layer style to use for this layer.</param>
        /// <param name="effectResolver">A lazy resolve func that asynchronously returns the effect <see cref="IImageProvider" /> based on context sensitive values such as source image size.</param>
        /// <returns>The adjustment layer.</returns>
        public AdjustmentLayer(LayerStyle layerStyle, Func<LayerContext, MaybeTask<IImageProvider>> effectResolver) :
            base(layerStyle, effectResolver, Size.Empty)
        {
        }
    }
}
