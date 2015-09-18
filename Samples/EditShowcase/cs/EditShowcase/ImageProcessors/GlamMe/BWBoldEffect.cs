using Windows.Foundation;
using Lumia.Imaging.Extras.Effects;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using SDKTestApp_Native;

namespace Lumia.Imaging.ShowCase.ImageProcessors.GlamMe
{
    public class BWBoldEffect : GlamMeEffect
    {
        public BWBoldEffect()
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
                new AdjustmentLayer(LayerStyle.Normal(), new GrayscaleEffect()),
                new AdjustmentLayer(LayerStyle.Normal(), curvesEffect),
                new AdjustmentLayer(LayerStyle.Normal(), new SharpnessEffect(0.1)),
                new AdjustmentLayer(LayerStyle.Hardlight(), new SDKTestApp_Native.HighpassEffect(15, false, 2))
            };
        }

        public override string ToString()
        {
            return "BW Bold";
        }
    }
}
