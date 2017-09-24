// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

// This shader is used to draw immediate tile previews.

Shader "Rotorz/Preview See Through" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Main Color", COLOR) = (1,0,0,0.5)
	}
	SubShader {
		Tags {
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		LOD 200

		// Force preview objects to render in front!
		Offset 0, -100000

		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _Color;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			float4 _MainTex_ST;

			v2f vert(appdata_base v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			half4 frag(v2f i) : COLOR {
				half4 c = tex2D(_MainTex, i.uv);

				// Use main color to tint texture
				return c * _Color;

				// Use the following for monotone effect
				//half average = (c.r + c.g + c.b) / 3;
				//return half4( average * _Color.r, average * _Color.g, average * _Color.b, c.a * _Color.a );
			}
			ENDCG
		}
	}

	FallBack "Unlit/Texture"
}
