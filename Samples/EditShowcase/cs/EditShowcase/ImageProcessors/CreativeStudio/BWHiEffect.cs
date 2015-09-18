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
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.CreativeStudio
{
    public class BWHiEffect : CreativeStudioEffect
    {
        public BWHiEffect()
        {
            var globalCurve = new Curve(CurveInterpolation.NaturalCubicSpline, new[]
            {
                new Point(0, 8),
                new Point(42, 29),
                new Point(137, 157),
                new Point(220, 229),
                new Point(255, 251)
            });

            var redCurve = new Curve(CurveInterpolation.NaturalCubicSpline, new[]
            {
                new Point(0, 0),
                new Point(93, 79),
                new Point(149, 117),
                new Point(207, 186),
                new Point(255, 255)
            });

            var greenCurve = new Curve(CurveInterpolation.NaturalCubicSpline, new[]
            {
                new Point(0, 0),
                new Point(118, 125),
                new Point(255, 255)
            });

            Curve.Compose(globalCurve, redCurve, redCurve);
            Curve.Compose(globalCurve, greenCurve, greenCurve);
            var blueCurve = globalCurve;

            LayerList.AddRange(
                new AdjustmentLayer(LayerStyle.Normal(), new CurvesEffect(redCurve, greenCurve, blueCurve)),
                new AdjustmentLayer(LayerStyle.Normal(), new GrayscaleEffect(0.39, 0.55, 0.32, -0.1))
            );
        }
    }
}
