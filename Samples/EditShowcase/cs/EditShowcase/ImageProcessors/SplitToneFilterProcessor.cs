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
using Lumia.Imaging.EditShowcase.Editors;
using Lumia.Imaging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Lumia.Imaging.Artistic;
using Lumia.Imaging.Extras;
using Windows.UI;

namespace Lumia.Imaging.EditShowcase.ImageProcessors
{
	public class SplitToneEffectProcessor : EffectProcessor
	{
        private SplitToneEffect m_splitToneEffect;
		public SplitToneEffectProcessor()
		{
			CanRenderAtPreviewSize = true;
			Name = "SplitTone";
			var range1 = new SplitToneRange(0, 120, Color.FromArgb(255, 255, 64, 64));
			var range2 = new SplitToneRange(135, 255, Color.FromArgb(255, 64, 64, 255));
			m_splitToneEffect = new SplitToneEffect(new [] { range1, range2 });
            Effect = m_splitToneEffect;
            SetupEffectCategory(m_splitToneEffect);

			AddRangeEditors(0);
			AddRangeEditors(1);
		}

		private void AddRangeEditors(int index)
		{
            Editors.Add(new RangeEditorViewModelEx<SplitToneEffect>(String.Format("[{0}] Red", index), 0, 255, this, effect => effect.SplitToneRanges[index].Color.R,
                (effect, value) => { var c = effect.SplitToneRanges[index].Color; effect.SplitToneRanges[index].Color = Color.FromArgb(255, (byte)value, c.G, c.B); }));

            Editors.Add(new RangeEditorViewModelEx<SplitToneEffect>(String.Format("[{0}] Green ", index), 0, 255, this, effect => effect.SplitToneRanges[index].Color.G,
                (effect, value) => { var c = effect.SplitToneRanges[index].Color; effect.SplitToneRanges[index].Color = Color.FromArgb(255, c.R, (byte)value, c.B); }));

            Editors.Add(new RangeEditorViewModelEx<SplitToneEffect>(String.Format("[{0}] Blue", index), 0, 255, this, effect => effect.SplitToneRanges[index].Color.B,

                (effect, value) => { var c = effect.SplitToneRanges[index].Color; effect.SplitToneRanges[index].Color = Color.FromArgb(255, c.R, c.G, (byte)value); }));

            Editors.Add(new RangeEditorViewModelEx<SplitToneEffect>(String.Format("[{0}] Luminance Low", index), 0, 255, this, effect => effect.SplitToneRanges[index].LuminanceLow,
                (effect, value) => effect.SplitToneRanges[index].LuminanceLow = Math.Min(effect.SplitToneRanges[index].LuminanceHigh, (byte)value)));

            Editors.Add(new RangeEditorViewModelEx<SplitToneEffect>(String.Format("[{0}] Luminance High", index), 0, 255, this, effect => effect.SplitToneRanges[index].LuminanceHigh,
                (effect, value) => effect.SplitToneRanges[index].LuminanceHigh = Math.Max(effect.SplitToneRanges[index].LuminanceLow, (byte)value)));
        }

        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Size sourceSize, Size renderSize)
        {
            m_splitToneEffect.Source = source;

            return new MaybeTask<IImageProvider>(m_splitToneEffect);
        }


		protected override void Dispose(bool disposing)
		{

		}
	}
}
