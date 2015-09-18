#define D2D_INPUT_COUNT 1 
#define D2D_INPUT0_SIMPLE 

#include "d2d1effecthelpers.hlsli"

D2D_PS_ENTRY(D2D_ENTRY)
{
	float4 color = D2DGetInput(0);

	color.rgb = dot(color.rgb, float3(0.2126, 0.7152, 0.0722));

	return color;
}