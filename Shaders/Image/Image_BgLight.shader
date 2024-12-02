Shader "2d/Image_BgLight"

//我的Unity Shader 标准模板
{
	Properties
	{
		
        [PerRendererData]_MainTex("Base (RGB)",2D) = "white" {}  //贴图
		
		_Slice("Slice" , int) = 16
		_Arc("Arc" , Range(0.0 , 0.1)) = 0.0
		_StencilComp ("Stencil Comparison", Float) = 8.0
        _Stencil ("Stencil ID", Float) = 0.0
        _StencilOp ("Stencil Operation", Float) = 0.0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255.0
        _StencilReadMask ("Stencil Read Mask", Float) = 255.0
        _ColorMask ("Color Mask", Float) = 15.0

        //_TexWidth("Sheet Width",float) = 0.0    //贴图宽度像素值
		_Color ("Tint", Color) = (1,1,1,1)
		_Egde("Egde",Range(0.0 , 1.0)) = 0.5
		_Speed("Speed",Range(-1.0 , 1.0)) = 1.0  


		
	}
	SubShader 
	{
		Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

		Stencil
        {
            Ref				[_Stencil]
			Comp			[_StencilComp]
			Pass				[_StencilOp]
			ReadMask	    [_StencilReadMask]
			WriteMask	[_StencilWriteMask]
        }

        ColorMask [_ColorMask]
															

		Zwrite	Off//深度测试，关闭的话不进行深度测试，可以去掉一些不必要的遮挡效果
		Ztest 	LEqual

		Blend SrcAlpha One


		Cull Off
		Pass 
		{
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag 
			#pragma target 2.0
			#include "UnityCG.cginc" 
			#include "UnityUI.cginc" 


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
				fixed4 Color:COLOR;
			};

			
			sampler2D _MainTex;            //主贴图
			float4 _MainTex_ST;

			fixed _Egde;
			fixed _Speed;
			int _Slice;
			fixed _Arc;
			#define TWO_PI 6.2831852


			fixed4 _Color;

			v2f vert(appdata i)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv = TRANSFORM_TEX(i.uv , _MainTex);
				o.Color = i.Color * _Color;

				return o;
			}
			
			

			fixed4 frag(v2f v) : SV_Target
			{
				
				float t = frac(_Time.y *0.001) * 1000.0;
				float2 st = (v.uv - float2(0.5,0.5)) * TWO_PI;
				float2 uv = mul(st , rot( t * _Speed));
				float mask = 0.0;//step(0.999,(st.x*st.x)/dot(st , st)); //step(fmod(((st.x*st.x)/dot(st , st)/6.2831852) ,0.02) , 0.01) ;
				float angleIncrease = TWO_PI/_Slice;

				//10次遍历性能消耗稍微大点，目前还没想到更好的写法
				for(int i = 0;i<_Slice*0.5;i++)
				{
					float2 coord = mul(uv, rot(angleIncrease*i ));
					mask +=smoothstep(0.99,0.9999,(coord.x*coord.x)/dot(coord , coord) * (1.0 + _Arc));
				}


				float f = abs(v.uv.y - 0.5)*2.0;
				fixed4 color = tex2D(_MainTex , v.uv) * v.Color;
				color.a *= mask *saturate((1.0 - length(st * 0.31))) * smoothstep(_Egde-0.05 , _Egde + 0.05 , 1.0 - f);
				return color;
			}

			ENDCG
		}

	
		
	}

}
