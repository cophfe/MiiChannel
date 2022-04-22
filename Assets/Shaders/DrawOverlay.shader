Shader "Custom/ConstructOverlay"
{
	Properties
	{
		_MainTex("Last Version", 2D) = "black" {}
		_DrawPosition ("Draw Position", Vector) = ( 0.0, 0.0, 0.0, 0.0 )
		_DrawRadius ("Draw Radius", Float) = 0.0
		_DrawColour ("Draw Colour", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
		Cull Off
		ZWrite Off
		ZTest Off

        Pass
        {
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
				float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
			float4 _DrawPosition;
			float4 _DrawColour;
			float _DrawRadius;

            v2f vert (appdata v)
            {
				v2f o;
				//remap uv space to clip space (0-1 to -1 to 1)
				float2 clipUv = v.uv * 2.0f - 1.0f;
				clipUv.y *= -1;

                o.vertex = fixed4(clipUv, 0.0f, 1.0f);
				o.uv = v.uv;
				o.worldPosition = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

			fixed4 frag(v2f i) : SV_Target
			{
				float drawValue = saturate(smoothstep(0, 0.01f, _DrawRadius - length(i.worldPosition.xyz - _DrawPosition.xyz)) * 100);
				fixed4 drawColour = _DrawColour * saturate(drawValue);

                fixed4 col = tex2D(_MainTex, i.uv);
                return (1-drawValue) * col + drawColour;
               // return fixed4(i.uv, 0, 1.0);
            }
            ENDCG
        }
    }
}
