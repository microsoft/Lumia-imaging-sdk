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

using Lumia.Imaging.Adjustments;
using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace Lumia.Imaging.Extras.Effects.DepthOfField.Internal
{
	public class KernelGenerator
	{
		private readonly static uint minPixelCountForMaxKernelSize = 8 * 1024 * 1024;
		private static double FocusToBlurTransitionGradient = 1;
		private static readonly int smallKernelSizeBreakPoint = 7;
		private static readonly int maxNumberOfLargeKernels = 5;

		private readonly ChangeTracker<int> m_kernelCount = new ChangeTracker<int>(kernelCount => kernelCount > 0 && kernelCount <= 50);
		private readonly ChangeTracker<Size> m_sourceSize = new ChangeTracker<Size>(size => size.Height > 0 && size.Width > 0);
		private readonly ChangeTracker<double> m_strength = new ChangeTracker<double>(strength => strength >= 0.0 && strength <= 1.0);
		private GroupChangeTracker m_anyProperty;
		private List<KernelBand> m_kernelBands;

		private static readonly double kernelAveragingFactor = 1.0;

		public KernelGenerator()
		{
			m_kernelBands = new List<KernelBand>();
			m_anyProperty = new GroupChangeTracker(m_kernelCount, m_sourceSize, m_strength);
		}

		public int KernelCount
		{
			get
			{
				return m_kernelCount.Value;
			}
			set
			{
				m_kernelCount.Value = value;
			}
		}

		public Size SourceSize
		{
			get
			{
				return m_sourceSize.Value;
			}

			set
			{
				m_sourceSize.Value = value;
			}
		}

		public double Strength
		{
			get
			{
				return m_strength.Value;
			}

			set
			{
				m_strength.Value = value;
			}
		}

		public bool IsDirty
		{
			get
			{
				return m_anyProperty.IsDirty;
			}
		}

		public List<KernelBand> GetKernelBands(List<KernelBand> kernelBands = null)
		{
			if (m_anyProperty.IsDirty)
			{
				m_kernelBands.Clear();
				CreateKernelBands(m_kernelBands);

				if (m_kernelBands.Count > 0)
				{
					TransformKernelBands();
					MergeKernelBands();
				}
			}

			m_anyProperty.Reset();

			kernelBands = kernelBands ?? new List<KernelBand>();
			kernelBands.AddRange(m_kernelBands);

			return kernelBands;
		}

		protected virtual void CreateKernelBands(List<KernelBand> kernelBands)
		{

			int maxKernelSize = (int)(GetMaxKernelSize() * kernelAveragingFactor);

			if (maxKernelSize == 0)
			{
				return;
			}

			FocusToBlurTransitionGradient = 0.7 / (1 + Strength * 2);

			for (int i = 1; i <= Math.Min(smallKernelSizeBreakPoint, maxKernelSize); i++)
			{
				var width = i;
				kernelBands.Add(new KernelBand(new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Circle, (uint)i), width));
			}

			if (maxKernelSize > smallKernelSizeBreakPoint)
			{
				var largeKernelSizes = GetLargeKernelSizes(maxKernelSize);
				for (int i = 0; i < largeKernelSizes.Length; i++)
				{
					var actualKernelSize = i >= this.KernelCount - 1 ? (uint)maxKernelSize : largeKernelSizes[i];
					kernelBands.Add(new KernelBand(new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Hexagon, actualKernelSize), (int)largeKernelSizes[i]));
				}
			}
		}

		private void TransformKernelBands()
		{
			foreach (var band in m_kernelBands)
			{
				band.Width = (int)Math.Pow(band.Width, FocusToBlurTransitionGradient);
			}
		}

		private void MergeKernelBands()
		{
			var newBands = new List<KernelBand>();
			var previousBand = m_kernelBands[0];
			for (int i = 1; i < m_kernelBands.Count; i++)
			{
				var band = m_kernelBands[i];
				if (previousBand.Kernel.Size == band.Kernel.Size)
				{
					previousBand.Width += band.Width;
				}
				else
				{
					newBands.Add(previousBand);
					previousBand = band;
				}
			}
			newBands.Add(previousBand);
			m_kernelBands = newBands;
		}

		private int GetMaxKernelSize()
		{
			var pixelCount = SourceSize.Width * SourceSize.Height;
			return (int)(Math.Min(Math.Sqrt(pixelCount / minPixelCountForMaxKernelSize) * 255 * Strength, 255));
		}

		private uint[] GetLargeKernelSizes(int maxKernelSize)
		{
			uint[] sizes;

			var numLargeKernels = Math.Min(maxNumberOfLargeKernels, (maxKernelSize - smallKernelSizeBreakPoint) / 2);

			if (numLargeKernels <= 0)
				return new uint[0];

			var span = maxKernelSize - smallKernelSizeBreakPoint;
			var step = Math.Max((double)span / (numLargeKernels + 1), 2);
			var minSize = smallKernelSizeBreakPoint + step;

			sizes = new uint[numLargeKernels];

			for (int i = 0; i < numLargeKernels; i++)
			{
				sizes[i] = (uint)(minSize + (i * step));
			}
			return sizes;
		}

	}
}
