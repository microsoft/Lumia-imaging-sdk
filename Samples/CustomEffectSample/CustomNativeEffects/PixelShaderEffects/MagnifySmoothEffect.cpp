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

using namespace Concurrency;
using namespace Lumia::Imaging;
using namespace Lumia::Imaging::Adjustments;
using namespace Platform;
using namespace CustomNativeEffects;

MagnifySmoothEffect::MagnifySmoothEffect()
{
	InnerRadius = 0.2;
	OuterRadius = 0.4;
	MagnificationAmount = 2.0;
	HorizontalPosition = 0.3;
	VerticalPosition = 0.3;
	AspectRatio = 1.0;
}

double MagnifySmoothEffect::OuterRadius::get()
{
	return m_properties.m_outerRadius;
}

void MagnifySmoothEffect::OuterRadius::set(double value)
{
	critical_section::scoped_lock lock(m_criticalSection);
	m_properties.m_outerRadius = value;
}

double MagnifySmoothEffect::InnerRadius::get()
{
	return m_properties.m_innerRadius;
}

void MagnifySmoothEffect::InnerRadius::set(double value)
{
	critical_section::scoped_lock lock(m_criticalSection);
	m_properties.m_innerRadius = value;
}

double MagnifySmoothEffect::MagnificationAmount::get()
{
	return m_properties.m_magnificationAmount;
}

void MagnifySmoothEffect::MagnificationAmount::set(double value)
{
	critical_section::scoped_lock lock(m_criticalSection);
	m_properties.m_magnificationAmount = value;
}

double MagnifySmoothEffect::HorizontalPosition::get()
{
	return m_properties.m_horizontalPosition;
}

void MagnifySmoothEffect::HorizontalPosition::set(double value)
{
	critical_section::scoped_lock lock(m_criticalSection);
	m_properties.m_horizontalPosition = value;
}

double MagnifySmoothEffect::VerticalPosition::get()
{
	return m_properties.m_verticalPosition;
}

void MagnifySmoothEffect::VerticalPosition::set(double value)
{
	critical_section::scoped_lock lock(m_criticalSection);
	m_properties.m_verticalPosition = value;
}

double MagnifySmoothEffect::AspectRatio::get()
{
	return m_properties.m_aspectRatio;
}

void MagnifySmoothEffect::AspectRatio::set(double value)
{
	critical_section::scoped_lock lock(m_criticalSection);
	m_properties.m_aspectRatio = value;
}

IImageProvider2^ MagnifySmoothEffect::Clone()
{
	critical_section::scoped_lock lock(m_criticalSection);

	if (!m_source)
	{
		throw ref new NullReferenceException();
	}

	auto clone = ref new MagnifySmoothEffect();
	clone->m_properties = m_properties;
	clone->m_source = m_source->Clone();
	return clone;
}

RenderOptions MagnifySmoothEffect::SupportedRenderOptions::get()
{
	return RenderOptions::Gpu;
}

Workers::IImageWorker^ MagnifySmoothEffect::CreateImageWorker(Workers::IImageWorkerRequest^ imageWorkerRequest)
{
	critical_section::scoped_lock lock(m_criticalSection);

	switch (imageWorkerRequest->RenderOptions)
	{
	case RenderOptions::Gpu:
		return ref new MagnifySmoothEffectDirect2DWorker(this);
	default:
		return nullptr;
	}
}

IImageProvider^ MagnifySmoothEffect::Source::get()
{
	concurrency::critical_section::scoped_lock lock(m_criticalSection);
	return m_source;
}

void MagnifySmoothEffect::Source::set(IImageProvider^ value)
{
	concurrency::critical_section::scoped_lock lock(m_criticalSection);
	m_source = safe_cast<IImageProvider2^>(value);
}

uint32 MagnifySmoothEffect::SourceCount::get()
{
	return 1;
}

void MagnifySmoothEffect::GetSources(Platform::WriteOnlyArray<IImageProvider2^>^ sources)
{
	if (SourceCount > sources->Length)
	{
		throw ref new Platform::InvalidArgumentException("sources");
	}

	concurrency::critical_section::scoped_lock lock(m_criticalSection);

	sources[0] = m_source;
}

void MagnifySmoothEffect::SetSource(uint32 sourceIndex, IImageProvider2^ source)
{
	if (sourceIndex >= SourceCount)
	{
		throw ref new Platform::InvalidArgumentException("sourceIndex");
	}

	concurrency::critical_section::scoped_lock lock(m_criticalSection);

	m_source = source;
}

