 Shader "Custom/Diffuse Warpy" {
    Properties {
      _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf Lambert
      #pragma target 3.0
      #include "UnityCG.cginc"
      struct Input {
          float2 uv_MainTex;
      };
      sampler2D _MainTex;
      void surf (Input IN, inout SurfaceOutput o) {
      		float2 warpedUV;
      		warpedUV.x = sin(IN.uv_MainTex.x + cos(IN.uv_MainTex.y + _Time.x) * sin(_Time.x));
      		warpedUV.y = sin(IN.uv_MainTex.y + cos(IN.uv_MainTex.x + _Time.x) * cos(_Time.x));
      		float a = sin(_Time.x + IN.uv_MainTex.x);
      		float b = sin(_Time.x) + 2;
      		float x = sin(IN.uv_MainTex.x);
      		float y = sin(IN.uv_MainTex.y + x);
      		float angle = a*exp(-(x*x+y*y)/(b*b));
			warpedUV.x += cos(angle) * x + sin(angle) * y;
			warpedUV.y += -sin(angle) * x + cos(angle) * y;
          o.Albedo = tex2D (_MainTex, warpedUV).rgb;
      }
      ENDCG
    } 
    Fallback "Diffuse"
  }
