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

using namespace Concurrency;
using namespace Lumia::Imaging;
using namespace Lumia::Imaging::Adjustments;
using namespace Platform;
using namespace CustomNativeEffects;

Direct2DSaturationEffect::Direct2DSaturationEffect()
{
	
}

double Direct2DSaturationEffect::Level::get()
{
	return m_properties.m_level;
}

void Direct2DSaturationEffect::Level::set(double value)
{
	critical_section::scoped_lock lock(m_criticalSection);
	m_properties.m_level = value;
}


IImageProvider2^ Direct2DSaturationEffect::Clone()
{
	critical_section::scoped_lock lock(m_criticalSection);

	if (!m_source)
	{
		throw ref new NullReferenceException();
	}

	auto clone = ref new Direct2DSaturationEffect();
	clone->m_properties = m_properties;
	clone->m_source = m_source->Clone();
	return clone;
}

RenderOptions Direct2DSaturationEffect::SupportedRenderOptions::get()
{
	return RenderOptions::Gpu;
}

Workers::IImageWorker^ Direct2DSaturationEffect::CreateImageWorker(Workers::IImageWorkerRequest^ imageWorkerRequest)
{
	critical_section::scoped_lock lock(m_criticalSection);

	switch (imageWorkerRequest->RenderOptions)
	{
	case RenderOptions::Gpu:
		return ref new Direct2DSaturationEffectDirect2DWorker(this);
	default:
		return nullptr;
	}
}

IImageProvider^ Direct2DSaturationEffect::Source::get()
{
	concurrency::critical_section::scoped_lock lock(m_criticalSection);
	return m_source;
}

void Direct2DSaturationEffect::Source::set(IImageProvider^ value)
{
	concurrency::critical_section::scoped_lock lock(m_criticalSection);
	m_source = safe_cast<IImageProvider2^>(value);
}

uint32 Direct2DSaturationEffect::SourceCount::get()
{
	return 1;
}

void Direct2DSaturationEffect::GetSources(Platform::WriteOnlyArray<IImageProvider2^>^ sources)
{
	if (SourceCount > sources->Length)
	{
		throw ref new Platform::InvalidArgumentException("sources");
	}

	concurrency::critical_section::scoped_lock lock(m_criticalSection);

	sources[0] = m_source;
}

void Direct2DSaturationEffect::SetSource(uint32 sourceIndex, IImageProvider2^ source)
{
	if (sourceIndex >= SourceCount)
	{
		throw ref new Platform::InvalidArgumentException("sourceIndex");
	}

	concurrency::critical_section::scoped_lock lock(m_criticalSection);

	m_source = source;
}


