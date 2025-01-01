using System.Collections.Generic;
using UnityEngine;

public class HaloSegment
{
    private int segmentXVertices;
    private int segmentYVertices;
    private float radiusInMeters;
    private float widthInMeters;
    private float uvScaleX;

    private int widthScale;
    private int heightScale;

    private ProceduralHaloChunks.HaloRenderOption renderOption;
    private Transform transform;

    public HaloSegment(int segmentXVertices, int segmentYVertices, float radiusInMeters, float widthInMeters, float uvScaleX, ProceduralHaloChunks.HaloRenderOption renderOption, int widthScale, int heightScale, Transform transform)
    {
        this.segmentXVertices = segmentXVertices;
        this.segmentYVertices = segmentYVertices;
        this.radiusInMeters = radiusInMeters;
        this.widthInMeters = widthInMeters;
        this.uvScaleX = uvScaleX;
        this.renderOption = renderOption;

        this.widthScale = widthScale;
        this.heightScale = heightScale;

        this.transform = transform;
    }

    public void GenerateSegment(int CircleSegmentCount, int segment, int segmentIndexCount, int segmentVertexCount)
    {
        var vertices = new List<Vector3>(segmentVertexCount);
        var uv = new Vector2[segmentVertexCount];
        var indices = new int[segmentIndexCount];

        // Generate vertices and indices for this segment
        GenerateSegmentVerticesAndIndices(segment, vertices, indices, CircleSegmentCount);
        // Calculate UVs for this segment
        CalculateSegmentUVs(segment, uv, CircleSegmentCount);
        // Set mesh data for this segment
        UpdateSegmentMesh(segment, vertices, uv, indices);
    }

    public void GenerateSegmentVerticesAndIndices(int segment, List<Vector3> vertices, int[] indices, int circleSegmentCount)
    {
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

    private void UpdateSegmentMesh(int segment, List<Vector3> vertices, Vector2[] uv, int[] indices)
    {
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
        proceduralTexture.wrapMode = TextureWrapMode.Repeat;
        proceduralTexture.filterMode = FilterMode.Bilinear;

        // Save the texture for visualization
        SaveTextureAsPNG(proceduralTexture, "HaloSegmentTexture_" + segment);

        // Create a new material instance for this segment
        Material newMaterial = new Material(Shader.Find("Standard"));
        newMaterial.mainTexture = proceduralTexture;
        meshRenderer.material = newMaterial;

        segmentObject.transform.SetParent(transform, false);
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
    private Texture2D GenerateProceduralTexture(int segmentXVertices, int segmentYVertices)
    {
        var mapWidth = (widthScale * segmentXVertices) + 1;
        var mapHeight = (heightScale * segmentYVertices) + 1;

        Texture2D texture = new Texture2D(segmentXVertices, mapHeight);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float xCoord = (float)x / mapWidth;
                float yCoord = (float)y / segmentYVertices;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                Color color = new Color(sample, sample, sample);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    // Modified method to generate a procedural texture using segmentXVertices and segmentYVertices
    private Texture2D GenerateProceduralNoiseTexture(int segmentXVertices, int segmentYVertices)
    {
        var mapWidth = (widthScale * segmentXVertices) + 1;
        var mapHeight = (heightScale * segmentXVertices) + 1;

        //var mapHeight = (heightScale * segmentYVertices) + 1;

        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, 1, 50, 8, 0.01f, 1, new Vector2(0, 0));

        return TextureGenerator.TextureFromHeightMap(noiseMap);
        /*
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
                
                // Generate a procedural texture using mapWidth and mapHeight
                Texture2D proceduralTexture = GenerateProceduralTexture(mapWidth, mapHeight);
                
                display.DrawMesh(meshData, proceduralTexture);
                break;
        }
        */
    }
}