using System;
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
using Lumia.Imaging.Extras.Effects.DepthOfField;
using Lumia.Imaging.EditShowcase.ViewModels.Editors;
using Lumia.Imaging.EditShowcase.Editors;


namespace Lumia.Imaging.EditShowcase.ImageProcessors.DepthOfField
{
    class EllipseTiltDoF : EffectProcessor
    {
        private EllipticFocusDepthOfFieldEffect m_effectEffect;
        private readonly FocusEllipse m_ellipseFocus1;
        private readonly FocusEllipse m_ellipseFocus2;
        private FocusEllipse m_selectedEllipse;
        private bool m_invertedEllipse = false;

        public EllipseTiltDoF()
        {
            Name = "EllipseTiltDoF";

            m_ellipseFocus1 = new FocusEllipse(new Point(0.5, 0.3), new EllipseRadius(0.05, 0.2));
            m_ellipseFocus2 = new FocusEllipse(new Point(0.5, 0.7), new EllipseRadius(0.05, 0.2));
            m_selectedEllipse = m_ellipseFocus1;

            //Editors.Add(new EnumEditorViewModel<EllipseTiltDoF, DepthOfFieldQuality>("Quality", this, (effect) =>
            //{
            //    if (effect.m_effectEffect == null) return DepthOfFieldQuality.Preview;
            //    return effect.m_effectEffect.Quality;
            //}, (effect, value) =>
            //{
            //    if (effect.m_effectEffect == null)
            //        return;
            //    effect.m_effectEffect.Quality = value;
            //}));

            //Editors.Add(new BoolEditorViewModel<EllipseTiltDoF>("InvertedEllipse", this, (effect) =>
            //{
            //    return effect.m_invertedEllipse;
            //}, (effect, value) =>
            //{
            //    effect.m_invertedEllipse = value;
            //    effect.m_selectedEllipse = value ? m_ellipseFocus2 : m_ellipseFocus1;
            //}));

            Editors.Add(new RangeEditorViewModelEx<EllipseTiltDoF>("Strength", 0, 1.0, this, (effect) =>
            {
                if (effect.m_effectEffect == null) return 1.0;
                return effect.m_effectEffect.Strength;
            }, (effect, value) =>
            {
                if (effect.m_effectEffect == null)
                    return;
                effect.m_effectEffect.Strength = value;
            }));
        }

        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Windows.Foundation.Size sourceSize, Windows.Foundation.Size renderSize)
        {
            if (m_effectEffect == null)
            {
                m_effectEffect = new EllipticFocusDepthOfFieldEffect(source, m_ellipseFocus1, 1.0, DepthOfFieldQuality.Preview);
            }
            else
            {
                if (source != m_effectEffect.Source)
                {
                    m_effectEffect.Source = source;
                }

                if (m_effectEffect.FocusArea != m_selectedEllipse)
                {
                    m_effectEffect.FocusArea = m_selectedEllipse;
                }
            }

            return new MaybeTask<IImageProvider>(m_effectEffect);
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}
