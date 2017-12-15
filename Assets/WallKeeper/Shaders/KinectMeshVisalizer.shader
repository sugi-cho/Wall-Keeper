Shader "Unlit/KinectMeshVisalizer"
{
	Properties{
		_Color ("color", Color) = (1,1,1,1)
		_EdgeThreshold ("edge max length", Range(0.01, 1.0)) = 0.2
		_WireWidth ("wireframe width", Range(0.0,2.0)) = 1.0
		_Tilt ("tilt z-direction", Float) = 0
	}
	CGINCLUDE
	#include "UnityCG.cginc"
	#define Size_X 512
	#define Size_Y 424

	struct v2f
	{
		float4 pos : SV_POSITION;
		half3 wPos : TEXCOORD0;
		uint idx : TEXCOORD1;
	};

	StructuredBuffer<float3> _VertexData;
	StructuredBuffer<float3> _AtackPoints;
	sampler2D _ColorTex;
	sampler2D _BodyIdxTex;

	float _EdgeThreshold;
	float _WireWidth;
	float _Tilt;
	half4 _Color;
	
	v2f getVertexOut(uint idx) {
		float3 pos = _VertexData[idx];
		pos.z += min(0, pos.y) * _Tilt;
		v2f o = (v2f)0;
		o.pos = UnityObjectToClipPos(pos);
		o.wPos = pos;
		o.idx = idx;
		return o;
	}

	v2f vert (uint idx : SV_VertexID)
	{
		return getVertexOut(idx);
	}

	float edgeLength(float3 v0, float3 v1, float3 v2) {
		float l = distance(v0, v1);
		l = max(l, distance(v1, v2));
		l = max(l, distance(v2, v0));
		return l;
	}

	[maxvertexcount(6)]
	void geom(point v2f input[1], inout TriangleStream<v2f> triStream) 
	{
		v2f p0 = input[0];
		uint idx = p0.idx;

		v2f p1 = getVertexOut(idx + 1);
		v2f p2 = getVertexOut(idx + Size_X);
		v2f p3 = getVertexOut(idx + Size_X+1);

		if (edgeLength(p0.pos.xyz, p1.pos.xyz, p2.pos.xyz) < _EdgeThreshold) {
			triStream.Append(p0);
			triStream.Append(p1);
			triStream.Append(p2);
			triStream.RestartStrip();
		}

		if (edgeLength(p1.pos.xyz, p3.pos.xyz, p2.pos.xyz) < _EdgeThreshold) {
			triStream.Append(p1);
			triStream.Append(p3);
			triStream.Append(p2);
			triStream.RestartStrip();
		}
	}
			
	fixed4 frag (v2f i) : SV_Target
	{
		float2 depthUV = float2(i.idx % 512 / 512.0, i.idx / 512 / 424.0);
		fixed bodyIdx = tex2D(_BodyIdxTex, depthUV).r;

		if (bodyIdx == 1) discard;
		half4 col = lerp(0, _Color, i.wPos.y+2);
		for (int idx = 0; idx < 3 * 6; idx++) {
			half3 diff = i.wPos - _AtackPoints[idx];
			half d = dot(diff, diff);
			col = lerp(col, half4(1, 0, 0, 1), saturate(1 - d * 20.0));
		}

		return col;
	}
	ENDCG
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100 Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			
			ENDCG
		}
	}
}
