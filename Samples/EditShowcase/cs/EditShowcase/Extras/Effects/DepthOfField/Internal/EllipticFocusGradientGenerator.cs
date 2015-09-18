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

using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;

namespace Lumia.Imaging.Extras.Effects.DepthOfField.Internal
{
	/// <summary>
	/// EllipticFocusGradientGenerator provides a mask for LensBlur with an ellipse focus area.
	/// </summary>
	public class EllipticFocusGradientGenerator : FocusGradientGenerator
	{
		private static readonly double FocusToBlurTransitionWidth = 0.25;

		/// <summary>
		/// Provides a configured LinearGradient that can be used in combination with a GradientImageSource to provide a mask for the LensBlurEffect.
		/// It will expect the LensBlur will be blurred with 1 kernel when the double is different from None.
		/// </summary>
		/// <param name="ellipse">The band that represents the focus area in the image. Pixels within this band won't be blurred. 
		/// Areas outside of the area will be progressively more blurred as the distance from the focus band increases.</param>
		/// <param name="strength">Strength of the blur.</param>
		/// <returns>A LinearGradient that can be used in combination with a GradientImageSource to be used by LensBlur.</returns>
		public static RadialGradient GenerateGradient(FocusEllipse ellipse, KernelGenerator kernelGenerator)
		{
			if (kernelGenerator.GetKernelBands().Count == 0)
			{
				return null;
			}

			EllipseRadius maxBlurRadius = null;

            if ((ellipse.Radius.X < minDiffBetweenStops) && (ellipse.Radius.Y < minDiffBetweenStops))
            {
                maxBlurRadius = new EllipseRadius(minDiffBetweenStops, minDiffBetweenStops);
            }
            else
            {
                double factor = 0.9;
                bool widthGtHight = ellipse.Radius.X > ellipse.Radius.Y;
                
                if (widthGtHight)
                {
                    maxBlurRadius = new EllipseRadius(factor, factor * ellipse.Radius.Y / ellipse.Radius.X);
                }
                else
                {
                    maxBlurRadius = new EllipseRadius(factor * ellipse.Radius.X / ellipse.Radius.Y, factor);
                }
            }

			var gradient = new RadialGradient(ellipse.Center, maxBlurRadius);
			SetGradientStops(gradient, ellipse, maxBlurRadius, kernelGenerator);

			return gradient;
		}

		private static void SetGradientStops(RadialGradient gradient, FocusEllipse ellipse, EllipseRadius maxBlurRadius, KernelGenerator kernelGenerator)
		{
			var stops = new List<GradientStop>();

            double firstStopOffset;

            if (maxBlurRadius.X > minDiffBetweenStops)
            {
                firstStopOffset = ellipse.Radius.X / maxBlurRadius.X;
                
                //Add the focus area at the center of the ellipse.
                stops.Add(new GradientStop() { Color = Color.FromArgb(255, 0, 0, 0), Offset = 0 });
                stops.Add(new GradientStop() { Color = Color.FromArgb(255, 0, 0, 0), Offset = firstStopOffset });
            }
            else
            {
                firstStopOffset = minDiffBetweenStops;
            }

			double sumOfBandWidths = kernelGenerator.GetKernelBands().Select(band => band.Width).Sum();
			var currentStopOffset = firstStopOffset + minDiffBetweenStops;

			var kernelBands = kernelGenerator.GetKernelBands();

			byte kernelIndex = 0;
			foreach (var band in kernelBands)
			{
				kernelIndex++;
				stops.Add(new GradientStop() { Color = Color.FromArgb(255, kernelIndex, kernelIndex, kernelIndex), Offset = currentStopOffset });
				currentStopOffset = currentStopOffset + (band.Width / sumOfBandWidths) * (1 - firstStopOffset) * FocusToBlurTransitionWidth;
			}
			stops.Add(new GradientStop() { Color = Color.FromArgb(255, kernelIndex, kernelIndex, kernelIndex), Offset = currentStopOffset });

			var validStops = EnsureMinDiffBetweenPoints(stops);

			gradient.Stops = validStops.ToArray();
		}

	}
}
