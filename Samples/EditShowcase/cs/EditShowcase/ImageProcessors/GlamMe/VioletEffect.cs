using Windows.Foundation;
using Windows.UI;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe
{
    public class VioletEffect : GlamMeEffect
    {
        public VioletEffect()
        {
            var globalCurve = new Curve
            {
                Points = new[]
                {
                  new Point(0,0),
                    new Point(41, 59),
                    new Point(112, 146),
                    new Point(189, 211),
                    new Point(255, 255),         
                }
            };

            var redCurve = new Curve
            {
                Points = new[]
                {
                    new Point(0, 0),             
                    new Point(255, 255)
                }
            };

            var greenCurve = new Curve
            {
                Points = new[]
                {
                    new Point(0, 0),             
                    new Point(255, 255)
                }
            };

            var blueCurve = new Curve
            {
                Points = new[]
                {
                    new Point(0, 0),             
                    new Point(57, 53),     
                    new Point(166, 160),     
                    new Point(255, 255)
                }
            };

            Curve.Compose(redCurve, globalCurve, redCurve);
            Curve.Compose(greenCurve, globalCurve, greenCurve);
            Curve.Compose(blueCurve, globalCurve, blueCurve);

            var gradient = new RadialGradient
            {
                CenterPoint = new Point(0.5, 0.5),
                EllipseRadius = new EllipseRadius(1.0, 0),
                Stops = new[]
                {
                    new GradientStop {Offset = 0, Color = Color.FromArgb(255, 156, 62, 68)},
                     new GradientStop {Offset = 1.0, Color = Color.FromArgb(255, 95, 1, 75)},                  
                }
            };

            LayerList = new LayerList()
            {
                new AdjustmentLayer(LayerStyle.Normal(), new CurvesEffect(redCurve, greenCurve, blueCurve)),
                new AdjustmentLayer(LayerStyle.Normal(), new ContrastEffect(0.5)),
                // .AdjustmentLayer(LayerStyle.Normal(), new ExposureEffect(ExposureMode.Natural, -0.2)),
                new AdjustmentLayer(LayerStyle.Normal(), new VibranceEffect() { Level = 0.3 }),
                new Layer(LayerStyle.Screen(), context => new GradientImageSource(context.BackgroundLayer.ImageSize, gradient))
            };
        }
    }
}
