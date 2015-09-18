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
using Lumia.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;


using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml;
using Windows.Foundation;
using Lumia.Imaging.EditShowcase.ViewModels;
using Lumia.Imaging.EditShowcase.Interfaces;
using Lumia.Imaging.EditShowcase.Utilities;
using Windows.Storage.Pickers;
using Lumia.Imaging.EditShowcase.Editors;
using System.ComponentModel;
using Lumia.Imaging.EditShowcase.ImageProcessors;
using Windows.ApplicationModel.Activation;

namespace Lumia.Imaging.EditShowcase.ViewModels
{
    public class ImageViewModelBase : ViewModelBase
    {
        private int m_ImageWidth;
        private int m_ImageHeight;
        private ImageSource m_SourceImage;
        private string m_SourceImageRenderMessage;
        private string m_TargetImageRenderMessage;

        public ImageViewModelBase()            
        {

        }

        public double ImageWidth
        {
            get { return m_ImageWidth; }
			set { SetProperty(ref m_ImageWidth, (int)value); }
        }

        public double ImageHeight
        {
            get { return m_ImageHeight; }
            set { SetProperty(ref m_ImageHeight, (int)value); }
        }

        public string SourceImageRenderMessage
        {
            get { return m_SourceImageRenderMessage; }
            set { SetProperty(ref m_SourceImageRenderMessage, value); }
        }

        public string TargetImageRenderMessage
        {
            get { return m_TargetImageRenderMessage; }
            set { SetProperty(ref m_TargetImageRenderMessage, value); }
        }

        public ImageSource SourceImageSource
        {
            get { return m_SourceImage; }
            set { SetProperty(ref m_SourceImage, value); }
        }
        
        protected async Task<StorageFile> PickSingleFileAsync(IList<string> fileFiters = null)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            if (fileFiters == null)
            {
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".png");
            }
            else
            {
                foreach (string fileExtension in fileFiters)
                {
                    openPicker.FileTypeFilter.Add(fileExtension);
                } 
            }
       

            return await openPicker.PickSingleFileAsync();           
        }


        protected async Task<StorageFile> PickSaveFileAsync()
        {
            FileSavePicker fileSavePicker = new FileSavePicker();
            fileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileSavePicker.DefaultFileExtension = ".jpg";
            fileSavePicker.FileTypeChoices.Add("Image", new List<string>() { ".jpg" });
            fileSavePicker.SuggestedFileName = string.Format("LumiaImagingShowcase_{0}", DateTime.Now.ToString("yyyyMMddHHmmss")); 

            return await fileSavePicker.PickSaveFileAsync();            
        }

    }

}




