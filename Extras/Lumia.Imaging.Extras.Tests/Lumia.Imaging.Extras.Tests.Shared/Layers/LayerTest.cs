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

namespace Lumia.Imaging.Extras.Tests.Layers
{
    [TestClass]
    public class LayerTest
    {
        private static Size BackgroundLayerSize = new Size(50, 50);
        private static Size TestLayerSize = new Size(100, 100);

        private LayerContext CreateFakeLayerContext(Layer layer)
        {
            var backgroundImage = new ColorImageSource(BackgroundLayerSize, Colors.AliceBlue);
            var backgroundLayer = new Layer(LayerStyle.Normal(), backgroundImage, BackgroundLayerSize);
            var invariants = new LayerContext.Invariants(backgroundLayer, new MaybeTask<IImageProvider>(backgroundImage), new Size(0, 0));
            var layerContext = new LayerContext(invariants, backgroundLayer, layer, 1);
            layerContext.PreviousImage = layerContext.BackgroundImage;
            return layerContext;
        }

        private IImageProvider CreateTestImageProvider()
        {
            return new ColorImageSource(TestLayerSize, Colors.Red);
        }

        private IImageProvider CreateTestImageProvider(LayerContext layerContext)
        {
            return new ColorImageSource(layerContext.BackgroundLayer.ImageSize, Colors.Red);
        }

        private Task<IImageProvider> CreateTestImageProviderAsync()
        {
            return Task.Factory.StartNew<IImageProvider>(() => (IImageProvider)new ColorImageSource(TestLayerSize, Colors.Red), TaskCreationOptions.LongRunning);
        }

        private Task<IImageProvider> CreateTestImageProviderAsync(LayerContext layerContext)
        {
            return Task.Factory.StartNew<IImageProvider>(() => (IImageProvider)new ColorImageSource(layerContext.BackgroundLayer.ImageSize, Colors.Red), TaskCreationOptions.LongRunning);
        }

        [TestMethod]
        public void LayerWithSpecifiedLayerStyleHasSpecifiedLayerStyle()
        {
            var layer = new Layer(LayerStyle.Add(0.6), CreateTestImageProvider());
            Assert.AreEqual(BlendFunction.Add, layer.Style.BlendFunction);
            Assert.AreEqual(0.6, layer.Style.Opacity);
        }

        [TestMethod]
        public void LayerWithSpecifiedLayerStyleHasSpecifiedLayerStyleWhenCreatedWithLambda()
        {
            var layer = new Layer(LayerStyle.Add(0.6), context => CreateTestImageProvider(context));
            Assert.AreEqual(BlendFunction.Add, layer.Style.BlendFunction);
            Assert.AreEqual(0.6, layer.Style.Opacity);
        }

        [TestMethod]
        public void LayerWithSpecifiedLayerStyleHasSpecifiedLayerStyleWhenCreatedWithAsynchronousImageProvider()
        {
            var layer = new Layer(LayerStyle.Add(0.6), CreateTestImageProviderAsync());
            Assert.AreEqual(BlendFunction.Add, layer.Style.BlendFunction);
            Assert.AreEqual(0.6, layer.Style.Opacity);
        }

        [TestMethod]
        public void LayerWithSpecifiedLayerStyleHasSpecifiedLayerStyleWhenCreatedWithLambdaReturningAsynchronousImageProvider()
        {
            var layer = new Layer(LayerStyle.Add(0.6), context => CreateTestImageProviderAsync(context));
            Assert.AreEqual(BlendFunction.Add, layer.Style.BlendFunction);
            Assert.AreEqual(0.6, layer.Style.Opacity);
        }

        [TestMethod]
        public void LayerWithoutSpecifiedImageSizeHasZeroImageSize()
        {
            var layer = new Layer(LayerStyle.Normal(), CreateTestImageProvider());
            Assert.AreEqual(new Size(0, 0), layer.ImageSize);
        }

        [TestMethod]
        public void LayerWithSpecifiedImageSizeHasCorrectImageSize()
        {
            var layer = new Layer(LayerStyle.Normal(), CreateTestImageProvider(), new Size(75,25));
            Assert.AreEqual(new Size(75, 25), layer.ImageSize);
        }

        [TestMethod]
        public void LayerWithSpecifiedImageSizeHasCorrectImageSizeWhenCreatedWithAsynchronousImageProvider()
        {
            var layer = new Layer(LayerStyle.Normal(), CreateTestImageProviderAsync(), new Size(75, 25));
            Assert.AreEqual(new Size(75, 25), layer.ImageSize);
        }

        [TestMethod]
        public void LayerWithSpecifiedImageSizeHasCorrectImageSizeWhenCreatedWithLambda()
        {
            var layer = new Layer(LayerStyle.Normal(), context => CreateTestImageProvider(context), new Size(75, 25));
            Assert.AreEqual(new Size(75, 25), layer.ImageSize);
        }

        [TestMethod]
        public void LayerWithSpecifiedImageSizeHasCorrectImageSizeWhenCreatedWithLambdaReturningAsynchronousImageProvider()
        {
            var layer = new Layer(LayerStyle.Normal(), context => CreateTestImageProviderAsync(context), new Size(75, 25));
            Assert.AreEqual(new Size(75, 25), layer.ImageSize);
        }

        [TestMethod]
        public async Task GetImageProviderReturnsExpectedResult()
        {
            var layer = new Layer(LayerStyle.Normal(), CreateTestImageProvider());
            var imageProvider = layer.GetImageProvider(CreateFakeLayerContext(layer));
            Assert.IsTrue(imageProvider.IsSynchronous);
            Assert.IsNotNull(imageProvider.Result);

            var imageProviderInfo = await imageProvider.Result.GetInfoAsync();
            Assert.AreEqual(TestLayerSize, imageProviderInfo.ImageSize);
        }

        [TestMethod]
        public async Task GetImageProviderReturnsExpectedResultWhenCreatedWithLambda()
        {
            var layer = new Layer(LayerStyle.Normal(), context => CreateTestImageProvider(context));
            var imageProvider = layer.GetImageProvider(CreateFakeLayerContext(layer));
            Assert.IsTrue(imageProvider.IsSynchronous);
            Assert.IsNotNull(imageProvider.Result);

            var imageProviderInfo = await imageProvider.Result.GetInfoAsync();
            Assert.AreEqual(BackgroundLayerSize, imageProviderInfo.ImageSize);
        }

        [TestMethod]
        public async Task GetImageProviderReturnsExpectedResultWhenCreatedWithAsynchronousImageProvider()
        {
            var layer = new Layer(LayerStyle.Normal(), CreateTestImageProviderAsync());
            var imageProvider = layer.GetImageProvider(CreateFakeLayerContext(layer));
            Assert.IsFalse(imageProvider.WasSynchronous);
            var imageProviderResult = await imageProvider.AsTask();
            Assert.IsNotNull(imageProviderResult);

            var imageProviderInfo = await imageProvider.Result.GetInfoAsync();
            Assert.AreEqual(TestLayerSize, imageProviderInfo.ImageSize);
        }

        [TestMethod]
        public async Task GetImageProviderReturnsExpectedResultWhenCreatedWithLambdaReturningAsynchronousImageProvider()
        {
            var layer = new Layer(LayerStyle.Normal(), context => CreateTestImageProviderAsync(context));
            var imageProvider = layer.GetImageProvider(CreateFakeLayerContext(layer));
            Assert.IsFalse(imageProvider.WasSynchronous);
            var imageProviderResult = await imageProvider.AsTask();
            Assert.IsNotNull(imageProviderResult);

            var imageProviderInfo = await imageProvider.Result.GetInfoAsync();
            Assert.AreEqual(BackgroundLayerSize, imageProviderInfo.ImageSize);
        }
    }
}