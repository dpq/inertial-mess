texture fieldTexture;
float2 dp;
float coef_F = 0.027;
float coef_k = 0.054;

sampler fieldSampler = sampler_state
{
    Texture = fieldTexture;
    
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    
    AddressU = Wrap;
    AddressV = Wrap;
};

float4 UpdatePS(float2 p: TEXCOORD0) : COLOR0
{
	const float dt = 0.5;
	const float inv_dl = 1.0 / 0.2;
	const float2 diffCoef = float2(0.082, 0.041);

    float2 uv = tex2D(fieldSampler, p).xy;
	float2 lap_uv = -4.0 * uv;
    lap_uv += tex2D(fieldSampler, p + float2( dp.x, 0)).xy;
    lap_uv += tex2D(fieldSampler, p + float2(-dp.x, 0)).xy;
    lap_uv += tex2D(fieldSampler, p + float2( 0, dp.y)).xy;
    lap_uv += tex2D(fieldSampler, p + float2( 0,-dp.y)).xy;
	lap_uv *= inv_dl;

	float2 dv = diffCoef*lap_uv;
	float rate = uv.x * uv.y * uv.y;
	dv += float2(-rate, rate);
    dv += float2(coef_F * (1.0 - uv.x), -(coef_F + coef_k) * uv.y);

	return float4(uv + dv * dt, 0, 0);
}

float4 VisualPS(float2 p: TEXCOORD0) : COLOR0
{
    float2 uv = tex2D(fieldSampler, p).xy;

	float3 col = float3(0, 1, 2)*uv.y*4.0;
    return float4(col, 1);
}

float4 SpotPS(float2 p: TEXCOORD0) : COLOR0
{
    if (length(p-0.5) > 0.5)
	    discard;
    return float4(0, 0.7, 0, 0);
}



float time;
float3 p0;
float3 p1;
float3 p2;
float3 p3;
float3 p4;
float3 p5;
float3 p6;
float4x4 xform;

float4 InterferencePS(float2 p: TEXCOORD0, float2 vpos : VPOS) : COLOR0
{
	float s = 0;
	s += p0.z * sin( length(vpos.xy - p0.xy ) / 5 - time * 40 ) / (length(vpos.xy - p0.xy ) + 0.1) * 10;
	s += p1.z * sin( length(vpos.xy - p1.xy ) / 5 - time * 40 ) / (length(vpos.xy - p1.xy ) + 0.1) * 10;
	s += p2.z * sin( length(vpos.xy - p2.xy ) / 5 - time * 40 ) / (length(vpos.xy - p2.xy ) + 0.1) * 10;
	s += p3.z * sin( length(vpos.xy - p3.xy ) / 5 - time * 40 ) / (length(vpos.xy - p3.xy ) + 0.1) * 10;
	s += p4.z * sin( length(vpos.xy - p4.xy ) / 5 - time * 40 ) / (length(vpos.xy - p4.xy ) + 0.1) * 10;
	s += p5.z * sin( length(vpos.xy - p5.xy ) / 5 - time * 40 ) / (length(vpos.xy - p5.xy ) + 0.1) * 10;
	s += p6.z * sin( length(vpos.xy - p6.xy ) / 5 - time * 40 ) / (length(vpos.xy - p6.xy ) + 0.1) * 10;
	return pow(saturate(float4(s/2, s/1.5f, s*2, 1)), 0.4f);
}

void InterferenceVS(float3 v:POSITION, float2 p: TEXCOORD0, out float4 pv:POSITION, out float2 po:TEXCOORD0)
{
	pv = mul(float4(v.xyz,1), xform);
	po = p;
}


technique Interference
{
    pass Pass1
    {
        PixelShader  = compile ps_3_0 InterferencePS();
		VertexShader = compile vs_3_0 InterferenceVS();
    }
}


technique UpdateField
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 UpdatePS();
    }
}

technique Visualize
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 VisualPS();
    }
}

technique Spot
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 SpotPS();
    }
}
