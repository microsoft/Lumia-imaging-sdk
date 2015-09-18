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

namespace Lumia.Imaging.Extras.Effects.DepthOfField
{
	/// <summary>
	/// A depth-of-field effect.
	/// </summary>
	public abstract class DepthOfFieldEffect : EffectGroupBase
	{
		private readonly ChangeTracker<DepthOfFieldQuality> m_quality = new ChangeTracker<DepthOfFieldQuality>(DepthOfFieldQuality.Preview, true);

		protected DepthOfFieldEffect(IImageProvider source, DepthOfFieldQuality quality)
			: base(source)
		{
			LensBlurEffect = new LensBlurEffect(source);
			Quality = quality;
		}

		protected bool IsDirty
		{
			get { return m_quality.IsDirty; }
		}

        public new IImageProvider Source
        {
            get { return base.Source; }
            set
            {
                if(base.Source != null && value is BitmapImageSource)
                {
                    ((BitmapImageSource)value).Invalidate();
                }
                base.Source = value;
            }
        }

		public DepthOfFieldQuality Quality
		{
			get { return m_quality.Value; }
			set { m_quality.Value = value; }
		}

		protected LensBlurEffect LensBlurEffect { get; set; }

		public IImageProvider KernelMapSource { get; set; }

		private bool m_mustGetNewSize;
		private Size m_sourceSize;

		protected abstract bool TryPrepareLensBlurProperties();

		protected sealed override IImageProvider PrepareGroup(IImageProvider groupSource)
		{
			m_mustGetNewSize = true;

			var blurCanBeApplied = TryPrepareLensBlurProperties();

			if (!blurCanBeApplied)
			{
				return Source;
			}

			LensBlurEffect.Source = groupSource;
			LensBlurEffect.KernelMap = KernelMapSource;
			LensBlurEffect.BlendKernelWidth = LensBlurEffect.Kernels.Max(w => w.Size) / 2;

			switch (Quality)
			{
				case DepthOfFieldQuality.Preview:
					LensBlurEffect.Quality = GetQualityForPreview();
					break;

				case DepthOfFieldQuality.Full:
					LensBlurEffect.Quality = GetQualityForFull();
					break;

				default:
					throw new NotImplementedException(String.Format("Quality {0} currently unsupported", Quality.ToString()));
			}

			m_quality.Reset();

			return LensBlurEffect;
		}

        private static double GetQualityForPreview()
        {
            return 0.5;
        }

        protected virtual double GetQualityForFull()
        {
            return 1.0;
        }

		protected Size GetSourceSize()
		{
			if (m_mustGetNewSize)
			{
				m_sourceSize = GetSize(Source);
				m_mustGetNewSize = false;
			}

			return m_sourceSize;
		}

		private static Size GetSize(IImageProvider imageProvider)
		{
			return imageProvider.GetInfoAsync().AsTask().Result.ImageSize;
		}

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

			if (LensBlurEffect != null)
			{
				LensBlurEffect.Dispose();
				LensBlurEffect = null;
			}

			if (KernelMapSource != null && KernelMapSource is IDisposable)
			{
				(KernelMapSource as IDisposable).Dispose();
				KernelMapSource = null;
			}
		}
    }
}
