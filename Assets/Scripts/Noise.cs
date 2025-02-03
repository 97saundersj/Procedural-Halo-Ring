﻿using UnityEngine;
//terrainGenerator by https://www.youtube.com/@SebastianLague on https://youtube.com/playlist?list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3&si=nUjByP-UTCn8Kv7a
public static class Noise
{
    //Generate scaled or unscaled noise float matrix.

    public enum NormalizeMode { Local, Global };

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, AnimationCurve heightCurve = null, float heightMultiplier = 1, int levelOfDetail = 0)
    {
        // Adjust the resolution based on levelOfDetail
        int detailFactor = Mathf.Max(1, (int)Mathf.Pow(2, levelOfDetail));

        // Calculate the effective map dimensions based on the level of detail
        int effectiveWidth = mapWidth / detailFactor;
        mapHeight = mapHeight / detailFactor;

        Vector2 sampleCentre = new Vector2(0, 0);
        var normalizeMode = NormalizeMode.Global;

        float[,] noiseMap = new float[effectiveWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-1000, 1000) + (offset.x + sampleCentre.x) / (scale);
            float offsetY = prng.Next(-1000, 1000) - (offset.y - sampleCentre.y) / (scale);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;
        float halfWidth = effectiveWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < effectiveWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = ((x * detailFactor) - halfWidth) / scale * frequency + octaveOffsets[i].x * frequency;
                    float sampleY = ((y * detailFactor) - halfHeight) / scale * frequency - octaveOffsets[i].y * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                if (heightCurve != null)
                {
                    noiseMap[x, y] = heightCurve.Evaluate(noiseHeight) * heightMultiplier;
                }
                else
                {
                    noiseMap[x, y] = noiseHeight;
                }

                if (normalizeMode == NormalizeMode.Global)
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight / 0.9f);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        if (normalizeMode == NormalizeMode.Local)
        {
            for (int y = 0; y < mapHeight; y++)
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