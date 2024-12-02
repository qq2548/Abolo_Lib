///这个shader的高光用于单独图像，与背景相加
Shader "2d/Image_Highlight_FlowFx"
{
	Properties
	{
		//main type of Properties ,annotation if no need
		_Color("myColor",Color) = (1,1,1,1)
		_MainTex("MainTex",2D) = "white"{}
		[Toggle] _UseAlpha("UseImageAlpha", Float) = 0  //翻转uv.x
		_subTex2D("subTex2D",2D) = "white"{}

		_DistanceDivision("speed" ,Range(0,10)) = 1.0

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
		Tags{"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

		Stencil			//模板缓冲，UI Image的shader如果没写Stencil，图片不会受到UI Mask的遮罩影响，在Content这类局部区域显示的UI上会有问题
		{

			Ref				[_Stencil]
			Comp			[_StencilComp]
			Pass				[_StencilOp]
			ReadMask	[_StencilReadMask]
			WriteMask	[_StencilWriteMask]
			
		}

		
		Blend SrcAlpha One

		ColorMask   [_ColorMask]					
		Cull Off
		Lighting Off
		ZWrite Off
        ZTest [unity_GUIZTestMode]


		Pass
		{
			Cull Off
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#include "UnityCG.cginc"  
			#include "Assets/Abolo_Lib/Shaders/AboloCG.cginc" 
			#include "UnityUI.cginc" 

			#pragma multi_compile __ _USEALPHA_ON

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _subTex2D;
			//float4 _Color;
			float _DistanceDivision;


			struct appdata
			{
				float4 vertex : POSITION ;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL ; 
				float4 Color:COLOR;

			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 WorldPos:TEXCOORD1;
				float3 normal : NORMAL ; 
				float4 Color:COLOR;

			};





			v2f vert(appdata i)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv = TRANSFORM_TEX(i.uv, _MainTex);
				o.WorldPos = mul(unity_ObjectToWorld, i.vertex);
				o.normal = i.normal;
				o.Color = i.Color;

				return o;
			}


			float4 frag(v2f v) : SV_Target
			{


				float TimeFac = ABL_FixTime(_Time.x);
				
				#ifdef _USEALPHA_ON
					TimeFac = (v.Color.a*1.1-0.5 )/_DistanceDivision;
				#endif


				float2 center = v.uv*2.0 -1.0;
				float4 subColor = tex2D(_subTex2D,v.uv);

				float2 fireuv = float2( subColor.g - TimeFac *_DistanceDivision,subColor.b- TimeFac *_DistanceDivision);
				fixed flowColor = tex2D(_subTex2D,fireuv).r;
				float flowColorShadow = tex2D(_subTex2D,fireuv*float2(1.0 , 3.0)+float2(0.1 , 0.5)).r;

				fixed4 color = tex2D(_MainTex,v.uv) ;

				
				color.rgb = v.Color.rgb*flowColor *v.Color.a;
				color.a *= subColor.a;

				return color;
			}

			ENDCG
		}


		
	}

}