Shader "2d/SpriteSelfRotation"
{
     Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _FacA("Speed" , Float) = 0.0

        [Toggle]_Turn_Off_HSB("TURN_OFF_HSB" , Float) = 0.0
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




		Blend One OneMinusSrcAlpha
       

        Pass
        {
            Name "SpriteSelfRotation"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc" 

            #pragma multi_compile_instancing
            #pragma multi_compile __ _TURN_OFF_HSB_ON

            #define TWO_PI 6.28318530718
            float mix(float a, float b , float t)
            {
                return b*t + a*(1.0 - t);
            }

            float3 mix3(float3 a , float3 b ,float t)
            {
                return float3(mix(a.x , b.x ,t) , mix(a.y , b.y ,t) , mix(a.z , b.z ,t));
            }

            float3 hsb2rgb(float3 c)
            {
                float3 rgb = saturate(abs(fmod(c.x * 6.0 + float3(0.0 , 4.0 , 2.0) , 6.0) -3.0) - 1.0);
                
                rgb = rgb * rgb * (3.0 - 2.0 * rgb);
                return c.z * mix3(float3(1.0,1.0,1.0) , rgb , c.y);
            }

            struct appdata_t
            {
                float4 vertex   : POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
               UNITY_VERTEX_INPUT_INSTANCE_ID //gpu instancing 默认的shader带了两个函数接口
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _FacA;

            UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
            UNITY_DEFINE_INSTANCED_PROP(float4, _UVRect)
            UNITY_DEFINE_INSTANCED_PROP(float4, _UVScale)

		    UNITY_INSTANCING_BUFFER_END(Props)
  

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord , _MainTex );
                OUT.color = v.color * UNITY_ACCESS_INSTANCED_PROP(Props , _Color);
                UNITY_TRANSFER_INSTANCE_ID(v,OUT);
                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                float2 OriUV = IN.texcoord;

                //IN.color.rgb *= saturate(sin(_Time.y)*0.3 +1.0);

                float t = frac(_Time.y/1000.0)*1000.0 * _FacA;

                //旋转矩阵
                float2x2 rot = float2x2(cos(t) , -sin(t),
                                                    sin(t) , cos(t));

                float2 uv = IN.texcoord;
                float2 center = OriUV - 0.5;

                //矩阵相乘旋转
                float2 st = mul(rot,center);

                //----------
                //坐标转换
                float2 toCenter = float2(0.5 , 0.5)-OriUV;
                float angle = atan2(toCenter.y,toCenter.x);
                float radius = length(toCenter)*2.0;
                float3 col = hsb2rgb(float3((angle/TWO_PI)+0.5,radius,1.0));
                #ifdef _TURN_OFF_HSB_ON
                    col = 1.0;
                #endif
                //----------

                fixed4 color = tex2D(_MainTex , st +0.5)*IN.color * float4(col , 1.0);
                color.rgb *= color.a;
                return color;

            }
        ENDCG
        }
    }
}
