using Lumia.Imaging.EditShowcase.Editors;
using Lumia.Imaging.EditShowcase.ImageProcessors;
using Lumia.Imaging;
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
using System.Threading.Tasks;
using Windows.Foundation;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Extras;
using Lumia.Imaging.Transforms;
using Windows.UI;

namespace Lumia.Imaging.EditShowcase.ImageProcessors
{
    public class LinearGradientImageSourceEffectProcessor : EffectProcessor, IPropertyDescriptions
    {
        private GradientImageSource m_gradientImageSource;
        private LinearGradient m_linearGradient;
        private Dictionary<string, PropertyDescription> m_propertyDescriptions;
        public LinearGradientImageSourceEffectProcessor()
        {
            Name = "Linear Gradient";

            m_endX = 1.0;
            m_endY = 1.0;

            m_linearGradient = new LinearGradient(new Point(0.0, 0.0), new Point(1.0, 1.0));
            m_linearGradient.Stops = new[]
                         {
                             new GradientStop{Offset=0.0, Color=Colors.Red},
                             new GradientStop{Offset=0.5, Color=Colors.Green},
                             new GradientStop{Offset=1.0, Color=Colors.Cyan}
                         };

            m_gradientImageSource = new GradientImageSource(new Size(800,500), m_linearGradient);

            SetupEffectCategory(m_gradientImageSource);

            m_propertyDescriptions = new Dictionary<string, PropertyDescription>();
            m_propertyDescriptions.Add("StartX", new PropertyDescription(-1.0, 2.0, 0));
            m_propertyDescriptions.Add("StartY", new PropertyDescription(-1.0, 2.0, 0));
            m_propertyDescriptions.Add("EndX", new PropertyDescription(-1.0, 2.0, 0));
            m_propertyDescriptions.Add("EndY", new PropertyDescription(-1.0, 2.0, 0));

            AddEditors();   

             
        }

        protected override void Dispose(bool disposing)
        {
        }

        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Size sourceSize, Size renderSize)
        {
            m_linearGradient.StartPoint = new Point(m_startX, m_startY);
            m_linearGradient.EndPoint = new Point(m_endX, m_endY);

            return new MaybeTask<IImageProvider>(m_gradientImageSource);
        }

        private double m_startX, m_startY, m_endX, m_endY;

        public double StartX
        {
            get { return m_startX; }
            set { m_startX = value; }
        }

        public double StartY
        {
            get { return m_startY; }
            set { m_startY = value; }
        }

        public double EndX
        {
            get { return m_endX; }
            set { m_endX = value; }
        }

        public double EndY
        {
            get { return m_endY; }
            set { m_endY = value; }
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

