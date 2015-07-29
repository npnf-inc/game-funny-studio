Shader "Custom/SeparateRGBandAlphaColorMask" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("RGB", 2D) = "white" {}
		_ShapeTex ("Alpha", 2D) = "white" {}
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 200
		pass {
			Cull Off
			Lighting On
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			
			Color[_Color]
			
			SetTexture [_ShapeTex] {
				combine texture
			}
			
			SetTexture [_MainTex] {
				combine texture * primary, previous * primary
			}
		}
		CGPROGRAM
		#pragma surface surf NoLighting Lambert

		sampler2D _MainTex;
		sampler2D _ShapeTex;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			half4 m = tex2D (_ShapeTex, IN.uv_MainTex);

			float colourPortion = m.r;

			if (m.r != 0 || m.g != 0 || m.b != 0)
			{
				o.Albedo = (c.rgb * (colourPortion))
					+ (_Color.rgb * (colourPortion));
			}
			else
			{
				o.Albedo = c.rgb;
			}
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
