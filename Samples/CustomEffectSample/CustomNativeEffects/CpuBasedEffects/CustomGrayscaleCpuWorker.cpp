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
#include "CustomGrayscaleCpuWorker.h"


using namespace Lumia::Imaging;
using namespace Lumia::Imaging::Workers::Cpu;
using namespace Microsoft::WRL;
using namespace Platform;
using namespace CustomNativeEffects;
using namespace Windows::Foundation;
using namespace Windows::Storage::Streams;

CustomGrayscaleCpuWorker::CustomGrayscaleCpuWorker(CustomGrayscaleEffect^ configuration) :
	m_configuration(configuration)
{
}

CustomGrayscaleCpuWorker::~CustomGrayscaleCpuWorker()
{
}

void CustomGrayscaleCpuWorker::Prepare(CpuImageWorkerParameters parameters)
{
	m_sourceBuffer.EnsureCapacity(parameters.SourceBufferLength);
	m_targetBuffer.EnsureCapacity(parameters.TargetBufferLength);
}

void CustomGrayscaleCpuWorker::Process(CpuImageWorkerRectangle rectangle)
{
	int red = 0;
	int green = 0;
	int blue = 0;

	const uint32* sourcePixels = m_sourceBuffer.GetData() + rectangle.SourceStartIndex;
	uint32* targetPixels = m_targetBuffer.GetData();

	for(int y = 0; y < rectangle.Height; ++y)
	{
		for(int x = 0; x < rectangle.Width; ++x)
		{
			UINT32 pixel = sourcePixels[x];

			red = (pixel >> 16) & 0x000000FF;
			green = (pixel >> 8) & 0x000000FF;
			blue = (pixel)& 0x000000FF;
		
			int average = (int)(0.0722 * blue + 0.7152 * green + 0.2126 * red); // weighted average component
			targetPixels[x] = 0xff000000 | average | (average << 8) | (average << 16); // use average for each color component			
		}

		sourcePixels += rectangle.SourcePitch;
		targetPixels += rectangle.Width;
	}	
}

void CustomGrayscaleCpuWorker::Configuration::set(IImageProvider^ value)
{
	m_configuration = safe_cast<CustomGrayscaleEffect^>(value);
}

IImageProvider^ CustomGrayscaleCpuWorker::Configuration::get()
{
	return m_configuration;
}

IBuffer^ CustomGrayscaleCpuWorker::SourceBuffer::get()
{
	return m_sourceBuffer.GetBuffer();
}

IBuffer^ CustomGrayscaleCpuWorker::TargetBuffer::get()
{
	return m_targetBuffer.GetBuffer();
}

ColorMode CustomGrayscaleCpuWorker::ColorMode::get()
{
	return Lumia::Imaging::ColorMode::Bgra8888;
}
