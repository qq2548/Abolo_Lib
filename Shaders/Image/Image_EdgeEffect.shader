///这个shader的高光用于单独图像，与背景相加
Shader "2d/Image_EdgeEffect"
{
	Properties
	{
		//main type of Properties ,annotation if no need
		_Color("myColor",Color) = (1,1,1,1)
		_MainTex("MainTex",2D) = "white"{}

		_gap("gap" , Range(0.0 , 0.5)) = 0.05
		_offsetX("offset_x" , Range(-0.5 , 0.5)) = 0.05
		_offsetY("offset_y" , Range(-0.5, 0.5)) = 0.05
		
		_WaveHeight("Wave Height",Range(0,0.1)) = 0.01
		_WaveFrequency("Wave Frequency",Range(1,100)) = 50
		_WaveSpeed("Wave Speed",Range(-10,10)) = 1
		 [KeywordEnum(Outter,Inner)]_Type("Type" , Float) = 0.0

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
			#include "UnityCG.cginc"  
			#include "UnityUI.cginc" 
            #include "Assets/Abolo_Lib/Shaders/AboloCG.cginc" 
			

			#pragma multi_compile  _TYPE_OUTTER _TYPE_INNER
			

			float2 WaveMotion(float4 mainTex_ST , float2 uv , float _WaveFrequency ,  float _WaveSpeed , float _WaveHeight , float TimeFactor )
			{
				float2 uvCoord = mainTex_ST.xy * float2(0.5 , 1) ;
				
				float2 uvDir = normalize(uv-uvCoord);
				float   uvDis = distance(uv,uvCoord);
				float2 wave_uv = uv+sin(uvDis*_WaveFrequency - TimeFactor *_WaveSpeed)*_WaveHeight*uvDir;
				return wave_uv;
			}

			sampler2D _MainTex;
			float4 _MainTex_ST;
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



			v2f vert(appdata i)
			{
				v2f o;
				//o.vertex.x *= (_SinTime.z * 0.5 + 0.5) * 2;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv = TRANSFORM_TEX(i.uv, _MainTex);
				o.Color = i.Color;

				return o;
			}


			float _WaveHeight;
			float _WaveFrequency;
			float _WaveSpeed;
			fixed _gap;
			fixed _offsetX;
			fixed _offsetY;
			
			fixed4 frag(v2f input) : SV_Target
			{

				//方便偏移计算
				const fixed2 Box5[5] = 
				{
					fixed2(-1 ,-1),fixed2(1 ,-1),
					fixed2(0 ,0),
					fixed2(-1 ,1),fixed2(1 ,1),
				};
				//时间变量
				float TimeFac = ABL_FixTime(_Time.y);
				fixed4 color = tex2D(_MainTex , input.uv);
				
				float channel = 0;
				
				#ifdef _TYPE_OUTTER
				{
				
					//波纹计算
					float2 uv = ABL_WaveMotion(_MainTex_ST , input.uv , _WaveFrequency , 
																		_WaveSpeed , _WaveHeight , TimeFac);
					for(int num = 0 ; num<5 ; num++)
					{
						channel += tex2D(_MainTex, uv + Box5[num] *_gap + fixed2(_offsetX , _offsetY)).a ;
					}
					//channel *= 1.0 - color.a;
				}
				#endif

				#ifdef _TYPE_INNER
				

					float2 uv = input.uv;
					
					float2 cuv = uv - float2(0.5 , 0.5);

					

					float mask1 = step(length(cuv * 2.0) , 0.95);
					cuv = cuv * smoothstep(0.99 , length(cuv * 2.0)* length(cuv * 2.0)* length(cuv * 2.0) , uv.y) ;
					cuv.x *=  saturate(uv.y + 0.3);
					//cuv.x += cos(TimeFac+ uv.y * 3.1415) * 0.05;
					cuv =  saturate(cuv + float2(0.5 , 0.5));

					float2 wave_uv = WaveMotion(_MainTex_ST , cuv, _WaveFrequency , 
																		_WaveSpeed , _WaveHeight , TimeFac);

					channel = tex2D(_MainTex , float2(cuv.x , cuv.y + TimeFac*0.2)).a;
					channel *= input.uv.y * smoothstep(0.95 ,0.9 ,input.uv.y) * mask1;
					//---test				
				#endif

				fixed4 edgeColor = input.Color;
				fixed4 col = fixed4(edgeColor.r,edgeColor.g,edgeColor.b,saturate(channel));
				//col *= saturate(channel) * edgeColor.a;


				color = saturate(col + color);
				col *= input.Color;
				return  col;
			}

			ENDCG
		}


		
	}

}