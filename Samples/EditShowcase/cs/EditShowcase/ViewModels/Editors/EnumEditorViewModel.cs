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
using System.Linq;
using Lumia.Imaging;
using System.Reflection;

namespace Lumia.Imaging.EditShowcase.ViewModels.Editors
{
    public class EnumEditorViewModel: EditorViewModelBase
    {
        private Enum m_Value;    
        public Array EnumValues { get; private set; }

        // Replacement for Enum.GetValues, to work around VS2015 .NET Native reflection bug.  
        static object[] GetEnumValues(Type type)
        {
            var values = from field in type.GetTypeInfo().DeclaredFields
                         where field.IsStatic
                         orderby field.GetValue(null)
                         select field.GetValue(null);

            return values.ToArray();
        }

        public EnumEditorViewModel(string propertyName, EffectProcessor effectViewModel, Type enumType)
        {
            m_EffectViewModel = effectViewModel;
            PropertyName = propertyName;

            EnumValues = GetEnumValues(enumType);
  
            if (m_EffectViewModel.Effect != null)
            {
                m_Value = (Enum)GetPropertyValue(propertyName);
            }

            EditorTemplateKey = "EnumEditorTemplate";
        }

        public string EffectName { get { return m_EffectViewModel.Name; } }

        public Enum Value
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
    //public class EnumEditorViewModel<T, T2> : EditorViewModelBase where T : IImageProvider
    //{
    //    private T2 m_Value;
    //    private Func<T, T2> m_GetValue;
    //    private Action<T, T2> m_SetValue;

    //    public IEnumerable<T2> EnumValues { get; private set; }

    //    public EnumEditorViewModel(string propertyName, EffectProcessor effectViewModel, Func<T, T2> getValue, Action<T, T2> setValue)
    //    {
    //        m_EffectViewModel = effectViewModel;
    //        PropertyName = propertyName;

    //        EnumValues = new List<T2>(Enum.GetValues(typeof(T2)).Cast<T2>());

    //        m_GetValue = getValue;
    //        m_SetValue = setValue;

    //        if (m_EffectViewModel.Effect != null)
    //        {
    //            m_Value = m_GetValue((T)m_EffectViewModel.Effect);
    //        }

    //        EditorTemplateKey = "EnumEditorTemplate";
    //    }

    //    public string EffectName { get { return m_EffectViewModel.Name; } }

    //    public T2 Value
    //    {
    //        get { return m_Value; }
    //        set
    //        {
    //            SetProperty(ref m_Value, value);
    //        }
    //    }

    //    protected override void DoApply()
    //    {
    //        if (m_EffectViewModel.Effect != null)
    //        {
    //            m_SetValue((T)m_EffectViewModel.Effect, Value);
    //        }            
    //    }
    //}
}

