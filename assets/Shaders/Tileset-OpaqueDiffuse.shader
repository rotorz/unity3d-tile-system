// Derived from 'Sprites-Diffuse.shader'
//
// Main changes:
//  - Main texture can be selected.
//  - Does not have color tinting.
//  - Does not have alpha blending.
//

Shader "Rotorz/Tileset/Opaque Diffuse"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"IgnoreProjector"="True"
			"RenderType"="Opaque"
			"PreviewType"="Plane"
		}

		Cull Off
		Lighting Off
		Fog { Mode Off }

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert
		#pragma multi_compile DUMMY PIXELSNAP_ON

		sampler2D _MainTex;

		struct Input
		{
			float2 uv_MainTex;
		};

		void vert(inout appdata_full v, out Input o)
		{
			#if defined(PIXELSNAP_ON) && !defined(SHADER_API_FLASH)
			v.vertex = UnityPixelSnap(v.vertex);
			#endif
			v.normal = float3(0,0,-1);

			UNITY_INITIALIZE_OUTPUT(Input, o);
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
		}
		ENDCG
	}

Fallback "VertexLit"
}
