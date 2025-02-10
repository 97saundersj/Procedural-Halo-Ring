using UnityEngine;
using System;

public static class Noise
{
    public enum NormalizeMode { Local, Global };

    public static float[,] GenerateNoiseMap(
        int mapWidth, 
        int mapHeight, 
        int seed, 
        float scale, 
        int octaves, 
        float persistance, 
        float lacunarity, 
        Vector2 offset, 
        AnimationCurve heightCurve = null, 
        float heightMultiplier = 1, 
        int levelOfDetail = 0)
    {
        // Adjust the resolution based on the level of detail.
        // For example, levelOfDetail = 0 means full resolution,
        // levelOfDetail = 1 halves the resolution, etc.
        int detailFactor = Mathf.Max(1, (int)Mathf.Pow(2, levelOfDetail));
        int effectiveWidth = mapWidth / detailFactor;
        int effectiveHeight = mapHeight / detailFactor;

        // Compute the center based on the original full-resolution dimensions.
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        float[,] noiseMap = new float[effectiveWidth, effectiveHeight];

        // Change this if you want to use local normalization.
        NormalizeMode normalizeMode = NormalizeMode.Global;

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        // Generate offsets for each octave and compute the maximum possible height.
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-1000, 1000) + offset.x / scale;
            float offsetY = prng.Next(-1000, 1000) - offset.y / scale;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        // Loop through each coordinate of the downsampled (effective) noise map.
        for (int y = 0; y < effectiveHeight; y++)
        {
            for (int x = 0; x < effectiveWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                // Combine noise for each octave.
                for (int i = 0; i < octaves; i++)
                {
                    // Multiply x and y by detailFactor to convert effective coordinates back to full resolution.
                    float sampleX = ((x * detailFactor) - halfWidth) / scale * frequency + octaveOffsets[i].x * frequency;
                    float sampleY = ((y * detailFactor) - halfHeight) / scale * frequency - octaveOffsets[i].y * frequency;

                    // Perlin noise returns values in [0,1]. Remap to [-1,1] for a more balanced distribution.
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                // Track the local min and max noise height.
                if (noiseHeight > maxLocalNoiseHeight)
                    maxLocalNoiseHeight = noiseHeight;
                if (noiseHeight < minLocalNoiseHeight)
                    minLocalNoiseHeight = noiseHeight;

                // Optionally apply a height curve and multiplier.
                if (heightCurve != null)
                    noiseMap[x, y] = heightCurve.Evaluate(noiseHeight) * heightMultiplier;
                else
                    noiseMap[x, y] = noiseHeight;

                // If using global normalization, normalize each value on the fly.
                if (normalizeMode == NormalizeMode.Global)
                {
                    // The divisor (maxPossibleHeight / 0.9f) is used to adjust the range.
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight / 0.9f);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        // If using local normalization, remap the noise map values after generation.
        if (normalizeMode == NormalizeMode.Local)
        {
            for (int y = 0; y < effectiveHeight; y++)
            {
                for (int x = 0; x < effectiveWidth; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
            }
        }

        return noiseMap;
    }
}
