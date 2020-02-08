//-----------------------------------------------------------------------------
// File: simEditor.fx
//
// Desc: Effect file for displaying editor shapes.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

float4x4 worldViewProjMatrix;

//-----------------------------------------------------------------------------
// Vertex Shader: RenderVS
// Desc: Renders highlight sphere
//-----------------------------------------------------------------------------
void RenderVS( float4 iPos : POSITION,
               out float4 oPos : POSITION,
               out float4 Color : COLOR
               )
{
    // Transform position to screen coord
    oPos = mul( iPos, worldViewProjMatrix );
    
    Color = float4(1, 1, 1, .4);
}



//-----------------------------------------------------------------------------
// Vertex Shader: RenderOutlineVS
// Desc: Renders larger, darker version of highlight sphere
//-----------------------------------------------------------------------------
void RenderOutlineVS( float4 iPos : POSITION,
                      float3 Normal : NORMAL,
                      out float4 oPos : POSITION,
                      out float4 Color : COLOR
                      )
{
    iPos.xyz += Normal * .09;

    // Transform position to screen coord
    oPos = mul( iPos, worldViewProjMatrix );
    
    Color = float4(0, 0, 0, .4);
}


//-----------------------------------------------------------------------------
// Pixel Shader: RenderPS
// Desc: 
//-----------------------------------------------------------------------------
float4 RenderPS(float4 color : COLOR) : COLOR
{
    return color;
}


//-----------------------------------------------------------------------------
// Technique: RenderScene
// Desc: Renders scene objects
//-----------------------------------------------------------------------------
technique RenderScene
{
    pass p0
    {
        CullMode = CW;
        VertexShader = compile vs_2_0 RenderOutlineVS();
        PixelShader = compile ps_2_0 RenderPS();
    }
    pass p1
    {
        CullMode = CCW;
        VertexShader = compile vs_2_0 RenderVS();
        PixelShader = compile ps_2_0 RenderPS();
    }
}
