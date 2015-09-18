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
using Lumia.Imaging.Extras.Effects;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging.ShowCase.Extensions;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using SDKTestApp_Native;

namespace Lumia.Imaging.ShowCase.ImageProcessors.CreativeStudio
{
    public class NeonEffect : CreativeStudioEffect
    {
        public NeonEffect()
        {
            var saturationLightnessEffect = new SaturationLightnessEffect(null, new Curve(CurveInterpolation.Linear, new[]
            {
                new Point(0, 36),
                new Point(45, 60),
                new Point(104, 146),
                new Point(197, 207),
                new Point(255, 245)
            }));

         
            var hslEffect = new HueSaturationLightnessEffect
            {
                SaturationCurve = PhotoshopAlike.CreateCurveFromHueRangeAdjustments(
                    new PhotoshopAlike.HueRangeAdjustment(0, 60),
                    new PhotoshopAlike.HueRangeAdjustment(1, -5),
                    new PhotoshopAlike.HueRangeAdjustment(2, -7),
                    new PhotoshopAlike.HueRangeAdjustment(3, -10),
                    new PhotoshopAlike.HueRangeAdjustment(5, 25),
                    new PhotoshopAlike.HueRangeAdjustment(6, 22))
            };

            
            LayerList.AddRange(
                 new AdjustmentLayer(LayerStyle.Normal(), saturationLightnessEffect),
                new AdjustmentLayer(LayerStyle.Normal(), Lookups.NeonCurvesEffect),
				new AdjustmentLayer(LayerStyle.Normal(), hslEffect),
                new AdjustmentLayer(LayerStyle.Normal(), context => new CachingEffect()),
                new AdjustmentLayer(LayerStyle.Softlight(0.75), context => new SDKTestApp_Native.HighpassEffect(32, false, 8))
            );
        }
    }
}
