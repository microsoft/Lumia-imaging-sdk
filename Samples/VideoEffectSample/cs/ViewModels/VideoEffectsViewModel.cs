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

namespace Lumia.Imaging.VideoEffectSample.ViewModels
{
    public class VideoEffectsViewModel : ViewModelBase
    {
        private ImageSource m_imageSource;        
        private string m_name;
        private double m_currentValue;
        private double m_maxValue;
        private double m_minValue;
        private IImageProvider2 m_effect;
        private string m_propertyName;

        public VideoEffectsViewModel(IImageProvider2 effect, string imageName)
        {
            m_propertyName = "Level";
            Effect = effect;
            LoadImageAsync(imageName);
        }

        public VideoEffectsViewModel(IImageProvider2 effect, string imageName, string forgroundImageName)
        {
            m_propertyName = "Level";
            Effect = effect;
            LoadImageAsync(imageName);
            
            LoadForgroundSource(forgroundImageName);            
        }

    private async void LoadForgroundSource(string forgroundImageName)
    {
        var blendEffect = Effect as BlendEffect;
        if (blendEffect != null)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new System.Uri(string.Format("ms-appx:///Images/{0}", forgroundImageName)));
            var storageFileImageSource = new StorageFileImageSource(file);
            blendEffect.ForegroundSource = storageFileImageSource;
        }
    }

    public async void LoadImageAsync(string imageName)
    {
        string imagePath = string.Format("ms-appx:///Icons/{0}", imageName);
        var uri = new System.Uri(imagePath);
        var file = await StorageFile.GetFileFromApplicationUriAsync(uri);

        BitmapImage bitmapImage = new BitmapImage();
        Windows.Storage.Streams.FileRandomAccessStream stream = (FileRandomAccessStream)await file.OpenAsync(FileAccessMode.Read);

        bitmapImage.SetSource(stream);

        ImageSource = bitmapImage;
    }

    public double MinValue
    {
        get { return m_minValue; }
        set
        {
            SetProperty(ref m_minValue, value);
        }
    }

        public IImageProvider2 Effect
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
        return (double)propertyInfo.GetValue(m_effect);
    }

    private void SetPropertyValue(string propertyName, object value)
    {
        PropertyInfo propertyInfo = m_effect.GetType().GetRuntimeProperties().Where(x => x.Name == propertyName).FirstOrDefault();
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
