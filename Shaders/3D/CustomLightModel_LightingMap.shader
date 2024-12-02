// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

// Upgrade NOTE: commented out 'sampler2D unity_Lightmap', a built-in variable

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "sample3d/CustomLightModel_LightingMap"
{
    Properties
    {
        [Header(My Standard 3D Shader)]//跟自定义编辑器的写法差不多，区别在string名字不带引号,且不能写中文...要报错
        
        _MainTex ("MainTex", 2D) = "white" {}
        //[Toggle]_UseColorMask("UseColorMask" , Float) = 0.0
        //_SideColor("_SideColor" , Color) = (1.0,1.0,1.0,1.0)

        [Header(R is SideLightRamp G is Disolve Sample B is Emission Sample)]
        _SubTex("SubTex" , 2D) = "black" {}
        
        _Color ("TintColor", Color) = (1.0,1.0,1.0,1.0)

        [Header(LightingMode)]
        _BrightColor("BrightColor" , Color) = (1.0,1.0,1.0,1.0)
        _ShadowColor("ShadowColor" , Color) = (1.0,1.0,1.0,1.0)
        _RampThreshold("RampThreshold", Range(0.001,1.0)) = 0.001
        _RampSmooth("RampSmooth", Range(0.001,1.0)) = 0.001

        [Header(Emission)]
        _Intensity("Indensity" , Range(1.0,3.0)) = 1.0

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
		//Avoid compile error if the properties are ending with a drawer
		[HideInInspector] __dummy__ ("unused", Float) = 0
    }

    SubShader
    {
        Tags 
        {
            "RenderType" = "Opaque"
            "IgnoreProjector"="True"
            "Queue" = "Geometry" 
        }
        
        
        Cull Back


		Zwrite	On //深度测试，关闭的话不进行深度测试，可以去掉一些不必要的遮挡效果




      Pass 
		{
            Cull Back
            //此pass就是 从默认的fallBack中找到的 "LightMode" = "ShadowCaster" 产生阴影的Pass
			Tags { "LightMode" = "ShadowCaster" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			//开启gpu批处理(3D绘制测试有效，2D图片无效)
			#pragma multi_compile_instancing
			#include "UnityCG.cginc" 


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
            Name "CustomLighting"

            Tags{ "LightMode" = "ForwardBase" }


            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            // make fog work
            #pragma multi_compile_fog

            //开启gpu批处理
			#pragma multi_compile_instancing

            
            //#pragma  multi_compile __ _USECOLORMASK_ON

            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            // 非常重要，不然没法正确计算阴影
            #include "AutoLight.cginc" 
            #include "Assets/Abolo_Lib/Shaders/AboloCG.cginc"  
            //#include "UnityStandardUtils.cginc" //Lighting.cginc 里面引用了 UnityGlobalIllumination.cginc ,它里面又引用了 UnityStandardUtils.cginc
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;// 第二纹理 用于光照贴图
                float3  normal: NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID //gpu instancing  
            };



            struct v2f
            {
                float2 uv : TEXCOORD0;
                
                
                float4 pos : SV_POSITION;
                float3  normal: NORMAL;  
                float3 WorldPos:TEXCOORD1;

                SHADOW_COORDS(2)//仅仅是阴影
                //fog coords
                UNITY_FOG_COORDS(3)
                float2 lightingMap_uv : TEXCOORD4;
                //V2F_SHADOW_CASTER; //阴影测试
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            // sampler2D unity_Lightmap ;

            //half4 _SideColor;
            //half4 _Color;
            half4 _SpecularColor;


            sampler2D _SubTex;
            half4 _MainTex_ST;
            half4 unity_Lightmap_ST;

            half4 _ShadowColor;


            half4 _BrightColor;
            half _ShadowStrength;
            half _LightStrength;
            half4 _DisolveColor;

            half4 _RimColor;
            half _RimMin;
            half _RimMax;
            //half _DisolveThreshold;



            UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
                UNITY_DEFINE_INSTANCED_PROP(half4, _SideColor)
                UNITY_DEFINE_INSTANCED_PROP(half4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(half, _DisolveThreshold)
                UNITY_DEFINE_INSTANCED_PROP(half, _Intensity)

		    UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.normal = v.normal;  //法线必须做一次世界坐标下的转换 不然明暗效果会有问题，不用视矩计算就是死的 不论怎么旋转调整都不会变化，                                                                                               
                //o.normal = normal;                                                                                                          //用视矩计算就是根据摄像机的位移旋转实时发生变化都不符合常理
                o.WorldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                //Fog 顶点转换
                UNITY_TRANSFER_FOG(o,o.pos);

                o.lightingMap_uv = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;

                TRANSFER_SHADOW(o);//仅仅是阴影 //xxxx Ori
                UNITY_TRANSFER_INSTANCE_ID(v,o);  
                return o;
            }

            half _RampThreshold;
            half _RampSmooth;
            half _SpecularSize;
            half _SpecSmooth;
            half _Gloss;

           half4 _SideLightColor;
           half _SideRampSmooth;
           half _SideOffset;

            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                // sample the texture
                half4 color = tex2D(_MainTex, i.uv) ;

                half3 albedo = color.rgb;
                half3 worldNormal = UnityObjectToWorldNormal(i.normal);//tmd 世界法线放在frag里计算转换精度会更高 淦！
                half3 lightColor = _LightColor0.rgb;
               //使用局部颜色替换
                    //用主贴图的 alpha 通道采样做剔除遮罩
                    //half sideColorMask = color.a;
                    //color.rgb =  (color.rgb * sideColorMask * UNITY_ACCESS_INSTANCED_PROP(Props, _SideColor)) + (color.rgb * (1.0f - sideColorMask));

                
                //归一化第一盏灯光的光照方向  
                float3 ligVec = normalize(UnityWorldSpaceLightDir(i.WorldPos));
                half shadow = (half)SHADOW_ATTENUATION(i);     // 阴影通道计算   根据平台自己得到相应值 //xxxx
                ligVec *= saturate(shadow); //xxxx
                //视点向量计算
                float3 ViewDir = normalize(UnityWorldSpaceViewDir(i.WorldPos));

                //2维向量保存光照数据，x分量是物体法线与主光源方向的夹角，y分量是物体法线与视线方向的夹角
                half2 ligFac = half2(max(0 , dot(worldNormal , ligVec)) , max(0 , dot(worldNormal , ViewDir)));  
                half3 ramp = smoothstep(_RampThreshold - _RampSmooth*0.5 , _RampThreshold + _RampSmooth*0.5 , ligFac.x); 

                _ShadowColor = lerp(_BrightColor , _ShadowColor , _ShadowColor.a);
                ramp = lerp(_ShadowColor.rgb , _BrightColor.rgb , ramp);
                color.rgb = albedo * ramp * lightColor;

                //计算高光
                float3 h = normalize (ligVec + ViewDir);
                float ndh = max (0, dot (worldNormal, h));
                float spec = pow(ndh, _SpecularSize*128.0)  * _Gloss;
                spec = smoothstep(0.5-_SpecSmooth*0.5, 0.5+_SpecSmooth*0.5, spec);
                spec *= ligFac.x;//消除暗部高光
                color.rgb += spec * lightColor * _SpecularColor.rgb;

                // 计算漫反射
                 // ambient 环境光

                 fixed3 upMask = max(worldNormal.y , 0.0);
                 fixed3 downMask = max(-worldNormal.y , 0.0);
                 fixed3 midMask = 1.0 - upMask - downMask;
                 fixed3 EnvLightCol = upMask * unity_AmbientSky.xyz + downMask * unity_AmbientGround.xyz + midMask * unity_AmbientEquator.xyz;
                 fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
                //兰伯特漫反射公式
                fixed3 diffuse = lightColor * albedo* (saturate (dot(worldNormal,ligVec))) + ambient;
                #if UNITY_SHOULD_SAMPLE_SH
                     diffuse = ShadeSHPerPixel (worldNormal, EnvLightCol, i.WorldPos);
                #endif
                color.rgb += albedo  * diffuse * 0.8;//这里当全局光照数据里 有三个环境光颜色时，漫反射计算的结果，效果有问题 ，用经验算法搞定

                //边缘光计算
                half rim = 1.0 - saturate( dot(ViewDir, worldNormal) );
			    rim = smoothstep(_RimMin, _RimMax, rim);
			    color.rgb += (_RimColor.rgb * rim) * _RimColor.a;

                //侧光计算
                //half sideLight = 1.0 - smoothstep(0.5-_SideRampSmooth*0.5, 0.5+_SideRampSmooth*0.5, ligFac.y -(-0.5 + _SideOffset));
                //color.rgb += sideLight;
                ligFac.y *= 1.0 - _SideOffset;
                half4 SCol = tex2D(_SubTex ,  ligFac );
                color.rgb += SCol.r * _SideLightColor;

                half3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap,i.lightingMap_uv)) * 2.0;
                color.rgb *= lm;
                //副纹理 主uv采样，用作后续效果的计算
                fixed4 subColor = tex2D(_SubTex , i.uv);


                //自发光计算
                half emission = subColor.b;
                color.rgb *= emission * UNITY_ACCESS_INSTANCED_PROP(Props, _Intensity) + 1.0;
               
               color.rgb *= UNITY_ACCESS_INSTANCED_PROP(Props, _Color).rgb;
               color.a *= UNITY_ACCESS_INSTANCED_PROP(Props, _Color).a;

               //消融计算 ,模型的渲染必须处在半透明的渲染序列
                                        half disolveMask = subColor.g;
                                        half dFac = UNITY_ACCESS_INSTANCED_PROP(Props, _DisolveThreshold);
				                        half DisolveFac =  saturate(step(disolveMask , dFac));

                                        half d =  step( disolveMask  + 0.01f, dFac) * step(dFac - 0.03f , disolveMask);
				                        color.rgb += _DisolveColor.rgb *d;
                                        color.a *= DisolveFac;



                //远景雾化
    //            half blendFac = saturate((1 - i.Zcoord *5.2 ) * 5);
				//	   blendFac *= blendFac; //pow(blendFac , 2);
    //            half3 c1 = color.rgb;
				//		 c1 *= (1 - blendFac * 0.9);
    //            half3 c2 = half3(0.1 , 0.45 , 0.75);
				//		 c2 *= blendFac;
				//half3 cc = saturate(c1 + c2);
    //            color.rgb = cc;
                //远景雾化
                

    //            half   blendfac = BlendColor.a ;
				//half3 c1 = color.rgb;
				//		 c1 *= blendfac;
    //            half3 c2 = BlendColor.rgb;
				//		 c2 *= 1.0f - blendfac;
				//half3 cc = saturate(c1 + c2);


                

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, color);
                return color;
            }
            ENDCG 
        }



             

  //      Pass //遮挡透视渲染剪影pass 
		//{
		//	ZWrite Off
		//	ZTest Always
		//	Stencil {
		//					Ref 4
		//					Comp NotEqual
		//					Fail Keep
		//					//Pass Replace
		//				}
		
		//	//Cull Off
		//	CGPROGRAM
		//	#pragma vertex vert
		//	#pragma fragment frag
			
		//	#include "UnityCG.cginc"
			
		//	struct appdata
		//	{
		//		float4 vertex : POSITION;
		//	};

		//	struct v2f
		//	{
		//		float4 vertex : SV_POSITION;
		//	};

		//	v2f vert (appdata v)
		//	{
		//		v2f o;
		//		o.vertex = UnityObjectToClipPos(v.vertex);
		//		return o;
		//	}
			
			
			
		//	half4 frag (v2f i) : SV_Target
		//	{
		//		return half4(0, .7, 1,1);
		//	}
		//	ENDCG
		//}
    }
    //FallBack "Mobile/Diffuse"  
}
