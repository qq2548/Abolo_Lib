Shader "2d/UV_FlowEffect_Additive"

{
	Properties
	{

		[PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}

		[Header(R_Flow __ G_Flow __ B_Mask)]
		_SubTex ("SubTexture", 2D) = "white" {}
		[Toggle]_FromAtlas("FromAtlas" , Float) = 0.0
		[KeywordEnum(Default , SelfFlow , SelfFlowMask , ShootLight , Flame)]_Pattern("Effect_Pattern" , Float) = 0.0

		_Color ("TintColor", Color) = (1.0,1.0,1.0,1.0)

		_Angle("Angle" , Range(0.0 , 6.2831)) = 0.0
		_ArcRange("ArcRange" , Range(-0.5 , 1.5)) = 0.0

		_UVscalar("UVscalar" , Range(0.0 , 1.0)) = 0.6


		[Header(StencilBuffer)]//模板缓冲
		_StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15


		
		//流光特效速度参数
		_XFlowSpeed("XFlowSpeed", Range(-15.0 , 15.0)) = 1.0
        _YFlowSpeed("YFlowSpeed", Range(-15.0 , 15.0)) = 1.0

		[HideInInspector]
        _UVRect("UVRect" , Vector) = (0,0,1,1)
        [HideInInspector]
        _UVScale("UVScale" , Vector) = (0,0,0,0)


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



		Blend One One



		Pass 
		{
			
			CGPROGRAM

			

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc" 
			#include "UnityUI.cginc"  

			#pragma multi_compile_instancing
			#pragma multi_compile __ _FROMATLAS_ON
			#pragma multi_compile __ _PATTERN_SELFFLOW _PATTERN_SELFFLOWMASK _PATTERN_SHOOTLIGHT _PATTERN_FLAME


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
				
				float2 uv1 : TEXCOORD2;
				
				fixed4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				
			};

			sampler2D _MainTex;
			sampler2D _SubTex;
			float4 _MainTex_ST;
			float4 _SubTex_ST;
			v2f vert(appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				//_SubTex_ST.xy = 1.0 / _MainTex_ST.xy;
				o.worldPosition = i.vertex;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.texcoord = TRANSFORM_TEX(i.uv, _MainTex);
				
				o.uv1 =TRANSFORM_TEX(i.uv, _SubTex);
				
				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);

				return o;
			}

			


			float _XFlowSpeed;
			float _YFlowSpeed;

			half _UVscalar;
			float _Angle;
			float _ArcRange;

			UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(half4, _Color)
			#ifdef _FROMATLAS_ON
				UNITY_DEFINE_INSTANCED_PROP(half4, _UVRect)
				UNITY_DEFINE_INSTANCED_PROP(half4, _UVScale)
			#endif
		    UNITY_INSTANCING_BUFFER_END(Props)


			fixed4 frag(v2f IN) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);

				#ifdef _FROMATLAS_ON
					//atlas 图片uv归一化
					float4 UVRect = UNITY_ACCESS_INSTANCED_PROP(Props , _UVRect);
					float4 UVScale = UNITY_ACCESS_INSTANCED_PROP(Props , _UVScale);

					float2 sprcenter = (UVRect.zw - UVRect.xy) * 0.5;
					IN.uv1 = IN.uv1 - UVRect.xy - sprcenter;
					IN.uv1 *= UVScale;
					IN.uv1 += sprcenter;
					IN.uv1 = IN.uv1 / (UVRect.zw - UVRect.xy);
					//----------------------------------------
				#endif
				fixed4 color = tex2D(_MainTex , IN.texcoord);
				float t = frac(_Time.y * 0.001) * 1000.0;
				//旋转矩阵
                half2x2 rot = half2x2(cos(_Angle) , sin(_Angle),
                                                    -sin(_Angle) , cos(_Angle));

				//half d = length(max(abs((IN.texcoord - 0.5) * 2.0) - _RectRadius, 0.0));
				//half mask =step(d , _CircleRadius);
				#ifdef _PATTERN_SELFFLOW
					color = tex2D(_MainTex , mul(IN.texcoord, rot) *_UVscalar + float2(t *_XFlowSpeed, t *_YFlowSpeed) );
				#endif

				#ifdef _PATTERN_SELFFLOWMASK
					fixed mask = tex2D(_SubTex , IN.uv1).b;
					fixed FlowLight = tex2D(_SubTex , IN.texcoord + float2(-t *_XFlowSpeed, -t *_YFlowSpeed) ).r;
					color = tex2D(_MainTex , mul(IN.texcoord, rot) *_UVscalar + float2(t *_XFlowSpeed, t *_YFlowSpeed) );
					color.rgb += FlowLight;
					color.a *= mask;
				#endif

				#ifdef _PATTERN_SHOOTLIGHT
					_ArcRange = frac(t * _XFlowSpeed * 1.5) * 2.0 - 0.5; 
					float _ofArcRange = frac(t  * _XFlowSpeed * 1.5 + 0.4) * 2.0 - 0.5; 
					float2 uv = IN.texcoord;
					float2 center = uv - 0.5;
					float arc  =abs( center.y /length(center));
					color.a *=saturate( 
										saturate(
											(1.0 - smoothstep(1.0 - _ArcRange, 1.1 - _ArcRange , arc)) * (ceil(-center.x)) 
											+(1.0 - smoothstep(_ArcRange, _ArcRange + 0.1 , arc)) * ceil(center.x)
											+ (step( max(0.0 , -center.x ), _ArcRange - 1.0))
											+(1.0 - step( min(0.0 , -center.x ), _ArcRange))
										)*.7
										+saturate(
											(1.0 - smoothstep(1.0 - _ofArcRange, 1.1 - _ofArcRange , arc)) * (ceil(-center.x)) 
											+(1.0 - smoothstep(_ofArcRange, _ofArcRange + 0.1 , arc)) * ceil(center.x)
											+ (step( max(0.0 , -center.x ), _ofArcRange - 1.0))
											+(1.0 - step( min(0.0 , -center.x ), _ofArcRange))
										)*.3
									);
				#endif

				#ifdef _PATTERN_FLAME
					float2 uv = IN.texcoord;
					IN.texcoord.y *= cos(t*_YFlowSpeed) *0.05 +0.95;
					uv.y *=(sin(t *_YFlowSpeed)*0.5 + 10.5 ) * 0.1;
					fixed4 subColor = tex2D(_SubTex , uv - float2(sin(t*0.1*_YFlowSpeed +uv.y * 8.0) * 0.05 , t * 0.1*_YFlowSpeed));
					uv.x += sin(t*_YFlowSpeed - uv.y * 2.0) * uv.y *0.05  + subColor.r * (IN.texcoord.x - 0.5) *smoothstep(0.1,1.0 , IN.texcoord.y)* 4.0;
					uv.x += cos(t*(_YFlowSpeed +1) - uv.y * 4.0 )* uv.y *0.01  + subColor.r * (IN.texcoord.x - 0.5) * IN.texcoord.y * 2.0;
					uv.x += sin(t*(_YFlowSpeed +4) - uv.y *6.0)* uv.y * 0.02 + subColor.g * (IN.texcoord.x - 0.5) * IN.texcoord.y * 0.5;
					color = tex2D(_MainTex , uv) ;
					color.rgb =saturate(color.rgb + lerp(fixed3(1.0,0.9,0.0) , fixed3(0.0,0.0,0.0) , (1.0 - IN.texcoord.y * 0.5)));
					color.rgb *= 1.0 - smoothstep(0.4,0.9,IN.texcoord.y);

					//color
				#endif
				
				color *= UNITY_ACCESS_INSTANCED_PROP(Props, _Color) * IN.Color;
				color.rgb *= color.a;

				return color;
			}

			ENDCG
		}

	
		
	}

}