//-----------------------------------------------------------------------------
// File: LaserRangeFinder.fx
// Desc: 
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Parameters
//--------------------------------------------------------------------------------------

float4x4 BatchedWorldMatricies[50];
float4x4 worldMatrix;
float4x4 viewMatrix;
float4x4 projMatrix;

texture  sceneTexture;
float4    timeAttenuation;

sampler2D g_samScene =
sampler_state
{
    Texture = <sceneTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};


//--------------------------------------------------------------------------------------
// Vertex shader output structure
//--------------------------------------------------------------------------------------
struct VS_OUTPUT
{
    float4 Position : POSITION;
    float2 Texture    : TEXCOORD0;  // texture coord 
};


//--------------------------------------------------------------------------------------
// This shader computes standard transform and lighting
//--------------------------------------------------------------------------------------
VS_OUTPUT RenderVS( float3 Pos : POSITION, 
                    float2 Texture :  TEXCOORD0
                     )
{
    VS_OUTPUT Output;
    
    // One might think we should just pass the worldViewProj matrix, but in reality this
    // vertex shader should not get run (rather the Instanced version), but we keep it 
    // here just in case someone has a very old card without enough vertex shader constants
    Output.Position = mul(mul(float4(Pos, 1.0f), worldMatrix), mul(viewMatrix, projMatrix) );    
    Output.Texture = Texture;
     
    return Output;    
}

//--------------------------------------------------------------------------------------
// This shader computes standard transform and lighting
//--------------------------------------------------------------------------------------
VS_OUTPUT RenderVSInstanced( float3 Pos : POSITION, 
                    float2 Texture :  TEXCOORD0,
                    float InstanceId :  TEXCOORD1
                     )
{
    VS_OUTPUT Output;
    
    Output.Position = mul( mul(float4(Pos, 1.0f), BatchedWorldMatricies[InstanceId]), mul(viewMatrix, projMatrix) );    
    Output.Texture = Texture;
     
    return Output;    
}


//--------------------------------------------------------------------------------------
float4 RenderPS(float2 Tex : TEXCOORD0) : COLOR
{
    return tex2D( g_samScene, Tex )*timeAttenuation;
}


//--------------------------------------------------------------------------------------
technique RenderSceneTextured
<
    bool SaveState = true;
    string VertexDeclaration = "PositionTextured";
>
{
    pass P0
    {      
        AlphaBlendEnable = true;  
        SrcBlend = SrcColor;
        DestBlend = InvSrcColor;
        CullMode = None;
        ZEnable = true;
        ZWriteEnable = false;
        
        VertexShader = compile vs_2_0 RenderVS();
        PixelShader  = compile ps_2_0 RenderPS(); 
    }
}

technique RenderSceneTexturedInstanced
<
    bool SaveState = true;
    string VertexDeclaration = "PositionTexturedInstanced";
>
{
    pass P0
    {      
        AlphaBlendEnable = true;  
        SrcBlend = SrcColor;
        DestBlend = InvSrcColor;
        CullMode = None;
        ZEnable = true;
        ZWriteEnable = false;
        
        VertexShader = compile vs_2_0 RenderVSInstanced();
        PixelShader  = compile ps_2_0 RenderPS(); 
    }
}

