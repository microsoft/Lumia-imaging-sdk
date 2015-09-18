using Windows.Foundation;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Artistic;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe
{

    class VintageEffect : GlamMeEffect
    {
        public VintageEffect()
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

            LayerList = new LayerList()
            {
                new AdjustmentLayer(LayerStyle.Normal(),  new CurvesEffect(globalCurve)),
                new AdjustmentLayer(LayerStyle.Normal(0.8), new AntiqueEffect()),
                new AdjustmentLayer(LayerStyle.Softlight(0.4), new LevelsEffect(145 / 255.0, (145 + 80) / 2.0 / 255.0, 80 / 255.0))
            };
        }

    }
}
