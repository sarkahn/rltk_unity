Shader "Custom/GlyphShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Cutoff ("Cutoff", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
				float4 fg : TEXCOORD1;
				float4 bg : TEXCOORD2;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float4 fg : FGCOLOR;
				float4 bg : BGCOLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _Cutoff;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.fg = v.fg;
				o.bg = v.bg;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texCol = tex2D(_MainTex, i.uv);
				fixed4 col = i.fg *texCol;
				col = texCol.a < _Cutoff ? i.bg : col;
                
				return col;
            }
            ENDCG
        }
    }
}
