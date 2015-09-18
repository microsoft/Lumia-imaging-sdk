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

namespace Lumia.Imaging.Extras.Effects.DepthOfField.Internal
{

	public class ChangeTracker<T> : IChangeTracker
	{
		private Func<T, bool> m_validator;
		private T m_value;
		private bool m_isDirty;

		public ChangeTracker()
			: this(x => true, default(T), true)
		{
		}

		public ChangeTracker(Func<T, bool> validator)
			: this(validator, default(T), true)
		{
		}

		public ChangeTracker(Func<T, bool> validator, T value)
			: this(validator, value, false)
		{
		}

		public ChangeTracker(T value, bool isDirty)
			: this(x => true, value, isDirty)
		{
		}

		public ChangeTracker(Func<T, bool> validator, T value, bool isDirty)
		{
			m_validator = validator;

			if (!isDirty)
			{
				Validate(value);
			}

			m_value = value;
			m_isDirty = isDirty;
		}

		public T Value
		{
			get
			{
				return m_value;
			}

			set
			{
				Validate(value);

				var hasChanged = m_value != null ? !m_value.Equals(value) : (value == null);

				if (hasChanged)
				{
					m_isDirty = true;
				}

				m_value = value;
			}
		}

		private void Validate(T value)
		{
			if (!m_validator(value))
			{
				throw new ArgumentException();
			}
		}

		public bool IsDirty
		{
			get { return m_isDirty; }
		}

		public void Reset()
		{
			m_isDirty = false;
		}
	}

}
