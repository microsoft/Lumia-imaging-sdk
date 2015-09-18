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
using System.Text;
using Lumia.Imaging;
using Lumia.Imaging.Extras;
using Lumia.Imaging.Compositing;
using Lumia.Imaging.EditShowcase.Editors;
using Lumia.Imaging.EditShowcase.ViewModels.Editors;
using Windows.Storage.Streams;
using System.Threading.Tasks;

namespace Lumia.Imaging.EditShowcase.ImageProcessors
{
    public class BlendEffectProcessor : EffectProcessor
    {
		// TODO: Add mask support

		private BufferImageSource m_foregroundSource;		
		private BlendFunction m_blendFunction;
		private BlendEffect m_blendEffect;
		private double m_globalAlpha;
	
		private void SetDefaultParameters()
		{
			CanRenderAtPreviewSize = true;
			m_globalAlpha = 0.5f;
			m_blendFunction = BlendFunction.Normal;
        }

        public BlendEffectProcessor()
        {
			SetDefaultParameters();
			m_blendEffect = new BlendEffect();
            SetupEffectCategory(m_blendEffect);
            AddEditors();
      }

		public BlendEffectProcessor(IImageProvider source)
        {
			SetDefaultParameters();
			m_blendEffect = new BlendEffect();
           // SetupEffectCategory(m_blendEffect);
            m_blendEffect.Source = source;
            AddEditors();

        }

		public double GlobalAlpha
        {
            get
            {
				return m_globalAlpha;
            }
            set
            {
				m_globalAlpha = value;
            }
        }

		public BlendFunction BlendMode
		{
			get
			{
				return m_blendFunction;
			}
			set
			{
				m_blendFunction = value;
			}
		}

        public override string ToString()
        {
            return Name;
        }

        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Windows.Foundation.Size sourceSize, Windows.Foundation.Size renderSize)
        {
	        return new MaybeTask<IImageProvider>(GetEffectInternal2Async(source, sourceSize, renderSize));
        }

		private async Task<IImageProvider> GetEffectInternal2Async(IImageProvider source, Windows.Foundation.Size sourceSize, Windows.Foundation.Size renderSize)
		{
			if (m_foregroundSource == null)
			{
                m_foregroundSource = new BufferImageSource(await PreloadedImages.Man.ConfigureAwait(false));
			}

			m_blendEffect.Source = source;
			m_blendEffect.ForegroundSource = m_foregroundSource;
			m_blendEffect.GlobalAlpha = m_globalAlpha;
			m_blendEffect.BlendFunction = m_blendFunction;

			return m_blendEffect;
		}

        protected override void Dispose(bool disposing)
        {

        }
    }
}
