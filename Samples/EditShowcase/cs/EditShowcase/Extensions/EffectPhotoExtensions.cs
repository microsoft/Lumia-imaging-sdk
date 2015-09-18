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

using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.UI;
using Lumia.Imaging;
using Lumia.Imaging.Extras.Layers;
using Lumia.Imaging.EditShowcase.Utilities;
using Lumia.Imaging.Adjustments;

namespace Lumia.Imaging.EditShowcase.Extensions
{
    public static class EffectPhotoExtensions
    {

        public static Curve CreateCurveFromHueRangeAdjustments(params HueRangeAdjustment[] adjustments)
        {
            Curve curve = new Curve(CurveInterpolation.Linear);
            curve.SetPoint(255, 0);
            foreach (var adjustment in adjustments)
            {
                var adjustmentLevel = ConvertAdjustmentLevel(adjustment.Adjustment);

                switch (adjustment.HueRange)
                {
                    case 0:
                        curve = Curve.Add(curve, new Curve(CurveInterpolation.Linear)
                        {
                            Points = new[]
                            {
                                new Point(0, adjustmentLevel),
                                new Point(255, adjustmentLevel),
                            }
                        }, null);
                        break;
                    case 1:
                        curve = Curve.Add(curve, new Curve(CurveInterpolation.Linear)
                        {
                            Points = new[]
                            {
                                new Point(0, adjustmentLevel),
                                new Point(AngleToHue(15), adjustmentLevel),
                                new Point(AngleToHue(45), 0),
                                new Point(AngleToHue(315), 0),
                                new Point(AngleToHue(345), adjustmentLevel),
                                new Point(255, adjustmentLevel),
                            }
                        }, null);
                        break;
                    case 2:
                        curve = Curve.Add(curve, new Curve(CurveInterpolation.Linear)
                        {
                            Points = new[]
                            {
                                new Point(AngleToHue(15), 0),
                                new Point(AngleToHue(45), adjustmentLevel),
                                new Point(AngleToHue(75), adjustmentLevel),
                                new Point(AngleToHue(105), 0),                  
                                new Point(255, 0),
                            }
                        }, null);
                        break;
                    case 3:
                        curve = Curve.Add(curve, new Curve(CurveInterpolation.Linear)
                        {
                            Points = new[]
                            {
                                new Point(AngleToHue(75), 0),
                                new Point(AngleToHue(105), adjustmentLevel),
                                new Point(AngleToHue(135), adjustmentLevel),
                                new Point(AngleToHue(165), 0),
                                new Point(255, 0),
                            }
                        }, null);
                        break;
                    case 4:
                        curve = Curve.Add(curve, new Curve(CurveInterpolation.Linear)
                        {
                            Points = new[]
                            {
                                new Point(AngleToHue(135), 0),
                                new Point(AngleToHue(165), adjustmentLevel),
                                new Point(AngleToHue(195), adjustmentLevel),
                                new Point(AngleToHue(225), 0),
                                new Point(255, 0),
                            }
                        }, null);
                        break;
                    case 5:
                        curve = Curve.Add(curve, new Curve(CurveInterpolation.Linear)
                        {
                            Points = new[]
                            {
                                new Point(AngleToHue(195), 0),
                                new Point(AngleToHue(225), adjustmentLevel),
                                new Point(AngleToHue(255), adjustmentLevel),
                                new Point(AngleToHue(285), 0),
                                new Point(255, 0),
                            }
                        }, null);
                        break;
                    case 6:
                        curve = Curve.Add(curve, new Curve(CurveInterpolation.Linear)
                        {
                            Points = new[]
                            {
                                new Point(AngleToHue(255), 0),
                                new Point(AngleToHue(285), adjustmentLevel),
                                new Point(AngleToHue(315), adjustmentLevel),
                                new Point(AngleToHue(345), 0),
                                new Point(255, 0),
                            }
                        }, null);
                        break;
                }
            }

            return curve;
        }

        public static int AngleToHue(int angle)
        {
            return (byte)(angle * 255.0 / 359.0);
        }

        public struct HueRangeAdjustment
        {
            public HueRangeAdjustment(int hueRange, int adjustment)
            {
                HueRange = hueRange;
                Adjustment = adjustment;
            }

            public readonly int HueRange;
            public readonly int Adjustment;
        };

        public static int ConvertAdjustmentLevel(int adj)
        {
            return Math.Min(255, Math.Max(-255, (int)(adj * 255.0 / 100.0)));
        }


        public static Curve CreateGainCurve(double gain)
        {
            int integerGain = Math.Max(0, Math.Min(255, (int)((gain - 1.0) * 255.0)));
            return new Curve(CurveInterpolation.Linear, new[]
            {
                new Point(255 - integerGain, 255),
            });
        }
    }
}