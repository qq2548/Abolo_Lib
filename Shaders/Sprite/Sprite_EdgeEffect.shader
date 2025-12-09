///这个shader的高光用于单独图像，与背景相加
Shader "2d/Sprite_EdgeEffect"
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
		_WaveSpeed("Wave Speed",Range(0,10)) = 1
		 [KeywordEnum(Outter,Inner)]_Type("Type" , Float) = 0.0
        [Toggle] WorldBending("WorldBending", Float) = 0.0  //开关
	}
	SubShader 
	{
		Tags{"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

		ZWrite Off
		ZTest LEqual
		Blend One OneMinusSrcAlpha




		Pass
		{
			Cull Off
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"  
			#include "Assets/Abolo_Lib/Shaders/AboloCG.cginc" 
			#pragma multi_compile_instancing
			#pragma multi_compile  _TYPE_OUTTER _TYPE_INNER
            #pragma multi_compile _ WORLDBENDING_ON			



			sampler2D _MainTex;
			float4 _MainTex_ST;
			struct appdata
			{
				float4 vertex : POSITION ;
				float2 uv : TEXCOORD0;

				float4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID //gpu instancing 默认的shader带了两个函数接口
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;

				float4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			
			//下列被宏包起来的属性才能在不破坏GPU合批的前提下被动态修改
            UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
			


		    UNITY_INSTANCING_BUFFER_END(Props)
			 uniform float _VertexCurveViewFac;
			v2f vert(appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				o.vertex = UnityObjectToClipPos(i.vertex);
			#ifdef WORLDBENDING_ON
				float4 vertexInfo = mul(unity_ObjectToWorld , i.vertex);
				float3 camDir = _WorldSpaceCameraPos.xyz - vertexInfo.xyz;
				float amount = -_VertexCurveViewFac;
				float fac_x = pow(camDir.x , 2) * amount;
				float fac_y = pow(camDir.z , 2) * amount;
				vertexInfo += float4(0, fac_y + fac_x , 0 , 0);
				//test end
				o.vertex = UnityObjectToClipPos(mul(unity_WorldToObject , vertexInfo));
			#endif
				o.uv = TRANSFORM_TEX(i.uv, _MainTex);
				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);
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
				UNITY_SETUP_INSTANCE_ID(v);


				////方便偏移计算
				//const fixed2 Box9[9] = 
				//{
				//	fixed2(-1 ,-1),fixed2(0 ,-1),fixed2(1 ,-1),
				//	fixed2(-1 ,0),fixed2(0 ,0),fixed2(1 ,0),
				//	fixed2(-1 ,1),fixed2(0 ,1),fixed2(1 ,1),
				//};

							//方便偏移计算
				const fixed2 Box5[5] = 
				{
					fixed2(-1 ,-1),fixed2(1 ,-1),
					fixed2(0 ,0),
					fixed2(-1 ,1),fixed2(1 ,1),
				};
				//时间变量
				float TimeFac = ABL_FixTime(_Time.y);
				fixed4 color = tex2D(_MainTex , input.uv) * input.Color;
				fixed channel = color.a;
				#ifdef _TYPE_OUTTER
				{
					
				
					//波纹计算
					float2 uv = ABL_WaveMotion(_MainTex_ST , input.uv , _WaveFrequency , 
																		_WaveSpeed , _WaveHeight , TimeFac);
					for(int num = 0 ; num<5 ; num++)
					{
						channel += tex2D(_MainTex, uv + Box5[num] *_gap + fixed2(_offsetX , _offsetY)).a ;
					}
					channel *= 1.0 - color.a;
				}
				#endif

				#ifdef _TYPE_INNER
				
					//波纹计算
					float2 uv = ABL_WaveMotion(-_MainTex_ST , input.uv , _WaveFrequency , 
																		_WaveSpeed , _WaveHeight , -TimeFac);
					for(int num = 0 ; num<5 ; num++)
					{
						channel *= tex2D(_MainTex, uv + Box5[num] *_gap + fixed2(_offsetX , _offsetY)).a ;
					}
					channel = (1.0 - channel) * color.a;
				#endif

				color.rgb *= color.a;
				fixed4 edgeColor = UNITY_ACCESS_INSTANCED_PROP(Pops , _Color);
				fixed4 col = fixed4(edgeColor.r,edgeColor.g,edgeColor.b,saturate(channel));
				col *= saturate(channel) * edgeColor.a;
				//col.rgb *= col.a;
				color = saturate(col + color);
				return  color;
			}

			ENDCG
		}


		
	}

}