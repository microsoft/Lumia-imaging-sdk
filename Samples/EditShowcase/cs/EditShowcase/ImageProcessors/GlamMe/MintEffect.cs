using Lumia.Imaging;
using Lumia.Imaging.Extras.Layers;
using System.Threading.Tasks;
using Windows.Foundation;
using Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe;
using Lumia.Imaging.Adjustments;


namespace Lumia.Imaging.EditShowcase.ImageProcessors
{
    public class MintEffect : GlamMeEffect
    {
        public MintEffect()
        {
            var globalCurve = new Curve(CurveInterpolation.NaturalCubicSpline)
            {
                Points = new[]
                {
                    new Point(167, 195),                    
               }
            };
            
            var redCurve = new Curve(CurveInterpolation.NaturalCubicSpline)
            {
                Points = new[]
                {
                new Point(0, 0),
                new Point(106, 50),
                new Point(160, 163),                
                new Point(255, 255)
                }
            };

            var greenCurve = new Curve(CurveInterpolation.NaturalCubicSpline)
            {
                Points = new[]
                {
                new Point(0, 0),
                new Point(74, 96),                
                new Point(255, 255)
                }
            };

            var blueCurve = new Curve(CurveInterpolation.NaturalCubicSpline)
            {
                Points = new[]
                {
                new Point(0, 0),
                new Point(49, 84),
                new Point(205, 168),                
                new Point(255, 255)
                }
            };

            LayerList = new LayerList()
            {
                new AdjustmentLayer(LayerStyle.Normal(), new ContrastEffect(0.2)),
                new AdjustmentLayer(LayerStyle.Normal(), new CurvesEffect(globalCurve)),
                new AdjustmentLayer(LayerStyle.Normal(0.5), new CurvesEffect(redCurve, greenCurve, blueCurve))
            };
        }
    }
}
