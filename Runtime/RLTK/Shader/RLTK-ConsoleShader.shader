Shader "Custom/RLTK-ConsoleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		// Alpha value from texture color determines FGColor/BGColor cutoff
		_BGCutoff ("Background Color Cutoff", Range(0,1)) = 0.1

		[Toggle(SCREEN_BURN)]
		_ScreenBurn("Screen Burn", Float) = 0
		_ScreenBurnIntensity("Screen Burn Intensity", Range(0,1)) = 0.2

		[Toggle(SCANLINES)]
		_Scanlines("Scanlines", Float) = 0

		// Hard lines completely ignores colors in non-scanline rows, resulting
		// in a brighter final image. Disabling this will somewhat darken rows between scanlines too.
		[Toggle(HARD_LINES)]
		_HardLines("Hard Lines", Float) = 0

		[Toggle(SCANLINES_FG_ONLY)]
		_ScanlinesFGOnly("Scanlines Foreground Only", Float) = 0

		_LineIntensity("Scanline Intensity", Range(0,1)) = 0.25
	
		// Applies the pixel scaling from the pixel perfect camera. 
		[Toggle(APPLY_PIXEL_SCALE)]
		_ApplyPixelScale("Apply pixel scaling to per-pixel effects", Float) = 0

		// If _ApplyPixelScale is true then VPOS will be adjust so 
		// per-pixel effects are scaled with virtual pixels
		_PixelScale("Pixel Scale (Set from script)", Float) = 1 
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
			#pragma target 3.0

			#pragma multi_compile __ SCREEN_BURN
			#pragma multi_compile __ SCANLINES
			#pragma multi_compile __ HARD_LINES
			#pragma multi_compile __ SCANLINES_FG_ONLY

			#pragma shader_feature APPLY_PIXEL_SCALE

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
				float4 fg : FGCOLOR;
				float4 bg : BGCOLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _BGCutoff;
			float _LineIntensity;
			float _Frequency;
			float _ScreenBurnIntensity;
			float _ScanlinesFGOnly;
			float _PixelScale;

            v2f vert (appdata v, out float4 outpos : SV_POSITION)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.fg = v.fg;
				o.bg = v.bg;
				outpos = UnityObjectToClipPos(v.vertex);
                return o;
            }

			fixed3 scanline2(fixed3 col, UNITY_VPOS_TYPE screenPos)
			{
				#ifdef HARD_LINES
					if ((uint)screenPos.y % 2 == 0)
						return col;
					else
						return col * (1 - _LineIntensity);
				#endif

				fixed scanline = fmod(screenPos.y, 2.0);
				fixed3 scanColor = col * (1 - (scanline * _LineIntensity) );

				return scanColor;
			}

			fixed3 screenburn2(fixed3 col, UNITY_VPOS_TYPE screenPos)
			{
				float2 screenDims = _ScreenParams.xy;

#if APPLY_PIXEL_SCALE
				screenDims /= _PixelScale;
#endif
				screenPos = floor(screenPos);

				float dist = (1.0 -
					distance(float2(screenPos.x / screenDims.x, screenPos.y / screenDims.y), float2(0.5, 0.5)));

				dist *= _ScreenBurnIntensity;

				return fixed3(0,dist,dist);
			}


            fixed4 frag (v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
            {
                fixed4 texCol = tex2D(_MainTex, i.uv);
				fixed4 col = i.fg * texCol;

				fixed3 fg = i.fg.rgb * texCol;
				fixed3 bg = i.bg.rgb;

#if APPLY_PIXEL_SCALE
				screenPos /= _PixelScale;
#endif

#if SCREEN_BURN
				bg = screenburn2(bg, screenPos);
#endif
 
#if SCANLINES
	#if SCANLINES_FG_ONLY
					fg = scanline2(fg, screenPos);
	#else
					fg = scanline2(fg, screenPos);
					bg = scanline2(bg, screenPos);
	#endif
#endif

				col.rgb = texCol.a < _BGCutoff ? bg : fg;
				col.a = 1;

				return col;
            }
            ENDCG
        }
    }
}
