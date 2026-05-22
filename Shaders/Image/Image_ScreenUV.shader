Shader "2d/Image_ScreenUV"

{
	Properties
	{

		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_SubTex("SubTex",2D) = "white" {}  //副贴图
		_Color ("TintColor", Color) = (1.0,1.0,1.0,1.0)
		//_HorizontalFactor("HorizontalFactor" , Range(0.0 , 1.0)) = 0.0
		_VerticalFactor("VerticalFactor" , Range(0.0 , 1.0)) = 0.0
		_FeatherAmount("FeatherAmount" , Range(0.0 , 1.0)) = 0.0
		[Header(StencilBuffer)]//模板缓冲
		_StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

		[HideInInspector]_BlendMode("BlendMode" , int) = 0
		[HideInInspector]_BlendSrc("BlendSrc" , int) = 0
		[HideInInspector]_BlendDst("BlendDst" , int) = 0
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



		Blend [_BlendSrc] [_BlendDst] 

		

		Pass 
		{
			Name "ImageScreenUV"
			CGPROGRAM

			

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc" 
			#include "UnityUI.cginc"  



			struct appdata
			{
				float4 vertex : POSITION ;
				float2 uv : TEXCOORD0;
				fixed4 Color:COLOR;

			};

			struct v2f
			{
				
				float4 vertex : SV_POSITION;
				float4 texcoord : TEXCOORD0;
				float2 texcoord_sub : TEXCOORD1;
				float4 worldPosition : TEXCOORD2;
				fixed4 Color:COLOR;
				
			};

			sampler2D _MainTex;
			sampler2D _SubTex;
			float4 _MainTex_ST;
			float4 _SubTex_ST;
			
		    float _HorizontalFactor;
            float _VerticalFactor;
            float _FeatherAmount;

			float applyDotGain30(float g) {
				return g + 0.3 * (g * (1.0 - g));
			}
			v2f vert(appdata i)
			{
				v2f o;
				o.worldPosition = i.vertex;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.texcoord.xy = TRANSFORM_TEX(i.uv, _MainTex);				
				o.texcoord.zw = UNITY_PROJ_COORD(ComputeGrabScreenPos(o.vertex)).xy;
				o.texcoord_sub = TRANSFORM_TEX(o.texcoord.zw , _SubTex);
				o.Color = i.Color;

				return o;
			}



			fixed4 _Color;

			fixed4 frag(v2f IN) : SV_Target
			{

				// // Sample texture and convert to grayscale
				// vec4 texColor = tex2D(uTexture, vTexCoord);
				// float gray = dot(texColor.rgb, vec3(0.299, 0.587, 0.114));
				
				// // Apply dot gain compensation
				// float adjustedGray = applyDotGain30(gray);
				
				// // Create a simple halftone pattern based on screen frequency
				// float frequency = 40.0;
				// vec2 st = gl_FragCoord.xy / uResolution.xy;
				// vec2 pattern = mod(st * frequency, 1.0) - 0.5;
				
				// // Calculate dot radius based on the gain-adjusted grayscale value
				// float radius = sqrt(adjustedGray * 0.5);
				// float dist = length(pattern);
				
				// // Smooth edges for the dot
				// float dotFactor = smoothstep(radius + 0.01, radius - 0.01, dist);
				
				// // Output the final halftone color
				// gl_FragColor = vec4(vec3(dotFactor), 1.0);



				fixed4 color = tex2D(_MainTex , IN.texcoord.xy);
				fixed4 subColr = tex2D(_SubTex , IN.texcoord_sub);
				float offset_horizontal = subColr.r * 0.1;
				color *= _Color * IN.Color;
				float duration = 0.01;
				float ruler = (IN.texcoord.z - 0.5 ) * 0.9 + 0.5;
				color.a *= 
					smoothstep( _HorizontalFactor  + offset_horizontal - _FeatherAmount * 0.5 ,
						_HorizontalFactor + offset_horizontal + _FeatherAmount * 0.5 , ruler);


				//测试dot gain 30%
				float gray = dot(color.rgb, float3(0.299, 0.587, 0.114));
				// Apply dot gain compensation
				float adjustedGray = applyDotGain30(gray);
				// Create a simple halftone pattern based on screen frequency
				float frequency = 40.0;
				float2 st = IN.texcoord.xy;
				float2 pattern = fmod(st * frequency, 1.0) - 0.5;
				
				// Calculate dot radius based on the gain-adjusted grayscale value
				float radius = sqrt(adjustedGray * 0.5);
				float dist = length(pattern);

				// Smooth edges for the dot
				float dotFactor = smoothstep(radius + 0.01, radius - 0.01, dist);

				fixed4 gl_FragColor = float4(float3(adjustedGray , adjustedGray, adjustedGray), color.a);
				return color;
			}

			ENDCG
		}

		
	}

	CustomEditor "AboloLib.AboloImageShaderGUI" 
}