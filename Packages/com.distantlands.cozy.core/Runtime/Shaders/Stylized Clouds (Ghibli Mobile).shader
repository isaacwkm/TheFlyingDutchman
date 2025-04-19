// Made with Amplify Shader Editor v1.9.5.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Distant Lands/Cozy/URP/Stylized Clouds (Ghibli Mobile)"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)


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

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Transparent-50" "UniversalMaterialType"="Unlit" }

		Cull Front
		AlphaToMask Off

		Stencil
		{
			Ref 221
			Comp Always
			Pass Zero
			Fail Keep
			ZFail Keep
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
			Tags { "LightMode"="UniversalForwardOnly" }

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			

			HLSLPROGRAM

			

			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#define _ALPHATEST_ON 1
			#define ASE_SRP_VERSION 140010


			

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

			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_SCREEN_POSITION


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
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
				float4 ase_texcoord4 : TEXCOORD4;
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

			float4 CZY_CloudHighlightColor;
			float CZY_FilterSaturation;
			float CZY_FilterValue;
			float4 CZY_FilterColor;
			float4 CZY_CloudFilterColor;
			float4 CZY_CloudColor;
			float4 CZY_CloudTextureColor;
			float CZY_Spherize;
			float CZY_WindSpeed;
			float CZY_CloudCohesion;
			float CZY_CumulusCoverageMultiplier;
			float CZY_MainCloudScale;
			float CZY_ShadowingDistance;
			float4 CZY_LightColor;
			float4 CZY_FogColor5;
			float CZY_LightFlareSquish;
			float3 CZY_SunDirection;
			half CZY_LightIntensity;
			half CZY_LightFalloff;
			float CZY_CloudsFogLightAmount;
			float4 CZY_SunFilterColor;
			float3 CZY_MoonDirection;
			float4 CZY_FogMoonFlareColor;
			float CZY_CloudsFogAmount;
			float CZY_FogSmoothness;
			float CZY_FogOffset;
			float CZY_FogIntensity;
			float _UnderwaterRenderingEnabled;
			float _FullySubmerged;
			sampler2D _UnderwaterMask;
			float CZY_ClippingThreshold;


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
			
			float2 voronoihash35_g75( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi35_g75( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash35_g75( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float2 voronoihash13_g75( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi13_g75( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash13_g75( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float2 voronoihash11_g75( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi11_g75( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash11_g75( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float2 voronoihash35_g78( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi35_g78( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash35_g78( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float2 voronoihash13_g78( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi13_g78( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash13_g78( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float2 voronoihash11_g78( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi11_g78( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash11_g78( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float4 SampleGradient( Gradient gradient, float time )
			{
				float3 color = gradient.colors[0].rgb;
				UNITY_UNROLL
				for (int c = 1; c < 8; c++)
				{
				float colorPos = saturate((time - gradient.colors[c-1].w) / ( 0.00001 + (gradient.colors[c].w - gradient.colors[c-1].w)) * step(c, gradient.colorsLength-1));
				color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
				}
				#ifndef UNITY_COLORSPACE_GAMMA
				color = SRGBToLinear(color);
				#endif
				float alpha = gradient.alphas[0].x;
				UNITY_UNROLL
				for (int a = 1; a < 8; a++)
				{
				float alphaPos = saturate((time - gradient.alphas[a-1].y) / ( 0.00001 + (gradient.alphas[a].y - gradient.alphas[a-1].y)) * step(a, gradient.alphasLength-1));
				alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
				}
				return float4(color, alpha);
			}
			
			float2 voronoihash35_g74( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi35_g74( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash35_g74( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float2 voronoihash13_g74( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi13_g74( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash13_g74( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float2 voronoihash11_g74( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi11_g74( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash11_g74( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float HLSL20_g79( bool enabled, bool submerged, float textureSample )
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

				o.ase_texcoord4.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.zw = 0;

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
				float4 ase_texcoord : TEXCOORD0;

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
				o.ase_texcoord = v.ase_texcoord;
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
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
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

				float3 hsvTorgb2_g77 = RGBToHSV( CZY_CloudHighlightColor.rgb );
				float3 hsvTorgb3_g77 = HSVToRGB( float3(hsvTorgb2_g77.x,saturate( ( hsvTorgb2_g77.y + CZY_FilterSaturation ) ),( hsvTorgb2_g77.z + CZY_FilterValue )) );
				float4 temp_output_10_0_g77 = ( float4( hsvTorgb3_g77 , 0.0 ) * CZY_FilterColor );
				float4 CloudHighlightColor91_g73 = ( temp_output_10_0_g77 * CZY_CloudFilterColor );
				float3 hsvTorgb2_g76 = RGBToHSV( CZY_CloudColor.rgb );
				float3 hsvTorgb3_g76 = HSVToRGB( float3(hsvTorgb2_g76.x,saturate( ( hsvTorgb2_g76.y + CZY_FilterSaturation ) ),( hsvTorgb2_g76.z + CZY_FilterValue )) );
				float4 temp_output_10_0_g76 = ( float4( hsvTorgb3_g76 , 0.0 ) * CZY_FilterColor );
				float4 CloudColor73_g73 = ( temp_output_10_0_g76 * CZY_CloudFilterColor );
				Gradient gradient68_g73 = NewGradient( 0, 2, 2, float4( 0, 0, 0, 0.8676432 ), float4( 1, 1, 1, 0.9294118 ), 0, 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
				float2 texCoord11_g73 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float2 temp_output_54_0_g73 = ( texCoord11_g73 - float2( 0.5,0.5 ) );
				float dotResult23_g73 = dot( temp_output_54_0_g73 , temp_output_54_0_g73 );
				float Dot28_g73 = saturate( (0.85 + (dotResult23_g73 - 0.0) * (3.0 - 0.85) / (1.0 - 0.0)) );
				float time35_g75 = 0.0;
				float2 voronoiSmoothId35_g75 = 0;
				float2 texCoord21_g73 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float2 CentralUV17_g73 = ( texCoord21_g73 + float2( -0.5,-0.5 ) );
				float2 temp_output_21_0_g75 = (CentralUV17_g73*1.58 + 0.0);
				float2 break2_g75 = abs( temp_output_21_0_g75 );
				float saferPower4_g75 = abs( break2_g75.x );
				float saferPower3_g75 = abs( break2_g75.y );
				float saferPower6_g75 = abs( ( pow( saferPower4_g75 , 2.0 ) + pow( saferPower3_g75 , 2.0 ) ) );
				float Spherize29_g73 = CZY_Spherize;
				float Flatness30_g73 = ( 20.0 * CZY_Spherize );
				float mulTime14_g73 = _TimeParameters.x * ( 0.001 * CZY_WindSpeed );
				float Time8_g73 = mulTime14_g73;
				float2 Wind51_g73 = ( Time8_g73 * float2( 0.1,0.2 ) );
				float2 temp_output_10_0_g75 = (( ( temp_output_21_0_g75 * ( pow( saferPower6_g75 , Spherize29_g73 ) * Flatness30_g73 ) ) + float2( 0.5,0.5 ) )*( 2.0 / 5.0 ) + Wind51_g73);
				float2 coords35_g75 = temp_output_10_0_g75 * 60.0;
				float2 id35_g75 = 0;
				float2 uv35_g75 = 0;
				float fade35_g75 = 0.5;
				float voroi35_g75 = 0;
				float rest35_g75 = 0;
				for( int it35_g75 = 0; it35_g75 <2; it35_g75++ ){
				voroi35_g75 += fade35_g75 * voronoi35_g75( coords35_g75, time35_g75, id35_g75, uv35_g75, 0,voronoiSmoothId35_g75 );
				rest35_g75 += fade35_g75;
				coords35_g75 *= 2;
				fade35_g75 *= 0.5;
				}//Voronoi35_g75
				voroi35_g75 /= rest35_g75;
				float time13_g75 = 0.0;
				float2 voronoiSmoothId13_g75 = 0;
				float2 coords13_g75 = temp_output_10_0_g75 * 25.0;
				float2 id13_g75 = 0;
				float2 uv13_g75 = 0;
				float fade13_g75 = 0.5;
				float voroi13_g75 = 0;
				float rest13_g75 = 0;
				for( int it13_g75 = 0; it13_g75 <2; it13_g75++ ){
				voroi13_g75 += fade13_g75 * voronoi13_g75( coords13_g75, time13_g75, id13_g75, uv13_g75, 0,voronoiSmoothId13_g75 );
				rest13_g75 += fade13_g75;
				coords13_g75 *= 2;
				fade13_g75 *= 0.5;
				}//Voronoi13_g75
				voroi13_g75 /= rest13_g75;
				float time11_g75 = 17.23;
				float2 voronoiSmoothId11_g75 = 0;
				float2 coords11_g75 = temp_output_10_0_g75 * 9.0;
				float2 id11_g75 = 0;
				float2 uv11_g75 = 0;
				float fade11_g75 = 0.5;
				float voroi11_g75 = 0;
				float rest11_g75 = 0;
				for( int it11_g75 = 0; it11_g75 <2; it11_g75++ ){
				voroi11_g75 += fade11_g75 * voronoi11_g75( coords11_g75, time11_g75, id11_g75, uv11_g75, 0,voronoiSmoothId11_g75 );
				rest11_g75 += fade11_g75;
				coords11_g75 *= 2;
				fade11_g75 *= 0.5;
				}//Voronoi11_g75
				voroi11_g75 /= rest11_g75;
				float2 texCoord12_g73 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float2 temp_output_15_0_g73 = ( texCoord12_g73 - float2( 0.5,0.5 ) );
				float dotResult7_g73 = dot( temp_output_15_0_g73 , temp_output_15_0_g73 );
				float ModifiedCohesion22_g73 = ( CZY_CloudCohesion * 1.0 * ( 1.0 - dotResult7_g73 ) );
				float lerpResult15_g75 = lerp( saturate( ( voroi35_g75 + voroi13_g75 ) ) , voroi11_g75 , ModifiedCohesion22_g73);
				float CumulusCoverage113_g73 = CZY_CumulusCoverageMultiplier;
				float lerpResult16_g75 = lerp( lerpResult15_g75 , 1.0 , ( ( 1.0 - CumulusCoverage113_g73 ) + -0.7 ));
				float time35_g78 = 0.0;
				float2 voronoiSmoothId35_g78 = 0;
				float2 temp_output_21_0_g78 = CentralUV17_g73;
				float2 break2_g78 = abs( temp_output_21_0_g78 );
				float saferPower4_g78 = abs( break2_g78.x );
				float saferPower3_g78 = abs( break2_g78.y );
				float saferPower6_g78 = abs( ( pow( saferPower4_g78 , 2.0 ) + pow( saferPower3_g78 , 2.0 ) ) );
				float Scale24_g73 = ( CZY_MainCloudScale * 0.1 );
				float2 temp_output_10_0_g78 = (( ( temp_output_21_0_g78 * ( pow( saferPower6_g78 , Spherize29_g73 ) * Flatness30_g73 ) ) + float2( 0.5,0.5 ) )*( 2.0 / ( Scale24_g73 * 1.5 ) ) + ( Wind51_g73 * float2( 0.5,0.5 ) ));
				float2 coords35_g78 = temp_output_10_0_g78 * 60.0;
				float2 id35_g78 = 0;
				float2 uv35_g78 = 0;
				float fade35_g78 = 0.5;
				float voroi35_g78 = 0;
				float rest35_g78 = 0;
				for( int it35_g78 = 0; it35_g78 <2; it35_g78++ ){
				voroi35_g78 += fade35_g78 * voronoi35_g78( coords35_g78, time35_g78, id35_g78, uv35_g78, 0,voronoiSmoothId35_g78 );
				rest35_g78 += fade35_g78;
				coords35_g78 *= 2;
				fade35_g78 *= 0.5;
				}//Voronoi35_g78
				voroi35_g78 /= rest35_g78;
				float time13_g78 = 0.0;
				float2 voronoiSmoothId13_g78 = 0;
				float2 coords13_g78 = temp_output_10_0_g78 * 25.0;
				float2 id13_g78 = 0;
				float2 uv13_g78 = 0;
				float fade13_g78 = 0.5;
				float voroi13_g78 = 0;
				float rest13_g78 = 0;
				for( int it13_g78 = 0; it13_g78 <2; it13_g78++ ){
				voroi13_g78 += fade13_g78 * voronoi13_g78( coords13_g78, time13_g78, id13_g78, uv13_g78, 0,voronoiSmoothId13_g78 );
				rest13_g78 += fade13_g78;
				coords13_g78 *= 2;
				fade13_g78 *= 0.5;
				}//Voronoi13_g78
				voroi13_g78 /= rest13_g78;
				float time11_g78 = 17.23;
				float2 voronoiSmoothId11_g78 = 0;
				float2 coords11_g78 = temp_output_10_0_g78 * 9.0;
				float2 id11_g78 = 0;
				float2 uv11_g78 = 0;
				float fade11_g78 = 0.5;
				float voroi11_g78 = 0;
				float rest11_g78 = 0;
				for( int it11_g78 = 0; it11_g78 <2; it11_g78++ ){
				voroi11_g78 += fade11_g78 * voronoi11_g78( coords11_g78, time11_g78, id11_g78, uv11_g78, 0,voronoiSmoothId11_g78 );
				rest11_g78 += fade11_g78;
				coords11_g78 *= 2;
				fade11_g78 *= 0.5;
				}//Voronoi11_g78
				voroi11_g78 /= rest11_g78;
				float lerpResult15_g78 = lerp( saturate( ( voroi35_g78 + voroi13_g78 ) ) , voroi11_g78 , ( ModifiedCohesion22_g73 * 1.1 ));
				float lerpResult16_g78 = lerp( lerpResult15_g78 , 1.0 , ( ( 1.0 - CumulusCoverage113_g73 ) + -0.7 ));
				float temp_output_25_0_g73 = saturate( (0.0 + (( Dot28_g73 * ( 1.0 - lerpResult16_g78 ) ) - 0.6) * (1.0 - 0.0) / (1.0 - 0.6)) );
				float IT2PreAlpha80_g73 = temp_output_25_0_g73;
				float temp_output_77_0_g73 = (0.0 + (( Dot28_g73 * ( 1.0 - lerpResult16_g75 ) ) - 0.6) * (IT2PreAlpha80_g73 - 0.0) / (1.5 - 0.6));
				float clampResult71_g73 = clamp( temp_output_77_0_g73 , 0.0 , 0.9 );
				float AdditionalLayer66_g73 = SampleGradient( gradient68_g73, clampResult71_g73 ).r;
				float4 lerpResult88_g73 = lerp( CloudColor73_g73 , ( CloudColor73_g73 * CZY_CloudTextureColor ) , AdditionalLayer66_g73);
				float4 ModifiedCloudColor97_g73 = lerpResult88_g73;
				Gradient gradient87_g73 = NewGradient( 0, 2, 2, float4( 0.06119964, 0.06119964, 0.06119964, 0.4411841 ), float4( 1, 1, 1, 0.5794156 ), 0, 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
				float time35_g74 = 0.0;
				float2 voronoiSmoothId35_g74 = 0;
				float2 ShadowUV59_g73 = ( CentralUV17_g73 + ( CentralUV17_g73 * float2( -1,-1 ) * CZY_ShadowingDistance * Dot28_g73 ) );
				float2 temp_output_21_0_g74 = ShadowUV59_g73;
				float2 break2_g74 = abs( temp_output_21_0_g74 );
				float saferPower4_g74 = abs( break2_g74.x );
				float saferPower3_g74 = abs( break2_g74.y );
				float saferPower6_g74 = abs( ( pow( saferPower4_g74 , 2.0 ) + pow( saferPower3_g74 , 2.0 ) ) );
				float2 temp_output_10_0_g74 = (( ( temp_output_21_0_g74 * ( pow( saferPower6_g74 , Spherize29_g73 ) * Flatness30_g73 ) ) + float2( 0.5,0.5 ) )*( 2.0 / ( Scale24_g73 * 1.5 ) ) + ( Wind51_g73 * float2( 0.5,0.5 ) ));
				float2 coords35_g74 = temp_output_10_0_g74 * 60.0;
				float2 id35_g74 = 0;
				float2 uv35_g74 = 0;
				float fade35_g74 = 0.5;
				float voroi35_g74 = 0;
				float rest35_g74 = 0;
				for( int it35_g74 = 0; it35_g74 <2; it35_g74++ ){
				voroi35_g74 += fade35_g74 * voronoi35_g74( coords35_g74, time35_g74, id35_g74, uv35_g74, 0,voronoiSmoothId35_g74 );
				rest35_g74 += fade35_g74;
				coords35_g74 *= 2;
				fade35_g74 *= 0.5;
				}//Voronoi35_g74
				voroi35_g74 /= rest35_g74;
				float time13_g74 = 0.0;
				float2 voronoiSmoothId13_g74 = 0;
				float2 coords13_g74 = temp_output_10_0_g74 * 25.0;
				float2 id13_g74 = 0;
				float2 uv13_g74 = 0;
				float fade13_g74 = 0.5;
				float voroi13_g74 = 0;
				float rest13_g74 = 0;
				for( int it13_g74 = 0; it13_g74 <2; it13_g74++ ){
				voroi13_g74 += fade13_g74 * voronoi13_g74( coords13_g74, time13_g74, id13_g74, uv13_g74, 0,voronoiSmoothId13_g74 );
				rest13_g74 += fade13_g74;
				coords13_g74 *= 2;
				fade13_g74 *= 0.5;
				}//Voronoi13_g74
				voroi13_g74 /= rest13_g74;
				float time11_g74 = 17.23;
				float2 voronoiSmoothId11_g74 = 0;
				float2 coords11_g74 = temp_output_10_0_g74 * 9.0;
				float2 id11_g74 = 0;
				float2 uv11_g74 = 0;
				float fade11_g74 = 0.5;
				float voroi11_g74 = 0;
				float rest11_g74 = 0;
				for( int it11_g74 = 0; it11_g74 <2; it11_g74++ ){
				voroi11_g74 += fade11_g74 * voronoi11_g74( coords11_g74, time11_g74, id11_g74, uv11_g74, 0,voronoiSmoothId11_g74 );
				rest11_g74 += fade11_g74;
				coords11_g74 *= 2;
				fade11_g74 *= 0.5;
				}//Voronoi11_g74
				voroi11_g74 /= rest11_g74;
				float lerpResult15_g74 = lerp( saturate( ( voroi35_g74 + voroi13_g74 ) ) , voroi11_g74 , ( ModifiedCohesion22_g73 * 1.1 ));
				float lerpResult16_g74 = lerp( lerpResult15_g74 , 1.0 , ( ( 1.0 - CumulusCoverage113_g73 ) + -0.7 ));
				float4 lerpResult104_g73 = lerp( CloudHighlightColor91_g73 , ModifiedCloudColor97_g73 , saturate( SampleGradient( gradient87_g73, saturate( (0.0 + (( Dot28_g73 * ( 1.0 - lerpResult16_g74 ) ) - 0.6) * (1.0 - 0.0) / (1.0 - 0.6)) ) ).r ));
				float4 IT2Color100_g73 = lerpResult104_g73;
				float3 hsvTorgb69_g369 = RGBToHSV( CZY_FogColor5.rgb );
				float3 normalizeResult54_g369 = normalize( ( WorldPosition - _WorldSpaceCameraPos ) );
				float3 temp_output_56_0_g369 = ( normalizeResult54_g369 * _ProjectionParams.z );
				float3 appendResult25_g369 = (float3(1.0 , CZY_LightFlareSquish , 1.0));
				float3 normalizeResult13_g369 = normalize( ( ( temp_output_56_0_g369 * appendResult25_g369 ) - _WorldSpaceCameraPos ) );
				float dotResult16_g369 = dot( normalizeResult13_g369 , CZY_SunDirection );
				half LightMask35_g369 = saturate( pow( abs( ( (dotResult16_g369*0.5 + 0.5) * CZY_LightIntensity ) ) , CZY_LightFalloff ) );
				float temp_output_91_0_g369 = CZY_CloudsFogLightAmount;
				float3 hsvTorgb2_g371 = RGBToHSV( ( CZY_LightColor * hsvTorgb69_g369.z * saturate( ( LightMask35_g369 * ( 1.5 * CZY_FogColor5.a ) * temp_output_91_0_g369 ) ) ).rgb );
				float3 hsvTorgb3_g371 = HSVToRGB( float3(hsvTorgb2_g371.x,saturate( ( hsvTorgb2_g371.y + CZY_FilterSaturation ) ),( hsvTorgb2_g371.z + CZY_FilterValue )) );
				float4 temp_output_10_0_g371 = ( float4( hsvTorgb3_g371 , 0.0 ) * CZY_FilterColor );
				float3 direction88_g369 = ( temp_output_56_0_g369 - _WorldSpaceCameraPos );
				float3 normalizeResult32_g369 = normalize( direction88_g369 );
				float3 normalizeResult30_g369 = normalize( CZY_MoonDirection );
				float dotResult28_g369 = dot( normalizeResult32_g369 , normalizeResult30_g369 );
				half MoonMask18_g369 = saturate( pow( abs( ( saturate( (dotResult28_g369*1.0 + 0.0) ) * CZY_LightIntensity ) ) , ( CZY_LightFalloff * 3.0 ) ) );
				float3 hsvTorgb2_g370 = RGBToHSV( ( CZY_FogColor5 + ( hsvTorgb69_g369.z * saturate( ( CZY_FogColor5.a * MoonMask18_g369 * temp_output_91_0_g369 ) ) * CZY_FogMoonFlareColor ) ).rgb );
				float3 hsvTorgb3_g370 = HSVToRGB( float3(hsvTorgb2_g370.x,saturate( ( hsvTorgb2_g370.y + CZY_FilterSaturation ) ),( hsvTorgb2_g370.z + CZY_FilterValue )) );
				float4 temp_output_10_0_g370 = ( float4( hsvTorgb3_g370 , 0.0 ) * CZY_FilterColor );
				float3 ase_objectScale = float3( length( GetObjectToWorldMatrix()[ 0 ].xyz ), length( GetObjectToWorldMatrix()[ 1 ].xyz ), length( GetObjectToWorldMatrix()[ 2 ].xyz ) );
				float temp_output_34_0_g369 = ( CZY_CloudsFogAmount * saturate( ( ( 1.0 - saturate( ( ( ( direction88_g369.y * 0.1 ) * ( 1.0 / ( ( CZY_FogSmoothness * length( ase_objectScale ) ) * 10.0 ) ) ) + ( 1.0 - CZY_FogOffset ) ) ) ) * CZY_FogIntensity ) ) );
				float4 lerpResult90_g369 = lerp( IT2Color100_g73 , ( ( temp_output_10_0_g371 * CZY_SunFilterColor ) + temp_output_10_0_g370 ) , temp_output_34_0_g369);
				
				Gradient gradient93_g73 = NewGradient( 0, 2, 2, float4( 0.06119964, 0.06119964, 0.06119964, 0.4617685 ), float4( 1, 1, 1, 0.5117723 ), 0, 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
				float IT2Alpha101_g73 = SampleGradient( gradient93_g73, temp_output_25_0_g73 ).r;
				bool enabled20_g79 =(bool)_UnderwaterRenderingEnabled;
				bool submerged20_g79 =(bool)_FullySubmerged;
				float4 ase_screenPosNorm = ScreenPos / ScreenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float textureSample20_g79 = tex2Dlod( _UnderwaterMask, float4( ase_screenPosNorm.xy, 0, 0.0) ).r;
				float localHLSL20_g79 = HLSL20_g79( enabled20_g79 , submerged20_g79 , textureSample20_g79 );
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = lerpResult90_g369.rgb;
				float Alpha = ( IT2Alpha101_g73 * ( 1.0 - localHLSL20_g79 ) );
				float AlphaClipThreshold = CZY_ClippingThreshold;
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
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			AlphaToMask Off
			ColorMask 0

			HLSLPROGRAM

			

			#pragma multi_compile_instancing
			#define _ALPHATEST_ON 1
			#define ASE_SRP_VERSION 140010


			

			#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_SHADOWCASTER

			
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

			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 positionWS : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
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

			float CZY_Spherize;
			float CZY_MainCloudScale;
			float CZY_WindSpeed;
			float CZY_CloudCohesion;
			float CZY_CumulusCoverageMultiplier;
			float _UnderwaterRenderingEnabled;
			float _FullySubmerged;
			sampler2D _UnderwaterMask;
			float CZY_ClippingThreshold;


			
			float2 voronoihash35_g78( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi35_g78( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash35_g78( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float2 voronoihash13_g78( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi13_g78( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash13_g78( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float2 voronoihash11_g78( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi11_g78( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash11_g78( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float4 SampleGradient( Gradient gradient, float time )
			{
				float3 color = gradient.colors[0].rgb;
				UNITY_UNROLL
				for (int c = 1; c < 8; c++)
				{
				float colorPos = saturate((time - gradient.colors[c-1].w) / ( 0.00001 + (gradient.colors[c].w - gradient.colors[c-1].w)) * step(c, gradient.colorsLength-1));
				color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
				}
				#ifndef UNITY_COLORSPACE_GAMMA
				color = SRGBToLinear(color);
				#endif
				float alpha = gradient.alphas[0].x;
				UNITY_UNROLL
				for (int a = 1; a < 8; a++)
				{
				float alphaPos = saturate((time - gradient.alphas[a-1].y) / ( 0.00001 + (gradient.alphas[a].y - gradient.alphas[a-1].y)) * step(a, gradient.alphasLength-1));
				alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
				}
				return float4(color, alpha);
			}
			
			float HLSL20_g79( bool enabled, bool submerged, float textureSample )
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
			

			float3 _LightDirection;
			float3 _LightPosition;

			VertexOutput VertexFunction( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float4 ase_clipPos = TransformObjectToHClip((v.positionOS).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord3 = screenPos;
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;

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

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.positionWS = positionWS;
				#endif

				float3 normalWS = TransformObjectToWorldDir( v.normalOS );

				#if _CASTING_PUNCTUAL_LIGHT_SHADOW
					float3 lightDirectionWS = normalize(_LightPosition - positionWS);
				#else
					float3 lightDirectionWS = _LightDirection;
				#endif

				float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

				#if UNITY_REVERSED_Z
					positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#else
					positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.positionCS = positionCS;

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

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
				o.ase_texcoord = v.ase_texcoord;
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
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
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
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.positionWS;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				Gradient gradient93_g73 = NewGradient( 0, 2, 2, float4( 0.06119964, 0.06119964, 0.06119964, 0.4617685 ), float4( 1, 1, 1, 0.5117723 ), 0, 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
				float2 texCoord11_g73 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 temp_output_54_0_g73 = ( texCoord11_g73 - float2( 0.5,0.5 ) );
				float dotResult23_g73 = dot( temp_output_54_0_g73 , temp_output_54_0_g73 );
				float Dot28_g73 = saturate( (0.85 + (dotResult23_g73 - 0.0) * (3.0 - 0.85) / (1.0 - 0.0)) );
				float time35_g78 = 0.0;
				float2 voronoiSmoothId35_g78 = 0;
				float2 texCoord21_g73 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 CentralUV17_g73 = ( texCoord21_g73 + float2( -0.5,-0.5 ) );
				float2 temp_output_21_0_g78 = CentralUV17_g73;
				float2 break2_g78 = abs( temp_output_21_0_g78 );
				float saferPower4_g78 = abs( break2_g78.x );
				float saferPower3_g78 = abs( break2_g78.y );
				float saferPower6_g78 = abs( ( pow( saferPower4_g78 , 2.0 ) + pow( saferPower3_g78 , 2.0 ) ) );
				float Spherize29_g73 = CZY_Spherize;
				float Flatness30_g73 = ( 20.0 * CZY_Spherize );
				float Scale24_g73 = ( CZY_MainCloudScale * 0.1 );
				float mulTime14_g73 = _TimeParameters.x * ( 0.001 * CZY_WindSpeed );
				float Time8_g73 = mulTime14_g73;
				float2 Wind51_g73 = ( Time8_g73 * float2( 0.1,0.2 ) );
				float2 temp_output_10_0_g78 = (( ( temp_output_21_0_g78 * ( pow( saferPower6_g78 , Spherize29_g73 ) * Flatness30_g73 ) ) + float2( 0.5,0.5 ) )*( 2.0 / ( Scale24_g73 * 1.5 ) ) + ( Wind51_g73 * float2( 0.5,0.5 ) ));
				float2 coords35_g78 = temp_output_10_0_g78 * 60.0;
				float2 id35_g78 = 0;
				float2 uv35_g78 = 0;
				float fade35_g78 = 0.5;
				float voroi35_g78 = 0;
				float rest35_g78 = 0;
				for( int it35_g78 = 0; it35_g78 <2; it35_g78++ ){
				voroi35_g78 += fade35_g78 * voronoi35_g78( coords35_g78, time35_g78, id35_g78, uv35_g78, 0,voronoiSmoothId35_g78 );
				rest35_g78 += fade35_g78;
				coords35_g78 *= 2;
				fade35_g78 *= 0.5;
				}//Voronoi35_g78
				voroi35_g78 /= rest35_g78;
				float time13_g78 = 0.0;
				float2 voronoiSmoothId13_g78 = 0;
				float2 coords13_g78 = temp_output_10_0_g78 * 25.0;
				float2 id13_g78 = 0;
				float2 uv13_g78 = 0;
				float fade13_g78 = 0.5;
				float voroi13_g78 = 0;
				float rest13_g78 = 0;
				for( int it13_g78 = 0; it13_g78 <2; it13_g78++ ){
				voroi13_g78 += fade13_g78 * voronoi13_g78( coords13_g78, time13_g78, id13_g78, uv13_g78, 0,voronoiSmoothId13_g78 );
				rest13_g78 += fade13_g78;
				coords13_g78 *= 2;
				fade13_g78 *= 0.5;
				}//Voronoi13_g78
				voroi13_g78 /= rest13_g78;
				float time11_g78 = 17.23;
				float2 voronoiSmoothId11_g78 = 0;
				float2 coords11_g78 = temp_output_10_0_g78 * 9.0;
				float2 id11_g78 = 0;
				float2 uv11_g78 = 0;
				float fade11_g78 = 0.5;
				float voroi11_g78 = 0;
				float rest11_g78 = 0;
				for( int it11_g78 = 0; it11_g78 <2; it11_g78++ ){
				voroi11_g78 += fade11_g78 * voronoi11_g78( coords11_g78, time11_g78, id11_g78, uv11_g78, 0,voronoiSmoothId11_g78 );
				rest11_g78 += fade11_g78;
				coords11_g78 *= 2;
				fade11_g78 *= 0.5;
				}//Voronoi11_g78
				voroi11_g78 /= rest11_g78;
				float2 texCoord12_g73 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 temp_output_15_0_g73 = ( texCoord12_g73 - float2( 0.5,0.5 ) );
				float dotResult7_g73 = dot( temp_output_15_0_g73 , temp_output_15_0_g73 );
				float ModifiedCohesion22_g73 = ( CZY_CloudCohesion * 1.0 * ( 1.0 - dotResult7_g73 ) );
				float lerpResult15_g78 = lerp( saturate( ( voroi35_g78 + voroi13_g78 ) ) , voroi11_g78 , ( ModifiedCohesion22_g73 * 1.1 ));
				float CumulusCoverage113_g73 = CZY_CumulusCoverageMultiplier;
				float lerpResult16_g78 = lerp( lerpResult15_g78 , 1.0 , ( ( 1.0 - CumulusCoverage113_g73 ) + -0.7 ));
				float temp_output_25_0_g73 = saturate( (0.0 + (( Dot28_g73 * ( 1.0 - lerpResult16_g78 ) ) - 0.6) * (1.0 - 0.0) / (1.0 - 0.6)) );
				float IT2Alpha101_g73 = SampleGradient( gradient93_g73, temp_output_25_0_g73 ).r;
				bool enabled20_g79 =(bool)_UnderwaterRenderingEnabled;
				bool submerged20_g79 =(bool)_FullySubmerged;
				float4 screenPos = IN.ase_texcoord3;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float textureSample20_g79 = tex2Dlod( _UnderwaterMask, float4( ase_screenPosNorm.xy, 0, 0.0) ).r;
				float localHLSL20_g79 = HLSL20_g79( enabled20_g79 , submerged20_g79 , textureSample20_g79 );
				

				float Alpha = ( IT2Alpha101_g73 * ( 1.0 - localHLSL20_g79 ) );
				float AlphaClipThreshold = CZY_ClippingThreshold;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_ON
					#ifdef _ALPHATEST_SHADOW_ON
						clip(Alpha - AlphaClipThresholdShadow);
					#else
						clip(Alpha - AlphaClipThreshold);
					#endif
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
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0
			AlphaToMask Off

			HLSLPROGRAM

			

			#pragma multi_compile_instancing
			#define _ALPHATEST_ON 1
			#define ASE_SRP_VERSION 140010


			

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

			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
			#define ASE_NEEDS_FRAG_SCREEN_POSITION


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
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
				float4 ase_texcoord3 : TEXCOORD3;
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

			float CZY_Spherize;
			float CZY_MainCloudScale;
			float CZY_WindSpeed;
			float CZY_CloudCohesion;
			float CZY_CumulusCoverageMultiplier;
			float _UnderwaterRenderingEnabled;
			float _FullySubmerged;
			sampler2D _UnderwaterMask;
			float CZY_ClippingThreshold;


			
			float2 voronoihash35_g78( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi35_g78( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash35_g78( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float2 voronoihash13_g78( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi13_g78( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash13_g78( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float2 voronoihash11_g78( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi11_g78( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash11_g78( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float4 SampleGradient( Gradient gradient, float time )
			{
				float3 color = gradient.colors[0].rgb;
				UNITY_UNROLL
				for (int c = 1; c < 8; c++)
				{
				float colorPos = saturate((time - gradient.colors[c-1].w) / ( 0.00001 + (gradient.colors[c].w - gradient.colors[c-1].w)) * step(c, gradient.colorsLength-1));
				color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
				}
				#ifndef UNITY_COLORSPACE_GAMMA
				color = SRGBToLinear(color);
				#endif
				float alpha = gradient.alphas[0].x;
				UNITY_UNROLL
				for (int a = 1; a < 8; a++)
				{
				float alphaPos = saturate((time - gradient.alphas[a-1].y) / ( 0.00001 + (gradient.alphas[a].y - gradient.alphas[a-1].y)) * step(a, gradient.alphasLength-1));
				alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
				}
				return float4(color, alpha);
			}
			
			float HLSL20_g79( bool enabled, bool submerged, float textureSample )
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

				o.ase_texcoord3.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.zw = 0;

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
				float4 ase_texcoord : TEXCOORD0;

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
				o.ase_texcoord = v.ase_texcoord;
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
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
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

				Gradient gradient93_g73 = NewGradient( 0, 2, 2, float4( 0.06119964, 0.06119964, 0.06119964, 0.4617685 ), float4( 1, 1, 1, 0.5117723 ), 0, 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
				float2 texCoord11_g73 = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 temp_output_54_0_g73 = ( texCoord11_g73 - float2( 0.5,0.5 ) );
				float dotResult23_g73 = dot( temp_output_54_0_g73 , temp_output_54_0_g73 );
				float Dot28_g73 = saturate( (0.85 + (dotResult23_g73 - 0.0) * (3.0 - 0.85) / (1.0 - 0.0)) );
				float time35_g78 = 0.0;
				float2 voronoiSmoothId35_g78 = 0;
				float2 texCoord21_g73 = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 CentralUV17_g73 = ( texCoord21_g73 + float2( -0.5,-0.5 ) );
				float2 temp_output_21_0_g78 = CentralUV17_g73;
				float2 break2_g78 = abs( temp_output_21_0_g78 );
				float saferPower4_g78 = abs( break2_g78.x );
				float saferPower3_g78 = abs( break2_g78.y );
				float saferPower6_g78 = abs( ( pow( saferPower4_g78 , 2.0 ) + pow( saferPower3_g78 , 2.0 ) ) );
				float Spherize29_g73 = CZY_Spherize;
				float Flatness30_g73 = ( 20.0 * CZY_Spherize );
				float Scale24_g73 = ( CZY_MainCloudScale * 0.1 );
				float mulTime14_g73 = _TimeParameters.x * ( 0.001 * CZY_WindSpeed );
				float Time8_g73 = mulTime14_g73;
				float2 Wind51_g73 = ( Time8_g73 * float2( 0.1,0.2 ) );
				float2 temp_output_10_0_g78 = (( ( temp_output_21_0_g78 * ( pow( saferPower6_g78 , Spherize29_g73 ) * Flatness30_g73 ) ) + float2( 0.5,0.5 ) )*( 2.0 / ( Scale24_g73 * 1.5 ) ) + ( Wind51_g73 * float2( 0.5,0.5 ) ));
				float2 coords35_g78 = temp_output_10_0_g78 * 60.0;
				float2 id35_g78 = 0;
				float2 uv35_g78 = 0;
				float fade35_g78 = 0.5;
				float voroi35_g78 = 0;
				float rest35_g78 = 0;
				for( int it35_g78 = 0; it35_g78 <2; it35_g78++ ){
				voroi35_g78 += fade35_g78 * voronoi35_g78( coords35_g78, time35_g78, id35_g78, uv35_g78, 0,voronoiSmoothId35_g78 );
				rest35_g78 += fade35_g78;
				coords35_g78 *= 2;
				fade35_g78 *= 0.5;
				}//Voronoi35_g78
				voroi35_g78 /= rest35_g78;
				float time13_g78 = 0.0;
				float2 voronoiSmoothId13_g78 = 0;
				float2 coords13_g78 = temp_output_10_0_g78 * 25.0;
				float2 id13_g78 = 0;
				float2 uv13_g78 = 0;
				float fade13_g78 = 0.5;
				float voroi13_g78 = 0;
				float rest13_g78 = 0;
				for( int it13_g78 = 0; it13_g78 <2; it13_g78++ ){
				voroi13_g78 += fade13_g78 * voronoi13_g78( coords13_g78, time13_g78, id13_g78, uv13_g78, 0,voronoiSmoothId13_g78 );
				rest13_g78 += fade13_g78;
				coords13_g78 *= 2;
				fade13_g78 *= 0.5;
				}//Voronoi13_g78
				voroi13_g78 /= rest13_g78;
				float time11_g78 = 17.23;
				float2 voronoiSmoothId11_g78 = 0;
				float2 coords11_g78 = temp_output_10_0_g78 * 9.0;
				float2 id11_g78 = 0;
				float2 uv11_g78 = 0;
				float fade11_g78 = 0.5;
				float voroi11_g78 = 0;
				float rest11_g78 = 0;
				for( int it11_g78 = 0; it11_g78 <2; it11_g78++ ){
				voroi11_g78 += fade11_g78 * voronoi11_g78( coords11_g78, time11_g78, id11_g78, uv11_g78, 0,voronoiSmoothId11_g78 );
				rest11_g78 += fade11_g78;
				coords11_g78 *= 2;
				fade11_g78 *= 0.5;
				}//Voronoi11_g78
				voroi11_g78 /= rest11_g78;
				float2 texCoord12_g73 = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float2 temp_output_15_0_g73 = ( texCoord12_g73 - float2( 0.5,0.5 ) );
				float dotResult7_g73 = dot( temp_output_15_0_g73 , temp_output_15_0_g73 );
				float ModifiedCohesion22_g73 = ( CZY_CloudCohesion * 1.0 * ( 1.0 - dotResult7_g73 ) );
				float lerpResult15_g78 = lerp( saturate( ( voroi35_g78 + voroi13_g78 ) ) , voroi11_g78 , ( ModifiedCohesion22_g73 * 1.1 ));
				float CumulusCoverage113_g73 = CZY_CumulusCoverageMultiplier;
				float lerpResult16_g78 = lerp( lerpResult15_g78 , 1.0 , ( ( 1.0 - CumulusCoverage113_g73 ) + -0.7 ));
				float temp_output_25_0_g73 = saturate( (0.0 + (( Dot28_g73 * ( 1.0 - lerpResult16_g78 ) ) - 0.6) * (1.0 - 0.0) / (1.0 - 0.6)) );
				float IT2Alpha101_g73 = SampleGradient( gradient93_g73, temp_output_25_0_g73 ).r;
				bool enabled20_g79 =(bool)_UnderwaterRenderingEnabled;
				bool submerged20_g79 =(bool)_FullySubmerged;
				float4 ase_screenPosNorm = ScreenPos / ScreenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float textureSample20_g79 = tex2Dlod( _UnderwaterMask, float4( ase_screenPosNorm.xy, 0, 0.0) ).r;
				float localHLSL20_g79 = HLSL20_g79( enabled20_g79 , submerged20_g79 , textureSample20_g79 );
				

				float Alpha = ( IT2Alpha101_g73 * ( 1.0 - localHLSL20_g79 ) );
				float AlphaClipThreshold = CZY_ClippingThreshold;

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

			

			#define _ALPHATEST_ON 1
			#define ASE_SRP_VERSION 140010


			

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

			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
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

			float CZY_Spherize;
			float CZY_MainCloudScale;
			float CZY_WindSpeed;
			float CZY_CloudCohesion;
			float CZY_CumulusCoverageMultiplier;
			float _UnderwaterRenderingEnabled;
			float _FullySubmerged;
			sampler2D _UnderwaterMask;
			float CZY_ClippingThreshold;


			
			float2 voronoihash35_g78( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi35_g78( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash35_g78( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float2 voronoihash13_g78( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi13_g78( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash13_g78( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float2 voronoihash11_g78( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi11_g78( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash11_g78( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float4 SampleGradient( Gradient gradient, float time )
			{
				float3 color = gradient.colors[0].rgb;
				UNITY_UNROLL
				for (int c = 1; c < 8; c++)
				{
				float colorPos = saturate((time - gradient.colors[c-1].w) / ( 0.00001 + (gradient.colors[c].w - gradient.colors[c-1].w)) * step(c, gradient.colorsLength-1));
				color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
				}
				#ifndef UNITY_COLORSPACE_GAMMA
				color = SRGBToLinear(color);
				#endif
				float alpha = gradient.alphas[0].x;
				UNITY_UNROLL
				for (int a = 1; a < 8; a++)
				{
				float alphaPos = saturate((time - gradient.alphas[a-1].y) / ( 0.00001 + (gradient.alphas[a].y - gradient.alphas[a-1].y)) * step(a, gradient.alphasLength-1));
				alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
				}
				return float4(color, alpha);
			}
			
			float HLSL20_g79( bool enabled, bool submerged, float textureSample )
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
				o.ase_texcoord1 = screenPos;
				
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;

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
				float4 ase_texcoord : TEXCOORD0;

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
				o.ase_texcoord = v.ase_texcoord;
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
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
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

				Gradient gradient93_g73 = NewGradient( 0, 2, 2, float4( 0.06119964, 0.06119964, 0.06119964, 0.4617685 ), float4( 1, 1, 1, 0.5117723 ), 0, 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
				float2 texCoord11_g73 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 temp_output_54_0_g73 = ( texCoord11_g73 - float2( 0.5,0.5 ) );
				float dotResult23_g73 = dot( temp_output_54_0_g73 , temp_output_54_0_g73 );
				float Dot28_g73 = saturate( (0.85 + (dotResult23_g73 - 0.0) * (3.0 - 0.85) / (1.0 - 0.0)) );
				float time35_g78 = 0.0;
				float2 voronoiSmoothId35_g78 = 0;
				float2 texCoord21_g73 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 CentralUV17_g73 = ( texCoord21_g73 + float2( -0.5,-0.5 ) );
				float2 temp_output_21_0_g78 = CentralUV17_g73;
				float2 break2_g78 = abs( temp_output_21_0_g78 );
				float saferPower4_g78 = abs( break2_g78.x );
				float saferPower3_g78 = abs( break2_g78.y );
				float saferPower6_g78 = abs( ( pow( saferPower4_g78 , 2.0 ) + pow( saferPower3_g78 , 2.0 ) ) );
				float Spherize29_g73 = CZY_Spherize;
				float Flatness30_g73 = ( 20.0 * CZY_Spherize );
				float Scale24_g73 = ( CZY_MainCloudScale * 0.1 );
				float mulTime14_g73 = _TimeParameters.x * ( 0.001 * CZY_WindSpeed );
				float Time8_g73 = mulTime14_g73;
				float2 Wind51_g73 = ( Time8_g73 * float2( 0.1,0.2 ) );
				float2 temp_output_10_0_g78 = (( ( temp_output_21_0_g78 * ( pow( saferPower6_g78 , Spherize29_g73 ) * Flatness30_g73 ) ) + float2( 0.5,0.5 ) )*( 2.0 / ( Scale24_g73 * 1.5 ) ) + ( Wind51_g73 * float2( 0.5,0.5 ) ));
				float2 coords35_g78 = temp_output_10_0_g78 * 60.0;
				float2 id35_g78 = 0;
				float2 uv35_g78 = 0;
				float fade35_g78 = 0.5;
				float voroi35_g78 = 0;
				float rest35_g78 = 0;
				for( int it35_g78 = 0; it35_g78 <2; it35_g78++ ){
				voroi35_g78 += fade35_g78 * voronoi35_g78( coords35_g78, time35_g78, id35_g78, uv35_g78, 0,voronoiSmoothId35_g78 );
				rest35_g78 += fade35_g78;
				coords35_g78 *= 2;
				fade35_g78 *= 0.5;
				}//Voronoi35_g78
				voroi35_g78 /= rest35_g78;
				float time13_g78 = 0.0;
				float2 voronoiSmoothId13_g78 = 0;
				float2 coords13_g78 = temp_output_10_0_g78 * 25.0;
				float2 id13_g78 = 0;
				float2 uv13_g78 = 0;
				float fade13_g78 = 0.5;
				float voroi13_g78 = 0;
				float rest13_g78 = 0;
				for( int it13_g78 = 0; it13_g78 <2; it13_g78++ ){
				voroi13_g78 += fade13_g78 * voronoi13_g78( coords13_g78, time13_g78, id13_g78, uv13_g78, 0,voronoiSmoothId13_g78 );
				rest13_g78 += fade13_g78;
				coords13_g78 *= 2;
				fade13_g78 *= 0.5;
				}//Voronoi13_g78
				voroi13_g78 /= rest13_g78;
				float time11_g78 = 17.23;
				float2 voronoiSmoothId11_g78 = 0;
				float2 coords11_g78 = temp_output_10_0_g78 * 9.0;
				float2 id11_g78 = 0;
				float2 uv11_g78 = 0;
				float fade11_g78 = 0.5;
				float voroi11_g78 = 0;
				float rest11_g78 = 0;
				for( int it11_g78 = 0; it11_g78 <2; it11_g78++ ){
				voroi11_g78 += fade11_g78 * voronoi11_g78( coords11_g78, time11_g78, id11_g78, uv11_g78, 0,voronoiSmoothId11_g78 );
				rest11_g78 += fade11_g78;
				coords11_g78 *= 2;
				fade11_g78 *= 0.5;
				}//Voronoi11_g78
				voroi11_g78 /= rest11_g78;
				float2 texCoord12_g73 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 temp_output_15_0_g73 = ( texCoord12_g73 - float2( 0.5,0.5 ) );
				float dotResult7_g73 = dot( temp_output_15_0_g73 , temp_output_15_0_g73 );
				float ModifiedCohesion22_g73 = ( CZY_CloudCohesion * 1.0 * ( 1.0 - dotResult7_g73 ) );
				float lerpResult15_g78 = lerp( saturate( ( voroi35_g78 + voroi13_g78 ) ) , voroi11_g78 , ( ModifiedCohesion22_g73 * 1.1 ));
				float CumulusCoverage113_g73 = CZY_CumulusCoverageMultiplier;
				float lerpResult16_g78 = lerp( lerpResult15_g78 , 1.0 , ( ( 1.0 - CumulusCoverage113_g73 ) + -0.7 ));
				float temp_output_25_0_g73 = saturate( (0.0 + (( Dot28_g73 * ( 1.0 - lerpResult16_g78 ) ) - 0.6) * (1.0 - 0.0) / (1.0 - 0.6)) );
				float IT2Alpha101_g73 = SampleGradient( gradient93_g73, temp_output_25_0_g73 ).r;
				bool enabled20_g79 =(bool)_UnderwaterRenderingEnabled;
				bool submerged20_g79 =(bool)_FullySubmerged;
				float4 screenPos = IN.ase_texcoord1;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float textureSample20_g79 = tex2Dlod( _UnderwaterMask, float4( ase_screenPosNorm.xy, 0, 0.0) ).r;
				float localHLSL20_g79 = HLSL20_g79( enabled20_g79 , submerged20_g79 , textureSample20_g79 );
				

				surfaceDescription.Alpha = ( IT2Alpha101_g73 * ( 1.0 - localHLSL20_g79 ) );
				surfaceDescription.AlphaClipThreshold = CZY_ClippingThreshold;

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

			

			#define _ALPHATEST_ON 1
			#define ASE_SRP_VERSION 140010


			

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

			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
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

			float CZY_Spherize;
			float CZY_MainCloudScale;
			float CZY_WindSpeed;
			float CZY_CloudCohesion;
			float CZY_CumulusCoverageMultiplier;
			float _UnderwaterRenderingEnabled;
			float _FullySubmerged;
			sampler2D _UnderwaterMask;
			float CZY_ClippingThreshold;


			
			float2 voronoihash35_g78( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi35_g78( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash35_g78( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float2 voronoihash13_g78( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi13_g78( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash13_g78( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float2 voronoihash11_g78( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi11_g78( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash11_g78( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float4 SampleGradient( Gradient gradient, float time )
			{
				float3 color = gradient.colors[0].rgb;
				UNITY_UNROLL
				for (int c = 1; c < 8; c++)
				{
				float colorPos = saturate((time - gradient.colors[c-1].w) / ( 0.00001 + (gradient.colors[c].w - gradient.colors[c-1].w)) * step(c, gradient.colorsLength-1));
				color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
				}
				#ifndef UNITY_COLORSPACE_GAMMA
				color = SRGBToLinear(color);
				#endif
				float alpha = gradient.alphas[0].x;
				UNITY_UNROLL
				for (int a = 1; a < 8; a++)
				{
				float alphaPos = saturate((time - gradient.alphas[a-1].y) / ( 0.00001 + (gradient.alphas[a].y - gradient.alphas[a-1].y)) * step(a, gradient.alphasLength-1));
				alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
				}
				return float4(color, alpha);
			}
			
			float HLSL20_g79( bool enabled, bool submerged, float textureSample )
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
				o.ase_texcoord1 = screenPos;
				
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;

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
				float4 ase_texcoord : TEXCOORD0;

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
				o.ase_texcoord = v.ase_texcoord;
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
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
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

				Gradient gradient93_g73 = NewGradient( 0, 2, 2, float4( 0.06119964, 0.06119964, 0.06119964, 0.4617685 ), float4( 1, 1, 1, 0.5117723 ), 0, 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
				float2 texCoord11_g73 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 temp_output_54_0_g73 = ( texCoord11_g73 - float2( 0.5,0.5 ) );
				float dotResult23_g73 = dot( temp_output_54_0_g73 , temp_output_54_0_g73 );
				float Dot28_g73 = saturate( (0.85 + (dotResult23_g73 - 0.0) * (3.0 - 0.85) / (1.0 - 0.0)) );
				float time35_g78 = 0.0;
				float2 voronoiSmoothId35_g78 = 0;
				float2 texCoord21_g73 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 CentralUV17_g73 = ( texCoord21_g73 + float2( -0.5,-0.5 ) );
				float2 temp_output_21_0_g78 = CentralUV17_g73;
				float2 break2_g78 = abs( temp_output_21_0_g78 );
				float saferPower4_g78 = abs( break2_g78.x );
				float saferPower3_g78 = abs( break2_g78.y );
				float saferPower6_g78 = abs( ( pow( saferPower4_g78 , 2.0 ) + pow( saferPower3_g78 , 2.0 ) ) );
				float Spherize29_g73 = CZY_Spherize;
				float Flatness30_g73 = ( 20.0 * CZY_Spherize );
				float Scale24_g73 = ( CZY_MainCloudScale * 0.1 );
				float mulTime14_g73 = _TimeParameters.x * ( 0.001 * CZY_WindSpeed );
				float Time8_g73 = mulTime14_g73;
				float2 Wind51_g73 = ( Time8_g73 * float2( 0.1,0.2 ) );
				float2 temp_output_10_0_g78 = (( ( temp_output_21_0_g78 * ( pow( saferPower6_g78 , Spherize29_g73 ) * Flatness30_g73 ) ) + float2( 0.5,0.5 ) )*( 2.0 / ( Scale24_g73 * 1.5 ) ) + ( Wind51_g73 * float2( 0.5,0.5 ) ));
				float2 coords35_g78 = temp_output_10_0_g78 * 60.0;
				float2 id35_g78 = 0;
				float2 uv35_g78 = 0;
				float fade35_g78 = 0.5;
				float voroi35_g78 = 0;
				float rest35_g78 = 0;
				for( int it35_g78 = 0; it35_g78 <2; it35_g78++ ){
				voroi35_g78 += fade35_g78 * voronoi35_g78( coords35_g78, time35_g78, id35_g78, uv35_g78, 0,voronoiSmoothId35_g78 );
				rest35_g78 += fade35_g78;
				coords35_g78 *= 2;
				fade35_g78 *= 0.5;
				}//Voronoi35_g78
				voroi35_g78 /= rest35_g78;
				float time13_g78 = 0.0;
				float2 voronoiSmoothId13_g78 = 0;
				float2 coords13_g78 = temp_output_10_0_g78 * 25.0;
				float2 id13_g78 = 0;
				float2 uv13_g78 = 0;
				float fade13_g78 = 0.5;
				float voroi13_g78 = 0;
				float rest13_g78 = 0;
				for( int it13_g78 = 0; it13_g78 <2; it13_g78++ ){
				voroi13_g78 += fade13_g78 * voronoi13_g78( coords13_g78, time13_g78, id13_g78, uv13_g78, 0,voronoiSmoothId13_g78 );
				rest13_g78 += fade13_g78;
				coords13_g78 *= 2;
				fade13_g78 *= 0.5;
				}//Voronoi13_g78
				voroi13_g78 /= rest13_g78;
				float time11_g78 = 17.23;
				float2 voronoiSmoothId11_g78 = 0;
				float2 coords11_g78 = temp_output_10_0_g78 * 9.0;
				float2 id11_g78 = 0;
				float2 uv11_g78 = 0;
				float fade11_g78 = 0.5;
				float voroi11_g78 = 0;
				float rest11_g78 = 0;
				for( int it11_g78 = 0; it11_g78 <2; it11_g78++ ){
				voroi11_g78 += fade11_g78 * voronoi11_g78( coords11_g78, time11_g78, id11_g78, uv11_g78, 0,voronoiSmoothId11_g78 );
				rest11_g78 += fade11_g78;
				coords11_g78 *= 2;
				fade11_g78 *= 0.5;
				}//Voronoi11_g78
				voroi11_g78 /= rest11_g78;
				float2 texCoord12_g73 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 temp_output_15_0_g73 = ( texCoord12_g73 - float2( 0.5,0.5 ) );
				float dotResult7_g73 = dot( temp_output_15_0_g73 , temp_output_15_0_g73 );
				float ModifiedCohesion22_g73 = ( CZY_CloudCohesion * 1.0 * ( 1.0 - dotResult7_g73 ) );
				float lerpResult15_g78 = lerp( saturate( ( voroi35_g78 + voroi13_g78 ) ) , voroi11_g78 , ( ModifiedCohesion22_g73 * 1.1 ));
				float CumulusCoverage113_g73 = CZY_CumulusCoverageMultiplier;
				float lerpResult16_g78 = lerp( lerpResult15_g78 , 1.0 , ( ( 1.0 - CumulusCoverage113_g73 ) + -0.7 ));
				float temp_output_25_0_g73 = saturate( (0.0 + (( Dot28_g73 * ( 1.0 - lerpResult16_g78 ) ) - 0.6) * (1.0 - 0.0) / (1.0 - 0.6)) );
				float IT2Alpha101_g73 = SampleGradient( gradient93_g73, temp_output_25_0_g73 ).r;
				bool enabled20_g79 =(bool)_UnderwaterRenderingEnabled;
				bool submerged20_g79 =(bool)_FullySubmerged;
				float4 screenPos = IN.ase_texcoord1;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float textureSample20_g79 = tex2Dlod( _UnderwaterMask, float4( ase_screenPosNorm.xy, 0, 0.0) ).r;
				float localHLSL20_g79 = HLSL20_g79( enabled20_g79 , submerged20_g79 , textureSample20_g79 );
				

				surfaceDescription.Alpha = ( IT2Alpha101_g73 * ( 1.0 - localHLSL20_g79 ) );
				surfaceDescription.AlphaClipThreshold = CZY_ClippingThreshold;

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
        	#define _ALPHATEST_ON 1
        	#define ASE_SRP_VERSION 140010


			

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

			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
			#define ASE_NEEDS_FRAG_SCREEN_POSITION


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
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

			float CZY_Spherize;
			float CZY_MainCloudScale;
			float CZY_WindSpeed;
			float CZY_CloudCohesion;
			float CZY_CumulusCoverageMultiplier;
			float _UnderwaterRenderingEnabled;
			float _FullySubmerged;
			sampler2D _UnderwaterMask;
			float CZY_ClippingThreshold;


			
			float2 voronoihash35_g78( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi35_g78( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash35_g78( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float2 voronoihash13_g78( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi13_g78( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash13_g78( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float2 voronoihash11_g78( float2 p )
			{
				
				p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
				return frac( sin( p ) *43758.5453);
			}
			
			float voronoi11_g78( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
			{
				float2 n = floor( v );
				float2 f = frac( v );
				float F1 = 8.0;
				float F2 = 8.0; float2 mg = 0;
				for ( int j = -1; j <= 1; j++ )
				{
					for ( int i = -1; i <= 1; i++ )
				 	{
				 		float2 g = float2( i, j );
				 		float2 o = voronoihash11_g78( n + g );
						o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
						float d = 0.5 * dot( r, r );
				 		if( d<F1 ) {
				 			F2 = F1;
				 			F1 = d; mg = g; mr = r; id = o;
				 		} else if( d<F2 ) {
				 			F2 = d;
				
				 		}
				 	}
				}
				return F1;
			}
			
			float4 SampleGradient( Gradient gradient, float time )
			{
				float3 color = gradient.colors[0].rgb;
				UNITY_UNROLL
				for (int c = 1; c < 8; c++)
				{
				float colorPos = saturate((time - gradient.colors[c-1].w) / ( 0.00001 + (gradient.colors[c].w - gradient.colors[c-1].w)) * step(c, gradient.colorsLength-1));
				color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
				}
				#ifndef UNITY_COLORSPACE_GAMMA
				color = SRGBToLinear(color);
				#endif
				float alpha = gradient.alphas[0].x;
				UNITY_UNROLL
				for (int a = 1; a < 8; a++)
				{
				float alphaPos = saturate((time - gradient.alphas[a-1].y) / ( 0.00001 + (gradient.alphas[a].y - gradient.alphas[a-1].y)) * step(a, gradient.alphasLength-1));
				alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
				}
				return float4(color, alpha);
			}
			
			float HLSL20_g79( bool enabled, bool submerged, float textureSample )
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

				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;

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
				float4 ase_texcoord : TEXCOORD0;

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
				o.ase_texcoord = v.ase_texcoord;
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
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
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

				Gradient gradient93_g73 = NewGradient( 0, 2, 2, float4( 0.06119964, 0.06119964, 0.06119964, 0.4617685 ), float4( 1, 1, 1, 0.5117723 ), 0, 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
				float2 texCoord11_g73 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 temp_output_54_0_g73 = ( texCoord11_g73 - float2( 0.5,0.5 ) );
				float dotResult23_g73 = dot( temp_output_54_0_g73 , temp_output_54_0_g73 );
				float Dot28_g73 = saturate( (0.85 + (dotResult23_g73 - 0.0) * (3.0 - 0.85) / (1.0 - 0.0)) );
				float time35_g78 = 0.0;
				float2 voronoiSmoothId35_g78 = 0;
				float2 texCoord21_g73 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 CentralUV17_g73 = ( texCoord21_g73 + float2( -0.5,-0.5 ) );
				float2 temp_output_21_0_g78 = CentralUV17_g73;
				float2 break2_g78 = abs( temp_output_21_0_g78 );
				float saferPower4_g78 = abs( break2_g78.x );
				float saferPower3_g78 = abs( break2_g78.y );
				float saferPower6_g78 = abs( ( pow( saferPower4_g78 , 2.0 ) + pow( saferPower3_g78 , 2.0 ) ) );
				float Spherize29_g73 = CZY_Spherize;
				float Flatness30_g73 = ( 20.0 * CZY_Spherize );
				float Scale24_g73 = ( CZY_MainCloudScale * 0.1 );
				float mulTime14_g73 = _TimeParameters.x * ( 0.001 * CZY_WindSpeed );
				float Time8_g73 = mulTime14_g73;
				float2 Wind51_g73 = ( Time8_g73 * float2( 0.1,0.2 ) );
				float2 temp_output_10_0_g78 = (( ( temp_output_21_0_g78 * ( pow( saferPower6_g78 , Spherize29_g73 ) * Flatness30_g73 ) ) + float2( 0.5,0.5 ) )*( 2.0 / ( Scale24_g73 * 1.5 ) ) + ( Wind51_g73 * float2( 0.5,0.5 ) ));
				float2 coords35_g78 = temp_output_10_0_g78 * 60.0;
				float2 id35_g78 = 0;
				float2 uv35_g78 = 0;
				float fade35_g78 = 0.5;
				float voroi35_g78 = 0;
				float rest35_g78 = 0;
				for( int it35_g78 = 0; it35_g78 <2; it35_g78++ ){
				voroi35_g78 += fade35_g78 * voronoi35_g78( coords35_g78, time35_g78, id35_g78, uv35_g78, 0,voronoiSmoothId35_g78 );
				rest35_g78 += fade35_g78;
				coords35_g78 *= 2;
				fade35_g78 *= 0.5;
				}//Voronoi35_g78
				voroi35_g78 /= rest35_g78;
				float time13_g78 = 0.0;
				float2 voronoiSmoothId13_g78 = 0;
				float2 coords13_g78 = temp_output_10_0_g78 * 25.0;
				float2 id13_g78 = 0;
				float2 uv13_g78 = 0;
				float fade13_g78 = 0.5;
				float voroi13_g78 = 0;
				float rest13_g78 = 0;
				for( int it13_g78 = 0; it13_g78 <2; it13_g78++ ){
				voroi13_g78 += fade13_g78 * voronoi13_g78( coords13_g78, time13_g78, id13_g78, uv13_g78, 0,voronoiSmoothId13_g78 );
				rest13_g78 += fade13_g78;
				coords13_g78 *= 2;
				fade13_g78 *= 0.5;
				}//Voronoi13_g78
				voroi13_g78 /= rest13_g78;
				float time11_g78 = 17.23;
				float2 voronoiSmoothId11_g78 = 0;
				float2 coords11_g78 = temp_output_10_0_g78 * 9.0;
				float2 id11_g78 = 0;
				float2 uv11_g78 = 0;
				float fade11_g78 = 0.5;
				float voroi11_g78 = 0;
				float rest11_g78 = 0;
				for( int it11_g78 = 0; it11_g78 <2; it11_g78++ ){
				voroi11_g78 += fade11_g78 * voronoi11_g78( coords11_g78, time11_g78, id11_g78, uv11_g78, 0,voronoiSmoothId11_g78 );
				rest11_g78 += fade11_g78;
				coords11_g78 *= 2;
				fade11_g78 *= 0.5;
				}//Voronoi11_g78
				voroi11_g78 /= rest11_g78;
				float2 texCoord12_g73 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 temp_output_15_0_g73 = ( texCoord12_g73 - float2( 0.5,0.5 ) );
				float dotResult7_g73 = dot( temp_output_15_0_g73 , temp_output_15_0_g73 );
				float ModifiedCohesion22_g73 = ( CZY_CloudCohesion * 1.0 * ( 1.0 - dotResult7_g73 ) );
				float lerpResult15_g78 = lerp( saturate( ( voroi35_g78 + voroi13_g78 ) ) , voroi11_g78 , ( ModifiedCohesion22_g73 * 1.1 ));
				float CumulusCoverage113_g73 = CZY_CumulusCoverageMultiplier;
				float lerpResult16_g78 = lerp( lerpResult15_g78 , 1.0 , ( ( 1.0 - CumulusCoverage113_g73 ) + -0.7 ));
				float temp_output_25_0_g73 = saturate( (0.0 + (( Dot28_g73 * ( 1.0 - lerpResult16_g78 ) ) - 0.6) * (1.0 - 0.0) / (1.0 - 0.6)) );
				float IT2Alpha101_g73 = SampleGradient( gradient93_g73, temp_output_25_0_g73 ).r;
				bool enabled20_g79 =(bool)_UnderwaterRenderingEnabled;
				bool submerged20_g79 =(bool)_FullySubmerged;
				float4 ase_screenPosNorm = ScreenPos / ScreenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float textureSample20_g79 = tex2Dlod( _UnderwaterMask, float4( ase_screenPosNorm.xy, 0, 0.0) ).r;
				float localHLSL20_g79 = HLSL20_g79( enabled20_g79 , submerged20_g79 , textureSample20_g79 );
				

				float Alpha = ( IT2Alpha101_g73 * ( 1.0 - localHLSL20_g79 ) );
				float AlphaClipThreshold = CZY_ClippingThreshold;

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
	
	CustomEditor "DistantLands.Cozy.EditorScripts.EmptyShaderGUI"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback "Hidden/InternalErrorShader"
}
/*ASEBEGIN
Version=19501
Node;AmplifyShaderEditor.FunctionNode;1247;-528,-400;Inherit;False;Stylized Clouds (Ghibli Mobile);0;;73;0e3bfd67c40d9414689466ab81c18717;0;0;3;COLOR;0;FLOAT;126;FLOAT;127
Node;AmplifyShaderEditor.RangedFloatNode;1248;-512,-192;Inherit;False;Global;CZY_CloudsFogAmount;CZY_CloudsFogAmount;8;0;Create;True;0;0;0;False;0;False;0;0.82;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1249;-512,-272;Inherit;False;Global;CZY_CloudsFogLightAmount;CZY_CloudsFogLightAmount;7;0;Create;True;0;0;0;False;0;False;0;0.443;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;1250;-176,-400;Inherit;False;AddFogToSkyLayer;-1;;369;36a78fe96c9f6fa4dab85c7793736468;0;3;89;COLOR;0,0,0,0;False;91;FLOAT;0;False;59;FLOAT;0;False;2;COLOR;84;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1231;96,-400;Float;False;True;-1;2;DistantLands.Cozy.EditorScripts.EmptyShaderGUI;0;13;Distant Lands/Cozy/URP/Stylized Clouds (Ghibli Mobile);2992e84f91cbeb14eab234972e07ea9d;True;Forward;0;1;Forward;8;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;1;False;;False;False;False;False;False;False;False;False;True;True;True;221;False;;255;False;;255;False;;7;False;;2;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Transparent=Queue=-50;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalForwardOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;22;Surface;0;0;  Blend;0;0;Two Sided;2;637952267559032767;Forward Only;1;638409620226334998;Cast Shadows;1;0;  Use Shadow Threshold;0;0;Receive Shadows;1;0;GPU Instancing;1;0;LOD CrossFade;0;0;Built-in Fog;0;0;Meta Pass;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Vertex Position,InvertActionOnDeselection;1;0;0;10;False;True;True;True;False;False;True;True;True;False;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1233;-406.0242,-441.7039;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1230;-406.0242,-441.7039;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1232;-406.0242,-441.7039;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1234;-406.0242,-441.7039;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1235;-406.0242,-391.7039;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=Universal2D;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1236;-406.0242,-391.7039;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;SceneSelectionPass;0;6;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1237;-406.0242,-391.7039;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ScenePickingPass;0;7;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1238;-406.0242,-391.7039;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormals;0;8;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1239;-406.0242,-391.7039;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormalsOnly;0;9;DepthNormalsOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;True;9;d3d11;metal;vulkan;xboxone;xboxseries;playstation;ps4;ps5;switch;0;;0;0;Standard;0;False;0
WireConnection;1250;89;1247;0
WireConnection;1250;91;1249;0
WireConnection;1250;59;1248;0
WireConnection;1231;2;1250;84
WireConnection;1231;3;1247;126
WireConnection;1231;4;1247;127
ASEEND*/
//CHKSM=578E50B87F6AC19A371593A6E9CB15A1C29ED282