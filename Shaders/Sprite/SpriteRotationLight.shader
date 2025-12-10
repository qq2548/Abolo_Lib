Shader "2d/SpriteRotationLight"
{
     Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _SubTex ("SubTexture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _subColor("RotColor" , Color) = (1,1,1,1)
        _LightScalar("LightScalar" , Range(0.001 , 1.0)) = 0.8


        _FacA("Speed" , Float) = 0.0
		[Toggle] WorldBending("WorldBending", Float) = 0.0  //开关
        //[HideInInspector]
        //_UVRect("UVRect" , Vector) = (0,0,1,1)
        //[HideInInspector]
        //_UVScale("UVScale" , Vector) = (0,0,0,0)
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



	
		Cull Off
		Lighting Off
		ZWrite Off


		Blend One One
       

        Pass
        {
            
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "Assets/Abolo_Lib/Shaders/AboloCG.cginc"  
            #pragma multi_compile_instancing
            #pragma multi_compile _ WORLDBENDING_ON	             
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
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
            sampler2D _SubTex;
            fixed4 _subColor;
            float _FacA;
            float _LightScalar;

            UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            UNITY_DEFINE_INSTANCED_PROP(float4, _UVRect)
            UNITY_DEFINE_INSTANCED_PROP(float4, _UVScale)

		    UNITY_INSTANCING_BUFFER_END(Props)
  
            uniform float _VertexCurveViewFac;
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
            #ifdef WORLDBENDING_ON
				OUT.vertex = ABL_WorldBendTransform(v.vertex , _VertexCurveViewFac);
			#endif                
                OUT.texcoord = TRANSFORM_TEX(v.texcoord , _MainTex );
                OUT.color = v.color * UNITY_ACCESS_INSTANCED_PROP(Props , _Color);
                UNITY_TRANSFER_INSTANCE_ID(v,OUT);
                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                float2 OriUV = IN.texcoord;
                //atlas 图片uv归一化
                //float4 UVRect = UNITY_ACCESS_INSTANCED_PROP(Props , _UVRect);
                //float4 UVScale = UNITY_ACCESS_INSTANCED_PROP(Props , _UVScale);

                //float2 sprcenter = (UVRect.zw - UVRect.xy) * 0.5;
                //IN.texcoord = IN.texcoord - UVRect.xy - sprcenter;
                //IN.texcoord *= UVScale;
                //IN.texcoord += sprcenter;
                //IN.texcoord = IN.texcoord / (UVRect.zw - UVRect.xy);
                //----------------------------------------

                float t = frac(_Time.x/100.0)*100.0 * _FacA;

                //旋转矩阵
                float2x2 rot = float2x2(cos(_Time.y * _FacA) , -sin(_Time.y * _FacA),
                                                    sin(_Time.y * _FacA) , cos(_Time.y * _FacA));

                float2 uv = IN.texcoord;
                float2 center = uv - 0.5;

                //矩阵相乘旋转
                float2 st = mul(rot,center)* _LightScalar;

                fixed4 subColor = tex2D(_SubTex , st + 0.5);


                center = center  + 0.5;
                fixed4 bg = float4(0.0,0.0,0.0,1.0);
                fixed4 color = tex2D(_MainTex , OriUV) * IN.color;
                color.rgb *= color.a;
                return fixed4(subColor.rgb*_subColor.rgb*color.a , 1.0) * IN.color;
                //return subColor;
            }
        ENDCG
        }
    }
}
