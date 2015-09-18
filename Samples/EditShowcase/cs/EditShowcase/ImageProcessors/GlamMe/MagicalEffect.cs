using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Extras;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe
{
    public class MagicalEffect : GlamMeEffect
    {
        public MagicalEffect()
        {
        }

        public override string ToString()
        {
            return "Magical";
        }

        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Size sourceSize, Size renderSize)
        {
            var current = source;

           var gradientImageSource =  new GradientImageSource(sourceSize, new RadialGradient(new Point(0.5, 0.3), new EllipseRadius(1.0, 0),
             new[] { 
                            new GradientStop() { Color = Color.FromArgb(255, 255, 255, 255), Offset = 0 }, 
                            new GradientStop() { Color = Color.FromArgb(255, 255, 255, 255), Offset = 0.1 }, 
                            new GradientStop() { Color = Color.FromArgb(255, 0, 0, 0), Offset = 0.101 }, 
                            new GradientStop() { Color = Color.FromArgb(255, 16, 16, 16), Offset = 0.8 }, 
                     }
                 ));

            var lensblur = new LensBlurEffect(source, gradientImageSource,
                new ILensBlurKernel[] 
                { 
                    new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Hexagon, 1), 
                    new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Hexagon, 2), 
                    new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Hexagon, 3), 
                    new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Hexagon, 4), 
                    new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Hexagon, 5), 
                    new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Hexagon, 6), 
                    new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Hexagon, 7), 
                    new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Hexagon, 8),
                    new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Hexagon, 9),                 
                    new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Hexagon, 10),
                    new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Hexagon, 11),
                    new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Hexagon, 12),
                    new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Hexagon, 13),
                    new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Hexagon, 14), 
                    new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Hexagon, 15), 
                    new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Hexagon, 20), 
                    new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Hexagon, 25), 
                    
                });

            lensblur.BlendKernelWidth = 10;
            lensblur.FocusAreaEdgeMirroring = LensBlurFocusAreaEdgeMirroring.Off;
            // TODO lower quality in preview mode.
            lensblur.Quality = 1.0;

            return new MaybeTask<IImageProvider>(lensblur);
        }
    }
}
