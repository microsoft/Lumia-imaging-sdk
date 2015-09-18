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
using Lumia.Imaging.Compositing;
using Lumia.Imaging.Transforms;
using System;


namespace Lumia.Imaging.Extras.Effects
{
    /// <summary>
    /// An effect that approximates a high-pass filter over the image, preserving high frequencies (edges).
    /// </summary>
    public class HighpassEffect : EffectGroupBase
    {
        private BlendEffect m_highPassBlendEffect;
        private GrayscaleEffect m_highPassGrayscaleEffect;
        private ScaleEffect m_downscaleEffect;
        private CachingEffect m_downscaleCachingEffect;
        private BlurEffect m_blurEffect;

        private uint m_kernelSize;
        private readonly uint m_downscaleDivisor;
        private readonly bool m_isGrayscale;

        /// <summary>
        /// Constructs a high pass effect with the specified parameters.
        /// </summary>
        /// <param name="kernelSize">The size of the filter kernel. A larger size preserves more of lower frequencies.</param>
        /// <param name="isGrayscale">True if the highpass effect should give a grayscale result. Otherwise the individual R, G, B channels are treated separately.</param>
        /// <param name="downscaleDivisor">How much to downscale the image to reduce the cost of the internal blur operation, trading speed for some fidelity. Suitable value depends on the kernelSize.</param>
        public HighpassEffect(uint kernelSize, bool isGrayscale = false, uint downscaleDivisor = 1)
        {
            m_kernelSize = kernelSize;
            m_downscaleDivisor = downscaleDivisor;
            m_isGrayscale = isGrayscale;
            
            if (m_downscaleDivisor > 1)
            {
                m_downscaleEffect = new ScaleEffect(/*source*/ 1.0 / m_downscaleDivisor);
                m_downscaleCachingEffect = new CachingEffect(m_downscaleEffect);

                int blurKernelSize = Math.Max(1, (int)(3.0 * m_kernelSize / m_downscaleDivisor));

                m_blurEffect = new BlurEffect(m_downscaleCachingEffect) { KernelSize = blurKernelSize };

                m_highPassBlendEffect = new BlendEffect( /*source*/)
                {
                    ForegroundSource = m_blurEffect,
                    BlendFunction = BlendFunction.SignedDifference
                };
            }
            else
            {
                int blurKernelSize = Math.Max(1, (int)(3.0 * m_kernelSize));

                m_blurEffect = new BlurEffect() { KernelSize = blurKernelSize };

                m_highPassBlendEffect = new BlendEffect( /*source*/)
                {
                    ForegroundSource = m_blurEffect,
                    BlendFunction = BlendFunction.SignedDifference
                };
            }
                 
            if (m_isGrayscale)
            {
                m_highPassGrayscaleEffect = new GrayscaleEffect(m_highPassBlendEffect);
            }
        }

        protected override IImageProvider PrepareGroup(IImageProvider groupSource)
        {
            if (m_downscaleDivisor > 1)
            {
                m_downscaleEffect.Source = groupSource;
            }
            else
            {
                m_blurEffect.Source = groupSource;
            }

            m_highPassBlendEffect.Source = groupSource;

            return m_isGrayscale ? (IImageProvider)m_highPassGrayscaleEffect : (IImageProvider)m_highPassBlendEffect;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "m_downscaleCachingEffect")]
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            DisposableHelper.TryDisposeAndSetToNull(ref m_blurEffect);
            DisposableHelper.TryDisposeAndSetToNull(ref m_downscaleEffect);
            DisposableHelper.TryDisposeAndSetToNull(ref m_downscaleCachingEffect);
            DisposableHelper.TryDisposeAndSetToNull(ref m_highPassBlendEffect);
            DisposableHelper.TryDisposeAndSetToNull(ref m_highPassGrayscaleEffect);
        }

        /// <summary>
        /// True if the highpass effect gives a grayscale result. Otherwise the individual R, G, B channels are treated separately.
        /// </summary>
        public bool IsGrayscale
        {
            get
            {
                return m_isGrayscale;   
            }
        }

        /// <summary>
        /// The size of the filter kernel. A larger kernel size preserves more of lower frequencies.
        /// </summary>
        public uint KernelSize
        {
            get
            {
                return m_kernelSize;
            }
            set
            {
                if (value == m_kernelSize)
                {
                    return;
                }

                m_kernelSize = value;

                m_blurEffect.KernelSize = Math.Max(1, (int)(3.0 * m_kernelSize / m_downscaleDivisor));
            }
        }

        /// <summary>
        /// Controls quality/speed by determinining whether the image should be downscaled internally or not, and by how much. The default value is 1, which means don't downscale.
        /// The user is advised to find the lowest possible value that performs well enough.
        /// </summary>
        public uint DownscaleDivisor
        {
            get
            {
                return m_downscaleDivisor;
            }
        }


    }
}
