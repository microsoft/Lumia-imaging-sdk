//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
#include "pch.h"
#include "SplitToneDirect2DWorker.h"
#include "SplitToneEffect.h"
#include "SplitTonePixelShader.hlsl.h"
#include "SplitToneLookups.h"
#include <robuffer.h>

using namespace Lumia::Imaging::Adjustments;
using namespace Lumia::Imaging::Workers::Direct2D;
using namespace Microsoft::WRL;
using namespace Platform;
using namespace CustomNativeEffects;
using namespace Windows::Storage::Streams;

const GUID GUID_SplitToneShader = { 0x59820389, 0xbbd5, 0x40e8, 0x9a, 0xf2, 0x30, 0x9b, 0xb2, 0x94, 0xb3, 0x96 };

static void ThrowIfFailed(HRESULT hr)
{
	__abi_ThrowIfFailed(hr);
}

IBuffer^ CreateBufferFromArray(const uint8* data, uint32 dataLength)
{
	auto dataWriter = ref new DataWriter();
	ArrayReference<uint8, 1> shaderArray((uint8*)data, dataLength);
	dataWriter->WriteBytes(shaderArray);
	return dataWriter->DetachBuffer();
}

SplitToneDirect2DWorker::SplitToneDirect2DWorker(SplitToneEffect^ configuration) :
	m_configuration(configuration)
{
	m_pixelShaderBuffer = CreateBufferFromArray((const uint8*)g_main, ARRAYSIZE(g_main));
}

uint32 SplitToneDirect2DWorker::InputCount::get()
{
	return 1;
}

WSS::IBuffer^ SplitToneDirect2DWorker::PixelShader::get()
{
	return m_pixelShaderBuffer;
}

WSS::IBuffer^ SplitToneDirect2DWorker::PixelShaderConstantBuffer::get()
{
	return nullptr;
}

uint32 SplitToneDirect2DWorker::PixelShaderResourceTextureCount::get()
{
	return 1;
}

void SplitToneDirect2DWorker::GetPixelShaderResourceTextures(Platform::WriteOnlyArray<Platform::Object^>^ resourceTextures)
{
	SetupSplitToneBitmap();

	resourceTextures[0] = m_splitToneBitmap;
}

void SplitToneDirect2DWorker::GetPixelShaderResourceTextureOptions(Platform::WriteOnlyArray<Direct2DResourceTextureOptions>^ resourceTextureOptions)
{
	resourceTextureOptions[0] = { Direct2DFilter::Point, Direct2DBorderSampling::Clamp, Direct2DBorderSampling::Clamp };
}

void SplitToneDirect2DWorker::SetupSplitToneBitmap()
{
	if(m_splitToneBitmap)
		return;

	SplitToneLookups::LookupTable lookupTable;
	SplitToneLookups splitToneLookups;
	splitToneLookups.Generate(m_configuration->HighlightsHue, m_configuration->HighlightsSaturation, m_configuration->ShadowsHue, m_configuration->ShadowsSaturation, lookupTable);

	auto lookupsBuffer = CreateBufferFromArray((const uint8*)lookupTable.data(), sizeof(uint32) * lookupTable.size());

	m_splitToneBitmap = ref new Bitmap(Windows::Foundation::Size(256, 2), ColorMode::Bgra8888, 256 * sizeof(uint32), lookupsBuffer);
}

void SplitToneDirect2DWorker::Configuration::set(IImageProvider^ configuration)
{
	m_configuration = safe_cast<SplitToneEffect^>(configuration);
}

IImageProvider^ SplitToneDirect2DWorker::Configuration::get()
{
	return m_configuration;
}
