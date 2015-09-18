using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Extras.Layers;
using Windows.Foundation;
using Windows.UI;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe
{
    public class OldPosterEffect: GlamMeEffect
    {
        public OldPosterEffect()
        {
            // Curves
            var globalCurve = new Curve(CurveInterpolation.NaturalCubicSpline)
            {
                Points = new[]
                {
                    new Point(0, 0),                  
                    new Point(38, 17),
                    new Point(160, 210),                    
                    new Point(231, 250),
                    new Point(255, 255)                   
                }
            };

            var overlayFactory = new OverlayFactory(
                "ms-appx:///images/Filters_Landscape_Overlay_Poster2.png",
                "ms-appx:///images/Filters_Portrait_Overlay_Poster2.png",
                "ms-appx:///images/Filters_Square_Overlay_Poster2.png");

            LayerList = new LayerList(
                new AdjustmentLayer(LayerStyle.Normal(), new GrayscaleEffect(0.54, 0.64, 0.0, -0.1)),
                new AdjustmentLayer(LayerStyle.Normal(), new CurvesEffect(globalCurve)),
                new Layer(LayerStyle.Multiply(), context => new ColorImageSource(context.BackgroundLayer.ImageSize, Color.FromArgb(255, 255, 167, 151))),
                new Layer(LayerStyle.Normal(), context => overlayFactory.CreateAsync(context.BackgroundLayer.ImageSize))
            );
        }
    }
}
