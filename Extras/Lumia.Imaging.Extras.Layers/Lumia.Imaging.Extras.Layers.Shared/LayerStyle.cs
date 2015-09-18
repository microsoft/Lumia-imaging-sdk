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
using Lumia.Imaging.Compositing;

namespace Lumia.Imaging.Extras.Layers
{
    /// <summary>
    /// Describes how a layer should be blended onto the preceding one.
    /// </summary>
    public struct LayerStyle
    {
        internal readonly Func<LayerContext, MaybeTask<IImageProvider>> MaskResolver;

        /// <summary>
        /// The blend function.
        /// </summary>
        public readonly BlendFunction BlendFunction;

        /// <summary>
        /// The opacity of the layer.
        /// </summary>
        public readonly double Opacity;

        /// <summary>
        /// If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.
        /// </summary>
        public readonly Rect? TargetArea;

        /// <summary>
        /// Construct a layer style.
        /// </summary>
        /// <param name="blendFunction">Blend function to use when combining the result of this layer with the previous.</param>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="maskImageProviderResolver">A lazy resolve func that returns the <see cref="IImageProvider" /> based on context sensitive values such as source image size.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public LayerStyle(BlendFunction blendFunction, double opacity, Func<LayerContext, IImageProvider> maskImageProviderResolver, Rect? targetArea = null)
        {
            BlendFunction = blendFunction;
            Opacity = opacity;
            TargetArea = targetArea;

            if (maskImageProviderResolver != null)
            {
                MaskResolver = context => new MaybeTask<IImageProvider>(maskImageProviderResolver(context));
            }
            else
            {
                MaskResolver = null;
            }
        }

        /// <summary>
        /// Construct a layer style.
        /// </summary>
        /// <param name="blendFunction">Blend function to use when combining the result of this layer with the previous.</param>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="maskImageProviderResolver">A lazy resolve func that returns the <see cref="IImageProvider" /> based on context sensitive values such as source image size.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public LayerStyle(BlendFunction blendFunction, double opacity, Func<LayerContext, Task<IImageProvider>> maskImageProviderResolver, Rect? targetArea = null)
        {
            BlendFunction = blendFunction;
            Opacity = opacity;
            TargetArea = targetArea;

            if (maskImageProviderResolver != null)
            {
                MaskResolver = context => new MaybeTask<IImageProvider>(maskImageProviderResolver(context));
            }
            else
            {
                MaskResolver = null;
            }
        }

        /// <summary>
        /// Construct a layer style.
        /// </summary>
        /// <param name="blendFunction">Blend function to use when combining the result of this layer with the previous.</param>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="maskImageProviderResolver">A lazy resolve func that returns the <see cref="IImageProvider" /> based on context sensitive values such as source image size.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public LayerStyle(BlendFunction blendFunction, double opacity, Func<LayerContext, MaybeTask<IImageProvider>> maskImageProviderResolver, Rect? targetArea = null)
        {
            BlendFunction = blendFunction;
            Opacity = opacity;
            TargetArea = targetArea;
            MaskResolver = maskImageProviderResolver;
        }

        /// <summary>
        /// Construct a layer style.
        /// </summary>
        /// <param name="blendFunction">Blend function to use when combining the result of this layer with the previous.</param>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="maskImage">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public LayerStyle(BlendFunction blendFunction, double opacity, IImageProvider maskImage, Rect? targetArea = null)
        {
            BlendFunction = blendFunction;
            Opacity = opacity;
            TargetArea = targetArea;
            
            if (maskImage != null)
            {
                MaskResolver = context => new MaybeTask<IImageProvider>(maskImage);
            }
            else
            {
                MaskResolver = null;
            }
        }

        /// <summary>
        /// Construct a layer style.
        /// </summary>
        /// <param name="blendFunction">Blend function to use when combining the result of this layer with the previous.</param>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="maskImage">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public LayerStyle(BlendFunction blendFunction, double opacity, Task<IImageProvider> maskImage, Rect? targetArea = null)
        {
            BlendFunction = blendFunction;
            Opacity = opacity;
            TargetArea = targetArea;

            if (maskImage != null)
            {
                MaskResolver = context => new MaybeTask<IImageProvider>(maskImage);
            }
            else
            {
                MaskResolver = null;
            }
        }

        /// <summary>
        /// Construct a layer style.
        /// </summary>
        /// <param name="blendFunction">Blend function to use when combining the result of this layer with the previous.</param>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="maskImage">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public LayerStyle(BlendFunction blendFunction, double opacity, MaybeTask<IImageProvider> maskImage, Rect? targetArea = null)
        {
            BlendFunction = blendFunction;
            Opacity = opacity;
            TargetArea = targetArea;

            if (!maskImage.IsEmpty)
            {
                MaskResolver = context => maskImage;
            }
            else
            {
                MaskResolver = null;
            }
        }

        /// <summary>
        /// Construct a layer style that uses the Add blend function and the specified parameters.
        /// </summary>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="mask">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public static LayerStyle Add(double opacity = 1.0, IImageProvider mask = null, Rect? targetArea = null)
        {
            return new LayerStyle(BlendFunction.Add, opacity, mask, targetArea);
        }

        /// <summary>
        /// Construct a layer style that uses the Color blend function and the specified parameters.
        /// </summary>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="mask">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public static LayerStyle Color(double opacity = 1.0, IImageProvider mask = null, Rect? targetArea = null)
        {
            return new LayerStyle(BlendFunction.Color, opacity, mask, targetArea);
        }

        /// <summary>
        /// Construct a layer style that uses the Colorburn blend function and the specified parameters.
        /// </summary>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="mask">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public static LayerStyle Colorburn(double opacity = 1.0, IImageProvider mask = null, Rect? targetArea = null)
        {
            return new LayerStyle(BlendFunction.Colorburn, opacity, mask, targetArea);
        }

        /// <summary>
        /// Construct a layer style that uses the Colordodge blend function and the specified parameters.
        /// </summary>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="mask">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public static LayerStyle Colordodge(double opacity = 1.0, IImageProvider mask = null, Rect? targetArea = null)
        {
            return new LayerStyle(BlendFunction.Colordodge, opacity, mask, targetArea);
        }

        /// <summary>
        /// Construct a layer style that uses the Darken blend function and the specified parameters.
        /// </summary>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="mask">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public static LayerStyle Darken(double opacity = 1.0, IImageProvider mask = null, Rect? targetArea = null)
        {
            return new LayerStyle(BlendFunction.Darken, opacity, mask, targetArea);
        }

        /// <summary>
        /// Construct a layer style that uses the Difference blend function and the specified parameters.
        /// </summary>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="mask">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public static LayerStyle Difference(double opacity = 1.0, IImageProvider mask = null, Rect? targetArea = null)
        {
            return new LayerStyle(BlendFunction.Difference, opacity, mask, targetArea);
        }

        /// <summary>
        /// Construct a layer style that uses the Exclusion blend function and the specified parameters.
        /// </summary>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="mask">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public static LayerStyle Exclusion(double opacity = 1.0, IImageProvider mask = null, Rect? targetArea = null)
        {
            return new LayerStyle(BlendFunction.Exclusion, opacity, mask, targetArea);
        }

        /// <summary>
        /// Construct a layer style that uses the Hardlight blend function and the specified parameters.
        /// </summary>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="mask">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public static LayerStyle Hardlight(double opacity = 1.0, IImageProvider mask = null, Rect? targetArea = null)
        {
            return new LayerStyle(BlendFunction.Hardlight, opacity, mask, targetArea);
        }

        /// <summary>
        /// Construct a layer style that uses the Hue blend function and the specified parameters.
        /// </summary>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="mask">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public static LayerStyle Hue(double opacity = 1.0, IImageProvider mask = null, Rect? targetArea = null)
        {
            return new LayerStyle(BlendFunction.Hue, opacity, mask, targetArea);
        }

        /// <summary>
        /// Construct a layer style that uses the Lighten blend function and the specified parameters.
        /// </summary>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="mask">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public static LayerStyle Lighten(double opacity = 1.0, IImageProvider mask = null, Rect? targetArea = null)
        {
            return new LayerStyle(BlendFunction.Lighten, opacity, mask, targetArea);
        }

        /// <summary>
        /// Construct a layer style that uses the Lineardodge blend function and the specified parameters.
        /// </summary>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="mask">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public static LayerStyle Lineardodge(double opacity = 1.0, IImageProvider mask = null, Rect? targetArea = null)
        {
            return new LayerStyle(BlendFunction.Lineardodge, opacity, mask, targetArea);
        }

        /// <summary>
        /// Construct a layer style that uses the Linearlight blend function and the specified parameters.
        /// </summary>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="mask">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public static LayerStyle Linearlight(double opacity = 1.0, IImageProvider mask = null, Rect? targetArea = null)
        {
            return new LayerStyle(BlendFunction.Linearlight, opacity, mask, targetArea);
        }

        /// <summary>
        /// Construct a layer style that uses the Multiply blend function and the specified parameters.
        /// </summary>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="mask">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public static LayerStyle Multiply(double opacity = 1.0, IImageProvider mask = null, Rect? targetArea = null)
        {
            return new LayerStyle(BlendFunction.Multiply, opacity, mask, targetArea);
        }

        /// <summary>
        /// Construct a layer style that uses the Normal blend function and the specified parameters.
        /// </summary>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="mask">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public static LayerStyle Normal(double opacity = 1.0, IImageProvider mask = null, Rect? targetArea = null)
        {
            return new LayerStyle(BlendFunction.Normal, opacity, mask, targetArea);
        }

        /// <summary>
        /// Construct a layer style that uses the Overlay blend function and the specified parameters.
        /// </summary>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="mask">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public static LayerStyle Overlay(double opacity = 1.0, IImageProvider mask = null, Rect? targetArea = null)
        {
            return new LayerStyle(BlendFunction.Overlay, opacity, mask, targetArea);
        }

        /// <summary>
        /// Construct a layer style that uses the Screen blend function and the specified parameters.
        /// </summary>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="mask">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public static LayerStyle Screen(double opacity = 1.0, IImageProvider mask = null, Rect? targetArea = null)
        {
            return new LayerStyle(BlendFunction.Screen, opacity, mask, targetArea); 
        }

        /// <summary>
        /// Construct a layer style that uses the Softlight blend function and the specified parameters.
        /// </summary>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="mask">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public static LayerStyle Softlight(double opacity = 1.0, IImageProvider mask = null, Rect? targetArea = null)
        {
            return new LayerStyle(BlendFunction.Softlight, opacity, mask, targetArea);
        }

        /// <summary>
        /// Construct a layer style that uses the Vividlight blend function and the specified parameters.
        /// </summary>
        /// <param name="opacity">The opacity of the layer.</param>
        /// <param name="mask">If non-null, specifies a mask image that controls how this layer is blended onto the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <param name="targetArea">If non-empty, positions and scales the layer within the bounds of the preceding one. See <see cref="Lumia.Imaging.Compositing.BlendEffect"/>.</param>
        /// <returns>A layer style.</returns>
        public static LayerStyle Vividlight(double opacity = 1.0, IImageProvider mask = null, Rect? targetArea = null)
        {
            return new LayerStyle(BlendFunction.Vividlight, opacity, mask, targetArea);
        }
    }
}
