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
using Windows.UI.Xaml.Media.Imaging;
using Lumia.Imaging;
using Windows.Storage;
using Windows.Foundation;
using System.Threading.Tasks;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Extras;

namespace Lumia.Imaging.EditShowcase.ImageProcessors
{
    class SaturationLightnessProcessor : ImageProcessor
    {
        private readonly SaturationLightnessEffect m_SaturationLightnessEffect;

        public SaturationLightnessProcessor()
            : this("SaturationLightness")
        {
        }

        public SaturationLightnessProcessor(string name)
        {
            CanRenderAtPreviewSize = true;
            Name = name;
            List<Curve> curves = new List<Curve>();
            m_SaturationLightnessEffect = new SaturationLightnessEffect();

            SetupEffectCategory(m_SaturationLightnessEffect);
            
            curves.Add(new Curve());
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 0), new Point(220, 255) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 0), new Point(170, 220), new Point(220, 255) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(0, 255), new Point(255, 0) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 50), new Point(170, 220), new Point(220, 190) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(10, 10), new Point(220, 255) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 0), new Point(255, 255) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 0), new Point(80, 20), new Point(220, 255) }));

            Curve[] curves1 = curves.ToArray();

          m_SaturationLightnessEffect.SaturationCurve = curves1[0];
            m_SaturationLightnessEffect.LightnessCurve = curves1[1];
        }

        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Size sourceSize, Size renderSize)
        {
             m_SaturationLightnessEffect.Source = source;

             return new MaybeTask<IImageProvider>(m_SaturationLightnessEffect);
        }

        protected override void Dispose(bool disposing)
        {
           
        }
    }
}
