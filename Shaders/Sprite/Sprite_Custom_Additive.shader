Shader "2d/Sprite_Custom_Additive"

{
	Properties
	{

		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

		[Toggle] WorldBending("WorldBending", Float) = 0.0  //开关
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




		Blend One One

		

		Pass 
		{
			Name "Sprite_Custom_Additive"
			CGPROGRAM

			

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc" 
            #include "Assets/Abolo_Lib/Shaders/AboloCG.cginc"  
            #pragma multi_compile_instancing
            #pragma multi_compile _ WORLDBENDING_ON	 




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
		
			uniform float _VertexCurveViewFac;
			v2f vert(appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				o.worldPosition = i.vertex;
				o.vertex = UnityObjectToClipPos(i.vertex);
            #ifdef WORLDBENDING_ON
				o.vertex = ABL_WorldBendTransform(i.vertex , _VertexCurveViewFac);
			#endif  
				o.texcoord = TRANSFORM_TEX(i.uv, _MainTex);
				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);

				return o;
			}


			UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(float, _Exposure)

		    UNITY_INSTANCING_BUFFER_END(Props)

			fixed4 frag(v2f IN) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				fixed4 color = tex2D(_MainTex , IN.texcoord)* IN.Color;


				color.rgb *= color.a;
				return color;
			}

			ENDCG
		}

		
	}
	FallBack "Sprite/Default"

}