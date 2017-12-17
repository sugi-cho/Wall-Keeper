Shader "Unlit/Wireframe"
{
	Properties
	{
		_Color("color", Color) = (1,1,1,1)
		_T("color amount", Float) = 1
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		Blend One OneMinusSrcAlpha
		ZWrite Off
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Assets/CGINC/SimplexNoise3D.cginc"

			struct appdata
			{
				float4 pos : POSITION;
			};

			struct v2f
			{
				float3 bary : TEXCOORD0;
				uint vid : TEXCOORD1;
				float val : TEXCOORD2;
				float3 wPos : TEXCOORD3;
				float4 pos : SV_POSITION;
			};

			half4 _Color;
			half _T;

			v2f vert(appdata v, uint vid:SV_VertexID)
			{
				v2f o = (v2f)0;
				o.pos = UnityObjectToClipPos(v.pos);
				o.vid = vid;
				o.wPos = v.pos.xyz;
				return o;
			}

			[maxvertexcount(3)]

			void geom(triangle v2f v[3], inout TriangleStream<v2f> triStream) {
				float3 center = (v[0].pos + v[1].pos + v[2].pos).xyz / 3.0;
				for (int i = 0; i < 3; i++) {
					v2f o = v[i];
					o.bary = half3(i == 0, i == 1, i == 2);
					o.val = snoise(float3(center.xy*0.75, _Time.y));
					triStream.Append(o);
				}
				triStream.RestartStrip();
			}

			fixed4 frag(v2f i) : SV_Target
			{
				half3 d = fwidth(i.bary);
				half3 a3 = smoothstep(half3(0,0,0), d*1.0, i.bary);
				half w = 1.0 - min(min(a3.x,a3.y),a3.z);

				half val = pow(i.val*0.5 + 0.5,2.0);

				half2 sUV = i.wPos.xz;

				half noise = 0.0;
				half amount = 1.0;
				half size = 5.0;
				for (int i = 0; i < 5; i++) {
					noise += amount*snoise(float3(sUV*size, _Time.y));
					amount *= 0.5;
					size *= 2.0;
				}
				val *= 0.5 + saturate(noise + 0.5);
				half4 col = lerp(0.25*_Color*_Color, _Color, val);

				return _T*col + w;
			}
			ENDCG
		}
	}
}
