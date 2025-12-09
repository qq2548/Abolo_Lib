
Shader "sample3d/CustomWater02"
{
	Properties
	{
		[Header(The Shadow Must Be On Otherwise Shader Will Not Work)] //阴影必须打开，否则shader无效,----Header只能写英文字母和数字 不能使用符号中文这些
		//main type of Properties ,annotation if no need
		[Space(20)] //与上一行的间距

		_MainTex("Base (RGB)", 2D) = "white" {}
		_SubTex("Sub (RGB)", 2D) = "white" {}

		_WaveHeight("Wave Height",Range(0,0.1)) = 0.01
		_WaveFrequency("Wave Frequency",Range(1,100)) = 50
		_WaveSpeed("Wave Speed",Range(0,10)) = 1

		_VertexAnimFac("VertexAnimFactor",Range(0,1)) = 0.5
		//_VertexCurveViewFac("VertexCurveViewFac",Range(0.0 , 0.1)) = 0.005

		_MainColor("Main Color", Color) = (1,1,1,1)
		_HighLightColor("HighLightColor", Color) = (1,1,1,1)
		_EdgeColor("EdgeColor", Color) = (1,1,1,1)
		_ShadowColor("ShadowColor" , Color) = (1,1,1,1)
		_FoamThickness("FoamThickness", Float) = 0.0

		_ReflectThreshold("ReflectThreshold",Range(0,1.0)) = 0.0
		_TestThreshold("TestThreshold",Range(0,100.0)) = 1.0
		_DepthScalor("DepthScalor",Range(0,1.0)) = 1.0

		_DepthOffset("DepthOffset",Range(0,1.0)) = 1.0

		_testFac("TestFactor",Range(0.0 , 100.0)) = 0.0
		
		[Toggle] WorldBending("WorldBending", Float) = 1.0  //开关
	}
	SubShader 
	{
		Tags{"Queue" = "Transparent" "IgnoreProjector" = "True" }

		ZWrite off
		ZTest LEqual
		//Blend [_SrcBlend] [_DstBlend]
		Blend SrcAlpha OneMinusSrcAlpha




		Pass
		{
			Cull Back
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"  
			//方法名带 ABL 的都在下面这个库里面
			#include "Assets/Abolo_Lib/Shaders/AboloCG.cginc" 
			#pragma multi_compile_instancing
			#pragma multi_compile _ WORLDBENDING_ON

			sampler2D _MainTex;
			sampler2D _SubTex;
			half4 _MainTex_ST;
			half4 _SubTex_ST;


			struct appdata
			{
				half4 vertex : POSITION ;
				half2 uv : TEXCOORD0;
				half3 normal : NORMAL ; 
				half4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID //gpu instancing 默认的shader带了两个函数接口
			};

			struct v2f
			{
				half4 vertex : SV_POSITION;
				half2 uv : TEXCOORD0;
				half4 screenPos : TEXCOORD1;
				half2 stretchUV : TEXCOORD2;
				half2 singleUV : TEXCOORD3;
				half4 grabScreenbPos : TEXCOORD4;
				half3 normal : NORMAL ; 
				half4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			half _VertexAnimFac;
			uniform float _VertexCurveViewFac;
			v2f vert(appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				//sphere_view test
				o.vertex = UnityObjectToClipPos(i.vertex);
			#ifdef WORLDBENDING_ON
				float4 vertexInfo = mul(unity_ObjectToWorld , i.vertex);
				float3 camDir = _WorldSpaceCameraPos.xyz - vertexInfo.xyz;
				float amount = -_VertexCurveViewFac;
				float fac_x = pow(camDir.x , 2) * amount;
				float fac_y = pow(camDir.z , 2) * amount;
				float fac_z = pow(camDir.y , 2) * amount;
				vertexInfo += float4(0, fac_y + fac_x , 0 , 0);
				//test end
				o.vertex = UnityObjectToClipPos(mul(unity_WorldToObject , vertexInfo));
			#endif
				o.vertex .y += _CosTime.z * _VertexAnimFac;
				o.uv =TRANSFORM_TEX(i.uv , _MainTex);
				o.stretchUV = TRANSFORM_TEX(i.uv , _SubTex);
				o.screenPos =  ComputeScreenPos(o.vertex); 
				o.grabScreenbPos = ComputeGrabScreenPos(o.vertex);
				o.singleUV = i.uv;
				o.normal = i.normal;
				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);
				return o;
			}

			//不同平台下可能由于获取的纹理精度不同会导致效果有瑕疵			试试这两种写法1：sampler2D_float _CameraDepthTexture;
			//	可以强行拿到高精度深度贴图																				 2:  UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);//unity内置变量，无需在Properties中声明  
			half _FoamThickness;
			half4 _MainColor;
			half4 _ShadowColor;
			half4 _EdgeColor;
			half4 _HighLightColor;

			half _WaveHeight;
			half _WaveFrequency;
			half _WaveSpeed;

			half _ReflectThreshold;
			float _TestThreshold;

			float _DepthScalor;
			float _DepthOffset;

			float _testFac;

			sampler2D _ReflectionTex;

			half4 frag(v2f IN) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				//时间变量
				half TimeFac = ABL_FixTime(_Time.y) * 0.5;
				
				//波纹计算
				half2 uv = ABL_WaveMotion(_MainTex_ST , IN.uv , _WaveFrequency , _WaveSpeed , _WaveHeight , TimeFac);
				//副纹理采样 ，用作主纹理输出结果的遮罩
				half4 subCol = tex2D(_SubTex , uv);

				half4 waveColor = {0,0,0,1};
				
				waveColor.rgb = tex2D(_MainTex, uv).r * subCol.r;
				
				//波纹计算 Over
				//IN.grabScreenbPos += sin(waveColor.r*10.0 ) ;
				//深度纹理采样
				half depth = ABL_ScreenSamplerDepth(_CameraDepthTexture , IN.grabScreenbPos);
				half maskDepth = ABL_ScreenSamplerDepth(_CameraDepthTexture , IN.grabScreenbPos + 1);
				half mask =  (saturate(maskDepth - IN.grabScreenbPos.w));

				//half4 OffsetGrabScreenbPos = IN.grabScreenbPos + waveColor.r*10.0;
				//depth = ABL_ScreenSamplerDepth(_CameraDepthTexture , IN.grabScreenbPos+ waveColor.r*10.0*mask);
				//计算倒影
				//反射纹理采样
				//half4 reflect = tex2D(_ReflectionTex , IN.grabScreenbPos.xy/IN.grabScreenbPos.w);

				
				half DistortionDepth = ABL_ScreenSamplerDepth(_CameraDepthTexture , IN.grabScreenbPos + (waveColor.r * _TestThreshold ) * mask);
				half dd = ( (DistortionDepth - IN.grabScreenbPos.w) * _DepthScalor + _DepthOffset) ;
				///half3 re =  saturate(reflect.rgb + half3(dd,dd,dd)) ;
				half3 src = waveColor.rgb;

				//waveColor.rgb = ABL_NormalBlendRGB(src , reflect.rgb,_ReflectThreshold);
				//waveColor.rgb = saturate(lerp(src , reflect.rgb , 1 - dd));
				waveColor.rgb += _MainColor.rgb * saturate(dd) ;
				waveColor.a = _MainColor.a;

				//水面交界处泡沫边缘 初始算法
				half foamLine = 1 - saturate(_FoamThickness * (depth - IN.grabScreenbPos.w));

				//魔改
				half simLight = tex2D(_MainTex, IN.stretchUV).b;
				half shadowFac = saturate(1.0 - tex2D(_MainTex, IN.singleUV).b + 0.5);
				half foamLine11 = 1 - saturate(4.0 * (depth - IN.grabScreenbPos.w)  + (sin(uv.x * (subCol.r*.5 + 5))*0.3 + 0.3));
				foamLine = foamLine * subCol.b + saturate(pow(foamLine11 , 2) * 4.0) ;
				half3 shadow = shadowFac * _ShadowColor.rgb;
				waveColor.rgb *= shadow;
				half4 color =  saturate(waveColor) + foamLine * _EdgeColor + simLight *_HighLightColor * 
					ABL_DouddleChannelCross(_SubTex , IN.stretchUV , fmod(TimeFac , 44) , half3(0.0 , 1.0 , 0.0));

				half4 tesColor = half4(1,1,1,1);
				tesColor.rgb =  1 - dd +  step(-1 , dd);
				tesColor.rgb *= 0.5;
				tesColor.rgb = mask;
				return color;
			}

			ENDCG
		}




		
	}
	//Fallback "fakeToon_3d"    
}