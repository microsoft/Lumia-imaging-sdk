using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Extras.Layers;
using Windows.Foundation;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe
{
    public class FantasticEffect : GlamMeEffect
    {
        public FantasticEffect()
        {
            var globalCurve = new Curve(CurveInterpolation.NaturalCubicSpline)
            {
                Points = new[]
                {
                    new Point(0, 0),
                    new Point(68, 60),                    
                    new Point(144, 163),                    
                    new Point(255, 255),            
                }
            };

            var overlayFactory1 = new OverlayFactory(
                "ms-appx:///images/Filters_Landscape_Overlay_Fantastic1.jpg",
                "ms-appx:///images/Filters_Portrait_Overlay_Fantastic1.jpg",
                "ms-appx:///images/Filters_Square_Overlay_Fantastic1.jpg");

            var overlayFactory2 = new OverlayFactory(
                "ms-appx:///images/Filters_Landscape_Overlay_Fantastic2.png",
                "ms-appx:///images/Filters_Portrait_Overlay_Fantastic2.png",
                "ms-appx:///images/Filters_Square_Overlay_Fantastic2.png");
            
            LayerList = new LayerList()
            {
                new AdjustmentLayer(LayerStyle.Normal(), new ContrastEffect(1.0)),
                new AdjustmentLayer(LayerStyle.Normal(), new CurvesEffect(globalCurve)),
                new AdjustmentLayer(LayerStyle.Normal(), new GrayscaleEffect(0.54, 0.64, 0.0, -0.1)),
                new Layer(LayerStyle.Softlight(), context => overlayFactory1.CreateAsync(context.BackgroundLayer.ImageSize)),
                new Layer(LayerStyle.Normal(), context => overlayFactory2.CreateAsync(context.BackgroundLayer.ImageSize))
            };
        }
    }
}
