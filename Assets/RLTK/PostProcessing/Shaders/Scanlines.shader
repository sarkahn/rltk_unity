Shader "Unlit/Scanlines"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		[Toggle(SCREEN_BURN)]
		_ScreenBurn ("Screen Burn", Float ) = 0 
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			#pragma target 3.0
			#pragma shader_feature SCREEN_BURN

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v, out float4 outpos : SV_POSITION)
			{
				v2f o;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				outpos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
			{
				
				float2 screenDims = _ScreenParams.xy;
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				
			fixed3 rgb = col.rgb;
			
			float scanline = fmod(screenPos.y, 2.0) * 0.25;

			fixed3 scanColor = rgb - scanline;

			if (rgb.r < 0.1f && rgb.g < 0.1f && rgb.b < 0.1f) 
			{
#ifdef SCREEN_BURN
				float dist = (1.0 - 
					distance( float2( screenPos.x / screenDims.x, screenPos.y / screenDims.y), float2(0.5, 0.5))) * 0.2;
				return float4(0, dist, dist, 1);
#else
				return float4(0, 0, 0, 1);
#endif
			}
				

				return float4(scanColor, 1.0);
			}
			ENDCG
		}
	}
}
