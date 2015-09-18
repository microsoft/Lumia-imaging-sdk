using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Extras.Effects;
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
    public class OpalEffect : CreativeStudioEffect
    {
        private SelectiveColorEffect m_selectiveColorEffect;
		private CachingEffect m_cachingEffect;

        public OpalEffect()
        {
			m_cachingEffect = new CachingEffect();
            var clarityList = new LayerList(
				new AdjustmentLayer(LayerStyle.Normal(), m_cachingEffect),
                new AdjustmentLayer(LayerStyle.Softlight(), new SDKTestApp_Native.HighpassEffect(100, true, 8))
            );

            m_selectiveColorEffect = new SelectiveColorEffect
            {                
                Red = new CMYK {Yellow = 5},
                Yellow = new CMYK {Yellow = 40, Key = 15},
                Green = new CMYK {Cyan = 20, Yellow = 20, Key = 30},
                Cyan = new CMYK {Key = 61},
                Blue = new CMYK {Key = 30}
            };

            LayerList.AddRange(

                // Clarity
                new Layer(LayerStyle.Darken(0.5), context => clarityList.ToImageProvider(context.BackgroundImage, context.HintedRenderSize)),
                new AdjustmentLayer(LayerStyle.Normal(), m_selectiveColorEffect),

                new AdjustmentLayer(LayerStyle.Normal(), new HueSaturationEffect(0.0, 0.06)),
                new AdjustmentLayer(LayerStyle.Normal(), new VibranceEffect() { Level = 0.1 }),

                new Layer(LayerStyle.Overlay(), context => new ColorImageSource(context.HintedRenderSize, Color.FromArgb(0xFF, 0xFF, 0xE6, 0x99)))
            );

            /*            Editors.Add(new RangeEditorViewModel<OpalEffect>("SaturationLevel", -1.0, 1.0, this, filter => filter.SaturationLevel, (filter, value) => filter.SaturationLevel = value));
                        //    Editors.Add(new RangeEditorViewModel<OpalEffect>("ContrastLevel", -1.0, 1.0, this, filter => filter.ContrastLevel, (filter, value) => filter.ContrastLevel = value));
                        Editors.Add(new RangeEditorViewModel<OpalEffect>("VibranceLevel", 0, 1.0, this, filter => filter.VibranceLevel, (filter, value) => filter.VibranceLevel = value));
                        Editors.Add(new RangeEditorViewModel<OpalEffect>("ShadowsHue", 0, 365, this, filter => filter.ShadowsHue, (filter, value) => filter.ShadowsHue = (int)value));
                        Editors.Add(new RangeEditorViewModel<OpalEffect>("ShadowsSaturation", 0, 100, this, filter => filter.ShadowsSaturation, (filter, value) => filter.ShadowsSaturation = (int)value));
                        Editors.Add(new RangeEditorViewModel<OpalEffect>("HighlightsHue", 0, 365, this, filter => filter.HighlightsHue, (filter, value) => filter.HighlightsHue = (int)value));
                        Editors.Add(new RangeEditorViewModel<OpalEffect>("HighlightsSaturation", 0, 100, this, filter => filter.HighlightsSaturation, (filter, value) => filter.HighlightsSaturation = (int)value));
              */
        }

        /*        public double SaturationLevel
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
                }*/

		protected override Extras.MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Size sourceSize, Size renderSize)
		{
			m_cachingEffect.Invalidate();
			return base.GetEffectInternalAsync(source, sourceSize, renderSize);
		}
    }
}
