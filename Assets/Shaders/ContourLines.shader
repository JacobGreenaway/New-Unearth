Shader "Hidden/ContourLines"
{
	Properties
	{
		_DepthTex ("Depth Texture", 2D) = "white" {}

		_SeaLevel ("Sea Level", float) = 0
		_HeightDivision ("Height Division", float) = 0.05
		_LineThickness ("Line Thickness", float) = 0.05

		_LineColor ("Line Color", Color) = (0,0,0,1)

		_RangeMin ("Range Min", float) = 0
		_RangeMax ("Range Max", float) = 1
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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _DepthTex;
			float _SeaLevel;
			float _HeightDivision;
			float _LineThickness;
			fixed4 _LineColor;
			float _RangeMin;
			float _RangeMax;

			fixed4 frag (v2f i) : SV_Target
			{
				float4 depth = tex2D(_DepthTex, i.uv);
				float height = 1.0 - depth.r;
				float halfLine = _LineThickness * 0.5; 

				//if(height < _RangeMin)
				//{
				//	return fixed4(0,0,0,0);
				//}
				//if(height > _RangeMax)
				//{
				//	return fixed4(0,0,0,0);
				//}

				float reduced = (abs(height - _SeaLevel) + halfLine) % _HeightDivision;
				return (reduced <= _LineThickness) ? _LineColor : fixed4(0,0,0,0);
			}
			ENDCG
		}
	}
}
