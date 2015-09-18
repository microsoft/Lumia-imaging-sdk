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

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Compositing;
using Lumia.Imaging.Transforms;

namespace Lumia.Imaging.Extras.Tests.Layers
{
    [TestClass]
    public class AdjustmentLayerTest
    {
        private static Size BackgroundLayerSize = new Size(50, 50);
        private static Size TestLayerSize = new Size(100, 100);

        private LayerContext CreateFakeLayerContext(Layer backgroundLayer, IImageProvider backgroundImage, Layer layer)
        {
            var invariants = new LayerContext.Invariants(backgroundLayer, new MaybeTask<IImageProvider>(backgroundImage), new Size(0, 0));
            var layerContext = new LayerContext(invariants, backgroundLayer, layer, 1);
            layerContext.PreviousImage = layerContext.BackgroundImage;
            return layerContext;
        }

        private IImageProvider CreateTestEffect()
        {
            return new ContrastEffect(0.5);
        }

        private IImageProvider CreateTestEffect(LayerContext layerContext)
        {
            return new ReframingEffect(new Rect(new Point(0,0), layerContext.BackgroundLayer.ImageSize), 0.0);
        }

        private Task<IImageProvider> CreateTestEffectAsync()
        {
            return Task.Factory.StartNew<IImageProvider>(() => new ContrastEffect(0.5));
        }

        private Task<IImageProvider> CreateTestEffectAsync(LayerContext layerContext)
        {
            return Task.Factory.StartNew<IImageProvider>(() => new ReframingEffect(new Rect(new Point(0, 0), layerContext.BackgroundLayer.ImageSize), 0.0));
        }

        [TestMethod]
        public void AdjustmentLayerWithSpecifiedLayerStyleHasSpecifiedLayerStyle()
        {
            var layer = new AdjustmentLayer(LayerStyle.Add(0.6), CreateTestEffect());
            Assert.AreEqual(BlendFunction.Add, layer.Style.BlendFunction);
            Assert.AreEqual(0.6, layer.Style.Opacity);
        }

        [TestMethod]
        public void AdjustmentLayerWithSpecifiedLayerStyleHasSpecifiedLayerStyleWhenCreatedWithLambda()
        {
            var layer = new AdjustmentLayer(LayerStyle.Add(0.6), context => CreateTestEffect(context));
            Assert.AreEqual(BlendFunction.Add, layer.Style.BlendFunction);
            Assert.AreEqual(0.6, layer.Style.Opacity);
        }

        [TestMethod]
        public void AdjustmentLayerWithSpecifiedLayerStyleHasSpecifiedLayerStyleWhenCreatedWithAsynchronousImageProvider()
        {
            var layer = new AdjustmentLayer(LayerStyle.Add(0.6), CreateTestEffectAsync());
            Assert.AreEqual(BlendFunction.Add, layer.Style.BlendFunction);
            Assert.AreEqual(0.6, layer.Style.Opacity);
        }

        [TestMethod]
        public void AdjustmentLayerWithSpecifiedLayerStyleHasSpecifiedLayerStyleWhenCreatedWithLambdaReturningAsynchronousImageProvider()
        {
            var layer = new AdjustmentLayer(LayerStyle.Add(0.6), context => CreateTestEffectAsync(context));
            Assert.AreEqual(BlendFunction.Add, layer.Style.BlendFunction);
            Assert.AreEqual(0.6, layer.Style.Opacity);
        }

        [TestMethod]
        public async Task GetImageProviderReturnsExpectedResult()
        {
            var backgroundImage = new ColorImageSource(BackgroundLayerSize, Colors.AliceBlue);
            var backgroundLayer = new Layer(LayerStyle.Normal(), backgroundImage, BackgroundLayerSize);

            var layer = new AdjustmentLayer(LayerStyle.Normal(), CreateTestEffect());
            var imageProvider = layer.GetImageProvider(CreateFakeLayerContext(backgroundLayer, backgroundImage, layer));
            ((IImageConsumer)await imageProvider.AsTask()).Source = backgroundImage;
            Assert.IsTrue(imageProvider.IsSynchronous);
            Assert.IsNotNull(imageProvider.Result);

            var bitmapRenderer = new BitmapRenderer(imageProvider.Result);
            var bitmap = await bitmapRenderer.RenderAsync();
            Assert.AreEqual(BackgroundLayerSize, bitmap.Dimensions);
        }

        [TestMethod]
        public async Task GetImageProviderReturnsExpectedResultWhenCreatedWithLambda()
        {
            var backgroundImage = new ColorImageSource(BackgroundLayerSize, Colors.AliceBlue);
            var backgroundLayer = new Layer(LayerStyle.Normal(), backgroundImage, BackgroundLayerSize);

            var layer = new AdjustmentLayer(LayerStyle.Normal(), context => CreateTestEffect(context));
            var imageProvider = layer.GetImageProvider(CreateFakeLayerContext(backgroundLayer, backgroundImage, layer));
            ((IImageConsumer)await imageProvider.AsTask()).Source = backgroundImage;
            Assert.IsTrue(imageProvider.IsSynchronous);
            Assert.IsNotNull(imageProvider.Result);

            var bitmapRenderer = new BitmapRenderer(imageProvider.Result);
            var bitmap = await bitmapRenderer.RenderAsync();
            Assert.AreEqual(BackgroundLayerSize, bitmap.Dimensions);
        }

        [TestMethod]
        public async Task GetImageProviderReturnsExpectedResultWhenCreatedWithAsynchronousImageProvider()
        {
            var backgroundImage = new ColorImageSource(BackgroundLayerSize, Colors.AliceBlue);
            var backgroundLayer = new Layer(LayerStyle.Normal(), backgroundImage, BackgroundLayerSize);

            var layer = new AdjustmentLayer(LayerStyle.Normal(), CreateTestEffectAsync());
            var imageProvider = layer.GetImageProvider(CreateFakeLayerContext(backgroundLayer, backgroundImage, layer));
            ((IImageConsumer)await imageProvider.AsTask()).Source = backgroundImage;
            Assert.IsFalse(imageProvider.WasSynchronous);
            var imageProviderResult = await imageProvider.AsTask();
            Assert.IsNotNull(imageProviderResult);

            var bitmapRenderer = new BitmapRenderer(imageProvider.Result);
            var bitmap = await bitmapRenderer.RenderAsync();
            Assert.AreEqual(BackgroundLayerSize, bitmap.Dimensions);
        }

        [TestMethod]
        public async Task GetImageProviderReturnsExpectedResultWhenCreatedWithLambdaReturningAsynchronousImageProvider()
        {
            var backgroundImage = new ColorImageSource(BackgroundLayerSize, Colors.AliceBlue);
            var backgroundLayer = new Layer(LayerStyle.Normal(), backgroundImage, BackgroundLayerSize);

            var layer = new AdjustmentLayer(LayerStyle.Normal(), context => CreateTestEffectAsync(context));
            var imageProvider = layer.GetImageProvider(CreateFakeLayerContext(backgroundLayer, backgroundImage, layer));
            ((IImageConsumer)await imageProvider.AsTask()).Source = backgroundImage;
            Assert.IsFalse(imageProvider.WasSynchronous);
            var imageProviderResult = await imageProvider.AsTask();
            Assert.IsNotNull(imageProviderResult);

            var bitmapRenderer = new BitmapRenderer(imageProvider.Result);
            var bitmap = await bitmapRenderer.RenderAsync();
            Assert.AreEqual(BackgroundLayerSize, bitmap.Dimensions);
        }
    }
}
