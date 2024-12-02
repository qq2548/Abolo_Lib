Shader "2d/Image_Distortion"
{
     Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
         _SubTex ("SubTexture", 2D) = "white" {}

        [Header(StencilBuffer)]//模板缓冲
		_StencilComp ("Stencil Comparison", Float) = 8.0
        _Stencil ("Stencil ID", Float) = 0.0
        _StencilOp ("Stencil Operation", Float) = 0.0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255.0
        _StencilReadMask ("Stencil Read Mask", Float) = 255.0
        _ColorMask ("Color Mask", Float) = 15.0

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
			ReadMask	[_StencilReadMask]
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


            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #include "Assets/Abolo_Lib/Shaders/AboloCG.cginc" 



            #define TWO_PI 6.28318530718


            struct appdata_t
            {
                float4 vertex   : POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;

            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;

            };

            sampler2D _MainTex;
            float4 _MainTex_ST;


            sampler2D _SubTex;
  

            v2f vert(appdata_t v)
            {
                v2f OUT;

                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord , _MainTex );
                OUT.color = v.color;

                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
            	//时间变量
				float TimeFac = ABL_FixTime(_Time.x * 0.5);
                float2 coord = IN.texcoord;
                fixed4 subColor = tex2D(_SubTex , IN.texcoord*0.5 + TimeFac) ;
                coord +=( subColor.rg * 0.05 - 0.05) * (1.2 - IN.texcoord.y);
                fixed4 color = tex2D(_MainTex , coord) * IN.color;
                

                return color;

            }
        ENDCG
        }
    }
}
