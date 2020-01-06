Shader "Custom/PixelGrid"
{
	Properties
	{
		_GridColorEven("Grid Color", Color) = (1,0,0,.15)
		_GridColorOdd("Grid Color", Color) = (.15,.15,.15,0)
		_PixelsPerUnit("PixelsPerUnit", Vector) = (8,8,0,0)
	}

	SubShader
	{
		Tags{ "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" "IgnoreProjector" = "True" }
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			HLSLPROGRAM
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			half4 _GridColorOdd;
			half4 _GridColorEven;
			float2 _PixelsPerUnit;

			struct Attributes
			{
				float4 positionOS       : POSITION;
				float2 uv               : TEXCOORD0;
			};

			struct Varyings
			{
				float2 uv     : TEXCOORD0;
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings vert(Attributes input)
			{
				Varyings output = (Varyings)0;
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
				output.vertex = vertexInput.positionCS;
				output.uv = input.uv * (_ScreenParams.xy / _PixelsPerUnit.xy);

				return output;
			}

			half4 frag(Varyings input) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				float2 v = input.uv;
				float2 p = floor(v) / 2.0;
				half t = frac(p.x + p.y) * 2.0;

				half4 col = lerp(_GridColorEven, _GridColorOdd, t);
				
				return col;
			}

			#pragma vertex vert
			#pragma fragment frag

			ENDHLSL
		}
	}
	FallBack "Diffuse"
}