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
using Windows.Foundation;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging.EditShowcase.Extensions;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.CreativeStudio
{
    public class SunsetEffect : CreativeStudioEffect
    {
        public SunsetEffect()
        {
            var curvesEffectTask = new LookupCurves("Images\\sunset_table.bmp").GetEffectAsync();

			var hslEffect = new HueSaturationLightnessEffect()
                {
                    SaturationCurve = EffectPhotoExtensions.CreateCurveFromHueRangeAdjustments(
                        new EffectPhotoExtensions.HueRangeAdjustment(0, 19), // master
                        new EffectPhotoExtensions.HueRangeAdjustment(1, 15), // red
                        new EffectPhotoExtensions.HueRangeAdjustment(2, 5), // etc
                        new EffectPhotoExtensions.HueRangeAdjustment(5, 15),
                        new EffectPhotoExtensions.HueRangeAdjustment(6, 20)),
                    LightnessCurve = EffectPhotoExtensions.CreateCurveFromHueRangeAdjustments(
                        new EffectPhotoExtensions.HueRangeAdjustment(1, -5),
                        new EffectPhotoExtensions.HueRangeAdjustment(2, -5),
                        new EffectPhotoExtensions.HueRangeAdjustment(5, -5),
                        new EffectPhotoExtensions.HueRangeAdjustment(6, -5))
                };


            var curvesEffect = new CurvesEffect(new Curve(CurveInterpolation.Linear, new[]
                {
                    new Point(255, 255 - EffectPhotoExtensions.ConvertAdjustmentLevel(5))
                }));

            LayerList.AddRange(
                // Curves
                // HSL
                new AdjustmentLayer(LayerStyle.Normal(), curvesEffectTask),
                new AdjustmentLayer(LayerStyle.Normal(), hslEffect),
				new AdjustmentLayer(LayerStyle.Normal(), curvesEffect),

                // Gradient
                new Layer(LayerStyle.Overlay(0.35), context => Lookups.SunsetVignette.GetAsync(context.BackgroundLayer.ImageSize))
            );
        }

        // artefakter runt stolpar.
        // banding i gradient.

        protected override void Dispose(bool disposing)
        {
        }
    }
}
