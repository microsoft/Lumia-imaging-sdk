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
	/// A depth-of-field effect with a user defined elliptic focus area. In high quality mode, the blurring of the background gets progressively stronger along the radii of the ellipse.
	/// </summary>
	public class EllipticFocusDepthOfFieldEffect : DepthOfFieldEffect
	{

		private readonly ChangeTracker<FocusEllipse> m_focusArea = new ChangeTracker<FocusEllipse>();
		private readonly ChangeTracker<double> m_strength = new ChangeTracker<double>();
		KernelGenerator m_kernelGenerator = new KernelGenerator();

		/// <summary>
		/// Creates and initializes a new elliptic focus depth-of-field effect.
		/// </summary>
		/// <param name="source">The source image to be blurred.</param>
		/// <param name="ellipse">The desired focus area within the image.</param>
		/// <param name="strength">The strength of the blur effect.</param>
		/// <param name="quality">The quality of the effect.</param>
		public EllipticFocusDepthOfFieldEffect(IImageProvider source, FocusEllipse focusArea, double strength, DepthOfFieldQuality quality)
			: base(source, quality)
		{
			FocusArea = focusArea;
			Strength = strength;
		}

		public FocusEllipse FocusArea
		{
			get { return m_focusArea.Value; }
			set
			{
				m_focusArea.Value = value;
			}
		}

		public double Strength
		{
			get { return m_strength.Value; }
			set
			{
				m_strength.Value = value;
			}
		}

		protected override bool TryPrepareLensBlurProperties()
		{
			m_kernelGenerator.KernelCount = (Quality == DepthOfFieldQuality.Preview)
				? 1
				: 5;

			m_kernelGenerator.SourceSize = GetSourceSize();
			m_kernelGenerator.Strength = Strength;

            var kernelGeneratorIsDirty = m_kernelGenerator.IsDirty;
            var kernelBands = m_kernelGenerator.GetKernelBands();
            bool blurShouldBeApplied = kernelBands.Count > 0;

            if (IsDirty || m_focusArea.IsDirty || kernelGeneratorIsDirty)
			{
				if (kernelBands.Count > 0)
				{

					var gradient = EllipticFocusGradientGenerator.GenerateGradient(m_focusArea.Value, m_kernelGenerator);
					KernelMapSource = new GradientImageSource(GetKernelMapSize(), gradient);

					if (IsDirty || m_strength.IsDirty)
					{

						LensBlurEffect.Kernels = kernelBands.Select(band => band.Kernel).ToList();

						LensBlurEffect.BlendKernelWidth = LensBlurEffect.Kernels.Max(w => w.Size) / 2;
						LensBlurEffect.KernelMapType = LensBlurKernelMapType.Continuous;
						LensBlurEffect.FocusAreaEdgeMirroring = LensBlurFocusAreaEdgeMirroring.Off;
					}
				}
			}

			m_strength.Reset();
			m_focusArea.Reset();

			return blurShouldBeApplied;
		}

		private Size GetKernelMapSize()
		{
			var sourceSize = GetSourceSize();
			return new Size(sourceSize.Width / 2, sourceSize.Height / 2);
		}
	}
}