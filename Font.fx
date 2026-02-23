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

struct VertexShaderInput
{
    float4 position : POSITION0;
    float2 uv : TEXCOORD0;
    float4 color : COLOR0;
    float curve : TEXCOORD1;
    float side : TEXCOORD2;
};

struct PixelShaderInput
{
    float4 position : SV_POSITION;
    float2 uv : TEXCOORD0;
    float4 color : COLOR0;
    float curve : TEXCOORD1;
    float side : TEXCOORD2;
};

PixelShaderInput VS(VertexShaderInput i)
{
    PixelShaderInput o;

    float4 worldPos = mul(i.position, World);
    float4 viewPos = mul(worldPos, View);
    o.position = mul(viewPos, Projection);

    o.uv = i.uv;
    o.color = i.color;
    o.curve = i.curve;
    o.side = i.side;
    
    return o;
}

float4 PS(PixelShaderInput i) : SV_TARGET
{
    if (i.curve > 0)
    {
        float x = i.uv.x;
        float y = i.uv.y;
    
        bool fill = x * x < y;
        
        if (i.side < 0)
        {
            fill = !fill;
        }
        if (fill)
            return float4(i.color.xyzw);
         else
            discard;
    }
    
    return float4(i.color.xyzw);
}

technique FONT
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VS();
        PixelShader = compile PS_SHADERMODEL PS();
    }
}