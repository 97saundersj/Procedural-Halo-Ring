using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HaloSegment : MonoBehaviour
{
    private ProceduralHaloChunks proceduralHaloChunks;
    private int segment;
    public int levelOfDetail;
    public int meshLevelOfDetail;

    public HaloSegment(ProceduralHaloChunks proceduralHaloChunks, int segment, int levelOfDetail, int meshLevelOfDetail)
    {
        this.proceduralHaloChunks = proceduralHaloChunks;
        this.segment = segment;
        this.levelOfDetail = levelOfDetail;
        this.meshLevelOfDetail = meshLevelOfDetail;
    }

    public void GenerateChunk(GameObject segmentObject, int segmentIndexCount, int segmentVertexCount)
    {
        int detailFactor = Mathf.Max(1, (int)Mathf.Pow(2, meshLevelOfDetail));
        int segmentXVertices = proceduralHaloChunks.segmentXVertices / detailFactor;
        int segmentYVertices = proceduralHaloChunks.segmentYVertices / detailFactor;

        // Calculate the correct number of vertices
        int totalVertices = (segmentXVertices + 1) * segmentYVertices;

        var vertices = new List<Vector3>(totalVertices);
        var uv = new Vector2[totalVertices];
        var indices = new int[segmentIndexCount];

        var segmentWidth = (2 * Mathf.PI * proceduralHaloChunks.radiusInMeters) / proceduralHaloChunks.CircleSegmentCount;

        float widthScale = segmentWidth / proceduralHaloChunks.textureMetersPerPixel;
        float heightScale = proceduralHaloChunks.widthInMeters / proceduralHaloChunks.textureMetersPerPixel;

        float[,] noiseMap = GenerateNoiseMap(widthScale, heightScale);

        // Generate Procedural Texture
        //var proceduralTexture = CreateTexture(widthScale, heightScale, noiseMap, false);

        // Check if a MeshRenderer already exists
        MeshRenderer meshRenderer = segmentObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            // Add a new MeshRenderer if it doesn't exist
            meshRenderer = segmentObject.AddComponent<MeshRenderer>();
        }
        // Update the material of the MeshRenderer
        meshRenderer.material = CreateMaterial(noiseMap, null, segmentObject);

        // Generate vertices and indices for this segment
        GenerateSegmentVertices(segment, vertices, noiseMap);
        GenerateSegmentIndices(indices);

        // Calculate UVs for this segment
        CalculateSegmentUVs(uv, segmentXVertices, segmentYVertices);

        // Set mesh data for this segment
        Mesh segmentMesh = GenerateMesh(vertices, uv, indices);

        // Check if a MeshFilter already exists
        MeshFilter meshFilter = segmentObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            // Add a new MeshFilter if it doesn't exist
            meshFilter = segmentObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = segmentMesh;

        // Add a MeshCollider to the segment
        if (meshLevelOfDetail == proceduralHaloChunks.maxMeshLevelOfDetail)
        {
            MeshCollider meshCollider = segmentObject.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                meshCollider = segmentObject.AddComponent<MeshCollider>();
            }
            meshCollider.sharedMesh = segmentMesh;
        }
    }

    public void GenerateSegmentVertices(int segment, List<Vector3> vertices, float[,] noiseMap)
    {
        float heightMultiplier = proceduralHaloChunks.meshHeightMultiplier;

        int detailFactor = Mathf.Max(1, (int)Mathf.Pow(2, meshLevelOfDetail));
        int segmentXVertices = proceduralHaloChunks.segmentXVertices / detailFactor;
        int segmentYVertices = proceduralHaloChunks.segmentYVertices / detailFactor;
        float widthInMeters = proceduralHaloChunks.widthInMeters;
        float radiusInMeters = proceduralHaloChunks.radiusInMeters;

        float segmentWidth = Mathf.PI * 2f / proceduralHaloChunks.CircleSegmentCount;
        float angleStep = segmentWidth / segmentXVertices;
        float startAngle = segment * segmentWidth;

        int noiseMapWidth = noiseMap.GetLength(0);
        int noiseMapHeight = noiseMap.GetLength(1);

        for (int x = 0; x <= segmentXVertices; x++)
        {
            float angle = startAngle + x * angleStep;
            for (int y = 0; y < segmentYVertices; y++)
            {
                float width = y * (widthInMeters / (segmentYVertices - 1));

                int noiseX = noiseMapHeight - 1 - (int)((float)y / segmentYVertices * (noiseMapHeight - 1));
                int noiseY = (int)((float)x / segmentXVertices * (noiseMapWidth - 1));

                float noiseValue = noiseMap[noiseY, noiseX];
                float adjustedRadius = radiusInMeters - heightMultiplier * noiseValue;

                vertices.Add(new Vector3(Mathf.Cos(angle) * adjustedRadius, width, Mathf.Sin(angle) * adjustedRadius));
            }
        }
    }

    public void GenerateSegmentIndices(int[] indices)
    {
        int detailFactor = Mathf.Max(1, (int)Mathf.Pow(2, meshLevelOfDetail));
        int segmentXVertices = proceduralHaloChunks.segmentXVertices / detailFactor;
        int segmentYVertices = proceduralHaloChunks.segmentYVertices / detailFactor;

        for (int x = 0; x < segmentXVertices; x++)
        {
            for (int y = 0; y < segmentYVertices - 1; y++)
            {
                int baseIndex = x * segmentYVertices + y;
                indices[(x * (segmentYVertices - 1) + y) * 6 + 0] = baseIndex;
                indices[(x * (segmentYVertices - 1) + y) * 6 + 1] = baseIndex + segmentYVertices;
                indices[(x * (segmentYVertices - 1) + y) * 6 + 2] = baseIndex + 1;
                indices[(x * (segmentYVertices - 1) + y) * 6 + 3] = baseIndex + 1;
                indices[(x * (segmentYVertices - 1) + y) * 6 + 4] = baseIndex + segmentYVertices;
                indices[(x * (segmentYVertices - 1) + y) * 6 + 5] = baseIndex + segmentYVertices + 1;
            }
        }
    }

    public void CalculateSegmentUVs(Vector2[] uv, int segmentXVertices, int segmentYVertices)
    {
        for (int i = 0; i <= segmentXVertices; i++)
        {
            float u = (float)i / segmentXVertices;
            for (int j = 0; j < segmentYVertices; j++)
            {
                float v = 1.0f - (float)j / (segmentYVertices - 1);
                uv[i * segmentYVertices + j] = new Vector2(u, v);
            }
        }
    }

    private Texture2D CreateTexture(float widthScale, float heightScale, float[,] noiseMap, bool createHeightMap)
    {
        //Debug.Log("Updating segment Texture...");

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

    private Material CreateMaterial(float[,] heightMap, Texture2D proceduralTexture, GameObject gameobject)
    {
        // Create a new material instance using the custom shader
        Material newMaterial = new Material(Shader.Find("Custom/Terrain"));
        //Material newMaterial = new Material(Shader.Find("Standard"));
        //newMaterial.mainTexture = proceduralTexture;

        // Extract colors and height percentages from proceduralHaloChunks.regions
        int baseColourCount = proceduralHaloChunks.regions.Length;
        Color[] baseColours = new Color[baseColourCount];
        float[] baseStartHeights = new float[baseColourCount];

        for (int i = 0; i < baseColourCount; i++)
        {
            baseColours[i] = proceduralHaloChunks.regions[i].colour;
            baseStartHeights[i] = proceduralHaloChunks.regions[i].height; // Assuming 'heightPercentage' is defined
        }

        // Assign shader properties
        newMaterial.SetInt("baseColourCount", baseColourCount);
        newMaterial.SetColorArray("baseColours", baseColours);
        newMaterial.SetFloatArray("baseStartHeights", baseStartHeights);

        // Set the center point for distance calculation
        Vector3 centerPoint = new Vector3(0,0,0); // Assuming 'centerPoint' is defined
        newMaterial.SetVector("_Center", centerPoint);

        // Set the min and max radius
        newMaterial.SetFloat("_MinRadius", proceduralHaloChunks.radiusInMeters);
        newMaterial.SetFloat("_MaxRadius", proceduralHaloChunks.radiusInMeters - (proceduralHaloChunks.meshHeightMultiplier/2));

        // Set the blend strength
        newMaterial.SetFloat("_BlendStrength", proceduralHaloChunks.regionBlendStrength); // Assuming 'blendStrength' is defined in proceduralHaloChunks

        // Set the main texture for triplanar mapping
        newMaterial.SetTexture("_MainTex", proceduralHaloChunks.testTexture);

        for (int i = 0; i < baseColourCount; i++)
        {
            newMaterial.SetTexture($"_Texture{i}", proceduralHaloChunks.regions[i].texture);
        }

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

        return Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, scale, octaves, persistance, lacunarity, offset, proceduralHaloChunks.heightCurve, proceduralHaloChunks.heightMultiplier);
    }

    // Modified method to generate a procedural texture using segmentXVertices and segmentYVertices
    private Texture2D GenerateProceduralNoiseTexture(float widthScale, float heightScale, float[,] noiseMap, bool createHeightMap)
    {
        // Adjust the resolution based on levelOfDetail
        int detailFactor = Mathf.Max(1, (int)Mathf.Pow(2, levelOfDetail));
        int mapWidth = noiseMap.GetLength(0) / detailFactor;
        int mapHeight = noiseMap.GetLength(1) / detailFactor;

        Color[] colourMap = new Color[mapWidth * mapHeight];
        Texture2D[] regionTextures = new Texture2D[proceduralHaloChunks.regions.Length];

        // Load or assign textures for each region
        for (int i = 0; i < proceduralHaloChunks.regions.Length; i++)
        {
            regionTextures[i] = proceduralHaloChunks.regions[i].texture; // Assuming each region has a 'texture' property
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x * detailFactor, y * detailFactor];
                for (int i = 0; i < proceduralHaloChunks.regions.Length; i++)
                {
                    if (currentHeight <= proceduralHaloChunks.regions[i].height)
                    {
                        // Check if the texture is not null before using it
                        if (regionTextures[i] != null)
                        {
                            // Apply the texture of the region
                            Color textureColor = regionTextures[i].GetPixelBilinear((float)x / mapWidth, (float)y / mapHeight);
                            colourMap[y * mapWidth + x] = textureColor;
                        }
                        else
                        {
                            Debug.LogWarning($"Texture for region {i} is null. Using default color.");
                            colourMap[y * mapWidth + x] = Color.white; // or any default color
                        }
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