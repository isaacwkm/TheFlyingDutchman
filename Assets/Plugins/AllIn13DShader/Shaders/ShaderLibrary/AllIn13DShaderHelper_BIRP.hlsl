#ifndef ALLIN13DSHADER_HELPER_BIRP_INCLUDED
#define ALLIN13DSHADER_HELPER_BIRP_INCLUDED

#include "AutoLight.cginc"

#ifdef REQUIRE_SCENE_DEPTH

	UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

	float GetSceneDepthDiff(float4 projPos)
	{
		float res = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(projPos))) - projPos.z;
		return res;
	}

#endif

float3 GetNormalWS(float3 normalOS)
{
	float3 normalWS = UnityObjectToWorldNormal(normalOS);
	return normalWS;
}

float3 GetViewDirWS(float3 vertexWS)
{
	float3 res = UnityWorldSpaceViewDir(vertexWS);
	return res;
}

float3 GetPositionVS(float3 positionOS)
{
	float3 res = UnityObjectToViewPos(positionOS);
	return res;
}

float3 GetMainLightDir(float3 vertexWS)
{
#ifdef _LIGHTMODEL_FASTLIGHTING
	float3 mainLightDir = global_lightDirection;
#else
	float3 mainLightDir = normalize(_WorldSpaceLightPos0.xyz - vertexWS * _WorldSpaceLightPos0.w);
#endif

	return mainLightDir;
}

float3 GetMainLightColorRGB()
{
#ifdef _LIGHTMODEL_FASTLIGHTING
	float3 res = global_lightColor.rgb;
#else
	float3 res = _LightColor0.rgb;
#endif

	return res;
}

float GetShadowAttenuation(EffectsData effectsData)
{
	UNITY_LIGHT_ATTENUATION(attenuation, effectsData, effectsData.vertexWS);
	return attenuation;
}

AllIn1LightData GetPointLightData(float3 vertexWS, float3 normalWS, float3 lightPositionWS, float3 lightColor, float pointLightAtten, EffectsData effectsData)
{
	AllIn1LightData lightData;

	float3 lightVec = lightPositionWS - vertexWS;
	float3 lightDir = normalize(lightVec);

	float atten = 1 / (1 + dot(lightVec, lightVec) * pointLightAtten);

	lightData.lightColor = lightColor;
	lightData.lightDir = lightDir;
	
#ifdef _CAST_SHADOWS_ON
	lightData.realtimeShadow = GetShadowAttenuation(effectsData);
#else
	lightData.realtimeShadow = 1.0;
#endif
	lightData.distanceAttenuation = atten;
	
	lightData.shadowColor = /*lerp(0, 1.0, lightData.realtimeShadow * lightData.distanceAttenuation)*/lightData.realtimeShadow;
	
	return lightData;
}

AllIn1LightData GetMainLightData(float3 vertexWS, EffectsData effectsData)
{
	AllIn1LightData lightData;
	
	lightData.lightColor = GetMainLightColorRGB();
	lightData.lightDir = GetMainLightDir(vertexWS);

#ifdef _CAST_SHADOWS_ON
	lightData.realtimeShadow = GetShadowAttenuation(effectsData);
#else
	lightData.realtimeShadow = 1.0;
#endif
	
#ifdef FORWARD_ADD_PASS
	lightData.shadowColor = lightData.realtimeShadow;
#else
	lightData.shadowColor = lerp(SHADOW_COLOR, 1.0, lightData.realtimeShadow);
#endif
	
	lightData.distanceAttenuation = 1.0;
	
	return lightData;
}

float3 GetPointLightPosition(int index)
{
	float3 pointLightPosition = float3(unity_4LightPosX0[index], unity_4LightPosY0[index], unity_4LightPosZ0[index]);
	return pointLightPosition;
}

AllIn1LightData GetPointLightData(int index, float3 vertexWS, float3 normalWS, EffectsData effectsData)
{
	return GetPointLightData(vertexWS, normalWS, GetPointLightPosition(index), unity_LightColor[index], unity_4LightAtten0[index], effectsData);
}

FragmentDataShadowCaster GetClipPosShadowCaster( /*VertexData v*/VertexData v, FragmentDataShadowCaster o)
{
	//float4 res = 0;
//#if defined(SHADOWS_CUBE) && !defined(SHADOWS_CUBE_IN_DEPTH_TEX)
//		// Rendering into point light (cubemap) shadows
//		//#define TRANSFER_SHADOW_CASTER_NOPOS(o,opos) o.vec = mul(unity_ObjectToWorld, v.vertex).xyz - _LightPositionRange.xyz; opos = UnityObjectToClipPos(v.vertex);
//		//#define SHADOW_CASTER_FRAGMENT(i) return UnityEncodeCubeShadowDepth ((length(i.vec) + unity_LightShadowBias.x) * _LightPositionRange.w);

//		float3 vec = mul(unity_ObjectToWorld, v.vertex).xyz - _LightPositionRange.xyz;
//		res = UnityObjectToClipPos(v.vertex);
//#else
//		// Rendering into directional or spot light shadows
//	res = UnityClipSpaceShadowCasterPos(v.vertex, v.normal);
//	res = UnityApplyLinearShadowBias(res);
//	#endif

	//res = UnityClipSpaceShadowCasterPos(v.vertex.xyz, v.normal);
	//res = UnityApplyLinearShadowBias(res);
	
	TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
	return o;
	
	//return res; 
} 

//float GetShadowAttenuation(FragmentData i, float3 vertexWS)
//{
//	//float shadowAttenuation = UNITY_SHADOW_ATTENUATION(i, vertexWS);

//	//float3 lightCoord = mul(unity_WorldToLight, float4(vertexWS, 1)).xyz;
// //   fixed shadow = UNITY_SHADOW_ATTENUATION(i, vertexWS);
// //   float res = tex2D(_LightTexture0, dot(lightCoord, lightCoord).rr).r * shadow;

//	/*TODO: Fix this*/
//	//shadowAttenuation = 1.0;

//	UNITY_LIGHT_ATTENUATION(attenuation, i, vertexWS);
	
//	return attenuation;
//}

ShadowCoordStruct GetShadowCoords(VertexData v, float4 clipPos, float3 vertexWS)
{
	ShadowCoordStruct res;

	res.pos = clipPos;
	res._ShadowCoord = 0;
	UNITY_TRANSFER_SHADOW(res, float2(0, 0));
	
	return res;
}
 
float3 GetLightmap(float2 uvLightmap)
{
	float3 res = 1.0;

#ifdef _AFFECTED_BY_LIGHTMAPS
	res = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, uvLightmap));
#endif
	
	return res;
}

float3 GetAmbientColor(float4 normalWS)
{
	return ShadeSH9(normalWS);
}

//float GetFogFactor(FragmentData fragmentData)
//{
//	float res = 0;

//#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
//	UNITY_TRANSFER_FOG(fragmentData, fragmentData.pos);
//	res = fragmentData.fogCoord;
//#endif
	
//	return res;
//}

float GetFogFactor(float4 clipPos)
{
	float res = 0;
#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
	FogStruct fogStruct;
	UNITY_TRANSFER_FOG(fogStruct, clipPos);
	res = fogStruct.fogCoord;
#endif
	
	return res;
}

float4 CustomMixFog(float fogFactor, float4 col)
{
	float4 res = col;
	UNITY_APPLY_FOG(fogFactor, res);
	return res;
}

#ifdef REFLECTIONS_ON
float3 GetSkyColor(float3 normalWS, float3 viewDirWS, float cubeLod)
{
	float3 worldRefl = normalize(reflect(-viewDirWS, normalWS));
	float4 skyData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, worldRefl, cubeLod);
	float3 res = DecodeHDR (skyData, unity_SpecCube0_HDR);

#ifdef _REFLECTIONS_TOON
	float posterizationLevel = lerp(2, 20, ACCESS_PROP(_ToonFactor));
	res = floor(res * posterizationLevel) / posterizationLevel;
#endif

	res *= ACCESS_PROP(_ReflectionsAtten);

	return res;
}
#endif

float3 ShadeSH(float4 normalWS)
{
	float3 res = ShadeSH9(normalWS);
	return res;
}


#define NUM_ADDITIONAL_LIGHTS 4;

#define OBJECT_TO_CLIP_SPACE(v) UnityObjectToClipPos(v.vertex)
#define OBJECT_TO_CLIP_SPACE_FLOAT4(pos) UnityObjectToClipPos(pos)

#endif