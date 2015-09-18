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
using Lumia.Imaging;
using Lumia.Imaging.Artistic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using CustomNativeEffects;
using CustomEffects;

namespace Lumia.Imaging.CustomEffectSample.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private CustomEffectViewModel m_currentSelectedEffect;
        public ObservableCollection<CustomEffectViewModel> CustomEffects { get; private set; }
        public DelegateCommand BrowseForImageCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand<FrameworkElement> TargetSwapChainPanelLoadedCommand { get; set; }
        private SwapChainPanel m_targetSwapChainPanel;
        private double m_sourceImageObservedWidth;
        private double m_sourceImageObservedHeight;
        private SwapChainPanelRenderer m_renderer;
        
        public MainViewModel()
        {
            CustomEffects = CreateCustomEffects();
             
            CurrentSelectedEffect = CustomEffects.FirstOrDefault();

            BrowseForImageCommand = new DelegateCommand(OnBrowseForImage);
            SaveCommand = new DelegateCommand(OnSaveImage);

            TargetSwapChainPanelLoadedCommand = new DelegateCommand<FrameworkElement>(OnTargetSwapChainPanelLoaded);

            SourceImageObservedHeight = 800;
            SourceImageObservedWidth = 800;

        }

        private async Task<bool> ApplyEffectAsync(StorageFile file)
        {

            if (CurrentSelectedEffect == null && CurrentSelectedEffect.Effect == null)
            {
                return false;
            }
            // Open a stream for the selected file.
            IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
            string errorMessage = null;

            try
            {
                // Rewind stream to start.                     
                fileStream.Seek(0);

                // A cartoon effect is initialized with selected image stream as source.
                var imageStream = new Lumia.Imaging.RandomAccessStreamImageSource(fileStream);
                UpdateCustomEffectSource(imageStream);
                ((IImageConsumer)CurrentSelectedEffect.Effect).Source = imageStream;
                await RenderAsync();

            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                var dialog = new MessageDialog(errorMessage);
                await dialog.ShowAsync();
                return false;
            }

            return true;
        }

        private async Task LoadDefaultImageAsync()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new System.Uri("ms-appx:///Assets/defaultImage.jpg"));
            await ApplyEffectAsync(file);
        }

        private async void UpdateCustomEffectSource(RandomAccessStreamImageSource imageStream)
        {
            foreach (var customEffectViewModel in CustomEffects)
            {
                ((IImageConsumer2)customEffectViewModel.Effect).Source = imageStream;
                await customEffectViewModel.UpdateThumbnailAsync();
            }

            
        }

        private async void OnTargetSwapChainPanelLoaded(FrameworkElement frameworkElement)
        {
            m_targetSwapChainPanel = (SwapChainPanel)frameworkElement;
            if (m_targetSwapChainPanel.ActualHeight == 0 || m_targetSwapChainPanel.ActualWidth == 0)
            {
                m_targetSwapChainPanel.Width = 800;
                m_targetSwapChainPanel.Height = 800;
            }
            m_renderer = new SwapChainPanelRenderer(CurrentSelectedEffect.Effect, m_targetSwapChainPanel);
            await LoadDefaultImageAsync();

            m_targetSwapChainPanel.SizeChanged += async (sender, args) =>
            {
                if (m_targetSwapChainPanel.ActualHeight > 0 && m_targetSwapChainPanel.ActualWidth > 0)
                {
                    SourceImageObservedWidth = m_targetSwapChainPanel.ActualWidth;
                    SourceImageObservedHeight = m_targetSwapChainPanel.ActualHeight;                    
                    await RenderAsync();
                }
            };

        }

        public double SourceImageObservedWidth
        {
            get { return m_sourceImageObservedWidth; }
            set { SetProperty(ref m_sourceImageObservedWidth, value); }
        }

        public double SourceImageObservedHeight
        {
            get { return m_sourceImageObservedHeight; }
            set { SetProperty(ref m_sourceImageObservedHeight, value); }
        }

        private async void OnBrowseForImage()
        {
            var file = await PickSingleFileAsync();
            if (file == null)
                return;

            await ApplyEffectAsync(file);
            await RenderAsync();
        }

        protected async void OnSaveImage()
        {
            var file = await PickSaveFileAsync();
            if (file == null)
                return;

            await SaveImageAsync(file);
        }

        protected async Task<StorageFile> PickSaveFileAsync()
        {
            FileSavePicker fileSavePicker = new FileSavePicker();
            fileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileSavePicker.DefaultFileExtension = ".jpg";
            fileSavePicker.FileTypeChoices.Add("Image", new List<string>() { ".jpg" });
            fileSavePicker.SuggestedFileName = string.Format("CustomEffectSample{0}", DateTime.Now.ToString("yyyyMMddHHmmss"));

            return await fileSavePicker.PickSaveFileAsync();
        }

        public async Task<bool> SaveImageAsync(StorageFile file)
        {
            if (CurrentSelectedEffect == null && CurrentSelectedEffect.Effect == null)
            {
                return false;
            }

            string errorMessage = null;

            try
            {
                using (var jpegRenderer = new JpegRenderer(CurrentSelectedEffect.Effect))
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    jpegRenderer.Size = new Windows.Foundation.Size(SourceImageObservedWidth, SourceImageObservedHeight);
                    // Jpeg renderer gives the raw buffer containing the filtered image.
                    IBuffer jpegBuffer = await jpegRenderer.RenderAsync();
                    await stream.WriteAsync(jpegBuffer);
                    await stream.FlushAsync();
                }
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                var dialog = new MessageDialog(errorMessage);
                await dialog.ShowAsync();
                return false;
            }
            return true;
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

        private ObservableCollection<CustomEffectViewModel> CreateCustomEffects()
        {
            var effects = new ObservableCollection<CustomEffectViewModel>();

            effects.Add(new CustomEffectViewModel(new CustomNativeEffects.CustomGrayscaleEffect()) { Name = "Grayscale" });
            effects.Add(new CustomEffectViewModel(new CustomEffects.CustomGrayscaleEffect()) { Name = "Grayscale c#" });

            var effectViewModel = new CustomEffectViewModel(new CustomNativeEffects.MagnifySmoothEffect(), "MagnificationAmount", 1, 10.0) { Name = "Magnify Smooth C++" };
            effectViewModel.PropertyChanged += OnEffectViewModelPropertyChanged;
            effects.Add(effectViewModel);

            effectViewModel = new CustomEffectViewModel(new CustomEffects.MagnifySmoothEffect(), "MagnificationAmount", 1, 10.0) { Name = "Magnify Smooth C#" };
            effectViewModel.PropertyChanged += OnEffectViewModelPropertyChanged;
            effects.Add(effectViewModel);

            effectViewModel = new CustomEffectViewModel(new Direct2DSaturationEffect(), "Level", 0, 1.0) { Name = "Saturation" };
            effectViewModel.PropertyChanged += OnEffectViewModelPropertyChanged;
            effects.Add(effectViewModel);
           
            effects.Add(new CustomEffectViewModel(new CustomNativeEffects.SplitToneEffect()) { Name = "SplitToneEffect" });

            return effects;
        }

        private async void OnEffectViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await RenderAsync();
        }

        public CustomEffectViewModel CurrentSelectedEffect
        {
            get { return m_currentSelectedEffect; }
            set
            {
                SetProperty(ref m_currentSelectedEffect, value);
                RenderAsync();
            }
        }

        private async Task RenderAsync()
        {
            if (m_renderer == null)
                return;

            m_renderer.Source = CurrentSelectedEffect.Effect;

            await m_renderer.RenderAsync();
        }
    }
}
