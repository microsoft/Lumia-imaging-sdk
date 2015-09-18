using Lumia.Imaging;
using Lumia.Imaging.Extras.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe;
using Lumia.Imaging.Adjustments;

namespace Lumia.Imaging.EditShowcase.ImageProcessors
{

    class FreshEffect: GlamMeEffect
    {
        public FreshEffect()
        {
            var globalCurve = new Curve(CurveInterpolation.NaturalCubicSpline)
            {
                Points = new[]
                {
                 new Point(0, 0),
                    new Point(167, 195),                    
                    new Point(255, 255),            
                }
            };

            var redCurve = new Curve(CurveInterpolation.NaturalCubicSpline)
            {
                Points = new[]
                {
                    new Point(0, 19),
                    new Point(147, 116),
                    new Point(184, 184),
                    new Point(255, 255)                
                }
            };

            var greenCurve = new Curve(CurveInterpolation.NaturalCubicSpline)
            {
                Points = new[]
                {
                    new Point(0, 0),
                    new Point(142, 133),
                    new Point(183, 183),
                    new Point(255, 255)         
                }
            };

            var blueCurve = new Curve(CurveInterpolation.NaturalCubicSpline)
            {
                Points = new[]
                {
                    new Point(0, 0),
                    new Point(155, 130),
                    new Point(205, 181),
                    new Point(255, 255)      
                }
            };
            
            LayerList = new LayerList()
            {
                new AdjustmentLayer(LayerStyle.Normal(), new ContrastEffect(0.2)),
                new AdjustmentLayer(LayerStyle.Normal(), new CurvesEffect(globalCurve)),
                new AdjustmentLayer(LayerStyle.Darken(), new CurvesEffect(redCurve, greenCurve, blueCurve))
            };
        }
    }
}
