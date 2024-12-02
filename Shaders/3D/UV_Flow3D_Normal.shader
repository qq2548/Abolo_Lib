///这个shader高光效果作用于sprite上，叠加sprite上，不分开渲染
Shader "sample3d/UV_Flow3D_Normal"
{
	Properties
	{
		//main type of Properties ,annotation if no need
		_Color("myColor",Color) = (1,1,1,1)
		_MainTex("MainTex",2D) = "white"{}
		_subTex2D("subTex2D",2D) = "black"{}


		_flowStrength("FlowStrength" ,Range(0,1)) = 1.0
		_flowSpeedX("flowSpeedX" ,Range(0,10)) = 1.0
		_flowSpeedY("flowSpeedY" ,Range(0,10)) = 1.0
		_reverseInt("reverseFac" , int) = 1

	}
	SubShader 
	{
		Tags 
        {
            "RenderType" = "Opaque"
            "IgnoreProjector"="True"
            "Queue" = "Geometry" 
        }

		ZWrite On






		Pass
		{
			Cull Off
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"  
			#include "Assets/Abolo_Lib/Shaders/AboloCG.cginc" 
			#pragma multi_compile_instancing


			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _subTex2D;
			//float4 _Color;
			float _flowSpeedX;
			float _flowSpeedY;
			int _reverseInt;

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
				float4 WorldPos:TEXCOORD1;
				float4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			fixed _flowStrength;


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
				o.WorldPos = mul(unity_ObjectToWorld, i.vertex);
				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);
				return o;
			}

			float4 frag(v2f v) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(v);

				fixed4 tint = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

				float TimeFac = ABL_FixTime(_Time.x);
				float2 center = v.uv*2.0 -1.0;
				float4 subColor = tex2D(_subTex2D,v.uv);

				float2 fireuv = float2( v.uv.x - TimeFac *_flowSpeedX *_reverseInt ,v.uv.y - TimeFac *_flowSpeedY * _reverseInt);
				float flowColor = tex2D(_subTex2D,fireuv);

				float4 color = tex2D(_MainTex,v.uv) * v.Color;
				flowColor *= subColor.g;
				
				color.rgb +=flowColor.r *_flowStrength;
				color.rgb = saturate(color.rgb * _Color.rgb);

				

				return color;
			}

			ENDCG
		}


		
	}

}