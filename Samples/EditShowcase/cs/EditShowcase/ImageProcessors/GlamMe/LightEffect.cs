using Windows.Foundation;
using Windows.UI;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe
{
    class LightEffect : GlamMeEffect
    {
        public LightEffect()
        {
            var globalCurve = new Curve(CurveInterpolation.Linear, new[]
            {
                    new Point(0, 0),
                    new Point(41, 59),
                    new Point(112, 146),
                    new Point(189, 211),
                    new Point(255, 255)
            });

            var redCurve = new Curve(CurveInterpolation.Linear, new[]
            {
                new Point(18, 0),
                new Point(126, 116),
                new Point(255, 228),
            });

            Curve.Compose(redCurve, globalCurve, redCurve);

            var greenCurve = globalCurve;

            var blueCurve = new Curve(CurveInterpolation.Linear, new[]
            {
                new Point(0, 44),
                new Point(113, 138),
                new Point(230, 255),
            });
            
            Curve.Compose(blueCurve, globalCurve, blueCurve);

            var gradient = new RadialGradient
            {
                EllipseRadius = new EllipseRadius(1.5, 0),
                CenterPoint = new Point(0.5, 0.5),
                Stops = new[]
                {
                    new GradientStop {Offset = 0, Color = Color.FromArgb(0, 0, 0, 0)},
                    new GradientStop {Offset = 1, Color = Color.FromArgb(255, 255, 255, 255)}
                }
            };

            var overlayFactory = new OverlayFactory(
                "ms-appx:///images/Filters_Landscape_Overlay_Light.jpg",
                "ms-appx:///images/Filters_Portrait_Overlay_Light.jpg",
                "ms-appx:///images/Filters_Square_Overlay_Light.jpg");

            LayerList = new LayerList()
            {
                new AdjustmentLayer(LayerStyle.Normal(0.2), new CurvesEffect(redCurve, greenCurve, blueCurve)),
                new Layer(LayerStyle.Softlight(0.3), context => new ColorImageSource(context.BackgroundLayer.ImageSize, Color.FromArgb(255, 200, 200, 200))),
                new Layer(LayerStyle.Softlight(0.7), context => new GradientImageSource(context.BackgroundLayer.ImageSize, gradient)),
                new Layer(LayerStyle.Screen(), context => overlayFactory.CreateAsync(context.BackgroundLayer.ImageSize))
            };
        }
    }
}
