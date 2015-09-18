using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using System.Threading;
using System.Threading.Tasks;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.CreativeStudio
{
    class Lookups
    {
        private static int m_hasPreloaded;

        public static void Preload()
        {
            if (Interlocked.Exchange(ref m_hasPreloaded, 1) == 1)
                return;

            CopperCurvesEffect = new LookupCurves("Images\\coppertable.bmp").GetEffectAsync();
            ColdCurvesEffect = new LookupCurves("Images\\cold_table.bmp").GetEffectAsync();
            ColdVignette = new LookupImage("Images\\cold_vignette.png");
            IndoorCurvesEffect = new LookupCurves("Images\\indoor_table.bmp").GetEffectAsync();
            LoSatWarmCurvesEffect = new LookupCurves("Images\\losat_table.bmp").GetEffectAsync();
            LoSatWarmVignette = new LookupImage("Images\\losat_vignette.jpg");
            NeonCurvesEffect = new LookupCurves("Images\\neon_table.bmp").GetEffectAsync();
            RetrotoneVignette = new LookupImage("Images\\retrotone_vignette_gray.png");
            SunsetVignette = new LookupImage("Images\\sunset_vignette.png");
            VividVignette = new LookupImage("Images\\vivid_vignette2.bmp");
            VividCurvesEffect = new LookupCurves("Images\\vivid_table.bmp", gain: 1.03).GetEffectAsync();
        }

        public static Task<IImageProvider> CopperCurvesEffect { get; private set; }
        public static Task<IImageProvider> ColdCurvesEffect { get; private set; }
        public static LookupImage ColdVignette { get; private set; }
        public static Task<IImageProvider> IndoorCurvesEffect { get; private set; }
        public static Task<IImageProvider> LoSatWarmCurvesEffect { get; private set; }
        public static LookupImage LoSatWarmVignette { get; private set; }
        public static Task<IImageProvider> NeonCurvesEffect { get; private set; }
        public static LookupImage RetrotoneVignette { get; private set; }
        public static LookupImage SunsetVignette { get; private set; }
        public static LookupImage VividVignette { get; private set; }
        public static Task<IImageProvider> VividCurvesEffect { get; private set; }
    }
}
