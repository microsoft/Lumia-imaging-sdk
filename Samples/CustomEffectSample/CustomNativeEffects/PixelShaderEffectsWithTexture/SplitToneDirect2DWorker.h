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

#include "SplitToneEffect.h"

namespace CustomNativeEffects {

	namespace {
		namespace MW = Microsoft::WRL;
		namespace LI = Lumia::Imaging;
		namespace LIWD = Lumia::Imaging::Workers::Direct2D;
		namespace LII = Lumia::Imaging::Interop;
		namespace WSS = Windows::Storage::Streams;
	}

	ref class SplitToneDirect2DWorker sealed : LIWD::IDirect2DPixelShaderImageWorker
	{
	internal:
		SplitToneDirect2DWorker(SplitToneEffect^ configuration);

	public:

#pragma region IDirect2DPixelShaderImageWorker implementation

		virtual property uint32 InputCount
		{
			uint32 get();
		}

		virtual property WSS::IBuffer^ PixelShader
		{
			WSS::IBuffer^ get();
		}

		virtual property WSS::IBuffer^ PixelShaderConstantBuffer
		{
			WSS::IBuffer^ get();
		}

		virtual property uint32 PixelShaderResourceTextureCount
		{
			uint32 get();
		}

		virtual void GetPixelShaderResourceTextures(Platform::WriteOnlyArray<Platform::Object^>^ resourceTextures);

		virtual void GetPixelShaderResourceTextureOptions(Platform::WriteOnlyArray<LIWD::Direct2DResourceTextureOptions>^ resourceTextureOptions);

		virtual property IImageProvider^ Configuration
		{
			IImageProvider^ get();
			void set(IImageProvider^ value);
		}

#pragma endregion

	private:
		void SetupSplitToneBitmap();

		SplitToneEffect^ m_configuration;
		Bitmap^ m_splitToneBitmap;
		WSS::IBuffer^ m_pixelShaderBuffer;
	};
}
