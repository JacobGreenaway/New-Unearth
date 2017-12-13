Shader "Hidden/ContourLines"
{
	Properties
	{
		_DepthTex ("Depth Texture", 2D) = "white" {}
		_SeaLevel ("Sea Level", float) = 0
		_HeightDivision ("Height Division", float) = 0.05
		_LineColor ("Line Color", Color) = (0,0,0,1)
		_SampleDistance ("Sample Distance", float) = 1
		_UOffset ("U Offset", float) = 0
		_VOffset ("V Offset", float) = 0
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

			uniform float4 _DepthTex_TexelSize;
			float _SampleDistance;
			float _UOffset;
			float _VOffset;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv[5] : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				v.uv = float2(abs(_UOffset - v.uv.x), abs(_VOffset - v.uv.y));
				o.uv[0] = v.uv;
				// Store the uv coordinates Up, down, left and right, modifies by the sample distance.
				o.uv[1] = v.uv + float2(_DepthTex_TexelSize.x, _DepthTex_TexelSize.y) * _SampleDistance;
				o.uv[2] = v.uv + float2(-_DepthTex_TexelSize.x, _DepthTex_TexelSize.y) * _SampleDistance;
				o.uv[3] = v.uv + float2(-_DepthTex_TexelSize.x, -_DepthTex_TexelSize.y) * _SampleDistance;
				o.uv[4] = v.uv + float2(_DepthTex_TexelSize.x, -_DepthTex_TexelSize.y) * _SampleDistance;
				return o;
			}
			
			sampler2D _DepthTex;
			
			float _SeaLevel;
			float _HeightDivision;
			fixed4 _LineColor;

			fixed4 frag (v2f i) : SV_Target
			{
				float4 depth = tex2D(_DepthTex, i.uv[0]);
				float height = depth.r;
				// Sample up, down, left and right
				float h1 = tex2D(_DepthTex, i.uv[1]).r;
				float h2 = tex2D(_DepthTex, i.uv[2]).r;
				float h3 = tex2D(_DepthTex, i.uv[3]).r;
				float h4 = tex2D(_DepthTex, i.uv[4]).r;

				// Get height band for each sampled texel
				float minLevel = _SeaLevel - floor(_SeaLevel / _HeightDivision) * _HeightDivision;
				int band = floor((height - minLevel) / _HeightDivision);
				int b1 = floor((h1 - minLevel) / _HeightDivision);
				int b2 = floor((h2 - minLevel) / _HeightDivision);
				int b3 = floor((h3 - minLevel) / _HeightDivision);
				int b4 = floor((h4 - minLevel) / _HeightDivision);

				// If any of the surrounding texels are on a lower band, then we are on an edge.
				return band > b1 || band > b2 || band > b3 ||  band > b4 ? _LineColor : fixed4(0,0,0,0);
			}
			ENDCG
		}
	}
}
