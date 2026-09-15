// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "sample3d/CustomLightModel_ShadowCross"
{
    Properties
    {
        [Header(My Standard 3D Shader)]//跟自定义编辑器的写法差不多，区别在string名字不带引号,且不能写中文...要报错
        
        _MainTex ("MainTex", 2D) = "white" {}
        //[Toggle]_UseColorMask("UseColorMask" , Float) = 0.0
        //_SideColor("_SideColor" , Color) = (1.0,1.0,1.0,1.0)



        [Header(R is SideLightRamp G is AO  B is Emission )]
        _SubTex("SubTex" , 2D) = "white" {}
        _HatchinTex_1("HatchinTex_1" , 2D) = "white" {}
        _HatchinTex_2("HatchinTex_2" , 2D) = "white" {}
        _HatchinTex_3("HatchinTex_3" , 2D) = "white" {}

        
        _Color ("TintColor", Color) = (1.0,1.0,1.0,1.0)

        [Header(SideColor)]
        _SideColor ("SideColor", Color) = (1.0,1.0,1.0,1.0)

        [Header(LightingMode)]
        _BrightColor("BrightColor" , Color) = (1.0,1.0,1.0,1.0)
        _ShadowColor("ShadowColor" , Color) = (1.0,1.0,1.0,1.0)
        _RampThreshold("RampThreshold", Range(0.001,1.0)) = 0.001
        _RampSmooth("RampSmooth", Range(0.001,1.0)) = 0.001

        [Header(Emission)]
        _Intensity("Indensity" , Range(0.0,3.0)) = 1.0

        [Header(Specular)]
        _SpecularColor ("SpecularColor", Color) = (1.0,1.0,1.0,1.0)
        _SpecularSize("SpecularSize", Float) = 0.0
        _SpecSmooth("SpecularSmooth", Range(0.001,1.0)) = 0.001
        _Gloss("Gloss", Range(0.001,2.0)) = 0.0

        [Header(RimLight)]
		//RIM LIGHT
		_RimColor ("Rim Color", Color) = (0.8,0.8,0.8,0.6)
		_RimMin ("Rim Min", Range(0,2)) = 0.5
		_RimMax ("Rim Max", Range(0,2)) = 1.0

        [Header(SideLight)]
         _SideLightColor ("SideLightColor", Color) = (1.0,1.0,1.0,1.0)
        _SideRampSmooth("SideRampSmooth", Range(0.001,1.0)) = 0.001
        _SideOffset("SideOffset", Range(0.0,1.0)) = 0.0

        [Header(Disolve)]
        _DisolveColor ("DisolveColor", Color) = (1.0,1.0,1.0,1.0)
        _DisolveThreshold("DisolveFactor", Range(0.0,1.0)) = 1.0

        _AmbientThreshold("AmbientThreshold", Range(0.0,1.0)) = 1.0
        _Exposure("Exposure", Range(1.0,5.0)) = 1.0
		//Avoid compile error if the properties are ending with a drawer
		[HideInInspector] __dummy__ ("unused", Float) = 0

        //_VertexCurveViewFac("VertexCurveViewFac",Range(0.0 , 0.1)) = 0.000
        [Toggle] WorldBending("WorldBending", Float) = 0.0  //开关
        _LightDir("LightDir",Vector) = (1.0,1.0,1.0,1.0)


        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0

        _FlashFactor("FlashFactor" , Float) = 0.0
        _FlashColor("FlashColor" , Color) = (1.0,1.0,1.0,1.0)

        [Toggle] SnowFx("SnowFx On", Float) = 1.0  //外描边开关
    }

    SubShader
    {
        Tags 
        {
            "RenderType" = "Opaque"
            "IgnoreProjector"="True"
            "Queue" = "Geometry" 
        }
        
        stencil			//模板缓冲，UI Image的shader如果没写Stencil，图片不会受到UI Mask的遮罩影响，在Content这类局部区域显示的UI上会有问题
		{

			Ref				[_Stencil]
			Comp			[_StencilComp]
			Pass				[_StencilOp]
			
		}

        //Blend SrcAlpha OneMinusSrcAlpha 
        //Cull Back


		Zwrite	On //深度测试，关闭的话不进行深度测试，可以去掉一些不必要的遮挡效果




      Pass 
		{
            Cull off
            //此pass就是 从默认的fallBack中找到的 "LightMode" = "ShadowCaster" 产生阴影的Pass
			Tags { "LightMode" = "ShadowCaster" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            //开启gpu instancing需要把目标平台写到3.0 , 2.0（opengl es2.0）平台不支持gpu instancing，PC上运行不会暴露这个问题
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			//开启gpu批处理(3D绘制测试有效，2D图片无效)
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
            #pragma multi_compile __ _SNOWFX_ON 

			struct v2f {
				V2F_SHADOW_CASTER;
				//UNITY_VERTEX_OUTPUT_STEREO
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert( appdata_base v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                UNITY_TRANSFER_INSTANCE_ID(v,o); 
				return o;
			}

			float4 frag( v2f i ) : SV_Target
			{
                 UNITY_SETUP_INSTANCE_ID(i);
				 SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG

		}




        Pass
        {
            Cull off
            Name "CustomLightingHatching"

            Tags{ "LightMode" = "ForwardBase" }

            //ColorMask RGBA
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            // make fog work
            #pragma multi_compile_fog
             
            //开启gpu批处理
			#pragma multi_compile_instancing
            #pragma multi_compile _ WORLDBENDING_ON
            
            //#pragma  multi_compile __ _USECOLORMASK_ON

            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            #include "Assets/Abolo_Lib/Shaders/AboloCG.cginc"  
            //#include "UnityStandardUtils.cginc" //Lighting.cginc 里面引用了 UnityGlobalIllumination.cginc ,它里面又引用了 UnityStandardUtils.cginc
            



            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float3  normal: NORMAL;
                fixed4 color: COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID //gpu instancing 
            };



            struct v2f
            {
                float2 uv : TEXCOORD0;
                
                
                float4 pos : SV_POSITION;
                float3  normal: NORMAL;  
                float3 WorldPos:TEXCOORD1;
                float4 texcoord : TEXCOORD2;
                float2 texcoord_sub : TEXCOORD3;
                fixed4 color: COLOR;
                //SHADOW_COORDS(2)//仅仅是阴影
                //fog coords
                UNITY_FOG_COORDS(4)

                //V2F_SHADOW_CASTER; //阴影测试
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;


            //half4 _SideColor;
            //half4 _Color;
            fixed4 _SpecularColor;


            sampler2D _SubTex;
            float4 _MainTex_ST;

            sampler2D _HatchinTex_1;
            sampler2D _HatchinTex_2;
            sampler2D _HatchinTex_3;


            fixed4 _ShadowColor;


            fixed4 _BrightColor;
            float _ShadowStrength;
            float _LightStrength;
            fixed4 _DisolveColor;

            fixed4 _RimColor;
            float _RimMin;
            float _RimMax;
            //half _DisolveThreshold;

            fixed3 CalcAmbientColor(fixed3 color, fixed3 worldNormal, fixed3 albedo, fixed3 lightDir)
                        {
  	                        fixed diff = max(0, dot(worldNormal, lightDir));
                            return diff * color * albedo.rgb;
                        }


            UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _SideColor)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _FlashColor)
                UNITY_DEFINE_INSTANCED_PROP(fixed, _DisolveThreshold)
                UNITY_DEFINE_INSTANCED_PROP(fixed, _Intensity)
                UNITY_DEFINE_INSTANCED_PROP(float, _Exposure)
                UNITY_DEFINE_INSTANCED_PROP(float, _FlashFactor)

		    UNITY_INSTANCING_BUFFER_END(Props)

            uniform float _VertexCurveViewFac;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.pos = UnityObjectToClipPos(v.vertex);

           #ifdef WORLDBENDING_ON
				float4 vertexInfo = mul(unity_ObjectToWorld , v.vertex);
				float3 camDir = _WorldSpaceCameraPos.xyz - vertexInfo.xyz;
				float amount = -_VertexCurveViewFac;
				float fac_x = pow(camDir.x , 2) * amount;
				float fac_y = pow(camDir.z , 2) * amount;
				vertexInfo += float4(0, fac_y + fac_x , 0 , 0);
				//test end
				o.pos = UnityObjectToClipPos(mul(unity_WorldToObject , vertexInfo));
			#endif

                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

                o.texcoord.zw = UNITY_PROJ_COORD(ComputeGrabScreenPos(UnityObjectToClipPos(v.vertex))).xy;
				o.texcoord_sub = TRANSFORM_TEX(o.texcoord.zw , _MainTex);

                o.normal = v.normal;  //法线必须做一次世界坐标下的转换 不然明暗效果会有问题，不用视矩计算就是死的 不论怎么旋转调整都不会变化，                                                                                               
                //o.normal = normal;                                                                                                          //用视矩计算就是根据摄像机的位移旋转实时发生变化都不符合常理
                o.WorldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.color = v.color;
                o.texcoord.xy = v.texcoord;
                //Fog 顶点转换
                UNITY_TRANSFER_FOG(o,o.pos);

                //TRANSFER_SHADOW(o);//仅仅是阴影 //xxxx Ori
                UNITY_TRANSFER_INSTANCE_ID(v,o);  
                return o;
            }

            //fixed4 _SideColor;


            float _RampThreshold;
            float _RampSmooth;
            float _SpecularSize;
            float _SpecSmooth;
            float _Gloss;

           fixed4 _SideLightColor;
           float _SideRampSmooth;
           float _SideOffset;
           fixed _AmbientThreshold;
           float4 _LightDir; 


           float _SnowIntens;



            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                const float hatch_fac[3] = {0.1 , 0.5 , 0.9};
                // sample the texture
                fixed4 color = tex2D(_MainTex, i.uv) ;
                const float4 lightDirection = _LightDir;
                //sidecolor added

                //color.a = 1.0;


                color.rgb = color.rgb * UNITY_ACCESS_INSTANCED_PROP(Props, _Color).rgb ;


                fixed3 albedo = color.rgb;
                fixed3 worldNormal = UnityObjectToWorldNormal(i.normal);//tmd 世界法线放在frag里计算转换精度会更高 淦！
                fixed3 lightColor = _BrightColor.rgb;
               //使用局部颜色替换
                    //用主贴图的 alpha 通道采样做剔除遮罩
                    //half sideColorMask = color.a;
                    //color.rgb =  (color.rgb * sideColorMask * UNITY_ACCESS_INSTANCED_PROP(Props, _SideColor)) + (color.rgb * (1.0f - sideColorMask));

                
                //归一化第一盏灯光的光照方向
                fixed3 ligVec = normalize(lightDirection.xyz);//normalize(UnityWorldSpaceLightDir(i.WorldPos));

                //float shadow = (float)SHADOW_ATTENUATION(i);     // 阴影通道计算   根据平台自己得到相应值 //xxxx
               // ligVec *= saturate(shadow); //xxxx
                //视点向量计算
                float3 ViewDir = normalize(UnityWorldSpaceViewDir(i.WorldPos));

                //2维向量保存光照数据，x分量是物体法线与主光源方向的夹角，y分量是物体法线与视线方向的夹角
                float2 ligFac = float2(max(0 , dot(worldNormal , ligVec)) , max(0 , dot(worldNormal , ViewDir)));  
                float3 ramp = smoothstep(_RampThreshold - _RampSmooth*0.5 , _RampThreshold + _RampSmooth*0.5 , ligFac.x); 

                _ShadowColor = lerp(_BrightColor , _ShadowColor , _ShadowColor.a);
                ramp = lerp(_ShadowColor.rgb , _BrightColor.rgb , ramp);
                color.rgb = albedo * ramp * lightColor;

                float range_0 = 0.2;   
                float range_1 = 0.2;   
                float range_2 = 0.1;   
          
                fixed4 hatch_color_0 = tex2D(_HatchinTex_1, i.texcoord_sub);
                float mask_0 = smoothstep(ramp.x-range_0 , ramp.x + range_0 ,hatch_fac[0]);

                fixed4 hatch_color_1 = tex2D(_HatchinTex_2, i.texcoord_sub);
                float mask_1 = smoothstep(ramp.x-range_1 , ramp.x + range_1 ,hatch_fac[1]);

                fixed4 hatch_color_2 = tex2D(_HatchinTex_3, i.texcoord_sub);
                float mask_2 = smoothstep(ramp.x-range_2 , ramp.x + range_2 ,hatch_fac[2]);

                color.rgb = lerp(hatch_color_0 , 1.0 , 1.0 - mask_0)
                            *lerp(hatch_color_1 , 1.0 , 1.0 - mask_1)
                            *lerp(hatch_color_2 , 1.0 , 1.0 - mask_2)
                            *albedo;
                //clip(color.a - 0.01);
               
                return color;
            }
            ENDCG
        }

    }
    //FallBack "Mobile/Diffuse"  
}
