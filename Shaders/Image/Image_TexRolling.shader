Shader "2d/Image_TexRolling"

{
	Properties
	{

		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        [Space(20)] //与上一行的间距

		_Color ("TintColor", Color) = (1.0,1.0,1.0,1.0)

		_X_Threshold("X_Threshold" , Range(0.0,1.0)) = 0.0
		_Y_Threshold("Y_Threshold" , Range(0.0,1.0)) = 0.0

		_X_Slice("X_Grid" , float) = 1.0
		_Y_Slice("Y_Grid" , float) = 1.0

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



		Blend SrcAlpha OneMinusSrcAlpha



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
				o.uv0 = TRANSFORM_TEX(i.uv, _MainTex);
				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);

				return o;
			}

			
		  float _X_Threshold;
		  float _Y_Threshold;
		  float _X_Slice;
		  float _Y_Slice;


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
                IN.uv0 = IN.uv0 - UVRect.xy - sprcenter;
                IN.uv0 *= UVScale;
                IN.uv0 += sprcenter;
                IN.uv0 = IN.uv0 / (UVRect.zw - UVRect.xy);
                //----------------------------------------

				fixed4 color = tex2D(_MainTex, IN.uv0/float2(_X_Slice,_Y_Slice) + fixed2(_X_Threshold,_Y_Threshold)) * IN.Color;

				return color;
			}

			ENDCG
		}

	
		
	}

}