using Windows.Foundation;
using Windows.UI;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe
{

    public class GlamMeLomoEffect : GlamMeEffect
    {
		public GlamMeLomoEffect()
        {
            var globalCurve = new Curve(CurveInterpolation.NaturalCubicSpline, new[]
            {                  
                    new Point(0, 0),
                    new Point(41, 59),
                    new Point(112, 146),
                    new Point(189, 211),
                    new Point(255, 255)
            });

            var redCurve = new Curve(CurveInterpolation.NaturalCubicSpline, new[]
            {
                new Point(0, 0),
                new Point(101, 45),
                new Point(191, 193),
                new Point(255, 255)
            });
            
            Curve.Compose(redCurve, globalCurve, redCurve);

            var greenCurve = new Curve(CurveInterpolation.NaturalCubicSpline, new[]
            {
                new Point(0, 0),
                new Point(78, 61),
                new Point(187, 199),
                new Point(255, 255)
            });

            Curve.Compose(greenCurve, globalCurve, greenCurve);

            var blueCurve = new Curve(CurveInterpolation.NaturalCubicSpline, new[]
            {
                new Point(0, 0),
                new Point(56, 71),
                new Point(204, 181),
                new Point(255, 255)
            });
            
            Curve.Compose(blueCurve, globalCurve, blueCurve);

            var linearGradient = new LinearGradient
            {
                StartPoint = new Point(0.0, 0.5),
                EndPoint = new Point(1.0, 0.5),

                Stops = new[]
                {
                    new GradientStop {Offset = 0, Color = Color.FromArgb(255, 135, 135, 135) },
                    new GradientStop {Offset = 1.0, Color = Color.FromArgb(255, 193, 193, 193) }
                }
            };

            var vignetteGradient = new RadialGradient
            {
                CenterPoint = new Point(0.5, 0.5),
                EllipseRadius = new EllipseRadius(2.0, 0),
                Stops = new[]
                {
                    new GradientStop {Offset = 0, Color = Color.FromArgb(0, 255, 255, 255)},
                    new GradientStop {Offset = 0.9, Color = Color.FromArgb(255, 0, 0, 0)}
                }
            };

            var vignette2Gradient = new RadialGradient
            {
                CenterPoint = new Point(0.5, 0.5),
                EllipseRadius = new EllipseRadius(1.0, 0),
                Stops = new[]
                {
                    new GradientStop {Offset = 0, Color = Color.FromArgb(0, 0, 0, 0)},                  
                    new GradientStop {Offset = 0.5, Color = Color.FromArgb(0, 0, 0, 0)},                  
                    new GradientStop {Offset = 1.0, Color = Color.FromArgb(255, 0, 0, 0)}
                }
            };

            LayerList = new LayerList()
            {
                new AdjustmentLayer(LayerStyle.Normal(), new CurvesEffect(redCurve, greenCurve, blueCurve)),
                new Layer(LayerStyle.Overlay(), context => new GradientImageSource(context.BackgroundLayer.ImageSize, linearGradient)),
                new Layer(LayerStyle.Softlight(0.4), context => new GradientImageSource(context.BackgroundLayer.ImageSize, vignetteGradient)),
                new Layer(LayerStyle.Darken(0.5), context => new GradientImageSource(context.BackgroundLayer.ImageSize, vignette2Gradient))
            };
        }
    }
}
