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

#include "pch.h"
#include "CustomEffectCxBuffer.h"

using namespace Lumia::Imaging::Extras::Detail;

static uint32* GetBufferData(Windows::Storage::Streams::IBufferByteAccess* bufferByteAccess);
static Microsoft::WRL::ComPtr<Windows::Storage::Streams::IBufferByteAccess> GetBufferByteAccess(IInspectable* buffer);

CustomEffectCxBuffer::CustomEffectCxBuffer() :
	m_bufferData(nullptr)
{
}

void CustomEffectCxBuffer::Clear()
{
	m_bufferData = nullptr;
	m_bufferByteAccess = nullptr;
	m_buffer = nullptr;
}

void CustomEffectCxBuffer::EnsureCapacity(uint32 requiredLength)
{
	if(!m_buffer || requiredLength > m_buffer->Capacity)
	{
		m_buffer = ref new Windows::Storage::Streams::Buffer(requiredLength);
		m_bufferByteAccess = GetBufferByteAccess(reinterpret_cast<IInspectable*>(m_buffer));
		m_bufferData = GetBufferData(m_bufferByteAccess.Get());
	}

	m_buffer->Length = requiredLength;
}

static uint32* GetBufferData(Windows::Storage::Streams::IBufferByteAccess* bufferByteAccess)
{
	uint32* bufferData = nullptr;

	__abi_ThrowIfFailed(
		bufferByteAccess->Buffer(reinterpret_cast<byte**>(&bufferData))
		);

	return bufferData;
}

static Microsoft::WRL::ComPtr<Windows::Storage::Streams::IBufferByteAccess> GetBufferByteAccess(IInspectable* buffer)
{
	Microsoft::WRL::ComPtr<Windows::Storage::Streams::IBufferByteAccess> bufferByteAccess;

	__abi_ThrowIfFailed(
		buffer->QueryInterface(IID_PPV_ARGS(&bufferByteAccess))
		);

	return bufferByteAccess;
}

