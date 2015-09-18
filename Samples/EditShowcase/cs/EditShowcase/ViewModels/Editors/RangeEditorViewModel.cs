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

using Lumia.Imaging.EditShowcase.ImageProcessors;
using Lumia.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumia.Imaging.EditShowcase.Editors;
using System.Reflection;
using Lumia.Imaging.Artistic;
using Lumia.Imaging.Transforms;

namespace Lumia.Imaging.EditShowcase.Editors
{
    public class RangeEditorViewModel : EditorViewModelBase
    {
        protected double m_Value;

        private PropertyDescription m_propertyDescription;

        public RangeEditorViewModel() { }

        public RangeEditorViewModel(string propertyName, PropertyDescription propertyDescription, EffectProcessor effectViewModel) : base(effectViewModel)
        {
            m_propertyDescription = propertyDescription;
            PropertyName = propertyName;

            Minimum = propertyDescription.MinValue != null ? Convert.ToDouble(propertyDescription.MinValue) : 0;            
            Maximum = AdjustMaximumValueIfNeeded(propertyDescription.MaxValue);

            EditorTemplateKey = "RangeEditorTemplate";
            if (m_EffectViewModel.Effect != null)
            {
                m_Value = Convert.ToDouble(GetPropertyValue(PropertyName));
            }
        }

        private double AdjustMaximumValueIfNeeded(object value)
        {
            if (value == null)
            {
                if (PropertyName == "RotationAngle" && m_EffectViewModel.Effect != null && m_EffectViewModel.Effect.GetType() == typeof(RotationEffect))
                    return 360;

                return 10;
            }
            if (PropertyName == "TransitionSize" && m_EffectViewModel.Effect != null && m_EffectViewModel.Effect.GetType() == typeof(VignettingEffect))
                return 1.5;

            return Convert.ToDouble(value);

        }

        internal void RestoreDefaultValues()
        {
            if (m_propertyDescription == null)
                return;

            SetPropertyValue(PropertyName, m_propertyDescription.DefaultValue);
            Value = Convert.ToDouble(GetPropertyValue(PropertyName));
        }

        public string EffectName { get { return m_EffectViewModel.Name; } }

        public double Minimum { get; protected set; }

        public double Maximum { get; protected set; }

        public virtual double Value
        {
            get { return m_Value; }
            set
            {
                m_IsDirty = true;
                SetProperty(ref m_Value, value);
            }
        }

        protected override void DoApply()
        {
            if (m_EffectViewModel.Effect != null)
            {
                SetPropertyValue(PropertyName, Value);
            }
        }
    }
}

