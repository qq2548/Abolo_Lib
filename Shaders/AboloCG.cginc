#ifndef AboloCG
	#define AboloCG
	//�ض�����ʱ�����������ֵ���ڹ����¾��Ȳ���������ʾ���⣬�׶��ǻ���ֶ����νӶ���
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

		
        float3 mix3(float3 a , float3 b ,float t)
        {
            return float3(mix(a.x , b.x ,t) , mix(a.y , b.y ,t) , mix(a.z , b.z ,t));
        }

		
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

	//���� uv ƫ�Ƶļ��� ���Ƽ�г�˶� ,͵�������в���������һ���� ������ϸ��©��
	float2 ABL_WaveMotion(float4 mainTex_ST , float2 uv , float _WaveFrequency ,  float _WaveSpeed , float _WaveHeight , float TimeFactor )
	{
		float2 uvCoord = mainTex_ST.xy*0.5;
		float2 uvDir = normalize(uv-uvCoord);
		float   uvDis = distance(uv,uvCoord);
		float2 wave_uv = uv+sin(uvDis*_WaveFrequency - TimeFactor *_WaveSpeed)*_WaveHeight*uvDir;
		return wave_uv;
	}

	//ʹ������ ͨ������ʵ��uv��ƫ��
	//��һ����ά���� channelWeight ��ΪȨ������������һ��ͨ��
	float2 ABL_TexChannelUVOffset(float4 SamplerColor , float2 uv , float2  speed ,  float DistortionFactor , float UndistortionFactor , float TimeFactor  , int reverse , float3 channelWeight)
	{
		float texOffset = dot(SamplerColor , channelWeight);
		float2 fireuv;
		fireuv.x = uv.x;
		fireuv.x += cos((uv.y  + TimeFactor * speed.x * reverse + (texOffset *0.5 - 1.0)) * DistortionFactor) / UndistortionFactor;
		fireuv.y = uv.y + TimeFactor * speed.y * 0.75 * reverse;
		return fireuv;
	}


	//������Ӱ����򿪣������޷���ȡ������������ͼ�����Ч���Ͳ���
	//��ĻͶӰ������Ȳ���
	float ABL_ScreenSamplerDepth(sampler2D CameraDepth , float4 ScreenPosition)
	{
		//SAMPLE_DEPTH_TEXTURE_PROJ ��ͬ��������������������������һ��float3��float4���͵��������꣬
		//�����ڲ�ʹ����tex2Dproj�����ĺ�������ͶӰ������������������ǰ�����������Ȼ�������һ��������
		//�ٽ����������������ṩ�˵��ĸ��������������һ�αȽϣ� ͨ��������Ӱ��ʵ���С�
		float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(CameraDepth , ScreenPosition);
		//Unity�ṩ����������������Ϊ���ǽ��������ļ�����̡���LinearEyeDepth �� Linear01Depth��
		//LinearEyeDepth ������������Ĳ������ת�����ӽǿռ��µ����ֵ��
		//�� Linear01Depth ��᷵��һ����Χ��[0, 1]���������ֵ��
		//�����������ڲ�ʹ�������õ�_ZBufferParams�������õ�Զ���ü�ƽ��ľ���
		float depth = LinearEyeDepth(depthSample);
		return depth;
	}


	//ʹ�������RGBͨ�� �����뷴�����ƶ������ٽ�����˵õ����ƹ����˸�Ĳ������
	//��һ����ά������ΪȨ������������һ��ͨ��
	//Ҫ��Ҫ������ tex2D �������������д�����
	float ABL_DouddleChannelCross(sampler2D _Tex , float2 uv , float TimeFactor , float3 channelWeight)
	{
		float2 suv1 = uv + TimeFactor * 0.01;
		float2 suv2 = uv  - TimeFactor * 0.01 - 0.5; 
		float3 c1 = tex2D(_Tex, suv1).rgb;
		float3 c2 = tex2D(_Tex, suv2).rgb;
		float sparkle1 = dot(c1 , channelWeight);
		float sparkle2 = dot(c2 , channelWeight);
		float sparcle = clamp((pow(sparkle1 * sparkle2 , 2))*2.0 , 0.0 , 2.0);
		return sparcle;
	}

	//��͸ɫ������ģʽ�µĻ�ϼ���
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

	//�ڷ������
	float ABL_Rim(float viewDir , float3 normal , float rimThreshold)
	{
		//�ڷ�����ɫ����
        float r = 1 -  max(0, dot(normal, viewDir));
        float rim =  saturate(pow(r  , rimThreshold*40.0));
		return rim;
	}

	//�ҽ׼���
	fixed3 ABL_Luminance(fixed3 InColor , float _Saturation)
	{
		//saturation���Ͷȣ����ȸ��ݹ�ʽ����ͬ����������±��Ͷ���͵�ֵ��
		float gray = 0.29f * InColor.r + 0.59f * InColor.g + 0.12f * InColor.b;
		//����Saturation�ڱ��Ͷ���͵�ͼ���ԭͼ֮���ֵ
		return lerp( fixed3(gray, gray, gray), InColor, _Saturation);
	}

	//�Աȶȼ���
	fixed3 ABL_ContrastModify(float3 InColor , float _Contrast)
	{
		//contrast�Աȶȣ����ȼ���Աȶ���͵�ֵ
		float3 avgColor = float3(0.5, 0.5, 0.5);
		//����Contrast�ڶԱȶ���͵�ͼ���ԭͼ֮���ֵ
		return lerp(avgColor, InColor, _Contrast);
	}

	//��������ͼ��������Ч��
	float4 ABL_FlowThroughPattern(fixed3 InColor ,sampler2D _Tex , float2 uv , float speed , float TimeFactor , int reverse)
	{
		float uu = InColor.r ;
		float vv = InColor.g;
		vv -= TimeFactor * speed * reverse + sin( length(uv) + uv.x);
		return tex2D(_Tex,float2(uu, vv)) * InColor.b;
	}
	//������д
	float4 ABL_FlowThroughPattern(fixed3 InColor ,sampler2D _Tex , float2 uv , float speed , float TimeFactor )
	{
		float uu = InColor.r ;
		float vv = InColor.g;
		vv -= TimeFactor * speed + sin( length(uv) + uv.x);
		return tex2D(_Tex,float2(uu, vv)) * InColor.b;
	}

	//2d��ת����
	float2x2 ABL_2dRotationMatrix(float speed , float angle)
	{
		return float2x2(
									cos(angle*speed) , -sin(angle*speed),
									sin(angle*speed)  , cos(angle*speed)
																													    );
	}

	//3d Y����ת���������ٶȰ� �����ʵҲ�ǰٶȵ�
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

	//
#endif