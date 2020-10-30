Shader "Custom/RLTK-ConsoleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		// Alpha value from texture color determines FGColor/BGColor cutoff
		_BGCutoff ("Background Color Cutoff", Range(0,1)) = 0.1


		[Toggle(SCREEN_BURN)]
		[Header(SCREEN BURN)]
		_ScreenBurn("Screen Burn", Float) = 0
		_ScreenBurnIntensity("Screen Burn Intensity", Range(0,1)) = 0.2


		[KeywordEnum(None, Pixels, Units)]
		[Header(SCANLINES)]
		_Scanlines("Scanlines", Float) = 0

		_LineIntensity("Scanline Intensity", Range(0,1)) = 0.25

		[Toggle(HARD_LINES)]
		_HardLines("Hard Lines", Float) = 0

		_ScanlineFrequency("ScanlineFrequency", Float) = 1


		[Toggle(SCANLINES_FG_ONLY)]
		_ScanlinesFGOnly("ForegroundOnly", Float) = 0
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
			//#pragma multi_compile __ SCANLINES
			#pragma multi_compile __ HARD_LINES
			#pragma multi_compile __ SCANLINES_FG_ONLY
			#pragma multi_compile _APPLYPIXELSCALE_NONE _APPLYPIXELSCALE_CAMERA _APPLYPIXELSCALE_MANUAL 
			#pragma multi_compile _SCANLINES_NONE _SCANLINES_PIXELS _SCANLINES_UNITS

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float4 fg : TEXCOORD1;
				float4 bg : TEXCOORD2;
				float2 pixelUV : TEXCOORD3;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float4 fg : FGCOLOR;
				float4 bg : BGCOLOR;
				float2 pixelUV : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			float _BGCutoff;
			float _LineIntensity;
			float _ScreenBurnIntensity;

			float _ScanlineFrequency;

			float2 _ConsoleSize;

			// Camera pixel scale, should be automatically set from script. See LockCameraToConsole for example
			// Only applies if _APPLYPIXELSCALE_PIXEL or _APPLYPIXELSCALE_HALFPIXEL is set
			float _CameraPixelScale;
			float _PixelRatio;

			float2 _PixelCount;

            v2f vert (appdata v, out float4 outpos : SV_POSITION)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.fg = v.fg;
				o.bg = v.bg;
				o.pixelUV = v.pixelUV;
				outpos = UnityObjectToClipPos(v.vertex);
                return o;
            }

			fixed3 scanline(fixed3 col, fixed2 screenPos)
			{
				#ifdef HARD_LINES
				//screenPos.y = screenPos.y / _CameraPixelScale;
					if (floor(screenPos.y) % 2 == 0)
						return col;
					else
						return col * (1 - _LineIntensity);
				#endif

				//screenPos *= 2;
				fixed scanline = fmod(screenPos.y, 2.0) / 2;
				fixed3 scanColor = col * (1 - (scanline * _LineIntensity) );

				return scanColor;
			}

			fixed3 screenburn(fixed3 col, fixed2 screenPos, fixed2 screenDims)
			{
				float dist = (1.0 -
					distance(float2(screenPos.x / screenDims.x, screenPos.y / screenDims.y), float2(0.5, 0.5)));

				dist *= _ScreenBurnIntensity;

				return fixed3(0,dist,dist);
			}


            fixed4 frag (v2f i, UNITY_VPOS_TYPE vPos : VPOS) : SV_Target
            {
                fixed4 texCol = tex2D(_MainTex, i.uv);
				fixed4 col = i.fg * texCol;

				fixed3 fg = i.fg.rgb * texCol;
				fixed3 bg = i.bg.rgb;
				 
				fixed2 screenPos = vPos.xy;
				fixed2 screenDims = _ScreenParams.xy;

				fixed2 pixel = i.pixelUV * _ScanlineFrequency; 
				fixed2 pixelCount = _PixelCount * _ScanlineFrequency;

#if SCREEN_BURN
				bg = lerp(bg, screenburn(bg, pixel, pixelCount), _ScreenBurnIntensity);
#endif
 
#if _SCANLINES_PIXELS
				pixel /= _ScanlineFrequency;
				float2 ppu = _MainTex_TexelSize.zw / 16;
				pixel *= _PixelRatio;
				pixel /= _ScanlineFrequency;
#endif

#if _SCANLINES_NONE
#else
	#if SCANLINES_FG_ONLY
				fg = scanline(fg, pixel);
	#else
				fg = scanline(fg, pixel);
				bg = scanline(bg, pixel);
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
