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
using System.Linq;
using Windows.Foundation;
using Lumia.Imaging.Extras.Effects.DepthOfField.Internal;
using System.Diagnostics;

namespace Lumia.Imaging.Extras.Effects.DepthOfField
{
	/// <summary>
	/// A depth-of-field effect with an object defined by a mask kept in focus. In high quality mode, the blurring of the background gets progressively stronger outward from a user defined horizon line.
	/// </summary>
	public class FocusObjectDepthOfFieldEffect : DepthOfFieldEffect
	{
		private readonly ChangeTracker<Point> m_horizonPoint1 = new ChangeTracker<Point>();
		private readonly ChangeTracker<Point> m_horizonPoint2 = new ChangeTracker<Point>();
		private readonly ChangeTracker<IImageProvider> m_objectMaskSource = new ChangeTracker<IImageProvider>(null, true);
		private KernelGenerator m_kernelGeneratorEdge1 = new KernelGenerator();
		private KernelGenerator m_kernelGeneratorEdge2 = new KernelGenerator();

		/// <summary>
		/// Creates and initializes a new focus object depth-of-field effect.
		/// </summary>
		/// <param name="source">The image source to apply the effect to</param>
		/// <param name="objectMaskSource">A grayscale mask that represents the object in the image that shall be kept in focus.</param>
		/// <param name="horizonPoint1">The first point defining the horizon line.</param>
		/// <param name="horizonPoint2">The second point defining the horizon line.</param>
		/// <param name="strengthBelowHorizon">The strength of the blur effect below the horizon line.</param>
		/// <param name="strengthAboveHorizon">The strength of the blur effect above the horizon line.</param>
		public FocusObjectDepthOfFieldEffect(IImageProvider source, IImageProvider objectMaskSource, Point horizonPoint1, Point horizonPoint2, double strengthBelowHorizon, double strengthAboveHorizon, DepthOfFieldQuality quality)
			: base(source, quality)
		{
			ObjectMaskSource = objectMaskSource;
			HorizonPoint1 = horizonPoint1;
			HorizonPoint2 = horizonPoint2;
			StrengthBelowHorizon = strengthBelowHorizon;
			StrengthAboveHorizon = strengthAboveHorizon;
		}

		public IImageProvider ObjectMaskSource
		{
			get
			{
				return m_objectMaskSource.Value;
			}

			set
			{
				m_objectMaskSource.Value = value;
			}
		}

		public Point HorizonPoint1
		{
			get
			{
				return m_horizonPoint1.Value;
			}

			set
			{
				m_horizonPoint1.Value = value;
			}
		}

		public Point HorizonPoint2
		{
			get
			{
				return m_horizonPoint2.Value;
			}

			set
			{
				m_horizonPoint2.Value = value;
			}
		}

		public double StrengthBelowHorizon
		{
			get
			{
				return m_kernelGeneratorEdge2.Strength;
			}

			set
			{
				m_kernelGeneratorEdge2.Strength = value;
			}
		}

		public double StrengthAboveHorizon
		{
			get
			{
				return m_kernelGeneratorEdge1.Strength;
			}

			set
			{
				m_kernelGeneratorEdge1.Strength = value;
			}
		}

		protected override bool TryPrepareLensBlurProperties()
		{
			var focusBand = GetBandFromHorizonLine(HorizonPoint1, HorizonPoint2);

			var sourceSize = GetSourceSize();
			m_kernelGeneratorEdge1.SourceSize = sourceSize;
			m_kernelGeneratorEdge2.SourceSize = sourceSize;

			var kernelCount = Quality == DepthOfFieldQuality.Full ? 5 : 1;
			m_kernelGeneratorEdge1.KernelCount = kernelCount;
			m_kernelGeneratorEdge2.KernelCount = kernelCount;

			var kernelGeneratorAbove = m_kernelGeneratorEdge1;
			var kernelGeneratorBelow = m_kernelGeneratorEdge2;

			if (focusBand.Edge1.Y > focusBand.Edge2.Y)
			{
				kernelGeneratorAbove = m_kernelGeneratorEdge2;
				kernelGeneratorBelow = m_kernelGeneratorEdge1;
			}
            
            bool kernelGeneratorsAreDirty = m_kernelGeneratorEdge1.IsDirty || m_kernelGeneratorEdge2.IsDirty;

            var kernelBands = kernelGeneratorBelow.GetKernelBands(kernelGeneratorAbove.GetKernelBands());
            bool blurShouldBeApplied = kernelBands.Count > 0;

            if (IsDirty || kernelGeneratorsAreDirty || m_horizonPoint1.IsDirty || m_horizonPoint2.IsDirty || m_objectMaskSource.IsDirty)
			{
				bool applySmallBlurToFocus = kernelGeneratorAbove.GetKernelBands() != null && kernelGeneratorBelow.GetKernelBands() != null;

				var kernels = kernelBands.Select(band => band.Kernel).ToList();

				if (kernels.Count > 0)
				{

                    var gradient = LensTiltFocusGradientGenerator.GenerateGradient(focusBand, sourceSize, kernelGeneratorBelow, kernelGeneratorAbove, applySmallBlurToFocus);
					KernelMapSource = FocusObjectKernelMapGenerator.Generate(ObjectMaskSource, gradient, GetKernelMapSize(), kernels);

					LensBlurEffect.Kernels = kernels;
					LensBlurEffect.KernelMapType = LensBlurKernelMapType.Continuous;
					LensBlurEffect.FocusAreaEdgeMirroring = LensBlurFocusAreaEdgeMirroring.On;
					LensBlurEffect.FocusEdgeSoftening = new LensBlurFocusEdgeSoftening(LensBlurFocusEdgeSofteningMode.Low);
				}
			}

			m_objectMaskSource.Reset();
			m_horizonPoint1.Reset();
			m_horizonPoint2.Reset();

			return blurShouldBeApplied;
		}

		private Size GetKernelMapSize()
		{
			var sourceSize = GetSourceSize();

			return new Size(sourceSize.Width / 2, sourceSize.Height / 2);
		}

		private FocusBand GetBandFromHorizonLine(Point p1, Point p2)
		{
			double xDiff = p1.X - p2.X;
			double yDiff = p1.Y - p2.Y;

			Point midpoint = new Point((p2.X + p1.X) / 2, (p2.Y + p1.Y) / 2);

			Func<double, Point> lineNormalFunction;

			if (Math.Abs(xDiff) < 1e-3)
			{
				//special case to avoid divide by zero
				lineNormalFunction = (x) => new Point(midpoint.X + x, midpoint.Y);
			}
			else if (Math.Abs(yDiff) < 1e-3)
			{
				//special case to avoid divide by zero
				lineNormalFunction = (y) => new Point(midpoint.X, midpoint.Y + y);
			}
			else
			{
				double slope = (p2.Y - p1.Y) / (p2.X - p1.X);
				double perpSlope = -1 / slope;

				double intercept = midpoint.Y - (perpSlope * midpoint.X);

				lineNormalFunction = (x) => new Point(midpoint.X + x, (perpSlope * (midpoint.X + x)) + intercept);
			}

			FocusBand band = new FocusBand(lineNormalFunction(0), lineNormalFunction(1e-5));

			return band;
		}

        protected override double GetQualityForFull()
        {
            return 1;
        }
	}
}