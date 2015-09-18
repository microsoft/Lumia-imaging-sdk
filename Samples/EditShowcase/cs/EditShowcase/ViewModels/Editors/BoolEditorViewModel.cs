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
using Lumia.Imaging.EditShowcase.Editors;
using System;
using System.Collections.Generic;
using System.Text;
using Lumia.Imaging;

namespace Lumia.Imaging.EditShowcase.ViewModels.Editors
{
    public class BoolEditorViewModel : EditorViewModelBase
    {
        private bool m_Value;       
    
        public BoolEditorViewModel(string propertyName, EffectProcessor effectViewModel) : base(effectViewModel)
        {
            PropertyName = propertyName;
            m_EffectViewModel = effectViewModel;         

            EditorTemplateKey = "BoolEditorTemplate";
            if (m_EffectViewModel.Effect != null)
            {
                m_Value = Convert.ToBoolean(GetPropertyValue(PropertyName));
            }
        }

        public string EffectName
        {
            get { return m_EffectViewModel.Name; }
        }

        public bool Value
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