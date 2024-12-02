///这个shader的高光用于单独图像，与背景相加
Shader "2d/Sprite_Highlight_FlowFx"
{
	Properties
	{
		//main type of Properties ,annotation if no need
		_Color("myColor",Color) = (1,1,1,1)
		_MainTex("MainTex",2D) = "white"{}
		[Toggle] _FlipX("Flip uv.x", Float) = 0  //翻转uv.x
		[Toggle] _FlipY("Flip uv.y", Float) = 0  //翻转uv.y
		_SubTex("subTex2D",2D) = "white"{}

		_flowSpeedX("flowSpeedX" ,Range(0,10)) = 1.0
		_flowSpeedY("flowSpeedY" ,Range(0,10)) = 1.0
		_reverseInt("reverseFac" , int) = 1

	}
	SubShader 
	{
		Tags{"Queue" = "Transparent +1" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

		ZWrite Off
		ZTest LEqual
		Blend One One




		Pass
		{
			Cull Off
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"  
			#include "Assets/Abolo_Lib/Shaders/AboloCG.cginc" 
			#pragma multi_compile_instancing
			#pragma multi_compile __ _FLIPX_ON _FLIPY_ON

			#define TWO_PI 6.28318530718

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _SubTex;
			float4 _SubTex_ST;
			//float4 _Color;
			float _flowSpeedX;
			float _flowSpeedY;
			int _reverseInt;

			struct appdata
			{
				float4 vertex : POSITION ;
				float2 uv : TEXCOORD0;

				float4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID //gpu instancing 默认的shader带了两个函数接口
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv1:TEXCOORD1;

				float4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};





			//下列被宏包起来的属性才能在不破坏GPU合批的前提下被动态修改
            UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(float4, _Color)

		    UNITY_INSTANCING_BUFFER_END(Props)

			v2f vert(appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv =TRANSFORM_TEX(i.uv, _MainTex);
				o.uv1 = TRANSFORM_TEX(i.uv, _SubTex);

				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);
				return o;
			}

			float4 frag(v2f v) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(v);

				#ifdef _FLIPX_ON
					v.uv.x = 1 - v.uv.x;
				#endif

				#ifdef _FLIPY_ON
					v.uv.y = 1 - v.uv.y;
				#endif
				float TimeFac = ABL_FixTime(_Time.x);
				float2 center = v.uv*2.0 -1.0;
				float4 subColor = tex2D(_SubTex,v.uv);
				fixed maskColor =  tex2D(_SubTex,v.uv1).b;

				float2 fireuv = float2( subColor.g - TimeFac *_flowSpeedX,TimeFac *_flowSpeedY);
				float flowColor = tex2D(_SubTex,fireuv).r;

				float4 color = tex2D(_MainTex,v.uv) ;

				float2 toCenter = float2(0.5 , 0.5)-v.uv;
                float angle = atan2(-toCenter.y , -toCenter.x);
                float radius = length(toCenter)*2.0;
				float3 col = hsb2rgb(float3((angle/TWO_PI)+0.5,radius,1.0));

				color.a *=maskColor;
				color.rgb = v.Color.rgb*flowColor *v.Color.a;
				color.rgb *= color.a;
				
				color.rgb *= col;

				return color;
			}

			ENDCG
		}


		
	}

}