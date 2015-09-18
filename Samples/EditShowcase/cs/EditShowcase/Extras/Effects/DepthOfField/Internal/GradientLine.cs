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
using Windows.Foundation;

namespace Lumia.Imaging.Extras.Effects.DepthOfField.Internal
{
	public class GradientLine
	{
		private FocusBand m_focusBand;

		public GradientLine(FocusBand band)
		{
			m_focusBand = band;
			IsVertical = (band.Edge1.X - band.Edge2.X) < 1e-3;
			IsHorizontal = (band.Edge1.Y - band.Edge2.Y) < 1e-3;
		}

		public bool IsVertical { get; private set; }
		public bool IsHorizontal { get; private set; }

		public Point PointFromX(double x)
		{
			if (IsVertical)
			{
				throw new InvalidOperationException("PointFromX not valid with a vertical line");
			}

			double slope = (m_focusBand.Edge2.Y - m_focusBand.Edge1.Y) / (m_focusBand.Edge2.X - m_focusBand.Edge1.X);
			double yIntercept = m_focusBand.Edge1.Y - m_focusBand.Edge1.X * slope;

			return new Point(x, slope * x + yIntercept);
		}

		public Point PointFromY(double y)
		{
			if (IsVertical)
			{
				throw new InvalidOperationException("PointFromY not valid with a vertical line");
			}

			double slope = (m_focusBand.Edge2.X - m_focusBand.Edge1.X) / (m_focusBand.Edge2.Y - m_focusBand.Edge1.Y);
			double xIntercept = m_focusBand.Edge1.X - m_focusBand.Edge1.Y * slope;

			return new Point(slope * y + xIntercept, y);
		}

		public static Func<double, Point> CreateFunction(FocusBand band)
		{
			double xDiff = band.Edge1.X - band.Edge2.X;

			Func<double, Point> lineFunction;

			if (Math.Abs(xDiff) < 1e-3)
			{
				//special case to avoid divide by zero
				lineFunction = (x) => new Point(band.Edge1.X, x);
			}
			else
			{
				double slope = (band.Edge2.Y - band.Edge1.Y) / (band.Edge2.X - band.Edge1.X);
				double yIntercept = band.Edge1.Y - band.Edge1.X * slope;

				lineFunction = (x) => new Point(x, slope * x + yIntercept);
			}

			return lineFunction;
		}


	}
}
