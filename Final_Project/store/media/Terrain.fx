//-----------------------------------------------------------------------------
// File: Terrain.fx
//
// Desc: Effect file for terrain, high contrast, for better geoemtry perception.
//       
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

// CompilerMacros inserted by effect compiler:
// LIGHT_TYPE = int3(lightType0, lightType1, lightType2)
// ACTIVE_LIGHT_COUNT = number of active lights in the scene
// SHADOW_CASTER = light index that specifies which light casts shadows

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


#define MAX_NUM_LIGHTS 3		

#define LIGHTTYPE_DIRECTIONAL 1
#define LIGHTTYPE_POINT       2

#ifndef FOG_START
#define FOG_START 100.0
#endif
#ifndef FOG_END
#define FOG_END   1000.0
#endif
float4 fogColor = float4(0.85, 0.85, 1.0, 1.0);

float4x4 worldMatrix;
float4x4 worldViewProjMatrix;

texture  sceneTexture;
float3   eyePos;
float4   lightIntensityVector[MAX_NUM_LIGHTS];
float4   lightPositionWorld[MAX_NUM_LIGHTS];  // Light positions in view space
float4   lightDirection[MAX_NUM_LIGHTS];  // Light direction in view space
float	 exposure;

sampler2D g_samScene =
sampler_state
{
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    Texture = <sceneTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

//-----------------------------------------------------------------------------
// Vertex Shader: RenderVS
// Desc: 
//-----------------------------------------------------------------------------
void RenderVS( float4 iPos    : POSITION,
               float3 iNormal : NORMAL,               
               float2 iTex    : TEXCOORD0,
               out float4 oPos : POSITION,  // Clip space position
               out float2 Tex              : TEXCOORD0,	
               out float3 Pos              : TEXCOORD1, // position, world 
               out float3 Normal           : TEXCOORD2, // normal, world
               out float3 LightAttenuation : TEXCOORD3,
               out float  FogVal           : TEXCOORD4  // Cylindrical fog value
               )
{
    // Transform position to world coord
    Pos = mul( iPos, worldMatrix );

    // Transform position to screen coord
    oPos = mul( iPos, worldViewProjMatrix );
    
    // Transform normal and write to texcoord2 for per-pixel lighting
    Normal = mul( iNormal, (float3x3)worldMatrix );

    // Compute the attenuation for all point lights
    float3 lightDistance = 0; // Get the distance to all lights
    for (int iLight = 0; iLight < ACTIVE_LIGHT_COUNT; ++iLight)
    {
        if (LIGHT_TYPE[iLight] == LIGHTTYPE_POINT)
            lightDistance[iLight] = distance(lightPositionWorld[iLight].xyz, Pos);
        else
            lightDistance[iLight] = 0;
    }
    LightAttenuation = (30 - lightDistance)/30; // Compute all attenuation based on distance

    // Propagate texture coord
    Tex = iTex;    
    
    // Compute cylindrical fog value
    FogVal = (distance(eyePos.xz, Pos.xz) - FOG_START)/(FOG_END-FOG_START);
}

void RenderVSNoTexture( float4 iPos    : POSITION,
               float3 iNormal : NORMAL,               
               float2 iTex    : TEXCOORD0,
               out float4 oPos : POSITION,  // Clip space position
               out float2 Tex              : TEXCOORD0,	
               out float3 Pos              : TEXCOORD1, // position, world 
               out float3 Normal           : TEXCOORD2, // normal, world
               out float3 LightAttenuation : TEXCOORD3,
               out float  FogVal           : TEXCOORD4  // Cylindrical fog value
               )
{
    RenderVS(iPos, iNormal, float2(0,0), oPos, Tex, Pos, Normal, LightAttenuation, FogVal);
}

// Valid options for the useTexture parameter in RenderPS
#define TEXTURED true
#define UNTEXTURED false

// Valid options for the renderMode parameter in RenderPS
#define DONT_USE_SHADOW_CASTER_LIGHT 0
#define ONLY_USE_SHADOW_CASTER_LIGHT 1
#define USE_ALL_LIGHTS 2

bool CanUseLight(int renderMode, int lightIndex)
{
    if (renderMode == USE_ALL_LIGHTS)
        return true;
    else if (renderMode == DONT_USE_SHADOW_CASTER_LIGHT)
        return lightIndex != SHADOW_CASTER;
    else
        return lightIndex == SHADOW_CASTER;
}

//-----------------------------------------------------------------------------
// Pixel Shader: RenderPS
// Desc: 
//-----------------------------------------------------------------------------
float4 RenderPS( float2 Tex : TEXCOORD0,
                float3 Pos : TEXCOORD1,
                float3 Normal : TEXCOORD2,
                float3 LightAttenuation : TEXCOORD3,
                float  FogVal : TEXCOORD4,
                uniform bool useTexture,
                uniform int renderMode
              ) : COLOR
{
    float3 lightdir;
    float3 nNormal = normalize(Normal);
 
    float4 texColor = useTexture ? tex2D( g_samScene, Tex ) : 1;
    
    float4 diffuse = 0; 

                                            
    // Iterate through each other light and apply the light on the pixel
    for( int LightIndex = 0; LightIndex < ACTIVE_LIGHT_COUNT; ++LightIndex )
    {
        if (CanUseLight(renderMode, LightIndex))
        {
            if (LIGHT_TYPE[LightIndex] == LIGHTTYPE_POINT) // If point light
                lightdir = normalize(lightPositionWorld[LightIndex].xyz - Pos);
            else 
                lightdir = -lightDirection[LightIndex];
                
            diffuse += saturate(LightAttenuation[LightIndex]) * saturate(dot(lightdir, nNormal)) * lightIntensityVector[LightIndex]; 
        }
    } 
      
    float4 texelColor = texColor*diffuse;

    // Apply fog
    if (renderMode != ONLY_USE_SHADOW_CASTER_LIGHT)
    {
        // Apply fog
        texelColor = lerp(texelColor, fogColor, saturate(FogVal));
    }
    else
    {
        texelColor *= 1 - saturate(FogVal);
    }
    
    // exposure control
    texelColor *= exposure;
  
    return texelColor;
}

float4 RenderPSNoTexture( 
                float3 Pos : TEXCOORD0,
                float3 Normal : TEXCOORD1,
                float3 LightAttenuation : TEXCOORD2,
                float  FogVal : TEXCOORD3,
                uniform bool useTexture,
                uniform int renderMode
              ) : COLOR
{
    return RenderPS(float2(0,0), Pos, Normal, LightAttenuation, FogVal, useTexture, renderMode);
}


#define DECLARE_TECHNIQUE(name, textured, lights, vertexDeclarationName) \
technique name\
<   \
    string VertexDeclaration = vertexDeclarationName; \
>   \
{\
    pass p0\
    {\
        VertexShader = compile vs_2_0 RenderVS();\
        PixelShader  = compile ps_2_0 RenderPS(textured, lights);\
    }\
}

DECLARE_TECHNIQUE(RenderScene, UNTEXTURED, USE_ALL_LIGHTS, "PositionNormal")
DECLARE_TECHNIQUE(RenderSceneTextured, TEXTURED, USE_ALL_LIGHTS, "PositionNormalTextured")

DECLARE_TECHNIQUE(RenderShadowCaster, UNTEXTURED, ONLY_USE_SHADOW_CASTER_LIGHT, "PositionNormal")
DECLARE_TECHNIQUE(RenderShadowCasterTextured, TEXTURED, ONLY_USE_SHADOW_CASTER_LIGHT, "PositionNormalTextured")

DECLARE_TECHNIQUE(RenderNonShadowCaster, UNTEXTURED, DONT_USE_SHADOW_CASTER_LIGHT, "PositionNormal")
DECLARE_TECHNIQUE(RenderNonShadowCasterTextured, TEXTURED, DONT_USE_SHADOW_CASTER_LIGHT, "PositionNormalTextured")
