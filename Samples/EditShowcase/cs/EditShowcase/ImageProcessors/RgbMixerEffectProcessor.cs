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
using Lumia.Imaging.Extras;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lumia.Imaging.EditShowcase.ImageProcessors
{
    public class RgbMixerEffectProcessor : EffectProcessor
    {
       private RgbMixerEffect m_RgbMixerEffect;

       public RgbMixerEffectProcessor()
        {
            Name = "RgbMixerEffect";            

            m_RgbMixerEffect = new RgbMixerEffect();

            m_RgbMixerEffect.Red.Red.SetPoint(0, 0);
            m_RgbMixerEffect.Red.Red.SetPoint(255, 64);

            m_RgbMixerEffect.Red.Blue.SetPoint(0, 0);
            m_RgbMixerEffect.Red.Blue.SetPoint(255, 64);

            m_RgbMixerEffect.Green.Red.SetPoint(0, 0);
            m_RgbMixerEffect.Green.Red.SetPoint(255, 64);

            m_RgbMixerEffect.Green.Green.SetPoint(0, 0);
            m_RgbMixerEffect.Green.Green.SetPoint(255, 64);

            m_RgbMixerEffect.Green.Blue.SetPoint(0, 0);
            m_RgbMixerEffect.Green.Blue.SetPoint(255, 64);

            m_RgbMixerEffect.Blue.Blue.SetPoint(0, 0);
            m_RgbMixerEffect.Blue.Blue.SetPoint(255, 64);

            SetupEffectCategory(m_RgbMixerEffect);
        }

        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Windows.Foundation.Size sourceSize, Windows.Foundation.Size renderSize)
        {
            m_RgbMixerEffect.Source = source;
            

            return new MaybeTask<IImageProvider>(m_RgbMixerEffect);
            
        }

        protected override void Dispose(bool disposing)
        {
            
        }
    }
}
