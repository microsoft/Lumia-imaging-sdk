
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
using Lumia.Imaging.Workers.Cpu;

namespace CustomEffects
{
    public class CustomGrayscaleCpuWorker : CpuImageWorkerBase
    {
        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {

            targetPixelRegion.ForEachRow((index, width, position) =>
            {
                for (int i = 0; i < width; ++i)
                {
                    var pixel = sourcePixelRegion.ImagePixels[index + i];

                    uint red = (pixel >> 16) & 0x000000FF;
                    uint green = (pixel >> 8) & 0x000000FF;
                    uint blue = (pixel) & 0x000000FF;

                    int average = (int)(0.0722 * blue + 0.7152 * green + 0.2126 * red); // weighted average component
                    targetPixelRegion.ImagePixels[index + i] = (uint)(0xff000000 | average | (average << 8) | (average << 16)); // use average for each color component	
                    
                }

            });
            
        }
    }
}
