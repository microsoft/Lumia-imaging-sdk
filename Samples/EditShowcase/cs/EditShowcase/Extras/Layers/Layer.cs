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

using Lumia.Imaging.Extras;
using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Lumia.Imaging.Extras.Layers
{
    /// <summary>
    /// Represents a layer with image content.
    /// </summary>
    public class Layer
    {
        internal readonly Func<LayerContext, MaybeTask<IImageProvider>> GetImageProvider;

        /// <summary>
        /// Style of the layer, including blend options.
        /// </summary>
        public readonly LayerStyle Style;
        
        /// <summary>
        /// The size of the image content. This is {0,0} if it wasn't set explicitly when creating the layer.
        /// </summary>
        public readonly Size ImageSize;

        /// <summary>
        /// Creates a layer with image content.
        /// </summary>
        /// <param name="layerStyle">Description of how to blend this layer onto the preceding one.</param>
        /// <param name="imageProvider">The image content of the layer.</param>
        /// <param name="imageSize">Optional, the size of the image content of the layer.</param>
        public Layer(LayerStyle layerStyle, IImageProvider imageProvider, Size imageSize = default(Size))
        {
            Style = layerStyle;
            GetImageProvider = context => new MaybeTask<IImageProvider>(imageProvider);
            ImageSize = imageSize;
        }

        /// <summary>
        /// Creates a layer with image content.
        /// </summary>
        /// <param name="layerStyle">Description of how to blend this layer onto the preceding one.</param>
        /// <param name="imageProviderTask">An already started task that asynchronously results in an <see cref="IImageProvider" /> that is the image content.</param>
        /// <param name="imageSize">Optional, the size of the image content of the layer.</param>
        public Layer(LayerStyle layerStyle, Task<IImageProvider> imageProviderTask, Size imageSize = default(Size))
        {
            Style = layerStyle;
            GetImageProvider = context => new MaybeTask<IImageProvider>(imageProviderTask);
            ImageSize = imageSize;
        }

        /// <summary>
        /// Creates a layer with image content.
        /// </summary>
        /// <param name="layerStyle">Description of how to blend this layer onto the preceding one.</param>
        /// <param name="imageProvider">A value that represents either the <see cref="IImageProvider" /> that is the image content or a task that asynchronously results in it.</param>
        /// <param name="imageSize">Optional, the size of the image content of the layer.</param>
        public Layer(LayerStyle layerStyle, MaybeTask<IImageProvider> imageProvider, Size imageSize = default(Size))
        {
            Style = layerStyle;
            GetImageProvider = context => imageProvider;
            ImageSize = imageSize;
        }

        /// <summary>
        /// Creates a layer with image content.
        /// </summary>
        /// <param name="layerStyle">Description of how to blend this layer onto the preceding one.</param>
        /// <param name="imageProviderResolver">A lazy resolve func that returns the <see cref="IImageProvider" /> based on context sensitive values such as source image size.</param>
        /// <param name="imageSize">Optional, the size of the image content of the layer.</param>
        public Layer(LayerStyle layerStyle, Func<LayerContext, IImageProvider> imageProviderResolver, Size imageSize = default(Size))
        {
            Style = layerStyle;
            GetImageProvider = context => new MaybeTask<IImageProvider>(imageProviderResolver(context));
            ImageSize = imageSize;
        }

        /// <summary>
        /// Creates a layer with image content.
        /// </summary>
        /// <param name="layerStyle">Description of how to blend this layer onto the preceding one.</param>
        /// <param name="imageProviderResolver">A lazy resolve func that returns the <see cref="IImageProvider" /> based on context sensitive values such as source image size.</param>
        /// <param name="imageSize">Optional, the size of the image content of the layer.</param>
        public Layer(LayerStyle layerStyle, Func<LayerContext, Task<IImageProvider>> imageProviderResolver, Size imageSize = default(Size))
        {
            Style = layerStyle;
            GetImageProvider = context => new MaybeTask<IImageProvider>(imageProviderResolver(context));
            ImageSize = imageSize;
        }

        /// <summary>
        /// Creates a layer with image content.
        /// </summary>
        /// <param name="layerStyle">Description of how to blend this layer onto the preceding one.</param>
        /// <param name="imageProviderResolver">A lazy resolve func that returns the <see cref="IImageProvider" /> based on context sensitive values such as source image size.</param>
        /// <param name="imageSize">Optional, the size of the image content of the layer.</param>
        public Layer(LayerStyle layerStyle, Func<LayerContext, MaybeTask<IImageProvider>> imageProviderResolver, Size imageSize = default(Size))
        {
            Style = layerStyle;
            GetImageProvider = imageProviderResolver;
            ImageSize = imageSize;
        }
    }
}
