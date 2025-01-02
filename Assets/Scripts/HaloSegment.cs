using System.Collections.Generic;
using UnityEngine;

public class HaloSegment
{
    private ProceduralHaloChunks proceduralHaloChunks;
    private int segment;

    public HaloSegment(ProceduralHaloChunks proceduralHaloChunks, int segment)
    {
        this.proceduralHaloChunks = proceduralHaloChunks;
        this.segment = segment;
    }

    public GameObject GenerateSegment(int segmentIndexCount, int segmentVertexCount)
    {
        GameObject segmentObject = new GameObject("HaloSegment_" + segment);

        var vertices = new List<Vector3>(segmentVertexCount);
        var uv = new Vector2[segmentVertexCount];
        var indices = new int[segmentIndexCount];

        var segmentWidth = (2 * Mathf.PI * proceduralHaloChunks.radiusInMeters) / proceduralHaloChunks.CircleSegmentCount;
        Debug.Log("segmentWidth: " + segmentWidth);

        float widthScale = segmentWidth / proceduralHaloChunks.textureMetersPerPixel;
        float heightScale = proceduralHaloChunks.widthInMeters / proceduralHaloChunks.textureMetersPerPixel;

        float[,] noiseMap = GenerateNoiseMap(widthScale, heightScale);

        // Generate Procedural Texture
        var proceduralTexture = CreateTexture(widthScale, heightScale, noiseMap);
        segmentObject.AddComponent<MeshRenderer>().material = CreateMaterial(proceduralTexture);

        // Generate vertices and indices for this segment
        GenerateSegmentVerticesAndIndices(segment, vertices, indices, noiseMap);

        // Calculate UVs for this segment
        CalculateSegmentUVs(uv);

        // Set mesh data for this segment
        segmentObject.AddComponent<MeshFilter>().mesh = GenerateMesh(vertices, uv, indices);

        return segmentObject;
    }

public void GenerateSegmentVerticesAndIndices(int segment, List<Vector3> vertices, int[] indices, float[,] noiseMap)
{
    float heightMultiplier = proceduralHaloChunks.heightMultiplier;

    int segmentXVertices = proceduralHaloChunks.segmentXVertices;
    int segmentYVertices = proceduralHaloChunks.segmentYVertices;
    float widthInMeters = proceduralHaloChunks.widthInMeters;
    float radiusInMeters = proceduralHaloChunks.radiusInMeters;
    ProceduralHaloChunks.HaloRenderOption renderOption = proceduralHaloChunks.renderOption;

    float segmentWidth = Mathf.PI * 2f / proceduralHaloChunks.CircleSegmentCount;
    float angleStep = segmentWidth / segmentXVertices;
    float startAngle = segment * segmentWidth;

    // Create vertices for the segment
    for (int i = 0; i <= segmentXVertices; i++)
    {
        float angle = startAngle + i * angleStep;
        for (int j = 0; j < segmentYVertices; j++)
        {
            float y = j * (widthInMeters / (segmentYVertices - 1));
            float noiseValue = noiseMap[i, j];
            float offset = noiseValue * heightMultiplier;

            // Move the vertex towards the center based on the noise value and height multiplier
            float adjustedRadius = radiusInMeters - offset;
            vertices.Add(new Vector3(Mathf.Cos(angle) * adjustedRadius, y, Mathf.Sin(angle) * adjustedRadius));
        }
    }

    // Create indices for the segment
    for (int i = 0; i < segmentXVertices; i++)
    {
        for (int j = 0; j < segmentYVertices - 1; j++)
        {
            int baseIndex = i * segmentYVertices + j;
            if (renderOption == ProceduralHaloChunks.HaloRenderOption.Inside)
            {
                // Reverse the order of indices for inside rendering
                indices[(i * (segmentYVertices - 1) + j) * 6 + 0] = baseIndex;
                indices[(i * (segmentYVertices - 1) + j) * 6 + 1] = baseIndex + segmentYVertices;
                indices[(i * (segmentYVertices - 1) + j) * 6 + 2] = baseIndex + 1;
                indices[(i * (segmentYVertices - 1) + j) * 6 + 3] = baseIndex + 1;
                indices[(i * (segmentYVertices - 1) + j) * 6 + 4] = baseIndex + segmentYVertices;
                indices[(i * (segmentYVertices - 1) + j) * 6 + 5] = baseIndex + segmentYVertices + 1;
            }
            else if (renderOption == ProceduralHaloChunks.HaloRenderOption.Outside)
            {
                // Default order for outside rendering
                indices[(i * (segmentYVertices - 1) + j) * 6 + 0] = baseIndex;
                indices[(i * (segmentYVertices - 1) + j) * 6 + 1] = baseIndex + 1;
                indices[(i * (segmentYVertices - 1) + j) * 6 + 2] = baseIndex + segmentYVertices;
                indices[(i * (segmentYVertices - 1) + j) * 6 + 3] = baseIndex + 1;
                indices[(i * (segmentYVertices - 1) + j) * 6 + 4] = baseIndex + segmentYVertices + 1;
                indices[(i * (segmentYVertices - 1) + j) * 6 + 5] = baseIndex + segmentYVertices;
            }
        }
    }
}

    public void CalculateSegmentUVs(Vector2[] uv)
    {
        int segmentXVertices = proceduralHaloChunks.segmentXVertices;
        int segmentYVertices = proceduralHaloChunks.segmentYVertices;

        // Set the UVs to cover the entire texture for each segment
        for (int i = 0; i <= segmentXVertices; i++)
        {
            float u = (float)i / segmentXVertices; // U coordinate ranges from 0 to 1
            for (int j = 0; j < segmentYVertices; j++)
            {
                float v = 1.0f - (float)j / (segmentYVertices - 1); // V coordinate ranges from 0 to 1
                uv[i * segmentYVertices + j] = new Vector2(u, v);
            }
        }
    }
    public void CalculateSegmentTiledUVs(int segment, Vector2[] uv, int circleSegmentCount)
    {
        int segmentXVertices = proceduralHaloChunks.segmentXVertices;
        int segmentYVertices = proceduralHaloChunks.segmentYVertices;
        float uvScaleX = proceduralHaloChunks.uvScaleX;

        float segmentRatio = (float)segment / circleSegmentCount;
        float nextSegmentRatio = (float)(segment + 1) / circleSegmentCount;

        for (int i = 0; i <= segmentXVertices; i++)
        {
            float t = (float)i / segmentXVertices;
            for (int j = 0; j < segmentYVertices; j++)
            {
                float v = 1.0f - (float)j / (segmentYVertices - 1); // Flip the V coordinate
                uv[i * segmentYVertices + j] = new Vector2(Mathf.Lerp(segmentRatio, nextSegmentRatio, t) * uvScaleX, v);
            }
        }
    }

    private Texture2D CreateTexture(float widthScale, float heightScale, float[,] noiseMap)
    {
        Debug.Log("Updating segment Texture...");

        Texture2D proceduralTexture = GenerateProceduralNoiseTexture(widthScale, heightScale, noiseMap);
        proceduralTexture.wrapMode = TextureWrapMode.Clamp;
        proceduralTexture.filterMode = FilterMode.Bilinear;

        // Save the texture for visualization
        if (proceduralHaloChunks.saveTexturesFiles)
        {
            SaveTextureAsPNG(proceduralTexture, "HaloSegmentTexture_" + segment);
        }

        return proceduralTexture;
    }

    private Material CreateMaterial(Texture2D proceduralTexture)
    {
        Debug.Log("Updating segment material...");

        // Create a new material instance for this segment
        Material newMaterial = new Material(Shader.Find("Standard"));
        newMaterial.mainTexture = proceduralTexture;

        return newMaterial;
    }

    private Mesh GenerateMesh(List<Vector3> vertices, Vector2[] uv, int[] indices)
    {
        Debug.Log("Updating segment mesh...");

        Mesh segmentMesh = new Mesh { name = "Procedural Halo Segment" };
        segmentMesh.SetVertices(vertices);
        segmentMesh.SetUVs(0, uv);
        segmentMesh.SetIndices(indices, MeshTopology.Triangles, 0);
        segmentMesh.RecalculateBounds();
        segmentMesh.RecalculateNormals();
        return segmentMesh;
    }

    private void SaveTextureAsPNG(Texture2D texture, string fileName)
    {
        byte[] bytes = texture.EncodeToPNG();
        string filePath = Application.dataPath + "/ProceduralTextures/" + fileName + ".png";

        // Ensure the directory exists
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));

        // Write the file
        System.IO.File.WriteAllBytes(filePath, bytes);

        Debug.Log("Texture saved to: " + filePath);
    }

    // Modified method to generate a procedural texture using segmentXVertices and segmentYVertices
    private float[,] GenerateNoiseMap(float widthScale, float heightScale)
    {
        int seed = proceduralHaloChunks.seed;
        float scale = proceduralHaloChunks.noiseScale;
        int octaves = proceduralHaloChunks.octaves;
        float persistance = proceduralHaloChunks.persistance;
        float lacunarity = proceduralHaloChunks.lacunarity;

        // TODO: Offset is not working correctly and overlaps between segments
        var xOffset = segment * widthScale;
        Vector2 offset = new Vector2(xOffset, 0);

        Debug.Log("widthScale: " + widthScale);
        Debug.Log("heightScale: " + heightScale);
        Debug.Log("scaled xOffset: " + xOffset);

        int mapWidth = Mathf.RoundToInt(widthScale) + 1;
        var mapHeight = Mathf.RoundToInt(heightScale) + 1;

        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, scale, octaves, persistance, lacunarity, offset);

        return noiseMap;
    }

    // Modified method to generate a procedural texture using segmentXVertices and segmentYVertices
    private Texture2D GenerateProceduralNoiseTexture(float widthScale, float heightScale, float[,] noiseMap)
    {
        int mapWidth = Mathf.RoundToInt(widthScale) + 1;
        var mapHeight = Mathf.RoundToInt(heightScale) + 1;

        Color[] colourMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < proceduralHaloChunks.regions.Length; i++)
                {
                    if (currentHeight <= proceduralHaloChunks.regions[i].height)
                    {
                        colourMap[y * mapWidth + x] = proceduralHaloChunks.regions[i].colour;
                        break;
                    }
                }
            }
        }

        //var proceduralTexture = TextureGenerator.TextureFromHeightMap(noiseMap);
        var proceduralTexture = TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight);

        return proceduralTexture;
    }
}