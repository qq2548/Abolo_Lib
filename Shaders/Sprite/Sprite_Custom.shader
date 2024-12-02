Shader "2d/Sprite_Custom"

{
	Properties
	{

		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        [Space(20)] //与上一行的间距
		
		_Exposure ("Exposure", Range(1.0,5.0)) = 1.0


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
			Name "Sprite_Custom"
			CGPROGRAM

			

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc" 
			#include "Assets/Abolo_Lib/Shaders/AboloCG.cginc" 

			#pragma multi_compile_instancing




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
			
		    float _SnowIntens;
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


			UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(float, _Exposure)

		    UNITY_INSTANCING_BUFFER_END(Props)

			fixed4 frag(v2f IN) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				fixed4 color = tex2D(_MainTex , IN.texcoord)* IN.Color;
				//曝光计算
               color.rgb += (UNITY_ACCESS_INSTANCED_PROP(Props, _Exposure) - 1.0)
                                        *(1.0 - ( color.r*0.29 + color.b*0.59 + color.b*0.12));

			   //积雪效果试做
               float Noise = noise( (IN.texcoord  + (IN.worldPosition.xy))*3);
               Noise =saturate(Noise* Noise * 0.6);
               fixed4 snowColor = fixed4(1,1,1,1);
                //fixed snowFac = dot(IN.vertex , fixed3(0.0 , 1.0 , 0.0));
                color.rgb = lerp(color.rgb , snowColor.rgb , Noise * _SnowIntens);
                //积雪效果试做


				color.rgb *= color.a;
				return color;
			}

			ENDCG
		}

		
	}
	FallBack "Sprite/Default"

}