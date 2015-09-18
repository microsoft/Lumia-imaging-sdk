// ----------------------------------------------------------------------
// Copyright © 2014 Microsoft Mobile. All rights reserved.
// Contact: Sergiy Dubovik <sergiy.dubovik@microsoft.com>
// 
// This software, including documentation, is protected by copyright controlled by
// Microsoft Mobile. All rights are reserved. Copying, including reproducing, storing,
// adapting or translating, any or all of this material requires the prior written consent of
// Microsoft Mobile. This material also contains confidential information which may not
// be disclosed to others without the prior written consent of Microsoft Mobile.
// ----------------------------------------------------------------------

using System.Threading.Tasks;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging.ShowCase.Extensions;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using SDKTestApp_Native;

namespace Lumia.Imaging.ShowCase.ImageProcessors.CreativeStudio
{
    public class VividEffect : CreativeStudioEffect
    {
        public VividEffect()
        {            
            LayerList.AddRange(
                new AdjustmentLayer(LayerStyle.Normal(), new VibranceEffect {Level = 0.4f/1.6f}),
                new AdjustmentLayer(LayerStyle.Normal(), Lookups.VividCurvesEffect),
                new AdjustmentLayer(LayerStyle.Normal(), context => new CachingEffect { Size = context.HintedRenderSize }),
				new AdjustmentLayer(LayerStyle.Softlight(0.75), new SDKTestApp_Native.HighpassEffect(72, true, 0)),
                new Layer(LayerStyle.Softlight(), context => Lookups.VividVignette.GetAsync(context.HintedRenderSize))
            );
        }
    }
}
