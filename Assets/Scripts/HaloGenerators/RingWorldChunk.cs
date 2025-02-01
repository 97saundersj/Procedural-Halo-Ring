using System.Collections.Generic;
using UnityEngine;

public class RingWorldChunk : MonoBehaviour
{
    public RingWorldGenerator ringWorldGenerator;
    public GameObject parentObject;
    public int numberOfCircumferenceChunks;
    public int circumferenceChunkIndex;
    public int numberOfWidthChunks;
    public int widthChunkIndex;
    public int levelOfDetail;
    public int meshLevelOfDetail;

    public Dictionary<Vector3, float> vertexNoiseMap;

    public RingWorldChunk(RingWorldGenerator proceduralHaloChunks, GameObject parentObject, int numberOfCircumferenceChunks, int circumferenceChunkIndex, 
        int numberOfWidthChunks, int widthChunkIndex, int levelOfDetail, int meshLevelOfDetail)
    {
        this.ringWorldGenerator = proceduralHaloChunks;
        this.parentObject = parentObject;
        this.numberOfCircumferenceChunks = numberOfCircumferenceChunks;
        this.circumferenceChunkIndex = circumferenceChunkIndex;
        this.numberOfWidthChunks = numberOfWidthChunks;
        this.widthChunkIndex = widthChunkIndex;
        this.levelOfDetail = levelOfDetail;
        this.meshLevelOfDetail = meshLevelOfDetail;
    }

    public void GenerateChunk(GameObject segmentObject, int segmentIndexCount)
    {
        segmentObject.transform.SetParent(parentObject.transform, false);

        // Add this RingWorldChunk script to the segmentObject if it doesn't already exist
        RingWorldChunk haloSegment = segmentObject.GetComponent<RingWorldChunk>();
        if (haloSegment == null)
        {
            haloSegment = segmentObject.AddComponent<RingWorldChunk>();
        }

        haloSegment.ringWorldGenerator = this.ringWorldGenerator;
        haloSegment.parentObject = this.parentObject;
        haloSegment.circumferenceChunkIndex = this.circumferenceChunkIndex;
        haloSegment.numberOfCircumferenceChunks = this.numberOfCircumferenceChunks;
        haloSegment.levelOfDetail = this.levelOfDetail;
        haloSegment.meshLevelOfDetail = this.meshLevelOfDetail;

        int detailFactor = Mathf.Max(1, (int)Mathf.Pow(2, meshLevelOfDetail));
        int segmentXVertices = ringWorldGenerator.segmentXVertices / detailFactor;
        int segmentYVertices = ringWorldGenerator.segmentYVertices / detailFactor;

        // Calculate the correct number of vertices
        int totalVertices = (segmentXVertices + 1) * segmentYVertices;

        var vertices = new List<Vector3>(totalVertices);
        var uv = new Vector2[totalVertices];
        var indices = new int[segmentIndexCount];

        var segmentWidth = (2 * Mathf.PI * ringWorldGenerator.radiusInMeters) / numberOfCircumferenceChunks;

        float widthScale = segmentWidth / ringWorldGenerator.textureMetersPerPixel;
        float heightScale = ringWorldGenerator.widthInMeters / ringWorldGenerator.textureMetersPerPixel;

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

        // Generate vertices and indices for this circumferenceChunkIndex
        GenerateSegmentVertices(circumferenceChunkIndex, vertices, noiseMap);
        GenerateSegmentIndices(indices);

        // Calculate UVs for this circumferenceChunkIndex
        CalculateSegmentUVs(uv, segmentXVertices, segmentYVertices);

        // Set mesh data for this circumferenceChunkIndex
        Mesh segmentMesh = GenerateMesh(vertices, uv, indices);

        // Check if a MeshFilter already exists
        MeshFilter meshFilter = segmentObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            // Add a new MeshFilter if it doesn't exist
            meshFilter = segmentObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = segmentMesh;

        // Rotate the circumferenceChunkIndex object
        segmentObject.transform.rotation = Quaternion.Euler(0, 0, 90);

        // Add a MeshCollider to the circumferenceChunkIndex
        if (meshLevelOfDetail == ringWorldGenerator.maxMeshLevelOfDetail)
        {
            MeshCollider meshCollider = segmentObject.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                meshCollider = segmentObject.AddComponent<MeshCollider>();
            }
            meshCollider.sharedMesh = segmentMesh;
        }

        if (ringWorldGenerator.saveTexturesFiles)
        {
            CreateTexture(widthScale, heightScale, noiseMap, false);
        }

        SpawnObjectsOnSurface(segmentObject, vertices, ringWorldGenerator.regions);
    }

    // New method to split a chunk into two new chunks
    public void SplitChunk()
    {
        // Determine new chunk positions
        int newChunkIndex1 = circumferenceChunkIndex*2;//circumferenceChunkIndex * 2; // Example logic for new chunk index
        int newChunkIndex2 = (circumferenceChunkIndex*2)+1;//circumferenceChunkIndex * 2 + 1;

        // Create two new GameObjects for the new chunks
        GameObject newChunk1 = new GameObject(newChunkIndex1.ToString());
        GameObject newChunk2 = new GameObject(newChunkIndex2.ToString());

        // Set the parent of the new chunks
        newChunk1.transform.SetParent(parentObject.transform, false);
        newChunk2.transform.SetParent(parentObject.transform, false);

        // Generate and assign RingWorldChunk components to the new chunks
        RingWorldChunk haloSegment1 = newChunk1.AddComponent<RingWorldChunk>();
        RingWorldChunk haloSegment2 = newChunk2.AddComponent<RingWorldChunk>();

        // Copy relevant properties from the current chunk to the new chunks
        haloSegment1.ringWorldGenerator = this.ringWorldGenerator;
        haloSegment1.parentObject = this.parentObject;
        haloSegment1.numberOfCircumferenceChunks = this.numberOfCircumferenceChunks * 2;
        haloSegment1.circumferenceChunkIndex = newChunkIndex1;
        haloSegment1.levelOfDetail = this.levelOfDetail;
        haloSegment1.meshLevelOfDetail = this.meshLevelOfDetail - 1;

        haloSegment2.ringWorldGenerator = this.ringWorldGenerator;
        haloSegment2.parentObject = this.parentObject;
        haloSegment2.numberOfCircumferenceChunks = this.numberOfCircumferenceChunks * 2;
        haloSegment2.circumferenceChunkIndex = newChunkIndex2;
        haloSegment2.levelOfDetail = this.levelOfDetail;
        haloSegment2.meshLevelOfDetail = this.meshLevelOfDetail - 1;

        int segmentIndexCount = ringWorldGenerator.segmentXVertices * (ringWorldGenerator.segmentYVertices - 1) * 6;

        // You may also need to update mesh data, noise maps, etc. for each chunk.
        haloSegment1.GenerateChunk(newChunk1, segmentIndexCount);
        haloSegment2.GenerateChunk(newChunk2, segmentIndexCount);

        ringWorldGenerator.createdSegments.Add(newChunk1);
        ringWorldGenerator.createdSegments.Add(newChunk2);
        // Check if in edit mode and use DestroyImmediate if true
        if (Application.isEditor && !Application.isPlaying)
        {
            DestroyImmediate(gameObject); // Use DestroyImmediate for edit mode
        }
        else
        {
            Destroy(gameObject); // Use Destroy during runtime
        }
        ringWorldGenerator.createdSegments.Remove(gameObject);
    }

    public void GenerateSegmentVertices(int chunkIndex, List<Vector3> vertices, float[,] noiseMap)
    {
        vertexNoiseMap = new Dictionary<Vector3, float>();

        float heightMultiplier = ringWorldGenerator.meshHeightMultiplier;
        int detailFactor = Mathf.Max(1, (int)Mathf.Pow(2, meshLevelOfDetail));
        int segmentXVertices = ringWorldGenerator.segmentXVertices / detailFactor;
        int segmentYVertices = ringWorldGenerator.segmentYVertices / detailFactor;
        float widthInMeters = ringWorldGenerator.widthInMeters;
        float radiusInMeters = ringWorldGenerator.radiusInMeters;

        float segmentWidth = Mathf.PI * 2f / numberOfCircumferenceChunks;
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
        int segmentXVertices = ringWorldGenerator.segmentXVertices / detailFactor;
        int segmentYVertices = ringWorldGenerator.segmentYVertices / detailFactor;

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
        //Debug.Log("Updating circumferenceChunkIndex Texture...");

        Texture2D proceduralTexture = GenerateProceduralNoiseTexture(widthScale, heightScale, noiseMap, createHeightMap);
        proceduralTexture.wrapMode = TextureWrapMode.Clamp;
        proceduralTexture.filterMode = FilterMode.Bilinear;

        // Save the texture for visualization
        if (ringWorldGenerator.saveTexturesFiles)
        {
            SaveTextureAsPNG(proceduralTexture, "HaloSegmentTexture_" + (createHeightMap ? "HeightMap_" : "") + circumferenceChunkIndex);
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

        // Extract colors and height percentages from ringWorldGenerator.regions
        int baseColourCount = ringWorldGenerator.regions.Length;
        Color[] baseColours = new Color[baseColourCount];
        float[] baseStartHeights = new float[baseColourCount];

        for (int i = 0; i < baseColourCount; i++)
        {
            baseColours[i] = ringWorldGenerator.regions[i].colour;
            baseStartHeights[i] = ringWorldGenerator.regions[i].height; // Assuming 'heightPercentage' is defined
        }

        // Assign shader properties
        newMaterial.SetInt("baseColourCount", baseColourCount);
        newMaterial.SetColorArray("baseColours", baseColours);
        newMaterial.SetFloatArray("baseStartHeights", baseStartHeights);

        // Set the center point for distance calculation
        newMaterial.SetVector("_Center", gameobject.transform.position);

        // Set the min and max radius
        newMaterial.SetFloat("_MinRadius", ringWorldGenerator.radiusInMeters);
        newMaterial.SetFloat("_MaxRadius", ringWorldGenerator.radiusInMeters - (ringWorldGenerator.meshHeightMultiplier / 2));

        // Set the blend strength
        newMaterial.SetFloat("_BlendStrength", ringWorldGenerator.regionBlendStrength); // Assuming 'blendStrength' is defined in ringWorldGenerator

        newMaterial.SetFloat("_TextureScale", ringWorldGenerator.regionTextureScale);

        for (int i = 0; i < baseColourCount; i++)
        {
            newMaterial.SetTexture($"_Texture{i}", ringWorldGenerator.regions[i].texture);
        }

        return newMaterial;
    }

    private Mesh GenerateMesh(List<Vector3> vertices, Vector2[] uv, int[] indices)
    {
        Debug.Log("Updating circumferenceChunkIndex mesh...");

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

        int seed = ringWorldGenerator.seed;
        float scale = ringWorldGenerator.noiseScale;
        int octaves = ringWorldGenerator.octaves;
        float persistance = ringWorldGenerator.persistance;
        float lacunarity = ringWorldGenerator.lacunarity;
        Vector2 offset = new(circumferenceChunkIndex * widthScale, 0);

        return Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, scale, octaves, persistance, lacunarity, offset, ringWorldGenerator.heightCurve, ringWorldGenerator.heightMultiplier);
    }

    // Modified method to generate a procedural texture using segmentXVertices and segmentYVertices
    private Texture2D GenerateProceduralNoiseTexture(float widthScale, float heightScale, float[,] noiseMap, bool createHeightMap)
    {
        // Adjust the resolution based on levelOfDetail
        int detailFactor = Mathf.Max(1, (int)Mathf.Pow(2, levelOfDetail));
        int mapWidth = noiseMap.GetLength(0) / detailFactor;
        int mapHeight = noiseMap.GetLength(1) / detailFactor;

        Color[] colourMap = new Color[mapWidth * mapHeight];
        Texture2D[] regionTextures = new Texture2D[ringWorldGenerator.regions.Length];

        // Load or assign textures for each region
        for (int i = 0; i < ringWorldGenerator.regions.Length; i++)
        {
            regionTextures[i] = ringWorldGenerator.regions[i].texture; // Assuming each region has a 'texture' property
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x * detailFactor, y * detailFactor];
                for (int i = 0; i < ringWorldGenerator.regions.Length; i++)
                {
                    if (currentHeight <= ringWorldGenerator.regions[i].height)
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
        if (meshLevelOfDetail != ringWorldGenerator.maxMeshLevelOfDetail)
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