///这个shader的高光用于单独图像，与背景相加
Shader "2d/Sprite_Rhythm"
{
	Properties
	{
		//main type of Properties ,annotation if no need
		_Color("myColor",Color) = (1,1,1,1)
		_MainTex("MainTex",2D) = "white"{}

		_gap("gap" , Float) = 0.05
		_width("width" , Float) = 0.1

		_division("division" , int) = 10

		samples0("Sample0" , Range(0.0 , 1.0)) = 0.0
		samples1("Sample1" , Range(0.0 , 1.0)) = 0.0
		samples2("Sample2" , Range(0.0 , 1.0)) = 0.0
		samples3("Sample3" , Range(0.0 , 1.0)) = 0.0
		samples4("Sample4" , Range(0.0 , 1.0)) = 0.0
		samples5("Sample5" , Range(0.0 , 1.0)) = 0.0
		

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
			#pragma multi_compile __ _FLIPX_ON _FLIPY_ON
			
            #define TWO_PI 6.28318530718




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

			float samples0;
			float samples1;
			float samples2;
			float samples3;
			float samples4;
			float samples5;
			
			//下列被宏包起来的属性才能在不破坏GPU合批的前提下被动态修改
            UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(float4, _Color)

		    UNITY_INSTANCING_BUFFER_END(Props)

			v2f vert(appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv = TRANSFORM_TEX(i.uv, _MainTex);
				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);
				return o;
			}



			int _division;
			float _gap;
			float _width;
			
			fixed4 frag(v2f v) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(v);

				fixed samples[6];
				samples[0] = floor(samples0 * _division) /_division;
				samples[1] = floor(samples1 * _division) /_division;
				samples[2] = floor(samples2 * _division) /_division;
				samples[3] = floor(samples3 * _division) /_division;
				samples[4] = floor(samples4 * _division) /_division;
				samples[5] = floor(samples5 * _division) /_division;

				float2 uv = v.uv;
				 //----------
                //坐标转换
                float2 toCenter = float2(0.5 , 0.5)-uv;
                float angle = atan2(toCenter.y,toCenter.x);
                float radius = length(toCenter)*2.0;
				float3 col = hsb2rgb(float3((angle/TWO_PI)+0.5,radius,1.0));

				//float gap = 0.03;
			 //   float width = 0.12;
				fixed bar = 0;
				float grid =  fmod( floor(uv.y * _division) , 2);
				for(int i =0 ; i < 6 ;i++)
				{
					bar += step(_gap * (i+1) + _width * i ,uv.x) * step(uv.x , (_width + _gap) * (i+1)) 
								* step(uv.y , samples[i]) * grid;

				}
				fixed4 outColor = fixed4(bar,bar,bar,bar);

				// Time varying pixel color
				fixed3 rimCol = 0.5 + 0.5*cos(_Time.y+uv.xyx+fixed3(0,2,4));


				fixed4 bgColor = UNITY_ACCESS_INSTANCED_PROP(Pops , _Color);
				fixed4 backgroundCol = fixed4(bgColor.r , bgColor.g , bgColor.b , (1.0 - bar));
				backgroundCol.rgb *= (1.0 - rimCol) * 0.5;
				//color.rgb = v.Color.rgb *v.Color.a;
				//color.rgb *= color.a;
				
				fixed rr = (cos(_Time.y*0.5 + 3.1415 * 0.5)*0.5+0.5);
				fixed gg = (sin(_Time.y*0.5)*0.5+0.5);
				fixed bb = (cos(_Time.y*0.5)*0.5+0.5) ;


				//outColor = bar0 + bar1 + bar2 + bar3 + bar4 + bar5;
				outColor.rgb *= (fixed3(rr,gg,bb) + uv.y) * saturate(uv.y + 0.5);

				outColor = outColor + backgroundCol;

				outColor.rgb *= outColor.a; 

				return outColor ;
			}

			ENDCG
		}


		
	}

}