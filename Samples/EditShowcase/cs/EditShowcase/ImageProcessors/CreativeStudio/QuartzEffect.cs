// ----------------------------------------------------------------------
// Copyright © 2014 Microsoft Mobile. All rights reserved.
// Contact: Magnus Olofstam <ext-magnus.olofstam@microsoft.com>
// 
// This software, including documentation, is protected by copyright controlled by
// Microsoft Mobile. All rights are reserved. Copying, including reproducing, storing,
// adapting or translating, any or all of this material requires the prior written consent of
// Microsoft Mobile. This material also contains confidential information which may not
// be disclosed to others without the prior written consent of Microsoft Mobile.
// ----------------------------------------------------------------------

using Windows.UI;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging.Extras.Effects;
using Lumia.Imaging.Artistic;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Custom;
using Windows.Foundation;
using SDKTestApp_Native;

namespace Lumia.Imaging.ShowCase.ImageProcessors.CreativeStudio
{
	class QuartzEffect : CreativeStudioEffect
	{
		private CachingEffect m_cachingEffect;

		public QuartzEffect()
        {
			m_cachingEffect = new CachingEffect();

			var curve = new Curve();
			curve.SetPoint(16, 7);
			curve.SetPoint(39, 24);
			curve.SetPoint(208, 223);
			curve.SetPoint(234, 247);

			var positiveBrightnessCurve = new Curve(CurveInterpolation.Linear, new[]
			{
				new Point(48, 124),
				new Point(64, 162),
				new Point(80, 193),
				new Point(96, 214),
				new Point(112, 229),
				new Point(128, 239),
				new Point(144, 245),
				new Point(160, 249),
				new Point(192, 253),
				new Point(240, 255)
			});

			var negativeBrightnessCurve = new Curve(CurveInterpolation.Linear, new[]
			{
				new Point(160, 63),
				new Point(192, 79),
				new Point(208, 90),
				new Point(224, 106),
				new Point(236, 123),
				new Point(244, 141),
				new Point(248, 156),
				new Point(252, 183)
			});

			var brightnessCurve = Curve.Interpolate(new Curve(), positiveBrightnessCurve, 0.1, null);
			
			var clarityList = new LayerList(
				new AdjustmentLayer(LayerStyle.Normal(), m_cachingEffect),
				new AdjustmentLayer(LayerStyle.Softlight(), new SDKTestApp_Native.HighpassEffect(100, true, 8))
			);
			
			LayerList.AddRange(
				new AdjustmentLayer(LayerStyle.Normal(), new CurvesEffect(curve)),
				new AdjustmentLayer(LayerStyle.Normal(), new CurvesEffect(brightnessCurve)),

				// Clarity
                new Layer(LayerStyle.Darken(), context => clarityList.ToImageProvider(context.BackgroundImage, context.HintedRenderSize)),

				new AdjustmentLayer(LayerStyle.Normal(), new HueSaturationEffect(0, 0.65-1)),
				new AdjustmentLayer(LayerStyle.Normal(), new VibranceEffect() { Level = 0.1 }),
				new AdjustmentLayer(LayerStyle.Normal(), new SDKTestApp_Native.SplitToneEffect() { HighlightsSaturation = 42, HighlightsHue = 45 })
			);
        }

		protected override Extras.MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Size sourceSize, Size renderSize)
		{
			m_cachingEffect.Invalidate();
			return base.GetEffectInternalAsync(source, sourceSize, renderSize);
		}
	}
}
