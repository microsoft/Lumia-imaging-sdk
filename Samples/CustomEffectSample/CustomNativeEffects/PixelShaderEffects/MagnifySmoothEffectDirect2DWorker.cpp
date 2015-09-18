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
#include "MagnifySmoothEffect.h"
#include "MagnifySmoothEffectDirect2DWorker.h"
#include "MagnifySmooth.hlsl.h"


using namespace Lumia::Imaging::Adjustments;
using namespace Lumia::Imaging::Workers::Direct2D;
using namespace Lumia::Imaging::Interop;
using namespace Microsoft::WRL;
using namespace Platform;
using namespace Lumia::Imaging;
using namespace CustomNativeEffects;
using namespace Windows::Storage::Streams;


// {7438B2C0-A722-4D47-BBF3-44040D5C11A9}
static const GUID GUID_MagnifySmoothShader =
{ 0x7438b2c0, 0xa722, 0x4d47,{ 0xbb, 0xf3, 0x44, 0x4, 0xd, 0x5c, 0x11, 0xa9 } };

static void ThrowIfFailed(HRESULT hr)
{
	__abi_ThrowIfFailed(hr);
}

MagnifySmoothEffectDirect2DWorker::MagnifySmoothEffectDirect2DWorker(MagnifySmoothEffect^ configuration) :
	m_configuration(configuration)
{

}

void MagnifySmoothEffectDirect2DWorker::Initialize(Platform::IntPtr effectContextUnk, Platform::IntPtr transformGraphUnk)
{
	m_effectContext = reinterpret_cast<ID2D1EffectContext*>(static_cast<void*>(effectContextUnk));
	m_transformGraph = reinterpret_cast<ID2D1TransformGraph*>(static_cast<void*>(transformGraphUnk));

	ThrowIfFailed(
		m_effectContext->LoadPixelShader(GUID_MagnifySmoothShader, g_D2D_ENTRY, ARRAYSIZE(g_D2D_ENTRY))
		);

	m_shaderDrawTransform = Direct2DShaderDrawTransformFactory::CreateShaderDrawTransform(1, GUID_MagnifySmoothShader, Direct2DPixelOptions::TrivialSampling);

	// IShaderDrawTransform implements the COM interface ID2D1TransformNode. 
	// Set it up as the only transform node in the transform graph.
	ComPtr<IUnknown> shaderDrawTransformUnknown = reinterpret_cast<IUnknown*>(m_shaderDrawTransform);

	ThrowIfFailed(
		shaderDrawTransformUnknown.As(&m_shaderDrawTransformNode)
		);

	m_transformGraph->SetSingleTransformNode(m_shaderDrawTransformNode.Get());
}

void MagnifySmoothEffectDirect2DWorker::PrepareForRender(uint32 changeType)
{
	if (changeType == D2D1_CHANGE_TYPE_NONE)
	{
		return;
	}

	m_constantBuffer.AspectRatio = safe_cast<float>(m_configuration->AspectRatio);
	m_constantBuffer.HorizontalPosition = safe_cast<float>(m_configuration->HorizontalPosition);
	m_constantBuffer.InnerRadius = safe_cast<float>(m_configuration->InnerRadius);
	m_constantBuffer.MagnificationAmount = safe_cast<float>(m_configuration->MagnificationAmount);
	m_constantBuffer.OuterRadius = safe_cast<float>(m_configuration->OuterRadius);
	m_constantBuffer.VerticalPosition = safe_cast<float>(m_configuration->VerticalPosition);


	ComPtr<ID2D1DrawInfo> d2dDrawInfo;

	ThrowIfFailed(
		reinterpret_cast<IInspectable*>(m_shaderDrawTransform->DrawInfo)->QueryInterface(IID_PPV_ARGS(&d2dDrawInfo))
		);

	d2dDrawInfo->SetPixelShaderConstantBuffer(reinterpret_cast<const BYTE*>(&m_constantBuffer), sizeof(m_constantBuffer));
}

void MagnifySmoothEffectDirect2DWorker::SetGraph(Platform::IntPtr transformGraphUnk)
{
	UNREFERENCED_PARAMETER(transformGraphUnk);
}


uint32 MagnifySmoothEffectDirect2DWorker::InputCount::get()
{
	return 1;
}

void MagnifySmoothEffectDirect2DWorker::Configuration::set(IImageProvider^ configuration)
{
	m_configuration = safe_cast<MagnifySmoothEffect^>(configuration);
}

IImageProvider^ MagnifySmoothEffectDirect2DWorker::Configuration::get()
{
	return m_configuration;
}

