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
#include "SplitToneLookups.h"
#include "ImageProcessingUtils.h"

using namespace Lumia::Imaging::Adjustments;
using namespace CustomNativeEffects;
using namespace ImageProcessingUtils;

#define MIN_SHIFT 0
#define MAX_SHIFT 100


SplitToneLookups::SplitToneLookups()
{

}

void SplitToneLookups::FillChannelLookup(int* lookup, Curve^ negativeCurve, Curve^ positiveCurve, int color)
{
	auto negativeValues = negativeCurve->Values;
	auto positiveValues = positiveCurve->Values;

	for (int i = 0; i < 256; i++)
	{
		lookup[i] = i + (negativeValues[i] * (255 - color) + positiveValues[i] * color) / 255;
	}
}

Curve^ SplitToneLookups::CreatePositiveShadowsCurve()
{
	Curve^ curve = ref new Curve();
	curve->SetPoint(4, 18);
	curve->SetPoint(8, 29);
	curve->SetPoint(16, 45);
	curve->SetPoint(32, 75);
	curve->SetPoint(48, 105);
	curve->SetPoint(64, 132);
	curve->SetPoint(80, 157);
	curve->SetPoint(96, 178);
	curve->SetPoint(112, 194);
	curve->SetPoint(128, 206);
	curve->SetPoint(144, 213);
	curve->SetPoint(160, 217);
	curve->SetPoint(176, 220);
	curve->SetPoint(192, 223);
	curve->SetPoint(208, 226);
	curve->SetPoint(224, 233);
	return curve;
}

Curve^ SplitToneLookups::CreateNegativeShadowsCurve()
{
	Curve^ curve = ref new Curve();

	curve->SetPoint(8, 1);
	curve->SetPoint(16, 3);
	curve->SetPoint(32, 8);
	curve->SetPoint(48, 15);
	curve->SetPoint(64, 26);
	curve->SetPoint(80, 38);
	curve->SetPoint(96, 53);
	curve->SetPoint(112, 69);
	curve->SetPoint(128, 87);
	curve->SetPoint(144, 107);
	curve->SetPoint(160, 127);
	curve->SetPoint(176, 148);
	curve->SetPoint(192, 170);
	curve->SetPoint(208, 192);
	curve->SetPoint(224, 214);
	curve->SetPoint(240, 236);

	return curve;
}

Curve^ SplitToneLookups::CreatePositiveHighlightsCurve()
{
	Curve^ curve = ref new Curve();

	curve->SetPoint(12, 12);
	curve->SetPoint(16, 17);
	curve->SetPoint(32, 35);
	curve->SetPoint(48, 55);
	curve->SetPoint(64, 78);
	curve->SetPoint(80, 103);
	curve->SetPoint(96, 130);
	curve->SetPoint(112, 157);
	curve->SetPoint(128, 184);
	curve->SetPoint(144, 210);
	curve->SetPoint(152, 223);
	curve->SetPoint(160, 235);
	curve->SetPoint(168, 245);
	curve->SetPoint(176, 255);

	return curve;
}

Curve^ SplitToneLookups::CreateNegativeHighlightsCurve()
{
	Curve^ curve = ref new Curve();

	curve->SetPoint(16, 16);
	curve->SetPoint(32, 30);
	curve->SetPoint(48, 43);
	curve->SetPoint(64, 53);
	curve->SetPoint(80, 60);
	curve->SetPoint(96, 66);
	curve->SetPoint(112, 70);
	curve->SetPoint(128, 74);
	curve->SetPoint(144, 81);
	curve->SetPoint(160, 91);
	curve->SetPoint(176, 106);
	curve->SetPoint(192, 125);
	curve->SetPoint(208, 149);
	curve->SetPoint(224, 179);
	curve->SetPoint(240, 214);
	curve->SetPoint(248, 235);

	return curve;
}

void SplitToneLookups::Generate(int32 highlightsHue, int32 highlightsSaturation, int32 shadowsHue, int32 shadowsSaturation, LookupTable& lookupTable)
{	
	shadowsHue %= 360;
	highlightsHue %= 360;

	shadowsHue += (shadowsHue < 0) ? 360 : 0;
	highlightsHue += (highlightsHue < 0) ? 360 : 0;

	shadowsSaturation = SAT(shadowsSaturation, MIN_SHIFT, MAX_SHIFT);
	highlightsSaturation = SAT(highlightsSaturation, MIN_SHIFT, MAX_SHIFT);

	// Generate the curve lookup arrays
	auto highPositiveCurve = CreatePositiveHighlightsCurve();
	auto highNegativeCurve = CreateNegativeHighlightsCurve();
	auto lowPositiveCurve = CreatePositiveShadowsCurve();
	auto lowNegativeCurve = CreateNegativeShadowsCurve();

	auto identityCurve = ref new Curve();

	// Convert the values in the curve lookups into deltas
	Curve::Subtract(highPositiveCurve, identityCurve, highPositiveCurve);
	Curve::Subtract(highNegativeCurve, identityCurve, highNegativeCurve);
	Curve::Subtract(lowPositiveCurve, identityCurve, lowPositiveCurve);
	Curve::Subtract(lowNegativeCurve, identityCurve, lowNegativeCurve);

	// Adjust the curve lookups based on the highlights shift
	if (highlightsSaturation < 100)
	{
		Curve::Multiply(highPositiveCurve, highlightsSaturation / 100.0, highPositiveCurve);
		Curve::Multiply(highNegativeCurve, highlightsSaturation / 100.0, highNegativeCurve);
	}

	// Adjust the curve lookups based on the shadows shift
	if (shadowsSaturation < 100)
	{
		Curve::Multiply(lowPositiveCurve, shadowsSaturation / 100.0, lowPositiveCurve);
		Curve::Multiply(lowNegativeCurve, shadowsSaturation / 100.0, lowNegativeCurve);
	}

	// Extract the channel values of the shift colors
	int lowColor = HueToRgb(shadowsHue);
	int highColor = HueToRgb(highlightsHue);
	int highRed = (highColor >> 16) & 0x000000FF;
	int highGreen = (highColor >> 8) & 0x000000FF;
	int highBlue = highColor & 0x000000FF;
	int lowRed = (lowColor >> 16) & 0x000000FF;
	int lowGreen = (lowColor >> 8) & 0x000000FF;
	int lowBlue = lowColor & 0x000000FF;

	//// Generate the channel lookups
	FillChannelLookup(m_highReds, highNegativeCurve, highPositiveCurve, highRed);
	FillChannelLookup(m_highGreens, highNegativeCurve, highPositiveCurve, highGreen);
	FillChannelLookup(m_highBlues, highNegativeCurve, highPositiveCurve, highBlue);
	FillChannelLookup(m_lowReds, lowNegativeCurve, lowPositiveCurve, lowRed);
	FillChannelLookup(m_lowGreens, lowNegativeCurve, lowPositiveCurve, lowGreen);
	FillChannelLookup(m_lowBlues, lowNegativeCurve, lowPositiveCurve, lowBlue);

	// Turn the lookup arrays into arrays of deltas
	for (int i = 0; i < 256; i++)
	{
		auto lowR = m_lowReds[i] - i;
		auto lowG = m_lowGreens[i] - i;
		auto lowB = m_lowBlues[i] - i;
		auto highR = m_highReds[i] - i;
		auto highG = m_highGreens[i] - i;
		auto highB = m_highBlues[i] - i;

		// Since the 8 bit per color format is unsigned, bias by 128.
		// This will be subtracted in the shader.
		lowR = 128 + lowR;
		lowG = 128 + lowG;
		lowB = 128 + lowB;
		highR = 128 + highR;
		highG = 128 + highG;
		highB = 128 + highB;

		lowR = MIN(255, MAX(0, lowR));
		lowG = MIN(255, MAX(0, lowG));
		lowB = MIN(255, MAX(0, lowB));
		highR = MIN(255, MAX(0, highR));
		highG = MIN(255, MAX(0, highG));
		highB = MIN(255, MAX(0, highB));

		uint32 lowRgb = (lowR << 16) | (lowG << 8) | lowB;
		uint32 highRgb = (highR << 16) | (highG << 8) | highB;

		// add all values in array with offset
		lookupTable[i] = lowRgb;
		lookupTable[i + 256] = highRgb;
	}

}
