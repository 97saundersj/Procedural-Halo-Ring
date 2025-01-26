// SimplexNoise3D.hlsl

inline float3 mod289_3(float3 x){
    return x - floor(x / 289.0) * 289.0;
}

inline float4 mod289_4(float4 x){
    return x - floor(x / 289.0) * 289.0;
}

inline float4 permute(float4 x){
    return mod289_4((x * 34.0 + 1.0) * x);
}

inline float4 taylorInvSqrt(float4 r){
    return 1.79284291400159 - r * 0.85373472095314;
}

inline float4 snoise_grad(float3 v){
    float2 C = float2(1.0 / 6.0, 1.0 / 3.0);
    // First corner
    float3 i  = floor(v + dot(v, C.yyy));
    float3 x0 = v   - i + dot(i, C.xxx);
    // Other corners
    float3 g = step(x0.yzx, x0.xyz);
    float3 l = 1.0 - g;
    float3 i1 = min(g.xyz, l.zxy);
    float3 i2 = max(g.xyz, l.zxy);

    float3 x1 = x0 - i1 + C.xxx;
    float3 x2 = x0 - i2 + C.yyy;
    float3 x3 = x0 - 0.5;
    // Permutations
    i = mod289_3(i); // Avoid truncation effects in permutation
    float4 p =
        permute(permute(permute(i.z + float4(0.0, i1.z, i2.z, 1.0))
                              + i.y + float4(0.0, i1.y, i2.y, 1.0))
                              + i.x + float4(0.0, i1.x, i2.x, 1.0));
    // Gradients: 7x7 points over a square, mapped onto an octahedron.
    // The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
    float4 j = p - 49.0 * floor(p / 49.0);  // mod(p,7*7)
    float4 x_ = floor(j / 7.0);
    float4 y_ = floor(j - 7.0 * x_);  // mod(j,N)
    float4 x = (x_ * 2.0 + 0.5) / 7.0 - 1.0;
    float4 y = (y_ * 2.0 + 0.5) / 7.0 - 1.0;
    float4 h = 1.0 - abs(x) - abs(y);
    float4 b0 = float4(x.xy, y.xy);
    float4 b1 = float4(x.zw, y.zw);
    //float4 s0 = float4(lessThan(b0, 0.0)) * 2.0 - 1.0;
    //float4 s1 = float4(lessThan(b1, 0.0)) * 2.0 - 1.0;
    float4 s0 = floor(b0) * 2.0 + 1.0;
    float4 s1 = floor(b1) * 2.0 + 1.0;
    float4 sh = -step(h, 0.0);
    float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
    float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
    float3 g0 = float3(a0.xy, h.x);
    float3 g1 = float3(a0.zw, h.y);
    float3 g2 = float3(a1.xy, h.z);
    float3 g3 = float3(a1.zw, h.w);
    // Normalise gradients
    float4 norm = taylorInvSqrt(float4(dot(g0, g0), dot(g1, g1), dot(g2, g2), dot(g3, g3)));
    g0 *= norm.x;
    g1 *= norm.y;
    g2 *= norm.z;
    g3 *= norm.w;
    // Compute noise and gradient at P
    float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
    float4 m2 = m * m;
    float4 m3 = m2 * m;
    float4 m4 = m2 * m2;
    float3 grad =
        -6.0 * m3.x * x0 * dot(x0, g0) + m4.x * g0 +
        -6.0 * m3.y * x1 * dot(x1, g1) + m4.y * g1 +
        -6.0 * m3.z * x2 * dot(x2, g2) + m4.z * g2 +
        -6.0 * m3.w * x3 * dot(x3, g3) + m4.w * g3;
    float4 px = float4(dot(x0, g0), dot(x1, g1), dot(x2, g2), dot(x3, g3));
    return 42.0 * float4(grad, dot(m4, px));
}

float4 EvaluateLayeredNoise(float3 p,float3 offset, float strength, int octaves, float baseRoughness, float roughness,float persistence){
    float4 noiseVector = 0.0;
    float frequency = baseRoughness;
    float amplitude  = 1.0;

    for(int i=0;i<octaves;i++){
        float4 n = snoise_grad(p * frequency + offset);
        noiseVector += (n+1.0) * 0.5 * amplitude;
        frequency *= roughness;
        amplitude *= persistence;
    }

    return noiseVector * strength;
}

void SimplexNoise3D_float(float3 In, out float Noise, out float3 Gradient){
    float4 noise_vector;
        noise_vector = snoise_grad(In);
        Noise = noise_vector.w;
        Gradient = noise_vector.xyz;
}

void SimplexNoise3DLayered_float(float3 In,float3 Offset,float Strength,int Octaves,float BaseRoughness,float Roughness,float Persistence, out float Noise, out float3 Gradient){
    float4 noise_vector;
    noise_vector = EvaluateLayeredNoise(In,Offset,Strength,Octaves,BaseRoughness, Roughness, Persistence);
    Noise = noise_vector.w;
    Gradient = noise_vector.xyz;
}