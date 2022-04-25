Shader "Custom/Line"
{
	//For the line renderer, just makes it so it renders behind selected player but in front of everything else
    Properties
    {
		_Color("Colour", Color) = (1,1,1,1)
    }
    SubShader
    {
		Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off 
		ZWrite Off 
		ZTest Off

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
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

			float4 _Color;
			
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

			fixed4 frag(v2f i) : SV_Target
			{
				return _Color;
            }
            ENDCG
        }
    }
}
