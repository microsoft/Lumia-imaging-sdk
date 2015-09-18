
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
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Numerics;
using Microsoft.Graphics.Canvas.Effects;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Lumia.Imaging;
using Windows.UI.Core;
using Windows.Foundation.Metadata;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace LumiaImagingSDKWin2DDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PhotoEffectView : Page
    {
        private SoftwareBitmap m_sepiaEffectSoftwareBitmap;
        private Size m_imageSize;

        public PhotoEffectView()
        {
            this.InitializeComponent();
            
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (s, a) =>
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                    a.Handled = true;
                }
            };

            if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
            {
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += (s, a) =>
                {
                    if (Frame.CanGoBack)
                    {
                        Frame.GoBack();
                        a.Handled = true;
                    }
                };
            }

            this.Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            var file = await StorageFile.GetFileFromApplicationUriAsync(new System.Uri("ms-appx:///Assets/defaultImage.jpg"));

            var storageFileImageSource = new StorageFileImageSource(file);
            var asyncImageResource = storageFileImageSource as IAsyncImageResource;
            var imageResource = await asyncImageResource.LoadAsync();
            m_imageSize = imageResource.ImageSize;
            var sepiaEffect = new Lumia.Imaging.Artistic.SepiaEffect(storageFileImageSource);

            SoftwareBitmapRenderer softwareBitmapRenderer = new SoftwareBitmapRenderer(sepiaEffect);

            m_sepiaEffectSoftwareBitmap = await softwareBitmapRenderer.RenderAsync();

            m_canvasControl.Invalidate();
        }

        Random rnd = new Random();
        private Vector2 RndPosition()
        {
            double x = rnd.NextDouble() * 500f;
            double y = rnd.NextDouble() * 500f;
            return new Vector2((float)x, (float)y);
        }

        private float RndRadius()
        {
            return (float)rnd.NextDouble() * 150f;
        }

        private byte RndByte()
        {
            return (byte)rnd.Next(256);
        }

        private ICanvasImage CreateDirectionalBlur(CanvasBitmap source)
        {
            var blurEffect = new DirectionalBlurEffect
            {
                Source = source
            };

            return blurEffect;
        }

        void OnCanvasControlDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            // Note It´s not allowed to use async calls in this method the DrawingSession is disposed when leving this function.
            // Thats why the m_sepiaEffectSoftwareBitmap is crated on loaded
            if (m_sepiaEffectSoftwareBitmap != null)
            {
                var destinationRect = new Rect(0, 0, sender.ActualWidth, sender.ActualHeight);
                CanvasBitmap canvasBitmap = CanvasBitmap.CreateFromSoftwareBitmap(sender.Device, m_sepiaEffectSoftwareBitmap);

                args.DrawingSession.DrawImage(CreateDirectionalBlur(canvasBitmap), destinationRect, new Rect(0, 0, m_imageSize.Width, m_imageSize.Height));
            }

            args.DrawingSession.DrawRectangle(new Rect(0, 0, sender.ActualWidth, sender.ActualHeight), Colors.Black, 20);

            for (int i = 0; i < 5; i++)
            {
                args.DrawingSession.DrawText("Lumia Imaging SDK!", RndPosition(), Color.FromArgb(255, RndByte(), RndByte(), RndByte()));
                args.DrawingSession.DrawText("Win2D!", RndPosition(), Color.FromArgb(255, RndByte(), RndByte(), RndByte()));
                args.DrawingSession.DrawCircle(RndPosition(), RndRadius(), Color.FromArgb(255, RndByte(), RndByte(), RndByte()));
                args.DrawingSession.DrawLine(RndPosition(), RndPosition(), Color.FromArgb(255, RndByte(), RndByte(), RndByte()));
            }
        }
    }
}
