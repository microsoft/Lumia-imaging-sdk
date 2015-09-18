using Lumia.Imaging.EditShowcase.ImageProcessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lumia.Imaging.EditShowcase.Editors
{
    public class RangeEditorViewModelEx<T> : RangeEditorViewModel where T : IImageProvider
    {       
        private Func<T, double> m_GetValue;
        private Action<T, double> m_SetValue;
     
        public RangeEditorViewModelEx(string propertyName, double minimum, double maximum, EffectProcessor filterViewModel, Func<T, double> getValue, Action<T, double> setValue)
        {
            Minimum = minimum;
            Maximum = maximum;

            PropertyName = propertyName;

            m_EffectViewModel = filterViewModel;

            m_GetValue = getValue;
            m_SetValue = setValue;

            EditorTemplateKey = "RangeEditorTemplate";
            if (m_EffectViewModel.Effect != null)
            {
                m_Value = m_GetValue((T)m_EffectViewModel.Effect);
            }
        }      

        public override double Value
        {
            get { return m_Value; }
            set
            {
                SetProperty(ref m_Value, value);
            }
        }

        protected override void DoApply()
        {
            double validatedValue = Math.Min(Math.Max(m_Value, Minimum), Maximum);

            if (m_EffectViewModel.Effect != null)
            {
                m_SetValue((T)m_EffectViewModel.Effect, validatedValue);
            }
        }
    }
}
