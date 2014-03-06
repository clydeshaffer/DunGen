Shader "Custom/WorldSpaceTex" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
			float3 worldNormal;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			
			half4 cxy = tex2D (_MainTex, float2(IN.worldPos.x, IN.worldPos.y));
			half4 cxz = tex2D (_MainTex, float2(IN.worldPos.x, IN.worldPos.z));
			half4 czy = tex2D (_MainTex, float2(IN.worldPos.z, IN.worldPos.y));
			float xy = abs (dot(IN.worldNormal, float3(0, 0, 1)));
			float xz = abs (dot(IN.worldNormal, float3(0, 1, 0)));
			float yz = abs (dot(IN.worldNormal, float3(1, 0, 0)));
			half4 c = cxy * xy + cxz * xz + czy * yz;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
