#ifndef AboloCG
	#define AboloCG
	//防止时间超过浮点数精度上限造成卡顿效果
	float ABL_FixTime(float UnityTime)
	{
		float c;
		c =  frac(UnityTime*0.001)*1000.0 * step(1000 , UnityTime) +UnityTime * step(UnityTime , 1000) ;
		return c;
	}

		//浮点数混合算法
        float mix(float a , float b , float t)
        {
            return b*t + a * (1.0 - t);
        }

		//三维浮点数混合算法
        float3 mix3(float3 a , float3 b ,float t)
        {
            return float3(mix(a.x , b.x ,t) , mix(a.y , b.y ,t) , mix(a.z , b.z ,t));
        }

		//色彩转换
		float3 hsb2rgb(float3 c)
        {
            float3 rgb = saturate(abs(fmod(c.x * 6.0 + float3(0.0 , 4.0 , 2.0) , 6.0) -3.0) - 1.0);
                
            rgb = rgb * rgb * (3.0 - 2.0 * rgb);
            return c.z * mix3(float3(1.0,1.0,1.0) , rgb , c.y);
        }

        //随机算法
        float random(float2 _uv)
        {
            return frac(sin(dot(_uv , float2(12.9898 , 78.233))) * 43758.5758321);
        }


	    //2D噪声
        float noise(float2 _uv)
        {
            float2 i = floor(_uv);
            float2 f = frac(_uv);

            //四向随机值
            float a = random(i);
            float b = random(i + float2(1.0 , 0.0));
            float c = random(i + float2(0.0 , 1.0));
            float d = random(i + float2(1.0 , 1.0));

            // Cubic Hermine Curve.  Same as SmoothStep()
            float2 u = f*f*(3.0-2.0*f); //软化 平滑
            // u = smoothstep(0.,1.,f);

            //百分比混合四角随机噪点
            return mix(a, b, u.x) +(c - a)* u.y * (1.0 - u.x) +(d - b) * u.x * u.y;
        }

	//波纹算法
	float2 ABL_WaveMotion(float4 mainTex_ST , float2 uv , float _WaveFrequency ,  float _WaveSpeed , float _WaveHeight , float TimeFactor )
	{
		float2 uvCoord = mainTex_ST.xy*0.5;
		float2 uvDir = normalize(uv-uvCoord);
		float   uvDis = distance(uv,uvCoord);
		float2 wave_uv = uv+sin(uvDis*_WaveFrequency - TimeFactor *_WaveSpeed)*_WaveHeight*uvDir;
		return wave_uv;
	}

	//通道偏移算法
	float2 ABL_TexChannelUVOffset(float4 SamplerColor , float2 uv , float2  speed ,  float DistortionFactor , float UndistortionFactor , float TimeFactor  , int reverse , float3 channelWeight)
	{
		float texOffset = dot(SamplerColor , channelWeight);
		float2 fireuv;
		fireuv.x = uv.x;
		fireuv.x += cos((uv.y  + TimeFactor * speed.x * reverse + (texOffset *0.5 - 1.0)) * DistortionFactor) / UndistortionFactor;
		fireuv.y = uv.y + TimeFactor * speed.y * 0.75 * reverse;
		return fireuv;
	}


	//深度纹理采样
	float ABL_ScreenSamplerDepth(sampler2D CameraDepth , float4 ScreenPosition)
	{
		
		float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(CameraDepth , ScreenPosition);
		float depth = LinearEyeDepth(depthSample);
		return depth;
	}


	//双通道对象运动混合,用于获得类似光点闪烁效果
	float ABL_DouddleChannelCross(sampler2D _Tex , float2 uv , float TimeFactor , float3 channelWeight)
	{
		float2 suv1 = uv + TimeFactor * 0.01;
		float2 suv2 = uv  - TimeFactor * 0.01 - 0.1; 
		float3 c1 = tex2D(_Tex, suv1).rgb;
		float3 c2 = tex2D(_Tex, suv2).rgb;
		float sparkle1 = dot(c1 , channelWeight);
		float sparkle2 = dot(c2 , channelWeight);
		float sparcle = clamp((pow(sparkle1 * sparkle2 , 2))*2.0 , 0.0 , 1.0);
		return sparcle;
	}

	//RGB通道使用Normal模式混合
	fixed3 ABL_NormalBlendRGB(fixed3 ScrColor , float3 DstColor , float Threshold)
	{
		//float4 BlendColor = DstColor;
        float   blendfac = 1.0 - Threshold ;
		fixed3 c1 = ScrColor;
					c1 *= blendfac;
        fixed3 c2 = DstColor;
					c2 *= 1 - blendfac;
		fixed3 cc = saturate(c1 + c2);
        return cc;
	}

	//绘制内发光
	float ABL_Rim(float viewDir , float3 normal , float rimThreshold)
	{
        float r = 1 -  max(0, dot(normal, viewDir));
        float rim =  saturate(pow(r  , rimThreshold*40.0));
		return rim;
	}

	//色彩纯度运算
	fixed3 ABL_Luminance(fixed3 InColor , float _Saturation)
	{

		float gray = 0.29f * InColor.r + 0.59f * InColor.g + 0.12f * InColor.b;

		return lerp( fixed3(gray, gray, gray), InColor, _Saturation);
	}

	//色彩对比度运算
	fixed3 ABL_ContrastModify(float3 InColor , float _Contrast)
	{

		float3 avgColor = float3(0.5, 0.5, 0.5);

		return lerp(avgColor, InColor, _Contrast);
	}

	//流光效果运算1，带方向翻转参数
	float4 ABL_FlowThroughPattern(fixed3 InColor ,sampler2D _Tex , float2 uv , float speed , float TimeFactor , int reverse)
	{
		float uu = InColor.r ;
		float vv = InColor.g;
		vv -= TimeFactor * speed * reverse + sin( length(uv) + uv.x);
		return tex2D(_Tex,float2(uu, vv)) * InColor.b;
	}
	//流光效果运算2
	float4 ABL_FlowThroughPattern(fixed3 InColor ,sampler2D _Tex , float2 uv , float speed , float TimeFactor )
	{
		float uu = InColor.r ;
		float vv = InColor.g;
		vv -= TimeFactor * speed + sin( length(uv) + uv.x);
		return tex2D(_Tex,float2(uu, vv)) * InColor.b;
	}

	//2d旋转矩阵
	float2x2 ABL_2dRotationMatrix(float speed , float angle)
	{
		return float2x2(
									cos(angle*speed) , -sin(angle*speed),
									sin(angle*speed)  , cos(angle*speed)
																													    );
	}

	//3d沿Y轴旋转矩阵
	float4x4 ABL_3dYaxisRotationMatrix(float speed , float angle)
	{
		//����д��,�����ź�С����
		/*
						float4x4 rot = {cos(angle) , 0 , sin(angle) , 0 ,
											0                 , 1 ,               0 , 0 ,
											-sin(angle) , 0 , cos(angle) , 0 , 
											0                 , 0 ,                0 , 1 };
		*/
		float4x4 rot = float4x4(cos(angle*speed) , 0 , sin(angle*speed) , 0 ,
											0                             , 1 ,                           0 , 0 ,
											-sin(angle*speed) , 0 , cos(angle*speed) , 0 , 
											0                             , 0 ,                           0 , 1 );
		return rot;
	}

	//顶点球面弯曲变形函数
	float4 ABL_WorldBendTransform(float4 vertex_input  , float factor_bend) 
	{
		float4 vertexInfo = mul(unity_ObjectToWorld , vertex_input);
		float3 camDir = _WorldSpaceCameraPos.xyz - vertexInfo.xyz;
		float amount = -factor_bend;
		float fac_x = pow(camDir.x , 2) * amount;
		float fac_y = pow(camDir.z , 2) * amount;
		vertexInfo += float4(0, fac_y + fac_x , 0 , 0);
		//test end
		return UnityObjectToClipPos(mul(unity_WorldToObject , vertexInfo));
	}

	const float ABL_PI = 3.1415926;
	//影像变形波纹
	float ABL_MorphingWave(float x , float t)
	{
		float result = sin(ABL_PI * (x + 0.5 * t));
		result += 0.5 * sin(ABL_PI * 2.0 * (x + t));
		return result;
	}
	//对向波纹动画
	float AnimatedRipple(float x , float t)
	{
		float d = abs(x);
		float result = sin(ABL_PI * (4.0 * d - t));
		return result/(1.0 + 10.0 * d);
	}

	//3d波纹动画
	float TrippleWave(float x , float z , float t)
	{
		float result = sin(ABL_PI * (x + 0.5 * t));
		result += 0.5 * sin(ABL_PI * 2.0 * (z + t));
		result += sin(ABL_PI * (x + z + 0.25 * t));
		return result * (1.0 / 2.5);
	}

	//3d Y轴扩散波纹动画
	float RippleOnXZ(float x , float z , float t)
	{
		float d = sqrt(x * x + z * z);
		float result = sin(ABL_PI * (4.0 * d - t));
		return result / (1.0 + 10.0 * d);
	}

	//
#endif