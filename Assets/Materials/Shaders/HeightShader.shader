Shader "Custom/Terrain" {
	Properties {
		_Center("Center Point", Vector) = (0, 0, 0)
		_MinRadius("Min Radius", Float) = 9990.0
		_MaxRadius("Max Radius", Float) = 10000.0
		_BlendStrength("Blend Strength", Float) = 0.1
		_MainTex("Base Texture", 2D) = "white" {}
		_Texture0("Texture 0", 2D) = "white" {}
		_Texture1("Texture 1", 2D) = "white" {}
		_Texture2("Texture 2", 2D) = "white" {}
		_Texture3("Texture 3", 2D) = "white" {}
		_Texture4("Texture 4", 2D) = "white" {}
		_Texture5("Texture 5", 2D) = "white" {}
		_Texture6("Texture 6", 2D) = "white" {}
		_Texture7("Texture 7", 2D) = "white" {}
		_TextureScale("Texture Scale", Float) = 0.1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		const static int maxColourCount = 8;

		int baseColourCount;
		float3 baseColours[maxColourCount];
		float baseStartHeights[maxColourCount];

		struct Input {
			float3 worldPos;
		};

		uniform float3 _Center;
		uniform float _MinRadius;
		uniform float _MaxRadius;
		uniform float _BlendStrength;
		sampler2D _MainTex;
		sampler2D _Texture0;
		sampler2D _Texture1;
		sampler2D _Texture2;
		sampler2D _Texture3;
		sampler2D _Texture4;
		sampler2D _Texture5;
		sampler2D _Texture6;
		sampler2D _Texture7;
		uniform float _TextureScale;

		float3 TriplanarMapping(float3 worldPos, sampler2D tex) {
	    // Adjust world position relative to the center
	    float3 localPos = worldPos - _Center;
	
	    float3 blending = abs(normalize(localPos));
	    blending = (blending - 0.2) * 1.25;
	    blending = max(blending, 0.0);
	    blending /= (blending.x + blending.y + blending.z);
	
	    float3 xaxis = tex2D(tex, localPos.yz * _TextureScale).rgb;
	    float3 yaxis = tex2D(tex, localPos.zx * _TextureScale).rgb;
	    float3 zaxis = tex2D(tex, localPos.xy * _TextureScale).rgb;
	
	    return xaxis * blending.x + yaxis * blending.y + zaxis * blending.z;
	}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float distanceFromAxis = length(float2(IN.worldPos.z - _Center.z, IN.worldPos.y - _Center.y));
			float normalizedDistance = saturate((distanceFromAxis - _MinRadius) / (_MaxRadius - _MinRadius));

			sampler2D textures[maxColourCount] = { _Texture0, _Texture1, _Texture2, _Texture3, _Texture4, _Texture5, _Texture6, _Texture7 };

			float3 defaultTextureColor = TriplanarMapping(IN.worldPos, textures[0]);
			o.Albedo = defaultTextureColor;

			for (int i = 0; i < baseColourCount; i++) {
				float blendFactor = smoothstep(baseStartHeights[i] - _BlendStrength, baseStartHeights[i] + _BlendStrength, normalizedDistance);
				float3 textureColor = TriplanarMapping(IN.worldPos, textures[i]);
				o.Albedo = o.Albedo * (1 - blendFactor) + textureColor * blendFactor;
			}
		}

		ENDCG
	}
	FallBack "Diffuse"
}