///这个shader高光效果作用于sprite上，叠加sprite上，不分开渲染
Shader "2d/ImageFlowEffect"
{
	Properties
	{
		//main type of Properties ,annotation if no need
		_Color("myColor",Color) = (1,1,1,1)
		_MainTex("MainTex",2D) = "white"{}
		[Toggle] _FlipX("Flip uv.x", Float) = 0  //翻转uv.x
		[Toggle] _FlipY("Flip uv.y", Float) = 0  //翻转uv.y
		_subTex2D("subTex2D",2D) = "white"{}


		_distortionFactor("distortionFactor" ,Range(1,100)) = 1.0
		_undistortionFactor("undistortionFactor" ,Range(1,100)) = 1.0
		_flowSpeedX("flowSpeedX" ,Range(0,10)) = 1.0
		_flowSpeedY("flowSpeedY" ,Range(0,10)) = 1.0
		_reverseInt("reverseFac" , int) = 1

		[Header(StencilBuffer)]//模板缓冲
		_StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

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
		stencil			//模板缓冲，UI Image的shader如果没写Stencil，图片不会受到UI Mask的遮罩影响，在Content这类局部区域显示的UI上会有问题
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
			CGPROGRAM

			

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc" 
			#include "Assets/Abolo_Lib/Shaders/AboloCG.cginc" 
			#include "UnityUI.cginc" 



			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _subTex2D;
			//float4 _Color;
			float _flowSpeedX;
			float _flowSpeedY;
			int _reverseInt;

			struct appdata
			{
				float4 vertex : POSITION ;
				float2 uv : TEXCOORD0;
				float4 Color:COLOR;

			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 Color:COLOR;
			};

			float _distortionFactor;
			float _undistortionFactor;

			fixed4 _Color;

			v2f vert(appdata i)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv = TRANSFORM_TEX(i.uv, _MainTex);
				o.Color = i.Color;
				return o;
			}

			float4 frag(v2f v) : SV_Target
			{
				#ifdef _FLIPX_ON
					v.uv.x = 1 - v.uv.x;
				#endif

				#ifdef _FLIPY_ON
					v.uv.y = 1 - v.uv.y;
				#endif
				float TimeFac = ABL_FixTime(_Time.x);  
				float2 center = v.uv*2.0 -1.0;
				float4 subColor = tex2D(_subTex2D,v.uv);

				float2 fireuv = float2( subColor.g - TimeFac *_flowSpeedX,subColor.b- TimeFac *_flowSpeedY);
				float flowColor = tex2D(_subTex2D,fireuv).r ;
				float flowColorShadow = tex2D(_subTex2D,fireuv*float2(1.0 , 3.0)+float2(0.1 , 0.5)).r;

				float4 color = tex2D(_MainTex,v.uv) * v.Color;
				flowColor *= subColor.g;
				
				color.rgb += v.Color.rgb*flowColor *v.Color.a;
				color.rgb = saturate(color.rgb);

				return color;
			}

			ENDCG
		}


		
	}

}