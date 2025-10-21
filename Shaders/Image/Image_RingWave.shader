Shader "2d/Image_RingWave"

{
	Properties
	{

        _MainTex("Base (RGB)",2D) = "white" {}  //贴图
        _SubTex("SubTex",2D) = "white" {}  //贴图
		_RingCount("RingCount" , int) = 3
        //_TexWidth("Sheet Width",float) = 0.0    //贴图宽度像素值
		_Color ("Tint", Color) = (1,1,1,1)
		_OuterColor ("OuterColor", Color) = (1,1,1,1)
		_InnerColor ("InnerColor", Color) = (1,1,1,1)
		_EgdeHardness("EgdeHardness",Range(0.0 , 1.0)) = 0.5
		_Thickness("Thickness",Range(0.0 , 1.0)) = 0.5
		_NoiseWeight("NoiseWeight",Range(0.0 , 1.0)) = 0.5
		_Speed("Speed",Range(-1.0 , 10.0)) = 1.0  


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



		Blend SrcAlpha One



		Pass 
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc" 
			#include "UnityUI.cginc" 



			//随机算法
             float random(float2 _uv)
            {
                return frac(sin(dot(_uv , float2(12.9898 , 78.233))) * 43758.5758321 );
            }

			//2D噪声
            float noise(float2 _uv)
            {
                float2 i = floor(_uv);
                float2 f = frac(_uv);

                //四向随机值
                float a = random(i);
                float b = random(i + float2(1.0 , 0.0));
                float c = random(i + float2(0.0 , 1.0));
                float d = random(i + float2(1.0 , 1.0));

                // Cubic Hermine Curve.  Same as SmoothStep()
                float2 u = f*f*(3.0-2.0*f); //软化 平滑
                // u = smoothstep(0.,1.,f);

                //百分比混合四角随机噪点
                return lerp(a, b, u.x) +(c - a)* u.y * (1.0 - u.x) +(d - b) * u.x * u.y;
            }


			float _EgdeHardness;

			float drawCircle(float2 st , float range , float thickness , float frequency , float time)
			{
			    thickness *= 1.0 - time;
				return (1.0 - smoothstep(range  * time, range * time  + _EgdeHardness, length(st) + noise(st)* frequency ))   
							* smoothstep(range * time - thickness, range  * time - thickness + _EgdeHardness, length(st) + noise(st) * frequency);
			}

			float2x2 rot(float angle)
			{
				return float2x2(cos(angle) , -sin(angle),
										sin(angle),    cos(angle)			
													);
			}



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
				float2 sub_uv : TEXCOORD1;
				fixed4 Color:COLOR;

			};

			
			sampler2D _MainTex;            //主贴图
			float4 _MainTex_ST;

			sampler2D _SubTex;            //副贴图
			float4 _SubTex_ST;

			v2f vert(appdata i)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv = TRANSFORM_TEX(i.uv , _MainTex);
				o.sub_uv = TRANSFORM_TEX(i.uv , _SubTex);
				o.Color = i.Color;

				return o;
			}
			
			float _RingCount;
			float _Thickness;
			float _Speed;
			float _NoiseWeight;
			fixed4 _OuterColor;
			fixed4 _InnerColor;


			fixed4 _Color;

			fixed4 frag(v2f v) : SV_Target
			{

				fixed4 tint = _Color;
				float2 st = (v.uv - float2(0.5 , 0.5)) * 10.0;
				float mask = 1.0 - smoothstep(2.0, 5.0 , length(st));
				float t = 0.0;
				float gap = 1.0/_RingCount;
				float col = 0.0;
				float time  = frac(_Time.y * 0.001)*1000.0;
				float2 coord = mul(rot(time) , st);
				for(int num = 0 ; num < _RingCount ; num++)
				{
					t = 1.0 - frac(_Time.y * _Speed + gap * num);
					col +=drawCircle(coord , 4.6 , _Thickness , _NoiseWeight , t );
				}

				fixed4 color = tex2D(_MainTex,v.uv  )  * v.Color;
				fixed4 sub_color = tex2D(_SubTex,v.sub_uv  ) ;
				col *= sub_color.r;
				fixed4 gradienColor = lerp(_InnerColor ,_OuterColor ,length(st) * 0.2);
				color = fixed4(col,col,col,1.0) * tint * v.Color; 
				color.rgb += gradienColor.rgb;
				color.rgb = saturate(color.rgb);
				color.a *= mask;
				color.rgb *= color.a;
				return color;
			}

			ENDCG
		}

	
		
	}

}
