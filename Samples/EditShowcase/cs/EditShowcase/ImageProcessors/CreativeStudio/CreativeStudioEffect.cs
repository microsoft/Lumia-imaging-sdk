// ----------------------------------------------------------------------
// Copyright © 2014 Microsoft Mobile. All rights reserved.
// Contact: Sergiy Dubovik <sergiy.dubovik@microsoft.com>
// 
// This software, including documentation, is protected by copyright controlled by
// Microsoft Mobile. All rights are reserved. Copying, including reproducing, storing,
// adapting or translating, any or all of this material requires the prior written consent of
// Microsoft Mobile. This material also contains confidential information which may not
// be disclosed to others without the prior written consent of Microsoft Mobile.
// ----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Lumia.Imaging;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging.EditShowcase.ImageProcessors;
using Lumia.Imaging.EditShowcase.Utilities;
using Lumia.Imaging.Extras;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.CreativeStudio
{
    public abstract class CreativeStudioEffect : EffectProcessor
    {
        private LayerList m_layerList;

        protected LayerList LayerList
        {
            get { return m_layerList; }
            private set
            {
                m_layerList = value;
            }
        }

        protected CreativeStudioEffect()
        {
            CanRenderAtPreviewSize = false;
            Lookups.Preload();
            LayerList = new LayerList();
            EffectCategory = EffectCategoryEnum.CreativeStudio;
        }

        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Size sourceSize, Size renderSize)
        {
            return LayerList.ToImageProvider(source, sourceSize, renderSize); 
        }

        protected override void Dispose(bool disposing)
        {
            DisposableHelper.TryDisposeAndSetToNull(ref m_layerList);
        }

    }
}
