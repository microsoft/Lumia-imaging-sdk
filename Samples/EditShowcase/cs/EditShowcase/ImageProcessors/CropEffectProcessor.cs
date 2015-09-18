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

namespace Lumia.Imaging.EditShowcase.ImageProcessors
{
    public class CropEffectProcessor : EffectProcessor, IPropertyDescriptions
    {
        private CropEffect m_cropEffect;
        private Dictionary<string, PropertyDescription> m_propertyDescriptions;

        public CropEffectProcessor()
        {
            Name = "Crop Effect";
            m_cropEffect = new CropEffect();

            m_propertyDescriptions = new Dictionary<string, PropertyDescription>();
            m_propertyDescriptions.Add("Left", new PropertyDescription(0,1.0, 0));
            m_propertyDescriptions.Add("Top", new PropertyDescription(0, 1.0, 0));
            m_propertyDescriptions.Add("Right", new PropertyDescription(0, 1.0, 0.5));
            m_propertyDescriptions.Add("Bottom", new PropertyDescription(0, 1.0, 0.5));

            SetupEffectCategory(m_cropEffect);

            AddEditors();         
                
        }

        protected override void Dispose(bool disposing)
        {
        }

        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Size sourceSize, Size renderSize)
        {
            m_cropEffect.Source = source;

            Size inputSize = ((IImageResource)source).ImageSize;

            var x = (int)(inputSize.Width * Left);
            var y = (int)(inputSize.Height * Top);
            var width = (int)(inputSize.Width- inputSize.Width * Right) - x;
            var height = (int)(inputSize.Height- inputSize.Height * Bottom) - y;
            m_cropEffect.CropArea = new Rect(x, y, width, height);
            
            return new MaybeTask<IImageProvider>(m_cropEffect);
        }

        private double m_left, m_top, m_right, m_bottom;

        public double Left
        {
            get { return m_left; }
            set { m_left = Math.Min(value, (1.0-m_right)-0.01); }
        }

        public double Top
        {
            get { return m_top; }
            set { m_top = Math.Min(value, (1.0-m_bottom) - 0.01); }
        }

        public double Right
        {
            get { return m_right; }
            set { m_right = Math.Min(value, (1.0-m_left) - 0.01); }
        }

        public double Bottom
        {
            get { return m_bottom; }
            set { m_bottom = Math.Min(value, (1.0-m_top) - 0.01); }
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

