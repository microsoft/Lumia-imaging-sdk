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

using Windows.Foundation;
using Lumia.Imaging.Extras;
using Lumia.Imaging.Extras.ImageSources;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.CreativeStudio
{
    public class BWloEffect : CreativeStudioEffect
    {
        public BWloEffect()
        {
            var curve = new Curve(CurveInterpolation.NaturalCubicSpline, new[]
            {
                new Point(0, 65),
                new Point(83, 109),
                new Point(160, 158),
                new Point(255, 239)
            });

            LayerList.AddRange(
                // Curves
                new AdjustmentLayer(LayerStyle.Normal(), new CurvesEffect(curve)),

                // Channel mixer.
                new AdjustmentLayer(LayerStyle.Normal(), new GrayscaleEffect(0.69, 0.61, -0.15, -0.1)),
                    
                // Noise
                new Layer(LayerStyle.Overlay(), context => new NoiseImageSource(context.HintedRenderSize, 19))
            );
        }
    }
}
