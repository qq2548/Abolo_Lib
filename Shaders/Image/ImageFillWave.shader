Shader "2d/ImageFillWave"

{
	Properties
	{

		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}



        [Space(20)] //与上一行的间距
		

		_Color ("TintColor", Color) = (1.0,1.0,1.0,1.0)

		_FillRange("FillRange" , Range(0.0 , 1.0)) = 0.5

		_WaveHeight("Wave Height",Range(0,0.5)) = 0.01
		_WaveFrequency("Wave Frequency",Range(1,100)) = 50
		_XFlowSpeed("XFlowSpeed",Range(0,10.0)) = 1


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
			Name "Front"
			CGPROGRAM

			

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc" 
			#include "UnityUI.cginc" 

			#pragma multi_compile_instancing


			float2 WaveMotion(float4 mainTex_ST , float2 uv , float _WaveFrequency ,  float _WaveSpeed , float _WaveHeight , float TimeFactor , fixed weight)
			{
				float2 uvCoord =  mainTex_ST.xy ;
				float2 uvDir = normalize(uvCoord);
				float   uvDis = distance(uv,uvCoord)* 3.1415926;
				float2 wave_uv = uv+cos(uvDis*_WaveFrequency - TimeFactor *_WaveSpeed)*_WaveHeight*uvDir *uv.y * (1.0 - abs(uv.x-0.5)*2.0)* weight;
				return wave_uv;
			}

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
			
			v2f vert(appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				o.worldPosition = i.vertex;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.texcoord = TRANSFORM_TEX(i.uv, _MainTex);
				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);

				return o;
			}

			




			UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)

		    UNITY_INSTANCING_BUFFER_END(Props)

			float _WaveHeight;
			float _WaveFrequency;
			float _XFlowSpeed;

			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;
			fixed _FillRange;
			fixed4 frag(v2f IN) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				float time = frac(_Time.y * 0.001) * 1000.0;
				fixed mask =  tex2D(_MainTex , IN.texcoord).a;
				//波纹计算
				float2 uv = WaveMotion(_MainTex_ST , IN.texcoord , _WaveFrequency , _XFlowSpeed , _WaveHeight , time ,1.0 - _FillRange);
				//副纹理采样 ，用作主纹理输出结果的遮罩
				
				//fixed4 subCol = tex2D(_SubTex , uv + float2(sin(time)*_XFlowSpeed , 0.0));


				fixed4 color = tex2D(_MainTex , uv) * IN.Color;

				//#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				//	if (_AlphaSplitEnabled)
				//		color.a = tex2D (_AlphaTex, uv).r;
				//#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				fixed edge = smoothstep(uv.y,uv.y + 0.02 , _FillRange);
				
				color.a *= edge * mask;
				fixed toplight = color.a *  (1.0 - smoothstep(uv.y+0.02,uv.y+0.04 ,  _FillRange));

				color.rgb += toplight * .2 *  (1.0 - abs(uv.x-0.5)*2.0);


				color *= UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
				 
				return color;
			}

			ENDCG
		}

	
		
	}

}