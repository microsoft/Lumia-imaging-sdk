
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomEffects
{
    class CustomGrayscaleDirect2DWorker : Direct2DPixelShaderImageWorkerBase
    {   
        private CustomGrayscaleEffect m_configuration;
        public CustomGrayscaleDirect2DWorker(CustomGrayscaleEffect configuration)
        {
            InputCount = 1;

            m_configuration = configuration;

            PixelShader = m_configuration.PixelShader;

        }

    }
}

