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

        //Create HeightMap
        //var heightMap = CreateTexture(widthScale, heightScale, noiseMap, true);

        // Generate Procedural Texture
        var proceduralTexture = CreateTexture(widthScale, heightScale, noiseMap, false);
        segmentObject.AddComponent<MeshRenderer>().material = CreateMaterial(noiseMap, proceduralTexture);

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

        float segmentWidth = Mathf.PI * 2f / proceduralHaloChunks.CircleSegmentCount;
        float angleStep = segmentWidth / segmentXVertices;
        float startAngle = segment * segmentWidth;

        // Create vertices for the segment
        for (int i = 0; i <= segmentXVertices; i++)
        {
            float angle = startAngle + i * angleStep;
            for (int j = 0; j < segmentYVertices; j++)
            {
                float width = j * (widthInMeters / (segmentYVertices - 1));

                // We have to flip the noise for some reason, it might be my uvs are wrong
                float noiseValue = noiseMap[segmentXVertices - i, j];
                float offset = 0; //noiseValue * heightMultiplier;

                // Move the vertex towards the center based on the noise value and height multiplier
                float adjustedRadius = radiusInMeters - offset;
                vertices.Add(new Vector3(Mathf.Cos(angle) * adjustedRadius, width, Mathf.Sin(angle) * adjustedRadius));
            }
        }

        // Create indices for the segment
        for (int i = 0; i < segmentXVertices; i++)
        {
            for (int j = 0; j < segmentYVertices - 1; j++)
            {
                int baseIndex = i * segmentYVertices + j;
                // Reverse the order of indices for inside rendering
                indices[(i * (segmentYVertices - 1) + j) * 6 + 0] = baseIndex;
                indices[(i * (segmentYVertices - 1) + j) * 6 + 1] = baseIndex + segmentYVertices;
                indices[(i * (segmentYVertices - 1) + j) * 6 + 2] = baseIndex + 1;
                indices[(i * (segmentYVertices - 1) + j) * 6 + 3] = baseIndex + 1;
                indices[(i * (segmentYVertices - 1) + j) * 6 + 4] = baseIndex + segmentYVertices;
                indices[(i * (segmentYVertices - 1) + j) * 6 + 5] = baseIndex + segmentYVertices + 1;
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

    private Texture2D CreateTexture(float widthScale, float heightScale, float[,] noiseMap, bool createHeightMap)
    {
        Debug.Log("Updating segment Texture...");

        Texture2D proceduralTexture = GenerateProceduralNoiseTexture(widthScale, heightScale, noiseMap, createHeightMap);
        proceduralTexture.wrapMode = TextureWrapMode.Clamp;
        proceduralTexture.filterMode = FilterMode.Bilinear;

        // Save the texture for visualization
        if (proceduralHaloChunks.saveTexturesFiles)
        {
            SaveTextureAsPNG(proceduralTexture, "HaloSegmentTexture_" + (createHeightMap ? "HeightMap_" : "") + segment);
        }

        return proceduralTexture;
    }

    private Texture2D ConvertNoiseMapToNormalMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        Texture2D normalMap = new Texture2D(width, height, TextureFormat.RGBA32, false);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float xLeft = noiseMap[x - 1 < 0 ? x : x - 1, y];
                float xRight = noiseMap[x + 1 >= width ? x : x + 1, y];
                float yUp = noiseMap[x, y + 1 >= height ? y : y + 1];
                float yDown = noiseMap[x, y - 1 < 0 ? y : y - 1];

                float xDelta = (xRight - xLeft) * 0.5f;
                float yDelta = (yUp - yDown) * 0.5f;

                Vector3 normal = new Vector3(-xDelta, -yDelta, 1.0f).normalized;
                Color normalColor = new Color(normal.x * 0.5f + 0.5f, normal.y * 0.5f + 0.5f, normal.z * 0.5f + 0.5f);

                normalMap.SetPixel(x, y, normalColor);
            }
        }

        normalMap.Apply();
        return normalMap;
    }

    private Material CreateMaterial(float[,] heightMap, Texture2D proceduralTexture)
    {
        Debug.Log("Updating segment material...");

        // Create a new material instance for this segment
        Material newMaterial = new Material(Shader.Find("Standard"));
        newMaterial.mainTexture = proceduralTexture;

        //newMaterial.SetTexture("_HeightMap", heightMap);
        //newMaterial.SetFloat("_HeightScale", 0.1f); // Adjust the height scale as needed

        // Set the height map as the parallax map
        //newMaterial.SetTexture("_ParallaxMap", heightMap);
        //newMaterial.SetFloat("_Parallax", 0.02f); // Adjust the parallax scale as needed

        // Convert heightMap to a normal map
        Texture2D normalMap = ConvertNoiseMapToNormalMap(heightMap);

        // Set the bump map texture
        newMaterial.SetTexture("_BumpMap", normalMap);
        newMaterial.SetFloat("_BumpScale", 1f);
        newMaterial.EnableKeyword("_NORMALMAP");

        // Set the smoothness of the material
        newMaterial.SetFloat("_Glossiness", 0.2f);

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
        int mapWidth = (Mathf.RoundToInt(widthScale)) + 1;
        var mapHeight = (Mathf.RoundToInt(heightScale)) + 1;

        int seed = proceduralHaloChunks.seed;
        float scale = proceduralHaloChunks.noiseScale;
        int octaves = proceduralHaloChunks.octaves;
        float persistance = proceduralHaloChunks.persistance;
        float lacunarity = proceduralHaloChunks.lacunarity;
        Vector2 offset = new(segment * widthScale, 0);

        return Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, scale, octaves, persistance, lacunarity, offset, proceduralHaloChunks.meshHeightCurve, proceduralHaloChunks.heightMultiplier);
    }

    // Modified method to generate a procedural texture using segmentXVertices and segmentYVertices
    private Texture2D GenerateProceduralNoiseTexture(float widthScale, float heightScale, float[,] noiseMap, bool createHeightMap)
    {
        int mapWidth = noiseMap.GetLength(0);
        var mapHeight = noiseMap.GetLength(1);

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

        if (createHeightMap)
        {
            return TextureGenerator.TextureFromHeightMap(noiseMap);
        }
        else
        {
            return TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight);
        }
    }
}