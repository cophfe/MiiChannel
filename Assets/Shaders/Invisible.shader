Shader "Custom/Invisible"
{
    Properties
    {
		[IntRange] _StencilValue("Stencil Value", Range(0,255)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Geometry - 1" }
        //Tags { "RenderType"="Opaque" }
		Cull Off 
		ZWrite Off 
		ZTest Off
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
			Stencil{
				Ref[_StencilValue]
				Comp Always
				Pass Replace
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return float4(0,0,0,0);
            }
            ENDCG
        }
    }
}
