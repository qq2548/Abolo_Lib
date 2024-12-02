Shader "2d/Particle_ScreenPosStream"

//我的Unity Shader 标准模板
{
	Properties
	{
		
        _MainTex("Base (RGB)",2D) = "white" {}  //贴图
		_Color("tint" , Color) = (1,1,1,1)
	}
	SubShader 
	{
		Tags
		{
				"RenderType" = "Opaque" //TransparentCutout
				 "Queue" = "Transparent"
				 "IgnoreProjector"="True"
		}
															
		Cull Off
        ZWrite Off
        ZTest LEqual

		Blend One One


		Pass 
		{

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag 
			#pragma target 3.0
			#include "UnityCG.cginc" 
			#pragma multi_compile_particles

			#pragma multi_compile_instancing





			struct appdata
			{
				float4 vertex : POSITION ;
				float4 uv : TEXCOORD0;
				fixed4 Color:COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID //gpu instancing 默认的shader带了两个函数接口
				
				
			};

			struct v2f
			{
				
				float4 vertex : SV_POSITION;
				float4 uv : TEXCOORD0;
				fixed4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				//UNITY_VERTEX_OUTPUT_STEREO //gpu instancing 默认的shader带了两个函数接口
			};

			
			sampler2D _MainTex;            //主贴图
			float4 _MainTex_ST;

			v2f vert(appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);

				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv.xy = TRANSFORM_TEX(i.uv.xy , _MainTex);
				o.uv.zw = UNITY_PROJ_COORD(ComputeGrabScreenPos(o.vertex)).xy;
				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);

				return o;
			}
			


			UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
		    UNITY_INSTANCING_BUFFER_END(Props)

			fixed4 frag(v2f v) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(v);
				fixed4 tint = UNITY_ACCESS_INSTANCED_PROP(Props , _Color);

				fixed4 color = tex2D(_MainTex,v.uv.xy  )  * v.Color * tint;
				float2 screenUV = v.uv.zw;
				float2 center = 2*screenUV - float2(1 , 1);
				color.a *=saturate(length(center) - 0.2) * max(0.5 , screenUV.y);
				color.a *= color.a;
				color.rgb *= color.a;
				
				return color;
			}

			ENDCG
		}

	
		
	}

}
