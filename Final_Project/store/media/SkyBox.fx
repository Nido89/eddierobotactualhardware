//-----------------------------------------------------------------------------
// File: SkyBox.fx
//
// Desc: Effect file for skybox rendering.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

float4x4 worldMatrix;
float4x4 worldViewProjMatrix;

texture  sceneTexture;
float	 exposure;

float4 fogColor = float4(0.85, 0.85, 1.0, 1.0);

samplerCUBE sam_sky = 
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
               out float4 oPos : POSITION,		// position, clip space
               out float3 Pos : TEXCOORD0		// position, world space
               )
{
    Pos = iPos;
    
    oPos = mul(iPos, worldViewProjMatrix);
}


//-----------------------------------------------------------------------------
// Pixel Shader: RenderPS
// Desc: 
//-----------------------------------------------------------------------------
float4 RenderPS( float3 Pos : TEXCOORD0 ) : COLOR
{
    // using position to access cubemap to avoid discontinuities
    // automatically generated normals HAVE border discontinuities
    // like sphere.x from MRI samples
    // BEWARE: sphere should be tessellated enough
    float4 texelcolor = texCUBE(sam_sky, Pos);

    // eposure control
    texelcolor = texelcolor * exposure;

    texelcolor = lerp(texelcolor,fogColor,saturate((0.08-Pos.y)/0.08));   

    return texelcolor;
}


//-----------------------------------------------------------------------------
// Technique: RenderScene
// Desc: Renders scene objects
//-----------------------------------------------------------------------------

technique RenderSceneTextured
<
    bool SaveState = true;
    string VertexDeclaration = "Position";
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
