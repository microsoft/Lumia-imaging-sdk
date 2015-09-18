using Windows.Foundation;
using Windows.UI;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe
{
    class DreamEffect : GlamMeEffect
    {
        public DreamEffect()
        {
            Name = "Dream";

            var diagonalGradient = new LinearGradient
            {
                StartPoint = new Point(0.0, 0.0),
                EndPoint = new Point(1.0, 1.0),

                Stops = new[]
                {
                    new GradientStop {Offset = 0, Color = Color.FromArgb(255, 133, 147, 201)},
                    new GradientStop {Offset = 1.0, Color = Color.FromArgb(255, 245, 152, 157)}
                }
            };

            var overlayFactory = new OverlayFactory(
                "ms-appx:///images/Filters_Landscape_Overlay_Dream.jpg", 
                "ms-appx:///images/Filters_Portrait_Overlay_Dream.jpg", 
                "ms-appx:///images/Filters_Square_Overlay_Dream.jpg");

            LayerList = new LayerList(
				new AdjustmentLayer(LayerStyle.Normal(), new LevelsEffect(0.75, 0.85, 0.15)),
                new Layer(LayerStyle.Softlight(), context => new GradientImageSource(context.BackgroundLayer.ImageSize, diagonalGradient)),
                new Layer(LayerStyle.Overlay(), context => overlayFactory.CreateAsync(context.BackgroundLayer.ImageSize))
            );
        }
    }
}
