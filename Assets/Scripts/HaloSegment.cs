using System.Collections.Generic;
using UnityEngine;

public class HaloSegment
{
    private ProceduralHaloChunks proceduralHaloChunks;

    public HaloSegment(ProceduralHaloChunks proceduralHaloChunks)
    {
        this.proceduralHaloChunks = proceduralHaloChunks;
    }

    public GameObject GenerateSegment(int CircleSegmentCount, int segment, int segmentIndexCount, int segmentVertexCount)
    {
        var vertices = new List<Vector3>(segmentVertexCount);
        var uv = new Vector2[segmentVertexCount];
        var indices = new int[segmentIndexCount];

        // Generate vertices and indices for this segment
        GenerateSegmentVerticesAndIndices(segment, vertices, indices, CircleSegmentCount);
        // Calculate UVs for this segment
        CalculateSegmentUVs(segment, uv, CircleSegmentCount);
        // Set mesh data for this segment
        return CreateSegmentMesh(segment, vertices, uv, indices);
    }

    public void GenerateSegmentVerticesAndIndices(int segment, List<Vector3> vertices, int[] indices, int circleSegmentCount)
    {
        int segmentXVertices = proceduralHaloChunks.segmentXVertices;
        int segmentYVertices = proceduralHaloChunks.segmentYVertices;
        float widthInMeters = proceduralHaloChunks.widthInMeters;
        float radiusInMeters = proceduralHaloChunks.radiusInMeters;
        ProceduralHaloChunks.HaloRenderOption renderOption = proceduralHaloChunks.renderOption;

        float segmentWidth = Mathf.PI * 2f / circleSegmentCount;
        float angleStep = segmentWidth / segmentXVertices;
        float startAngle = segment * segmentWidth;

        // Create vertices for the segment
        for (int i = 0; i <= segmentXVertices; i++)
        {
            float angle = startAngle + i * angleStep;
            for (int j = 0; j < segmentYVertices; j++)
            {
                float y = j * (widthInMeters / (segmentYVertices - 1));
                vertices.Add(new Vector3(Mathf.Cos(angle) * radiusInMeters, y, Mathf.Sin(angle) * radiusInMeters));
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

    public void CalculateSegmentUVs(int segment, Vector2[] uv, int circleSegmentCount)
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

    private GameObject CreateSegmentMesh(int segment, List<Vector3> vertices, Vector2[] uv, int[] indices)
    {
        int segmentXVertices = proceduralHaloChunks.segmentXVertices;
        int segmentYVertices = proceduralHaloChunks.segmentYVertices;

        Debug.Log("Updating segment mesh...");
        Mesh segmentMesh = new Mesh { name = "Procedural Halo Segment" };
        segmentMesh.SetVertices(vertices);
        segmentMesh.SetUVs(0, uv);
        segmentMesh.SetIndices(indices, MeshTopology.Triangles, 0);
        segmentMesh.RecalculateBounds();
        segmentMesh.RecalculateNormals();

        // Assign the segment mesh to a new GameObject
        GameObject segmentObject = new GameObject("HaloSegment");
        segmentObject.AddComponent<MeshFilter>().mesh = segmentMesh;
        MeshRenderer meshRenderer = segmentObject.AddComponent<MeshRenderer>();

        // Apply the procedural texture to the new material
        Texture2D proceduralTexture = GenerateProceduralNoiseTexture(segmentXVertices, segmentYVertices);

        // Save the texture for visualization
        if (proceduralHaloChunks.saveTexturesFiles)
        {
            SaveTextureAsPNG(proceduralTexture, "HaloSegmentTexture_" + segment);
        }

        // Create a new material instance for this segment
        Material newMaterial = new Material(Shader.Find("Standard"));
        newMaterial.mainTexture = proceduralTexture;
        meshRenderer.material = newMaterial;

        return segmentObject;
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
    private Texture2D GenerateProceduralNoiseTexture(int segmentXVertices, int segmentYVertices)
    {
        int widthScale = proceduralHaloChunks.widthScale;
        int heightScale = proceduralHaloChunks.heightScale;
        int seed = proceduralHaloChunks.seed;
        float scale = proceduralHaloChunks.noiseScale;

        var mapWidth = (widthScale * segmentXVertices) + 1;
        var mapHeight = (heightScale * segmentYVertices) + 1;

        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, scale, proceduralHaloChunks.octaves, proceduralHaloChunks.persistance, proceduralHaloChunks.lacunarity, new Vector2(0, 0));

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

        proceduralTexture.wrapMode = TextureWrapMode.Repeat;
        proceduralTexture.filterMode = FilterMode.Bilinear;

        return proceduralTexture;
    }
}