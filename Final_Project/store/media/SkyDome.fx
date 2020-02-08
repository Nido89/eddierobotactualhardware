//-----------------------------------------------------------------------------
// File: SkyDome.fx
//
// Desc: Effect file for skydome rendering.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

float4x4 worldMatrix;
float4x4 worldViewProjMatrix;

texture  sceneTexture;
float	 exposure;

float4 fogColor = float4(0.85, 0.85, 1.0, 1.0);

sampler2D sam_sky = 
sampler_state
{
	Texture = <sceneTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};


//-----------------------------------------------------------------------------
// Vertex Shader: RenderVS
// Desc: 
//-----------------------------------------------------------------------------
void RenderVS( float4 iPos : POSITION,
               float2 iTex : TEXCOORD0,
               out float4 oPos : POSITION,
               out float2 Tex : TEXCOORD0
               )
{
    oPos = mul(iPos, worldViewProjMatrix);
    
    Tex = iTex;
}


//-----------------------------------------------------------------------------
// Pixel Shader: RenderPS
// Desc: 
//-----------------------------------------------------------------------------
float4 RenderPS( float2 Tex : TEXCOORD0 ) : COLOR
{
    float4 texelcolor = tex2D(sam_sky, Tex);

    // eposure control
    texelcolor = texelcolor * exposure;

    float4 fogcolor = float4(0.85, 0.85, 1.0, 1.0);
    texelcolor = lerp(texelcolor,fogColor,saturate((0.08+Tex.y)/0.08));   

    return texelcolor;
}


//-----------------------------------------------------------------------------
// Technique: RenderScene
// Desc: Renders scene objects
//-----------------------------------------------------------------------------

technique RenderSceneTextured
<
    bool SaveState = true;
    string VertexDeclaration = "PositionTextured";
>
{
    pass p0
    {
        CullMode = None;
        ZEnable = true;
        ZWriteEnable = false;
        VertexShader = compile vs_2_0 RenderVS();
        PixelShader = compile ps_2_0 RenderPS();
    }
}