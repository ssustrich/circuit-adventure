Shader "Custom/InvertedHullOutline"
{
    Properties
    {
        _Color ("Color", Color) = (0, 1, 1, 1)
        _Thickness ("Thickness", Float) = 0.03
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Front        // draw backfaces only (inverted hull)
        ZWrite Off        // don't write depth
        ZTest LEqual      // draw where it's not occluded by the main mesh
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Thickness;
            fixed4 _Color;

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct v2f {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                // expand along normals to make the shell slightly larger
                v.vertex.xyz += normalize(v.normal) * _Thickness;
                o.pos = UnityObjectToClipPos(v.vertex);
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
