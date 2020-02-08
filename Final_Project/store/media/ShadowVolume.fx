//-----------------------------------------------------------------------------
// File: ShadowVolume.fx
//
// Desc: Effect file for standard entity visualization.
//       LOWER SHADER PROFILE 
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

// CompilerMacros inserted by effect compiler:
// LIGHT_TYPE = int3(lightType0, lightType1, lightType2)
// ACTIVE_LIGHT_COUNT = number of active lights in the scene
// SHADOW_CASTER = light index that specifies which light casts shadows

#ifndef STDINCLUDE_FXH
#define STDINCLUDE_FXH

// for simplfied command line compiling
#ifndef LIGHT_TYPE 
    #define LIGHT_TYPE int3(0, 1, 2)
#endif
#ifndef ACTIVE_LIGHT_COUNT 
    #define ACTIVE_LIGHT_COUNT 2
#endif
#ifndef SHADOW_CASTER 
    #define SHADOW_CASTER 0
#endif


static const int MAX_BATCH_SIZE = 40;
float4 materialAmbients[MAX_BATCH_SIZE];
float4 materialDiffuses[MAX_BATCH_SIZE];
float4 materialSpeculars[MAX_BATCH_SIZE];
float4 uvTransforms[MAX_BATCH_SIZE];
float  materialShininesses[MAX_BATCH_SIZE];


#endif


#define MAX_NUM_LIGHTS 3		

#define LIGHTTYPE_DIRECTIONAL 1
#define LIGHTTYPE_POINT       2
#define LIGHTTYPE_SPOT        3

float4x4 worldMatrix;
float4x4 viewMatrix;
float4x4 projMatrix;
float4x4 worldViewMatrix;
float3x3 worldViewMatrixIT;
float4x4 worldViewProjMatrix;
float    farPlaneDist;

float    farPlaneDistTimesPoint9;

texture sceneTexture;
float4   lightIntensityVector[MAX_NUM_LIGHTS];
float4   lightPositionWorld[MAX_NUM_LIGHTS];  // Light positions in world space
float4   lightDirection[MAX_NUM_LIGHTS];  // Light direction in world space
float    exposure;

// These variables below basically do what the preshader used to do, however,
//  since we often have a very large number of shadow verticies, we want to make
//  sure we only set these once, whereas there's a possibility the preshader might
//  redundantly set them

// shadowCasterWorldPosition = lightPositionWorld[SHADOW_CASTER]
float4 shadowCasterWorldPosition;

// shadowCasterLightDirection = lightDirection[SHADOW_CASTER].xyz
float3 shadowCasterLightDirection;

// lightPosVS = mul(shadowCasterWorldPosition, viewMatrix).xyz
float3 lightPosVS;

// for directional / omni only
// lightDirVS = mul(shadowCasterLightDirection, (float3x3)viewMatrix);
float3 lightDirVS;

// lightDirVSPrime = lightDirVS * (-1.0f / lightDirVS.z)
float3 lightDirVSPrime;

// lightDirProj = lightDirVS * projMatrix
float4 lightDirProj;

//-----------------------------------------------------------------------------
// Vertex Shader: RenderStencilVS
// Desc: we align inputs on a 32-byte boundary
//-----------------------------------------------------------------------------
void RenderStencilVS( float4 iPos    : POSITION,
                      float4 iNormal : NORMAL,
                      out float4 oPos : POSITION
                    )
{
    // this branch isn't really a branch, since it alls gets compiled away at compile time  
    if (LIGHT_TYPE[SHADOW_CASTER] == LIGHTTYPE_POINT || LIGHT_TYPE[SHADOW_CASTER] == LIGHTTYPE_SPOT)
    {
        float3 normalVS = mul(iNormal.xyz, worldViewMatrixIT);
        float4 positionVS = mul(iPos, worldViewMatrix);
        
        float3 lightDirVS = (positionVS - lightPosVS);
        
        if (dot(normalVS, -lightDirVS) <= 0)
        {
            if (positionVS.z < lightPosVS.z)
            {
                positionVS.xyz += lightDirVS*(farPlaneDistTimesPoint9 + positionVS.z)/-lightDirVS.z;
                oPos = mul(positionVS, projMatrix);
            }
            else 
            {
                oPos = mul(float4(lightDirVS,0), projMatrix);
            }
        }
        else
        {
            oPos = mul(iPos, worldViewProjMatrix);
        }
    }
    else // directional / omni lights
    {
        float3 normalVS = mul(iNormal.xyz, worldViewMatrixIT);
        float4 positionVS = mul(iPos, worldViewMatrix);
    
        if (dot(normalVS, -lightDirVS) <= 0)
        {
            if (lightDirVS.z < 0)
            {
                positionVS.xyz += lightDirVSPrime * (farPlaneDistTimesPoint9 + positionVS.z);
                oPos = mul(positionVS, projMatrix);
            }
            else
            {
                oPos = lightDirProj;
            }
        }
        else
        {
            oPos = mul(iPos, worldViewProjMatrix);
        }
    }
}

float4 NullPixelShader( ) : COLOR
{
    float4 outputColor = 1;
    return outputColor;
}

//-----------------------------------------------------------------------------
// Technique: RenderScene
// Desc: 
//-----------------------------------------------------------------------------
technique RenderScene 
{
    pass p0
    {
        VertexShader = compile vs_2_0 RenderStencilVS();
        PixelShader = compile ps_2_0 NullPixelShader();
    }
}
