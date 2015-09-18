#define D2D_INPUT_COUNT 1 
#define D2D_INPUT0_SIMPLE 

#include "d2d1effecthelpers.hlsli"


cbuffer constants : register(b0)
{
	float InnerRadius;
	float OuterRadius;
	float MagnificationAmount;
	float HorizontalPosition;
	float VerticalPosition;
	float AspectRatio;
}


D2D_PS_ENTRY(D2D_ENTRY)
{
	float2 CenterPoint = float2(HorizontalPosition, VerticalPosition);
	float2 inputCoordinate = D2DGetInputCoordinate(0);
	float2 centerToPixel = inputCoordinate - CenterPoint;

	float dist = length(centerToPixel / float2(1, AspectRatio));
	float ratio = smoothstep(InnerRadius, max(InnerRadius, OuterRadius), dist);
	float2 samplePoint = lerp(CenterPoint + centerToPixel / MagnificationAmount, inputCoordinate, ratio);
	return D2DSampleInput(0, samplePoint);	
}

