Shader "2d/Sprite_ColorBlendNormal"

{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

		 _FlashFactor("FlashFactor" , Float) = 0.0
         _FlashColor("FlashColor" , Color) = (1.0,1.0,1.0,1.0)
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
			Name "Sprite_ColorBlendNormal"
			CGPROGRAM

			#pragma vertex vert 
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc" 
			#include "Assets/Abolo_Lib/Shaders/AboloCG.cginc"  
			#pragma multi_compile_instancing


			 UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here

                UNITY_DEFINE_INSTANCED_PROP(fixed4, _FlashColor)
				UNITY_DEFINE_INSTANCED_PROP(float, _FlashFactor)

		    UNITY_INSTANCING_BUFFER_END(Props)

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
				fixed4 flashColor = UNITY_ACCESS_INSTANCED_PROP(Props , _FlashColor);
				float flashFactor = UNITY_ACCESS_INSTANCED_PROP(Props , _FlashFactor);
				fixed4 color = tex2D(_MainTex , IN.texcoord);
				color.rgb = mix3(color.rgb , flashColor.rgb , flashFactor);
				color.rgb *= color.a;
				return color;
			}

			ENDCG
		}

	}
	FallBack "Sprite/Default"

}