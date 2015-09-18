/*
* Copyright (c) 2014 Microsoft Mobile
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Lumia.Imaging.Extras;
using Lumia.Imaging.EditShowcase.ImageProcessors.CreativeStudio;
using Lumia.Imaging.EditShowcase.ImageProcessors.GlamMe;
using Lumia.Imaging.EditShowcase.ImageProcessors;
using Lumia.Imaging.EditShowcase.Interfaces;
using Lumia.Imaging.EditShowcase.ViewModels.Editors;
using Lumia.Imaging.EditShowcase.Editors;
using System.Collections.Generic;

using Lumia.Imaging;
using Lumia.Imaging.Artistic;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Compositing;
using Lumia.Imaging.Transforms;
using Lumia.Imaging.EditShowcase.ImageProcessors.DepthOfField;


namespace Lumia.Imaging.EditShowcase
{
    public static class EffectFactory
    {
        public enum ChannelType { RGB, Red, Green, Blue };
        public static Task<List<IImageProcessor>> CreateEffects()
        {
            // This seems to take 150-200 ms minimum, so better do it on the thread pool.
            return Task.Run(async () =>
                 {

                     var imageProcessors = new List<IImageProcessor>();
                     EffectProcessor effectViewModel = null;

                     var source = await PreloadedImages.Man;

                     imageProcessors.Add(new EffectProcessor("Antique", new AntiqueEffect()));
                     imageProcessors.Add(new EffectProcessor("Auto Enhance", new AutoEnhanceEffect()));
                     imageProcessors.Add(new EffectProcessor("Auto Levels", new AutoLevelsEffect()));
                     imageProcessors.Add(new BlendEffectProcessor());
                     imageProcessors.Add(new EffectProcessor("Blur", new BlurEffect()));
                     imageProcessors.Add(new EffectProcessor("Brightness Effect", new BrightnessEffect()));
                     imageProcessors.Add(new EffectProcessor("Cartoon", new CartoonEffect()));                   
                     imageProcessors.Add(new EffectProcessor("ChromaKey", new ChromaKeyEffect()));    
                     imageProcessors.Add(new EffectProcessor("ColorAdjust", new ColorAdjustEffect()));
                     imageProcessors.Add(new EffectProcessor("ColorBoost", new ColorBoostEffect(1.0)));
                     imageProcessors.Add(new EffectProcessor("Colorization", new ColorizationEffect()));
                     imageProcessors.Add(new EffectProcessor("Color Swap", new ColorSwapEffect(Windows.UI.Color.FromArgb(255, 255, 0, 0), Windows.UI.Color.FromArgb(255, 0, 255, 0), 0.8, false, true)));                 
                     imageProcessors.Add(new EffectProcessor("Contrast", new ContrastEffect(0.5)));
                     imageProcessors.Add(new CropEffectProcessor());
                     imageProcessors.Add(new CurveProcessor());
                     imageProcessors.Add(new EffectProcessor("Despeckle", new DespeckleEffect(DespeckleLevel.Low)));
                     imageProcessors.Add(new EffectProcessor("Emboss", new EmbossEffect(0.5)));
                     imageProcessors.Add(new EffectProcessor("Exposure", new ExposureEffect(ExposureMode.Natural, 0.5)));
                     imageProcessors.Add(new EffectProcessor("Flip", new FlipEffect(FlipMode.Both)));                  
                     imageProcessors.Add(new EffectProcessor("Fog", new FogEffect()));
                     imageProcessors.Add(new EffectProcessor("Foundation", new FoundationEffect()));
                     imageProcessors.Add(new EffectProcessor("GaussianNoise", new GaussianNoiseEffect(1.0)));

                     var gradient = new RadialGradient(new Point(0.5, 0.5), new EllipseRadius(0.5, 0.5));
                     gradient.Stops = new[]
                         {
                                  new GradientStop{Offset=0.0, Color=Colors.Red},
                                  new GradientStop{Offset=1.0, Color=Colors.Cyan}
                              };

                     var gradientImageSource = new GradientImageSource(new Size(800, 500), gradient);

                     imageProcessors.Add(new EffectProcessor("Grayscale", new GrayscaleEffect()));
                     imageProcessors.Add(new EffectProcessor("Grayscale Negative", new GrayscaleNegativeEffect()));
                     imageProcessors.Add(new HSLProcessor());
                     imageProcessors.Add(new EffectProcessor("HdrEffect", new HdrEffect()));
                     imageProcessors.Add(new EffectProcessor("HueSaturationEffect", new HueSaturationEffect()));
                
                     effectViewModel = new EffectProcessor("Levels", new LevelsEffect(), true, true);
                     {
                         RangeEditorViewModelEx<LevelsEffect> blackEditor = null;
                         RangeEditorViewModelEx<LevelsEffect> grayEditor = null;
                         RangeEditorViewModelEx<LevelsEffect> whiteEditor = null;

                               blackEditor = new RangeEditorViewModelEx<LevelsEffect>(
                                   "Black",
                                   0.0,
                                   1.0,
                                   effectViewModel,
                                   effect => effect.Black,
                                   (effect, value) =>
                                   {
                                       effect.Black = value;
                                       if (grayEditor.Value < value) grayEditor.Value = value;
                                       if (whiteEditor.Value < value) whiteEditor.Value = value;
                                   });

                               grayEditor = new RangeEditorViewModelEx<LevelsEffect>(
                                   "Gray",
                                   0.0,
                                   1.0,
                                   effectViewModel,
                                   effect => effect.Gray,
                                   (effect, value) =>
                                   {
                                       effect.Gray = value;
                                       if (blackEditor.Value > value) blackEditor.Value = value;
                                       if (whiteEditor.Value < value) whiteEditor.Value = value;
                                   });

                               whiteEditor = new RangeEditorViewModelEx<LevelsEffect>(
                                   "White",
                                   0.0,
                                   1.0,
                                   effectViewModel,
                                   effect => effect.White,
                                   (effect, value) =>
                                   {
                                       effect.White = value;
                                       if (blackEditor.Value > value) blackEditor.Value = value;
                                       if (grayEditor.Value > value) grayEditor.Value = value;
                                   });

                               effectViewModel.Editors.Add(blackEditor);
                               effectViewModel.Editors.Add(grayEditor);
                               effectViewModel.Editors.Add(whiteEditor); 
                     }
                     imageProcessors.Add(effectViewModel);
                     imageProcessors.Add(new LinearGradientImageSourceEffectProcessor());
                     imageProcessors.Add(new EffectProcessor("Local Boost Automatic", new LocalBoostAutomaticEffect(8)));
                     imageProcessors.Add(new EffectProcessor("LocalBoost", new LocalBoostEffect()));
                     imageProcessors.Add(new EffectProcessor("Lomo", new LomoEffect()));
                     imageProcessors.Add(new EffectProcessor("Magic Pen", new MagicPenEffect()));
                     imageProcessors.Add(new EffectProcessor("Milky", new MilkyEffect()));
                     imageProcessors.Add(new EffectProcessor("Mirror", new MirrorEffect()));
                     imageProcessors.Add(new EffectProcessor("Mono Color", new MonoColorEffect(Windows.UI.Color.FromArgb(255, 255, 0, 0), 0.8)));
                     imageProcessors.Add(new EffectProcessor("Moonlight", new MoonlightEffect(21)));
                     imageProcessors.Add(new EffectProcessor("Negative", new NegativeEffect()));
                     imageProcessors.Add(new EffectProcessor("Noise", new NoiseEffect(NoiseLevel.Maximum)));
                     imageProcessors.Add(new EffectProcessor("Oily", new OilyEffect(OilBrushSize.Medium)));                 
                     imageProcessors.Add(new EffectProcessor("Paint", new PaintEffect(4)));
                     imageProcessors.Add(new EffectProcessor("Posterize", new PosterizeEffect(10)));
                     imageProcessors.Add(new RadialGradientImageSourceEffectProcessor());
                     imageProcessors.Add(new ReframingEffectProcessor());
                     imageProcessors.Add(new EffectProcessor("Rotation", new RotationEffect()));
                     imageProcessors.Add(new RgbLevelsEffectProcessor());
                     imageProcessors.Add(new RgbMixerEffectProcessor());
                     imageProcessors.Add(new SaturationLightnessProcessor());
                     imageProcessors.Add(new EffectProcessor("Sharpness", new SharpnessEffect(0.2)));                     
                     imageProcessors.Add(new EffectProcessor("Sepia", new SepiaEffect()));
                     imageProcessors.Add(new EffectProcessor("Sketch", new Lumia.Imaging.Artistic.SketchEffect(SketchMode.Color)));               
                     imageProcessors.Add(new EffectProcessor("Solarize", new SolarizeEffect(0.8)));
                     imageProcessors.Add(new SplitToneEffectProcessor());
                     imageProcessors.Add(new SpotlightEffectProcessor());
                     imageProcessors.Add(new EffectProcessor("Stamp", new StampEffect(5, 200.0 / 255.0)));
                     imageProcessors.Add(new EffectProcessor("Temperature and Tint", new TemperatureAndTintEffect(50.0 / 255.0, 50.0 / 255.0)));
                     imageProcessors.Add(new EffectProcessor("Vibrance", new VibranceEffect()));
                     imageProcessors.Add(new EffectProcessor("Vignetting", new VignettingEffect(0.8, Windows.UI.Color.FromArgb(255, 255, 0, 0))));
                     imageProcessors.Add(new EffectProcessor("Watercolor", new WatercolorEffect(0.8, 0.8)));
                     imageProcessors.Add(new EffectProcessor("Warping", new WarpingEffect(WarpMode.Twister, 0.8)));
                     imageProcessors.Add(new EffectProcessor("White Balance", new WhiteBalanceEffect(WhitePointCalculationMode.Maximum, Color.FromArgb(1,219,213,199))));
                     imageProcessors.Add(new EffectProcessor("Whiteboard Enhancement", new WhiteboardEnhancementEffect(WhiteboardEnhancementMode.Hard), canRenderAtPreviewSize: false));
                   
                     //GlamMe effects
                     imageProcessors.Add(new BWEffect());
                     imageProcessors.Add(new ElegantEffect());
                     imageProcessors.Add(new RetroEffect());
                     imageProcessors.Add(new VintageEffect());                   
                     imageProcessors.Add(new MintEffect());                 
                     imageProcessors.Add(new OldPosterEffect());                 
                     imageProcessors.Add(new LensBlureSampleEffect());
                     imageProcessors.Add(new GlamMeLomoEffect());
                     imageProcessors.Add(new FreshEffect());                    
                     imageProcessors.Add(new LightEffect());

                     // Creative Studio Effects
                     imageProcessors.Add(new BWloEffect());
                     imageProcessors.Add(new IndoorEffect());                    
                     imageProcessors.Add(new SunsetEffect());                    
                     imageProcessors.Add(new BWHiEffect());
                     imageProcessors.Add(new BWCopperEffect());
                     imageProcessors.Add(new LoSatWarmEffect());

              //       imageProcessors.Add(new BlockTiltDoF());
              //       imageProcessors.Add(new EllipseTiltDoF());
              //       imageProcessors.Add(new LandscapeBackgroundDoF());                
              //         imageProcessors.Add(new LensBlurProcessor()); // Special command editor             

                     return imageProcessors;

                 });
        }
    }
}
