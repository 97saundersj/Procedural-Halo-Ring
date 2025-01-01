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

    [Range(1, 64)]
    public int CircleSegmentCount = 4;

    [Range(0.01f, 300)]
    public float widthInMeters = 300;

    [Range(0.1f, 10000f)]
    public float radiusInMeters = 10000f;

    [Range(2, 256)]
    public int segmentXVertices = 16; // Number of vertices along the X axis

    [Range(2, 256)]
    public int segmentYVertices = 2; // Number of vertices along the Y axis (top and bottom)

    // Procedural Terrain
    [Range(1, 256)]
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

    public TerrainType[] regions;

    public bool autoUpdate;

    [HideInInspector]
    public float circumference;
    [HideInInspector]
    public float uvScaleX;

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

        circumference = 2 * Mathf.PI * radiusInMeters;
        uvScaleX = circumference / widthInMeters;

        // Calculate vertex and index counts for a single segment
        int segmentVertexCount = (segmentXVertices + 1) * segmentYVertices;
        int segmentIndexCount = segmentXVertices * (segmentYVertices - 1) * 6;

        for (int segment = 0; segment < CircleSegmentCount; segment++)
        {
            // Create a new HaloSegment instance
            var haloSegment = new HaloSegment(this, segment);

            var segmentObject = haloSegment.GenerateSegment(CircleSegmentCount, segment, segmentIndexCount, segmentVertexCount);
            segmentObject.transform.SetParent(transform, false);
        }
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
        Debug.Log("Deleting previous segments...");
        foreach (Transform child in transform)
        {
            if (child.name.Contains("HaloSegment"))
            {
                StartCoroutine(DestroyGO(child.gameObject));
            }
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

        if (autoUpdate && !Application.isPlaying)
        {
            Generate();
        }
    }
}