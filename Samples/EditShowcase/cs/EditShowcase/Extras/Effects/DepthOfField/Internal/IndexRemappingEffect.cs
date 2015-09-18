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
using Lumia.Imaging.Workers;
using Lumia.Imaging.Workers.Cpu;

namespace Lumia.Imaging.Extras.Effects.DepthOfField.Internal
{
    class IndexRemappingCpuRenderWorker : CpuImageWorkerBase
	{
		uint m_whiteThreshold;

        public IndexRemappingCpuRenderWorker(uint whiteThreshold)
		{
			m_whiteThreshold = (uint)whiteThreshold;
		}

		protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
		{
			for (int i = 0; i < sourcePixelRegion.ImagePixels.Length; i++)
			{
				uint newPixelValue = 0;

				for (int j = 0; j < 3; j++)
				{
					uint pixelValue = (sourcePixelRegion.ImagePixels[i] & (255u << (8 * j))) >> (8 * j);

					if (pixelValue > m_whiteThreshold)
					{
						pixelValue = 0;
					}

					newPixelValue += pixelValue << (8 * j);
				}

				targetPixelRegion.ImagePixels[i] = newPixelValue + (255u << 24);
			}
		}
	}

    class IndexRemappingEffect : EffectBase
    {
		uint m_whiteThreshold;

		public IndexRemappingEffect(IImageProvider source, int whiteThreshold)
		{
			m_whiteThreshold = (uint)whiteThreshold;
		}

        public override RenderOptions SupportedRenderOptions { get { return RenderOptions.Cpu; }}

        public override IImageProvider2 Clone()
        {
            return new IndexRemappingEffect(Source, (int)m_whiteThreshold);
        }

        public override IImageWorker CreateImageWorker(IImageWorkerRequest imageWorkerRequest)
        {
            if(imageWorkerRequest.RenderOptions == RenderOptions.Cpu)
            {
                return new IndexRemappingCpuRenderWorker(m_whiteThreshold);
            }

            return null;
        }
        
    }
}
