using Windows.Foundation;
using Lumia.Imaging.Extras.Effects;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using SDKTestApp_Native;

namespace Lumia.Imaging.ShowCase.ImageProcessors.GlamMe
{
    class BoldEffect : GlamMeEffect
    {
        public BoldEffect()
        {   
            var curvesEffect = new CurvesEffect(new Curve(CurveInterpolation.NaturalCubicSpline)
            {
                Points = new[]
                {                   
                    new Point(0, 0),
                    new Point(69, 66),
                    new Point(212, 218),                    
                    new Point(255, 255)
                }
            });

            LayerList = new LayerList()
            {
                new AdjustmentLayer(LayerStyle.Normal(), curvesEffect),
               new AdjustmentLayer(LayerStyle.Hardlight(), new SDKTestApp_Native.HighpassEffect(8,false,0)),
                new AdjustmentLayer(LayerStyle.Normal(0.2), new HueSaturationEffect(-0.4, -1.0))
            };
        }

        public override string ToString()
        {
            return "Bold";
        }
    }
}
