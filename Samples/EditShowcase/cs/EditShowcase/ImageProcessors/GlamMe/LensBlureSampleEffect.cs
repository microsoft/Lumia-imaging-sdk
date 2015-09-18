using Windows.Foundation;
using Lumia.Imaging;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Extras;

namespace Lumia.Imaging.EditShowcase.ImageProcessors
{
    class LensBlureSampleEffect : GlamMeEffect
    {
        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Size sourceSize, Size renderSize)
        {
            return new MaybeTask<IImageProvider>(GetEffectTaskAsync(source, sourceSize, renderSize));
        }

        private async Task<IImageProvider> GetEffectTaskAsync(IImageProvider source, Size sourceSize, Size renderSize)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///images/LensBlurMask.jpg"));
            StorageFileImageSource mapSource = new StorageFileImageSource(file);
            var lensBlurEffect = new LensBlurEffect(source, mapSource);
            lensBlurEffect.FocusAreaEdgeMirroring= LensBlurFocusAreaEdgeMirroring.Off;
            lensBlurEffect.PointLightStrength = 10;
            lensBlurEffect.Kernels = new LensBlurPredefinedKernel[] { new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Heart, 15)};
            return lensBlurEffect;
        }
    }
}

