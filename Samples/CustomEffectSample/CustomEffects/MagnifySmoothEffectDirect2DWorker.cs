
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
using Lumia.Imaging.Workers.Direct2D;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace CustomEffects
{
    public class MagnifySmoothEffectDirect2DWorker : Direct2DPixelShaderImageWorkerBase
    {
        struct PixelShaderConstant
        {
            public float InnerRadius;
            public float OuterRadius;
            public float MagnificationAmount;
            public float HorizontalPosition;
            public float VerticalPosition;
            public float AspectRatio;
        };

        private MagnifySmoothEffect m_configuration;
        public MagnifySmoothEffectDirect2DWorker(MagnifySmoothEffect configuration)
        {
            InputCount = 1;

            m_configuration = configuration;
           
            PixelShader = m_configuration.PixelShader;

            PixelShaderConstant pixelShaderConstant = InitializePixelShaderConstantBuffer();

            PixelShaderConstantBuffer = StructureToIBuffer(pixelShaderConstant);

        }

        private PixelShaderConstant InitializePixelShaderConstantBuffer()
        {       
            PixelShaderConstant pixelShaderConstant = new PixelShaderConstant();
            pixelShaderConstant.InnerRadius = (float)m_configuration.InnerRadius;
            pixelShaderConstant.OuterRadius = (float)m_configuration.OuterRadius;
            pixelShaderConstant.MagnificationAmount = (float)m_configuration.MagnificationAmount;
            pixelShaderConstant.HorizontalPosition = (float)m_configuration.HorizontalPosition;
            pixelShaderConstant.VerticalPosition = (float)m_configuration.VerticalPosition;
            pixelShaderConstant.AspectRatio = (float)m_configuration.AspectRatio;
            return pixelShaderConstant;
        }
        private IBuffer StructureToIBuffer(PixelShaderConstant pixelShaderConstant)
        {
            int length = Marshal.SizeOf<PixelShaderConstant>(pixelShaderConstant);

            byte[] pixelShaderConstantArray = new byte[length];

            IntPtr ptr = Marshal.AllocHGlobal(length);

            Marshal.StructureToPtr(pixelShaderConstant, ptr, true);

            Marshal.Copy(ptr, pixelShaderConstantArray, 0, length);

            Marshal.FreeHGlobal(ptr);

            return pixelShaderConstantArray.AsBuffer();
        }
    }
}
