Shader "Custom/Selectable"
{
	Properties
	{
		[PerRendererData] _ID("Render ID", Int) = -1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
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
				float3 worldPos : TEXCOORD0;
            };

			int _ID;

            v2f vert (appdata v)
            {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
            }

			fixed4 frag(v2f i) : SV_Target
			{
				return half4(i.worldPos.xyz, _ID);
            }
            ENDCG
        }
    }
}
