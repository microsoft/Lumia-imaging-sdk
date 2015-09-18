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
using Lumia.Imaging.EditShowcase.Extensions;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Compositing;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.CreativeStudio
{
    class RetrotoneEffect : CreativeStudioEffect
    {
        public RetrotoneEffect()
        {
            // Curves
            var globalCurve = new Curve(CurveInterpolation.Linear, new[]
            {
                new Point(0, 45),
                new Point(33, 57),
                new Point(178, 207),
                new Point(255, 236)
            });

            var curves = new Curve[3];

            curves[0] = new Curve(CurveInterpolation.Linear, new[]
            {
                new Point(10, 0),
                new Point(34, 25),
                new Point(124, 119),
                new Point(255, 246),
                new Point(255, 255)
            });

            curves[1] = new Curve(CurveInterpolation.Linear, new[]
            {
                new Point(0, 0),
                new Point(37, 32),
                new Point(209, 213),
                new Point(255, 250),
                new Point(255, 255)
            });

            curves[2] = new Curve(CurveInterpolation.Linear, new[]
            {
                new Point(0, 52),
                new Point(29, 62),
                new Point(100, 102),
                new Point(176, 171),
                new Point(222, 211),
                new Point(255, 239),
                new Point(255, 255)
            });

            Curve.Compose(globalCurve, curves[0], curves[0]);
            Curve.Compose(globalCurve, curves[1], curves[1]);
            Curve.Compose(globalCurve, curves[2], curves[2]);
            
            var curvesEffect = new CurvesEffect(curves[0], curves[1], curves[2]);

            var hslEffect = new HueSaturationLightnessEffect
            {
                SaturationCurve = EffectPhotoExtensions.CreateCurveFromHueRangeAdjustments(
                    new EffectPhotoExtensions.HueRangeAdjustment(0, -25),
                    new EffectPhotoExtensions.HueRangeAdjustment(2, 5),
                    new EffectPhotoExtensions.HueRangeAdjustment(5, 10)),
                LightnessCurve = EffectPhotoExtensions.CreateCurveFromHueRangeAdjustments(
                    new EffectPhotoExtensions.HueRangeAdjustment(1, -4),
                    new EffectPhotoExtensions.HueRangeAdjustment(2, -9),
                    new EffectPhotoExtensions.HueRangeAdjustment(3, -4),
                    new EffectPhotoExtensions.HueRangeAdjustment(4, -4),
                    new EffectPhotoExtensions.HueRangeAdjustment(5, -9),
                    new EffectPhotoExtensions.HueRangeAdjustment(6, -4))
            };

            LayerList.AddRange(
                new AdjustmentLayer(LayerStyle.Normal(), curvesEffect),

                // Hue/Saturation    
                new AdjustmentLayer(LayerStyle.Normal(), hslEffect),

                // Noise
                new Layer(LayerStyle.Overlay(), context => new NoiseImageSource(context.BackgroundLayer.ImageSize, 13)),

                // Vignette
                new Layer(new LayerStyle(BlendFunction.Hardlight, 0.25, context => Lookups.RetrotoneVignette.GetAsync(context.BackgroundLayer.ImageSize)),
                    context => new ColorImageSource(context.BackgroundLayer.ImageSize, Windows.UI.Color.FromArgb(255, 0, 0, 0)))
            );
        }
    }
}
