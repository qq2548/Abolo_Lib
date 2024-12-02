Shader "2d/Image_Breath_Additive"

{
	Properties
	{

		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        [Space(20)] //与上一行的间距

	    _TimeOffset("TimeOffset" , Float) = 0.0
        _BreathMinAlpha("BreathMinAlpha" , Float) = 0.0
        _Speed("Speed" , Float) = 1.0

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



		Blend SrcAlpha One

		

		Pass 
		{
			Name "ImageBreath"
			CGPROGRAM

			

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc" 
			#include "UnityUI.cginc" 
            #include "Assets/Abolo_Lib/Shaders/AboloCG.cginc" 




			struct appdata
			{
				float4 vertex : POSITION ;
				float2 uv : TEXCOORD0;
				fixed4 Color:COLOR;

			};

			struct v2f
			{
				
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				fixed4 Color:COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
             float _TimeOffset;
             fixed _BreathMinAlpha;
             float _Speed;

			v2f vert(appdata input)
			{
				v2f output;
				output.worldPosition = input.vertex;
				output.vertex = UnityObjectToClipPos(input.vertex);
				output.uv = TRANSFORM_TEX(input.uv, _MainTex);
				output.Color = input.Color;

				return output;
			}



			fixed4 frag(v2f IN) : SV_Target
			{
				float t = ABL_FixTime(_Time.z  * _Speed);
                float n = _BreathMinAlpha*0.5 + 0.5;
                //fixed fac = saturate(sin(t + _TimeOffset)*0.5 + 0.5 + _BreathMinAlpha);
                fixed fac = saturate(sin(t + _TimeOffset) * (0.5 -_BreathMinAlpha*0.5) + 0.5 + (_BreathMinAlpha * 0.5));
                // sample the texture
                fixed4 col = tex2D(_MainTex, IN.uv) * IN.Color;
                 col.a *= fac;
                col.rgb *= col.a;
               
                return col;
			}

			ENDCG
		}

		
	}
	//FallBack "UI/Default"

}