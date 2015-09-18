using Windows.Foundation;
using Lumia.Imaging.Extras.Layers;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe
{
    public class MagzineStarEffect : GlamMeEffect
    {
        public MagzineStarEffect()
        {
            var overlayFactory = new OverlayFactory(
                "ms-appx:///images/Filters_Landscape_Overlay_Magazine.png",
                "ms-appx:///images/Filters_Portrait_Overlay_Magazine.png",
                "ms-appx:///images/Filters_Square_Overlay_Magazine.png")
            {
                HorizontalAlignment = HorizontalAlignment.None
            };

            LayerList.AddRange(
                new Layer(LayerStyle.Normal(targetArea: new Rect(0.02, 0.02, 0.96, 0.96)), context => overlayFactory.CreateAsync(context.HintedRenderSize))
            );
        }
    }
}
