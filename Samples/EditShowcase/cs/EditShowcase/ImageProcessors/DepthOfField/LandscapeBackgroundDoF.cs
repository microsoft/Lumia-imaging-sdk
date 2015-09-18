using System;
using Windows.UI;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Media.Imaging;
using Lumia.Imaging;
using Windows.Storage;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Lumia.Imaging.Extras;
using Lumia.Imaging.Extras.Effects;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Compositing;
using Lumia.Imaging.Extras.Effects.DepthOfField;
using Lumia.Imaging.EditShowcase.ViewModels.Editors;
using Lumia.Imaging.EditShowcase.Editors;


namespace Lumia.Imaging.EditShowcase.ImageProcessors.DepthOfField
{
    class LandscapeBackgroundDoF : EffectProcessor
    {
        private FocusObjectDepthOfFieldEffect m_effectEffect;
        private InteractiveForegroundSegmenter m_segmenter;
        private GradientImageSource m_scribbles;
        private Color foreground = Colors.Red;
        private Color background = Colors.Green;

        public LandscapeBackgroundDoF()
        {
            Name = "LandscapeBackgroundDoF";

            var radialGradient = new RadialGradient(new Point(0.5, 0.3), new EllipseRadius(0.2, 0.4)) 
                { Stops = new [] { 
                            new GradientStop() {Color = foreground, Offset = 0}, 
                            new GradientStop() {Color = foreground, Offset = 0.5}, 
                            new GradientStop() {Color = background, Offset = 0.51}, 
                            new GradientStop() {Color = background, Offset = 0.9} 
                    } 
                };

            m_scribbles = new GradientImageSource(new Size(500, 500), radialGradient);

            //Editors.Add(new EnumEditorViewModel<LandscapeBackgroundDoF, DepthOfFieldQuality>("Quality", this, (effect) =>
            //{
            //    if (effect.m_effectEffect == null) return DepthOfFieldQuality.Preview;
            //    return effect.m_effectEffect.Quality;
            //}, (effect, value) =>
            //{
            //    if (effect.m_effectEffect == null)
            //        return;
            //    effect.m_effectEffect.Quality = value;
            //}));

            Editors.Add(new RangeEditorViewModelEx<LandscapeBackgroundDoF>("Strength above horizon", 0, 1.0, this, (effect) =>
            {
                if (effect.m_effectEffect == null) return 1.0;
                return effect.m_effectEffect.StrengthAboveHorizon;
            }, (effect, value) =>
            {
                if (effect.m_effectEffect == null)
                    return;
                effect.m_effectEffect.StrengthAboveHorizon = value;
            }));

            Editors.Add(new RangeEditorViewModelEx<LandscapeBackgroundDoF>("Strength below horizon", 0, 1.0, this, (effect) =>
            {
                if (effect.m_effectEffect == null) return 1.0;
                return effect.m_effectEffect.StrengthBelowHorizon;
            }, (effect, value) =>
            {
                if (effect.m_effectEffect == null)
                    return;
                effect.m_effectEffect.StrengthBelowHorizon = value;
            }));
        }

        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Windows.Foundation.Size sourceSize, Windows.Foundation.Size renderSize)
        {
            if (m_effectEffect == null)
            {
                m_segmenter = new InteractiveForegroundSegmenter(source, foreground, background, m_scribbles);
                m_segmenter.Quality = 0.5;

                m_effectEffect = new FocusObjectDepthOfFieldEffect(source, m_segmenter, new Point(0.0, 0.7), new Point(1.0, 0.75), 1.0, 1.0, DepthOfFieldQuality.Preview);
            }
            else if (m_effectEffect.Source != source)
            {
                m_effectEffect.Source = source;
            }

            return new MaybeTask<IImageProvider>(m_effectEffect);
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}
