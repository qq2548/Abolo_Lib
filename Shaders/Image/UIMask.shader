Shader "2d/UI_Mask"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_subTex("Subtexture" , 2D) = "white"{}
		_Color("Tint", Color) = (1,1,1,1)


		[Header(StencilBuffer)]//模板缓冲
		_StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15


		_Count("Count" , int) = 1
		//_SliderFactor("SilderForClockMask" , Range(0, 1)) = 0
		_smoothRange("SmoothRange" , Range(0.001, 0.1)) = 0

		[HideInInspector]_rectWidth("RectWidth" ,Float) = 0
		[HideInInspector]_rectHeight("RectHeight", Float) = 0

		//[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
		[HideInInspector]_screenWidth("ScreenWidth" , Float) = 0
		[HideInInspector]_screenHeight("ScreenHight" , Float) = 0

		[HideInInspector]_centerPosX("CenterPosX", Float) = 0
		[HideInInspector]_centerPosY("CenterPosY", Float) = 0

		[HideInInspector]_centerPosX2("CenterPosX2", Float) = 0.5
		[HideInInspector]_centerPosY2("CenterPosY2", Float) = 0.5

		[HideInInspector]_centerPosX3("CenterPosX3", Float) = 0.2
		[HideInInspector]_centerPosY3("CenterPosY3", Float) = 0.1

		[KeywordEnum( CircleMask , SpriteMask , RectMask , BetterRectMask)]_Style("MaskStyle" , Float) = 0.0

	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Stencil
			{
				Ref				[_Stencil]
				Comp			[_StencilComp]
				Pass				[_StencilOp]
				ReadMask	[_StencilReadMask]
				WriteMask	[_StencilWriteMask]
			}

			Cull Off
			Lighting Off
			ZWrite Off
			ZTest[unity_GUIZTestMode]
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask   [_ColorMask]				

			Pass
			{
				Name "GuideMask"
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0

				#include "UnityCG.cginc"
				#include "UnityUI.cginc" 

				#pragma multi_compile __  _STYLE_SPRITEMASK _STYLE_RECTMASK _STYLE_CIRCLEMASK _STYLE_BETTERRECTMASK


				//#pragma multi_compile __ UNITY_UI_ALPHACLIP

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
					//UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
					float4 worldPosition : TEXCOORD1;
					//UNITY_VERTEX_OUTPUT_STEREO

				};


				
				v2f vert(appdata_t IN)
				{
					v2f OUT;
					UNITY_SETUP_INSTANCE_ID(IN);
					//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
					OUT.worldPosition = IN.vertex;
					OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

					OUT.texcoord = IN.texcoord;

					OUT.color = IN.color ;
					return OUT;
				}

				sampler2D _MainTex;
				#ifdef _STYLE_SPRITEMASK
					sampler2D _subTex;
				#endif
				
				fixed4 _Color;
				float _rectWidth;
				float _rectHeight;

				float _centerPosX;
				float _centerPosY;

				float _centerPosX2;
				float _centerPosY2;

				float _centerPosX3;
				float _centerPosY3;

				float _SliderFactor;
				float _smoothRange;
				float _screenWidth;
				float _screenHeight;
				int _Count;

				//圆角大小写死
				float _DistanceDividsion = 0.03;

				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 color = tex2D(_MainTex, IN.texcoord) ;
					color.rgb *= IN.color.rgb;
					float2 uv = IN.texcoord;

					float2 coords[3];
                    coords[0] = float2(_centerPosX , _centerPosY);
                    coords[1] = float2(_centerPosX2 , _centerPosY2);
                    coords[2] = float2(_centerPosX3 , _centerPosY3);

					float2 rectPos = float2(_centerPosX , _centerPosY);
					float2 rectUV = uv - rectPos;
					
					float2 _screenSize = float2(_screenWidth , _screenHeight);
					float scaleFactor = _screenSize.x/_screenSize.y;

					float2 rectSize = float2(_rectWidth  , _rectHeight) ;

					float x = _screenSize.y/_screenSize.x;
					float y = _screenSize.x/_screenSize.y;

					float2 ScaleFac = float2(max(1.0 , y) , max(1.0 , x));

					//mask范围越大渐变过渡越宽
					float SmoothGap = _smoothRange * rectSize.x * 2.0; 
					
					

					float mask = 0;
					//为了消除四个角的渐变重叠试了另一种切法，用上下左右四个三角形来组成矩形,但是效果还是不太理想
					//half slice4Area = (1 - step(rectSize.y/rectSize.x , rectUV.y/rectUV.x)) * step(rectSize.y/-rectSize.x , rectUV.y/rectUV.x );

					//half t0 = step(rectSize.x/2 , rectUV.x)  ;
					//half triDown = slice4Area + step( 0 , rectUV.y) + p2;
					//half triUp = slice4Area + step(rectUV.y , 0 ) + p3;
					
					//half triLeft = 1 - slice4Area + step(0 , rectUV.x) + p0;
					//half triRight = 1 - slice4Area + step(rectUV.x , 0 ) + p1;

					//half tri4Mask = triDown*triUp*triLeft*triRight;

					//half2 shrinkFactor = half2(0.04 , 0.04*scaleFactor) ;
					#ifdef _STYLE_RECTMASK

						//rectSize.xy += shrinkFactor ;
						//rectSize.y *= 1+0.5 ;

						float p0 = 1.0f - smoothstep(-rectSize.x * 0.5f , -rectSize.x * 0.5f + SmoothGap*ScaleFac .y, rectUV.x);
						float p1 =  smoothstep(rectSize.x * 0.5f -SmoothGap*ScaleFac.y , rectSize.x * 0.5f , rectUV.x);
						float p2 = 1.0f - smoothstep(-rectSize.y * 0.5f , -rectSize.y * 0.5f + SmoothGap*ScaleFac.x , rectUV.y);
						float p3 =  smoothstep(rectSize.y * 0.5f - SmoothGap*ScaleFac.x , rectSize.y * 0.5f , rectUV.y);

						float line4Mask = p0+p1+p2+p3;
						mask = saturate(line4Mask);
					#endif



					#ifdef _STYLE_BETTERRECTMASK

						 float2 st = (frac(uv ) - rectPos) * 2.0 ;
						 //正式版用 scaleFactor
						st *=  ScaleFac;
						//计算距离场,几种算法
						float d = 0.0;//length(abs(st) - _DistanceOffset);
								//d = length(min(abs(st) - _DistanceOffset , 0.0));
								d = length(max(abs(st) - rectSize*ScaleFac, 0.0));

						//绘制距离场,几种算法
						float channel = 0.0;//frac(d * _DistanceDividsion);
								//channel = step(_DistanceDividsion , d );
							   //channel = step(_DistanceDividsion , d ) * step(d , _DistanceDividsion+ 0.1);
								channel = smoothstep(0.03 - 0.005,0.03 + 0.005 , d );
								mask = channel *IN.color.a;
					#endif

					

					
					#ifdef _STYLE_SPRITEMASK
						float Sfac = lerp(0.5 , 4 , _rectHeight/_screenHeight) ;
						float2 subuv = float2(rectUV.x * ((1)/ _rectWidth) , rectUV.y * ((1)/ _rectHeight))  + 0.5;//half2(rectUV.x * (1.5 / _rectWidth/2) , rectUV.y * (1.5 / _rectHeight/2))  + .5;
						fixed4 subColor = tex2D(_subTex , subuv);
						subColor.a *= step(subuv.x , 1) * step(subuv.y , 1) * step( 0 , subuv.x) * step( 0 , subuv.y);
						float spriteMask = 1 - subColor.a;
						mask = saturate(spriteMask);
					#endif
					
					#ifdef _STYLE_CIRCLEMASK
						mask = 1.0;
						//这里数量最多为3，写的最多只支持3个mask
						_Count = clamp(_Count , 1 , 3);
						for(int i = 0; i<_Count ; i++)
						{
							rectUV = uv - coords[i];
							float lenght = length(rectUV*ScaleFac);
							mask *= 1.0 - smoothstep(lenght , lenght + SmoothGap*ScaleFac , max(rectSize.x,rectSize.y)*.5);
						}
					#endif

					/*
					//圆形遮罩计算
					half angle = 6.283*(_SliderFactor-0.5);  
					//对于任何角度'a'，旋转矩阵为[[cos（a）， - sin（a）]，[sin（a），cos（a）]]
					half2x2 rot =  half2x2(0 , -1 , 1 , 0);
					rectUV = mul(rot , rectUV);
					mask = (1+sign(angle-atan2(rectUV.y, rectUV.x)))/2; 
					*/

					//color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
					//----------------------另一种原版改的写法----------------------------------
					//color.a *= (distance(IN.worldPosition.x,_Center.x) > _rectWidth) + (distance(IN.worldPosition.y,_Center.y) > _rectHeight);
					//color.a = clamp(color.a , 0, 1);
					//----------------------另一种原版改的写法----------------------------------
					
					color.a *= mask;
					color.a *= IN.color.a;

					return color;

				}
			ENDCG
			}
		}
}