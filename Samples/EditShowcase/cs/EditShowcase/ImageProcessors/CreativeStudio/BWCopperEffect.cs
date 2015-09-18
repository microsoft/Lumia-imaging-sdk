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

using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Extras.Layers;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.CreativeStudio
{
    public class BWCopperEffect : CreativeStudioEffect
    {
        public BWCopperEffect()
        {
            LayerList.AddRange(
                // Channel mixer
                new AdjustmentLayer(LayerStyle.Normal(), new GrayscaleEffect(0.54, 0.64, 0.0, -0.1)),

                // "Curves 1" and "Layer 1".
                new AdjustmentLayer(LayerStyle.Normal(), Lookups.CopperCurvesEffect)
            );
        }
    }
}
