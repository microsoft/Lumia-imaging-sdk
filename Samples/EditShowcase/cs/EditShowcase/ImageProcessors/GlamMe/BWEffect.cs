using Windows.Foundation;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging.EditShowcase.Utilities;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Artistic;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe
{
    class BWEffect : GlamMeEffect
    {
        private LayerList m_lomoStack;

        public BWEffect()
        {
            var curvesEffect = new CurvesEffect(new Curve(CurveInterpolation.NaturalCubicSpline)
            {
                Points = new[]
                {                   
                    new Point(0,0),
                    new Point(41, 59),
                    new Point(112, 146),
                    new Point(189, 211),
                    new Point(255, 255),
                }
            });

            m_lomoStack = new LayerList()
            {
                new AdjustmentLayer(LayerStyle.Normal(), new LomoEffect(0.3, 0.0, LomoVignetting.High, LomoStyle.Neutral))
            };

            LayerList = new LayerList()
            {
                new AdjustmentLayer(LayerStyle.Normal(), curvesEffect),
                new AdjustmentLayer(LayerStyle.Normal(), new GrayscaleEffect()),
                new AdjustmentLayer(LayerStyle.Normal(), new SharpnessEffect(0.15)),
                new Layer(LayerStyle.Hardlight(0.1), context => m_lomoStack.ToImageProvider(context.BackgroundImage, context.HintedRenderSize))
            };
        }

        public override string ToString()
        {
            return "BW";
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DisposableHelper.TryDisposeAndSetToNull(ref m_lomoStack);
        }
    }
}
