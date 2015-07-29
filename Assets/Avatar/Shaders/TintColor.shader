// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Custom/TintColor" {
Properties {
	_Color ("Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 100
	
	Pass {
		Lighting Off
		SetTexture [_MainTex] { combine texture } 
	}

	CGPROGRAM
		#pragma surface surf NoLighting Lambert

		sampler2D _MainTex;

		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);

			 o.Albedo = c.rgb + _Color.rgb;
		}

		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
	    {
	        fixed4 c;
	        c.rgb = s.Albedo; 
	        c.a = s.Alpha;
	        return c;
	    }

		ENDCG
}
}
