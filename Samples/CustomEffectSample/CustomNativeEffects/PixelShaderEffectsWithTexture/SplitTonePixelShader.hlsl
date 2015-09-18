Texture2D InputTexture : register(t0);
Texture2D SplitToneLookupsTexture : register(t1);
SamplerState InputSampler : register(s0);

SamplerState SplitToneLookupsSampler : register(s1);

float4 main(
	float4 pos      : SV_POSITION,
	float4 posScene : SCENE_POSITION,
	float4 uv0 : TEXCOORD0
	) : SV_Target
{
	float4 color = InputTexture.Sample(InputSampler, uv0.xy);

	float intensity = (color.r + color.g + color.b) / 3.0;

	// This is how the texture coordinates work. 
	// Note, SplitToneLookupsTexture is 256x2.
	//
	// 0.0,0.0               1.0,0.0
	//         +--- ... ---+
	//         |           | 
	// 0.0,0.5 +--- ... ---+ 1.0,0.5
	//	       |           | 
	//	       +--- ... ---+
	// 0.0,1.0               1.0,1.0
	//
	color.rgb += SplitToneLookupsTexture.Sample(SplitToneLookupsSampler, float2(intensity, 0.25)).rgb - 0.5;
	color.rgb += SplitToneLookupsTexture.Sample(SplitToneLookupsSampler, float2(intensity, 0.75)).rgb - 0.5;

	return saturate(color);
}
