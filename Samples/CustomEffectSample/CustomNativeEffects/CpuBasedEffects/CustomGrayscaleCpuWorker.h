
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

#pragma once

#include "CustomGrayscaleEffect.h"
#include "Extras\CustomEffectCxBuffer.h"

namespace CustomNativeEffects {

	namespace {
		namespace MW = Microsoft::WRL;
		namespace LI = Lumia::Imaging;
		namespace LIWC = Lumia::Imaging::Workers::Cpu;
		namespace WF = Windows::Foundation;
		namespace WSS = Windows::Storage::Streams;
	}

	public ref class CustomGrayscaleCpuWorker sealed : LIWC::ICpuImageWorker
	{
	public:
		CustomGrayscaleCpuWorker(CustomGrayscaleEffect^ configuration);
		virtual ~CustomGrayscaleCpuWorker();

		virtual void Prepare(LIWC::CpuImageWorkerParameters parameters);

		virtual void Process(LIWC::CpuImageWorkerRectangle rectangle);

		virtual property LI::ColorMode ColorMode
		{
			LI::ColorMode get();
		}

		virtual property WSS::IBuffer^ SourceBuffer
		{
			WSS::IBuffer^ get();
		}

		virtual property WSS::IBuffer^ TargetBuffer
		{
			WSS::IBuffer^ get();
		}

		virtual property LI::IImageProvider^ Configuration
		{
			LI::IImageProvider^ get();
			void set(LI::IImageProvider^ value);
		}

	private:
		void UpdateParameters();

		CustomGrayscaleEffect^ m_configuration;
		LI::Extras::Detail::CustomEffectCxBuffer m_sourceBuffer;
		LI::Extras::Detail::CustomEffectCxBuffer m_targetBuffer;
		CustomGrayscaleEffect::Properties m_properties;
	};
}
