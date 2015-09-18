//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.Storage.Streams;
using Lumia.Imaging.Compositing;

namespace Lumia.Imaging.CustomEffectSample.ViewModels
{
    public class CustomEffectViewModel : ViewModelBase
    {
        private ImageSource m_imageSource;
        private string m_name;
        private double m_currentValue;
        private double m_maxValue;
        private double m_minValue;
        private IImageProvider m_effect;
        private string m_propertyName;
        private WriteableBitmap m_writeableBitmap;
        private WriteableBitmapRenderer m_renderer;
        public CustomEffectViewModel(IImageProvider effect)
        {
            Effect = effect;            
            m_writeableBitmap = new WriteableBitmap(100, 100);
            m_renderer = new WriteableBitmapRenderer(Effect, m_writeableBitmap);
            MinValue = 0;
            MaxValue = 1;
        }

        public CustomEffectViewModel(IImageProvider effect, string propertyName, double minValue, double maxValue)
        {
            m_propertyName = propertyName;
            Effect = effect;
            m_writeableBitmap = new WriteableBitmap(100, 100);
            m_renderer = new WriteableBitmapRenderer(Effect, m_writeableBitmap);
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public async Task UpdateThumbnailAsync()
        {
            await m_renderer.RenderAsync();
            ImageSource = m_writeableBitmap;
            m_writeableBitmap.Invalidate();

        }

        public double MinValue
        {
            get { return m_minValue; }
            set
            {
                SetProperty(ref m_minValue, value);
            }
        }

        public IImageProvider Effect
        {
            get { return m_effect; }
            set
            {
                SetProperty(ref m_effect, value);
            }
        }

        public double MaxValue
        {
            get { return m_maxValue; }
            set
            {
                SetProperty(ref m_maxValue, value);
            }
        }

        private double GetPropertyValue(string propertyName)
        {
            PropertyInfo propertyInfo = m_effect.GetType().GetRuntimeProperties().Where(x => x.Name == propertyName).FirstOrDefault();
            if (propertyInfo == null)
                return 0;

            return (double)propertyInfo.GetValue(m_effect);
        }

        private void SetPropertyValue(string propertyName, object value)
        {
            PropertyInfo propertyInfo = m_effect.GetType().GetRuntimeProperties().Where(x => x.Name == propertyName).FirstOrDefault();
            if (propertyInfo == null)
                return;

            propertyInfo.SetValue(m_effect, value);
        }

        public double CurrentValue
        {
            get { return GetPropertyValue(PropertyName); }
            set
            {
                SetPropertyValue(PropertyName, value);
                SetProperty(ref m_currentValue, value);
            }
        }

        public string Name
        {
            get { return m_name; }
            set
            {
                SetProperty(ref m_name, value);
            }
        }

        public bool IsEditingSupported
        {
            get { return !string.IsNullOrEmpty(m_propertyName); }
        }

        public string PropertyName
        {
            get { return m_propertyName; }
            set
            {
                SetProperty(ref m_propertyName, value);
            }
        }

        public ImageSource ImageSource
        {
            get { return m_imageSource; }
            private set
            {
                m_imageSource = value;
                SetProperty(ref m_imageSource, value);
                OnPropertyChanged(() => ImageSource);
            }
        }

    }
}
