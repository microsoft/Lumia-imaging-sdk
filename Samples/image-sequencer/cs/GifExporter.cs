using Lumia.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using Lumia.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Storage;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Lumia.Imaging.Compositing;
using Lumia.Imaging.Transforms;

namespace ImageSequencer
{
    class GifExporter
    {

        public static async Task Export(IReadOnlyList<IImageProvider> images, Rect? animatedArea)
        {
            // List of aligned images may contain Null items if the particular image couldn't be aligned
            List<IImageProvider> sanitizedImages = new List<IImageProvider>();
            foreach (IImageProvider image in images) {
                if (image != null) {
                    sanitizedImages.Add(image);
                }
            }

            IImageResource info = null;
            IAsyncImageResource asyncImageResource = sanitizedImages[0] as IAsyncImageResource;
            if (asyncImageResource == null)
            {
                info = sanitizedImages[0] as IImageResource;
            }
         
            int w = (int)info.ImageSize.Width;
            int h = (int)info.ImageSize.Height;

            IReadOnlyList<IImageProvider> gifRendererSources;
            if (animatedArea.HasValue)
            {
                // Ensure the animated area dimensions are smaller than the image dimensions
                double rectW = animatedArea.Value.Width;
                double rectH = animatedArea.Value.Height;
                if ((animatedArea.Value.Width + animatedArea.Value.Left) >= w)
                {
                    rectW = w - animatedArea.Value.Left - 1;
                }
                if ((animatedArea.Value.Top + animatedArea.Value.Height) >= h)
                {
                    rectH = h - animatedArea.Value.Top - 1;
                }

                Rect rect = new Rect(animatedArea.Value.Left, animatedArea.Value.Top, rectW, rectH);
                gifRendererSources = CreateFramedAnimation(sanitizedImages, rect, w, h);
            }
            else
            {
                gifRendererSources = sanitizedImages;
            }

            using (GifRenderer gifRenderer = new GifRenderer())
            {
                gifRenderer.Duration = 100;
                gifRenderer.NumberOfAnimationLoops = 10000;
                gifRenderer.Sources = gifRendererSources;

                var buffer = await gifRenderer.RenderAsync();

                var filename = "Sequence" + (await GetFileNameRunningNumber()) + ".gif";
                var storageFile = await KnownFolders.SavedPictures.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                using (var stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await stream.WriteAsync(buffer);
                }
            }
        }

        
        private static IReadOnlyList<IImageProvider> CreateFramedAnimation(IReadOnlyList<IImageProvider> images, Rect animationBounds, int w, int h)
        {
            List<IImageProvider> framedAnimation = new List<IImageProvider>();

            foreach (IImageProvider frame in images)
            {
                BlendEffect blendEffect = new BlendEffect();
                blendEffect.ForegroundSource = new CropEffect(animationBounds);
                blendEffect.GlobalAlpha = 1.0;
                blendEffect.BlendFunction = BlendFunction.Normal;
                blendEffect.TargetArea = new Rect(
                    animationBounds.Left / w,
                    animationBounds.Top / h,
                    animationBounds.Width / w,
                    animationBounds.Height / h
                );            

                framedAnimation.Add(blendEffect);
            }

            return framedAnimation;
        }

        private static async Task<int> GetFileNameRunningNumber()
        {
            var files = await KnownFolders.SavedPictures.GetFilesAsync();
            int max = 0;
            foreach (StorageFile storageFile in files)
            {
                var pattern = "Sequence\\d+\\.gif";
                if (System.Text.RegularExpressions.Regex.IsMatch(storageFile.Name, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    max = Math.Max(max, Convert.ToInt32(storageFile.Name.Split('.')[0].Substring(8)));
                }
            }

            return max + 1;
        }

    }
}
