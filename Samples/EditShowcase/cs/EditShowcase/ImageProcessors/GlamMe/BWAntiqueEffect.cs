using Windows.Foundation;
using Windows.UI;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe
{
    public class BwAntiqueEffect : GlamMeEffect
    {
        public BwAntiqueEffect()
        {
            
            var globalCurvesEffect = new CurvesEffect(new Curve(CurveInterpolation.NaturalCubicSpline)
            {
                Points = new[]
                {
                    new Point(0, 0),
                    new Point(58, 73),
                    new Point(212, 218),                    
                    new Point(255, 255)                
                }
            });

            LayerList.AddRange(new []
            {
                new AdjustmentLayer(LayerStyle.Normal(), new GrayscaleEffect(0.54, 0.64, 0.0, -0.1)),
                new AdjustmentLayer(LayerStyle.Normal(), globalCurvesEffect),
                new Layer(LayerStyle.Hardlight(0.4), context => new ColorImageSource(context.BackgroundLayer.ImageSize, Color.FromArgb(255, 166, 124, 82)))
            });
        }

        public override string ToString()
        {
            return "BW Antique";
        }
    }
}
