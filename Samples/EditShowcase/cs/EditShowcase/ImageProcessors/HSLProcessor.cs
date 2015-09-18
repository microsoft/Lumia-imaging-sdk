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
using Lumia.Imaging.EditShowcase.Editors;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Extras;

namespace Lumia.Imaging.EditShowcase.ImageProcessors
{
    public class HSLProcessor : EffectProcessor, IPropertyDescriptions
    {
        private Curve[] m_HueCurves;
        private Curve[] m_SaturationLightnessCurves;
        private HueSaturationLightnessEffect m_HueSaturationLightnessEffect;
        private Dictionary<string, PropertyDescription> m_propertyDescriptions;

        public int HueCurveIndex { get; set; }
        public int SaturationCurveIndex { get; set; }
        public int LightnessCurveIndex { get; set; }

        public HSLProcessor()            
        {
            List<Curve> curves = new List<Curve>();

			
			Name = "HSL";
			m_HueSaturationLightnessEffect = new HueSaturationLightnessEffect();

            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(0, 0), new Point(255, 0) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 0), new Point(220, 255) }));
			curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 0), new Point(170, 220), new Point(220, 255) }));
			curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(0, 255), new Point(255, 0) }));
			curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 50), new Point(170, 220), new Point(220, 190) }));
			curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(10, 10), new Point(220, 255) }));
			curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 0), new Point(255, 255) }));
			curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 0), new Point(80, 20), new Point(220, 255) }));

			m_SaturationLightnessCurves = curves.ToArray();

			curves = new List<Curve>();

			curves.Add(new Curve());
			curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(0, 0), new Point(255, 510) }));
			curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 0), new Point(170, 220), new Point(220, 255) }));
			curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(0, 255), new Point(255, 0) }));
			curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 50), new Point(170, 220), new Point(220, 190) }));
			curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(10, 10), new Point(220, 255) }));
			curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 0), new Point(255, 255) }));
			curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 0), new Point(80, 20), new Point(220, 255) }));

			m_HueCurves = curves.ToArray();

            m_propertyDescriptions = new Dictionary<string, PropertyDescription>();
            m_propertyDescriptions.Add("HueCurveIndex", new PropertyDescription(0, m_HueCurves.Length - 1, 0));
            m_propertyDescriptions.Add("SaturationCurveIndex", new PropertyDescription(0, m_SaturationLightnessCurves.Length - 1, 0));
            m_propertyDescriptions.Add("LightnessCurveIndex", new PropertyDescription(0, m_SaturationLightnessCurves.Length - 1, 0));

            AddEditors();
           
            SetupEffectCategory(m_HueSaturationLightnessEffect);
        }        

        protected override void Dispose(bool disposing)
        {            
        }

        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Size sourceSize, Size renderSize)
        {
            m_HueSaturationLightnessEffect.Source = source;
			m_HueSaturationLightnessEffect.HueCurve = HueCurveIndex == 0 ? null : m_HueCurves[HueCurveIndex];
			m_HueSaturationLightnessEffect.SaturationCurve = SaturationCurveIndex == 0 ? null : m_SaturationLightnessCurves[SaturationCurveIndex];
			m_HueSaturationLightnessEffect.LightnessCurve = LightnessCurveIndex == 0 ? null : m_SaturationLightnessCurves[LightnessCurveIndex];
            return new MaybeTask<IImageProvider>(m_HueSaturationLightnessEffect);
        }

        public IReadOnlyDictionary<string, PropertyDescription> PropertyDescriptions
        {
            get
            {
                return m_propertyDescriptions;
            }
        }
    }
}
