using System.Collections.Generic;
using UnityEngine;

public class HaloSegment : MonoBehaviour
{
    public ProceduralHaloChunks proceduralHaloChunks;
    public GameObject parentObject;
    public int circleSegmentCount;
    public int chunkIndex;
    public int levelOfDetail;
    public int meshLevelOfDetail;

    public Dictionary<Vector3, float> vertexNoiseMap;

    public HaloSegment(ProceduralHaloChunks proceduralHaloChunks, GameObject parentObject, int circleSegmentCount, int chunkIndex, int levelOfDetail, int meshLevelOfDetail)
    {
        this.proceduralHaloChunks = proceduralHaloChunks;
        this.parentObject = parentObject;
        this.circleSegmentCount = circleSegmentCount;
        this.chunkIndex = chunkIndex;
        this.levelOfDetail = levelOfDetail;
        this.meshLevelOfDetail = meshLevelOfDetail;
    }

    public void GenerateChunk(GameObject segmentObject, int segmentIndexCount, int segmentVertexCount)
    {
        segmentObject.transform.SetParent(parentObject.transform, false);

        // Add this HaloSegment script to the segmentObject if it doesn't already exist
        HaloSegment haloSegment = segmentObject.GetComponent<HaloSegment>();
        if (haloSegment == null)
        {
            haloSegment = segmentObject.AddComponent<HaloSegment>();
        }

        haloSegment.proceduralHaloChunks = this.proceduralHaloChunks;
        haloSegment.parentObject = this.parentObject;
        haloSegment.chunkIndex = this.chunkIndex;
        haloSegment.circleSegmentCount = this.circleSegmentCount;
        haloSegment.levelOfDetail = this.levelOfDetail;
        haloSegment.meshLevelOfDetail = this.meshLevelOfDetail;

        int detailFactor = Mathf.Max(1, (int)Mathf.Pow(2, meshLevelOfDetail));
        int segmentXVertices = proceduralHaloChunks.segmentXVertices / detailFactor;
        int segmentYVertices = proceduralHaloChunks.segmentYVertices / detailFactor;

        // Calculate the correct number of vertices
        int totalVertices = (segmentXVertices + 1) * segmentYVertices;

        var vertices = new List<Vector3>(totalVertices);
        var uv = new Vector2[totalVertices];
        var indices = new int[segmentIndexCount];

        var segmentWidth = (2 * Mathf.PI * proceduralHaloChunks.radiusInMeters) / circleSegmentCount;

        float widthScale = segmentWidth / proceduralHaloChunks.textureMetersPerPixel;
        float heightScale = proceduralHaloChunks.widthInMeters / proceduralHaloChunks.textureMetersPerPixel;

        float[,] noiseMap = GenerateNoiseMap(widthScale, heightScale);

        // Check if a MeshRenderer already exists
        MeshRenderer meshRenderer = segmentObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            // Add a new MeshRenderer if it doesn't exist
            meshRenderer = segmentObject.AddComponent<MeshRenderer>();
        }
        // Update the material of the MeshRenderer
        meshRenderer.material = CreateMaterial(noiseMap, null, segmentObject);

        // Generate vertices and indices for this chunkIndex
        GenerateSegmentVertices(chunkIndex, vertices, noiseMap);
        GenerateSegmentIndices(indices);

        // Calculate UVs for this chunkIndex
        CalculateSegmentUVs(uv, segmentXVertices, segmentYVertices);

        // Set mesh data for this chunkIndex
        Mesh segmentMesh = GenerateMesh(vertices, uv, indices);

        // Check if a MeshFilter already exists
        MeshFilter meshFilter = segmentObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            // Add a new MeshFilter if it doesn't exist
            meshFilter = segmentObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = segmentMesh;

        // Rotate the chunkIndex object
        segmentObject.transform.rotation = Quaternion.Euler(0, 0, 90);

        // Add a MeshCollider to the chunkIndex
        if (meshLevelOfDetail == proceduralHaloChunks.maxMeshLevelOfDetail)
        {
            MeshCollider meshCollider = segmentObject.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                meshCollider = segmentObject.AddComponent<MeshCollider>();
            }
            meshCollider.sharedMesh = segmentMesh;
        }

        SpawnObjectsOnSurface(segmentObject, vertices, proceduralHaloChunks.regions);
    }

    // New method to split a chunk into two new chunks
    public void SplitChunk()
    {
        // Determine new chunk positions
        int newChunkIndex1 = chunkIndex*2;//chunkIndex * 2; // Example logic for new chunk index
        int newChunkIndex2 = (chunkIndex*2)+1;//chunkIndex * 2 + 1;

        // Create two new GameObjects for the new chunks
        GameObject newChunk1 = new GameObject(newChunkIndex1.ToString());
        GameObject newChunk2 = new GameObject(newChunkIndex2.ToString());

        // Set the parent of the new chunks
        newChunk1.transform.SetParent(parentObject.transform, false);
        newChunk2.transform.SetParent(parentObject.transform, false);

        // Generate and assign HaloSegment components to the new chunks
        HaloSegment haloSegment1 = newChunk1.AddComponent<HaloSegment>();
        HaloSegment haloSegment2 = newChunk2.AddComponent<HaloSegment>();

        // Copy relevant properties from the current chunk to the new chunks
        haloSegment1.proceduralHaloChunks = this.proceduralHaloChunks;
        haloSegment1.parentObject = this.parentObject;
        haloSegment1.circleSegmentCount = this.circleSegmentCount * 2;
        haloSegment1.chunkIndex = newChunkIndex1;
        haloSegment1.levelOfDetail = this.levelOfDetail;
        haloSegment1.meshLevelOfDetail = this.meshLevelOfDetail - 1;

        haloSegment2.proceduralHaloChunks = this.proceduralHaloChunks;
        haloSegment2.parentObject = this.parentObject;
        haloSegment2.circleSegmentCount = this.circleSegmentCount * 2;
        haloSegment2.chunkIndex = newChunkIndex2;
        haloSegment2.levelOfDetail = this.levelOfDetail;
        haloSegment2.meshLevelOfDetail = this.meshLevelOfDetail - 1;

        int segmentVertexCount = (proceduralHaloChunks.segmentXVertices + 1) * proceduralHaloChunks.segmentYVertices;
        int segmentIndexCount = proceduralHaloChunks.segmentXVertices * (proceduralHaloChunks.segmentYVertices - 1) * 6;

        // You may also need to update mesh data, noise maps, etc. for each chunk.
        haloSegment1.GenerateChunk(newChunk1, segmentIndexCount, segmentVertexCount);
        haloSegment2.GenerateChunk(newChunk2, segmentIndexCount, segmentVertexCount);

        proceduralHaloChunks.createdSegments.Add(newChunk1);
        proceduralHaloChunks.createdSegments.Add(newChunk2);
        // Check if in edit mode and use DestroyImmediate if true
        if (Application.isEditor && !Application.isPlaying)
        {
            DestroyImmediate(gameObject); // Use DestroyImmediate for edit mode
        }
        else
        {
            Destroy(gameObject); // Use Destroy during runtime
        }
        proceduralHaloChunks.createdSegments.Remove(gameObject);
    }

    public void GenerateSegmentVertices(int chunkIndex, List<Vector3> vertices, float[,] noiseMap)
    {
        vertexNoiseMap = new Dictionary<Vector3, float>();

        float heightMultiplier = proceduralHaloChunks.meshHeightMultiplier;
        int detailFactor = Mathf.Max(1, (int)Mathf.Pow(2, meshLevelOfDetail));
        int segmentXVertices = proceduralHaloChunks.segmentXVertices / detailFactor;
        int segmentYVertices = proceduralHaloChunks.segmentYVertices / detailFactor;
        float widthInMeters = proceduralHaloChunks.widthInMeters;
        float radiusInMeters = proceduralHaloChunks.radiusInMeters;

        float segmentWidth = Mathf.PI * 2f / circleSegmentCount;
        float angleStep = segmentWidth / segmentXVertices;
        float startAngle = chunkIndex * segmentWidth;

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

                Vector3 vertex = new Vector3(Mathf.Cos(angle) * adjustedRadius, width, Mathf.Sin(angle) * adjustedRadius);
                vertices.Add(vertex);

                // Store the vertex and its noise value in the dictionary
                vertexNoiseMap[vertex] = noiseValue;
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
        //Debug.Log("Updating chunkIndex Texture...");

        Texture2D proceduralTexture = GenerateProceduralNoiseTexture(widthScale, heightScale, noiseMap, createHeightMap);
        proceduralTexture.wrapMode = TextureWrapMode.Clamp;
        proceduralTexture.filterMode = FilterMode.Bilinear;

        // Save the texture for visualization
        if (proceduralHaloChunks.saveTexturesFiles)
        {
            SaveTextureAsPNG(proceduralTexture, "HaloSegmentTexture_" + (createHeightMap ? "HeightMap_" : "") + chunkIndex);
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
        newMaterial.SetVector("_Center", gameobject.transform.position);

        // Set the min and max radius
        newMaterial.SetFloat("_MinRadius", proceduralHaloChunks.radiusInMeters);
        newMaterial.SetFloat("_MaxRadius", proceduralHaloChunks.radiusInMeters - (proceduralHaloChunks.meshHeightMultiplier / 2));

        // Set the blend strength
        newMaterial.SetFloat("_BlendStrength", proceduralHaloChunks.regionBlendStrength); // Assuming 'blendStrength' is defined in proceduralHaloChunks

        newMaterial.SetFloat("_TextureScale", proceduralHaloChunks.regionTextureScale);

        for (int i = 0; i < baseColourCount; i++)
        {
            newMaterial.SetTexture($"_Texture{i}", proceduralHaloChunks.regions[i].texture);
        }

        return newMaterial;
    }

    private Mesh GenerateMesh(List<Vector3> vertices, Vector2[] uv, int[] indices)
    {
        Debug.Log("Updating chunkIndex mesh...");

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
        Vector2 offset = new(chunkIndex * widthScale, 0);

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

    public void SpawnObjectsOnSurface(GameObject parentObject, List<Vector3> vertices, TerrainType[] terrainTypes)
    {
        if (meshLevelOfDetail != proceduralHaloChunks.maxMeshLevelOfDetail)
        {
            return; // Only spawn objects at the highest mesh level of detail
        }

        foreach (var vertexNoise in vertexNoiseMap)
        {
            TerrainType? selectedTerrainType = null;
            float noiseValue = vertexNoise.Value;

            foreach (var terrainType in terrainTypes)
            {
                if (noiseValue <= terrainType.height)
                {
                    selectedTerrainType = terrainType;
                    break;
                }
            }

            if (!selectedTerrainType.HasValue)
            {
                continue; // No suitable terrain type found
            }

            Vector3 localVertexPosition = vertexNoise.Key;
            Vector3 worldVertexPosition = parentObject.transform.TransformPoint(localVertexPosition);

            foreach (var spawnableObject in selectedTerrainType.Value.objectsToSpawn)
            {
                // Determine if an object should be spawned based on density
                if (Random.value <= spawnableObject.density)
                {
                    // Instantiate the prefab at the vertex position
                    GameObject spawnedObject = Instantiate(spawnableObject.prefab, worldVertexPosition, Quaternion.identity);
                    spawnedObject.transform.SetParent(parentObject.transform);

                    // Calculate the normal at the vertex position
                    Vector3 normal = (worldVertexPosition - parentObject.transform.position).normalized;
                    spawnedObject.transform.up = normal;

                    // Randomly scale the object between minSize and maxSize
                    float randomScale = Random.Range(spawnableObject.minSize, spawnableObject.maxSize);
                    spawnedObject.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

                    if (spawnableObject.randomRotation)
                    {
                        float randomRotationX = Random.Range(0f, 360f);
                        float randomRotationY = Random.Range(0f, 360f);
                        float randomRotationZ = Random.Range(0f, 360f);
                        spawnedObject.transform.rotation = Quaternion.Euler(randomRotationX, randomRotationY, randomRotationZ);
                    }
                    else
                    {
                        // Randomly rotate the object on y
                        float randomRotation = Random.Range(0f, 360f);
                        spawnedObject.transform.rotation = Quaternion.Euler(0, randomRotation, 0);
                    }

                    break; // One object per vertex
                }
            }
        }
    }
}