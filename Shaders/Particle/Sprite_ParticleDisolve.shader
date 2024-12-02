Shader "2d/Sprite_ParticleDisolve"
{
	Properties
	{

		_MainTex ("Sprite Texture", 2D) = "white" {}
		_SubTex ("_SubTex", 2D) = "white" {}

		_Color ("TintColor", Color) = (1.0,1.0,1.0,1.0)

		[KeywordEnum(Default , RemainEdge)]_Pattern("Effect_Pattern" , Float) = 0.0

		_YFlowSpeed("YFlowSpeed" , Range(-5.0 , 5.0)) = 1.0

		_SmoothRange("SmoothRange" , Range(0.0 , 0.5)) = 0.1

		_StartPercentage("StartDisolvePercentage" , Range(0.0 , 1.0)) = 0.5



	}
	SubShader 
	{
		Tags
		{
				"RenderType" = "Transparent"
				 "Queue" = "Transparent"
				 "IgnoreProjector"="True"
				 "PreviewType"="Plane"
				 "CanUseSpriteAtlas"="True"
		}
															

					
		Cull Off
		Lighting Off
		ZWrite Off



		Blend one OneMinusSrcAlpha



		Pass 
		{
			
			CGPROGRAM

			

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc" 
			#pragma multi_compile_particles 
			#pragma multi_compile_instancing
			#pragma multi_compile __ _PATTERN_DEFAULT _PATTERN_REMAINEDGE


			struct appdata
			{
				float4 vertex : POSITION ;
				float3 uv : TEXCOORD0;
				fixed4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID //gpu instancing 默认的shader带了两个函数接口
				
				
			};

			struct v2f
			{
				
				float4 vertex : SV_POSITION;
				float3 uv : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				float2 subuv : TEXCOORD2;
				fixed4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				
			};

			sampler2D _MainTex;
			sampler2D _SubTex;
			float4 _MainTex_ST;
			float4 _SubTex_ST;

			v2f vert(appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				o.worldPosition = i.vertex;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv.xy = TRANSFORM_TEX(i.uv, _MainTex);
				o.subuv = TRANSFORM_TEX(i.uv, _SubTex);
				o.Color = i.Color;
				o.uv.z = i.uv.z;
				UNITY_TRANSFER_INSTANCE_ID(i,o);

				return o;
			}

			



			float _YFlowSpeed;
			fixed _StartPercentage;
			fixed _SmoothRange;
			UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
		    UNITY_INSTANCING_BUFFER_END(Props)

			
			fixed4 frag(v2f v) : SV_Target
			{
				
				UNITY_SETUP_INSTANCE_ID(v);
				float t = frac(_Time.y * 0.001) * 1000.0;

				half2x2 rot = {
											cos(t * _YFlowSpeed),sin(t * _YFlowSpeed),
											-sin(t * _YFlowSpeed),cos(t * _YFlowSpeed)
										};
				
				float2 oriUV = v.subuv + float2(0.0 , t * 0.1 * _YFlowSpeed);
				fixed4 subColor = tex2D(_SubTex , oriUV);
				float2 uv = (frac(v.uv *_SubTex_ST.xy) - 0.5) * (sin(t*6.0)*0.05 + 0.995);
				
				float mask2 = saturate( length(max(abs(uv*1.5) , 0.0)));

				float2 coord =v.uv - 0.5 ;
				float2 Rcoord = mul(frac(v.uv*_SubTex_ST.xy ) -0.5,rot);
				float2 grid = floor(v.uv * 2.0);


				float facX = saturate(sin(Rcoord.x * 30.1415 + 123)*0.005 + sin(Rcoord.x * 2.5 + 66)*0.001 +  cos(Rcoord.x * 23.5)*0.007);
				float facY = saturate(sin(Rcoord.y * 30.1415 + 123)*0.005 + sin(Rcoord.y * 2.5 + 66)*0.01 +  cos(Rcoord.y * 23.5)*0.007);
				
				fixed4 color = tex2D(_MainTex , (coord +float2(facX , facY)+ 0.5));

				color *= UNITY_ACCESS_INSTANCED_PROP(Props, _Color) * v.Color;

				fixed fac1 = 1.0 - subColor.r;
				fixed fac2 = 1.0 - subColor.g;
				#ifdef _PATTERN_DEFAULT
					color.a *= smoothstep(saturate(fac1 - _SmoothRange*.5) , 
														  saturate(fac1 + _SmoothRange*.5), 
														  1.0 - (saturate(v.uv.z  - _StartPercentage)) / (1.0 - _StartPercentage));
					//color.a *= step(fac2 ,  v.Color.a) ;
				#endif

				#ifdef _PATTERN_REMAINEDGE
					color *= smoothstep(saturate(fac1 - _SmoothRange*.5) , 
													   saturate(fac1 + _SmoothRange*.5), 
													   1.0 - (saturate(v.uv.z - _StartPercentage)) / (1.0 - _StartPercentage));
					//color *= smoothstep(saturate(fac2 - 0.2) , fac2 , 1.0 - (saturate(v.uv.z - _StartPercentage)) / (1.0 - _StartPercentage)) * v.Color.a;
				#endif
				color.rgb *= color.a;
				return   color;
			}

			ENDCG
		}

	
		
	}

}
