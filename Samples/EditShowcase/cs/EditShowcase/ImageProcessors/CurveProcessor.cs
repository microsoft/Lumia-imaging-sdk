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
using Lumia.Imaging.EditShowcase.Editors;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;

namespace Lumia.Imaging.EditShowcase.ImageProcessors
{
    public class CurveProcessor : EffectProcessor, IPropertyDescriptions
    {
        private Curve[] m_RedCurves;
        private Curve[] m_GreenCurves;
        private Curve[] m_BlueCurves;
        private CurvesEffect m_CurvesEffect;
        private Dictionary<string, PropertyDescription> m_propertyDescriptions;

        public int RedCurveIndex { get; set; }
        public int GreenCurveIndex { get; set; }
        public int BlueCurveIndex { get; set; }

        public CurveProcessor()            
        {
            Name = "Curves Effect";
            List<Curve> curves = new List<Curve>();
            m_CurvesEffect = new CurvesEffect();

            curves.Add(new Curve());
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 0), new Point(220, 255) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 0), new Point(170, 220), new Point(220, 255) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(0, 255), new Point(255, 0) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 50), new Point(170, 220), new Point(220, 190) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(10, 10), new Point(220, 255) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 0), new Point(255, 255) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 0), new Point(80, 20), new Point(220, 255) }));

            m_GreenCurves = curves.ToArray();           

            curves = new List<Curve>();

            curves.Add(new Curve());
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(0, 0), new Point(255, 510) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 0), new Point(170, 220), new Point(220, 255) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(0, 255), new Point(255, 0) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 50), new Point(170, 220), new Point(220, 190) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(10, 10), new Point(220, 255) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 0), new Point(255, 255) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 0), new Point(80, 20), new Point(220, 255) }));

            m_RedCurves = curves.ToArray();

            curves = new List<Curve>();
            curves.Add(new Curve());
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(0, 0), new Point(255, 510) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(0, 0), new Point(64, 64), new Point(255, 255) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(0, 25), new Point(255, 200) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 100), new Point(170, 220), new Point(220, 190) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(10, 150), new Point(220, 255) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 200), new Point(255, 255) }));
            curves.Add(new Curve(CurveInterpolation.NaturalCubicSpline, new[] { new Point(30, 40), new Point(80, 20), new Point(220, 255) }));

            m_BlueCurves = curves.ToArray();

            m_propertyDescriptions = new Dictionary<string, PropertyDescription>();
            m_propertyDescriptions.Add("RedCurveIndex", new PropertyDescription(0, m_RedCurves.Length - 1, 0));
            m_propertyDescriptions.Add("GreenCurveIndex", new PropertyDescription(0, m_GreenCurves.Length - 1, 0));
            m_propertyDescriptions.Add("BlueCurveIndex", new PropertyDescription(0, m_BlueCurves.Length - 1, 0.5));          

           
            SetupEffectCategory(m_CurvesEffect);

            AddEditors();
        }        

        protected override void Dispose(bool disposing)
        {            
        }

        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Size sourceSize, Size renderSize)
        {
            m_CurvesEffect.Source = source;

            m_CurvesEffect.Red = m_RedCurves[RedCurveIndex];
            m_CurvesEffect.Green = m_GreenCurves[GreenCurveIndex];
            m_CurvesEffect.Blue = m_BlueCurves[BlueCurveIndex];

            return new MaybeTask<IImageProvider>(m_CurvesEffect);
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

