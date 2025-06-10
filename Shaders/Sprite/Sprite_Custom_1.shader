Shader "2d/Sprite_Custom_1"

{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
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
															
		LOD 100		

	
		Cull Off
		Lighting Off
		ZWrite Off

		Blend One OneMinusSrcAlpha

		Pass 
		{
			Name "Sprite_Custom_1"
			CGPROGRAM

			#pragma vertex vert 
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc" 

			#pragma multi_compile_instancing


			fixed3 ABL_NormalBlendRGB(fixed3 ScrColor , float3 DstColor , float Threshold)
			{
				//float4 BlendColor = DstColor;
				float   blendfac = 1.0 - Threshold ;
				fixed3 c1 = ScrColor;
							c1 *= blendfac;
				fixed3 c2 = DstColor;
							c2 *= 1 - blendfac;
				fixed3 cc = saturate(c1 + c2);
				return cc;
			}

			struct appdata
			{
				float4 vertex : POSITION ;
				float2 uv : TEXCOORD0;
				fixed4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID //gpu instancing 默认的shader带了两个函数接口
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				fixed4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				o.worldPosition = i.vertex;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.texcoord = TRANSFORM_TEX(i.uv, _MainTex);
				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);

				return o;
			}



			fixed4 frag(v2f IN) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				fixed4 color = tex2D(_MainTex , IN.texcoord);
				color.rgb = ABL_NormalBlendRGB(color.rgb , IN.Color.rgb , 1.0 - IN.Color.a);
				color.rgb *= color.a;
				return color;
			}

			ENDCG
		}

	}
	FallBack "Sprite/Default"

}