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
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;

namespace Lumia.Imaging.EditShowcase
{
    internal class LookupCurves : IDisposable
    {
        private Task<Curve[]> m_loadingTask;

        public LookupCurves(string name, double gain = 1.0, int offset = 0)
        {
            m_loadingTask = LoadRgbLookupCurves(name, gain, offset);
        }

        private static async Task<Curve[]> LoadRgbLookupCurves(string path, double outputGain, int outputOffset)
        {
            var storageFile = await Package.Current.InstalledLocation.GetFileAsync(path).AsTask().ConfigureAwait(false);
            using (var storageFileImageSource = new StorageFileImageSource(storageFile))
            using (var renderer = new BitmapRenderer(storageFileImageSource) { OutputOption = OutputOption.PreserveAspectRatio })
            using (var tableBitmap = await renderer.RenderAsync().AsTask().ConfigureAwait(false))
            {
                var redCurve = new Curve();
                var greenCurve = new Curve();
                var blueCurve = new Curve();

                var redValues = new Point[256];
                var greenValues = new Point[256];
                var blueValues = new Point[256];

                var table = tableBitmap.Buffers[0].Buffer.ToArray();
                
                for (int i = 0; i < 256; ++i)
                {
                    redValues[i] = new Point(i,Math.Min(255, Math.Max(0, (int)(table[i * 4 + 2] * outputGain + outputOffset))));
                    greenValues[i] = new Point(i,Math.Min(255, Math.Max(0, (int)(table[i * 4 + 1] * outputGain + outputOffset))));
                    blueValues[i] = new Point(i,Math.Min(255, Math.Max(0, (int)(table[i * 4 + 0] * outputGain + outputOffset))));
                }

                redCurve.Points = redValues;
                greenCurve.Points = greenValues;
                blueCurve.Points = blueValues;

                return new[] { redCurve, greenCurve, blueCurve };
            }
        }

        public Task<Curve[]> GetAsync()
        {
            if (m_loadingTask == null)
            {
                throw new ObjectDisposedException("LookupCurves");
            }

            return m_loadingTask;
        }



        public async Task<IImageProvider> GetEffectAsync()
        {
            if (m_loadingTask == null)
            {
                throw new ObjectDisposedException("LookupCurves");
            }

            var curves = await m_loadingTask.ConfigureAwait(false);
            return new CurvesEffect(curves[0], curves[1], curves[2]);
        }

        public void Dispose()
        {
            m_loadingTask = null;
        }
    }
}
