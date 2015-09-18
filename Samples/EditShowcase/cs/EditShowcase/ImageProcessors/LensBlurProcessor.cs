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
using System.Runtime.InteropServices.WindowsRuntime;

using System.Threading.Tasks;
using Lumia.Imaging.EditShowcase.Editors;
using Lumia.Imaging.EditShowcase.ViewModels.Editors;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Extras;

namespace Lumia.Imaging.EditShowcase.ImageProcessors
{
    class LensBlurProcessor : EffectProcessor, IPropertyDescriptions
    {
        private IImageProvider[] m_KernelMaps;
        private LensBlurEffect m_lensBlurEffect;
        private Dictionary<string, PropertyDescription> m_propertyDescriptions;

        public LensBlurProcessor()          
        {
            CanRenderAtPreviewSize = false;
            Name = "LensBlurEffect";
            List<IImageProvider> list = new List<IImageProvider>();

            byte[] map = new byte[] { 255, 255, 255, 255, 0, 255, 255, 255, 255 };

            Bitmap bitmap = new Bitmap(new Size(3, 3), ColorMode.Gray8, 3, map.AsBuffer());
            BitmapImageSource mapSource = new BitmapImageSource(bitmap);
            list.Add(mapSource);

            map = new byte[] { 255, 0 };
            bitmap = new Bitmap(new Size(2, 1), ColorMode.Gray8, 2, map.AsBuffer());
            mapSource = new BitmapImageSource(bitmap);
            list.Add(mapSource);

            map = new byte[] { 0, 255 };
            bitmap = new Bitmap(new Size(2, 1), ColorMode.Gray8, 2, map.AsBuffer());
            mapSource = new BitmapImageSource(bitmap);
            list.Add(mapSource);

            map = new byte[] { 255, 0 };
            bitmap = new Bitmap(new Size(1, 2), ColorMode.Gray8, 1, map.AsBuffer());
            mapSource = new BitmapImageSource(bitmap);
            list.Add(mapSource);

            map = new byte[] { 0, 255 };
            bitmap = new Bitmap(new Size(1, 2), ColorMode.Gray8, 1, map.AsBuffer());
            mapSource = new BitmapImageSource(bitmap);
            list.Add(mapSource);

            map = new byte[] { 0 };
            bitmap = new Bitmap(new Size(1, 1), ColorMode.Gray8, 1, map.AsBuffer());
            mapSource = new BitmapImageSource(bitmap);
            list.Add(mapSource);

            m_KernelMaps = list.ToArray();

            m_lensBlurEffect = new LensBlurEffect();
            SetupEffectCategory(m_lensBlurEffect);
            m_lensBlurEffect.KernelMap = m_KernelMaps[0];

            m_propertyDescriptions = new Dictionary<string, PropertyDescription>();
            m_propertyDescriptions.Add("BlendKernelWidth", new PropertyDescription(0, 255, 5));
            m_propertyDescriptions.Add("PointLightStrength", new PropertyDescription(1, 10, 7));
            m_propertyDescriptions.Add("Quality", new PropertyDescription(0, 1.0, 1.0));

            AddEditors();
   
        }

        public uint BlendKernelWidth
        {
            get
            {
                return m_lensBlurEffect.BlendKernelWidth;
            }
            set
            {
                m_lensBlurEffect.BlendKernelWidth = value;
            }
        }
        public LensBlurFocusAreaEdgeMirroring FocusAreaEdgeMirroring
        {
            get
            {
                return m_lensBlurEffect.FocusAreaEdgeMirroring;
            }
            set
            {
                m_lensBlurEffect.FocusAreaEdgeMirroring = value;
            }
        }
        public LensBlurKernelMapType KernelMapType
        {
            get
            {
                return m_lensBlurEffect.KernelMapType;
            }
            set
            {
                m_lensBlurEffect.KernelMapType = value;
            }
        }
        public uint PointLightStrength
        {
            get
            {
                return m_lensBlurEffect.PointLightStrength;
            }
            set
            {
                m_lensBlurEffect.PointLightStrength = value;
            }
        }
        public double Quality
        {
            get
            {
                return m_lensBlurEffect.Quality;
            }
            set
            {
                m_lensBlurEffect.Quality = value;
            }
        }

        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Size sourceSize, Size renderSize)
        {           
            if (m_lensBlurEffect.Source != source)
            {
                m_lensBlurEffect.Source = source;
            }            

            return new MaybeTask<IImageProvider>(m_lensBlurEffect);
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
