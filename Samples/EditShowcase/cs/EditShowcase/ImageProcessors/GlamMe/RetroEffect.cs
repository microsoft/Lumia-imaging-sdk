using Windows.Foundation;
using Windows.UI;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe
{
    public class RetroEffect : GlamMeEffect
    {
        public RetroEffect()
        {
            var globalCurve = new Curve
            {
                Points = new[]
                {
                    new Point(0, 0),
                    new Point(41, 59),
                    new Point(112, 146),
                    new Point(189, 211),
                    new Point(255, 255)                 
                }
            };

            var redCurve = new Curve(CurveInterpolation.NaturalCubicSpline, new[]
            {
                new Point(0, 0),
                new Point(75, 61),
                new Point(255, 255)
            });

            Curve.Compose(redCurve, globalCurve, redCurve);


            var greenCurve = new Curve(CurveInterpolation.NaturalCubicSpline, new[]
            {
                new Point(0, 0),
                new Point(59, 34),
                new Point(182, 199),
                new Point(255, 255)

            });

            Curve.Compose(greenCurve, globalCurve, greenCurve);

            var blueCurve = new Curve(CurveInterpolation.NaturalCubicSpline, new[]
            {
                new Point(0, 91),
                new Point(128, 128),
                new Point(255, 185)

            });

            Curve.Compose(blueCurve, globalCurve, blueCurve);

            var yellowPurpleGradient = new LinearGradient
            {
                StartPoint = new Point(0.0, 0.5),
                EndPoint = new Point(1.0, 0.5),

                Stops = new[]
                {
                    new GradientStop {Offset = 0, Color = Color.FromArgb(255, 255, 255, 0)},
                    new GradientStop {Offset = 0.5, Color = Color.FromArgb(255, 148, 106, 77)},
                    new GradientStop {Offset = 1.0, Color = Color.FromArgb(255, 72, 3, 131)}
                }
            };

            var redTransparentGradient = new LinearGradient
            {
                StartPoint = new Point(0.0, 0.5),
                EndPoint = new Point(1.0, 0.5),

                Stops = new[]
                {
                    new GradientStop {Offset = 0, Color = Color.FromArgb(255, 255, 0, 0)},
                    new GradientStop {Offset = 1.0, Color = Color.FromArgb(0, 255, 0, 0)}
                }
            };

            var spotGradient = new RadialGradient
            {
                CenterPoint = new Point(0.5, 0.3),
                EllipseRadius = new EllipseRadius(0, 0.2),
                Stops = new[]
                {
                    new GradientStop {Offset = 0.0, Color = Color.FromArgb(255, 255, 255, 255)},
                    new GradientStop {Offset = 1.0, Color = Color.FromArgb(0, 255, 255, 255)}
                }
            };


            var vignetteGradient = new RadialGradient
            {
                CenterPoint = new Point(0.5, 0.5),
                EllipseRadius = new EllipseRadius(1.0, 0),
                Stops = new[]
                {
                    new GradientStop {Offset = 0, Color = Color.FromArgb(255, 255, 255, 255)},
                    new GradientStop {Offset = 0.9, Color = Color.FromArgb(255, 0, 0, 0)}
                }
            };

            LayerList = new LayerList()
            {
                new AdjustmentLayer(LayerStyle.Normal(), new CurvesEffect(redCurve, greenCurve, blueCurve)),
                new Layer(LayerStyle.Softlight(0.3), context => new GradientImageSource(context.BackgroundLayer.ImageSize, yellowPurpleGradient)),
                new Layer(LayerStyle.Screen(0.75, targetArea: new Rect(0, 0, 0.4, 1.0)), context => new GradientImageSource(new Size(context.BackgroundLayer.ImageSize.Width * 0.4, context.BackgroundLayer.ImageSize.Height), redTransparentGradient)),
                new Layer(LayerStyle.Softlight(), context => new GradientImageSource(context.BackgroundLayer.ImageSize, spotGradient)),
                new Layer(LayerStyle.Softlight(0.6), context => new GradientImageSource(context.BackgroundLayer.ImageSize, vignetteGradient))
            };
        }
    }
}
