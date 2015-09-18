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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Lumia.Imaging.Extras.Layers.Internal;
using Size = Windows.Foundation.Size;
using Windows.Foundation;

namespace Lumia.Imaging.Extras.Layers
{
    /// <summary>
    /// A list of abstracted and developer-friendly layers that can be realized into a graph of <see cref="IImageProvider" /> objects.
    /// </summary>
    /// <remarks>
    /// The model used by the layer system is that each layer in a <see cref="LayerList" /> will be applied, in turn, on top of a background image. 
    /// The first layer in the list is at the bottom in the Z-order and the last layer is at the top. While the background image itself is not a concrete 
    /// <see cref="Layer" /> in the list, conceptually it is placed at the bottom below the first layer.
    /// <para>
    /// A layer object can be a <see cref="Layer" />, which is like a piece of paper (opaque or semi-transparent) placed on top of the previous
    /// composite image. How it is blended onto the image can be controlled using the <see cref="LayerStyle" />. In addition, the image content of
    /// the <see cref="Layer" /> is passed as an <see cref="IImageProvider" />. This <see cref="IImageProvider" /> must originate in an image source 
    /// (i.e. the developer is required to connect it all the way up to an image source).
    /// </para>
    /// <para>
    /// The second type of layer object is <see cref="AdjustmentLayer" />, which does not provide an image in itself. Instead it modifies the 
    /// existing image. The adjustment layer can be constructed with one or more <see cref="IFilter" /> objects, or with an <see cref="IImageProvider" />.
    /// The <see cref="IImageProvider" /> passed to an adjustment layer must be an effect (so, must also implement <see cref="IImageConsumer" />). 
    /// The layer system will automatically connect a source to it as necessary.
    /// </para>
    /// <para>
    /// An <see cref="AdjustmentLayer" /> that uses <see cref="BlendFunction.Normal" /> and an Opacity of 1.0 is applied without blending. This can
    /// be a significant performance win if no blending is required. Passing zero arguments to the <see cref="LayerStyle.Normal" /> factory method 
    /// will ensure this fast-path is used.
    /// </para>
    /// <para>
    /// Most methods that accept a <see cref="IFilter" /> or <see cref="IImageProvider" /> can be passed one of the following:
    /// <list type="bullet">
    /// <item><term>Object</term><description>T</description></item>
    /// <item><term>Delegate returning object</term><description>Func&lt;LayerContext, T&gt;</description></item>
    /// <item><term>Task resulting in object</term><description>Task&lt;T&gt;</description></item>
    /// <item><term>Delegate returning task resulting in object</term><description>Func&lt;LayerContext, Task&lt;T&gt;&gt;</description></item>
    /// <item><term>Object OR task resulting in object</term><description>MaybeTask&lt;T&gt;</description></item>
    /// </list>
    /// This allows the developer to express flexible behavior. For instance, asynchronous creation of layer content:
    /// <code>
    /// new Layer(LayerStyle.Screen(), LoadImageAsync("flower.jpg")) // LoadImageAsync task started now
    /// new Layer(LayerStyle.Screen(), context => LoadImageAsync("flower.jpg")) // LoadImageAsync task started each time the layer list is realized using ToImageProvider
    /// </code>
    /// </para>
    /// <para>
    /// As another example, the overloads taking delegates will pass back a <see cref="LayerContext" /> when the layer list is realized, allowing the 
    /// content of the layers to be adapted to the size of the background image. This ensures that the images match up when they are blended together.
    /// <code>
    /// new Layer(LayerStyle.Overlay(0.7), context => new GradientImageSource(context.BackgroundSize, myGradient))
    /// </code>
    /// </para>
    /// <para>
    /// Note also that <see cref="LayerList" /> objects can be nested, by passing a delegate that calls ToImageProvider on the inner layer list.
    /// This allows grouping of layers, or combining multiple layer lists in interesting ways.
    /// <code>
    /// new Layer(LayerStyle.Normal(), context => secondLayerList.ToImageProvider(context.BackgroundSize))
    /// </code>
    /// </para>
    /// </remarks>
    public class LayerList : List<Layer>, IDisposable
    {
        private static readonly Layer EmptyLayer = new Layer(LayerStyle.Normal(), (Func<LayerContext, MaybeTask<IImageProvider>>)null);

        /// <summary>
        /// Creates a LayerList without a background image specified. A background image must be specified in the call to <see cref="ToImageProvider(Lumia.Imaging.IImageProvider,Windows.Foundation.Size,Windows.Foundation.Size)"/>.
        /// </summary>
        /// <param name="layers">A list of layers to add following the background layer.</param>
        public LayerList(params Layer[] layers)
        {
            Add(EmptyLayer);
            AddRange(layers);
        }

        /// <summary>
        /// Creates a LayerList with a specified background image.
        /// </summary>
        /// <param name="backgroundImage">The image to use for the background layer.</param>
        /// <param name="backgroundImageSize">Optionally, the size of the background image. If omitted, the natural size of the source will be used, but the size will not available in the LayerLinkingContext for lazy resolvers.</param>
        /// <param name="layers">A list of layers to add following the background layer.</param>
        public LayerList(IImageProvider backgroundImage, Size backgroundImageSize, params Layer[] layers)
        {
            Add(new Layer(LayerStyle.Normal(), new MaybeTask<IImageProvider>(backgroundImage), backgroundImageSize));
            AddRange(layers);
        }

        ~LayerList()
        {
            Dispose(false);
        }

        /// <summary>
        /// Resolve an IImageProvider that represents the whole layer stack.
        /// </summary>
        /// <param name="backgroundImage">The image to use for the background layer.</param>
        /// <param name="backgroundImageSize">Optionally, the size of the background image. If omitted, the natural size of the source will be used, but the size will not available in the LayerLinkingContext for any lazy resolvers.</param>
        /// <param name="renderSizeHint">Optionally, the size of image that the layer stack will be rendered to. Note that this is just a hint, the resulting <see cref="IImageProvider" /> should also be rendered at this size by the user.</param>
        /// <returns>An IImageProvider that represents the whole layer stack.</returns>
        public MaybeTask<IImageProvider> ToImageProvider(IImageProvider backgroundImage, Size backgroundImageSize = default(Size), Size renderSizeHint = default(Size))
        {
            return ToImageProvider(new MaybeTask<IImageProvider>(backgroundImage), backgroundImageSize, renderSizeHint);
        }

        /// <summary>
        /// Resolve a MaybeTask&lt;IImageProvider&gt; that represents the whole layer stack.
        /// </summary>
        /// <param name="backgroundImage">A task resulting in the image to use for the background layer.</param>
        /// <param name="backgroundImageSize">Optionally, the size of the background image. If omitted, the natural size of the source will be used, but the size will not available in the LayerLinkingContext for lazy resolvers.</param>
        /// <param name="renderSizeHint">Optionally, the size of image that the layer stack will be rendered to. Note that this is just a hint, the resulting <see cref="IImageProvider" /> should also be rendered at this size by the user.</param>
        /// <returns>An IImageProvider that represents the whole layer stack.</returns>
        public MaybeTask<IImageProvider> ToImageProvider(Task<IImageProvider> backgroundImage = null, Size backgroundImageSize = default(Size), Size renderSizeHint = default(Size))
        {
            return ToImageProvider(new MaybeTask<IImageProvider>(backgroundImage), backgroundImageSize, renderSizeHint);
        }

        private Layer GetActiveBackgroundLayer(MaybeTask<IImageProvider> backgroundImage, Size backgroundImageSize)
        {
            var backgroundLayer = this[0];

            // Replace BG layer?
            if (!backgroundImage.IsEmpty)
            {
                if (backgroundLayer != EmptyLayer)
                {
                    throw new ArgumentException("LayerList was constructed with a background image, do not pass one into ToImageProvider.", "backgroundImage");
                }

                backgroundLayer = new Layer(LayerStyle.Normal(), backgroundImage, backgroundImageSize);
            }

            return backgroundLayer;
        }

        /// <summary>
        /// Resolve a MaybeTask&lt;IImageProvider&gt; that represents the whole layer stack.
        /// </summary>
        /// <param name="renderSizeHint">Optionally, the size of image that the layer stack will be rendered to. Note that this is just a hint, the resulting <see cref="IImageProvider" /> should also be rendered at this size by the user.</param>
        /// <returns>An IImageProvider that represents the whole layer stack.</returns>
        public MaybeTask<IImageProvider> ToImageProvider(Size renderSizeHint)
        {
            return ToImageProvider((Task<IImageProvider>)null, default(Size), renderSizeHint);
        }

        /// <summary>
        /// Resolve a MaybeTask&lt;IImageProvider&gt; that represents the whole layer stack.
        /// </summary>
        /// <param name="backgroundImage">A MaybeTask&lt;IImageProvider&gt; resulting in the image to use for the background layer.</param>
        /// <param name="backgroundImageSize">Optionally, the size of the background image. If omitted, the natural size of the source will be used, but the size will not available in the LayerLinkingContext for lazy resolvers.</param>
        /// <param name="renderSizeHint">Optionally, the size of image that the layer stack will be rendered to. Note that this is just a hint, the resulting <see cref="IImageProvider" /> should also be rendered at this size by the user.</param>
        /// <returns>An IImageProvider that represents the whole layer stack.</returns>
        public MaybeTask<IImageProvider> ToImageProvider(MaybeTask<IImageProvider> backgroundImage, Size backgroundImageSize = default(Size), Size renderSizeHint = default(Size))
        {
            var backgroundLayer = GetActiveBackgroundLayer(backgroundImage, backgroundImageSize);

            var layerContextInvariants = new LayerContext.Invariants(backgroundLayer, backgroundImage, renderSizeHint);

            var layerContexts = new LayerContext[Count];
            layerContexts[0] = new LayerContext(layerContextInvariants, null, backgroundLayer, 0);

            backgroundImage = backgroundLayer.GetImageProvider(layerContexts[0]);

            var currentImage = backgroundImage;

            for (int layerIndex = 1; layerIndex < layerContexts.Length; ++layerIndex)
            {
                //Debug.WriteLine("Resolving layer " + layerIndex);

                var context = new LayerContext(layerContextInvariants, layerContexts[layerIndex-1].CurrentLayer, this[layerIndex], layerIndex);

                context.PreviousImage = currentImage;

                currentImage = context.CurrentLayer.GetImageProvider(context);
                Debug.Assert(!currentImage.IsEmpty, "Resolved imageprovider is null.");

                // Bind the previous image as the source of the current one.
                currentImage = LayerSource.Bind(currentImage, context.PreviousImage);

                // If blending is used, wrap the current image in a BlendEffect and bind the source to it as well.
                var blendedCurrentImage = LayerBlend.Apply(context, currentImage);
                if (!blendedCurrentImage.IsEmpty)
                {
                    blendedCurrentImage = LayerSource.Bind(blendedCurrentImage, context.PreviousImage);
                    currentImage = blendedCurrentImage;
                }

                layerContexts[layerIndex] = context;
            }

            //Debug.WriteLine("Resolved final image provider.");

            return currentImage;
        }

        /// <summary>
        /// Dispose the layer list.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clear();
            }
        }

        public void AddRange(params Layer[] layers)
        {
            base.AddRange(layers);
        }
    }
}
