Shader "2d/Sprite_Custom_Screw"

{
	Properties
	{

		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_SubTex ("SubTexture", 2D) = "white" {}

        [Space(20)] //与上一行的间距
		
		_PatternStrength ("PatternStrength", Range(0.0,1.0)) = 1.0

		[Header(StencilBuffer)]//模板缓冲
		_StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
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

		Stencil			//模板缓冲
		{
			Ref				[_Stencil]
			Comp			[_StencilComp]
			Pass				[_StencilOp]
		}


		Blend One OneMinusSrcAlpha

		

		Pass 
		{
			Name "Sprite_Custom_Screw"
			CGPROGRAM

			

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc" 
			#include "Assets/Abolo_Lib/Shaders/AboloCG.cginc" 






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

				float2 sub_texcoord : TEXCOORD2;

				fixed4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				
			};

			sampler2D _MainTex;
			sampler _SubTex;
			float4 _MainTex_ST;
			float4 _SubTex_ST;
			float4 _ClipRect;
			float _PatternStrength;
			v2f vert(appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				o.worldPosition = i.vertex;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.texcoord = TRANSFORM_TEX(i.uv, _MainTex);
				o.sub_texcoord = TRANSFORM_TEX( i.vertex, _SubTex);
				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);

				return o;
			}


			fixed4 frag(v2f IN) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				fixed4 color = tex2D(_MainTex , IN.texcoord)* IN.Color;
				//曝光计算
				fixed4 sub_color = tex2D(_SubTex , IN.sub_texcoord);


				sub_color.rgb = ABL_NormalBlendRGB(fixed3(1,1,1) , sub_color.rgb , _PatternStrength);
                color.rgb   *= saturate(sub_color.rgb);


				color.rgb *= color.a;

				return _ClipRect;
			}

			ENDCG
		}

		
	}
	FallBack "Sprite/Default"

}