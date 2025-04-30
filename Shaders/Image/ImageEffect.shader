Shader "2d/ImageEffect"
{
     Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _Speed("Speed" , Range(-10.0 , 10.0)) = 1.0

        [Header(StencilBuffer)]//模板缓冲
		_StencilComp ("Stencil Comparison", Float) = 8.0
        _Stencil ("Stencil ID", Float) = 0.0
        _StencilOp ("Stencil Operation", Float) = 0.0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255.0
        _StencilReadMask ("Stencil Read Mask", Float) = 255.0
        _ColorMask ("Color Mask", Float) = 15.0

        [KeywordEnum(Default , BSC , Blur , EdgeDetect , Outline, Innerline , SelfFlow)]_Pattern("Effect" , Float) = 0.0

        [HideInInspector]
        _UVRect("UVRect" , Vector) = (0,0,1,1)
        [HideInInspector]
        _UVScale("UVScale" , Vector) = (0,0,0,0)


        _BlurThreshold("BlurThreshold" , Range(0.0 , 10.0)) = 0.5

        _Brightness("Brightness", Float) = 1	//调整亮度
		_Saturation("Saturation", Float) = 1	//调整饱和度
		_Contrast("Contrast", Float) = 1		//调整对比度

        _EdgeThreshold("EdgeThreshold" , Range(0.0 , 10.0)) = 0.5
        _EdgeColor("EdgeColor" , Color) = (1.0,1.0,1.0,1.0)

        _OutlineThreshold("OutlineThreshold" , Range(0.0 , 10.0)) = 0.5
        _ShadowOffsetX("ShadowOffsetX" , Range(-0.1 , 0.1)) = 0.0
        _ShadowOffsetY("ShadowOffsetY" , Range(-0.1 , 0.1)) = 0.0
        _OutlineColor("OutlineColor" , Color) = (1.0,1.0,1.0,1.0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref				[_Stencil]
			Comp			[_StencilComp]
			Pass				[_StencilOp]
			ReadMask	    [_StencilReadMask]
			WriteMask	[_StencilWriteMask]
        }

        ColorMask [_ColorMask]
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
       

        Pass
        {
            
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_instancing
            #pragma multi_compile _PATTERN_DEFAULT _PATTERN_BSC _PATTERN_BLUR _PATTERN_EDGEDETECT _PATTERN_OUTLINE _PATTERN_INNERLINE _PATTERN_SELFFLOW

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
            float4 _MainTex_TexelSize;

            float _BlurThreshold;

            float _Brightness;
            float _Saturation;
            float _Contrast;

            float _EdgeThreshold;
            fixed4 _EdgeColor;

            float _OutlineThreshold;
            fixed4 _OutlineColor;
            float _ShadowOffsetX;
            float _ShadowOffsetY;

            float _Speed;

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

            fixed4 frag(v2f IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                //方便偏移计算
                 float2 Box9[9] = 
                    {
                        float2(-1 ,-1),float2(0 ,-1),float2(1 ,-1),
                        float2(-1 ,0),float2(0 ,0),float2(1 ,0),
                        float2(-1 ,1),float2(0 ,1),float2(1 ,1)
                    };

                const float2 BoxReverse9[9] = 
                    {
                        float2(-1 ,-1),float2(0 ,-1),float2(1 ,-1),
                        float2(-1 ,0),float2(0 ,0),float2(1 ,0),
                        float2(-1 ,1),float2(0 ,1),float2(1 ,1)
                    };

                float2 OriUV = IN.texcoord;
                
                //atlas 图片uv归一化
                float4 UVRect = UNITY_ACCESS_INSTANCED_PROP(Props , _UVRect);
                float4 UVScale = UNITY_ACCESS_INSTANCED_PROP(Props , _UVScale);

                float2 sprcenter = (UVRect.zw - UVRect.xy) * 0.5;
                //IN.texcoord = IN.texcoord - UVRect.xy - sprcenter;
                //IN.texcoord *= UVScale;
                //IN.texcoord += sprcenter;
                //IN.texcoord = IN.texcoord / (UVRect.zw - UVRect.xy);
                //----------------------------------------


                float t = frac(_Time.y*_Speed *0.001) * 1000.0;
                  //旋转矩阵
                float2x2 rot = float2x2(cos(t) , -sin(t),
                                                    sin(t) , cos(t));
                    
                    float2 scalor = float2(0.75 , 1.0 + IN.texcoord.y*2.0);
                    OriUV = mul((OriUV  - float2(0.5 , 0.5 ))*scalor , rot);
                    OriUV +=  float2(0.5 , 0.5 );


                fixed4 color = tex2D(_MainTex , OriUV);

                #ifdef _PATTERN_BLUR
                    const fixed GaussianSobel[9] = //高斯模糊权重矩阵
                    {
                        0.0751 , 0.125 , 0.0751,
                        0.1238 , 0.2042, 0.1238,
                        0.0751 , 0.125 , 0.0751
                    };

                    float4 channel = 0.0;
                    float4 blur = float4(0.0 , 0.0 , 0.0 , 0.0);
                    //遍历算卷积
                    for(int num = 0 ; num<9 ; num++)
                    {
                        channel = tex2D(_MainTex , OriUV + Box9[num] * _BlurThreshold * _MainTex_TexelSize.xy);
                        blur += channel * GaussianSobel[num];
                    }
                    color = saturate(blur);
               #endif
                    
               #ifdef _PATTERN_BSC
                    //brigtness亮度直接乘以一个系数，也就是RGB整体缩放，调整亮度
					color.rgb =saturate(_Brightness * color.rgb);
					//saturation饱和度：首先根据公式计算同等亮度情况下饱和度最低的值：
					float gray = 0.29 * color.r + 0.59 * color.g + 0.12 * color.b;
					float3 grayColor = float3(gray, gray, gray);
					//根据Saturation在饱和度最低的图像和原图之间差值
					color.rgb = lerp(grayColor, color.rgb, _Saturation);
					//contrast对比度：首先计算对比度最低的值
					float3 avgColor = float3(0.5, 0.5, 0.5);
					//根据Contrast在对比度最低的图像和原图之间差值
					color.rgb = lerp(avgColor, color.rgb, _Contrast);
               #endif

               //高斯差分算子计算边缘查找
               #ifdef _PATTERN_EDGEDETECT
                    const fixed GaussianSobel1[9] = //高斯权重矩阵1，sigma 1
                    {
                        0.0751 , 0.1238 , 0.0751,
                        0.1238 , 0.2041 , 0.1238,
                        0.0751 , 0.1238 , 0.0751
                    };
                    const fixed GaussianSobel2[9] = //高斯权重矩阵2，sigma 0.6
                    {
                        0.0277 , 0.1110 , 0.0277, 
                        0.1110 , 0.4452 , 0.1110,
                        0.0277 , 0.1110 , 0.0277
                    };


                    float4 channel = 0.0;
                    float DoG1 = 0.0;
                    float DoG2 = 0.0;
                    //遍历算卷积
                    for(int num = 0 ; num<9 ; num++)
                    {
                        channel = Luminance(tex2D(_MainTex , OriUV + Box9[num] * _EdgeThreshold * _MainTex_TexelSize.xy * 4.0));
                        DoG1 += channel * GaussianSobel1[num];
                        DoG2 += channel * GaussianSobel2[num];
                    }
                    channel = (1.0 - saturate(DoG1 - DoG2))*15.0 - 14.0 ;
                    channel = smoothstep(0.8 , 1.0  , channel);
                    color = fixed4(lerp(color.rgb , _EdgeColor.rgb , 1.0 - channel) , color.a * _EdgeColor.a);
               #endif

               #ifdef _PATTERN_OUTLINE

                    float channel = 0.0;
                    
                    float shadow = 0.0;
                    float outline = 0.0;

                    
                    for(int num = 0 ; num<9 ; num++)
                    {
                        float2 off_set = float2(_ShadowOffsetX , _ShadowOffsetY);
                        off_set = mul(off_set , rot);

                        channel = tex2D(_MainTex , OriUV + Box9[num] * _OutlineThreshold * _MainTex_TexelSize.xy).a;
                        shadow = tex2D(_MainTex , OriUV+off_set  + Box9[num] * _OutlineThreshold * _MainTex_TexelSize.xy).a;
                        outline += channel ;
                        outline += shadow ;
                    }
                    float fac = saturate(outline) - color.a;
                    color.rgb = lerp(color.rgb , _OutlineColor.rgb * (IN.texcoord.y * (1.0 - IN.texcoord.x)+0.3), fac);
                    color.a = lerp(color.a , saturate(outline)*_OutlineColor.a , fac);
               #endif

               #ifdef _PATTERN_INNERLINE
                    float channel = 0.0;
                    
                    float shadow = 0.0;
                    float outline = 0.0;
                    for(int num = 0 ; num<9 ; num++)
                    {
                        channel = tex2D(_MainTex , OriUV + Box9[num] * _OutlineThreshold * _MainTex_TexelSize.xy).a;
                        shadow = tex2D(_MainTex , OriUV+float2(_ShadowOffsetX , _ShadowOffsetY) + Box9[num] * _OutlineThreshold * _MainTex_TexelSize.xy).a;
                        outline += channel ;
                        outline += shadow ;
                    }
                    float fac = saturate(outline) - color.a;
                    color.rgb = lerp(color.rgb , _OutlineColor.rgb , fac);
                    color.a = lerp(color.a , saturate(outline)*_OutlineColor.a , fac);
               #endif

               #ifdef _PATTERN_SELFFLOW
                    color = tex2D(_MainTex , IN.texcoord + t);
               #endif
               color *= IN.color;
                return color;

            }
        ENDCG
        }
        
    }
    FallBack "Mobile/Diffuse"
}
