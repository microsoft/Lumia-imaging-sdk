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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Lumia.Imaging.EditShowcase.ImageProcessors;
using System.Reflection;

#if NETFX_CORE
using Windows.UI.Xaml;
using Lumia.Imaging.EditShowcase.ViewModels;



#endif
namespace Lumia.Imaging.EditShowcase.Editors
{
    public class EditorViewModelBase : ViewModelBase
    {
        protected bool m_IsDirty;
        protected EffectProcessor m_EffectViewModel;
        public string PropertyName { get; protected set; }

        public string EditorTemplateKey { get; protected set; }
        
        protected EditorViewModelBase()
        {
            PropertyChanged += (s, e) => m_IsDirty = true;
        }

        protected EditorViewModelBase(EffectProcessor effectViewModel) : base()
        {
            m_EffectViewModel = effectViewModel;
        }

        protected object GetPropertyValue(string propertyName)
        {
            PropertyInfo propertyInfo = m_EffectViewModel.Effect.GetType().GetRuntimeProperties().Where(x => x.Name == propertyName).FirstOrDefault();
            if (propertyInfo == null)
                return 0;

            return propertyInfo.GetValue(m_EffectViewModel.Effect);
        }

      

        protected void SetPropertyValue(string propertyName, object value)
        {
            PropertyInfo propertyInfo = m_EffectViewModel.Effect.GetType().GetRuntimeProperties().Where(x => x.Name == propertyName).FirstOrDefault();
            if (propertyInfo == null)
                return;

            if (propertyInfo.PropertyType == typeof(double))
            {
                propertyInfo.SetValue(m_EffectViewModel.Effect, value);
            }
            else if (propertyInfo.PropertyType == typeof(int))
            {
                propertyInfo.SetValue(m_EffectViewModel.Effect, Convert.ToInt32(value));
            }
            else if (propertyInfo.PropertyType == typeof(bool))
            {
                propertyInfo.SetValue(m_EffectViewModel.Effect, Convert.ToBoolean(value));
            }
            else 
            {
                var typeInfo = propertyInfo.PropertyType.GetTypeInfo();
                if (typeInfo.BaseType == typeof(Enum))
                {
                    if (propertyInfo.SetMethod != null)
                    {
                        propertyInfo.SetValue(m_EffectViewModel.Effect, (Enum)(value));
                    }
                }
              
            }
        }

        public DataTemplate EditorTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(EditorTemplateKey))
                    return null;

                return Application.Current.Resources[EditorTemplateKey] as DataTemplate;
            }
        }

        public bool Apply()
        {
            if (!m_IsDirty)
                return false;

            DoApply();
            
            m_IsDirty = false;
            return true;
        }

        protected virtual void DoApply()
        {
        }

    }
}
