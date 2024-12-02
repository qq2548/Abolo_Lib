
Shader "sample3d/CustomWater02"
{
	Properties
	{
		[Header(The Shadow Must Be On Otherwise Shader Will Not Work)] //阴影必须打开，否则shader无效,----Header只能写英文字母和数字 不能使用符号中文这些
		//main type of Properties ,annotation if no need
		[Space(20)] //与上一行的间距

		_MainTex("Base (RGB)", 2D) = "white" {}
		_SubTex("Sub (RGB)", 2D) = "white" {}

		_VerticalSpeed("VerticalSpeed",Range(0,2)) = 0.01
		_FoamStrength("FoamStrength",Range(0,1)) = 0.01

		_WaveHeight("Wave Height",Range(0,0.1)) = 0.01
		_WaveFrequency("Wave Frequency",Range(1,100)) = 50
		_WaveSpeed("Wave Speed",Range(0,10)) = 1


		_MainColor("Main Color", Color) = (1,1,1,1)
		_HighLightColor("HighLightColor", Color) = (1,1,1,1)
		_EdgeColor("EdgeColor", Color) = (1,1,1,1)
		_ShadowColor("ShadowColor" , Color) = (1,1,1,1)
		_FoamThickness("FoamThickness", Float) = 0.0

		_ReflectThreshold("ReflectThreshold",Range(0,1.0)) = 0.0
		_TestThreshold("TestThreshold",Range(0,100.0)) = 1.0

		
	}
	SubShader 
	{
		Tags{"Queue" = "Transparent" "IgnoreProjector" = "True" }

		ZWrite Off
		ZTest LEqual
		//Blend [_SrcBlend] [_DstBlend]
		Blend SrcAlpha OneMinusSrcAlpha




		Pass
		{
			Cull Back
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"  
			//方法名带 ABL 的都在下面这个库里面
			#include "Assets/Abolo_Lib/Shaders/AboloCG.cginc" 
			#pragma multi_compile_instancing

			sampler2D _MainTex;
			sampler2D _SubTex;
			float4 _MainTex_ST;
			float4 _SubTex_ST;


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

				float2 stretchUV : TEXCOORD1;
				float2 singleUV : TEXCOORD2;
				float4 grabScreenbPos : TEXCOORD3;

				fixed4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			float _VerticalSpeed;
			v2f vert(appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.vertex .y += _CosTime.z *_VerticalSpeed;
				//o.vertex .y -= 3;
				o.uv =TRANSFORM_TEX(i.uv , _MainTex);
				o.stretchUV = TRANSFORM_TEX(i.uv , _SubTex);

				o.grabScreenbPos = ComputeGrabScreenPos(o.vertex);
				o.singleUV = i.uv;

				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);
				return o;
			}

			//不同平台下可能由于获取的纹理精度不同会导致效果有瑕疵			试试这两种写法1：sampler2D_float _CameraDepthTexture;
			//	可以强行拿到高精度深度贴图																				 2:  UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);//unity内置变量，无需在Properties中声明  
			float _FoamThickness;
			fixed4 _MainColor;
			fixed4 _ShadowColor;
			fixed4 _EdgeColor;
			fixed4 _HighLightColor;

			float _WaveHeight;
			float _WaveFrequency;
			float _WaveSpeed;

			float _ReflectThreshold;
			float _TestThreshold;

			fixed _FoamStrength;


			fixed4 frag(v2f IN) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				//时间变量
				float TimeFac = ABL_FixTime(_Time.y);
				
				//波纹计算
				float2 uv = ABL_WaveMotion(_MainTex_ST , IN.uv , _WaveFrequency , _WaveSpeed , _WaveHeight , TimeFac);
				//副纹理采样 ，用作主纹理输出结果的遮罩
				fixed4 subCol = tex2D(_SubTex , uv);

				fixed4 waveColor = {0,0,0,1};
				
				waveColor.rgb = tex2D(_MainTex, uv).r * subCol.r;
				
				//波纹计算 Over
				//IN.grabScreenbPos += sin(waveColor.r*10.0 ) ;
				//深度纹理采样
				float depth = ABL_ScreenSamplerDepth(_CameraDepthTexture , IN.grabScreenbPos);
				//float maskDepth = ABL_ScreenSamplerDepth(_CameraDepthTexture , IN.grabScreenbPos + 1);
				//float mask =  (saturate(maskDepth - IN.grabScreenbPos.w));

				//float4 OffsetGrabScreenbPos = IN.grabScreenbPos + waveColor.r*10.0;
				//depth = ABL_ScreenSamplerDepth(_CameraDepthTexture , IN.grabScreenbPos+ waveColor.r*10.0*mask);


				
				

				//waveColor.rgb = ABL_NormalBlendRGB(src , reflect.rgb,_ReflectThreshold);
				//waveColor.rgb = saturate(lerp(src , reflect.rgb , 1 - dd));
				waveColor.rgb += _MainColor.rgb ;
				//waveColor.rgb *= dd ;
				waveColor.a = _MainColor.a;
				fixed depthMask = saturate(0.35 * (depth - IN.grabScreenbPos.w));
				//水面交界处泡沫边缘 初始算法
				fixed foamLine = 1 -  saturate(_FoamThickness * (depth - IN.grabScreenbPos.w));
				waveColor.a *= depthMask;
				waveColor += foamLine * _FoamStrength;
				//魔改
				fixed simLight = tex2D(_MainTex, IN.stretchUV).b;
				//fixed shadowFac = saturate(1.0 - tex2D(_MainTex, IN.singleUV).b + 0.5);
				fixed foamLine11 = 1 - saturate(4.0 * (depth - IN.grabScreenbPos.w)  + (sin(uv.x * (subCol.r*.5 + 5))*0.3 + 0.3));
				foamLine = foamLine * subCol.b + saturate(pow(foamLine11 , 2) * 4.0) ;
				//fixed3 shadow = shadowFac * _ShadowColor.rgb;
				fixed4 color =  saturate(waveColor) + foamLine * _EdgeColor + simLight * ABL_DouddleChannelCross(_SubTex , IN.stretchUV , TimeFac , float3(0.0 , 1.0 , 0.0))*0.5;


				color = saturate(color);
				return color;
			}

			ENDCG
		}


		
	}
	//Fallback "fakeToon_3d"    
}