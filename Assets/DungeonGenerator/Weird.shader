Shader "Parallax Window" {
    Properties {
      _MainTex ("Texture", 2D) = "white" {}
       _Cube ("Cubemap", CUBE) = "" {}
    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf Lambert
      struct Input {
          float2 uv_MainTex;
          float4 screenPos;
          float3 viewDir;
          float3 worldNormal;
      };
      
      sampler2D _MainTex;
      samplerCUBE _Cube;
      
      void surf (Input IN, inout SurfaceOutput o) {
      	  float4 mainSample = tex2D (_MainTex, IN.uv_MainTex).rgba;
          float4 backSample = texCUBE(_Cube, IN.viewDir * -1).rgba;
          o.Albedo = mainSample.rgb * mainSample.a;
          o.Emission = backSample.rgb * (1 - mainSample.a);
      }
      ENDCG
    } 
    Fallback "Diffuse"
  }