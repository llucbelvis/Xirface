#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;
float4 Color;
texture Texture;

sampler Sampler0 = sampler_state
{
    Texture = <Texture>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION;
    float2 Coords : TEXCOORD0;
    float4 Color : COLOR0;
};

struct PixelShaderInput
{
    float4 Position : SV_POSITION;
    float2 Coords : TEXCOORD0;
    float4 Color : COLOR0;
};

PixelShaderInput VS(VertexShaderInput i)
{
    PixelShaderInput o;
    
    float4 worldPos = mul(i.Position, World);
    float4 viewPos = mul(worldPos, View);
    o.Position = mul(viewPos, Projection);
    
    o.Color = i.Color;
    o.Coords = i.Coords;
    return o;
}

float4 PS_TEXTURE(PixelShaderInput i) : SV_TARGET
{  
    return tex2D(Sampler0, i.Coords) * i.Color;
}

float4 PS_VERTEX(PixelShaderInput i) : SV_TARGET
{
    return i.Color;
}

float4 PS_ABSOLUTE(PixelShaderInput i) : SV_TARGET
{
    return Color;
}

technique TEXTURE
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VS();
        PixelShader = compile PS_SHADERMODEL PS_TEXTURE();
    }
}

technique VERTEX
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VS();
        PixelShader = compile PS_SHADERMODEL PS_VERTEX();
    }
}

technique ABSOLUTE
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VS();
        PixelShader = compile PS_SHADERMODEL PS_ABSOLUTE();
    }
}