using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Media.Imaging;
using Lumia.Imaging;
using Windows.Storage;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Lumia.Imaging.Extras.Effects;
using Lumia.Imaging.Extras.Effects.DepthOfField;
using Lumia.Imaging.EditShowcase.ImageProcessors;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Extras;
using Lumia.Imaging.EditShowcase.ViewModels.Editors;
using Lumia.Imaging.EditShowcase.Editors;


namespace Lumia.Imaging.EditShowcase.ImageProcessors.DepthOfField
{
    class BlockTiltDoF : EffectProcessor
    {
        private LensTiltDepthOfFieldEffect m_effectEffect;
        private FocusBand m_focus;

        public BlockTiltDoF()
        {
            Name = "BlockTiltDoF";

            //Editors.Add(new EnumEditorViewModel<BlockTiltDoF, DepthOfFieldQuality>("Quality", this, (effect) =>
            //{
            //    if (effect.m_effectEffect == null) return DepthOfFieldQuality.Preview;
            //    return effect.m_effectEffect.Quality;
            //}, (effect, value) =>
            //{
            //    if (effect.m_effectEffect == null)
            //        return;
            //    effect.m_effectEffect.Quality = value;
            //}));

            Editors.Add(new RangeEditorViewModelEx<BlockTiltDoF>("Strength at edge 1", 0, 1.0, this, (effect) =>
            {
                if (effect.m_effectEffect == null) return 1.0;
                return effect.m_effectEffect.StrengthAtEdge1;
            }, (effect, value) =>
            {
                if (effect.m_effectEffect == null)
                    return;
                effect.m_effectEffect.StrengthAtEdge1 = value;
            }));

            Editors.Add(new RangeEditorViewModelEx<BlockTiltDoF>("Strength at edge 2", 0, 1.0, this, (effect) =>
            {
                if (effect.m_effectEffect == null) return 1.0;
                return effect.m_effectEffect.StrengthAtEdge2;
            }, (effect, value) =>
            {
                if (effect.m_effectEffect == null)
                    return;
                effect.m_effectEffect.StrengthAtEdge2 = value;
            }));

        }
        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Windows.Foundation.Size sourceSize, Windows.Foundation.Size renderSize)
        {
            if (m_effectEffect == null)
            {
                m_focus = new FocusBand(new Point(0.5, 0.3), new Point(0.5, 0.4));

                m_effectEffect = new LensTiltDepthOfFieldEffect(source, m_focus, 1.0, 1.0, DepthOfFieldQuality.Preview);

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
