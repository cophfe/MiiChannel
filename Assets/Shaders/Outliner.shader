Shader "Custom/Outliner"
{
    Properties
    {
		[HideInInspector] _MainTex("Texture", 2D) = "white" {} //this is the camera colour texture
		[HideInInspector] _OutlineData("Outline Data Texture", 2D) = "black" {} //this texture shows what objects to outline
		_OutlineColour("Outline Colour", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
		Cull Off 
		ZWrite Off 
		ZTest Always

        Pass
        {
			Stencil{
				Ref 200
				Comp NotEqual
			}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float2 _MainTex_TexelSize;
			float4 _OutlineColour;
            sampler2D _OutlineData;
			
			//sampler2D _CameraColorTexture;
			//sampler2D _CameraDepthTexture;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

			//based on https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/
			//super basic outline shader

			fixed4 frag(v2f i) : SV_Target
			{
				const int pixels = 4;

				float outlinePower = 0;
				for (int k = 0; k < pixels; ++k)
				{
					 for (int j = 0; j < pixels; ++j)
					 {
						 //increase our output color by the pixels in the area
						 outlinePower += tex2D(_OutlineData, i.uv + _MainTex_TexelSize * float2((k - (float)pixels / 2.0), (j - (float)pixels / 2.0))).r;
					 }
				}
				outlinePower = saturate(outlinePower);
				outlinePower *= _OutlineColour.a;

				if (outlinePower == 0)
					discard;

                return (1- outlinePower) * tex2D(_MainTex, i.uv) + outlinePower * _OutlineColour;
            }
            ENDCG
        }
    }
}
