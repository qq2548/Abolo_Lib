Shader "2d/Sprite_BSC"

{
	Properties
	{

		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        [Space(20)] //与上一行的间距
		

		_Color ("TintColor", Color) = (1.0,1.0,1.0,1.0)

		_Brightness("Brightness", Float) = 1	//调整亮度
		_Saturation("Saturation", Float) = 1	//调整饱和度
		_Contrast("Contrast", Float) = 1		//调整对比度

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
			Name "SpriteBSC"
			CGPROGRAM

			

			#pragma vertex vert
			#pragma fragment frag 
			#pragma target 3.0

			#include "UnityCG.cginc" 


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
			
		    float _Brightness;
            float _Saturation;
            float _Contrast;

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
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)

		    UNITY_INSTANCING_BUFFER_END(Props)

			fixed4 frag(v2f IN) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				fixed4 color = tex2D(_MainTex , IN.texcoord)* IN.Color;

				
				//brigtness亮度直接乘以一个系数，也就是RGB整体缩放，调整亮度
				color.rgb =saturate(_Brightness * color.rgb);
				//saturation饱和度：首先根据公式计算同等亮度情况下饱和度最低的值：
				fixed gray = 0.29 * color.r + 0.59 * color.g + 0.12 * color.b;
				fixed3 grayColor = fixed3(gray, gray, gray);
				//根据Saturation在饱和度最低的图像和原图之间差值
				color.rgb = lerp(grayColor, color.rgb, _Saturation*IN.Color.r);
				//contrast对比度：首先计算对比度最低的值
				fixed3 avgColor = fixed3(0.5, 0.5, 0.5);
				//根据Contrast在对比度最低的图像和原图之间差值
				color.rgb = lerp(avgColor, color.rgb, _Contrast);
				color *= UNITY_ACCESS_INSTANCED_PROP(Props, _Color) ;
				color.rgb *= color.a;
				return color;
			}

			ENDCG
		}

		
	}
	FallBack "Sprite/Default"

}