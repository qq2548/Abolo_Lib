 Shader "2d/Sprite_RingWave_Patterns"

//我的Unity Shader 标准模板
{
	Properties
	{
		
        _MainTex("Base (RGB)",2D) = "white" {}  //贴图
        _SubTex("Sub (RGB)",2D) = "white" {}  //贴图
		_RingCount("RingCount" , int) = 3
        //_TexWidth("Sheet Width",float) = 0.0    //贴图宽度像素值
		_Color ("Tint", Color) = (1,1,1,1)
		_OuterColor ("OuterColor", Color) = (1,1,1,1)
		_InnerColor ("InnerColor", Color) = (1,1,1,1)
		_EgdeHardness("EgdeHardness",Range(0.0 , 1.0)) = 0.5
		_Thickness("Thickness",Range(0.0 , 10.0)) = 0.5
		_NoiseWeight("NoiseWeight",Range(0.0 , 1.0)) = 0.5
		_Speed("Speed",Range(-1.0 , 10.0)) = 1.0  
		
	}
	SubShader 
	{
		Tags
		{
				"RenderType" = "Opaque" //TransparentCutout
				 "Queue" = "Transparent"
				 "IgnoreProjector"="True"
		}
															
		Cull Off
        ZWrite Off
        ZTest LEqual

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
			#define TWO_PI 6.28318530718

			////随机算法
            // float random(float2 _uv)
            //{
            //    return frac(sin(dot(_uv , float2(12.9898 , 78.233))) * 43758.5758321 );
            //}

			////2D噪声
            //float noise(float2 _uv)
            //{
            //    float2 i = floor(_uv);
            //    float2 f = frac(_uv);

            //    //四向随机值
            //    float a = random(i);
            //    float b = random(i + float2(1.0 , 0.0));
            //    float c = random(i + float2(0.0 , 1.0));
            //    float d = random(i + float2(1.0 , 1.0));

            //    // Cubic Hermine Curve.  Same as SmoothStep()
            //    float2 u = f*f*(3.0-2.0*f); //软化 平滑
            //    // u = smoothstep(0.,1.,f);

            //    //百分比混合四角随机噪点
            //    return lerp(a, b, u.x) +(c - a)* u.y * (1.0 - u.x) +(d - b) * u.x * u.y;
            //}


			float _EgdeHardness;

			float drawCircle(float2 st , float range , float thickness , float frequency , float time)
			{
			    thickness *= 1.0 - time;
				return (1.0 - smoothstep(range  * time, range * time  + _EgdeHardness, length(st) + noise(st)* frequency ))   
							* smoothstep(range * time - thickness, range  * time - thickness + _EgdeHardness, length(st) + noise(st) * frequency);
			}

			float2x2 rot(float angle)
			{
				return float2x2(  cos(angle) , -sin(angle),
										sin(angle),    cos(angle)	);
			}



			struct appdata
			{
				float4 vertex : POSITION ;
				float3 uv : TEXCOORD0;
				fixed4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID //gpu instancing 默认的shader带了两个函数接口
				
				
			};

			struct v2f
			{
				
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 sub_uv : TEXCOORD1;
				fixed4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				//UNITY_VERTEX_OUTPUT_STEREO //gpu instancing 默认的shader带了两个函数接口
			};

			
			sampler2D _MainTex;            //主贴图
			float4 _MainTex_ST;

			sampler2D _SubTex;            //副贴图
			float4 _SubTex_ST;

			v2f vert(appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);

				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv.xy = TRANSFORM_TEX(i.uv.xy , _MainTex);
				//o.uv.z = i.uv.z;
				o.sub_uv = TRANSFORM_TEX(i.uv.xy , _SubTex);
				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);

				return o;
			}
			
			float _RingCount;
			float _Thickness;
			float _Speed;
			float _NoiseWeight;
			fixed4 _OuterColor;
			fixed4 _InnerColor;

			UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
		    UNITY_INSTANCING_BUFFER_END(Props)

			fixed4 frag(v2f v) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(v);
				fixed4 tint = UNITY_ACCESS_INSTANCED_PROP(Props , _Color);
				float2 center = v.uv.xy - float2(0.5 , 0.5);
				float2 st = center * 10.0;
				float mask = 1.0 - smoothstep(2.0, 5.0 , length(st));
				float t = 0;
				float timeFac = ABL_FixTime(_Time.y);
				float gap = 1.0/_RingCount;
				float col = 0.0;
				float time  = frac(_Time.y * 0.001)*1000.0;
				float2 coord = mul(rot(time) , st);
				for(int num = 0 ; num < _RingCount ; num++)
				{
					t = 1.0 - frac((1.0 - timeFac)*_Speed + gap * num);
					col +=drawCircle(coord , 4.6 , _Thickness , _NoiseWeight , t );
				}

				fixed4 color = tex2D(_MainTex,v.uv.xy)  * v.Color;
				fixed4 sub_color = tex2D(_SubTex,v.sub_uv);

				fixed4 gradienColor = lerp(_InnerColor ,_OuterColor ,length(st) * 0.2);
				color = fixed4(col,col,col,1.0) * tint * v.Color; 
				color.rgb += gradienColor.rgb;
				color.rgb = saturate(color.rgb);

				float angle = atan2(center.y , center.x);
				float radius = length(center)*2.0;
				float3 tint_col = hsb2rgb(float3((angle/TWO_PI)+0.5,radius,1.0));
				color.rgb *= tint_col;

				color.a *= mask * sub_color.b;
				color.rgb *= color.a;
				return color;
			}

			ENDCG
		}

	
		
	}

}
