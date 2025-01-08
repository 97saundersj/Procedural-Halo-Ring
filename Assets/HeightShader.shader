Shader "Custom/Terrain" {
	Properties {
		_Center("Center Point", Vector) = (0, 0, 0)
		_MinRadius("Min Radius", Float) = 9990.0
		_MaxRadius("Max Radius", Float) = 10000.0
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

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Calculate the distance from the center axis (ignoring the y-axis for a horizontal ring)
			float distanceFromAxis = length(float2(IN.worldPos.z - _Center.z, IN.worldPos.y - _Center.y));

			// Normalize the distance between min and max radius
			float normalizedDistance = saturate((distanceFromAxis - _MinRadius) / (_MaxRadius - _MinRadius));

			o.Albedo = baseColours[0];
			
			// Use baseStartHeights as percentages of the normalized distance
			for (int i = 0; i < baseColourCount; i++) {
				float drawStrength = saturate(sign(normalizedDistance - baseStartHeights[i]));
				o.Albedo = o.Albedo * (1 - drawStrength) + baseColours[i] * drawStrength;
			}
		}

		ENDCG
	}
	FallBack "Diffuse"
}