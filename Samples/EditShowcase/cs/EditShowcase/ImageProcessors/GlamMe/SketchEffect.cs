using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Artistic;
using Lumia.Imaging.Extras.Layers;

namespace Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe
{

    class SketchEffect : GlamMeEffect
    {
        public SketchEffect()
        {
            LayerList.AddRange(new Layer[]
            {
                new AdjustmentLayer(LayerStyle.Normal(0.64), new GrayscaleEffect(0.23, 0.60, 0.33, -0.1)),
                new AdjustmentLayer(LayerStyle.Multiply(), new Lumia.Imaging.Artistic.SketchEffect(SketchMode.Gray))
            });
        }
    }
}
