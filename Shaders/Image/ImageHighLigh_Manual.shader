Shader "2d/ImageHighLight_Additive"

{
	Properties
	{

		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        [Space(20)] //与上一行的间距
		
		_SubTex("SubTexture",2D) = "white"{} //副贴图，用于特效动画的之类的
		_Color ("TintColor", Color) = (1.0,1.0,1.0,1.0)


	    _Speed("Speed" , Range(0.0 , 2.0)) = 0.5
	    _Duration("Duration" , Range(1.0 , 5.0)) = 1.0
	    _LightScalor("LightScalor" , Range(1.0 , 2.0)) = 1.0

		[Header(StencilBuffer)]//模板缓冲
		_StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15


		[HideInInspector]
        _UVRect("UVRect" , Vector) = (0,0,1,1)
        [HideInInspector]
        _UVScale("UVScale" , Vector) = (1,1,0,0)
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



		Blend SrcAlpha One



		Pass 
		{
			
			CGPROGRAM

			

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc" 
			#include "UnityUI.cginc" 

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
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float4 worldPosition : TEXCOORD2;
				fixed4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				
			};

			sampler2D _MainTex;
			sampler2D _SubTex;//用作特效的副贴图
			float4 _SubTex_ST;
			float4 _MainTex_ST;
			sampler2D _Heightmap;
			float4 _ClipRect;

			v2f vert(appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				o.worldPosition = i.vertex;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv0 = TRANSFORM_TEX(i.uv, _MainTex);
				o.uv1 = TRANSFORM_TEX(i.uv, _SubTex);
				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);

				return o;
			}

			
		  float _Speed;
		  float _Duration;
		  half _LightScalor;


			UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
			UNITY_DEFINE_INSTANCED_PROP(float4, _UVRect)
            UNITY_DEFINE_INSTANCED_PROP(float4, _UVScale)
		    UNITY_INSTANCING_BUFFER_END(Props)


			fixed4 frag(v2f IN) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);

				//atlas 图片uv归一化
                float4 UVRect = UNITY_ACCESS_INSTANCED_PROP(Props , _UVRect);
                float4 UVScale = UNITY_ACCESS_INSTANCED_PROP(Props , _UVScale);

                float2 sprcenter = (UVRect.zw - UVRect.xy) * 0.5;
                IN.uv1 = IN.uv1 - UVRect.xy - sprcenter;
                IN.uv1 *= UVScale;
                IN.uv1 += sprcenter;
                IN.uv1 = IN.uv1 / (UVRect.zw - UVRect.xy);
                //----------------------------------------

				fixed4 color = tex2D(_MainTex, IN.uv0) * IN.Color;
				
				float t  = min(fmod((frac(_Time.y*0.001) * 1000.0)* _Speed , _Duration) , 0.75);
				float2 subuv =( IN.uv1 - 0.5) *0.6 + 0.5;

				//旋转矩阵
                const float2x2 rot = half2x2(0.666 , -0.27, 
                                                              0.27, 0.666);  

				float2 uv = IN.uv1;
                float2 center = uv - 0.5;

                //矩阵相乘旋转
                subuv = mul(rot,center) * (2.0 - _LightScalor) + 0.5;
				//开启流光特效
				//#ifdef _FLOWLIGHT_ON
                float2 flowUV = float2(subuv.x - t , subuv.y -  t ) ;
			    fixed subColor = tex2D(_SubTex, flowUV).r;
				//g通道作为mask
				fixed maskColor = tex2D(_SubTex , uv).g;
 
				color.rgb = 0.0;
                color.rgb += saturate(UNITY_ACCESS_INSTANCED_PROP(Props, _Color).rgb * subColor  * maskColor);
                //#endif
				
				//color *= UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

				return color;
			}

			ENDCG
		}

	
		
	}

}