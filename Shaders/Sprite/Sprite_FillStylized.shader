Shader "2d/Sprite_FillStylized"
{
	Properties
	{

		[PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}

		_Color ("TintColor", Color) = (1.0,1.0,1.0,1.0)

		[KeywordEnum(Default , ClockWise , Verticle , Horizontle)]_Pattern("Effect_Pattern" , Float) = 0.0

		_Fill("Fill" , Range(0.0, 1.0)) = 1.0


		[Toggle]_GrayMode("GrayMode" , Int) = 0.0
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


					
		Cull Off
		Lighting Off
		ZWrite Off



		Blend one OneMinusSrcAlpha
		Pass 
		{
			
			CGPROGRAM
			


			#pragma vertex vert
			#pragma fragment frag 
			#pragma target 3.0
			#include "UnityCG.cginc" 

			#pragma multi_compile_instancing
			#pragma multi_compile __ _PATTERN_DEFAULT _PATTERN_CLOCKWISE _PATTERN_VERTICLE _PATTERN_HORIZONTLE
			#pragma shader_feature _GRAYMODE_ON
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
				o.uv.xy = TRANSFORM_TEX(i.uv, _MainTex);
				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);

				return o;
			}

			fixed _Fill;
			UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
		    UNITY_INSTANCING_BUFFER_END(Props)

			
			fixed4 frag(v2f v) : SV_Target
			{
				
				const float2x2 rot = float2x2(
													cos(-1.5708) , sin(-1.5708),
													-sin(-1.5708), cos(-1.5708)
												);
				UNITY_SETUP_INSTANCE_ID(v);

				fixed4 color = tex2D(_MainTex , v.uv);

				color *= UNITY_ACCESS_INSTANCED_PROP(Props, _Color) ;
				
				#ifdef _PATTERN_DEFAULT
					//color.a = color.a;
					//do nothing
				#endif

				#ifdef _PATTERN_VERTICLE
					color.a *= step(v.uv.y , _Fill);
				#endif

				#ifdef _PATTERN_HORIZONTLE
					color.a *= step(v.uv.x , _Fill);
				#endif

				#ifdef _PATTERN_CLOCKWISE
					float2 ra = v.uv - 0.5;
					ra = mul(rot , ra);
					fixed factor = step(atan2( ra.y,ra.x)  , (_Fill -0.5)  * 3.1415 * 2.0);
					color.a *= factor;
				#endif
					

				#ifdef _GRAYMODE_ON
					color.rgb = dot(color.rgb , fixed3(0.29 , 0.59 , 0.12));
					color.rg = saturate(color.rg + fixed2(0.05,0.05));
				#endif
				color.rgb *= color.a;
				color *= v.Color;

				return   color;
			}

			ENDCG
		}

	
		
	}

}
