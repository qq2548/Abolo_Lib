Shader "2d/Sprite_Custom_WaterFloat"

{
	Properties
	{

		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        [Space(20)] //与上一行的间距
		_WaterColor("WaterColor" , Color) = (1.0,1.0,1.0,1.0)
		_WaterHeight("WaterHeight" , Range(0.0 , 1.0)) = 0.0
		_MaxHeight("MaxHeight" , Range(0.0 , 1.0)) = 0.0
		_Exposure ("Exposure", Range(1.0,5.0)) = 1.0

		_WaveHeight("Wave Height",Range(0,0.1)) = 0.01
		_WaveFrequency("Wave Frequency",Range(1,100)) = 50
		_WaveSpeed("Wave Speed",Range(0,10)) = 1

		_VerticalSpeed("VerticalSpeed",Range(0,2)) = 0.01
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
			Name "Sprite_Custom"
			CGPROGRAM

			

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc" 
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
				float2 texcoord : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				fixed4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _WaterColor;
			fixed _WaterHeight;
			fixed _MaxHeight;
			float _VerticalSpeed;
			v2f vert(appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				o.worldPosition = i.vertex;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.vertex .y += _CosTime.z *_VerticalSpeed;
				o.texcoord = TRANSFORM_TEX(i.uv, _MainTex);
				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);

				return o;
			}


			UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(float, _Exposure)

		    UNITY_INSTANCING_BUFFER_END(Props)

			float _WaveHeight;
			float _WaveFrequency;
			float _WaveSpeed;
			fixed4 frag(v2f IN) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				fixed2 centerPos = fixed2(0.5 , 1.0); 
				fixed2 uv = IN.texcoord;
				fixed2 center = uv - centerPos;
				
				fixed4 color = tex2D(_MainTex , IN.texcoord)* IN.Color;

				_WaterHeight += 0.05 *_CosTime.z;
				//时间变量
				float TimeFac = ABL_FixTime(_Time.y);
				//波纹计算
				float2 wave_uv = ABL_WaveMotion(_MainTex_ST , float2( uv.x , uv.y) , _WaveFrequency , 
																		_WaveSpeed , _WaveHeight , -TimeFac);

				fixed2 wave =fixed2(0.0 , cos(_Time.y*6.24318 + uv.x * 20.0) * 0.02);
				color.rgb *= UNITY_ACCESS_INSTANCED_PROP(Props, _Exposure) ;
				fixed alpha = tex2D(_MainTex ,wave_uv + fixed2(0.0 , -_WaterHeight)).a;
				fixed alpha1 =  tex2D(_MainTex , wave_uv + fixed2(0.0 , -_WaterHeight + 0.03)).a;
				alpha = color.a * (1.0 - alpha) * step( uv.y , _MaxHeight);

				alpha1 *= alpha * step( uv.y , 0.4);

				color.a *=  saturate(uv.y * 5.0);
				color.rgb = ABL_NormalBlendRGB(color.rgb , _WaterColor.rgb , alpha * (1.0 - uv.y) * _WaterColor.a);
				color.rgb += _WaterColor.rgb * alpha1 * 0.1;
				color.rgb = saturate(color.rgb);
				color.rgb *= color.a;
				return color;
			}

			ENDCG
		}

		
	}
	FallBack "Sprite/Default"

}