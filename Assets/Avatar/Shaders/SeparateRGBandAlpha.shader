Shader "Custom/SeparateRGBandAlpha" {
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
			Lighting Off
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
	}

}
