// Made with Amplify Shader Editor v1.9.5.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Distant Lands/Cozy/URP/Stylized Fog (Physical Height)"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		_FogVariationTexture("Fog Variation Texture", 2D) = "white" {}


		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25

		[HideInInspector] _QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector] _QueueControl("_QueueControl", Float) = -1

        [HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

		[HideInInspector][ToggleOff] _ReceiveShadows("Receive Shadows", Float) = 1.0
	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-100" "UniversalMaterialType"="Unlit" }

		Cull Front
		AlphaToMask Off

		Stencil
		{
			Ref 221
			ReadMask 222
			WriteMask 222
			Comp NotEqual
			Pass Keep
		}

		HLSLINCLUDE
		#pragma target 4.5
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}

		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			ZTest Always
			Offset 0 , 0
			ColorMask RGBA

			

			HLSLPROGRAM

			

			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_SRP_VERSION 140010
			#define REQUIRE_DEPTH_TEXTURE 1


			

			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3

			

			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_UNLIT

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140010
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#endif
		

			

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#define ASE_NEEDS_FRAG_SCREEN_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 positionWS : TEXCOORD1;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD2;
				#endif
				#ifdef ASE_FOG
					float fogFactor : TEXCOORD3;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
						#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			float4 CZY_LightColor;
			float4 CZY_FogColor1;
			float4 CZY_FogColor2;
			float CZY_FogDepthMultiplier;
			sampler2D _FogVariationTexture;
			float3 CZY_VariationWindDirection;
			float CZY_VariationScale;
			float CZY_VariationAmount;
			float CZY_VariationDistance;
			float CZY_FogColorStart1;
			float4 CZY_FogColor3;
			float CZY_FogColorStart2;
			float4 CZY_FogColor4;
			float CZY_FogColorStart3;
			float4 CZY_FogColor5;
			float CZY_FogColorStart4;
			float CZY_LightFlareSquish;
			float3 CZY_SunDirection;
			half CZY_LightIntensity;
			half CZY_LightFalloff;
			float CZY_FilterSaturation;
			float CZY_FilterValue;
			float4 CZY_FilterColor;
			float4 CZY_SunFilterColor;
			float3 CZY_MoonDirection;
			float4 CZY_FogMoonFlareColor;
			float4 CZY_HeightFogColor;
			float CZY_HeightFogBase;
			float CZY_HeightFogTransition;
			float CZY_HeightFogBaseVariationScale;
			float CZY_HeightFogBaseVariationAmount;
			float CZY_HeightFogIntensity;
			float _UnderwaterRenderingEnabled;
			float _FullySubmerged;
			sampler2D _UnderwaterMask;
			float CZY_FogSmoothness;
			float CZY_FogOffset;
			float CZY_FogIntensity;


			float3 HSVToRGB( float3 c )
			{
				float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
				float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
				return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
			}
			
			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
				float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
				float d = q.x - min( q.w, q.y );
				float e = 1.0e-10;
				return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}
			float2 UnStereo( float2 UV )
			{
				#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[ unity_StereoEyeIndex ];
				UV.xy = (UV.xy - scaleOffset.zw) / scaleOffset.xy;
				#endif
				return UV;
			}
			
			float3 InvertDepthDirURP75_g128( float3 In )
			{
				float3 result = In;
				#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301 || ASE_SRP_VERSION == 70503 || ASE_SRP_VERSION == 70600 || ASE_SRP_VERSION == 70700 || ASE_SRP_VERSION == 70701 || ASE_SRP_VERSION >= 80301
				result *= float3(1,1,-1);
				#endif
				return result;
			}
			
			float3 InvertDepthDirURP75_g125( float3 In )
			{
				float3 result = In;
				#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301 || ASE_SRP_VERSION == 70503 || ASE_SRP_VERSION == 70600 || ASE_SRP_VERSION == 70700 || ASE_SRP_VERSION == 70701 || ASE_SRP_VERSION >= 80301
				result *= float3(1,1,-1);
				#endif
				return result;
			}
			
			float HLSL20_g123( bool enabled, bool submerged, float textureSample )
			{
				if(enabled)
				{
					if(submerged) return 1.0;
					else return textureSample;
				}
				else
				{
					return 0.0;
				}
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.positionWS = vertexInput.positionWS;
				#endif

				#ifdef ASE_FOG
					o.fogFactor = ComputeFogFactor( vertexInput.positionCS.z );
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.positionCS = vertexInput.positionCS;
				o.clipPosV = vertexInput.positionCS;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag ( VertexOutput IN
				#ifdef _WRITE_RENDERING_LAYERS
				, out float4 outRenderingLayers : SV_Target1
				#endif
				 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.positionWS;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float4 ase_screenPosNorm = ScreenPos / ScreenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float2 UV22_g129 = ase_screenPosNorm.xy;
				float2 localUnStereo22_g129 = UnStereo( UV22_g129 );
				float2 break64_g128 = localUnStereo22_g129;
				float clampDepth69_g128 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch38_g128 = ( 1.0 - clampDepth69_g128 );
				#else
				float staticSwitch38_g128 = clampDepth69_g128;
				#endif
				float3 appendResult39_g128 = (float3(break64_g128.x , break64_g128.y , staticSwitch38_g128));
				float4 appendResult42_g128 = (float4((appendResult39_g128*2.0 + -1.0) , 1.0));
				float4 temp_output_43_0_g128 = mul( unity_CameraInvProjection, appendResult42_g128 );
				float3 temp_output_46_0_g128 = ( (temp_output_43_0_g128).xyz / (temp_output_43_0_g128).w );
				float3 In75_g128 = temp_output_46_0_g128;
				float3 localInvertDepthDirURP75_g128 = InvertDepthDirURP75_g128( In75_g128 );
				float4 appendResult49_g128 = (float4(localInvertDepthDirURP75_g128 , 1.0));
				float4 temp_output_97_0_g127 = mul( unity_CameraToWorld, appendResult49_g128 );
				float preDepth120_g127 = distance( temp_output_97_0_g127 , float4( _WorldSpaceCameraPos , 0.0 ) );
				float lerpResult114_g127 = lerp( preDepth120_g127 , ( preDepth120_g127 * (( 1.0 - CZY_VariationAmount ) + (tex2D( _FogVariationTexture, (( (temp_output_97_0_g127).xz + ( (CZY_VariationWindDirection).xz * _TimeParameters.x ) )*( 0.1 / CZY_VariationScale ) + 0.0) ).r - 0.0) * (1.0 - ( 1.0 - CZY_VariationAmount )) / (1.0 - 0.0)) ) , ( 1.0 - saturate( ( preDepth120_g127 / CZY_VariationDistance ) ) ));
				float newFogDepth103_g127 = lerpResult114_g127;
				float temp_output_15_0_g127 = ( CZY_FogDepthMultiplier * sqrt( newFogDepth103_g127 ) );
				float temp_output_1_0_g132 = temp_output_15_0_g127;
				float4 lerpResult28_g132 = lerp( CZY_FogColor1 , CZY_FogColor2 , saturate( ( temp_output_1_0_g132 / CZY_FogColorStart1 ) ));
				float4 lerpResult41_g132 = lerp( saturate( lerpResult28_g132 ) , CZY_FogColor3 , saturate( ( ( CZY_FogColorStart1 - temp_output_1_0_g132 ) / ( CZY_FogColorStart1 - CZY_FogColorStart2 ) ) ));
				float4 lerpResult35_g132 = lerp( lerpResult41_g132 , CZY_FogColor4 , saturate( ( ( CZY_FogColorStart2 - temp_output_1_0_g132 ) / ( CZY_FogColorStart2 - CZY_FogColorStart3 ) ) ));
				float4 lerpResult113_g132 = lerp( lerpResult35_g132 , CZY_FogColor5 , saturate( ( ( CZY_FogColorStart3 - temp_output_1_0_g132 ) / ( CZY_FogColorStart3 - CZY_FogColorStart4 ) ) ));
				float4 temp_output_157_0_g127 = lerpResult113_g132;
				float3 hsvTorgb32_g127 = RGBToHSV( temp_output_157_0_g127.rgb );
				float3 normalizeResult160_g127 = normalize( ( WorldPosition - _WorldSpaceCameraPos ) );
				float3 temp_output_91_0_g127 = ( normalizeResult160_g127 * _ProjectionParams.z );
				float3 appendResult73_g127 = (float3(1.0 , CZY_LightFlareSquish , 1.0));
				float3 normalizeResult5_g127 = normalize( ( ( temp_output_91_0_g127 * appendResult73_g127 ) - _WorldSpaceCameraPos ) );
				float dotResult6_g127 = dot( normalizeResult5_g127 , CZY_SunDirection );
				half LightMask27_g127 = saturate( pow( abs( ( (dotResult6_g127*0.5 + 0.5) * CZY_LightIntensity ) ) , CZY_LightFalloff ) );
				float temp_output_26_0_g127 = ( (temp_output_157_0_g127).a * saturate( temp_output_15_0_g127 ) );
				float3 hsvTorgb2_g131 = RGBToHSV( ( CZY_LightColor * hsvTorgb32_g127.z * saturate( ( LightMask27_g127 * ( 1.5 * temp_output_26_0_g127 ) ) ) ).rgb );
				float3 hsvTorgb3_g131 = HSVToRGB( float3(hsvTorgb2_g131.x,saturate( ( hsvTorgb2_g131.y + CZY_FilterSaturation ) ),( hsvTorgb2_g131.z + CZY_FilterValue )) );
				float4 temp_output_10_0_g131 = ( float4( hsvTorgb3_g131 , 0.0 ) * CZY_FilterColor );
				float3 direction90_g127 = ( temp_output_91_0_g127 - _WorldSpaceCameraPos );
				float3 normalizeResult93_g127 = normalize( direction90_g127 );
				float3 normalizeResult88_g127 = normalize( CZY_MoonDirection );
				float dotResult49_g127 = dot( normalizeResult93_g127 , normalizeResult88_g127 );
				half MoonMask47_g127 = saturate( pow( abs( ( saturate( (dotResult49_g127*1.0 + 0.0) ) * CZY_LightIntensity ) ) , ( CZY_LightFalloff * 3.0 ) ) );
				float3 hsvTorgb2_g130 = RGBToHSV( ( temp_output_157_0_g127 + ( hsvTorgb32_g127.z * saturate( ( temp_output_26_0_g127 * MoonMask47_g127 ) ) * CZY_FogMoonFlareColor ) ).rgb );
				float3 hsvTorgb3_g130 = HSVToRGB( float3(hsvTorgb2_g130.x,saturate( ( hsvTorgb2_g130.y + CZY_FilterSaturation ) ),( hsvTorgb2_g130.z + CZY_FilterValue )) );
				float4 temp_output_10_0_g130 = ( float4( hsvTorgb3_g130 , 0.0 ) * CZY_FilterColor );
				float2 UV22_g126 = ase_screenPosNorm.xy;
				float2 localUnStereo22_g126 = UnStereo( UV22_g126 );
				float2 break64_g125 = localUnStereo22_g126;
				float clampDepth69_g125 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch38_g125 = ( 1.0 - clampDepth69_g125 );
				#else
				float staticSwitch38_g125 = clampDepth69_g125;
				#endif
				float3 appendResult39_g125 = (float3(break64_g125.x , break64_g125.y , staticSwitch38_g125));
				float4 appendResult42_g125 = (float4((appendResult39_g125*2.0 + -1.0) , 1.0));
				float4 temp_output_43_0_g125 = mul( unity_CameraInvProjection, appendResult42_g125 );
				float3 temp_output_46_0_g125 = ( (temp_output_43_0_g125).xyz / (temp_output_43_0_g125).w );
				float3 In75_g125 = temp_output_46_0_g125;
				float3 localInvertDepthDirURP75_g125 = InvertDepthDirURP75_g125( In75_g125 );
				float4 appendResult49_g125 = (float4(localInvertDepthDirURP75_g125 , 1.0));
				float4 temp_output_18_0_g124 = mul( unity_CameraToWorld, appendResult49_g125 );
				float mulTime63_g124 = _TimeParameters.x * 0.01;
				float eyeDepth31_g124 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float temp_output_121_0_g122 = ( ( 1.0 - saturate( ( ( temp_output_18_0_g124.y - CZY_HeightFogBase ) / ( CZY_HeightFogTransition + ( ( 1.0 - tex2D( _FogVariationTexture, ((temp_output_18_0_g124).xz*( 1.0 / CZY_HeightFogBaseVariationScale ) + mulTime63_g124) ).r ) * CZY_HeightFogBaseVariationAmount ) ) ) ) ) * saturate( ( eyeDepth31_g124 * 0.01 * CZY_HeightFogIntensity ) ) * CZY_HeightFogColor.a );
				float4 lerpResult108_g122 = lerp( ( ( temp_output_10_0_g131 * CZY_SunFilterColor ) + temp_output_10_0_g130 ) , CZY_HeightFogColor , temp_output_121_0_g122);
				
				bool enabled20_g123 =(bool)_UnderwaterRenderingEnabled;
				bool submerged20_g123 =(bool)_FullySubmerged;
				float textureSample20_g123 = tex2Dlod( _UnderwaterMask, float4( ase_screenPosNorm.xy, 0, 0.0) ).r;
				float localHLSL20_g123 = HLSL20_g123( enabled20_g123 , submerged20_g123 , textureSample20_g123 );
				float finalAlpha141_g127 = temp_output_26_0_g127;
				float3 ase_objectScale = float3( length( GetObjectToWorldMatrix()[ 0 ].xyz ), length( GetObjectToWorldMatrix()[ 1 ].xyz ), length( GetObjectToWorldMatrix()[ 2 ].xyz ) );
				float temp_output_124_56_g122 = ( finalAlpha141_g127 * saturate( ( ( 1.0 - saturate( ( ( ( direction90_g127.y * 0.1 ) * ( 1.0 / ( ( CZY_FogSmoothness * length( ase_objectScale ) ) * 10.0 ) ) ) + ( 1.0 - CZY_FogOffset ) ) ) ) * CZY_FogIntensity ) ) );
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = lerpResult108_g122.rgb;
				float Alpha = ( ( 1.0 - localHLSL20_g123 ) * max( temp_output_121_0_g122 , temp_output_124_56_g122 ) );
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#if defined(_DBUFFER)
					ApplyDecalToBaseColor(IN.positionCS, Color);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.positionCS );
				#endif

				#ifdef ASE_FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
				#endif

				return half4( Color, Alpha );
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0
			AlphaToMask Off

			HLSLPROGRAM

			

			#pragma multi_compile_instancing
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_SRP_VERSION 140010
			#define REQUIRE_DEPTH_TEXTURE 1


			

			#pragma vertex vert
			#pragma fragment frag

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#define ASE_NEEDS_FRAG_SCREEN_POSITION
			#define ASE_NEEDS_FRAG_WORLD_POSITION


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 positionWS : TEXCOORD1;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD2;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
						#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			float _UnderwaterRenderingEnabled;
			float _FullySubmerged;
			sampler2D _UnderwaterMask;
			float CZY_HeightFogBase;
			float CZY_HeightFogTransition;
			sampler2D _FogVariationTexture;
			float CZY_HeightFogBaseVariationScale;
			float CZY_HeightFogBaseVariationAmount;
			float CZY_HeightFogIntensity;
			float4 CZY_HeightFogColor;
			float4 CZY_FogColor1;
			float4 CZY_FogColor2;
			float CZY_FogDepthMultiplier;
			float3 CZY_VariationWindDirection;
			float CZY_VariationScale;
			float CZY_VariationAmount;
			float CZY_VariationDistance;
			float CZY_FogColorStart1;
			float4 CZY_FogColor3;
			float CZY_FogColorStart2;
			float4 CZY_FogColor4;
			float CZY_FogColorStart3;
			float4 CZY_FogColor5;
			float CZY_FogColorStart4;
			float CZY_FogSmoothness;
			float CZY_FogOffset;
			float CZY_FogIntensity;


			float HLSL20_g123( bool enabled, bool submerged, float textureSample )
			{
				if(enabled)
				{
					if(submerged) return 1.0;
					else return textureSample;
				}
				else
				{
					return 0.0;
				}
			}
			
			float2 UnStereo( float2 UV )
			{
				#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[ unity_StereoEyeIndex ];
				UV.xy = (UV.xy - scaleOffset.zw) / scaleOffset.xy;
				#endif
				return UV;
			}
			
			float3 InvertDepthDirURP75_g125( float3 In )
			{
				float3 result = In;
				#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301 || ASE_SRP_VERSION == 70503 || ASE_SRP_VERSION == 70600 || ASE_SRP_VERSION == 70700 || ASE_SRP_VERSION == 70701 || ASE_SRP_VERSION >= 80301
				result *= float3(1,1,-1);
				#endif
				return result;
			}
			
			float3 InvertDepthDirURP75_g128( float3 In )
			{
				float3 result = In;
				#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301 || ASE_SRP_VERSION == 70503 || ASE_SRP_VERSION == 70600 || ASE_SRP_VERSION == 70700 || ASE_SRP_VERSION == 70701 || ASE_SRP_VERSION >= 80301
				result *= float3(1,1,-1);
				#endif
				return result;
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.positionWS = vertexInput.positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.positionCS = vertexInput.positionCS;
				o.clipPosV = vertexInput.positionCS;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.positionWS;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				bool enabled20_g123 =(bool)_UnderwaterRenderingEnabled;
				bool submerged20_g123 =(bool)_FullySubmerged;
				float4 ase_screenPosNorm = ScreenPos / ScreenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float textureSample20_g123 = tex2Dlod( _UnderwaterMask, float4( ase_screenPosNorm.xy, 0, 0.0) ).r;
				float localHLSL20_g123 = HLSL20_g123( enabled20_g123 , submerged20_g123 , textureSample20_g123 );
				float2 UV22_g126 = ase_screenPosNorm.xy;
				float2 localUnStereo22_g126 = UnStereo( UV22_g126 );
				float2 break64_g125 = localUnStereo22_g126;
				float clampDepth69_g125 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch38_g125 = ( 1.0 - clampDepth69_g125 );
				#else
				float staticSwitch38_g125 = clampDepth69_g125;
				#endif
				float3 appendResult39_g125 = (float3(break64_g125.x , break64_g125.y , staticSwitch38_g125));
				float4 appendResult42_g125 = (float4((appendResult39_g125*2.0 + -1.0) , 1.0));
				float4 temp_output_43_0_g125 = mul( unity_CameraInvProjection, appendResult42_g125 );
				float3 temp_output_46_0_g125 = ( (temp_output_43_0_g125).xyz / (temp_output_43_0_g125).w );
				float3 In75_g125 = temp_output_46_0_g125;
				float3 localInvertDepthDirURP75_g125 = InvertDepthDirURP75_g125( In75_g125 );
				float4 appendResult49_g125 = (float4(localInvertDepthDirURP75_g125 , 1.0));
				float4 temp_output_18_0_g124 = mul( unity_CameraToWorld, appendResult49_g125 );
				float mulTime63_g124 = _TimeParameters.x * 0.01;
				float eyeDepth31_g124 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float temp_output_121_0_g122 = ( ( 1.0 - saturate( ( ( temp_output_18_0_g124.y - CZY_HeightFogBase ) / ( CZY_HeightFogTransition + ( ( 1.0 - tex2D( _FogVariationTexture, ((temp_output_18_0_g124).xz*( 1.0 / CZY_HeightFogBaseVariationScale ) + mulTime63_g124) ).r ) * CZY_HeightFogBaseVariationAmount ) ) ) ) ) * saturate( ( eyeDepth31_g124 * 0.01 * CZY_HeightFogIntensity ) ) * CZY_HeightFogColor.a );
				float2 UV22_g129 = ase_screenPosNorm.xy;
				float2 localUnStereo22_g129 = UnStereo( UV22_g129 );
				float2 break64_g128 = localUnStereo22_g129;
				float clampDepth69_g128 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch38_g128 = ( 1.0 - clampDepth69_g128 );
				#else
				float staticSwitch38_g128 = clampDepth69_g128;
				#endif
				float3 appendResult39_g128 = (float3(break64_g128.x , break64_g128.y , staticSwitch38_g128));
				float4 appendResult42_g128 = (float4((appendResult39_g128*2.0 + -1.0) , 1.0));
				float4 temp_output_43_0_g128 = mul( unity_CameraInvProjection, appendResult42_g128 );
				float3 temp_output_46_0_g128 = ( (temp_output_43_0_g128).xyz / (temp_output_43_0_g128).w );
				float3 In75_g128 = temp_output_46_0_g128;
				float3 localInvertDepthDirURP75_g128 = InvertDepthDirURP75_g128( In75_g128 );
				float4 appendResult49_g128 = (float4(localInvertDepthDirURP75_g128 , 1.0));
				float4 temp_output_97_0_g127 = mul( unity_CameraToWorld, appendResult49_g128 );
				float preDepth120_g127 = distance( temp_output_97_0_g127 , float4( _WorldSpaceCameraPos , 0.0 ) );
				float lerpResult114_g127 = lerp( preDepth120_g127 , ( preDepth120_g127 * (( 1.0 - CZY_VariationAmount ) + (tex2D( _FogVariationTexture, (( (temp_output_97_0_g127).xz + ( (CZY_VariationWindDirection).xz * _TimeParameters.x ) )*( 0.1 / CZY_VariationScale ) + 0.0) ).r - 0.0) * (1.0 - ( 1.0 - CZY_VariationAmount )) / (1.0 - 0.0)) ) , ( 1.0 - saturate( ( preDepth120_g127 / CZY_VariationDistance ) ) ));
				float newFogDepth103_g127 = lerpResult114_g127;
				float temp_output_15_0_g127 = ( CZY_FogDepthMultiplier * sqrt( newFogDepth103_g127 ) );
				float temp_output_1_0_g132 = temp_output_15_0_g127;
				float4 lerpResult28_g132 = lerp( CZY_FogColor1 , CZY_FogColor2 , saturate( ( temp_output_1_0_g132 / CZY_FogColorStart1 ) ));
				float4 lerpResult41_g132 = lerp( saturate( lerpResult28_g132 ) , CZY_FogColor3 , saturate( ( ( CZY_FogColorStart1 - temp_output_1_0_g132 ) / ( CZY_FogColorStart1 - CZY_FogColorStart2 ) ) ));
				float4 lerpResult35_g132 = lerp( lerpResult41_g132 , CZY_FogColor4 , saturate( ( ( CZY_FogColorStart2 - temp_output_1_0_g132 ) / ( CZY_FogColorStart2 - CZY_FogColorStart3 ) ) ));
				float4 lerpResult113_g132 = lerp( lerpResult35_g132 , CZY_FogColor5 , saturate( ( ( CZY_FogColorStart3 - temp_output_1_0_g132 ) / ( CZY_FogColorStart3 - CZY_FogColorStart4 ) ) ));
				float4 temp_output_157_0_g127 = lerpResult113_g132;
				float temp_output_26_0_g127 = ( (temp_output_157_0_g127).a * saturate( temp_output_15_0_g127 ) );
				float finalAlpha141_g127 = temp_output_26_0_g127;
				float3 normalizeResult160_g127 = normalize( ( WorldPosition - _WorldSpaceCameraPos ) );
				float3 temp_output_91_0_g127 = ( normalizeResult160_g127 * _ProjectionParams.z );
				float3 direction90_g127 = ( temp_output_91_0_g127 - _WorldSpaceCameraPos );
				float3 ase_objectScale = float3( length( GetObjectToWorldMatrix()[ 0 ].xyz ), length( GetObjectToWorldMatrix()[ 1 ].xyz ), length( GetObjectToWorldMatrix()[ 2 ].xyz ) );
				float temp_output_124_56_g122 = ( finalAlpha141_g127 * saturate( ( ( 1.0 - saturate( ( ( ( direction90_g127.y * 0.1 ) * ( 1.0 / ( ( CZY_FogSmoothness * length( ase_objectScale ) ) * 10.0 ) ) ) + ( 1.0 - CZY_FogOffset ) ) ) ) * CZY_FogIntensity ) ) );
				

				float Alpha = ( ( 1.0 - localHLSL20_g123 ) * max( temp_output_121_0_g122 , temp_output_124_56_g122 ) );
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.positionCS );
				#endif
				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "SceneSelectionPass"
			Tags { "LightMode"="SceneSelectionPass" }

			Cull Off
			AlphaToMask Off

			HLSLPROGRAM

			

			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_SRP_VERSION 140010
			#define REQUIRE_DEPTH_TEXTURE 1


			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140010
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#endif
		

			

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			

			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
						#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			float _UnderwaterRenderingEnabled;
			float _FullySubmerged;
			sampler2D _UnderwaterMask;
			float CZY_HeightFogBase;
			float CZY_HeightFogTransition;
			sampler2D _FogVariationTexture;
			float CZY_HeightFogBaseVariationScale;
			float CZY_HeightFogBaseVariationAmount;
			float CZY_HeightFogIntensity;
			float4 CZY_HeightFogColor;
			float4 CZY_FogColor1;
			float4 CZY_FogColor2;
			float CZY_FogDepthMultiplier;
			float3 CZY_VariationWindDirection;
			float CZY_VariationScale;
			float CZY_VariationAmount;
			float CZY_VariationDistance;
			float CZY_FogColorStart1;
			float4 CZY_FogColor3;
			float CZY_FogColorStart2;
			float4 CZY_FogColor4;
			float CZY_FogColorStart3;
			float4 CZY_FogColor5;
			float CZY_FogColorStart4;
			float CZY_FogSmoothness;
			float CZY_FogOffset;
			float CZY_FogIntensity;


			float HLSL20_g123( bool enabled, bool submerged, float textureSample )
			{
				if(enabled)
				{
					if(submerged) return 1.0;
					else return textureSample;
				}
				else
				{
					return 0.0;
				}
			}
			
			float2 UnStereo( float2 UV )
			{
				#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[ unity_StereoEyeIndex ];
				UV.xy = (UV.xy - scaleOffset.zw) / scaleOffset.xy;
				#endif
				return UV;
			}
			
			float3 InvertDepthDirURP75_g125( float3 In )
			{
				float3 result = In;
				#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301 || ASE_SRP_VERSION == 70503 || ASE_SRP_VERSION == 70600 || ASE_SRP_VERSION == 70700 || ASE_SRP_VERSION == 70701 || ASE_SRP_VERSION >= 80301
				result *= float3(1,1,-1);
				#endif
				return result;
			}
			
			float3 InvertDepthDirURP75_g128( float3 In )
			{
				float3 result = In;
				#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301 || ASE_SRP_VERSION == 70503 || ASE_SRP_VERSION == 70600 || ASE_SRP_VERSION == 70700 || ASE_SRP_VERSION == 70701 || ASE_SRP_VERSION >= 80301
				result *= float3(1,1,-1);
				#endif
				return result;
			}
			

			int _ObjectId;
			int _PassValue;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float4 ase_clipPos = TransformObjectToHClip((v.positionOS).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord = screenPos;
				float3 ase_worldPos = TransformObjectToWorld( (v.positionOS).xyz );
				o.ase_texcoord1.xyz = ase_worldPos;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				float3 positionWS = TransformObjectToWorld( v.positionOS.xyz );

				o.positionCS = TransformWorldToHClip(positionWS);

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				bool enabled20_g123 =(bool)_UnderwaterRenderingEnabled;
				bool submerged20_g123 =(bool)_FullySubmerged;
				float4 screenPos = IN.ase_texcoord;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float textureSample20_g123 = tex2Dlod( _UnderwaterMask, float4( ase_screenPosNorm.xy, 0, 0.0) ).r;
				float localHLSL20_g123 = HLSL20_g123( enabled20_g123 , submerged20_g123 , textureSample20_g123 );
				float2 UV22_g126 = ase_screenPosNorm.xy;
				float2 localUnStereo22_g126 = UnStereo( UV22_g126 );
				float2 break64_g125 = localUnStereo22_g126;
				float clampDepth69_g125 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch38_g125 = ( 1.0 - clampDepth69_g125 );
				#else
				float staticSwitch38_g125 = clampDepth69_g125;
				#endif
				float3 appendResult39_g125 = (float3(break64_g125.x , break64_g125.y , staticSwitch38_g125));
				float4 appendResult42_g125 = (float4((appendResult39_g125*2.0 + -1.0) , 1.0));
				float4 temp_output_43_0_g125 = mul( unity_CameraInvProjection, appendResult42_g125 );
				float3 temp_output_46_0_g125 = ( (temp_output_43_0_g125).xyz / (temp_output_43_0_g125).w );
				float3 In75_g125 = temp_output_46_0_g125;
				float3 localInvertDepthDirURP75_g125 = InvertDepthDirURP75_g125( In75_g125 );
				float4 appendResult49_g125 = (float4(localInvertDepthDirURP75_g125 , 1.0));
				float4 temp_output_18_0_g124 = mul( unity_CameraToWorld, appendResult49_g125 );
				float mulTime63_g124 = _TimeParameters.x * 0.01;
				float eyeDepth31_g124 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float temp_output_121_0_g122 = ( ( 1.0 - saturate( ( ( temp_output_18_0_g124.y - CZY_HeightFogBase ) / ( CZY_HeightFogTransition + ( ( 1.0 - tex2D( _FogVariationTexture, ((temp_output_18_0_g124).xz*( 1.0 / CZY_HeightFogBaseVariationScale ) + mulTime63_g124) ).r ) * CZY_HeightFogBaseVariationAmount ) ) ) ) ) * saturate( ( eyeDepth31_g124 * 0.01 * CZY_HeightFogIntensity ) ) * CZY_HeightFogColor.a );
				float2 UV22_g129 = ase_screenPosNorm.xy;
				float2 localUnStereo22_g129 = UnStereo( UV22_g129 );
				float2 break64_g128 = localUnStereo22_g129;
				float clampDepth69_g128 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch38_g128 = ( 1.0 - clampDepth69_g128 );
				#else
				float staticSwitch38_g128 = clampDepth69_g128;
				#endif
				float3 appendResult39_g128 = (float3(break64_g128.x , break64_g128.y , staticSwitch38_g128));
				float4 appendResult42_g128 = (float4((appendResult39_g128*2.0 + -1.0) , 1.0));
				float4 temp_output_43_0_g128 = mul( unity_CameraInvProjection, appendResult42_g128 );
				float3 temp_output_46_0_g128 = ( (temp_output_43_0_g128).xyz / (temp_output_43_0_g128).w );
				float3 In75_g128 = temp_output_46_0_g128;
				float3 localInvertDepthDirURP75_g128 = InvertDepthDirURP75_g128( In75_g128 );
				float4 appendResult49_g128 = (float4(localInvertDepthDirURP75_g128 , 1.0));
				float4 temp_output_97_0_g127 = mul( unity_CameraToWorld, appendResult49_g128 );
				float preDepth120_g127 = distance( temp_output_97_0_g127 , float4( _WorldSpaceCameraPos , 0.0 ) );
				float lerpResult114_g127 = lerp( preDepth120_g127 , ( preDepth120_g127 * (( 1.0 - CZY_VariationAmount ) + (tex2D( _FogVariationTexture, (( (temp_output_97_0_g127).xz + ( (CZY_VariationWindDirection).xz * _TimeParameters.x ) )*( 0.1 / CZY_VariationScale ) + 0.0) ).r - 0.0) * (1.0 - ( 1.0 - CZY_VariationAmount )) / (1.0 - 0.0)) ) , ( 1.0 - saturate( ( preDepth120_g127 / CZY_VariationDistance ) ) ));
				float newFogDepth103_g127 = lerpResult114_g127;
				float temp_output_15_0_g127 = ( CZY_FogDepthMultiplier * sqrt( newFogDepth103_g127 ) );
				float temp_output_1_0_g132 = temp_output_15_0_g127;
				float4 lerpResult28_g132 = lerp( CZY_FogColor1 , CZY_FogColor2 , saturate( ( temp_output_1_0_g132 / CZY_FogColorStart1 ) ));
				float4 lerpResult41_g132 = lerp( saturate( lerpResult28_g132 ) , CZY_FogColor3 , saturate( ( ( CZY_FogColorStart1 - temp_output_1_0_g132 ) / ( CZY_FogColorStart1 - CZY_FogColorStart2 ) ) ));
				float4 lerpResult35_g132 = lerp( lerpResult41_g132 , CZY_FogColor4 , saturate( ( ( CZY_FogColorStart2 - temp_output_1_0_g132 ) / ( CZY_FogColorStart2 - CZY_FogColorStart3 ) ) ));
				float4 lerpResult113_g132 = lerp( lerpResult35_g132 , CZY_FogColor5 , saturate( ( ( CZY_FogColorStart3 - temp_output_1_0_g132 ) / ( CZY_FogColorStart3 - CZY_FogColorStart4 ) ) ));
				float4 temp_output_157_0_g127 = lerpResult113_g132;
				float temp_output_26_0_g127 = ( (temp_output_157_0_g127).a * saturate( temp_output_15_0_g127 ) );
				float finalAlpha141_g127 = temp_output_26_0_g127;
				float3 ase_worldPos = IN.ase_texcoord1.xyz;
				float3 normalizeResult160_g127 = normalize( ( ase_worldPos - _WorldSpaceCameraPos ) );
				float3 temp_output_91_0_g127 = ( normalizeResult160_g127 * _ProjectionParams.z );
				float3 direction90_g127 = ( temp_output_91_0_g127 - _WorldSpaceCameraPos );
				float3 ase_objectScale = float3( length( GetObjectToWorldMatrix()[ 0 ].xyz ), length( GetObjectToWorldMatrix()[ 1 ].xyz ), length( GetObjectToWorldMatrix()[ 2 ].xyz ) );
				float temp_output_124_56_g122 = ( finalAlpha141_g127 * saturate( ( ( 1.0 - saturate( ( ( ( direction90_g127.y * 0.1 ) * ( 1.0 / ( ( CZY_FogSmoothness * length( ase_objectScale ) ) * 10.0 ) ) ) + ( 1.0 - CZY_FogOffset ) ) ) ) * CZY_FogIntensity ) ) );
				

				surfaceDescription.Alpha = ( ( 1.0 - localHLSL20_g123 ) * max( temp_output_121_0_g122 , temp_output_124_56_g122 ) );
				surfaceDescription.AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				return outColor;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ScenePickingPass"
			Tags { "LightMode"="Picking" }

			AlphaToMask Off

			HLSLPROGRAM

			

			#define _SURFACE_TYPE_TRANSPARENT 1
			#define ASE_SRP_VERSION 140010
			#define REQUIRE_DEPTH_TEXTURE 1


			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT

			#define SHADERPASS SHADERPASS_DEPTHONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140010
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#endif
		

			

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			

			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
						#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			float _UnderwaterRenderingEnabled;
			float _FullySubmerged;
			sampler2D _UnderwaterMask;
			float CZY_HeightFogBase;
			float CZY_HeightFogTransition;
			sampler2D _FogVariationTexture;
			float CZY_HeightFogBaseVariationScale;
			float CZY_HeightFogBaseVariationAmount;
			float CZY_HeightFogIntensity;
			float4 CZY_HeightFogColor;
			float4 CZY_FogColor1;
			float4 CZY_FogColor2;
			float CZY_FogDepthMultiplier;
			float3 CZY_VariationWindDirection;
			float CZY_VariationScale;
			float CZY_VariationAmount;
			float CZY_VariationDistance;
			float CZY_FogColorStart1;
			float4 CZY_FogColor3;
			float CZY_FogColorStart2;
			float4 CZY_FogColor4;
			float CZY_FogColorStart3;
			float4 CZY_FogColor5;
			float CZY_FogColorStart4;
			float CZY_FogSmoothness;
			float CZY_FogOffset;
			float CZY_FogIntensity;


			float HLSL20_g123( bool enabled, bool submerged, float textureSample )
			{
				if(enabled)
				{
					if(submerged) return 1.0;
					else return textureSample;
				}
				else
				{
					return 0.0;
				}
			}
			
			float2 UnStereo( float2 UV )
			{
				#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[ unity_StereoEyeIndex ];
				UV.xy = (UV.xy - scaleOffset.zw) / scaleOffset.xy;
				#endif
				return UV;
			}
			
			float3 InvertDepthDirURP75_g125( float3 In )
			{
				float3 result = In;
				#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301 || ASE_SRP_VERSION == 70503 || ASE_SRP_VERSION == 70600 || ASE_SRP_VERSION == 70700 || ASE_SRP_VERSION == 70701 || ASE_SRP_VERSION >= 80301
				result *= float3(1,1,-1);
				#endif
				return result;
			}
			
			float3 InvertDepthDirURP75_g128( float3 In )
			{
				float3 result = In;
				#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301 || ASE_SRP_VERSION == 70503 || ASE_SRP_VERSION == 70600 || ASE_SRP_VERSION == 70700 || ASE_SRP_VERSION == 70701 || ASE_SRP_VERSION >= 80301
				result *= float3(1,1,-1);
				#endif
				return result;
			}
			

			float4 _SelectionID;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float4 ase_clipPos = TransformObjectToHClip((v.positionOS).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord = screenPos;
				float3 ase_worldPos = TransformObjectToWorld( (v.positionOS).xyz );
				o.ase_texcoord1.xyz = ase_worldPos;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				float3 positionWS = TransformObjectToWorld( v.positionOS.xyz );
				o.positionCS = TransformWorldToHClip(positionWS);
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				bool enabled20_g123 =(bool)_UnderwaterRenderingEnabled;
				bool submerged20_g123 =(bool)_FullySubmerged;
				float4 screenPos = IN.ase_texcoord;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float textureSample20_g123 = tex2Dlod( _UnderwaterMask, float4( ase_screenPosNorm.xy, 0, 0.0) ).r;
				float localHLSL20_g123 = HLSL20_g123( enabled20_g123 , submerged20_g123 , textureSample20_g123 );
				float2 UV22_g126 = ase_screenPosNorm.xy;
				float2 localUnStereo22_g126 = UnStereo( UV22_g126 );
				float2 break64_g125 = localUnStereo22_g126;
				float clampDepth69_g125 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch38_g125 = ( 1.0 - clampDepth69_g125 );
				#else
				float staticSwitch38_g125 = clampDepth69_g125;
				#endif
				float3 appendResult39_g125 = (float3(break64_g125.x , break64_g125.y , staticSwitch38_g125));
				float4 appendResult42_g125 = (float4((appendResult39_g125*2.0 + -1.0) , 1.0));
				float4 temp_output_43_0_g125 = mul( unity_CameraInvProjection, appendResult42_g125 );
				float3 temp_output_46_0_g125 = ( (temp_output_43_0_g125).xyz / (temp_output_43_0_g125).w );
				float3 In75_g125 = temp_output_46_0_g125;
				float3 localInvertDepthDirURP75_g125 = InvertDepthDirURP75_g125( In75_g125 );
				float4 appendResult49_g125 = (float4(localInvertDepthDirURP75_g125 , 1.0));
				float4 temp_output_18_0_g124 = mul( unity_CameraToWorld, appendResult49_g125 );
				float mulTime63_g124 = _TimeParameters.x * 0.01;
				float eyeDepth31_g124 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float temp_output_121_0_g122 = ( ( 1.0 - saturate( ( ( temp_output_18_0_g124.y - CZY_HeightFogBase ) / ( CZY_HeightFogTransition + ( ( 1.0 - tex2D( _FogVariationTexture, ((temp_output_18_0_g124).xz*( 1.0 / CZY_HeightFogBaseVariationScale ) + mulTime63_g124) ).r ) * CZY_HeightFogBaseVariationAmount ) ) ) ) ) * saturate( ( eyeDepth31_g124 * 0.01 * CZY_HeightFogIntensity ) ) * CZY_HeightFogColor.a );
				float2 UV22_g129 = ase_screenPosNorm.xy;
				float2 localUnStereo22_g129 = UnStereo( UV22_g129 );
				float2 break64_g128 = localUnStereo22_g129;
				float clampDepth69_g128 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch38_g128 = ( 1.0 - clampDepth69_g128 );
				#else
				float staticSwitch38_g128 = clampDepth69_g128;
				#endif
				float3 appendResult39_g128 = (float3(break64_g128.x , break64_g128.y , staticSwitch38_g128));
				float4 appendResult42_g128 = (float4((appendResult39_g128*2.0 + -1.0) , 1.0));
				float4 temp_output_43_0_g128 = mul( unity_CameraInvProjection, appendResult42_g128 );
				float3 temp_output_46_0_g128 = ( (temp_output_43_0_g128).xyz / (temp_output_43_0_g128).w );
				float3 In75_g128 = temp_output_46_0_g128;
				float3 localInvertDepthDirURP75_g128 = InvertDepthDirURP75_g128( In75_g128 );
				float4 appendResult49_g128 = (float4(localInvertDepthDirURP75_g128 , 1.0));
				float4 temp_output_97_0_g127 = mul( unity_CameraToWorld, appendResult49_g128 );
				float preDepth120_g127 = distance( temp_output_97_0_g127 , float4( _WorldSpaceCameraPos , 0.0 ) );
				float lerpResult114_g127 = lerp( preDepth120_g127 , ( preDepth120_g127 * (( 1.0 - CZY_VariationAmount ) + (tex2D( _FogVariationTexture, (( (temp_output_97_0_g127).xz + ( (CZY_VariationWindDirection).xz * _TimeParameters.x ) )*( 0.1 / CZY_VariationScale ) + 0.0) ).r - 0.0) * (1.0 - ( 1.0 - CZY_VariationAmount )) / (1.0 - 0.0)) ) , ( 1.0 - saturate( ( preDepth120_g127 / CZY_VariationDistance ) ) ));
				float newFogDepth103_g127 = lerpResult114_g127;
				float temp_output_15_0_g127 = ( CZY_FogDepthMultiplier * sqrt( newFogDepth103_g127 ) );
				float temp_output_1_0_g132 = temp_output_15_0_g127;
				float4 lerpResult28_g132 = lerp( CZY_FogColor1 , CZY_FogColor2 , saturate( ( temp_output_1_0_g132 / CZY_FogColorStart1 ) ));
				float4 lerpResult41_g132 = lerp( saturate( lerpResult28_g132 ) , CZY_FogColor3 , saturate( ( ( CZY_FogColorStart1 - temp_output_1_0_g132 ) / ( CZY_FogColorStart1 - CZY_FogColorStart2 ) ) ));
				float4 lerpResult35_g132 = lerp( lerpResult41_g132 , CZY_FogColor4 , saturate( ( ( CZY_FogColorStart2 - temp_output_1_0_g132 ) / ( CZY_FogColorStart2 - CZY_FogColorStart3 ) ) ));
				float4 lerpResult113_g132 = lerp( lerpResult35_g132 , CZY_FogColor5 , saturate( ( ( CZY_FogColorStart3 - temp_output_1_0_g132 ) / ( CZY_FogColorStart3 - CZY_FogColorStart4 ) ) ));
				float4 temp_output_157_0_g127 = lerpResult113_g132;
				float temp_output_26_0_g127 = ( (temp_output_157_0_g127).a * saturate( temp_output_15_0_g127 ) );
				float finalAlpha141_g127 = temp_output_26_0_g127;
				float3 ase_worldPos = IN.ase_texcoord1.xyz;
				float3 normalizeResult160_g127 = normalize( ( ase_worldPos - _WorldSpaceCameraPos ) );
				float3 temp_output_91_0_g127 = ( normalizeResult160_g127 * _ProjectionParams.z );
				float3 direction90_g127 = ( temp_output_91_0_g127 - _WorldSpaceCameraPos );
				float3 ase_objectScale = float3( length( GetObjectToWorldMatrix()[ 0 ].xyz ), length( GetObjectToWorldMatrix()[ 1 ].xyz ), length( GetObjectToWorldMatrix()[ 2 ].xyz ) );
				float temp_output_124_56_g122 = ( finalAlpha141_g127 * saturate( ( ( 1.0 - saturate( ( ( ( direction90_g127.y * 0.1 ) * ( 1.0 / ( ( CZY_FogSmoothness * length( ase_objectScale ) ) * 10.0 ) ) ) + ( 1.0 - CZY_FogOffset ) ) ) ) * CZY_FogIntensity ) ) );
				

				surfaceDescription.Alpha = ( ( 1.0 - localHLSL20_g123 ) * max( temp_output_121_0_g122 , temp_output_124_56_g122 ) );
				surfaceDescription.AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = 0;
				outColor = _SelectionID;

				return outColor;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormalsOnly" }

			ZTest LEqual
			ZWrite On

			HLSLPROGRAM

			

        	#pragma multi_compile_instancing
        	#define _SURFACE_TYPE_TRANSPARENT 1
        	#define ASE_SRP_VERSION 140010
        	#define REQUIRE_DEPTH_TEXTURE 1


			

        	#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define VARYINGS_NEED_NORMAL_WS

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140010
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#endif
		

			

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

            #if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#define ASE_NEEDS_FRAG_SCREEN_POSITION


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				float3 normalWS : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
						#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			float _UnderwaterRenderingEnabled;
			float _FullySubmerged;
			sampler2D _UnderwaterMask;
			float CZY_HeightFogBase;
			float CZY_HeightFogTransition;
			sampler2D _FogVariationTexture;
			float CZY_HeightFogBaseVariationScale;
			float CZY_HeightFogBaseVariationAmount;
			float CZY_HeightFogIntensity;
			float4 CZY_HeightFogColor;
			float4 CZY_FogColor1;
			float4 CZY_FogColor2;
			float CZY_FogDepthMultiplier;
			float3 CZY_VariationWindDirection;
			float CZY_VariationScale;
			float CZY_VariationAmount;
			float CZY_VariationDistance;
			float CZY_FogColorStart1;
			float4 CZY_FogColor3;
			float CZY_FogColorStart2;
			float4 CZY_FogColor4;
			float CZY_FogColorStart3;
			float4 CZY_FogColor5;
			float CZY_FogColorStart4;
			float CZY_FogSmoothness;
			float CZY_FogOffset;
			float CZY_FogIntensity;


			float HLSL20_g123( bool enabled, bool submerged, float textureSample )
			{
				if(enabled)
				{
					if(submerged) return 1.0;
					else return textureSample;
				}
				else
				{
					return 0.0;
				}
			}
			
			float2 UnStereo( float2 UV )
			{
				#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[ unity_StereoEyeIndex ];
				UV.xy = (UV.xy - scaleOffset.zw) / scaleOffset.xy;
				#endif
				return UV;
			}
			
			float3 InvertDepthDirURP75_g125( float3 In )
			{
				float3 result = In;
				#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301 || ASE_SRP_VERSION == 70503 || ASE_SRP_VERSION == 70600 || ASE_SRP_VERSION == 70700 || ASE_SRP_VERSION == 70701 || ASE_SRP_VERSION >= 80301
				result *= float3(1,1,-1);
				#endif
				return result;
			}
			
			float3 InvertDepthDirURP75_g128( float3 In )
			{
				float3 result = In;
				#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301 || ASE_SRP_VERSION == 70503 || ASE_SRP_VERSION == 70600 || ASE_SRP_VERSION == 70700 || ASE_SRP_VERSION == 70701 || ASE_SRP_VERSION >= 80301
				result *= float3(1,1,-1);
				#endif
				return result;
			}
			

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = TransformObjectToWorld( (v.positionOS).xyz );
				o.ase_texcoord2.xyz = ase_worldPos;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				o.positionCS = vertexInput.positionCS;
				o.clipPosV = vertexInput.positionCS;
				o.normalWS = TransformObjectToWorldNormal( v.normalOS );
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			void frag( VertexOutput IN
				, out half4 outNormalWS : SV_Target0
			#ifdef _WRITE_RENDERING_LAYERS
				, out float4 outRenderingLayers : SV_Target1
			#endif
				 )
			{
				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				bool enabled20_g123 =(bool)_UnderwaterRenderingEnabled;
				bool submerged20_g123 =(bool)_FullySubmerged;
				float4 ase_screenPosNorm = ScreenPos / ScreenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float textureSample20_g123 = tex2Dlod( _UnderwaterMask, float4( ase_screenPosNorm.xy, 0, 0.0) ).r;
				float localHLSL20_g123 = HLSL20_g123( enabled20_g123 , submerged20_g123 , textureSample20_g123 );
				float2 UV22_g126 = ase_screenPosNorm.xy;
				float2 localUnStereo22_g126 = UnStereo( UV22_g126 );
				float2 break64_g125 = localUnStereo22_g126;
				float clampDepth69_g125 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch38_g125 = ( 1.0 - clampDepth69_g125 );
				#else
				float staticSwitch38_g125 = clampDepth69_g125;
				#endif
				float3 appendResult39_g125 = (float3(break64_g125.x , break64_g125.y , staticSwitch38_g125));
				float4 appendResult42_g125 = (float4((appendResult39_g125*2.0 + -1.0) , 1.0));
				float4 temp_output_43_0_g125 = mul( unity_CameraInvProjection, appendResult42_g125 );
				float3 temp_output_46_0_g125 = ( (temp_output_43_0_g125).xyz / (temp_output_43_0_g125).w );
				float3 In75_g125 = temp_output_46_0_g125;
				float3 localInvertDepthDirURP75_g125 = InvertDepthDirURP75_g125( In75_g125 );
				float4 appendResult49_g125 = (float4(localInvertDepthDirURP75_g125 , 1.0));
				float4 temp_output_18_0_g124 = mul( unity_CameraToWorld, appendResult49_g125 );
				float mulTime63_g124 = _TimeParameters.x * 0.01;
				float eyeDepth31_g124 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float temp_output_121_0_g122 = ( ( 1.0 - saturate( ( ( temp_output_18_0_g124.y - CZY_HeightFogBase ) / ( CZY_HeightFogTransition + ( ( 1.0 - tex2D( _FogVariationTexture, ((temp_output_18_0_g124).xz*( 1.0 / CZY_HeightFogBaseVariationScale ) + mulTime63_g124) ).r ) * CZY_HeightFogBaseVariationAmount ) ) ) ) ) * saturate( ( eyeDepth31_g124 * 0.01 * CZY_HeightFogIntensity ) ) * CZY_HeightFogColor.a );
				float2 UV22_g129 = ase_screenPosNorm.xy;
				float2 localUnStereo22_g129 = UnStereo( UV22_g129 );
				float2 break64_g128 = localUnStereo22_g129;
				float clampDepth69_g128 = SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch38_g128 = ( 1.0 - clampDepth69_g128 );
				#else
				float staticSwitch38_g128 = clampDepth69_g128;
				#endif
				float3 appendResult39_g128 = (float3(break64_g128.x , break64_g128.y , staticSwitch38_g128));
				float4 appendResult42_g128 = (float4((appendResult39_g128*2.0 + -1.0) , 1.0));
				float4 temp_output_43_0_g128 = mul( unity_CameraInvProjection, appendResult42_g128 );
				float3 temp_output_46_0_g128 = ( (temp_output_43_0_g128).xyz / (temp_output_43_0_g128).w );
				float3 In75_g128 = temp_output_46_0_g128;
				float3 localInvertDepthDirURP75_g128 = InvertDepthDirURP75_g128( In75_g128 );
				float4 appendResult49_g128 = (float4(localInvertDepthDirURP75_g128 , 1.0));
				float4 temp_output_97_0_g127 = mul( unity_CameraToWorld, appendResult49_g128 );
				float preDepth120_g127 = distance( temp_output_97_0_g127 , float4( _WorldSpaceCameraPos , 0.0 ) );
				float lerpResult114_g127 = lerp( preDepth120_g127 , ( preDepth120_g127 * (( 1.0 - CZY_VariationAmount ) + (tex2D( _FogVariationTexture, (( (temp_output_97_0_g127).xz + ( (CZY_VariationWindDirection).xz * _TimeParameters.x ) )*( 0.1 / CZY_VariationScale ) + 0.0) ).r - 0.0) * (1.0 - ( 1.0 - CZY_VariationAmount )) / (1.0 - 0.0)) ) , ( 1.0 - saturate( ( preDepth120_g127 / CZY_VariationDistance ) ) ));
				float newFogDepth103_g127 = lerpResult114_g127;
				float temp_output_15_0_g127 = ( CZY_FogDepthMultiplier * sqrt( newFogDepth103_g127 ) );
				float temp_output_1_0_g132 = temp_output_15_0_g127;
				float4 lerpResult28_g132 = lerp( CZY_FogColor1 , CZY_FogColor2 , saturate( ( temp_output_1_0_g132 / CZY_FogColorStart1 ) ));
				float4 lerpResult41_g132 = lerp( saturate( lerpResult28_g132 ) , CZY_FogColor3 , saturate( ( ( CZY_FogColorStart1 - temp_output_1_0_g132 ) / ( CZY_FogColorStart1 - CZY_FogColorStart2 ) ) ));
				float4 lerpResult35_g132 = lerp( lerpResult41_g132 , CZY_FogColor4 , saturate( ( ( CZY_FogColorStart2 - temp_output_1_0_g132 ) / ( CZY_FogColorStart2 - CZY_FogColorStart3 ) ) ));
				float4 lerpResult113_g132 = lerp( lerpResult35_g132 , CZY_FogColor5 , saturate( ( ( CZY_FogColorStart3 - temp_output_1_0_g132 ) / ( CZY_FogColorStart3 - CZY_FogColorStart4 ) ) ));
				float4 temp_output_157_0_g127 = lerpResult113_g132;
				float temp_output_26_0_g127 = ( (temp_output_157_0_g127).a * saturate( temp_output_15_0_g127 ) );
				float finalAlpha141_g127 = temp_output_26_0_g127;
				float3 ase_worldPos = IN.ase_texcoord2.xyz;
				float3 normalizeResult160_g127 = normalize( ( ase_worldPos - _WorldSpaceCameraPos ) );
				float3 temp_output_91_0_g127 = ( normalizeResult160_g127 * _ProjectionParams.z );
				float3 direction90_g127 = ( temp_output_91_0_g127 - _WorldSpaceCameraPos );
				float3 ase_objectScale = float3( length( GetObjectToWorldMatrix()[ 0 ].xyz ), length( GetObjectToWorldMatrix()[ 1 ].xyz ), length( GetObjectToWorldMatrix()[ 2 ].xyz ) );
				float temp_output_124_56_g122 = ( finalAlpha141_g127 * saturate( ( ( 1.0 - saturate( ( ( ( direction90_g127.y * 0.1 ) * ( 1.0 / ( ( CZY_FogSmoothness * length( ase_objectScale ) ) * 10.0 ) ) ) + ( 1.0 - CZY_FogOffset ) ) ) ) * CZY_FogIntensity ) ) );
				

				float Alpha = ( ( 1.0 - localHLSL20_g123 ) * max( temp_output_121_0_g122 , temp_output_124_56_g122 ) );
				float AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.positionCS );
				#endif

				#if defined(_GBUFFER_NORMALS_OCT)
					float3 normalWS = normalize(IN.normalWS);
					float2 octNormalWS = PackNormalOctQuadEncode(normalWS);           // values between [-1, +1], must use fp32 on some platforms
					float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);   // values between [ 0,  1]
					half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);      // values between [ 0,  1]
					outNormalWS = half4(packedNormalWS, 0.0);
				#else
					float3 normalWS = IN.normalWS;
					outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
				#endif
			}

			ENDHLSL
		}

	
	}
	
	CustomEditor "EmptyShaderGUI"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback "Hidden/InternalErrorShader"
}
/*ASEBEGIN
Version=19501
Node;AmplifyShaderEditor.FunctionNode;540;1168,-736;Inherit;False;Stylized Fog (Physical Height);0;;122;6863d88adda26194cbbb00d58f08515c;0;0;2;COLOR;0;FLOAT;123
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;280;768,448;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;363;570.9207,90.06461;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=Universal2D;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;364;570.9207,90.06461;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;SceneSelectionPass;0;6;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;365;570.9207,90.06461;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ScenePickingPass;0;7;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;366;570.9207,90.06461;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormals;0;8;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;367;570.9207,90.06461;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormalsOnly;0;9;DepthNormalsOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;True;9;d3d11;metal;vulkan;xboxone;xboxseries;playstation;ps4;ps5;switch;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;282;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;283;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;284;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;281;1456,-736;Float;False;True;-1;2;EmptyShaderGUI;0;13;Distant Lands/Cozy/URP/Stylized Fog (Physical Height);2992e84f91cbeb14eab234972e07ea9d;True;Forward;0;1;Forward;8;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;1;False;;False;False;False;False;False;False;False;False;True;True;True;221;False;;222;False;;222;False;;6;False;;1;False;;0;False;;0;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=-100;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;5;False;;10;False;;1;1;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;True;True;2;False;;True;7;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalForward;False;False;0;Hidden/InternalErrorShader;0;0;Standard;22;Surface;1;637952286753557635;  Blend;0;0;Two Sided;2;637952286781860590;Forward Only;0;0;Cast Shadows;0;637995616325711392;  Use Shadow Threshold;0;0;Receive Shadows;1;0;GPU Instancing;1;0;LOD CrossFade;0;0;Built-in Fog;0;0;Meta Pass;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Vertex Position,InvertActionOnDeselection;1;0;0;10;False;True;False;True;False;False;True;True;True;False;False;;False;0
WireConnection;281;2;540;0
WireConnection;281;3;540;123
ASEEND*/
//CHKSM=FEE492DD2987E2A3048063126CBF98B326D7E80E