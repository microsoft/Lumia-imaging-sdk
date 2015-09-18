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

namespace Lumia.Imaging.EditShowcase.ImageProcessors
{

    public class SpotlightEffectProcessor : EffectProcessor, IPropertyDescriptions
    {
        private Dictionary<string, PropertyDescription> m_propertyDescriptions;
        public SpotlightEffectProcessor()
        {

            CanRenderAtPreviewSize = false;
            Name = "Spotlight";
            Effect = new SpotlightEffect() { Radius = 250, TransitionSize = 0.8 };
      
            m_propertyDescriptions = new Dictionary<string, PropertyDescription>();
            m_propertyDescriptions.Add("Radius", new PropertyDescription(0, 1000, 0));
            m_propertyDescriptions.Add("TransitionSize", new PropertyDescription(0, 1.0, 0));          

            AddEditors();
        }

        protected override void AddRangeEditors()
        {
            
            foreach (string propertyName in PropertyDescriptions.Keys)
            {
                PropertyDescription propertyDescription;
                PropertyDescriptions.TryGetValue(propertyName, out propertyDescription);

                Editors.Add(new RangeEditorViewModel(propertyName, propertyDescription, this));
            }
        }

        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Size sourceSize, Size renderSize)
        {
            ((IImageConsumer)Effect).Source = source;
            ((SpotlightEffect)Effect).Position = new Windows.Foundation.Point(sourceSize.Width / 2, sourceSize.Height / 2);

            return new MaybeTask<IImageProvider>(Effect);
        }

        protected override void Dispose(bool disposing)
        {

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
