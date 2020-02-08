//-----------------------------------------------------------------------------
// File: PhysicsScene.fx
// Desc: Renders an unlit scene for displaying physics volumes
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------

float4x4 worldViewProjMatrix;


//--------------------------------------------------------------------------------------
// Vertex shader output structure
//--------------------------------------------------------------------------------------
struct VS_OUTPUT
{
    float4 Position   : POSITION;   // vertex position 
    float4 Diffuse    : COLOR0;     // vertex diffuse color 
};


//--------------------------------------------------------------------------------------
// This shader computes standard transform and lighting
//--------------------------------------------------------------------------------------
VS_OUTPUT RenderVS( float4 vPos : POSITION, 
                    float4 color : COLOR0
                   )
{
    VS_OUTPUT Output;
  
    // Transform the position from object space to homogeneous projection space
    Output.Position = mul(vPos, worldViewProjMatrix);
        
    Output.Diffuse = color;
    
    return Output;    
}


//--------------------------------------------------------------------------------------
float4 RenderPS( float4 Diffuse : COLOR0 ) : COLOR0
{ 
    return Diffuse;
}


//--------------------------------------------------------------------------------------
technique RenderScene
{
    pass P0
    {          
        VertexShader = compile vs_2_0 RenderVS();
        PixelShader  = compile ps_2_0 RenderPS(); 
    }
}

