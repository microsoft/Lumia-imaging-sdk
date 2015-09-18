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
using Lumia.Imaging.EditShowcase.Editors;
using Lumia.Imaging.Extras;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using Lumia.Imaging.EditShowcase.ViewModels.Editors;
using System.Collections.Generic;

namespace Lumia.Imaging.EditShowcase.ImageProcessors
{
    public class EffectProcessor : ImageProcessor
    {
        private IImageProvider m_effect;
        private bool m_useCustomRangeEditor;

        public EffectProcessor()
        {
            CanRenderAtPreviewSize = true;
            Effect = this;
        }

        public EffectProcessor(string name, IImageProvider effect, bool canRenderAtPreviewSize = true, bool useCustomRangeEditor = false)
        {
            m_useCustomRangeEditor = useCustomRangeEditor;
            CanRenderAtPreviewSize = canRenderAtPreviewSize;
            Name = name;
            Effect = effect;          
            AddEditors();
        }

        protected virtual void AddEditors()
        {
            AddBooleanEditors();

            AddEnumEditors();

            AddRangeEditors();
        }

        public override void RestoreDefaultValues()
        {
            var editors = Editors.OfType<RangeEditorViewModel>();
            foreach(var editor in editors)
            {
                editor.RestoreDefaultValues();
            }
        }

        protected virtual void AddEnumEditors()
        {
            var propertyInfos = Effect.GetType().GetRuntimeProperties();
            if (propertyInfos == null)
                return;

            foreach (var propertyInfo in propertyInfos)
            {
                if (propertyInfo.PropertyType == null)
                    continue;

                TypeInfo typeInfo = propertyInfo.PropertyType.GetTypeInfo();
                if (typeInfo == null)
                    continue;

                if (typeInfo.BaseType == typeof(System.Enum) && propertyInfo.PropertyType != typeof(RenderOptions) && propertyInfo.PropertyType != typeof(EffectCategoryEnum))
                {
                    Editors.Add(new EnumEditorViewModel(propertyInfo.Name, this, propertyInfo.PropertyType));
                }
            }
        }
        protected virtual void AddBooleanEditors()
        {           
            var propertyInfos = Effect.GetType().GetRuntimeProperties().Where(x => x.PropertyType == typeof(bool));
            if (propertyInfos == null)
                return;

            foreach (var propertyInfo in propertyInfos)
            {
                if (propertyInfo.Name != "CanRenderAtPreviewSize")
                {
                    Editors.Add(new BoolEditorViewModel(propertyInfo.Name, this));
                }               
            }
        }

        protected virtual  void AddRangeEditors()
        {
            if (m_useCustomRangeEditor)
                return;

            var propertyDescriptions = Effect as IPropertyDescriptions;
            if (propertyDescriptions == null)
                return;

            foreach (string propertyName in propertyDescriptions.PropertyDescriptions.Keys)
            {
                PropertyDescription propertyDescription;
                propertyDescriptions.PropertyDescriptions.TryGetValue(propertyName, out propertyDescription);

               
                Editors.Add(new RangeEditorViewModel(propertyName, propertyDescription, this));               

              
            }
        }

        public IImageProvider Effect
        {
            get
            {
                return m_effect;
            }
            protected set
            {
                m_effect = value;
                SetupEffectCategory(value);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        protected override MaybeTask<IImageProvider> GetEffectInternalAsync(IImageProvider source, Windows.Foundation.Size sourceSize, Windows.Foundation.Size renderSize)
        {
            IImageConsumer imageConsumer = Effect as IImageConsumer;
            if (imageConsumer != null)
            {
                imageConsumer.Source = source;
            }
            return new MaybeTask<IImageProvider>(Effect);
        }

        protected override void Dispose(bool disposing)
        {

        }
    }
}
