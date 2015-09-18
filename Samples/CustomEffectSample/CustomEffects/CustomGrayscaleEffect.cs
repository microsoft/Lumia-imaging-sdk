
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
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Reflection;
using Windows.Storage.Streams;

namespace CustomEffects
{
    public class CustomGrayscaleEffect : EffectBase
    {
        public IBuffer PixelShader { get; set; }
        public CustomGrayscaleEffect()
        {
            LoadPixelShader();
        }

        public override RenderOptions SupportedRenderOptions
        {
            get { return RenderOptions.Mixed; }
        }

        public void LoadPixelShader()
        {
            var assembly = typeof(CustomGrayscaleEffect).GetTypeInfo().Assembly;

            Stream stream = assembly.GetManifestResourceStream("CustomEffects.Grayscale.pso");
            var streamReader = new MemoryStream();
            stream.CopyTo(streamReader);
            PixelShader = streamReader.ToArray().AsBuffer();
        }

        public override IImageProvider2 Clone()
        {
            var effect = new CustomGrayscaleEffect();
            effect.Source = ((IImageProvider2)Source).Clone();

            return effect;
        }

        public override IImageWorker CreateImageWorker(IImageWorkerRequest imageWorkerRequest)
        {
            if (imageWorkerRequest.RenderOptions == RenderOptions.Cpu)
            {
                return new CustomGrayscaleCpuWorker();
            }
            else if (imageWorkerRequest.RenderOptions == RenderOptions.Gpu)
            {
                return new CustomGrayscaleDirect2DWorker(this);
            }

            return null;
        }
    }
}
