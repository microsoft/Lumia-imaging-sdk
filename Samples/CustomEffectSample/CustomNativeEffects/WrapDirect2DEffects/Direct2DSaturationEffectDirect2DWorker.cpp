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
#include "Direct2DSaturationEffect.h"
#include "Direct2DSaturationEffectDirect2DWorker.h"


using namespace Lumia::Imaging::Adjustments;
using namespace Lumia::Imaging::Workers::Direct2D;
using namespace Lumia::Imaging::Interop;
using namespace Microsoft::WRL;
using namespace Platform;
using namespace Lumia::Imaging;
using namespace CustomNativeEffects;
using namespace Windows::Storage::Streams;

static void ThrowIfFailed(HRESULT hr)
{
	__abi_ThrowIfFailed(hr);
}

Direct2DSaturationEffectDirect2DWorker::Direct2DSaturationEffectDirect2DWorker(Direct2DSaturationEffect^ configuration) :
	m_configuration(configuration)
{

}

void Direct2DSaturationEffectDirect2DWorker::Initialize(Platform::IntPtr effectContextUnk, Platform::IntPtr transformGraphUnk)
{
	m_effectContext = reinterpret_cast<ID2D1EffectContext*>(static_cast<void*>(effectContextUnk));
	m_transformGraph = reinterpret_cast<ID2D1TransformGraph*>(static_cast<void*>(transformGraphUnk));

	ThrowIfFailed(
		m_effectContext->CreateEffect(CLSID_D2D1Saturation, &m_d2dSaturationEffect)
		);

	ThrowIfFailed(
		m_d2dSaturationEffect->SetValue(D2D1_SATURATION_PROP_SATURATION, 0.8f)
		);

	ThrowIfFailed(
		m_effectContext->CreateTransformNodeFromEffect(m_d2dSaturationEffect.Get(), &m_shaderDrawTransformNode)
		);	

	m_transformGraph->SetSingleTransformNode(m_shaderDrawTransformNode.Get());
}

void Direct2DSaturationEffectDirect2DWorker::PrepareForRender(uint32 changeType)
{
	if (changeType == D2D1_CHANGE_TYPE_NONE)
	{
		return;
	}	

	ThrowIfFailed(
		m_d2dSaturationEffect->SetValue(D2D1_SATURATION_PROP_SATURATION, (float)m_configuration->Level)
		);
}

void Direct2DSaturationEffectDirect2DWorker::SetGraph(Platform::IntPtr transformGraphUnk)
{
	UNREFERENCED_PARAMETER(transformGraphUnk);
}


uint32 Direct2DSaturationEffectDirect2DWorker::InputCount::get()
{
	return 1;
}

void Direct2DSaturationEffectDirect2DWorker::Configuration::set(IImageProvider^ configuration)
{
	m_configuration = safe_cast<Direct2DSaturationEffect^>(configuration);
}

IImageProvider^ Direct2DSaturationEffectDirect2DWorker::Configuration::get()
{
	return m_configuration;
}

