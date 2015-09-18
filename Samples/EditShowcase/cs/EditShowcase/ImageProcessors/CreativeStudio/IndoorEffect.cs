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
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging.EditShowcase.Extensions;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.CreativeStudio
{
    public class IndoorEffect : CreativeStudioEffect
    {
        public IndoorEffect()
        {
			LayerList.AddRange(
                new AdjustmentLayer(LayerStyle.Normal(), Lookups.IndoorCurvesEffect),
                new AdjustmentLayer(LayerStyle.Normal(), new HueSaturationLightnessEffect
				{
					SaturationCurve = EffectPhotoExtensions.CreateCurveFromHueRangeAdjustments(
						new EffectPhotoExtensions.HueRangeAdjustment(0, -10),
						new EffectPhotoExtensions.HueRangeAdjustment(1, 15),
						new EffectPhotoExtensions.HueRangeAdjustment(2, -35),
						new EffectPhotoExtensions.HueRangeAdjustment(5, -35)
						),
					LightnessCurve = EffectPhotoExtensions.CreateCurveFromHueRangeAdjustments(
						new EffectPhotoExtensions.HueRangeAdjustment(2, -5)
						)
				}),
				new AdjustmentLayer(LayerStyle.Normal(), new SaturationLightnessEffect(null, new Curve(CurveInterpolation.Linear, new[] { new Point(0, 25) }))),
				new AdjustmentLayer(LayerStyle.Normal(), new VibranceEffect { Level = 0.3 / 1.6 })
			);
        }
    }
}
