Shader "Custom/Diffuse" {
    Properties 
    {
        _MainTex ("MainTex", 2D) = "white" {}

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 150

    CGPROGRAM
    #pragma surface surf Lambert noforwardadd
    #include "Assets/Abolo_Lib/Shaders/AboloCG.cginc" 
    sampler2D _MainTex;
    struct Input
    {
        float2 uv_MainTex;
    };

    void myNormal (inout appdata_full v ,  inout SurfaceOutput o) {
			o.Normal = UnityObjectToWorldNormal(v.normal);
		}
    float _SnowIntens;
    void surf (Input IN, inout SurfaceOutput o) {
        fixed4 color = tex2D(_MainTex, IN.uv_MainTex);
        
        //积雪效果试做
        float Noise = noise( IN.uv_MainTex * 20);
        Noise =saturate(Noise* Noise * 1.2);
        fixed4 snowColor = fixed4(1,1,1,1) * 0.75;
        fixed snowFac = dot(o.Normal , fixed3(0.0 , 1.0 , 0.0));
        color.rgb = lerp(color.rgb , snowColor.rgb , snowFac * Noise * _SnowIntens);
        //积雪效果试做
        o.Albedo =  (color.rgb);
        o.Alpha = color.a;


    }
    ENDCG
    }

    Fallback "Mobile/Diffuse"
}