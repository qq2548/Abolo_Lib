Shader "2d/BlurEffect_3X3"
{
    Properties
    {
        [PerRendererData]_MainTex ("MainTex", 2D) = "white" {}

        _Range("BlurRange" , Range(0.0 , 10.0)) = 0.0
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
		Cull Off
		Lighting Off
		ZWrite Off 
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
		
        //GrabPass{"_GrabTexture"}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc" 
            #include "UnityUI.cginc" 

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 Color:COLOR;
                //UNITY_VERTEX_INPUT_INSTANCE_ID //gpu instancing 默认的shader带了两个函数接口
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 worldPos : TEXCOORD1;

                fixed4 Color:COLOR;
                float4 vertex : SV_POSITION;
                //UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;




            v2f vert (appdata v)
            {
                v2f o;
                //UNITY_SETUP_INSTANCE_ID(v);
    
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                o.Color = v.Color;
                //UNITY_TRANSFER_INSTANCE_ID(v,o);
                return o;
            }

            float _Range;

            fixed4 frag (v2f input) : SV_Target
            {
                //方便偏移计算
                const fixed4 Box9[9] = 
                    {
                        fixed4(-1 ,-1 ,0,0),fixed4(0 ,-1,0,0),fixed4(1 ,-1,0,0),
                        fixed4(-1 ,0,0,0),fixed4(0 ,0,0,0),fixed4(1 ,0,0,0),
                        fixed4(-1 ,1,0,0),fixed4(0 ,1,0,0),fixed4(1 ,1,0,0)
                    };

                const fixed GaussianSobel[9] = //高斯模糊权重矩阵
                {
                    0.0751 , 0.125 , 0.0751,
                    0.1238 , 0.2042, 0.1238,
                    0.0751 , 0.125 , 0.0751
                };


                //const fixed4 Box25[25] = 
                //    {
                //        fixed4(-2 ,2 ,0,0) , fixed4(-1 ,2,0,0) , fixed4(0 ,2,0,0) , fixed4(1 ,2,0,0) , fixed4(2 ,2,0,0) ,
                //        fixed4(-2 ,1 ,0,0) , fixed4(-1 ,1,0,0) , fixed4(0 ,1,0,0) , fixed4(1 ,1,0,0) , fixed4(2 ,1,0,0) ,
                //        fixed4(-2 ,0 ,0,0) , fixed4(-1 ,0,0,0) , fixed4(0 ,0,0,0) , fixed4(1 ,0,0,0) , fixed4(2 ,0,0,0) ,
                //        fixed4(-2 ,-1 ,0,0) , fixed4(-1 ,-1,0,0) , fixed4(0 ,-1,0,0) , fixed4(1 ,-1,0,0) , fixed4(2 ,-1,0,0) ,
                //        fixed4(-2 ,-2 ,0,0) , fixed4(-1 ,-2,0,0) , fixed4(0 ,-2,0,0) , fixed4(1 ,-2,0,0) , fixed4(2 ,-2,0,0) 
                //    };


                //const fixed GaussianSobel25[25] = //高斯模糊权重矩阵
                //    {
                //        0.0125 , 0.0251 , 0.0314 , 0.0251 , 0.0125 ,
                //        0.0251 , 0.0566 , 0.0754 , 0.0566 , 0.0251 ,
                //        0.0314 , 0.0754 , 0.0943 , 0.0754 , 0.0314,
                //        0.0251 , 0.0566 , 0.0754 , 0.0566 , 0.0251 ,
                //        0.0125 , 0.0251 , 0.0314 , 0.0251 , 0.0125 
                //    };

                  //  2   4   5   4   2  

                  //4   9   12   9   4  

                  //5   12   15   12   5  

                  //4   9   12   9   4  

                  //2   4   5   4   2  


                
                #if UNITY_UV_STARTS_AT_TOP
                
                #else
               
                #endif

  
                fixed4 color = 0.0;

                float4 channel =0.0;
                float4 channel2 =0.0;
                //float4 channel3 =0.0;
                //float4 channel4 =0.0;
                //float4 channel5 =0.0;

                float4 blur =0.0;
                //float aspectradius = _GrabTexture_TexelSize.z/_GrabTexture_TexelSize.w;
                float scalar = _MainTex_TexelSize.z * 0.001;
                float4 rect =  float4(_MainTex_TexelSize.xy , 1.0 ,1.0);
                //float4 factor = {_Range  , _Range * aspectradius, 1.0 ,1.0};
                //遍历算卷积，9 次采样，性能表现有待观察
                for(int num = 0 ; num<9 ; num++)
                {
                    channel = tex2D(_MainTex, input.uv + Box9[num] * _Range * scalar * rect) ;
                    //channel2 = tex2Dproj(_GrabTexture, input.grabPos + Box25[num] * _Range * 0.5 * scalar * float4(_GrabTexture_TexelSize.xy , 1.0 ,1.0)) ;

                    blur += channel * GaussianSobel[num];// + channel2 * GaussianSobel25[num] ;
                }

                color = saturate(blur) * input.Color; 

                color.a = 1.0;
                return color;
            }
            ENDCG
        }
    }
}
