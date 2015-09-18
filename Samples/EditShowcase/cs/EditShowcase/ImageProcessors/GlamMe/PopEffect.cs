using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Extras.Layers;
using Windows.Foundation;
using Windows.UI;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe
{
    public class PopEffect: GlamMeEffect
    {
        public PopEffect()
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

            var overlayFactory = new OverlayFactory(
                "ms-appx:///images/Filters_Landscape_Overlay_Pop3.png",
                "ms-appx:///images/Filters_Portrait_Overlay_Pop3.png",
                "ms-appx:///images/Filters_Square_Overlay_Pop3.png");

            LayerList = new LayerList()
            {
                new AdjustmentLayer(LayerStyle.Normal(), new ContrastEffect(1.0)),
                new AdjustmentLayer(LayerStyle.Normal(),  new CurvesEffect(globalCurve)),
                new AdjustmentLayer(LayerStyle.Normal(), new GrayscaleEffect(0.54, 0.64, 0.0, -0.1)),
                new AdjustmentLayer(LayerStyle.Normal(), new LevelsEffect(0.7, 0.7 / 2, 0)),
                new Layer(LayerStyle.Screen(1.0), context => new ColorImageSource(context.HintedRenderSize, Color.FromArgb(255, 108, 0, 148))),
                new Layer(LayerStyle.Lighten(), context => overlayFactory.CreateAsync(context.HintedRenderSize))
            };
        }

    }
}
