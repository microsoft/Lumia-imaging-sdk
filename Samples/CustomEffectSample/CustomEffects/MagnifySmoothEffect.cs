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
using Lumia.Imaging.Workers;
using Windows.Storage.Streams;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

namespace CustomEffects
{
    public class MagnifySmoothEffect : EffectBase
    {
        public IBuffer PixelShader { get; set; }
        public double OuterRadius { get; set; }
        public double MagnificationAmount { get; set; }
        public double HorizontalPosition { get; set; }
        public double VerticalPosition { get; set; }
        public double AspectRatio { get; set; }
        public double InnerRadius { get; set; }

        public MagnifySmoothEffect()
        {
            InnerRadius = 0.2f;
            OuterRadius = 0.4f;
            MagnificationAmount = 2.0f;
            HorizontalPosition = 0.3f;
            VerticalPosition = 0.3f;
            AspectRatio = 1.0f;

            LoadPixelShader();
        }
        public void LoadPixelShader()
        {
            var assembly = typeof(MagnifySmoothEffect).GetTypeInfo().Assembly;
            
            Stream stream = assembly.GetManifestResourceStream("CustomEffects.MagnifySmooth.pso");
            var streamReader = new MemoryStream();
            stream.CopyTo(streamReader);
            PixelShader = streamReader.ToArray().AsBuffer();
        }

        public override RenderOptions SupportedRenderOptions
        {
            get
            {
                return RenderOptions.Gpu;
            }
        }
      
        public override IImageProvider2 Clone()
        {
            var effect = new MagnifySmoothEffect();
            effect.PixelShader = PixelShader;
            effect.Source = ((IImageProvider2)Source).Clone();
            effect.AspectRatio = AspectRatio;
            effect.HorizontalPosition = HorizontalPosition;
            effect.InnerRadius = InnerRadius;
            effect.MagnificationAmount = MagnificationAmount;
            effect.OuterRadius = OuterRadius;
            return effect;
        }

        public override IImageWorker CreateImageWorker(IImageWorkerRequest imageWorkerRequest)
        {
            if (imageWorkerRequest.RenderOptions == RenderOptions.Gpu)
            {
                return new MagnifySmoothEffectDirect2DWorker(this);
            }

            return null;
        }
    }
}
