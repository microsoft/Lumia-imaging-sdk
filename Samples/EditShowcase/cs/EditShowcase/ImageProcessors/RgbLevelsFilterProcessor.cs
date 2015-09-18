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
using Lumia.Imaging.EditShowcase.Editors;
using Lumia.Imaging.EditShowcase.ImageProcessors;
using Lumia.Imaging.EditShowcase.ViewModels.Editors;
using Lumia.Imaging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Extras;

namespace Lumia.Imaging.EditShowcase.ImageProcessors
{
    public class RgbLevelsEffectProcessor : EffectProcessor, IPropertyDescriptions
    {
        private Dictionary<string, PropertyDescription> m_propertyDescriptions;
        public enum ChannelType {RGB, Red, Green, Blue};       
        private RgbLevelsEffect m_RgbLevelsEffect;

        public ChannelType Channel { get; set; }

        public int RgbMinValue { get; set; }
        public double RgbMidToneValue { get; set; }
        public int RgbMaxValue { get; set; }

        public int RedMinValue { get; set; }
        public double RedMidToneValue { get; set; }
        public int RedMaxValue { get; set; }

        public int GreenMinValue { get; set; }
        public double GreenMidToneValue { get; set; }
        public int GreenMaxValue { get; set; }

        public int BlueMinValue { get; set; }
        public double BlueMidToneValue { get; set; }
        public int BlueMaxValue { get; set; }


        public int MinValue
        {
            get
            {
                if (Channel == ChannelType.RGB)
                {
                    return RgbMinValue;
                }
                else if (Channel == ChannelType.Red)
                {
                    return RedMinValue;
                }
                else if (Channel == ChannelType.Green)
                {
                    return GreenMinValue;
                }
               
                return BlueMinValue;
            }
            set
            {
                if (Channel == ChannelType.RGB)
                {
                    RgbMinValue = value;
                }
                else if (Channel == ChannelType.Red)
                {
                    RedMinValue = value;
                }
                else if (Channel == ChannelType.Green)
                {
                    GreenMinValue = value;
                }
                else
                {
                    BlueMinValue = value;
                }
            }
        }
        public double MidToneValue
        {
            get
            {
                if (Channel == ChannelType.RGB)
                {
                    return RgbMidToneValue;
                }
                else if (Channel == ChannelType.Red)
                {
                    return RedMidToneValue;
                }
                else if (Channel == ChannelType.Green)
                {
                    return GreenMidToneValue;
                }

                return BlueMidToneValue;
            }
            set
            {
                if (Channel == ChannelType.RGB)
                {
                    RgbMidToneValue= value;
                }
                else if (Channel == ChannelType.Red)
                {
                    RedMidToneValue = value;
                }
                else if (Channel == ChannelType.Green)
                {
                    GreenMidToneValue = value;
                }
                else
                {
                    BlueMidToneValue = value;
                }
            }
        }
        public int MaxValue
        {
            get
            {
                if (Channel == ChannelType.RGB)
                {
                    return RgbMaxValue;
                }
                else if (Channel == ChannelType.Red)
                {
                    return RedMaxValue;
                }
                else if (Channel == ChannelType.Green)
                {
                    return GreenMaxValue;
                }

                return BlueMaxValue;
            }
            set
            {
                if (Channel == ChannelType.RGB)
                {
                    RgbMaxValue = value;
                }
                else if (Channel == ChannelType.Red)
                {
                    RedMaxValue = value;
                }
                else if (Channel == ChannelType.Green)
                {
                    GreenMaxValue = value;
                }
                else
                {
                    BlueMaxValue = value;
                }
            }
        }

        public RgbLevelsEffectProcessor()
        {
            Name = "RgbLevelsEffect";

            MinValue = 0;
            MidToneValue = 1;
            MaxValue = 255;

            RgbMinValue = 0;
            RgbMidToneValue = 1;
            RgbMaxValue = 255;

            RedMinValue = 0;
            RedMidToneValue = 1;
            RedMaxValue = 255;

            GreenMinValue = 0;
            GreenMidToneValue = 1;
            GreenMaxValue = 255;

            BlueMinValue = 0;
            BlueMidToneValue = 1;
            BlueMaxValue = 255;


            m_RgbLevelsEffect = new RgbLevelsEffect();

            SetupEffectCategory(m_RgbLevelsEffect);

            m_propertyDescriptions = new Dictionary<string, PropertyDescription>();
            m_propertyDescriptions.Add("MinValue", new PropertyDescription(0, 255, 0));
            m_propertyDescriptions.Add("MidToneValue", new PropertyDescription(0.01, 9.9, 0));
            m_propertyDescriptions.Add("MaxValue", new PropertyDescription(0, 255, 0));

            AddEditors();           

        }

        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Windows.Foundation.Size sourceSize, Windows.Foundation.Size renderSize)
        {
            m_RgbLevelsEffect.Source = source;
            Levels levels = new Levels();
            levels.Min = RgbMinValue;
            levels.MidTone = RgbMidToneValue;
            levels.Max = RgbMaxValue;

            m_RgbLevelsEffect.Rgb = levels;

            levels.Min = RedMinValue;
            levels.MidTone = RedMidToneValue;
            levels.Max = RedMaxValue;

            m_RgbLevelsEffect.Red = levels;

            levels.Min = GreenMinValue;
            levels.MidTone = GreenMidToneValue;
            levels.Max = GreenMaxValue;

            m_RgbLevelsEffect.Green = levels;

            levels.Min = BlueMinValue;
            levels.MidTone = BlueMidToneValue;
            levels.Max = BlueMaxValue;

            m_RgbLevelsEffect.Blue = levels;

            return new MaybeTask<IImageProvider>(m_RgbLevelsEffect);
            
        }

        protected override void Dispose(bool disposing)
        {
            
        }

        public IReadOnlyDictionary<string, PropertyDescription> PropertyDescriptions
        {
            get
            {
                return m_propertyDescriptions;
            }
        }
    }
}
