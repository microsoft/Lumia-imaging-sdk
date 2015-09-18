#if NETFX_CORE
#else
using System.Windows.Media;
#endif
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging.EditShowcase.ImageProcessors;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Extras;


namespace Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe
{
    public class GlamMeEffect : ImageProcessor
    {       
        protected LayerList LayerList;

        public GlamMeEffect()
        {
            EffectCategory = EffectCategoryEnum.GlamMe;
            var redCurve = new Curve(CurveInterpolation.Linear);

            var greenCurve = new Curve(CurveInterpolation.Linear)
            {
                Points = new[]
                {
                    new Point(0, 0),
                    new Point(69, 66),
                    new Point(163, 170),
                    new Point(255, 255),
                   
                }
            };

            var blueCurve = new Curve(CurveInterpolation.Linear)
            {
                Points = new[]
                {
                    new Point(0, 0),
                    new Point(76, 72),
                    new Point(156, 161),
                    new Point(255, 255),
                   
                }
            };

            LayerList = new LayerList()
            {
                    new AdjustmentLayer(LayerStyle.Normal(),  new LevelsEffect(232.0 / 255.0, 119.0 / 255.0, 6.0 / 255.0)),     
                    new AdjustmentLayer(LayerStyle.Normal(),  new CurvesEffect(redCurve, greenCurve, blueCurve)),                    
                    new AdjustmentLayer(LayerStyle.Normal(), new VibranceEffect() { Level = 0.13 }),                     
                    new AdjustmentLayer(LayerStyle.Normal(), new ExposureEffect(ExposureMode.Gamma, 1.0 - 0.92)),
                    new AdjustmentLayer(LayerStyle.Normal(), new SharpnessEffect(0.1))
                    
            };
        }

        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Size sourceSize, Size renderSize)
        {
            return LayerList.ToImageProvider(source, sourceSize, renderSize);
        }

        protected void GetSaturationMappedValue(double sat, out int startSat, out int endSat)
        {
            sat = sat *= 255.0 / 100.0; ; // -100 till 100

            startSat = (int)sat;
            endSat = (int)(255.0 + sat);
            startSat = Math.Min(255, Math.Max(0, startSat));
            endSat = Math.Min(255, Math.Max(0, endSat));
        }

        protected override void Dispose(bool disposing)
        {
           
        }
    }
}
