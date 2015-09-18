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

using Lumia.Imaging.Compositing;
using Lumia.Imaging.Extras.Layers;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;

namespace Lumia.Imaging.Extras.Tests.Layers
{
    [TestClass]
    public class LayerStyleTest
    {
        [TestMethod]
        public void Create()
        {
            var mask = new ColorImageSource(new Size(100, 100), Colors.AliceBlue);
            var rect = new Rect(1,2,3,4);

            var layerStyle = new LayerStyle(BlendFunction.Screen, 0.7, mask, rect);
            Assert.AreEqual(BlendFunction.Screen, layerStyle.BlendFunction);
            Assert.AreEqual(0.7, layerStyle.Opacity);
            Assert.AreSame(mask, layerStyle.MaskResolver(null).Result);
            Assert.AreEqual(rect, layerStyle.TargetArea);
        }

        [TestMethod]
        public void CreateWithMaybeTaskMask()
        {
            var mask = new ColorImageSource(new Size(100, 100), Colors.AliceBlue);
            var rect = new Rect(1, 2, 3, 4);

            var layerStyle = new LayerStyle(BlendFunction.Screen, 0.7, new MaybeTask<IImageProvider>(mask), rect);
            Assert.AreEqual(BlendFunction.Screen, layerStyle.BlendFunction);
            Assert.AreEqual(0.7, layerStyle.Opacity);
            Assert.AreSame(mask, layerStyle.MaskResolver(null).Result);
            Assert.AreEqual(rect, layerStyle.TargetArea);
        }

        [TestMethod]
        public void CreateWithTaskMask()
        {
            var mask = new ColorImageSource(new Size(100, 100), Colors.AliceBlue);
            var rect = new Rect(1, 2, 3, 4);

            var layerStyle = new LayerStyle(BlendFunction.Screen, 0.7, Task.FromResult<IImageProvider>(mask), rect);
            Assert.AreEqual(BlendFunction.Screen, layerStyle.BlendFunction);
            Assert.AreEqual(0.7, layerStyle.Opacity);
            Assert.AreSame(mask, layerStyle.MaskResolver(null).Result);
            Assert.AreEqual(rect, layerStyle.TargetArea);
        }

        [TestMethod]
        public void FactoryMethodsGiveCorrectBlendFunction()
        {
            Assert.AreEqual(BlendFunction.Normal, LayerStyle.Normal().BlendFunction);
            Assert.AreEqual(BlendFunction.Add, LayerStyle.Add().BlendFunction);
            Assert.AreEqual(BlendFunction.Color, LayerStyle.Color().BlendFunction);
            Assert.AreEqual(BlendFunction.Colorburn, LayerStyle.Colorburn().BlendFunction);
            Assert.AreEqual(BlendFunction.Colordodge, LayerStyle.Colordodge().BlendFunction);
            Assert.AreEqual(BlendFunction.Darken, LayerStyle.Darken().BlendFunction);
            Assert.AreEqual(BlendFunction.Difference, LayerStyle.Difference().BlendFunction);
            Assert.AreEqual(BlendFunction.Exclusion, LayerStyle.Exclusion().BlendFunction);
            Assert.AreEqual(BlendFunction.Hardlight, LayerStyle.Hardlight().BlendFunction);
            Assert.AreEqual(BlendFunction.Hue, LayerStyle.Hue().BlendFunction);
            Assert.AreEqual(BlendFunction.Lighten, LayerStyle.Lighten().BlendFunction);
            Assert.AreEqual(BlendFunction.Lineardodge, LayerStyle.Lineardodge().BlendFunction);
            Assert.AreEqual(BlendFunction.Linearlight, LayerStyle.Linearlight().BlendFunction);
            Assert.AreEqual(BlendFunction.Multiply, LayerStyle.Multiply().BlendFunction);
            Assert.AreEqual(BlendFunction.Overlay, LayerStyle.Overlay().BlendFunction);
            Assert.AreEqual(BlendFunction.Screen, LayerStyle.Screen().BlendFunction);
            Assert.AreEqual(BlendFunction.Softlight, LayerStyle.Softlight().BlendFunction);
            Assert.AreEqual(BlendFunction.Vividlight, LayerStyle.Vividlight().BlendFunction);
        }

        [TestMethod]
        public void FactoryMethodsGiveCorrectOpacity()
        {
            Assert.AreEqual(0.4, LayerStyle.Normal(0.4).Opacity);
            Assert.AreEqual(0.4, LayerStyle.Add(0.4).Opacity);
            Assert.AreEqual(0.4, LayerStyle.Color(0.4).Opacity);
            Assert.AreEqual(0.4, LayerStyle.Colorburn(0.4).Opacity);
            Assert.AreEqual(0.4, LayerStyle.Colordodge(0.4).Opacity);
            Assert.AreEqual(0.4, LayerStyle.Darken(0.4).Opacity);
            Assert.AreEqual(0.4, LayerStyle.Difference(0.4).Opacity);
            Assert.AreEqual(0.4, LayerStyle.Exclusion(0.4).Opacity);
            Assert.AreEqual(0.4, LayerStyle.Hardlight(0.4).Opacity);
            Assert.AreEqual(0.4, LayerStyle.Hue(0.4).Opacity);
            Assert.AreEqual(0.4, LayerStyle.Lighten(0.4).Opacity);
            Assert.AreEqual(0.4, LayerStyle.Lineardodge(0.4).Opacity);
            Assert.AreEqual(0.4, LayerStyle.Linearlight(0.4).Opacity);
            Assert.AreEqual(0.4, LayerStyle.Multiply(0.4).Opacity);
            Assert.AreEqual(0.4, LayerStyle.Overlay(0.4).Opacity);
            Assert.AreEqual(0.4, LayerStyle.Screen(0.4).Opacity);
            Assert.AreEqual(0.4, LayerStyle.Softlight(0.4).Opacity);
            Assert.AreEqual(0.4, LayerStyle.Vividlight(0.4).Opacity);
        }

        [TestMethod]
        public void FactoryMethodsGiveCorrectMask()
        {
            var mask = new ColorImageSource(new Size(100, 100), Colors.AliceBlue);

            Assert.AreSame(mask, LayerStyle.Normal(1.0, mask).MaskResolver(null).Result);
            Assert.AreSame(mask, LayerStyle.Add(1.0, mask).MaskResolver(null).Result);
            Assert.AreSame(mask, LayerStyle.Color(1.0, mask).MaskResolver(null).Result);
            Assert.AreSame(mask, LayerStyle.Colorburn(1.0, mask).MaskResolver(null).Result);
            Assert.AreSame(mask, LayerStyle.Colordodge(1.0, mask).MaskResolver(null).Result);
            Assert.AreSame(mask, LayerStyle.Darken(1.0, mask).MaskResolver(null).Result);
            Assert.AreSame(mask, LayerStyle.Difference(1.0, mask).MaskResolver(null).Result);
            Assert.AreSame(mask, LayerStyle.Exclusion(1.0, mask).MaskResolver(null).Result);
            Assert.AreSame(mask, LayerStyle.Hardlight(1.0, mask).MaskResolver(null).Result);
            Assert.AreSame(mask, LayerStyle.Hue(1.0, mask).MaskResolver(null).Result);
            Assert.AreSame(mask, LayerStyle.Lighten(1.0, mask).MaskResolver(null).Result);
            Assert.AreSame(mask, LayerStyle.Lineardodge(1.0, mask).MaskResolver(null).Result);
            Assert.AreSame(mask, LayerStyle.Linearlight(1.0, mask).MaskResolver(null).Result);
            Assert.AreSame(mask, LayerStyle.Multiply(1.0, mask).MaskResolver(null).Result);
            Assert.AreSame(mask, LayerStyle.Overlay(1.0, mask).MaskResolver(null).Result);
            Assert.AreSame(mask, LayerStyle.Screen(1.0, mask).MaskResolver(null).Result);
            Assert.AreSame(mask, LayerStyle.Softlight(1.0, mask).MaskResolver(null).Result);
            Assert.AreSame(mask, LayerStyle.Vividlight(1.0, mask).MaskResolver(null).Result);
        }

        [TestMethod]
        public void FactoryMethodsGiveCorrectTargetArea()
        {
            Rect rect = new Rect(1,2,3,4);

            Assert.AreEqual(rect, LayerStyle.Normal(1.0, null, rect).TargetArea);
            Assert.AreEqual(rect, LayerStyle.Add(1.0, null, rect).TargetArea);
            Assert.AreEqual(rect, LayerStyle.Color(1.0, null, rect).TargetArea);
            Assert.AreEqual(rect, LayerStyle.Colorburn(1.0, null, rect).TargetArea);
            Assert.AreEqual(rect, LayerStyle.Colordodge(1.0, null, rect).TargetArea);
            Assert.AreEqual(rect, LayerStyle.Darken(1.0, null, rect).TargetArea);
            Assert.AreEqual(rect, LayerStyle.Difference(1.0, null, rect).TargetArea);
            Assert.AreEqual(rect, LayerStyle.Exclusion(1.0, null, rect).TargetArea);
            Assert.AreEqual(rect, LayerStyle.Hardlight(1.0, null, rect).TargetArea);
            Assert.AreEqual(rect, LayerStyle.Hue(1.0, null, rect).TargetArea);
            Assert.AreEqual(rect, LayerStyle.Lighten(1.0, null, rect).TargetArea);
            Assert.AreEqual(rect, LayerStyle.Lineardodge(1.0, null, rect).TargetArea);
            Assert.AreEqual(rect, LayerStyle.Linearlight(1.0, null, rect).TargetArea);
            Assert.AreEqual(rect, LayerStyle.Multiply(1.0, null, rect).TargetArea);
            Assert.AreEqual(rect, LayerStyle.Overlay(1.0, null, rect).TargetArea);
            Assert.AreEqual(rect, LayerStyle.Screen(1.0, null, rect).TargetArea);
            Assert.AreEqual(rect, LayerStyle.Softlight(1.0, null, rect).TargetArea);
            Assert.AreEqual(rect, LayerStyle.Vividlight(1.0, null, rect).TargetArea);
        }
    }
}
