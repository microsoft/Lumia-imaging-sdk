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
using Lumia.Imaging.EditShowcase.ImageProcessors;
using Lumia.Imaging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Extras;
using Lumia.Imaging.Transforms;
using Windows.UI;

namespace Lumia.Imaging.EditShowcase.ImageProcessors
{
    public class RadialGradientImageSourceEffectProcessor : EffectProcessor , IPropertyDescriptions
    {
        private GradientImageSource m_gradientImageSource;
        private RadialGradient m_radialGradient;
        private Dictionary<string, PropertyDescription> m_propertyDescriptions;

        public RadialGradientImageSourceEffectProcessor()
        {
            Name = "Radial Gradient";

            m_radialGradient = new RadialGradient(new Point(0.5, 0.5), new EllipseRadius(0.5, 0.5));
            m_radialGradient.Stops = new[]
                         {
                             new GradientStop{Offset=0.0, Color=Colors.Red},
                             new GradientStop{Offset=0.5, Color=Colors.Green},
                             new GradientStop{Offset=1.0, Color=Colors.Cyan}
                         };

            m_gradientImageSource = new GradientImageSource(new Size(800,500), m_radialGradient);

            SetupEffectCategory(m_gradientImageSource);
      
            m_propertyDescriptions = new Dictionary<string, PropertyDescription>();
            m_propertyDescriptions.Add("CenterX", new PropertyDescription(0, 1.0, 0.5));
            m_propertyDescriptions.Add("CenterY", new PropertyDescription(0, 1.0, 0.5));
            m_propertyDescriptions.Add("RadiuxX", new PropertyDescription(0, 1.0, 0.5));
            m_propertyDescriptions.Add("RadiuxY", new PropertyDescription(0, 1.0, 0.5));

            AddEditors();

            m_centerX = 0.5;
            m_centerY = 0.5;          
            m_radiusX = 0.5;
            m_radiusY = 0.5;
         
        }

        protected override void Dispose(bool disposing)
        {
        }

        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Size sourceSize, Size renderSize)
        {
            m_radialGradient.CenterPoint = new Point(m_centerX, m_centerY);
            m_radialGradient.EllipseRadius = new EllipseRadius(m_radiusX, m_radiusY);

            return new MaybeTask<IImageProvider>(m_gradientImageSource);
        }

        private double m_centerX, m_centerY, m_radiusX, m_radiusY;

        public double CenterX
        {
            get { return m_centerX; }
            set { m_centerX = value; }
        }

        public double CenterY
        {
            get { return m_centerY; }
            set { m_centerY = value; }
        }

        public double RadiusX
        {
            get { return m_radiusX; }
            set { m_radiusX = value; }
        }

        public double RadiusY
        {
            get { return m_radiusY; }
            set { m_radiusY = value; }
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

