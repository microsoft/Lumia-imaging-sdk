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
	/// A depth-of-field effect that creates the illusion of a photograph taken using lens tilt by way of letting the user specify a band representing the focus area in the image. 
	/// In high quality mode, the blurring of the background gets progressively stronger outward from this band.
	/// </summary>
	public class LensTiltDepthOfFieldEffect : DepthOfFieldEffect
	{
		private readonly ChangeTracker<FocusBand> m_focusBand = new ChangeTracker<FocusBand>();
		private KernelGenerator m_edge1KernelGenerator = new KernelGenerator();
		private KernelGenerator m_edge2KernelGenerator = new KernelGenerator();

		/// <summary>
		/// Creates and initializes a new lens tilt depth-of-field effect.
		/// </summary>
		/// <param name="source">The source image to be blurred.</param>
		/// <param name="focusBand">The band that represents the focus area in the image. Pixels within this band won't be blurred. 
		/// Areas outside of the area will be progressively more blurred as the distance from the focus band increases..</param>
		/// <param name="blurStrengthAtEdge1">Strength of the blur on the Edge1 side of the focus band.</param>
		/// <param name="blurStrengthAtEdge2">Strength of the blur on the Edge2 side of the focus band.</param>
		public LensTiltDepthOfFieldEffect(IImageProvider source, FocusBand focusBand, double strengthAtEdge1, double strengthAtEdge2, DepthOfFieldQuality quality)
			: base(source, quality)
		{
			FocusBand = focusBand;
			StrengthAtEdge1 = strengthAtEdge1;
			StrengthAtEdge2 = strengthAtEdge2;
		}
		public FocusBand FocusBand
		{
			get
			{
				return m_focusBand.Value;
			}

			set
			{
				m_focusBand.Value = value;
			}
		}

		public double StrengthAtEdge1
		{
			get
			{
				return m_edge1KernelGenerator.Strength;
			}

			set
			{
				m_edge1KernelGenerator.Strength = value;
			}
		}

		public double StrengthAtEdge2
		{
			get
			{
				return m_edge2KernelGenerator.Strength;
			}

			set
			{
				m_edge2KernelGenerator.Strength = value;
			}
		}

		protected override bool TryPrepareLensBlurProperties()
		{
			var kernelCount = Quality == DepthOfFieldQuality.Full ? 5 : 1;
			m_edge1KernelGenerator.KernelCount = kernelCount;
			m_edge2KernelGenerator.KernelCount = kernelCount;

			var sourceSize = GetSourceSize();
			m_edge1KernelGenerator.SourceSize = sourceSize;
			m_edge2KernelGenerator.SourceSize = sourceSize;

			var kernels = LensBlurEffect.Kernels;

			var kernelGeneratorsAreDirty = m_edge1KernelGenerator.IsDirty || m_edge2KernelGenerator.IsDirty;

            var kernelBands = m_edge2KernelGenerator.GetKernelBands(m_edge1KernelGenerator.GetKernelBands());
            bool blurShouldBeApplied = kernelBands.Count > 0;

			if (IsDirty || kernelGeneratorsAreDirty)
			{
				kernels = kernelBands.Select(band => band.Kernel).ToList();

				if (kernels.Count > 0)
				{
					LensBlurEffect.Kernels = kernels;
				    blurShouldBeApplied = true;
				}
			}
            
			if (blurShouldBeApplied && (IsDirty || kernelGeneratorsAreDirty || m_focusBand.IsDirty))
			{
				var gradient = LensTiltFocusGradientGenerator.GenerateGradient(FocusBand, sourceSize, m_edge1KernelGenerator, m_edge2KernelGenerator, false);
				KernelMapSource = new GradientImageSource(GetKernelMapSize(), gradient);

				LensBlurEffect.KernelMapType = LensBlurKernelMapType.Continuous;
				LensBlurEffect.FocusAreaEdgeMirroring = LensBlurFocusAreaEdgeMirroring.Off;
			}

			m_focusBand.Reset();

			return blurShouldBeApplied;
		}

		private Size GetKernelMapSize()
		{
			var sourceSize = GetSourceSize();

			return new Size(sourceSize.Width / 2, sourceSize.Height / 2);
		}

	}
}