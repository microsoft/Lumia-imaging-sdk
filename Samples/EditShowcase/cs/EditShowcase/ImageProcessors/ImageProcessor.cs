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
using System.Threading.Tasks;
using Windows.Foundation;
using Lumia.Imaging.EditShowcase.Interfaces;
using Windows.UI.Xaml.Media;
using Lumia.Imaging.EditShowcase.Editors;
using System.Collections.ObjectModel;
using Lumia.Imaging.EditShowcase.ViewModels;
using Lumia.Imaging;
using Lumia.Imaging.Extras;
using Lumia.Imaging.Workers;


namespace Lumia.Imaging.EditShowcase.ImageProcessors
{
    public abstract class ImageProcessor : ViewModelBase, IImageProcessor, IImageProvider2
    {
        private ImageSource m_thumbnailImagSource;
        private ObservableCollection<EditorViewModelBase> m_editors;
        private string m_name;
        public EffectCategoryEnum EffectCategory { get; set; }


        protected ImageProcessor()
        {
        }

        public IImageProvider2 DescribingEffect
        {
            get;
            private set;
        }

        protected void SetupEffectCategory(object effect)
        {
            if (effect == null)
                return;

          //  if (effect == this)
          //      throw new ArgumentException("Can't use the image processor itself as the DescribingEffect.");

            DescribingEffect = effect as IImageProvider2;

            var namspaceParts = effect.GetType().FullName.Split('.');
            string categoryName = namspaceParts[namspaceParts.Length - 2];
            foreach (var value in Enum.GetValues(typeof(EffectCategoryEnum)))
            {
                if (Enum.GetName(typeof(EffectCategoryEnum), value) == categoryName)
                {
                    EffectCategory = (EffectCategoryEnum)value;
                    break;
                }
            }
        }

        public ObservableCollection<EditorViewModelBase> Editors
        {
            get { return m_editors ?? (m_editors = new ObservableCollection<EditorViewModelBase>()); }
        }

        public bool CanRenderAtPreviewSize { get; protected set; }

        public MaybeTask<IImageProvider> GetEffectAsync(IImageProvider source, Size sourceSize, Size renderSize)
        {
            if (source == null)
            {
                throw new NullReferenceException("source");
            }

            return GetEffectInternalAsync(source, sourceSize, renderSize);
        }

        protected abstract MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Size sourceSize, Size renderSize);

        protected abstract void Dispose(bool disposing);

        public void Dispose()
        {
            Dispose(true);
        }

        public string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(m_name))
                {
                    m_name = GetNormalizedTypeName();
                }

                return m_name;
            }
            set
            {
                SetProperty(ref m_name, value);
            }
        }

        private string GetNormalizedTypeName()
        {
            var name = GetType().Name;
            //name = name.Replace("Lumia.Imaging.EditShowcase.ImageProcessors.", "");
            //name = name.Replace("Effect", "");
            //name = name.Replace("EffectProcessor", "");
            return name;
        }

        public ImageSource ThumbnailImagSource
        {
            get
            {
                return m_thumbnailImagSource;
            }
            set
            {
                SetProperty(ref m_thumbnailImagSource, value);
            }
        }



        IAsyncOperation<ImageProviderInfo> IImageProvider.GetInfoAsync()
        {
            throw new NotImplementedException();
        }

        bool IImageProvider.Lock(RenderRequest renderRequest)
        {
            return true;
        }

		public virtual void OnBeforeRender()
		{
		}

        IImageProvider2 IImageProvider2.Clone()
        {
            throw new NotImplementedException();
        }

        IImageWorker IImageProvider2.CreateImageWorker(IImageWorkerRequest imageWorkerRequest)
        {
            throw new NotImplementedException();
        }

        RenderOptions IImageProvider2.SupportedRenderOptions
        {
            get { if (DescribingEffect != null) return DescribingEffect.SupportedRenderOptions; else return RenderOptions.None; }
        }

        IAsyncOperation<Bitmap> IImageProvider.GetBitmapAsync(Bitmap bitmap, OutputOption outputOption)
        {
            throw new NotImplementedException();
        }

        IAsyncAction IImageProvider.PreloadAsync()
        {
            throw new NotImplementedException();
        }

        public virtual void RestoreDefaultValues()
        {

        }
    }

    
}
