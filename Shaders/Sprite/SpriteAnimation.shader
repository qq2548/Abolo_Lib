Shader "2d/SpriteAnimation"

//我的Unity Shader 标准模板
{
	Properties
	{
		
         _MainTex ("Sprite Texture", 2D) = "white" {}
        //_TexWidth("Sheet Width",float) = 0.0    //贴图宽度像素值
        _HorizaontalCount("HorizaontalCount",float) = 1.0  //总帧数

		//_VerticalPartAmount("VerticalAmount",float) = 3.0  //一共有几列
        _VerticalCount("VerticalCount",float) = 1.0  //当前播放第几列

        _Speed("Speed ",Range(0.0,32)) = 12    //播放速度

		_Color("myColor",Color) = (1.0,1.0,1.0,1.0)
		

		_reverseInt("reverseFac" , Float) = 1.
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

		ZWrite Off
		ZTest LEqual
		Blend One One



		Pass 
		{
			Cull Off
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			//#pragma target 2.5
			#include "UnityCG.cginc" 
			#include "Assets/Abolo_Lib/Shaders/AboloCG.cginc"
			#pragma multi_compile_instancing




			struct appdata
			{
				float4 vertex : POSITION ;
				float2 uv : TEXCOORD0;
				float4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID //gpu instancing 默认的shader带了两个函数接口
				
				
			};

			struct v2f
			{
				
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				fixed4 Color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				//UNITY_VERTEX_OUTPUT_STEREO //gpu instancing 默认的shader带了两个函数接口

				
			};

			

			v2f vert(appdata i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);

				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv = i.uv;
				o.Color = i.Color;
				UNITY_TRANSFER_INSTANCE_ID(i,o);

				return o;
			}

			//float4 _Color;
			float _reverseInt;

			sampler2D _MainTex;            //主贴图
			//float _TexWidth;            //贴图宽度像素值

			float _Speed;                //播放速度
			float  _TimeValue;            //从脚本传递过来的数

			//float _VerticalPartAmount;
			float _HorizaontalCount;        //动画帧数
			float _VerticalCount;
			float _TestFac;

			UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            UNITY_DEFINE_INSTANCED_PROP(float, _TimeOffset)
		    UNITY_INSTANCING_BUFFER_END(Props)

			float4 frag(v2f v) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(v);

				fixed4 tint = UNITY_ACCESS_INSTANCED_PROP(Props , _Color);
				float timeOffset = UNITY_ACCESS_INSTANCED_PROP(Props , _TimeOffset);
				//----------
				float2 fireuv = v.uv;
				//float4 color = tex2D(_MainTex,fireuv) ;
				
				//----------

				float2 spriteUV = fireuv;
				float uAddPerFrame = 1.0 / _HorizaontalCount;         //每一帧U值的增量
				float vAddPerFrame = 1.0/_VerticalCount;         //每一帧V值的增量
				float TimeFac = ABL_FixTime(_Time.y) ;
				//获取一个0 1 2 3 循环的值
				//fmod 返回 x/y 的余数(取模)。如果 y 为 0 ，结果不可预料
				//float timeVal = fmod((TimeFac+0.001) * _Speed,_HorizaontalCount * _VerticalCount);  //进行取余数操作 得到当前要显示的图片的下标
				//timeVal = floor(timeVal);

				//UV横向位移角标计算
				float timeVal_u = fmod((TimeFac+0.001 + (v.Color.r * 100)) * _Speed,_HorizaontalCount);  //进行取余数操作 得到当前要显示的图片的下标
				timeVal_u = floor(timeVal_u);
				//UV纵向位移角标计算
				float timeVal_v = fmod((TimeFac+0.001 + (v.Color.r * 100))* _Speed/_HorizaontalCount,_VerticalCount);  //进行取余数操作 得到当前要显示的图片的下标
				timeVal_v = _VerticalCount - floor(timeVal_v) - 1;

				
				float yValue = fireuv.y;          
				float xValue = fireuv.x;          
				       
				xValue *= uAddPerFrame; 
				yValue *= vAddPerFrame;                    

				xValue += timeVal_u * uAddPerFrame; 
				yValue += timeVal_v * vAddPerFrame; 
				spriteUV = float2(xValue,yValue);
				fixed4 c = tex2D (_MainTex, spriteUV) * tint ;
				
				c.a *= v.Color.a;
				c.rgb *= c.a;
				return c;
			}

			ENDCG
		}

	
		
	}

}
