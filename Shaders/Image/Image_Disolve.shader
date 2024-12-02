Shader "2d/ImageDisolve"
{
	Properties
	{

		[PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
		_SubTex ("_SubTex", 2D) = "white" {}


		[KeywordEnum(Default , XDir , YDir)]_Pattern("Effect_Pattern" , Float) = 0.0

		//_DisolveThreshold("DisolveThreshold" , Range(0.0 , 1.0)) = 0.0

		_SmoothRange("SmoothRange" , Range(0.0 , 0.5)) = 0.1

		_StartPercentage("StartDisolvePercentage" , Range(0.0 , 1.0)) = 0.5

		_TileScalor_X ("TileScalor_X", Float) =1.0
		_TileScalor_Y ("TileScalor_Y", Float) =1.0
		_Mirror ("Mirror", Float) =1.0
		
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
			#pragma target 2.0

			#include "UnityCG.cginc" 
			 
			#include "UnityUI.cginc" 
			#pragma multi_compile __ _PATTERN_DEFAULT _PATTERN_XDIR _PATTERN_YDIR


			struct appdata
			{
				float4 vertex : POSITION ;
				float2 uv : TEXCOORD0;
				fixed4 Color:COLOR;

			};

			struct v2f
			{
				
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;

				fixed4 Color:COLOR;

			};

			sampler2D _MainTex;
			sampler2D _SubTex;
			float4 _MainTex_ST;
			float4 _SubTex_ST;

			v2f vert(appdata i)
			{
				v2f o;

				o.worldPosition = i.vertex;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv.xy = TRANSFORM_TEX(i.uv, _MainTex);

				o.Color = i.Color;

				return o;
			}



			fixed _StartPercentage;
			fixed _SmoothRange;


			//float _DisolveThreshold;

			float _TileScalor_X;
			float _TileScalor_Y;
			float _Mirror;

			fixed4 frag(v2f IN) : SV_Target
			{
				float2 oriUV = IN.uv;


				//atlas 图片uv归一化
                //float4 UVRect = UNITY_ACCESS_INSTANCED_PROP(Props , _UVRect);
                //float4 UVScale = UNITY_ACCESS_INSTANCED_PROP(Props , _UVScale);

                //float2 sprcenter = (UVRect.zw - UVRect.xy) * 0.5;
                //IN.uv = IN.uv - UVRect.xy - sprcenter;
                //IN.uv *= UVScale;
                //IN.uv += sprcenter;
                //IN.uv = IN.uv / (UVRect.zw - UVRect.xy);





				//float t = frac(_Time.y * 0.001) * 1000.0;

				
				
				//float2 uv = (frac(IN.uv *_SubTex_ST.xy) - 0.5) * (sin(t*6.0)*0.05 + 0.995);
				
				//float mask2 = saturate( length(max(abs(uv*1.5) , 0.0)));

				//float2 coord =IN.uv - 0.5 ;



				
				fixed4 color = tex2D(_MainTex , oriUV);
				float2 fixedUV = IN.uv * float2(_TileScalor_X , _TileScalor_Y);
				color.rgb *= IN.Color.rgb;
				fixed fac1 = 0;
				//fixed fac2 = 1.0 - subColor.g;
				#ifdef _PATTERN_DEFAULT
					float4 subColor = tex2D(_SubTex , fixedUV);
					fac1 = 1.0 - subColor.r;
					color.a *= smoothstep(saturate(fac1 - _SmoothRange*.5) , 
														  saturate(fac1 + _SmoothRange*.5), 
														   (saturate(IN.Color.a  - _StartPercentage)) / (1.0 - _StartPercentage));
					//color.a *= step(fac2 ,  v.Color.a) ; 
				#endif

				#ifdef _PATTERN_XDIR
					float a =  fixedUV.x;
					float b =  1.0 - fixedUV.x;
					fac1 = a * step(0, _Mirror) + b * step(0 , -_Mirror);
					color *= smoothstep(saturate(fac1 - _SmoothRange*.5) , 
													   saturate(fac1 + _SmoothRange*.5), 
													   (saturate(IN.Color.a  - _StartPercentage)) / (1.0 - _StartPercentage));
					//color *= smoothstep(saturate(fac2 - 0.2) , fac2 , 1.0 - (saturate(v.uv.z - _StartPercentage)) / (1.0 - _StartPercentage)) * v.Color.a;
				#endif
				#ifdef _PATTERN_YDIR
					float a =  fixedUV.y;
					float b =  1.0 - fixedUV.y;
					fac1 = a * step(0, _Mirror) + b * step(0 , -_Mirror);
					color *= smoothstep(saturate(fac1 - _SmoothRange*.5) , 
													   saturate(fac1 + _SmoothRange*.5), 
													   (saturate(IN.Color.a  - _StartPercentage)) / (1.0 - _StartPercentage));
					//color *= smoothstep(saturate(fac2 - 0.2) , fac2 , 1.0 - (saturate(v.uv.z - _StartPercentage)) / (1.0 - _StartPercentage)) * v.Color.a;
				#endif

				return   color;
			}

			ENDCG
		}

	
		
	}

}
