﻿Shader "GLTFUtility/Standard Transparent (Specular)" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _SpecGlossMap ("Specular Map", 2D) = "white" {}
		_SpecColor ("Specular Color", Color) = (1,1,1,1)
		_GlossyReflections ("Glossiness", Range(0,1)) = 0.0
		[Normal][NoScaleOffset] _BumpMap ("Normal", 2D) = "bump" {}
		[NoScaleOffset] _OcclusionMap ("Occlusion", 2D) = "white" {}
		[NoScaleOffset] _EmissionMap ("Emission", 2D) = "black" {}
		_EmissionColor ("Emission Color", Color) = (0,0,0,0)
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 200

		CGPROGRAM
		// Physically based StandardSpecular lighting model, and enable shadows on all light types
		#pragma surface surf StandardSpecular fullforwardshadows alpha:fade

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _SpecGlossMap;
		sampler2D _BumpMap;
		sampler2D _OcclusionMap;
		sampler2D _EmissionMap;

		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
		};

		half _GlossyReflections;
		fixed4 _Color;
		fixed4 _EmissionColor;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb * IN.color;
			o.Alpha = c.a;
			// Specular / roughness
			fixed4 s = tex2D (_SpecGlossMap, IN.uv_MainTex);
			o.Specular = s.rgb * _SpecColor;
			o.Smoothness = 1 - (s.a * _GlossyReflections);
			// Normal comes from a bump map
			o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_MainTex));
			// Ambient Occlusion comes from red channel
			o.Occlusion = tex2D (_OcclusionMap, IN.uv_MainTex).r;
			// Emission comes from a texture tinted by color
			o.Emission = tex2D (_EmissionMap, IN.uv_MainTex) * _EmissionColor;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
