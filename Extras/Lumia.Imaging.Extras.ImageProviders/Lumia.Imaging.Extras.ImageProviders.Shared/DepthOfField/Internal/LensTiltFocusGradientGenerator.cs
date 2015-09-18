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
using System.Linq;
using Windows.Foundation;
using Windows.UI;

namespace Lumia.Imaging.Extras.Effects.DepthOfField.Internal
{
	/// <summary>
	/// LensTiltFocusGradientGenerator provides a gradient for LensBlur with a focus band area.
	/// </summary>
	public class LensTiltFocusGradientGenerator : FocusGradientGenerator
	{

		private static readonly double FocusToBlurTransitionWidth = 0.25;

		/// <summary>
		/// Provides a configured LinearGradient that can be used in combination with a GradientImageSource to provide a mask for the LensBlurEffect.
		/// Generates a map for BlendEffect.
		/// </summary>
		/// <param name="band">The band that represents the focus area in the image. Pixels within this band won't be blurred. 
		/// Areas outside of the area will be progressively more blurred as the distance from the focus band increases.</param>
		/// <param name="kernelSpanFromEdge1">Strength at Edge1.</param>
		/// <param name="kernelSpanFromEdge2">Strength at Edge2.</param>
		/// <returns>A LinearGradient that can be used in combination with a GradientImageSource to be used by LensBlur.</returns>

		public static LinearGradient GenerateGradient(FocusBand band, Size sourceSize, KernelGenerator edge1KernelGenerator, KernelGenerator edge2KernelGenerator, bool applySmallBlurFocusArea)
		{
			var gradient = new LinearGradient();

			var lineFunction = GradientLine.CreateFunction(band);

			SetGradientPoints(gradient, lineFunction);
            SetGradientStops(gradient, band, sourceSize, edge1KernelGenerator, edge2KernelGenerator, applySmallBlurFocusArea);

			return gradient;
		}


		private static void SetGradientPoints(LinearGradient gradient, Func<double, Point> pointFunction)
		{
			gradient.StartPoint = pointFunction(0);
			gradient.EndPoint = pointFunction(1);
		}

		private static void SetGradientStops(LinearGradient gradient, FocusBand focusBand, Size sourceSize, KernelGenerator edge1KernelGenerator, KernelGenerator edge2KernelGenerator, bool applySmallBlurFocusArea)
		{
			var blurArea1FirstOffset = GetOffset(gradient, focusBand.Edge1);
			var blurArea2FirstOffset = GetOffset(gradient, focusBand.Edge2);

			var focusBandEdge1Pixels = new Point(focusBand.Edge1.X * sourceSize.Width, focusBand.Edge1.Y * sourceSize.Height);
			var focusBandEdge2Pixels = new Point(focusBand.Edge2.X * sourceSize.Width, focusBand.Edge2.Y * sourceSize.Height);

			var focusBandWidth = Math.Abs(blurArea1FirstOffset - blurArea2FirstOffset);
            var focusBandWidthPixels = Distance(focusBandEdge1Pixels, focusBandEdge2Pixels);

            double blurAreaWidth;
            if (focusBandWidthPixels > 0)
            {
                var scaleFactor = focusBandWidth / focusBandWidthPixels;

                var blurAreaWidthPixels = (sourceSize.Height - focusBandWidthPixels) / 2 * 0.9;

                if (blurAreaWidthPixels < 1.0)
                {
                    gradient.Stops = new[] { new GradientStop() { Offset = 0.5, Color = Color.FromArgb(255, 0, 0, 0) } };
                    return;
                }

                blurAreaWidth = blurAreaWidthPixels * scaleFactor;
            }
            else
            {
                blurAreaWidth = 0.5;
                applySmallBlurFocusArea = true;
            }

			double blurArea1LastOffset;
			double blurArea2LastOffset;

			if (blurArea1FirstOffset > blurArea2FirstOffset)
			{
				blurArea1LastOffset = blurArea1FirstOffset + blurAreaWidth;
				blurArea2LastOffset = blurArea2FirstOffset - blurAreaWidth;
			}
			else
			{
				blurArea1LastOffset = blurArea1FirstOffset - blurAreaWidth;
				blurArea2LastOffset = blurArea2FirstOffset + blurAreaWidth;
			}

			var stops1 = GetGradientStops(gradient, applySmallBlurFocusArea, edge1KernelGenerator, blurArea1FirstOffset, blurArea1LastOffset);
			var stops2 = GetGradientStops(gradient, applySmallBlurFocusArea, edge2KernelGenerator, blurArea2FirstOffset, blurArea2LastOffset, (byte)(edge1KernelGenerator.GetKernelBands().Count));

			List<GradientStop> stops = new List<GradientStop>();
			stops.AddRange(stops1);
			stops.AddRange(stops2);

			var validStops = EnsureMinDiffBetweenPoints(stops);

			gradient.Stops = validStops.ToArray();
		}

		private static List<GradientStop> GetGradientStops(LinearGradient gradient, bool applySmallBlurFocusArea, KernelGenerator kernelGenerator, double firstStopOffset, double lastStopOffset, byte firstKernelIndex = 0)
		{
			var stops = new List<GradientStop>();

			if (kernelGenerator.GetKernelBands().Count > 0)
			{
				double sumOfBandWidths = kernelGenerator.GetKernelBands().Select(band => band.Width).Sum();

				var focusAreaColor = GetFocusAreaColor(applySmallBlurFocusArea);
				stops.Add(new GradientStop() { Color = Color.FromArgb(255, focusAreaColor, focusAreaColor, focusAreaColor), Offset = firstStopOffset });

				var currentStopOffset = lastStopOffset - firstStopOffset > 0
					? firstStopOffset + minDiffBetweenStops
					: firstStopOffset - minDiffBetweenStops;

				var kernelBands = kernelGenerator.GetKernelBands();

				byte kernelIndex = firstKernelIndex;
				foreach (var band in kernelBands)
				{
					kernelIndex++;
					stops.Add(new GradientStop() { Color = Color.FromArgb(255, kernelIndex, kernelIndex, kernelIndex), Offset = currentStopOffset });
					currentStopOffset = currentStopOffset + (band.Width / sumOfBandWidths) * (lastStopOffset - firstStopOffset) * FocusToBlurTransitionWidth;
				}
				stops.Add(new GradientStop() { Color = Color.FromArgb(255, kernelIndex, kernelIndex, kernelIndex), Offset = currentStopOffset });

				stops = EnsureMinDiffBetweenPoints(stops);
			}

			return stops;
		}

		private static byte GetFocusAreaColor(bool applySmallBlurFocusArea)
		{
			return (byte)(applySmallBlurFocusArea ? 1 : 0);
		}

		private static double GetOffset(LinearGradient gradient, Point p)
		{

			var distnacePFromP1 = Distance(p, gradient.StartPoint);
			var distnaceP1FromP2 = Distance(gradient.StartPoint, gradient.EndPoint);

			var offset = distnacePFromP1 / distnaceP1FromP2;
			return offset;
		}

		private static double Distance(Point p1, Point p2)
		{
			return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
		}


	}
}
