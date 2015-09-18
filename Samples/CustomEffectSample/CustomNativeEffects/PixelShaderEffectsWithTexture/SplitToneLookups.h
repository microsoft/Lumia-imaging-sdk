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

namespace CustomNativeEffects {

	class SplitToneLookups final
	{
	public:

		SplitToneLookups();
		
		typedef std::array<uint32, 2*256> LookupTable;
	    void Generate(_In_ const int32 highlightsHue, int32 highlightsSaturation, int32 shadowsHue, int32 shadowsSaturation, LookupTable& lookupTable);
	
	private:
		 void FillChannelLookup(int* lookup, Lumia::Imaging::Adjustments::Curve^ negativeCurve, Lumia::Imaging::Adjustments::Curve^ positiveCurve, int color);
		 Lumia::Imaging::Adjustments::Curve^ CreatePositiveShadowsCurve();
		 Lumia::Imaging::Adjustments::Curve^ CreateNegativeShadowsCurve();
		 Lumia::Imaging::Adjustments::Curve^ CreatePositiveHighlightsCurve();
		 Lumia::Imaging::Adjustments::Curve^ CreateNegativeHighlightsCurve();

		 int m_highReds[256];
		 int m_highGreens[256];
		 int m_highBlues[256];
		 int m_lowReds[256];
		 int m_lowGreens[256];
		 int m_lowBlues[256];	
	};
}
