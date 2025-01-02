using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProceduralHaloChunks : MonoBehaviour
{
    // Enum for rendering options
    public enum HaloRenderOption
    {
        Inside,
        Outside
    }

    // Public field for selecting rendering option
    public HaloRenderOption renderOption;

    [Range(1, 360)]
    public int CircleSegmentCount = 4;

    [Range(1, 300)]
    public int widthInMeters = 300;

    [Range(0.1f, 10000f)]
    public float radiusInMeters = 10000f;

    [Range(2, 256)]
    public int segmentXVertices = 16; // Number of vertices along the X axis

    [Range(2, 256)]
    public int segmentYVertices = 2; // Number of vertices along the Y axis (top and bottom)

    // Procedural Terrain
    [Range(1, 16)]
    public int textureMetersPerPixel = 4;

    public bool saveTexturesFiles;

    [Range(0, 6)]
    public int levelOfDetail;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;

    public float heightMultiplier;

    public TerrainType[] regions;

    public bool autoUpdate;

    [HideInInspector]
    public float circumference;
    [HideInInspector]
    public float uvScaleX;

    private GameObject segmentsParent;

    [Range(-1, 256)]
    public int specificSegmentIndex = -1; // -1 means create all segments

    [Range(-1, 256)]
    public int specificSegmentTerrainMeshIndex = -1; // -1 means create terrain meshes for all segments

    private void Awake()
    {
        Generate();
    }

    private void Update()
    {
        // Regenerate mesh if parameters are invalid
        if (CircleSegmentCount < 3 || widthInMeters <= 0 || radiusInMeters <= 0)
        {
            Generate();
        }
    }

    public void Generate()
    {
        GenerateCircleMesh();
    }

    private void GenerateCircleMesh()
    {
        Debug.Log("Generating circle mesh...");

        // Delete previous texture files
        DeletePreviousTextureFiles();

        // Delete previous segments
        DeletePreviousSegments();

        // Create a new GameObject to hold all segments
        segmentsParent = new GameObject("Segments");
        segmentsParent.transform.SetParent(this.transform);

        // Maintain the position and rotation of the current transform
        segmentsParent.transform.localPosition = Vector3.zero;
        segmentsParent.transform.localRotation = Quaternion.identity;

        circumference = 2 * Mathf.PI * radiusInMeters;
        uvScaleX = circumference / widthInMeters;

        // Calculate vertex and index counts for a single segment
        int segmentVertexCount = (segmentXVertices + 1) * segmentYVertices;
        int segmentIndexCount = segmentXVertices * (segmentYVertices - 1) * 6;

        // Check if a specific segment index is set
        if (specificSegmentIndex >= 0 && specificSegmentIndex < CircleSegmentCount)
        {
            // Create only the specified segment
            CreateSegment(specificSegmentIndex, segmentIndexCount, segmentVertexCount);
        }
        else
        {
            // Create all segments
            for (int segment = 0; segment < CircleSegmentCount; segment++)
            {
                CreateSegment(segment, segmentIndexCount, segmentVertexCount);
            }
        }
    }

    private void CreateSegment(int segment, int segmentIndexCount, int segmentVertexCount)
    {
        // Create a new HaloSegment instance
        var haloSegment = new HaloSegment(this, segment);

        var segmentObject = haloSegment.GenerateSegment(segmentIndexCount, segmentVertexCount);
        segmentObject.transform.SetParent(segmentsParent.transform, false);
    }

    private void DeletePreviousTextureFiles()
{
    string directoryPath = Application.dataPath + "/ProceduralTextures/";
    if (System.IO.Directory.Exists(directoryPath))
    {
        string[] files = System.IO.Directory.GetFiles(directoryPath, "*.png");
        foreach (string file in files)
        {
            try
            {
                System.IO.File.Delete(file);
                Debug.Log("Deleted file: " + file);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to delete file: " + file + " Error: " + e.Message);
            }
        }
    }
    else
    {
        Debug.LogWarning("Directory does not exist: " + directoryPath);
    }
}

    private void DeletePreviousSegments()
    {
        if (segmentsParent != null)
        {
            Debug.Log("Deleting previous segments...");
            StartCoroutine(DestroyGO(segmentsParent));
        }
    }

    IEnumerator DestroyGO(GameObject go) {
        yield return new WaitForSeconds(0);
        DestroyImmediate(go);
    }
    
    void OnValidate()
    {
        if (CircleSegmentCount < 1)
        {
            CircleSegmentCount = 1;
        }

        // Adjust the range of specificSegmentIndex based on CircleSegmentCount
        specificSegmentIndex = Mathf.Clamp(specificSegmentIndex, -1, CircleSegmentCount);

        // Adjust the range of specificSegmentIndex based on CircleSegmentCount
        specificSegmentTerrainMeshIndex = Mathf.Clamp(specificSegmentTerrainMeshIndex, -1, CircleSegmentCount);

        if (autoUpdate && !Application.isPlaying)
        {
            Generate();
        }
    }
}