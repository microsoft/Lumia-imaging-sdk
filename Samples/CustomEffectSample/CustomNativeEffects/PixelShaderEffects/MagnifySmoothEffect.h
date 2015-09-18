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

#pragma warning(push)
#pragma warning(disable: 4973)

namespace CustomNativeEffects {

	using namespace Lumia::Imaging;

	public ref class MagnifySmoothEffect sealed : IImageProvider2, IImageConsumer2
	{
	internal:
		struct Properties final
		{
			Properties() :
				m_innerRadius(0.2),
				m_outerRadius(0.4),
				m_magnificationAmount(2.0),
				m_horizontalPosition(0.5),
				m_verticalPosition(0.5),
				m_aspectRatio(1.0)
			{

			}
			double m_innerRadius;
			double m_outerRadius;
			double m_magnificationAmount;
			double m_horizontalPosition;
			double m_verticalPosition;
			double m_aspectRatio;

		};

	public:
		MagnifySmoothEffect();

		property double AspectRatio
		{
			double get();
			void set(double value);
		}

		property double InnerRadius
		{
			double get();
			void set(double value);
		}

		property double OuterRadius
		{
			double get();
			void set(double value);
		}

		property double MagnificationAmount
		{
			double get();
			void set(double value);
		}

		property double HorizontalPosition
		{
			double get();
			void set(double value);
		}

		property double VerticalPosition
		{
			double get();
			void set(double value);
		}

		virtual property IImageProvider^ Source
		{
			IImageProvider^ get();
			void set(IImageProvider^ value);
		}

#pragma region IImageProvider implementation

		virtual Windows::Foundation::IAsyncAction^ PreloadAsync()
		{
			throw ref new Platform::NotImplementedException();
		}

		virtual Windows::Foundation::IAsyncOperation<Bitmap^>^ GetBitmapAsync(Bitmap^ bitmap, OutputOption outputOption)
		{
			UNREFERENCED_PARAMETER(bitmap);
			UNREFERENCED_PARAMETER(outputOption);
			throw ref new Platform::NotImplementedException();
		}

		virtual Windows::Foundation::IAsyncOperation<ImageProviderInfo^>^ GetInfoAsync()
		{
			throw ref new Platform::NotImplementedException();
		}

		virtual bool Lock(RenderRequest^ renderRequest)
		{
			UNREFERENCED_PARAMETER(renderRequest);
			throw ref new Platform::NotImplementedException();
		}

		virtual property uint32 SourceCount
		{
			uint32 get();
		}

		virtual void GetSources(Platform::WriteOnlyArray<IImageProvider2^>^ sources);

		virtual void SetSource(uint32 sourceIndex, IImageProvider2^ source);

		virtual property RenderOptions SupportedRenderOptions
		{
			RenderOptions get();
		}

		virtual IImageProvider2^ Clone();

		virtual Workers::IImageWorker^ CreateImageWorker(Workers::IImageWorkerRequest^ imageWorkerRequest);

#pragma endregion

	private:
		concurrency::critical_section m_criticalSection;
		IImageProvider2^ m_source;
		Properties m_properties;
	};
}

#pragma warning(pop)

