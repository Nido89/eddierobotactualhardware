//-----------------------------------------------------------------------------
// File: simplevisualizer.fx
//
// Desc: Effect file for high dynamic range cube mapping sample.
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

#ifndef FOG_START
#define FOG_START 100.0
#endif
#ifndef FOG_END
#define FOG_END   1000.0
#endif
float4 fogColor = float4(0.85, 0.85, 1.0, 1.0);

float4x4 worldMatrix;
float4x4 BatchedWorldMatricies[50];
float4x4 viewMatrix, projMatrix;

float3x3 worldMatrixIT;
float4x4 worldViewProjMatrix;

texture sceneTexture;
texture skyDiffTexture;
float3   eyePos;
float4   lightIntensityVector[MAX_NUM_LIGHTS];
float4   lightPositionWorld[MAX_NUM_LIGHTS];  // Light positions in world space
float4   lightDirection[MAX_NUM_LIGHTS];  // Light direction in world space
float    spotCosfloatUmbra[MAX_NUM_LIGHTS]; // cos(theta/2) for spotlights
float2   lightFalloff[MAX_NUM_LIGHTS]; // Start falloff (x) and end fallof (y)
float    exposure;

// Material parameters
float4 materialAmbient;
float4 materialDiffuse;
float4 materialSpecular;
float  materialShininess;

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

sampler2D g_samSceneNoWrap =
sampler_state
{
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    BORDERCOLOR = 0xFFFFFFFF;
    Texture = <sceneTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

samplerCUBE sam_skyDiff = 
sampler_state
{
	Texture = <skyDiffTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

//-----------------------------------------------------------------------------
// Vertex Shader: RenderVS
// Desc: 
//-----------------------------------------------------------------------------
void RenderVSInternal( float4 iPos, 
               float3 iNormal, 
               out float4 oPos, // position, clip space
               out float3 Pos, // position, world space
               out float3 Normal,
               out float2 FogVec)
{
    // Transform position to world coord
    Pos = mul( iPos, worldMatrix ).xyz;

    // Transform position to clip space
    oPos = mul( iPos, worldViewProjMatrix );
    
    // Transform normal and write to texcoord2 for per-pixel lighting
    Normal = mul( iNormal, worldMatrixIT );

    // Compute cylindrical fog vector
    FogVec = (eyePos.xz - Pos.xz)/(FOG_END-FOG_START);
}

//-----------------------------------------------------------------------------
// Vertex Shader: RenderVS
// Desc: 
//-----------------------------------------------------------------------------
void RenderVS( float4 iPos    : POSITION,
               float3 iNormal : NORMAL,               
               float2 iTex    : TEXCOORD0,
               out float4 oPos : POSITION, // position, clip space
               out float2 Tex    : TEXCOORD0,	
               out float3 Pos    : TEXCOORD1, // position, world space
               out float3 Normal : TEXCOORD2,
               out float2 FogVec : TEXCOORD3
               )
{
    RenderVSInternal(iPos, iNormal, oPos, Pos, Normal, FogVec);

    // Propagate texture coord
    Tex = iTex;    
}

//-----------------------------------------------------------------------------
// Vertex Shader: RenderVSNoTexture
// Desc: position/normal packed to 32 bytes for optimal cache utilization
//-----------------------------------------------------------------------------
void RenderVSNoTexture( 
               float4 iPos    : POSITION,
               float4 iNormal : NORMAL,               
               out float4 oPos : POSITION, // position, clip space
               out float3 Pos    : TEXCOORD0, // position, world space
               out float3 Normal : TEXCOORD1,
               out float2 FogVec : TEXCOORD2
               )
{
    RenderVSInternal(iPos, iNormal.xyz, oPos, Pos, Normal, FogVec);
}

//-----------------------------------------------------------------------------
// Vertex Shader: RenderVSInstanced
// Desc: 
//-----------------------------------------------------------------------------
void RenderVSInstancedInternal( float4 iPos,
               float3 iNormal,               
               float instanceId,
               out float4 oPos, 
               out float3 Pos,
               out float3 Normal,
               out float2 FogVec
               )
{
    // Transform position to world coord
    Pos = mul( iPos, BatchedWorldMatricies[instanceId] ).xyz;

    // Transform position to clip space
    oPos = mul( Pos, mul(viewMatrix, projMatrix) );
    
    // Transform normal and write to texcoord2 for per-pixel lighting
    Normal = mul( iNormal, worldMatrixIT );

    // Compute cylindrical fog vector
    FogVec = (eyePos.xz - Pos.xz)/(FOG_END-FOG_START);

}
//-----------------------------------------------------------------------------
// Vertex Shader: RenderVSInstanced
// Desc: 
//-----------------------------------------------------------------------------
void RenderVSInstanced( float4 iPos    : POSITION,
               float3 iNormal : NORMAL,               
               float2 iTex    : TEXCOORD0,
               float instanceId : TEXCOORD1,
               out float4 oPos : POSITION, // position, clip space
               out float2 Tex    : TEXCOORD0,	
               out float3 Pos    : TEXCOORD1, // position, world space
               out float3 Normal : TEXCOORD2,
               out float2 FogVec : TEXCOORD3
               )
{
    RenderVSInstancedInternal(iPos, iNormal, instanceId, oPos, Pos, Normal, FogVec);

    // Propagate texture coord
    Tex = iTex;    
}
//-----------------------------------------------------------------------------
// Vertex Shader: RenderVSInstancedNoTexture
// Desc: 
//-----------------------------------------------------------------------------
void RenderVSInstancedNoTexture( float4 iPos    : POSITION,
               float3 iNormal : NORMAL,               
               float2 iTex    : TEXCOORD0,
               float instanceId : TEXCOORD1,
               out float4 oPos : POSITION, // position, clip space
               out float3 Pos    : TEXCOORD0, // position, world space
               out float3 Normal : TEXCOORD1,
               out float2 FogVec : TEXCOORD2
               )
{
    RenderVSInstancedInternal(iPos, iNormal, instanceId, oPos, Pos, Normal, FogVec);
}

//-----------------------------------------------------------------------------
// Vertex Shader: RenderVSBatchedInternal
// Desc: 
//-----------------------------------------------------------------------------
void RenderVSBatchedInternal( float4 iPos,
           float3 iNormal,               
           float batchId,
           out float4 oPos, // position, clip space
           out float3 Pos, // position, world space
           out float3 Normal,
           out float3 FogVec,
           out float4 matAmbient,	
           out float4 matDiffuse,
           out float4 matSpecular,
           out float4 outWrapVec
           )
{
    // Transform position to world coord
    Pos = mul( iPos, worldMatrix ).xyz;

    // Transform position to clip space
    oPos = mul( iPos, worldViewProjMatrix );
    
    // Transform normal and write to texcoord2 for per-pixel lighting
    Normal = mul( iNormal, worldMatrixIT );

    // Compute cylindrical fog vector
    FogVec.xy = (eyePos.xz - Pos.xz)/(FOG_END-FOG_START);

    matAmbient   = materialAmbients[batchId];
    matDiffuse   = materialDiffuses[batchId];
    matSpecular  = materialSpeculars[batchId];
    /*really: matShininess*/ FogVec.z = materialShininesses[batchId];

    outWrapVec = uvTransforms[batchId];
}

void RenderVSBatched( float3 iPos3    : POSITION,
           float3 iNormal : NORMAL,               
           float2 iTex    : TEXCOORD0,
           float batchId : TEXCOORD1,
           out float4 oPos : POSITION, // position, clip space
           out float2 Tex    : TEXCOORD0,	
           out float3 Pos    : TEXCOORD1, // position, world space
           out float3 Normal : TEXCOORD2,
           out float3 FogVec : TEXCOORD3,
           out float4 matAmbient    : TEXCOORD4,	
           out float4 matDiffuse    : TEXCOORD5,
           out float4 matSpecular : TEXCOORD6,
           out float4 outWrapVec : TEXCOORD7
           )
{
    float4 iPos = float4(iPos3, 1.0f);

    RenderVSBatchedInternal(iPos, iNormal, batchId, oPos, Pos, Normal, FogVec, matAmbient, matDiffuse, matSpecular, outWrapVec);

    // Propagate texture coord
    Tex = iTex;   
}

void RenderVSBatchedNoTexture( float4 iPos    : POSITION,
               float3 iNormal : NORMAL,   
               float batchId : TEXCOORD0,
               out float4 oPos : POSITION, // position, clip space
               out float3 Pos    : TEXCOORD0, // position, world space
               out float3 Normal : TEXCOORD1,
               out float3 FogVec : TEXCOORD2,
               out float4 matAmbient    : TEXCOORD3,	
               out float4 matDiffuse    : TEXCOORD4,
               out float4 matSpecular : TEXCOORD5,
               out float4 outWrapVec : TEXCOORD6
               )
{
    RenderVSBatchedInternal(iPos, iNormal, batchId, oPos, Pos, Normal, FogVec, matAmbient, matDiffuse, matSpecular, outWrapVec);
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
// Pixel Shader: RenderPSInternal
// Desc: 
//-----------------------------------------------------------------------------
float4 RenderPSInternal( float2 Tex : TEXCOORD0,
                float3 Pos : TEXCOORD1,
                float3 Normal : TEXCOORD2,
                float2 FogVec : TEXCOORD3,
                float4 matAmbient    : TEXCOORD4,	
                float4 matDiffuse    : TEXCOORD5,
                float4 matSpecular : TEXCOORD6,
                float matShininess : TEXCOORD7,
                uniform bool useTexture,
                uniform int renderMode,
                uniform bool useWrappedTexture
              ) : COLOR
{
    float3 lightdir;
    float3 nNormal = normalize(Normal);
    float3 nViewDir = normalize(eyePos - Pos);

    float4 texColor = useTexture ? 
    (useWrappedTexture ? tex2D( g_samScene, Tex ) : tex2D( g_samSceneNoWrap, Tex ))
    : 1;
    
    if(texColor.a < .5)
        discard;

    float4 ambient = 0;
    float4 diffuse = 0; 
    float4 specular = 0;
    if (renderMode != ONLY_USE_SHADOW_CASTER_LIGHT)
    {
        ambient = matAmbient;
        diffuse = texCUBE(sam_skyDiff, Normal);
    }

                                                
    // Iterate through each other light and apply the light on the pixel
    for( int LightIndex = 0; LightIndex < ACTIVE_LIGHT_COUNT; ++LightIndex )
    {
        if (CanUseLight(renderMode, LightIndex))
        {
            float attenuation;
            if (LIGHT_TYPE[LightIndex] == LIGHTTYPE_POINT)
            {
                lightdir = normalize(lightPositionWorld[LightIndex].xyz - Pos);
                attenuation = saturate(1-(distance(lightPositionWorld[LightIndex].xyz, Pos)-lightFalloff[LightIndex].x)/lightFalloff[LightIndex].y);
            }
            else if (LIGHT_TYPE[LightIndex] == LIGHTTYPE_DIRECTIONAL)
            {
                lightdir = -lightDirection[LightIndex];
                attenuation = 1;
            }
            else
            {
                lightdir = normalize(lightPositionWorld[LightIndex].xyz - Pos);
                
                float rho = dot(-lightDirection[LightIndex], lightdir);
                attenuation = saturate(1-(distance(lightPositionWorld[LightIndex].xyz, Pos)-lightFalloff[LightIndex].x)/lightFalloff[LightIndex].y);
                attenuation *= step(spotCosfloatUmbra[LightIndex], rho);
            }

            float kDiffuse = saturate(dot(lightdir, nNormal));
            float3 reflectLightDir = reflect(-lightdir, nNormal);
            float RdotV = saturate(dot(reflectLightDir, nViewDir));
            float4 kSpecular = pow(RdotV, matShininess); 
            
            float4 attenuatedColor = attenuation*lightIntensityVector[LightIndex];
            diffuse += attenuatedColor*kDiffuse;
            specular.rgb += attenuatedColor.rgb*kSpecular;
        }
    }

    diffuse *= matDiffuse;
    specular *= matSpecular;
    float4 texelColor = texColor*(diffuse + ambient) + specular;

    // Apply fog
    float fogScale = saturate(length(FogVec) - FOG_START/(FOG_END-FOG_START));
    if (renderMode != ONLY_USE_SHADOW_CASTER_LIGHT)
    {
        texelColor = lerp(texelColor, fogColor, fogScale);
    }
    else
    {
        texelColor *= 1 - fogScale;
    }
    // exposure control
    texelColor = texelColor * exposure;

    return texelColor;
}


//-----------------------------------------------------------------------------
// Pixel Shader: RenderPS
// Desc: 
//-----------------------------------------------------------------------------
float4 RenderPS( float2 Tex : TEXCOORD0,
                float3 Pos : TEXCOORD1,
                float3 Normal : TEXCOORD2,
                float2 FogVec : TEXCOORD3,
                uniform bool useTexture,
                uniform int renderMode
              ) : COLOR
{
    return RenderPSInternal(Tex, Pos, Normal, FogVec, materialAmbient, materialDiffuse,
        materialSpecular, materialShininess, useTexture, renderMode, true);
}

//-----------------------------------------------------------------------------
// Pixel Shader: RenderPSNoTexture
// Desc: 
//-----------------------------------------------------------------------------
float4 RenderPSNoTexture( float3 Pos : TEXCOORD0,
                float3 Normal : TEXCOORD1,
                float2 FogVec : TEXCOORD2,
                uniform bool useTexture,
                uniform int renderMode
              ) : COLOR
{
    // the (0,0) texture coordinate doesn't matter because it should get
    // optimized out via the uniform renderMode being UNTEXTURED
    return RenderPSInternal(float2(0,0), Pos, Normal, FogVec, materialAmbient, materialDiffuse,
        materialSpecular, materialShininess, useTexture, renderMode, true);
}

//-----------------------------------------------------------------------------
// Function: ComputeMipLevel
// Desc: uses formula for computing mip level from Real Time Rendering 2nd Ed.
//-----------------------------------------------------------------------------
float2 textureSize;
float ComputeMipLevel(float2 uv)
{
    float2 dx = ddx(uv * textureSize.x);
    float2 dy = ddy(uv * textureSize.y);
    float d = max(dot(dx, dx), dot(dy, dy));
    return 0.5f * log2(d); 
    // or alternative method: return log2( sqrt(d) )
}


//-----------------------------------------------------------------------------
// Function: Wrap
// Desc: wraps a number between [0,1]
//-----------------------------------------------------------------------------
float2 Wrap( float2 x )
{
    x = frac(x);
    if(x.x < 0) x.x = 1 - x.x;
    if(x.y < 0) x.y = 1 - x.y;

    return x;
}

//-----------------------------------------------------------------------------
// Pixel Shader: RenderPSBatched
// Desc: 
//-----------------------------------------------------------------------------
float4 RenderPSBatched( float2 Tex : TEXCOORD0,
                float3 Pos : TEXCOORD1,
                float3 Normal : TEXCOORD2,
                float3 FogVec : TEXCOORD3,
                float4 matAmbient    : TEXCOORD4,
                float4 matDiffuse    : TEXCOORD5,
                float4 matSpecular : TEXCOORD6,
                float4 wrapVector : TEXCOORD7,
                uniform bool useTexture,
                uniform int renderMode
              ) : COLOR
{
    // texture atlasing mode needs manual wrapping
    // t' = (frac( (t - x0)/(x1 - x0) )) * (x1 - x0) + x0
    // t' = (frac( (t - x0)/xW )) * xW + x0
    float2 normalizedTexCoord = Wrap( (Tex.xy - wrapVector.xy)/wrapVector.zw ); // [0,1)
    Tex = normalizedTexCoord * wrapVector.zw + wrapVector.xy;
    // Tex = clamp(Tex, wrapVector.xy, wrapVector.xy + wrapVector.zw );
    
    //Tex = clamp(Tex, .001f, .9f);
    //return (Tex.xyxy < .01f)*1000;
    
    return RenderPSInternal(Tex, Pos, Normal, FogVec.xy, matAmbient, matDiffuse, matSpecular,
        FogVec.z /* really: matShininess */, useTexture, renderMode, true);
}

//-----------------------------------------------------------------------------
// Pixel Shader: RenderPSBatchedNoTexture
// Desc: 
//-----------------------------------------------------------------------------
float4 RenderPSBatchedNoTexture( float3 Pos : TEXCOORD0,
                float3 Normal : TEXCOORD1,
                float3 FogVec : TEXCOORD2,
                float4 matAmbient    : TEXCOORD3,
                float4 matDiffuse    : TEXCOORD4,
                float4 matSpecular : TEXCOORD5,
                float4 wrapVector : TEXCOORD6,
                uniform bool useTexture,
                uniform int renderMode
              ) : COLOR
{
    return RenderPSInternal(float2(0,0), Pos, Normal, FogVec.xy, matAmbient, matDiffuse, matSpecular,
        FogVec.z /* really: matShininess */, useTexture, renderMode, true);
}



#define VERTEX_SHADER_VERSION vs_2_0 
#define PIXEL_SHADER_VERSION ps_2_a 

#define DECLARE_TECHNIQUE(name, textured, lights, vertexShaderName, pixelShaderName, vertexDeclarationName) \
technique name\
<    \
    string VertexDeclaration = vertexDeclarationName; \
> \
{\
    pass p0\
    {\
        VertexShader = compile VERTEX_SHADER_VERSION vertexShaderName();\
        PixelShader  = compile PIXEL_SHADER_VERSION pixelShaderName(textured, lights);\
    }\
}


DECLARE_TECHNIQUE(RenderScene, UNTEXTURED, USE_ALL_LIGHTS, RenderVSNoTexture, RenderPSNoTexture, "PositionNormal")
DECLARE_TECHNIQUE(RenderSceneTextured, TEXTURED, USE_ALL_LIGHTS, RenderVS, RenderPS, "PositionNormalTextured")

DECLARE_TECHNIQUE(RenderShadowCaster, UNTEXTURED, ONLY_USE_SHADOW_CASTER_LIGHT, RenderVSNoTexture, RenderPSNoTexture, "PositionNormal")
DECLARE_TECHNIQUE(RenderShadowCasterTextured, TEXTURED, ONLY_USE_SHADOW_CASTER_LIGHT, RenderVS, RenderPS, "PositionNormalTextured")

DECLARE_TECHNIQUE(RenderNonShadowCaster, UNTEXTURED, DONT_USE_SHADOW_CASTER_LIGHT, RenderVSNoTexture, RenderPSNoTexture, "PositionNormal")
DECLARE_TECHNIQUE(RenderNonShadowCasterTextured, TEXTURED, DONT_USE_SHADOW_CASTER_LIGHT, RenderVS, RenderPS, "PositionNormalTextured")

// instanced
DECLARE_TECHNIQUE(RenderSceneInstanced, UNTEXTURED, USE_ALL_LIGHTS, RenderVSInstancedNoTexture, RenderPSNoTexture, "PositionNormalInstanced")
DECLARE_TECHNIQUE(RenderSceneTexturedInstanced, TEXTURED, USE_ALL_LIGHTS, RenderVSInstanced, RenderPS, "PositionNormalTexturedInstanced")

DECLARE_TECHNIQUE(RenderShadowCasterInstanced, UNTEXTURED, ONLY_USE_SHADOW_CASTER_LIGHT, RenderVSInstancedNoTexture, RenderPSNoTexture, "PositionNormalInstanced")
DECLARE_TECHNIQUE(RenderShadowCasterTexturedInstanced, TEXTURED, ONLY_USE_SHADOW_CASTER_LIGHT, RenderVSInstanced, RenderPS, "PositionNormalTexturedInstanced")

DECLARE_TECHNIQUE(RenderNonShadowCasterInstanced, UNTEXTURED, DONT_USE_SHADOW_CASTER_LIGHT, RenderVSInstancedNoTexture, RenderPSNoTexture, "PositionNormalInstanced")
DECLARE_TECHNIQUE(RenderNonShadowCasterTexturedInstanced, TEXTURED, DONT_USE_SHADOW_CASTER_LIGHT, RenderVSInstanced, RenderPS, "PositionNormalTexturedInstanced")

// batched 
DECLARE_TECHNIQUE(RenderSceneBatched, UNTEXTURED, USE_ALL_LIGHTS, RenderVSBatchedNoTexture, RenderPSBatchedNoTexture, "PositionNormalInstanced")
DECLARE_TECHNIQUE(RenderSceneTexturedBatched, TEXTURED, USE_ALL_LIGHTS, RenderVSBatched, RenderPSBatched, "PositionNormalTexturedInstanced")

DECLARE_TECHNIQUE(RenderShadowCasterBatched, UNTEXTURED, ONLY_USE_SHADOW_CASTER_LIGHT, RenderVSBatchedNoTexture, RenderPSBatchedNoTexture, "PositionNormalInstanced")
DECLARE_TECHNIQUE(RenderShadowCasterTexturedBatched, TEXTURED, ONLY_USE_SHADOW_CASTER_LIGHT, RenderVSBatched, RenderPSBatched, "PositionNormalTexturedInstanced")

DECLARE_TECHNIQUE(RenderNonShadowCasterBatched, UNTEXTURED, DONT_USE_SHADOW_CASTER_LIGHT, RenderVSBatchedNoTexture, RenderPSBatchedNoTexture, "PositionNormalInstanced")
DECLARE_TECHNIQUE(RenderNonShadowCasterTexturedBatched, TEXTURED, DONT_USE_SHADOW_CASTER_LIGHT, RenderVSBatched, RenderPSBatched, "PositionNormalTexturedInstanced")



