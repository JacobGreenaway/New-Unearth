Shader "Hidden/LayersConversion"
{
	Properties
	{
		_DepthTex ("Depth Texture", 2D) = "white" {}

		_RangeMin ("Range Min", float) = 0
		_RangeMax ("Range Max", float) = 1
		_FadeRange ("Fade Range", float) = 0.05
		_UOffset ("U Offset", float) = 0
		_VOffset ("V Offset", float) = 0

		_Layer1Color ("Layer 1 Color", Color) = (0,0,0,1)
		_Layer1Max ("Layer 1 Max", float) = 0

		_Layer2Color ("Layer 2 Color", Color) = (0,0,0,1)
		_Layer2Max ("Layer 2 Max", float) = 0

		_Layer3Color ("Layer 3 Color", Color) = (0,0,0,1)
		_Layer3Max ("Layer 3 Max", float) = 0

		_Layer4Color ("Layer 4 Color", Color) = (0,0,0,1)
		_Layer4Max ("Layer 4 Max", float) = 0

		_Layer5Color ("Layer 5 Color", Color) = (0,0,0,1)
		_Layer5Max ("Layer 5 Max", float) = 0

		_Layer6Color ("Layer 6 Color", Color) = (0,0,0,1)
		_Layer6Max ("Layer 6 Max", float) = 0

		_Layer7Color ("Layer 7 Color", Color) = (0,0,0,1)
		_Layer7Max ("Layer 7 Max", float) = 0

		_Layer8Color ("Layer 8 Color", Color) = (0,0,0,1)
		_Layer8Max ("Layer 8 Max", float) = 0

		_Layer1Color ("Layer 9 Color", Color) = (0,0,0,1)
		_Layer1Max ("Layer 9 Max", float) = 0

		_Layer1Color ("Layer 10 Color", Color) = (0,0,0,1)
		_Layer1Max ("Layer 10 Max", float) = 0
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
				o.uv = float2(abs(_UOffset - v.uv.x), abs(_VOffset - v.uv.y));
				return o;
			}
			
			sampler2D _DepthTex;
			fixed _RangeMin;
			fixed _RangeMax;
			fixed _FadeRange;
			fixed4 _Layer1Color;
			fixed _Layer1Max;
			fixed4 _Layer2Color;
			fixed _Layer2Max;
			fixed4 _Layer3Color;
			fixed _Layer3Max;
			fixed4 _Layer4Color;
			fixed _Layer4Max;
			fixed4 _Layer5Color;
			fixed _Layer5Max;
			fixed4 _Layer6Color;
			fixed _Layer6Max;
			fixed4 _Layer7Color;
			fixed _Layer7Max;
			fixed4 _Layer8Color;
			fixed _Layer8Max;
			fixed4 _Layer9Color;
			fixed _Layer9Max;
			fixed4 _Layer10Color;
			fixed _Layer10Max;

			fixed4 frag (v2f i) : SV_Target
			{
				float4 depth = tex2D(_DepthTex, i.uv);
				float height = 1.0 -depth.r;
				float halfFade = _FadeRange * 0.5;

				if(height < 0.001)
				{
					return fixed4(0,0,0,0);
				}

				//if(height > _RangeMax)
				//{
				//	return fixed4(1,0,0,1);
				//}

				if(height < _Layer1Max - halfFade)
				{
					return _Layer1Color;
				}

				if(height < _Layer1Max + halfFade)
				{
					return lerp(_Layer1Color, _Layer2Color, (height - (_Layer1Max - halfFade)) / _FadeRange);
				}

				if(height < _Layer2Max - halfFade)
				{
					return _Layer2Color;
				}

				if(height < _Layer2Max + halfFade)
				{
					return lerp(_Layer2Color, _Layer3Color, (height - (_Layer2Max - halfFade)) / _FadeRange);
				}

				if(height < _Layer3Max - halfFade)
				{
					return _Layer3Color;
				}

				if(height < _Layer3Max + halfFade)
				{
					return lerp(_Layer3Color, _Layer4Color, (height - (_Layer3Max - halfFade)) / _FadeRange);
				}

				if(height < _Layer4Max - halfFade)
				{
					return _Layer4Color;
				}

				if(height < _Layer4Max + halfFade)
				{
					return lerp(_Layer4Color, _Layer5Color, (height - (_Layer4Max - halfFade)) / _FadeRange);
				}

				if(height < _Layer5Max - halfFade)
				{
					return _Layer5Color;
				}

				if(height < _Layer5Max + halfFade)
				{
					return lerp(_Layer5Color, _Layer6Color, (height - (_Layer5Max - halfFade)) / _FadeRange);
				}

				if(height < _Layer6Max - halfFade)
				{
					return _Layer6Color;
				}

				if(height < _Layer6Max + halfFade)
				{
					return lerp(_Layer6Color, _Layer7Color, (height - (_Layer6Max - halfFade)) / _FadeRange);
				}

				if(height < _Layer7Max - halfFade)
				{
					return _Layer7Color;
				}

				if(height < _Layer7Max + halfFade)
				{
					return lerp(_Layer7Color, _Layer8Color, (height - (_Layer7Max - halfFade)) / _FadeRange);
				}

				if(height < _Layer8Max - halfFade)
				{
					return _Layer8Color;
				}

				if(height < _Layer8Max + halfFade)
				{
					return lerp(_Layer8Color, _Layer9Color, (height - (_Layer8Max - halfFade)) / _FadeRange);
				}

				if(height < _Layer9Max - halfFade)
				{
					return _Layer9Color;
				}

				if(height < _Layer9Max + halfFade)
				{
					return lerp(_Layer9Color, _Layer10Color, (height - (_Layer9Max - halfFade)) / _FadeRange);
				}

				if(height < _Layer10Max)
				{
					return _Layer10Color;
				}

				// Return black as default
				return fixed4(0,0,0,1);
			}
			ENDCG
		}
	}
}
