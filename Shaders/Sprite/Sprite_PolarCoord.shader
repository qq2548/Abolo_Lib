Shader "2d/Sprite_PolarCoord"

{
	Properties
	{

		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Subtex ("Sub Texture", 2D) = "white" {}
        [Space(20)] //与上一行的间距
		_DistanceOffset ("DistanceOffset", Float) = 0.0
		_DistanceDividsion ("DistanceDividsion", Float) = 0.0
		_Speed ("Speed", Float) = 0.0
		_Thickness ("Thickness", Float) = 0.0
		_Color ("TintColor", Color) = (1.0,1.0,1.0,1.0)


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
															
		LOD 100		


		
	
		Cull Off
		Lighting Off
		ZWrite Off




		Blend One OneMinusSrcAlpha



		Pass 
		{
			Name "SpritePolarCoord"
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc" 
			#include "UnityUI.cginc" 
			#include "Assets/Abolo_Lib/Shaders/AboloCG.cginc" 

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
				float2 uv : TEXCOORD0;
				float2 sub_uv : TEXCOORD1;
				float4 worldPosition : TEXCOORD2;
				fixed4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				
			};

			sampler2D _MainTex;
			sampler2D _Subtex;	
			float4 _MainTex_ST;
			float4 _Subtex_ST;



			v2f vert(appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				o.worldPosition = i.vertex;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv = TRANSFORM_TEX(i.uv, _MainTex);
				o.sub_uv = TRANSFORM_TEX(i.uv, _Subtex);
				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);

				return o;
			}




			UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
		    UNITY_INSTANCING_BUFFER_END(Props)

			float _DistanceOffset;
			float _DistanceDividsion;
			float _Speed;
			float _Thickness;
			fixed4 frag(v2f IN) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				float t = ABL_FixTime(_Time.y);
				float2 coord = IN.uv - 0.5;//mul(IN.uv - 0.5 , ABL_2dRotationMatrix(10.0 , -t)) ;
				float2 rot_coord = mul(coord, ABL_2dRotationMatrix(_Speed , -t)) ;
				float polar = atan2(rot_coord.x, rot_coord.y) / 6.2830 + 0.5;
				polar = saturate(polar * 2 - step(0.5 , polar));
				float sub_polar = atan2(coord.x, coord.y) / 6.2830 + 0.5;
				 float2 st = frac(IN.uv) * 2.0 - 1.0;
                //st *= aspectRatio;
                float d = length(max(abs(st) - _DistanceOffset , 0.0)) ;  
				float a = _DistanceDividsion - _Thickness;
				float b = _DistanceDividsion;
				fixed channel = smoothstep(a ,b , d)* step(d , _DistanceDividsion);//step(_DistanceDividsion - 0.1 , d ) * step(d , _DistanceDividsion+ 0.1);
				fixed4 color = tex2D(_MainTex, float2(polar , channel)) * IN.Color;
				fixed4 sub_color = tex2D(_Subtex , mul(IN.sub_uv - 0.5 , ABL_2dRotationMatrix(0.1 , t)));

				fixed mask = polar;
				mask = saturate(mask  + sub_color.r);
				color.a *= mask * mask;//sub_color.r * (abs(polar - 0.5) * 2.0);

				fixed4 test_color = 1.0;
				test_color.rgb = mask;
				color.rgb *= color.a;
				return color;
			}

			ENDCG
		}

	
		
	}

}