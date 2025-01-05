using System.Collections;
using UnityEditor;
using UnityEngine;

public class ProceduralHaloChunks : MonoBehaviour
{
    [Range(1, 360)]
    public int CircleSegmentCount = 4;

    [Range(1, 300)]
    public int widthInMeters = 300;

    [Range(0.1f, 10000f)]
    public float radiusInMeters = 10000f;

    // Anything higher than 5 breaks everything
    [Range(2, 87)]
    public int segmentXVertices = 16; // Number of vertices along the X axis

    // Anything higher than 5 breaks everything
    [Range(2, 87)]
    public int segmentYVertices = 2; // Number of vertices along the Y axis (top and bottom)

    // Procedural Terrain

    // Anything higher than 5 breaks everything
    [Range(1, 5)]
    public int textureMetersPerPixel = 5;

    public bool saveTexturesFiles;

    [Range(0, 6)]
    public int levelOfDetail;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    [Range(0, 64)]
    public int seed;

    [Range(-1, 1000)]
    public float heightMultiplier;

    public AnimationCurve heightCurve;

    [Range(-1, 1000)]
    public float meshHeightMultiplier;

    public AnimationCurve meshHeightCurve;

    public TerrainType[] regions;

    public bool autoUpdate;

    [HideInInspector]
    public float circumference;

    //[HideInInspector]
    //public int mapChunkfactor = 24;

    [HideInInspector]
    public float uvScaleX;

    private GameObject segmentsParent;

    [HideInInspector]
    public int minSegmentIndex = -1; // Minimum segment index
    [HideInInspector]
    public int maxSegmentIndex = 360; // Maximum segment index

    public bool generateOnPlay;

    private void Awake()
    { 
        if (generateOnPlay)
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

        // Create segments within the specified range
        for (int i = Mathf.Max(0, minSegmentIndex); i <= Mathf.Min(CircleSegmentCount - 1, maxSegmentIndex); i++)
        {
            CreateSegment(i, segmentIndexCount, segmentVertexCount);
        }
    }

    // Make coroutine to visualise creation in editor, you will have to click constantly on the editer though
    private void CreateSegments(int segmentIndexCount, int segmentVertexCount)
    {
        for (int segment = 0; segment < CircleSegmentCount; segment++)
        {
#if UNITY_EDITOR
            bool cancel = EditorUtility.DisplayCancelableProgressBar(
                "Forging Halo Installation",
                $"Creating segment {segment + 1} of {CircleSegmentCount}",
                (float)segment / CircleSegmentCount
            );


            // Check if the user clicked the cancel button
            if (cancel)
            {
                Debug.Log("Operation canceled by the user.");
                break;
            }
#endif
            CreateSegment(segment, segmentIndexCount, segmentVertexCount);
        }

        // Clear the progress bar after completion or cancellation
#if UNITY_EDITOR
        EditorUtility.ClearProgressBar();
#endif
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

    IEnumerator DestroyGO(GameObject go)
    {
        yield return new WaitForSeconds(0);
        DestroyImmediate(go);
    }

    void OnValidate()
    {
        if (autoUpdate && !Application.isPlaying)
        {
            Generate();
        }
    }
}