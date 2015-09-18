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

#pragma once

#include <wrl.h>
#include <robuffer.h>

namespace Lumia { namespace Imaging { namespace Extras {

	namespace Detail {

		class CustomEffectCxBuffer final
		{
		public:
			CustomEffectCxBuffer();

			CustomEffectCxBuffer(const CustomEffectCxBuffer&) = delete;

			CustomEffectCxBuffer(CustomEffectCxBuffer&&) = delete;

			CustomEffectCxBuffer& operator=(const CustomEffectCxBuffer&) = delete;

			CustomEffectCxBuffer& operator=(CustomEffectCxBuffer&&) = delete;

			void Clear();

			void EnsureCapacity(uint32 requiredLength);

			Windows::Storage::Streams::IBuffer^ GetBuffer() const
			{
				return m_buffer;
			}

			uint32* GetData() const
			{
				return m_bufferData;
			}

			uint32 GetLength() const
			{
				return m_buffer->Length;
			}

		private:
			Windows::Storage::Streams::IBuffer^ m_buffer;
			Microsoft::WRL::ComPtr<Windows::Storage::Streams::IBufferByteAccess> m_bufferByteAccess;
			uint32* m_bufferData;
		};
	}

}}}
