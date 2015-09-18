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

#include "MagnifySmoothEffect.h"

namespace CustomNativeEffects {

	namespace {
		namespace MW = Microsoft::WRL;
		namespace LI = Lumia::Imaging;
		namespace LIWD = Lumia::Imaging::Workers::Direct2D;
		namespace LII = Lumia::Imaging::Interop;
		namespace WSS = Windows::Storage::Streams;
	}


	// Interop version of the ID2D1EffectImpl interface.
	//
	[Platform::Runtime::InteropServices::InterfaceType(Platform::Runtime::InteropServices::ComInterfaceType::InterfaceIsIUnknown)]
	[uuid(a248fd3f-3e6c-4e63-9f03-7f68ecc91db9)]
	public interface class ID2D1EffectImpl
	{
		void Initialize(Platform::IntPtr effectContext, Platform::IntPtr transformGraph);
		void PrepareForRender(uint32 changeType);
		void SetGraph(Platform::IntPtr transformGraph);
	};

	ref class MagnifySmoothEffectDirect2DWorker sealed : LIWD::IDirect2DImageWorker, ID2D1EffectImpl
	{
	public:
		MagnifySmoothEffectDirect2DWorker(MagnifySmoothEffect^ configuration);

#pragma region ID2D1EffectImpl implementation

		virtual void Initialize(Platform::IntPtr effectContext, Platform::IntPtr transformGraph);

		virtual void PrepareForRender(uint32 changeType);

		virtual void SetGraph(Platform::IntPtr transformGraph);

#pragma endregion

#pragma region IDirect2DNativeEffectWorker implementation

		virtual property uint32 InputCount
		{
			uint32 get();
		}

		virtual property IImageProvider^ Configuration
		{
			IImageProvider^ get();
			void set(IImageProvider^ value);
		}

#pragma endregion

	private:
		struct
		{
			float InnerRadius;
			float OuterRadius;
			float MagnificationAmount;
			float HorizontalPosition;
			float VerticalPosition;
			float AspectRatio;		

		} m_constantBuffer;

		MagnifySmoothEffect^ m_configuration;
		MW::ComPtr<ID2D1EffectContext> m_effectContext;
		MW::ComPtr<ID2D1TransformGraph> m_transformGraph;
		LIWD::IDirect2DShaderDrawTransform^ m_shaderDrawTransform;
		MW::ComPtr<ID2D1TransformNode> m_shaderDrawTransformNode;
	
	};
}

