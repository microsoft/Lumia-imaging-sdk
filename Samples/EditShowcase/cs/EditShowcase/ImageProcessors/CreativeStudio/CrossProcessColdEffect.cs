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

using Lumia.Imaging.Extras;
using Lumia.Imaging.Extras.ImageSources;
using Lumia.Imaging.Extras.Layers;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.CreativeStudio
{
    public class CrossProcessColdEffect : CreativeStudioEffect
    {
        public CrossProcessColdEffect()
        {
            LayerList.AddRange(
                // "Curves 1"
                new AdjustmentLayer(LayerStyle.Normal(), Lookups.ColdCurvesEffect),
            
                // "Layer 1"
                new Layer(LayerStyle.Overlay(0.8), context => new NoiseImageSource(context.HintedRenderSize, 12)),

                // "Layer 2"
                new Layer(LayerStyle.Softlight(), context => Lookups.ColdVignette.GetAsync(context.HintedRenderSize))
            );
        }
    }
}
