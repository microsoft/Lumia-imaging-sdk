using Windows.Foundation;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe
{
    public class ElegantEffect : GlamMeEffect
    {
        public ElegantEffect()
        {
            var globalCurve = new Curve(CurveInterpolation.Linear)
            {
                Points = new[]
                {
                    new Point(0, 0),                                      
                    new Point(52, 78),                                      
                    new Point(168,195),                                      
                    new Point(255, 255)                   
                }
            };

            LayerList = new LayerList()
            {
                new AdjustmentLayer(LayerStyle.Normal(0.6), new HueSaturationEffect(0, -0.3)),
                new AdjustmentLayer(LayerStyle.Normal(0.6), new CurvesEffect(globalCurve))
            };
        }
    }
}
