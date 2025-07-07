Shader "2d/Sprite_UV_Mapping"
{
     Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}

        [Header(Rchannel is U __Gchannel is V__ Bchannel is Alpha)]
        _Sub_Tex("Sub Texture", 2D) = "white" {}

        _Color ("Tint", Color) = (1,1,1,1)
        _XaxisSpeed("Xaxis" , Float) = 1.0
        _YaxisSpeed("Yaxis" , Float) = 1.0
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



	
		Cull Back
		Lighting Off
		ZWrite Off

		Blend One OneMinusSrcAlpha
       

        Pass
        {
            Name "Sprite_UV_Mapping"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_instancing

            #define TWO_PI 6.28318530718

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
            sampler2D _Sub_Tex;
            float4 _MainTex_ST;
            float _XaxisSpeed;
            float _YaxisSpeed;

            UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(float4, _Color)

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

                float t = frac(_Time.y/1000.0)*1000.0;
                
                fixed4 subColor = tex2D(_Sub_Tex , OriUV);
                fixed4 color = tex2D(_MainTex , float2(subColor.r + t * _XaxisSpeed, subColor.g + t * _YaxisSpeed))*IN.color;
                color.a *= subColor.b;
                color.rgb *= color.a;
                return color;

            }
        ENDCG
        }
    }
}
