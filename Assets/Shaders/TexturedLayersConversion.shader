Shader "Hidden/TexturedLayersConversion"
{
	Properties
	{
		_DepthTex ("Depth Texture", 2D) = "white" {}

		_RangeMin ("Range Min", float) = 0
		_RangeMax ("Range Max", float) = 1
		_FadeRange ("Fade Range", float) = 0.05
		_UOffset ("U Offset", float) = 0
		_VOffset ("V Offset", float) = 0

		_Layer1Tex ("Layer 1 Tex", 2D) = "white" {}
		_Layer1Max ("Layer 1 Max", float) = 0

		_Layer2Tex ("Layer 2 Tex", 2D) = "white" {}
		_Layer2Max ("Layer 2 Max", float) = 0

		_Layer3Tex ("Layer 3 Tex", 2D) = "white" {}
		_Layer3Max ("Layer 3 Max", float) = 0

		_Layer4Tex ("Layer 4 Tex", 2D) = "white" {}
		_Layer4Max ("Layer 4 Max", float) = 0

		_Layer5Tex ("Layer 5 Tex", 2D) = "white" {}
		_Layer5Max ("Layer 5 Max", float) = 0

		_Layer6Tex ("Layer 6 Tex", 2D) = "white" {}
		_Layer6Max ("Layer 6 Max", float) = 0

		_Layer7Tex ("Layer 7 Tex", 2D) = "white" {}
		_Layer7Max ("Layer 7 Max", float) = 0

		_Layer8Tex ("Layer 8 Tex", 2D) = "white" {}
		_Layer8Max ("Layer 8 Max", float) = 0

		_Layer9Tex ("Layer 9 Tex", 2D) = "white" {}
		_Layer9Max ("Layer 9 Max", float) = 0

		_Layer10Tex ("Layer 10 Tex", 2D) = "white" {}
		_Layer10Max ("Layer 10 Max", float) = 0
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			#define SampleTex(name, uv) (tex2D(name##, TRANSFORM_TEX(uv, name##)))
			
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

			float _UOffset;
			float _VOffset;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				// We take the absolute values here to deal with flipping in the UV offset.
				o.uv = float2(abs(_UOffset - v.uv.x), abs(_VOffset - v.uv.y));
				return o;
			}
			
			sampler2D _DepthTex;
			fixed _RangeMin;
			fixed _RangeMax;
			fixed _FadeRange;

			sampler2D _Layer1Tex;
			float4 _Layer1Tex_ST;
			fixed _Layer1Max;

			sampler2D _Layer2Tex;
			float4 _Layer2Tex_ST;
			fixed _Layer2Max;

			sampler2D _Layer3Tex;
			float4 _Layer3Tex_ST;
			fixed _Layer3Max;

			sampler2D _Layer4Tex;
			float4 _Layer4Tex_ST;
			fixed _Layer4Max;

			sampler2D _Layer5Tex;
			float4 _Layer5Tex_ST;
			fixed _Layer5Max;

			sampler2D _Layer6Tex;
			float4 _Layer6Tex_ST;
			fixed _Layer6Max;

			sampler2D _Layer7Tex;
			float4 _Layer7Tex_ST;
			fixed _Layer7Max;

			sampler2D _Layer8Tex;
			float4 _Layer8Tex_ST;
			fixed _Layer8Max;

			sampler2D _Layer9Tex;
			float4 _Layer9Tex_ST;
			fixed _Layer9Max;

			sampler2D _Layer10Tex;
			float4 _Layer10Tex_ST;
			fixed _Layer10Max;

			fixed4 frag (v2f i) : SV_Target
			{
				// Sample the depth texture
				float4 depth = tex2D(_DepthTex, i.uv);
				// Get the height 
				float height = depth.r;
				// Get half the fade range
				float halfFade = _FadeRange * 0.5;

				// If the height is really low, just render black
				if(height < 0.001)
				{
					return fixed4(0,0,0,0);
				}

				// If height is less than the Layer1Max minus the half fade range, render the full texture
				if(height < _Layer1Max - halfFade)
				{
					return SampleTex(_Layer1Tex, i.uv);
				}

				// If height is in the fade range, linearly interpolate between the two layers.
				if(height < _Layer1Max + halfFade)
				{
					return lerp(SampleTex(_Layer1Tex, i.uv), SampleTex(_Layer2Tex, i.uv), (height - (_Layer1Max - halfFade)) / _FadeRange);
				}

				if(height < _Layer2Max - halfFade)
				{
					return SampleTex(_Layer2Tex, i.uv);
				}

				if(height < _Layer2Max + halfFade)
				{
					return lerp(SampleTex(_Layer2Tex, i.uv), SampleTex(_Layer3Tex, i.uv), (height - (_Layer2Max - halfFade)) / _FadeRange);
				}

				if(height < _Layer3Max - halfFade)
				{
					return SampleTex(_Layer3Tex, i.uv);
				}

				if(height < _Layer3Max + halfFade)
				{
					return lerp(SampleTex(_Layer3Tex, i.uv), SampleTex(_Layer4Tex, i.uv), (height - (_Layer3Max - halfFade)) / _FadeRange);
				}

				if(height < _Layer4Max - halfFade)
				{
					return SampleTex(_Layer4Tex, i.uv);
				}

				if(height < _Layer4Max + halfFade)
				{
					return lerp(SampleTex(_Layer4Tex, i.uv), SampleTex(_Layer5Tex, i.uv), (height - (_Layer4Max - halfFade)) / _FadeRange);
				}

				if(height < _Layer5Max - halfFade)
				{
					return SampleTex(_Layer5Tex, i.uv);
				}

				if(height < _Layer5Max + halfFade)
				{
					return lerp(SampleTex(_Layer5Tex, i.uv), SampleTex(_Layer6Tex, i.uv), (height - (_Layer5Max - halfFade)) / _FadeRange);
				}

				if(height < _Layer6Max - halfFade)
				{
					return SampleTex(_Layer6Tex, i.uv);
				}

				if(height < _Layer6Max + halfFade)
				{
					return lerp(SampleTex(_Layer6Tex, i.uv), SampleTex(_Layer7Tex, i.uv), (height - (_Layer6Max - halfFade)) / _FadeRange);
				}

				if(height < _Layer7Max - halfFade)
				{
					return SampleTex(_Layer7Tex, i.uv);
				}

				if(height < _Layer7Max + halfFade)
				{
					return lerp(SampleTex(_Layer7Tex, i.uv), SampleTex(_Layer8Tex, i.uv), (height - (_Layer7Max - halfFade)) / _FadeRange);
				}

				if(height < _Layer8Max - halfFade)
				{
					return SampleTex(_Layer8Tex, i.uv);
				}

				if(height < _Layer8Max + halfFade)
				{
					return lerp(SampleTex(_Layer8Tex, i.uv), SampleTex(_Layer9Tex, i.uv), (height - (_Layer8Max - halfFade)) / _FadeRange);
				}

				if(height < _Layer9Max - halfFade)
				{
					return SampleTex(_Layer9Tex, i.uv);
				}

				if(height < _Layer9Max + halfFade)
				{
					return lerp(SampleTex(_Layer9Tex, i.uv), SampleTex(_Layer10Tex, i.uv), (height - (_Layer9Max - halfFade)) / _FadeRange);
				}

				if(height < _Layer10Max)
				{
					return SampleTex(_Layer10Tex, i.uv);
				}

				// Return black as default
				return fixed4(0,0,0,1);
			}

			ENDCG
		}
	}
}
