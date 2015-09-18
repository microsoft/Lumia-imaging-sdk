using Lumia.Imaging;
using Lumia.Imaging.Extras.Layers;
using System.Threading.Tasks;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe
{
    public class ReflectiveGlassEffect : GlamMeEffect
    {
        public ReflectiveGlassEffect()
        {
            var overlayFactory = new OverlayFactory(
                "ms-appx:///images/Filters_Landscape_Overlay_Reflect.jpg",
                "ms-appx:///images/Filters_Portrait_Overlay_Reflect.jpg",
                "ms-appx:///images/Filters_Square_Overlay_Reflect.jpg")
            {
                HorizontalAlignment = HorizontalAlignment.None
            };

            LayerList.AddRange(
                new Layer(LayerStyle.Screen(), context => overlayFactory.CreateAsync(context.BackgroundLayer.ImageSize))            
            );
        }
    }
}
