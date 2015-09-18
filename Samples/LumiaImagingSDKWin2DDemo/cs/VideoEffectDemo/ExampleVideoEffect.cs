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
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.UI;
using Windows.UI.Text;

namespace VideoEffectDemo
{
    public sealed class ExampleVideoEffect : IBasicVideoEffect
    {
        CanvasDevice canvasDevice;
        Direct3DSurfaceImageSource m_direct3DSurfaceImageSource;
        Lumia.Imaging.Adjustments.GrayscaleEffect m_grayScaleEffect = new Lumia.Imaging.Adjustments.GrayscaleEffect();
        Direct3DSurfaceRenderer m_direct3DSurfaceRenderer;

        public bool IsReadOnly { get { return true; } }

        public IReadOnlyList<VideoEncodingProperties> SupportedEncodingProperties
        {
            get
            {
                return new List<VideoEncodingProperties>();
            }
        }

        public MediaMemoryTypes SupportedMemoryTypes { get { return MediaMemoryTypes.Gpu; } }

        public bool TimeIndependent { get { return false; } }

        public void Close(MediaEffectClosedReason reason)
        {
            if (canvasDevice != null) { canvasDevice.Dispose(); }
        }

        public void DiscardQueuedFrames() { }

        public void ProcessFrame(ProcessVideoFrameContext context)
        {
            var inputDirect3DSurface = context.InputFrame.Direct3DSurface;

            float anyDpi = 96.0f;
            var width = context.OutputFrame.Direct3DSurface.Description.Width;
            var height = context.OutputFrame.Direct3DSurface.Description.Height;

            // Part 1: Apply effect using Imaging SDK
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(canvasDevice, (float)width, (float)height, anyDpi);

            if (inputDirect3DSurface != null && renderTarget != null)
            {
                if (m_direct3DSurfaceImageSource == null)
                {
                    m_direct3DSurfaceImageSource = new Direct3DSurfaceImageSource();
                }

                m_direct3DSurfaceImageSource.Direct3DSurface = inputDirect3DSurface;
                ((IImageConsumer)m_grayScaleEffect).Source = m_direct3DSurfaceImageSource;

                if (m_direct3DSurfaceRenderer == null)
                {
                    m_direct3DSurfaceRenderer = new Direct3DSurfaceRenderer();
                }

                m_direct3DSurfaceRenderer.Direct3DSurface = renderTarget;
                m_direct3DSurfaceRenderer.Source = m_grayScaleEffect;

                var task = m_direct3DSurfaceRenderer.RenderAsync().AsTask();
                task.Wait();

                m_direct3DSurfaceRenderer.Direct3DSurface = null;
                m_direct3DSurfaceImageSource.Direct3DSurface = null;
            }

            // Part 2: Apply another effect using Win2D
            var size = context.OutputFrame.Direct3DSurface.Description.Width * context.OutputFrame.Direct3DSurface.Description.Height;
            var colors = Enumerable.Repeat(Colors.Black, size).ToArray();

            var inputBitmap = CanvasBitmap.CreateFromColors(canvasDevice, colors, width, height, anyDpi);
            inputBitmap.CopyPixelsFromBitmap(renderTarget);

            using (CanvasRenderTarget output = CanvasRenderTarget.CreateFromDirect3D11Surface(canvasDevice, context.OutputFrame.Direct3DSurface))
            using (CanvasDrawingSession ds = output.CreateDrawingSession())
            {
                TimeSpan time = context.InputFrame.RelativeTime.HasValue ? context.InputFrame.RelativeTime.Value : new TimeSpan();

                float dispX = (float)Math.Cos(time.TotalSeconds) * 75f;
                float dispY = (float)Math.Sin(time.TotalSeconds) * 75f;

                ds.Clear(Colors.Black);

                var dispMap = new DisplacementMapEffect()
                {
                    Source = inputBitmap,
                    XChannelSelect = EffectChannelSelect.Red,
                    YChannelSelect = EffectChannelSelect.Green,
                    Amount = 100f,
                    Displacement = new Transform2DEffect()
                    {
                        TransformMatrix = Matrix3x2.CreateTranslation(dispX, dispY),
                        Source = new BorderEffect()
                        {
                            ExtendX = CanvasEdgeBehavior.Mirror,
                            ExtendY = CanvasEdgeBehavior.Mirror,
                            Source = new TurbulenceEffect()
                            {
                                Octaves = 3
                            }
                        }
                    }
                };

                ds.DrawImage(dispMap, -25f, -25f);
            }

            ((IDisposable)renderTarget).Dispose();
            ((IDisposable)inputBitmap).Dispose();            
        }

        public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device)
        {
            canvasDevice = CanvasDevice.CreateFromDirect3D11Device(device);        
        }

        public void SetProperties(IPropertySet configuration) { }
    }
}
