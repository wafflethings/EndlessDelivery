Shader "psx/unlit/transparent/transparent_backface" {
	Properties{
		_Color("Color", Color) = (1, 1, 1, 1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_OpacScale("Transparency Scalar", Range(0,1)) = 1
		_VertexWarpScale("Vertex Warping Scalar", Range(0,10)) = 1
	}
		SubShader{
			Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
			LOD 200
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha
			Lighting On
			Pass{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#define UseStandardVert
				#define UseStandardFrag
				#define UseTransparency
				#include "Assets/Common/Shaders/PSX_Core.cginc"
				ENDCG
			}
		}
}