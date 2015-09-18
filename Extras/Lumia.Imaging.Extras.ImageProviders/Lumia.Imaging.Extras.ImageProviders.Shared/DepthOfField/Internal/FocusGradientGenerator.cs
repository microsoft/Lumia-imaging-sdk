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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lumia.Imaging.Extras.Effects.DepthOfField.Internal
{
	public abstract class FocusGradientGenerator
	{
		protected const double minDiffBetweenStops = 1e-5;
		protected const double transitionBandFactor = 0.5;

		protected static List<GradientStop> EnsureMinDiffBetweenPoints(List<GradientStop> stops)
		{
			stops = stops.OrderBy(w => w.Offset).ToList();

			var newList = new List<GradientStop>();
			newList.Add(stops[0]);

			var lower = stops[0];
			for (int i = 1; i < stops.Count; i++)
			{
				var higher = stops[i];

				if ((higher.Offset - lower.Offset) < minDiffBetweenStops)
				{
					var replacement = new GradientStop() { Offset = Math.Max(higher.Offset, lower.Offset) + minDiffBetweenStops, Color = higher.Color };

					newList.Add(replacement);
					lower = replacement;
				}
				else
				{
					newList.Add(higher);
					lower = higher;
				}
			}

			return newList;
		}
	}
}
