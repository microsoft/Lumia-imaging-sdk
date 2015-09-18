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
#include "CustomGrayscaleEffect.h"
#include "CustomGrayscaleCpuWorker.h"

using namespace CustomNativeEffects;
using namespace Platform;
using namespace Lumia::Imaging;
using namespace Concurrency;
using namespace Lumia::Imaging::Adjustments;

CustomGrayscaleEffect::CustomGrayscaleEffect()
{
}

IImageProvider2^ CustomGrayscaleEffect::Clone()
{
	critical_section::scoped_lock lock(m_criticalSection);

	auto clone = ref new CustomGrayscaleEffect();
	clone->m_properties = m_properties;
	clone->m_source = m_source->Clone();
	return clone;
}

RenderOptions CustomGrayscaleEffect::SupportedRenderOptions::get()
{
	return RenderOptions::Cpu;
}

Workers::IImageWorker^ CustomGrayscaleEffect::CreateImageWorker(Workers::IImageWorkerRequest^ imageWorkerRequest)
{
	if(imageWorkerRequest->RenderOptions == RenderOptions::Cpu)
	{
		return ref new CustomGrayscaleCpuWorker(this);
	}

	return nullptr;
}

IImageProvider^ CustomGrayscaleEffect::Source::get()
{
	concurrency::critical_section::scoped_lock lock(m_criticalSection);
	return m_source;
}

void CustomGrayscaleEffect::Source::set(IImageProvider^ value)
{
	concurrency::critical_section::scoped_lock lock(m_criticalSection);
	m_source = safe_cast<IImageProvider2^>(value);
}

uint32 CustomGrayscaleEffect::SourceCount::get()
{
	return 1;
}

void CustomGrayscaleEffect::GetSources(Platform::WriteOnlyArray<IImageProvider2^>^ sources)
{
	if(SourceCount > sources->Length)
	{
		throw ref new Platform::InvalidArgumentException("sources");
	}

	concurrency::critical_section::scoped_lock lock(m_criticalSection);

	sources[0] = m_source;
}

void CustomGrayscaleEffect::SetSource(uint32 sourceIndex, IImageProvider2^ source)
{
	if(sourceIndex >= SourceCount)
	{
		throw ref new Platform::InvalidArgumentException("sourceIndex");
	}

	concurrency::critical_section::scoped_lock lock(m_criticalSection);

	m_source = source;
}
