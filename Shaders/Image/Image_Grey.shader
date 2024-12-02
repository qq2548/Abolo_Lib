Shader "2d/Image_Grey"

{
	Properties
	{

		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        [Space(20)] //与上一行的间距
		

		_Color ("TintColor", Color) = (1.0,1.0,1.0,1.0)

		_Brightness("Brightness", Float) = 1	//调整亮度
		_Saturation("Saturation", Float) = 1	//调整饱和度
		_Contrast("Contrast", Float) = 1		//调整对比度

		[Header(StencilBuffer)]//模板缓冲
		_StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

		[Toggle]_TextMode("TextMode" ,Float) = 0.0

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

		Stencil			//模板缓冲，UI Image的shader如果没写Stencil，图片不会受到UI Mask的遮罩影响，在Content这类局部区域显示的UI上会有问题
		{

			Ref				[_Stencil]
			Comp			[_StencilComp]
			Pass				[_StencilOp]
			ReadMask	[_StencilReadMask]
			WriteMask	[_StencilWriteMask]
			
		}

		

		ColorMask   [_ColorMask]					
		Cull Off
		Lighting Off
		ZWrite Off
        ZTest [unity_GUIZTestMode]



		Blend SrcAlpha OneMinusSrcAlpha

		

		Pass 
		{
			Name "ImageGrey"
			CGPROGRAM

			

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc" 
			#include "UnityUI.cginc"  


			#pragma multi_compile __ _TEXTMODE_ON


			struct appdata
			{
				float4 vertex : POSITION ;
				float2 uv : TEXCOORD0;
				fixed4 Color:COLOR;

			};

			struct v2f
			{
				
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				fixed4 Color:COLOR;
				
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
		    float _Brightness;
            float _Saturation;
            float _Contrast;

			v2f vert(appdata i)
			{
				v2f o;
				o.worldPosition = i.vertex;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.texcoord = TRANSFORM_TEX(i.uv, _MainTex);
				o.Color = i.Color;

				return o;
			}



			fixed4 _Color;

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 color = tex2D(_MainTex , IN.texcoord)* IN.Color;
				#ifdef _TEXTMODE_ON
					color.rgb =  IN.Color.rgb;
				#endif
				    //brigtness亮度直接乘以一个系数，也就是RGB整体缩放，调整亮度
					color.rgb =saturate(_Brightness * color.rgb);
					//saturation饱和度：首先根据公式计算同等亮度情况下饱和度最低的值：
					fixed gray = 0.29 * color.r + 0.59 * color.g + 0.12 * color.b;
					fixed3 grayColor = fixed3(gray, gray, gray);
					//根据Saturation在饱和度最低的图像和原图之间差值
					color.rgb = lerp(grayColor, color.rgb, _Saturation);
					//contrast对比度：首先计算对比度最低的值
					fixed3 avgColor = fixed3(0.5, 0.5, 0.5);
					//根据Contrast在对比度最低的图像和原图之间差值
					color.rgb = lerp(avgColor, color.rgb, _Contrast);
					color.rg = saturate(color.rg + fixed2(0.05,0.02));
				color *= _Color ;
				//color = tex2D(_MainTex , IN.texcoord)* IN.Color;


				/////强光混合模式算法
				//fixed4 ccc = tex2D(_MainTex , IN.texcoord);
				//fixed gray1 = 0.29 * ccc.r + 0.59 * ccc.g + 0.12 * ccc.b;
				//float3 grayColor1 = float3(gray1,gray1,gray1);
				//float3 goldColor = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
				//fixed targetlumin = 0.29 * goldColor.r + 0.59 * goldColor.g + 0.12 * goldColor.b;
				//float3 resultColor1 = grayColor1*goldColor + goldColor * 2.0 - 1.0;//grayColor1 + goldColor-(grayColor1*goldColor) ;//
				//float3 resultColor3 = grayColor1 + goldColor * (1.0 - grayColor1) ;

				//float3 resultColor2 = grayColor1* goldColor * 2.0;
				//float3 result = step(targetlumin , 0.5)*resultColor2 + step(0.5 , targetlumin)*resultColor1;
				//float threshold = 0.5;
				//color = saturate(float4(result.r,result.g,result.b,1.0));
				/////


				return color;
			}

			ENDCG
		}

		
	}


}