Shader "Unlit/Wireframe"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
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
				float3 bary : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o = (v2f)0;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			[maxvertexcount(3)]

			void geom(triangle v2f v[3], inout TriangleStream<v2f> triStream) {
				for (int i = 0; i<3; i++) {
					v2f o = v[i];
					o.bary = half3(i == 0, i == 1, i == 2);
					triStream.Append(o);
				}
				triStream.RestartStrip();
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				half3 d = fwidth(i.bary);
				half3 a3 = smoothstep(half3(0,0,0), d*1.0, i.bary);
				half w = 1.0 - min(min(a3.x,a3.y),a3.z);
				return w;
			}
			ENDCG
		}
	}
}
