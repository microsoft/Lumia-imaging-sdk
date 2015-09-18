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
#define RENDER_TO_BITMAP
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.Graphics.Imaging;
using Lumia.Imaging.Extras.Extensions;
using Lumia.Imaging.EditShowcase.Interfaces;
using Lumia.Imaging.EditShowcase.Utilities;
using Lumia.Imaging.EditShowcase.Editors;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Controls;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using Lumia.Imaging;
using Lumia.Imaging.EditShowcase.Views;
using Windows.Graphics.Display;

namespace Lumia.Imaging.EditShowcase.ViewModels
{   
    public class FilterExplorerViewModel : ImageViewModelBase
    {
        private const string SourceImageRenderMessageFormatString = "Original {0}";
        private const string TargetImageRenderMessageFormatString = "Result {0}, setup/render {1}/{2} ms";
        private bool m_IsEditingViewVisible;
        private ObservableCollection<EditorViewModelBase> m_CurrentEditors;
        private IEnumerable<IImageProcessor> m_FilterEffects;
        private IImageProcessor m_SelectedEffect;
        private bool m_CanSelectNewEffect;
        private readonly DispatcherTimer m_slowUpdateTimer;
        private bool m_isTargetImageRenderMessageDirty = true;
        private ImageProcessorRenderer m_renderer;
        private bool m_isRenderDirty;
        private bool m_isRendering;
        private EffectCategoryItem m_selectedFilterCategory;
        private bool m_IsFlyOutClosed;        
        
        public ObservableCollection<EffectCategoryItem> FilterCategories { get; set; }
        public DelegateCommand<FrameworkElement> SourceImageLoadedCommand { get; set; }
        public DelegateCommand<FrameworkElement> TargetSwapChainPanelLoadedCommand { get; set; }
        public DelegateCommand BrowseForImageCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand EditPhotoCommand { get; private set; }

        public DelegateCommand RestoreDefaultCommand { get; set; }

        private SwapChainPanel m_targetSwapChainPanel;
        private string m_effectDescription;

        private double m_sourceImageObservedWidth;
        private double m_sourceImageObservedHeight;

        public FilterExplorerViewModel()
        {
            CanSelectNewEffect = true;
            SourceImageLoadedCommand = new DelegateCommand<FrameworkElement>(OnSourceImageLoaded);
            TargetSwapChainPanelLoadedCommand = new DelegateCommand<FrameworkElement>(OnTargetSwapChainPanelLoaded);
            EditPhotoCommand = new DelegateCommand(() => EditPhoto(), () => CanExecuteEditPhoto());
            BrowseForImageCommand = new DelegateCommand(OnBrowseForImage);
            SaveCommand = new DelegateCommand(OnSaveImage);

            RestoreDefaultCommand = new DelegateCommand(OnRestoreDefaultValues);

            m_slowUpdateTimer = new DispatcherTimer();
            m_slowUpdateTimer.Tick += OnSlowUpdate;
            m_slowUpdateTimer.Interval = TimeSpan.FromSeconds(1);
            SelectedEffect = null;

            InitializeCategoryFilter();
        }

        private void OnRestoreDefaultValues()
        {
            foreach(IImageProcessor imageProcessor in FilterEffects)
            {
                imageProcessor.RestoreDefaultValues();
            }           
        }

        protected async void OnSaveImage()
        {
            var file = await PickSaveFileAsync();
            if (file == null)
                return;

            await SaveImageAsync(file);
        }

        private async void OnBrowseForImage()
        {
            var file = await PickSingleFileAsync();
            if (file == null)
                return;

            await LoadPhotoAsync(file);
        }

        public EffectCategoryItem SelectedFilterCategory
        {
            get { return m_selectedFilterCategory; }
            set
            {
                m_selectedFilterCategory.IsSelected = false;
                SetProperty(ref m_selectedFilterCategory, value);
                IsFlyOutClosed = true;
                m_selectedFilterCategory.IsSelected = true;
                OnPropertyChanged("FilterEffects");
                OnPropertyChanged("CurrentFilterText");
            }
        }

        public string CurrentFilterText
        {
            get
            {
                if (m_selectedFilterCategory == null)
                    return string.Empty;

                return string.Format("{0} Filters", m_selectedFilterCategory.Name);
            }
        }

        private void InitializeCategoryFilter()
        {
            var tempFilterCategories = new List<EffectCategoryItem>();

            foreach (var effectCategory in Enum.GetValues(typeof(EffectCategoryEnum)))
            {
                tempFilterCategories.Add(new EffectCategoryItem() { Name = Enum.GetName(typeof(EffectCategoryEnum), effectCategory), Value = (EffectCategoryEnum)effectCategory });
            }

            m_selectedFilterCategory = tempFilterCategories.FirstOrDefault(x => x.Value == EffectCategoryEnum.All);
            m_selectedFilterCategory.IsSelected = true;

            FilterCategories = new ObservableCollection<EffectCategoryItem>(tempFilterCategories);
        }
        
        private void EditPhoto()
        {
            IsEditingViewVisible = !IsEditingViewVisible;
        }

        private bool CanExecuteEditPhoto()
        {
            return CurrentEditors != null && CurrentEditors.Count > 0;
        }

        public bool IsFlyOutClosed
        {
            get { return m_IsFlyOutClosed; }
            set
            {
                SetProperty(ref m_IsFlyOutClosed, value);
                //Reset variable when flyout is closed
                SetProperty(ref m_IsFlyOutClosed, false);
            }
        }

        private void OnTargetSwapChainPanelLoaded(FrameworkElement frameworkElement)
        {
            m_targetSwapChainPanel = (SwapChainPanel)frameworkElement;

            if (m_targetSwapChainPanel.ActualHeight == 0 || m_targetSwapChainPanel.ActualWidth == 0)
            {
                m_targetSwapChainPanel.Width = SourceImageObservedWidth;
                m_targetSwapChainPanel.Height = SourceImageObservedHeight;
            }
                    
            m_targetSwapChainPanel.SizeChanged +=  async (sender, args) =>
            {
                if (m_targetSwapChainPanel.ActualHeight > 0 && m_targetSwapChainPanel.ActualWidth > 0)
                {
                    SourceImageObservedWidth = m_targetSwapChainPanel.ActualWidth;
                    SourceImageObservedHeight = m_targetSwapChainPanel.ActualHeight;
                    if (SelectedEffect != null)
                    {
                        await m_renderer.RenderAsync(SelectedEffect);
                    }                    
                }
            };

        }

        private static double GetRawPixelsPerViewPixel()
        {
            var displayInformation = Windows.Graphics.Display.DisplayInformation.GetForCurrentView();

            return displayInformation.RawPixelsPerViewPixel;
        }

        private void OnSourceImageLoaded(FrameworkElement frameworkElement)
        {
            var rawPixelsPerViewPixel = GetRawPixelsPerViewPixel();
            
            var rawPixelWidth = frameworkElement.ActualWidth * rawPixelsPerViewPixel;
            var rawPixelHeight = frameworkElement.ActualHeight * rawPixelsPerViewPixel;

            SourceImageObservedWidth = frameworkElement.ActualWidth;
            SourceImageObservedHeight = frameworkElement.ActualHeight;

            if (rawPixelWidth < rawPixelHeight)
            {
                rawPixelHeight = 0;
            }
            else
            {
                rawPixelWidth = 0;
            }

            m_renderer = new ImageProcessorRenderer(new Size(rawPixelWidth, rawPixelHeight));
            m_renderer.SourcePreviewAvailable += (s, _) =>
            {
                // ReSharper disable once CSharpWarnings::CS4014
                TaskUtilities.RunOnDispatcherThreadAsync(() => OnSourcePreviewAvailable());
            };

            // ReSharper disable once CSharpWarnings::CS4014
            TaskUtilities.RunOnDispatcherThreadAsync(async () =>
            {
                var imageProcessors = await m_renderer.GetImageProcessorsAsync();

                FilterEffects = new ObservableCollection<IImageProcessor>(imageProcessors);
            });
        }
        private async void OnSourcePreviewAvailable()
        {
            var sourceWriteableBitmap = await ConvertPreviewToWriteableBitmap(m_renderer.SourcePreviewBitmap, null);
            SourceImageSource = sourceWriteableBitmap;

			var size = new Size(m_renderer.SourcePreviewBitmap.PixelWidth, m_renderer.SourcePreviewBitmap.PixelHeight);
            SourceImageRenderMessage = string.Format(SourceImageRenderMessageFormatString, size, 0, 0);

			if (SelectedEffect != null)
            {
                UpdateTargetPreviewImage();
            }
        }

        private void OnSlowUpdate(object sender, object o)
        {
            m_isTargetImageRenderMessageDirty = true;
        }

        public bool IsEditingViewVisible
        {
            get
            {
                if (CurrentEditors == null || CurrentEditors.Count == 0)
                    return false;

                return m_IsEditingViewVisible;
            }
            set
            {
                SetProperty(ref m_IsEditingViewVisible, value);
                OnPropertyChanged(() => CurrentState);
            }
        }

        public string CurrentState
        {
            get { return IsEditingViewVisible ? "Editing" : "Normal"; }
        }

        public IEnumerable<IImageProcessor> FilterEffects
        {
            get
            {
                if (m_selectedFilterCategory.Value == EffectCategoryEnum.All)
                {
                    return m_FilterEffects;
                }
                return m_FilterEffects.Where(x => x.EffectCategory == m_selectedFilterCategory.Value);
            }
            protected set { SetProperty(ref m_FilterEffects, value); }
        }

        public IImageProcessor SelectedEffect
        {
            get { return m_SelectedEffect; }
            set
            {
                if (SetProperty(ref m_SelectedEffect, value))
                {
                    if (m_SelectedEffect != null)
                    {
                        CurrentEditors = m_SelectedEffect.Editors;

                        UpdateFilterDescription();
                    }
                    else
                    {
                        CurrentEditors = null;
                    }

                    UpdateTargetPreviewImage();
                }
            }
        }

        private async void UpdateFilterDescription()
        {
            if (m_SelectedEffect == null)
            {
                FilterDescription = "";
                return;
            }

            string desc = m_SelectedEffect.Name + "\n";
            
            desc += "\n";

            FilterDescription = desc;
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

        public bool CanSelectNewEffect
        {
            get { return m_CanSelectNewEffect; }
            set { SetProperty(ref m_CanSelectNewEffect, value); }
        }

        public static async Task SaveGraphFilePicturesLibraryAsync(string graph, string fileName)
        {
            try
            {
                var file = await KnownFolders.PicturesLibrary.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
                var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                using (var dataWriter = new DataWriter(stream))
                {
                    dataWriter.WriteString(graph);
                    await dataWriter.StoreAsync();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public ObservableCollection<EditorViewModelBase> CurrentEditors
        {
            get { return m_CurrentEditors; }
            set
            {
                if (m_CurrentEditors == value)
                    return;

                if (m_CurrentEditors != null)
                {
                    foreach (var editor in m_CurrentEditors)
                    {
                        var notifyPropertyChanged = editor as INotifyPropertyChanged;
                        if (notifyPropertyChanged != null)
                        {
                            notifyPropertyChanged.PropertyChanged -= OnEditorPropertyChanged;
                        }
                    }
                }

                m_CurrentEditors = value;

                if (m_CurrentEditors != null)
                {
                    foreach (var editor in m_CurrentEditors)
                    {
                        var notifyPropertyChanged = editor as INotifyPropertyChanged;
                        if (notifyPropertyChanged != null)
                        {
                            notifyPropertyChanged.PropertyChanged += OnEditorPropertyChanged;
                        }
                    }
                }
                base.OnPropertyChanged();

                base.OnPropertyChanged(() => IsEditingViewVisible);
                base.OnPropertyChanged(() => CurrentState);
                EditPhotoCommand.NotifyCanExecuteChanged();
            }
        }

        private void OnEditorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateTargetPreviewImage();
        }

        private async void UpdateTargetPreviewImage()
        {
            var localSelectedEffect = SelectedEffect;
            if (SelectedEffect == null)
            {
                return;
            }

            m_isRenderDirty = true;

            if (m_isRendering)
            {
                return;
            }

            m_isRendering = true;

            while (m_isRenderDirty)
            {
                if (CurrentEditors != null)
                {
                    foreach (var editor in CurrentEditors)
                    {
                        editor.Apply();
                    }
                }
                m_isRenderDirty = false;

                RenderResult renderResult;
                m_renderer.SwapChainPanel = m_targetSwapChainPanel;
                renderResult = await m_renderer.RenderAsync(SelectedEffect, RenderOption.RenderToSwapChainPanel);

                Debug.Assert(renderResult.SwapChainPanel != null);

                if (m_isTargetImageRenderMessageDirty)
                {
                    m_isTargetImageRenderMessageDirty = false;

                    TargetImageRenderMessage = string.Format(TargetImageRenderMessageFormatString, renderResult.Size, renderResult.SetupTimeMillis, renderResult.RenderTimeMillis);
                }
            }

            m_isRendering = false;
        }

        public async Task<bool> SaveImageAsync(StorageFile file)
        {
            if (m_SelectedEffect == null)
            {
                return false;
            }

            string errorMessage = null;

            try
            {
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    // Jpeg renderer gives the raw buffer containing the effected image.
                    var renderResult = await m_renderer.RenderAsync(m_SelectedEffect, RenderOption.RenderAtSourceSize | RenderOption.RenderToJpeg);
                    await stream.WriteAsync(renderResult.Buffer);
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

		private async Task<WriteableBitmap> ConvertPreviewToWriteableBitmap(SoftwareBitmap softwareBitmap, WriteableBitmap writeableBitmap)
		{
			int previewWidth = (int)m_renderer.PreviewSize.Width;
			int previewHeight = (int)m_renderer.PreviewSize.Height;

			if (writeableBitmap == null || writeableBitmap.PixelWidth != previewWidth || writeableBitmap.PixelHeight != previewHeight)
			{
				writeableBitmap = new WriteableBitmap(previewWidth, previewHeight);
			}

			if (softwareBitmap.PixelWidth != previewWidth || softwareBitmap.PixelHeight != previewHeight)
			{
				using (var renderer = new WriteableBitmapRenderer(new SoftwareBitmapImageSource(softwareBitmap)))
				{
					renderer.WriteableBitmap = writeableBitmap;

					await renderer.RenderAsync();
				}
			}
			else
			{
				softwareBitmap.CopyToBuffer(writeableBitmap.PixelBuffer);
			}

			writeableBitmap.Invalidate();
		
			return writeableBitmap;
		}

		private async Task<WriteableBitmap> ConvertPreviewToWriteableBitmap(Bitmap bitmap, WriteableBitmap writeableBitmap)
        {
            int previewWidth = (int)m_renderer.PreviewSize.Width;
            int previewHeight = (int)m_renderer.PreviewSize.Height;

            if (writeableBitmap == null || writeableBitmap.PixelWidth != previewWidth || writeableBitmap.PixelHeight != previewHeight)
            {
                writeableBitmap = new WriteableBitmap(previewWidth, previewHeight);
            }

            if (bitmap.Dimensions != m_renderer.PreviewSize)
            {
                // Re-render Bitmap to WriteableBitmap at the correct size.
                using (var bitmapImageSource = new BitmapImageSource(bitmap))
                using (var renderer = new WriteableBitmapRenderer(bitmapImageSource, writeableBitmap))
                {
                    renderer.RenderOptions = RenderOptions.Cpu;
                    await renderer.RenderAsync().AsTask();
                    writeableBitmap.Invalidate();
                }
            }
            else
            {
                // Already at the display size, so just copy.
                bitmap.CopyTo(writeableBitmap);
                writeableBitmap.Invalidate();
            }

            return writeableBitmap;
        }

        public Task LoadPhotoAsync(StorageFile file)
        {
            return m_renderer.LoadPhotoAsync(new StorageFileImageSource(file));
        }

        private static void OnThumbnailComplete(IImageProcessor processor, Bitmap bitmap)
        {
            // Note: intentionally not awaited. We just deliver the thumbnail Bitmaps to the UI thread.
            // ReSharper disable once CSharpWarnings::CS4014
            TaskUtilities.RunOnDispatcherThreadAsync(() =>
            {
                processor.ThumbnailImagSource = bitmap.ToWriteableBitmap();

                // The thumbnail image has been copied, so we can destroy the Bitmap.
                bitmap.Dispose();
            });
        }

        public string FilterDescription
        {
            get { return m_effectDescription; }
            set
            {
                SetProperty(ref m_effectDescription, value);
            }
        }

    }
}
