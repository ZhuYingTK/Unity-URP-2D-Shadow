Shader "Custom/2DShadow" {
	Properties {
		_MainTex ("Example Texture", 2D) = "white" {}
		_Count ("步数",int) = 60
	}
	SubShader {
		Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "DisableBatching" = "true"}
 
		HLSLINCLUDE
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 
			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			int _Count;
			int _OutCount;
			CBUFFER_END
		ENDHLSL
 
		Pass {
			Name "Example"
			Tags { "LightMode"="Universal2D"}
			Blend One One
 
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
 
			struct Attributes {
				float4 positionOS	: POSITION;
				float2 uv		: TEXCOORD0;
				float4 color		: COLOR;
			};
 
			struct Varyings {
				float4 positionCS 	: SV_POSITION;
				float2 uv		: TEXCOORD0;
				float4 color		: COLOR;
				float4 screenUV		: TEXCOORD1;
			};
 
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			TEXTURE2D(_WallRT);
			SAMPLER(sampler_WallRT);
 
			Varyings vert(Attributes IN) {
				Varyings OUT;
				OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
				float4 ObjCS = TransformObjectToHClip(float3(0,0,0));
				float4 screenPos = ComputeScreenPos(OUT.positionCS);
				OUT.screenUV.xy = screenPos.xy/screenPos.w;

				float4 ObjScreenPos = ComputeScreenPos(ObjCS);
				OUT.screenUV.zw = ObjScreenPos.xy/ObjScreenPos.w;
				
				OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
				OUT.color = IN.color;
				return OUT;
			}
 
			half4 frag(Varyings IN) : SV_Target {

				float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,IN.uv);

				float2 uvDirUnit = (IN.screenUV.zw - IN.screenUV.xy)/(float)_Count;
				//col.rgb *= SAMPLE_TEXTURE2D(_WallRT,sampler_WallRT,IN.screenUV.xy);

				for (int i = 0;i < _Count + 1;i++)
				{
					float2 uv = lerp(IN.screenUV.zw,IN.screenUV.xy,1- (float)i/(float)_Count);
					col *= SAMPLE_TEXTURE2D(_WallRT,sampler_WallRT,uv);
				}
				return col;
			}
			ENDHLSL
		}
	}
}