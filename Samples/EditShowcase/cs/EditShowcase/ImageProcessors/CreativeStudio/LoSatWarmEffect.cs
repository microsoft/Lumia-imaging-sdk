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

using Windows.UI;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.CreativeStudio
{
    public class LoSatWarmEffect : CreativeStudioEffect
    {
        public LoSatWarmEffect()
        {
            LayerList.AddRange(
                new Layer(LayerStyle.Overlay(0.1), context => new ColorImageSource(context.BackgroundLayer.ImageSize, Color.FromArgb(255, 255, 242, 0))),
                new AdjustmentLayer(LayerStyle.Normal(0.45), new GrayscaleEffect(0.35, 0.66, 0.0, -0.05)),
                new AdjustmentLayer(LayerStyle.Normal(), Lookups.LoSatWarmCurvesEffect),
                new Layer(LayerStyle.Softlight(0.5), context => Lookups.LoSatWarmVignette.GetAsync(context.BackgroundLayer.ImageSize))
            );
        }
    }
}
