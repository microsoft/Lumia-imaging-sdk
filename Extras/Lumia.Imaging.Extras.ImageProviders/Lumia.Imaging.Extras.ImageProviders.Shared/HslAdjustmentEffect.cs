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

using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Extras.Layers;
using System;
using Windows.Foundation;

namespace Lumia.Imaging.Extras.Effects
{
    /// <summary>
    /// HSL adjustment effect, modelled after a well known type of 6+1 channel HSL adjustment seen in paint packages.
    /// </summary>
    public class HslAdjustmentEffect : EffectGroupBase
    {
        // Note: Written with performance in mind more than readability, trying to avoid allocations, WinRT class activations
        // and calls across the WinRT ABI border.   

        private const int ChannelCount = 7;
        private const int MasterChannelIndex = 0;
        private const int RedChannelIndex = 1;
        private const int YellowChannelIndex = 2;
        private const int GreenChannelIndex = 3;
        private const int CyanChannelIndex = 4;
        private const int BlueChannelIndex = 5;
        private const int MagentaChannelIndex = 6;

        private HueSaturationLightnessEffect m_filtersWithoutMasterLightness;
        private CurvesEffect m_allEffects;
        private int[] m_saturation;
        private int[] m_lightness;
        private bool m_isSaturationDirty = true;
        private bool m_isLightnessDirty = true;
        private Curve m_masterLightnessCurve;
        private Curve m_tempCurve;

        public HslAdjustmentEffect()
        {
            m_saturation = new int[ChannelCount];
            m_lightness = new int[ChannelCount];

			m_filtersWithoutMasterLightness = new HueSaturationLightnessEffect();
			m_masterLightnessCurve = new Curve(CurveInterpolation.Linear);
			m_allEffects = new CurvesEffect(m_filtersWithoutMasterLightness, m_masterLightnessCurve);
			m_tempCurve = new Curve(CurveInterpolation.Linear);
        }

        /// <summary>
        /// Master lightness adjustment value, in the range [-100, 100].
        /// </summary>
        public int MasterLightness
        {
            get { return GetLightness(MasterChannelIndex); }
            set { SetLightness(MasterChannelIndex, value); }
        }

        /// <summary>
        /// Red lightness adjustment value, in the range [-100, 100].
        /// </summary>
        public int RedLightness
        {
            get { return GetLightness(RedChannelIndex); }
            set { SetLightness(RedChannelIndex, value); }
        }

        /// <summary>
        /// Yellow lightness adjustment value, in the range [-100, 100].
        /// </summary>
        public int YellowLightness
        {
            get { return GetLightness(YellowChannelIndex); }
            set { SetLightness(YellowChannelIndex, value); }
        }

        /// <summary>
        /// Green lightness adjustment value, in the range [-100, 100].
        /// </summary>
        public int GreenLightness
        {
            get { return GetLightness(GreenChannelIndex); }
            set { SetLightness(GreenChannelIndex, value); }
        }

        /// <summary>
        /// Cyan lightness adjustment value, in the range [-100, 100].
        /// </summary>
        public int CyanLightness
        {
            get { return GetLightness(CyanChannelIndex); }
            set { SetLightness(CyanChannelIndex, value); }
        }

        /// <summary>
        /// Blue lightness adjustment value, in the range [-100, 100].
        /// </summary>
        public int BlueLightness
        {
            get { return GetLightness(BlueChannelIndex); }
            set { SetLightness(BlueChannelIndex, value); }
        }

        /// <summary>
        /// Magenta lightness adjustment value, in the range [-100, 100].
        /// </summary>
        public int MagentaLightness
        {
            get { return GetLightness(MagentaChannelIndex); }
            set { SetLightness(MagentaChannelIndex, value); }
        }

        /// <summary>
        /// Master saturation adjustment value, in the range [-100, 100].
        /// </summary>
        public int MasterSaturation
        {
            get { return GetSaturation(MasterChannelIndex); }
            set { SetSaturation(MasterChannelIndex, value); }
        }

        /// <summary>
        /// Red saturation adjustment value, in the range [-100, 100].
        /// </summary>
        public int RedSaturation
        {
            get { return GetSaturation(RedChannelIndex); }
            set { SetSaturation(RedChannelIndex, value); }
        }

        /// <summary>
        /// Yellow saturation adjustment value, in the range [-100, 100].
        /// </summary>
        public int YellowSaturation
        {
            get { return GetSaturation(YellowChannelIndex); }
            set { SetSaturation(YellowChannelIndex, value); }
        }

        /// <summary>
        /// Green saturation adjustment value, in the range [-100, 100].
        /// </summary>
        public int GreenSaturation
        {
            get { return GetSaturation(GreenChannelIndex); }
            set { SetSaturation(GreenChannelIndex, value); }
        }

        /// <summary>
        /// Cyan saturation adjustment value, in the range [-100, 100].
        /// </summary>
        public int CyanSaturation
        {
            get { return GetSaturation(CyanChannelIndex); }
            set { SetSaturation(CyanChannelIndex, value); }
        }

        /// <summary>
        /// Blue saturation adjustment value, in the range [-100, 100].
        /// </summary>
        public int BlueSaturation
        {
            get { return GetSaturation(BlueChannelIndex); }
            set { SetSaturation(BlueChannelIndex, value); }
        }

        /// <summary>
        /// Magenta saturation adjustment value, in the range [-100, 100].
        /// </summary>
        public int MagentaSaturation
        {
            get { return GetSaturation(MagentaChannelIndex); }
            set { SetSaturation(MagentaChannelIndex, value); }
        }

        protected override IImageProvider PrepareGroup(IImageProvider groupSource)
        {
            ThrowIfDisposed();
			IImageProvider currentEffect = m_filtersWithoutMasterLightness;
            if (m_isSaturationDirty || m_isLightnessDirty)
            {
				if (m_lightness[MasterChannelIndex] == 0)
				{
					currentEffect = m_filtersWithoutMasterLightness;
				}
				else
				{
					currentEffect = m_allEffects;
				}
            }

            if (m_isSaturationDirty)
            {
                m_isSaturationDirty = false;
                m_filtersWithoutMasterLightness.SaturationCurve = CreateCurveFromHueRangeAdjustments(m_saturation);
            }

            if (m_isLightnessDirty)
            {
                m_isLightnessDirty = false;

                // Master lightness is handled differently than in the separate channels.
                m_filtersWithoutMasterLightness.LightnessCurve = CreateCurveFromHueRangeAdjustments(m_lightness, skipMaster: true);

                if (m_lightness[MasterChannelIndex] < 0)
                {
                    m_masterLightnessCurve.SetPoint(0, 0);
                    m_masterLightnessCurve.SetPoint(255, 255 + ConvertAdjustmentLevel(m_lightness[MasterChannelIndex]));
                }
                else
                {
                    m_masterLightnessCurve.SetPoint(0, ConvertAdjustmentLevel(m_lightness[MasterChannelIndex]));
                    m_masterLightnessCurve.SetPoint(255, 255);
                }
            }
			return currentEffect;
        }

        private int GetLightness(int channel)
        {
            ThrowIfDisposed();

            return m_lightness[channel];
        }

        private void SetLightness(int channel, int value)
        {
            ThrowIfDisposed();

            if (m_lightness[channel] == value)
            {
                return;
            }

            if (value < -100 || value > 100)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            m_lightness[channel] = value;
            m_isLightnessDirty = true;
        }

        private int GetSaturation(int channel)
        {
            ThrowIfDisposed();

            return m_saturation[channel];
        }

        private void SetSaturation(int channel, int value)
        {
            ThrowIfDisposed();

            if (m_saturation[channel] == value)
            {
                return;
            }

            if (value < -100 || value > 100)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            m_saturation[channel] = value;
            m_isSaturationDirty = true;
        }

        private static int ConvertAdjustmentLevel(int adj)
        {
            return Math.Min(255, Math.Max(-255, (int)(adj * 255.0 / 100.0)));
        }

        private Curve CreateCurveFromHueRangeAdjustments(int[] adjustmentLevels, bool skipMaster = false)
        {
            ThrowIfDisposed();

            var curve = new Curve(CurveInterpolation.Linear, new[] { new Point(0, 0), new Point(255, 0) });

            int firstChannelIndex = skipMaster ? RedChannelIndex : MasterChannelIndex;

            for (int channelIndex = firstChannelIndex; channelIndex < ChannelCount; ++channelIndex)
            {
                AddAdjustmentCurvePoints(adjustmentLevels[channelIndex], TemplatePoints[channelIndex], m_tempCurve, ref curve);
            }

            return curve;
        }

        private static void AddAdjustmentCurvePoints(int adjustmentLevel, int[] templateTable, Curve tempCurve, ref Curve curve)
        {
            if (adjustmentLevel == 0)
            {
                return;
            }

            adjustmentLevel = ConvertAdjustmentLevel(adjustmentLevel);

            var points = new Point[templateTable.Length / 2];

            for (int i = 0; i < points.Length; ++i)
            {
                points[i] = new Point(templateTable[i * 2 + 0], templateTable[i * 2 + 1] * adjustmentLevel);
            }

            tempCurve.Points = points;

            Curve.Add(curve, tempCurve, curve);
        }

        private static int AngleToHue(int angle)
        {
            return (byte)(angle * 255.0 / 359.0);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "m_filterEffect")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "m_masterLightnessCurve")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "m_tempCurve")]
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            m_filtersWithoutMasterLightness = null;
            m_lightness = null;
            m_saturation = null;
            DisposeAndSetToNull(ref m_masterLightnessCurve);
            DisposeAndSetToNull(ref m_tempCurve);
        }

        // Template data for the points, describing the ramp on the hue axis for each channel.
        private static readonly int[][] TemplatePoints = 
        {
            new[]
            {
                0, 1,
                255, 1
            },
            new[]
            {
                0, 1,
                AngleToHue(15), 1,
                AngleToHue(45), 0,
                AngleToHue(315), 0,
                AngleToHue(345), 1,
                255, 1
            },
            new[]
            {
                AngleToHue(15), 0,
                AngleToHue(45), 1,
                AngleToHue(75), 1,
                AngleToHue(105), 0,
                255, 0
            },
            new[]
            {
                AngleToHue(75), 0,
                AngleToHue(105), 1,
                AngleToHue(135), 1,
                AngleToHue(165), 0,
                255, 0
            },
            new[]
            {
                AngleToHue(135), 0,
                AngleToHue(165), 1,
                AngleToHue(195), 1,
                AngleToHue(225), 0,
                255, 0
            },
            new[]
            {
                AngleToHue(195), 0,
                AngleToHue(225), 1,
                AngleToHue(255), 1,
                AngleToHue(285), 0,
                255, 0
            },
            new[]
            {
                AngleToHue(255), 0,
                AngleToHue(285), 1,
                AngleToHue(315), 1,
                AngleToHue(345), 0,
                255, 0
            }

        };

        /// <summary>
        /// Examine the referenced field and if it is IDisposable, dispose it. Also set it to null.
        /// </summary>
        /// <typeparam name="TDisposable"></typeparam>
        /// <param name="obj"></param>
        protected void DisposeAndSetToNull<TDisposable>(ref TDisposable obj)
        {
            var disposable = obj as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }

            obj = default(TDisposable);
        }

    }
}
