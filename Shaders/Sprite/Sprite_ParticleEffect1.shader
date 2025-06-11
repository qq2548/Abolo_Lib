Shader "2d/Sprite_ParticleEffect1"
{
	Properties
	{

		_MainTex ("Sprite Texture", 2D) = "white" {}
		_SubTex ("_SubTex", 2D) = "white" {}

		_Color ("TintColor", Color) = (1.0,1.0,1.0,1.0)


		_OutlineColor ("OutlineColor", Color) = (1.0,1.0,1.0,1.0)

		_OutlineThreshold("OutlineThreshold", Range(0.0,1.0)) = 0.0
		_ShadowOffsetX("ShadowOffsetX", Range(-1,1.0)) = 0.0
		_ShadowOffsetY("ShadowOffsetY", Range(0.0,1.0)) = 0.0
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
				float4 uv : TEXCOORD0;
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
			float4 _MainTex_TexelSize;
			float4 _SubTex_ST;
			v2f vert(appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				o.worldPosition = i.vertex;
				float2 Poffset = i.uv.zw - 0.5;
				o.vertex = UnityObjectToClipPos(i.vertex );

				o.uv.xy = TRANSFORM_TEX(i.uv, _MainTex);
				o.subuv = TRANSFORM_TEX(i.uv, _SubTex);
				o.Color = i.Color;
				o.uv.z = i.uv.z;
				UNITY_TRANSFER_INSTANCE_ID(i,o);

				return o;
			}

			



			UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
		    UNITY_INSTANCING_BUFFER_END(Props)

			float _OutlineThreshold;
			float _ShadowOffsetX;
			float _ShadowOffsetY;

			fixed4 _OutlineColor;

			fixed4 frag(v2f Input) : SV_Target
			{
				
				UNITY_SETUP_INSTANCE_ID(v);

				//方便偏移计算
                const float2 Box9[9] = 
                {
                    float2(-1 ,-1),float2(0 ,-1),float2(1 ,-1),
                    float2(-1 ,0),float2(0 ,0),float2(1 ,0),
                    float2(-1 ,1),float2(0 ,1),float2(1 ,1)
                };

				float2 OriUV = Input.uv.xy;

				fixed4 color = tex2D(_MainTex , Input.uv.xy);

				float channel = 0.0;
                    
				float shadow = 0.0;
				float outline = 0.0;
				for(int num = 0 ; num<9 ; num++)
				{
				channel = tex2D(_MainTex , OriUV + Box9[num] * _OutlineThreshold * _MainTex_TexelSize.xy).a;
				shadow = tex2D(_MainTex , OriUV+float2(_ShadowOffsetX , _ShadowOffsetY) + Box9[num] * _OutlineThreshold * _MainTex_TexelSize.xy).a;
				outline += channel ;
				outline += shadow ;
				}
				float fac = saturate(outline) - color.a;
				color.rgb = lerp(color.rgb , _OutlineColor.rgb , fac);
				color.a = lerp(color.a , saturate(outline)*_OutlineColor.a , fac);


				color *= UNITY_ACCESS_INSTANCED_PROP(Props, _Color) * Input.Color;
				color.rgb *= color.a;
				return   color;
			}

			ENDCG
		}

	
		
	}

}
