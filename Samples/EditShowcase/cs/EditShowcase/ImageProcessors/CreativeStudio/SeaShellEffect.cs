using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging.ShowCase.Editors;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;
using Windows.UI;
using SDKTestApp_Native;

namespace Lumia.Imaging.ShowCase.ImageProcessors.CreativeStudio
{
    public class SeaShellEffect : CreativeStudioEffect
    {
        private HueSaturationEffect m_hueSaturationEffect;
        private VibranceEffect m_vibranceEffect;
        private SplitToneEffect m_splitToningEffect;

        public SeaShellEffect()
        {
            m_hueSaturationEffect = new HueSaturationEffect() { Saturation = 0.6 -1 };
            m_vibranceEffect = new VibranceEffect() { Level = .6 };
            m_splitToningEffect = new SplitToneEffect
            {
                ShadowsHue = 230, 
                ShadowsSaturation = 37, 
                HighlightsHue = 50, 
                HighlightsSaturation = 20
            };

            var globalCurve = new Curve(CurveInterpolation.Linear, new[]
            {
                new Point(0, 10),
                new Point(32, 68),
                new Point(64, 119),
                new Point(96, 158),
                new Point(128, 187),
                new Point(160, 209),
                new Point(192, 226),
                new Point(255, 248)                
            });

            var curve = new Curve(CurveInterpolation.Linear, new[]
            {
                new Point(10, 0),
                new Point(32, 27),
                new Point(70, 70)                
            });

            var redCurve = globalCurve;
            var greenCurve = Curve.Compose(curve, globalCurve, null);
            var blueCurve = globalCurve;

            var curvesEffect = new CurvesEffect(redCurve, greenCurve, blueCurve);

            var colorizationLayerList = new LayerList(
					new AdjustmentLayer(LayerStyle.Normal(), new GrayscaleEffect()),
					new Layer(LayerStyle.Multiply(), context => new ColorImageSource(context.HintedRenderSize, Color.FromArgb(0xff, 0xff, 0xe6, 0x99)))
                );

            LayerList.AddRange(
                new AdjustmentLayer(LayerStyle.Normal(0.2), context => colorizationLayerList.ToImageProvider(context.BackgroundImage, context.HintedRenderSize, context.HintedRenderSize)),

                new AdjustmentLayer(LayerStyle.Normal(), new ContrastEffect(-0.15)),
                new AdjustmentLayer(LayerStyle.Normal(), curvesEffect),
                new AdjustmentLayer(LayerStyle.Normal(), m_hueSaturationEffect),
                new AdjustmentLayer(LayerStyle.Normal(), m_vibranceEffect),
                new AdjustmentLayer(LayerStyle.Normal(), m_splitToningEffect)               
           );

            Editors.Add(new RangeEditorViewModel<SeaShellEffect>("SaturationLevel", -1.0, 1.0, this, filter => filter.SaturationLevel, (filter, value) => filter.SaturationLevel = value));
        //    Editors.Add(new RangeEditorViewModel<SeaShellEffect>("ContrastLevel", -1.0, 1.0, this, filter => filter.ContrastLevel, (filter, value) => filter.ContrastLevel = value));
            Editors.Add(new RangeEditorViewModel<SeaShellEffect>("VibranceLevel", 0, 1.0, this, filter => filter.VibranceLevel, (filter, value) => filter.VibranceLevel = value));
			Editors.Add(new RangeEditorViewModel<SeaShellEffect>("ShadowsHue", 0, 365, this, filter => filter.ShadowsHue, (filter, value) => filter.ShadowsHue = (int)value));
			Editors.Add(new RangeEditorViewModel<SeaShellEffect>("ShadowsSaturation", 0, 100, this, filter => filter.ShadowsSaturation, (filter, value) => filter.ShadowsSaturation = (int)value));
			Editors.Add(new RangeEditorViewModel<SeaShellEffect>("HighlightsHue", 0, 365, this, filter => filter.HighlightsHue, (filter, value) => filter.HighlightsHue = (int)value));
			Editors.Add(new RangeEditorViewModel<SeaShellEffect>("HighlightsSaturation", 0, 100, this, filter => filter.HighlightsSaturation, (filter, value) => filter.HighlightsSaturation = (int)value));          
        }

        public double SaturationLevel
        {
            get { return m_hueSaturationEffect.Saturation; }
            set { m_hueSaturationEffect.Saturation = value; }
        }

        public double VibranceLevel
        {
            get { return m_vibranceEffect.Level; }
            set { m_vibranceEffect.Level = value; }
        }

        public int ShadowsHue
        {
            get { return m_splitToningEffect.ShadowsHue; }
            set { m_splitToningEffect.ShadowsHue = value; }
        }

        public int ShadowsSaturation
        {
            get { return m_splitToningEffect.ShadowsSaturation; }
            set { m_splitToningEffect.ShadowsSaturation = value; }
        }

        public int HighlightsHue
        {
            get { return m_splitToningEffect.HighlightsHue; }
            set { m_splitToningEffect.HighlightsHue = value; }
        }

        public int HighlightsSaturation
        {
            get { return m_splitToningEffect.HighlightsSaturation; }
            set { m_splitToningEffect.HighlightsSaturation = value; }
        }
    }
}
