using UnityEngine;
using System.Collections.Generic;

public class HaloGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColourMap, Full };
    public DrawMode drawMode;

    [Range(1, 400)]
    public int widthScale;

    [Range(1, 400)]
    public int heightScale;

    public int mapWidth;
    public int mapHeight;

    private int mapChunkfactor = 24;

    private int mapChunkSize = 241;

    [Range(0, 6)]
    public int levelOfDetail;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    public TerrainType[] regions;

    private bool createHalo = true;

    private void Awake()
    {
        GenerateHalo();
    }

    public void GenerateHalo()
    {
        mapWidth = (widthScale * mapChunkfactor) + 1;
        mapHeight = (heightScale * mapChunkfactor) + 1;

        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colourMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colourMap[y * mapWidth + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();

        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
                break;
            case DrawMode.ColourMap:
                display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
                break;
            case DrawMode.Full:
                var meshData = MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail, createHalo);
                var texture = TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight);
                display.DrawMesh(meshData, texture);
                break;
        }
    }

    void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }

        if (mapWidth != (widthScale * mapChunkfactor) + 1)
        {
            mapWidth = (widthScale * mapChunkfactor) + 1;
        }
        if (mapHeight != (heightScale * mapChunkfactor) + 1)
        {
            mapHeight = (heightScale * mapChunkfactor) + 1;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
    public Texture2D texture;
    public List<SpawnableObject> objectsToSpawn;
}

[System.Serializable]
public class SpawnableObject
{
    public GameObject prefab;
    public float minSize;
    public float maxSize;
    public float density;
    public bool randomRotation;
}