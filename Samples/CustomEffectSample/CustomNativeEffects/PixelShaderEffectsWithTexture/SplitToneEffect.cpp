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
#include "SplitToneEffect.h"
#include "SplitToneDirect2DWorker.h"

using namespace CustomNativeEffects;
using namespace Platform;
using namespace Lumia::Imaging;
using namespace Concurrency;
using namespace Lumia::Imaging::Adjustments;

SplitToneEffect::SplitToneEffect()
{
}

int32 SplitToneEffect::HighlightsHue::get()
{
	critical_section::scoped_lock lock(m_criticalSection);
	return m_properties.m_highHue;
}

void SplitToneEffect::HighlightsHue::set(int32 value)
{
	critical_section::scoped_lock lock(m_criticalSection);
	m_properties.m_highHue = value;
}

int32 SplitToneEffect::HighlightsSaturation::get()
{
	critical_section::scoped_lock lock(m_criticalSection);
	return m_properties.m_highShift;
}

void SplitToneEffect::HighlightsSaturation::set(int32 value)
{
	if(value < MinSaturation || value > MaxSaturation)
	{
		throw ref new Platform::InvalidArgumentException("HighlightsSaturation");
	}

	critical_section::scoped_lock lock(m_criticalSection);
	m_properties.m_highShift = value;
}

int32 SplitToneEffect::ShadowsHue::get()
{
	critical_section::scoped_lock lock(m_criticalSection);
	return m_properties.m_lowHue;
}

void SplitToneEffect::ShadowsHue::set(int32 value)
{
	critical_section::scoped_lock lock(m_criticalSection);
	m_properties.m_lowHue = value;
}

int32 SplitToneEffect::ShadowsSaturation::get()
{
	critical_section::scoped_lock lock(m_criticalSection);
	return m_properties.m_lowShift;
}

void SplitToneEffect::ShadowsSaturation::set(int32 value)
{
	if(value < MinSaturation || value > MaxSaturation)
	{
		throw ref new Platform::InvalidArgumentException("ShadowsSaturation");
	}

	critical_section::scoped_lock lock(m_criticalSection);
	m_properties.m_lowShift = value;
}

IImageProvider2^ SplitToneEffect::Clone()
{
	critical_section::scoped_lock lock(m_criticalSection);

	auto clone = ref new SplitToneEffect();
	clone->m_properties = m_properties; 
	clone->m_source = m_source->Clone();
	return clone;
}

RenderOptions SplitToneEffect::SupportedRenderOptions::get()
{
	return RenderOptions::Gpu;
}

Workers::IImageWorker^ SplitToneEffect::CreateImageWorker(Workers::IImageWorkerRequest^ imageWorkerRequest)
{
	critical_section::scoped_lock lock(m_criticalSection);

	switch(imageWorkerRequest->RenderOptions)
	{
	
	case RenderOptions::Gpu:
		return ref new SplitToneDirect2DWorker(this);
	default:
		return nullptr;
	}
}

IImageProvider^ SplitToneEffect::Source::get()
{
	concurrency::critical_section::scoped_lock lock(m_criticalSection);
	return m_source;
}

void SplitToneEffect::Source::set(IImageProvider^ value)
{
	concurrency::critical_section::scoped_lock lock(m_criticalSection);
	m_source = safe_cast<IImageProvider2^>(value);
}

uint32 SplitToneEffect::SourceCount::get()
{
	return 1;
}

void SplitToneEffect::GetSources(Platform::WriteOnlyArray<IImageProvider2^>^ sources)
{
	if(SourceCount > sources->Length)
	{
		throw ref new Platform::InvalidArgumentException("sources");
	}

	concurrency::critical_section::scoped_lock lock(m_criticalSection);

	sources[0] = m_source;
}

void SplitToneEffect::SetSource(uint32 sourceIndex, IImageProvider2^ source)
{
	if(sourceIndex >= SourceCount)
	{
		throw ref new Platform::InvalidArgumentException("sourceIndex");
	}

	concurrency::critical_section::scoped_lock lock(m_criticalSection);

	m_source = source;
}
