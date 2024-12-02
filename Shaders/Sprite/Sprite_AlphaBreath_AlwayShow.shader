Shader "2d/Sprite_AlphaBreath_AlwayShow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TimeOffset("TimeOffset" , Float) = 0.0
        _BreathMinAlpha("BreathMinAlpha" , Float) = 0.0
        _Speed("Speed" , Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent"}
        LOD 100
        Blend One One
        Cull off
        ZWrite off
        ZTest LEqual
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Assets/Abolo_Lib/Shaders/AboloCG.cginc" 
            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            { 
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                 float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
             float _TimeOffset;
             fixed _BreathMinAlpha;
             float _Speed;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float t = ABL_FixTime(_Time.z  * _Speed);
                float n = _BreathMinAlpha*0.5 + 0.5;
                //fixed fac = saturate(sin(t + _TimeOffset)*0.5 + 0.5 + _BreathMinAlpha);
                fixed fac = saturate(sin(t + _TimeOffset) * (0.5 -_BreathMinAlpha*0.5) + 0.5 + (_BreathMinAlpha * 0.5));
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                 col.a *= fac;
                col.rgb *= col.a;
               
                return col;
            }
            ENDCG
        }
    }
}
