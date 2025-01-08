using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ProceduralHaloChunks : MonoBehaviour
{
    [Range(1, 360)]
    public int CircleSegmentCount = 4;

    [Range(1, 300)]
    public int widthInMeters = 300;

    [Range(0.1f, 15000f)]
    public float radiusInMeters = 15000f;

    // Anything higher than 5 breaks everything
    [Range(2, 255)]
    public int segmentXVertices = 16; // Number of vertices along the X axis

    // Anything higher than 5 breaks everything
    [Range(2, 255)]
    public int segmentYVertices = 2; // Number of vertices along the Y axis (top and bottom)

    // Procedural Terrain

    public Material material;

    // Anything higher than 5 breaks everything
    [Range(0.01f, 5)]
    public float textureMetersPerPixel = 5;
    [Range(0, 6)]
    public int levelOfDetail;

    public bool saveTexturesFiles;

    [Range(0, 6)]
    public int meshLevelOfDetail;

    [Range(0, 6)]
    public int maxMeshLevelOfDetail;

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

    [Range(-1, 10000)]
    public float meshHeightMultiplier;

    public AnimationCurve meshHeightCurve;

    public TerrainType[] regions;

    [Range(0, 1)]
    public float regionBlendStrength;

    public Texture2D testTexture;

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

    private List<GameObject> createdSegments = new List<GameObject>(); // List to store created segments
    private List<int> closestSegmentIndices = new List<int>(); // Change to a list to store multiple indices
    
    public GameObject player; // Reference to the player GameObject

    public float proximityThreshold = 300f;

    private void Awake()
    { 
        if (generateOnPlay)
        {
            Generate();
        }
    }

    public void Generate()
    {
        createdSegments.Clear(); // Clear the list before generating new segments
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
            var segmentObject = CreateSegment(null, i, segmentIndexCount, segmentVertexCount, levelOfDetail, meshLevelOfDetail);
            createdSegments.Add(segmentObject); // Add the created segment to the list
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
            CreateSegment(null, segment, segmentIndexCount, segmentVertexCount, levelOfDetail, meshLevelOfDetail);
        }

        // Clear the progress bar after completion or cancellation
#if UNITY_EDITOR
        EditorUtility.ClearProgressBar();
#endif
    }

    private GameObject CreateSegment(GameObject segmentObject, int segment, int segmentIndexCount, int segmentVertexCount, int lod, int meshLod)
    {
        // Create a new HaloSegment instance
        var haloSegment = new HaloSegment(this, segment, lod, meshLod);

        if (segmentObject == null)
        {
            segmentObject = new GameObject(segment.ToString());
        }
        

        haloSegment.GenerateChunk(segmentObject,segmentIndexCount, segmentVertexCount);

        segmentObject.transform.SetParent(segmentsParent.transform, false);

        return segmentObject; // Return the created segment
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

    private void Start()
    {
        StartCoroutine(CheckPlayerProximityRoutine());
    }

    private IEnumerator CheckPlayerProximityRoutine()
    {
        while (true)
        {
            CheckPlayerProximity();
            yield return new WaitForSeconds(1f); // Wait for 1 second before checking again
        }
    }

    private void CheckPlayerProximity()
    {
        if (player == null) return;

        GameObject closestSegment = null;
        float minDistance = float.MaxValue;
        int newClosestSegmentIndex = -1;

        for (int i = 0; i < createdSegments.Count; i++)
        {
            if (closestSegmentIndices.Contains(i)) continue;

            var segment = createdSegments[i];
            MeshRenderer meshRenderer = segment.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                Vector3 closestPoint = meshRenderer.bounds.ClosestPoint(player.transform.position);
                float distance = Vector3.Distance(player.transform.position, closestPoint);

                if (distance < minDistance && distance <= proximityThreshold)
                {
                    minDistance = distance;
                    closestSegment = segment;
                    newClosestSegmentIndex = i;
                }
            }
        }

        if (closestSegment != null && newClosestSegmentIndex != -1)
        {
            closestSegmentIndices.Add(newClosestSegmentIndex);
            Debug.Log($"Closest segment is {closestSegment.name} with a distance of {minDistance}");

            // Calculate vertex and index counts for a single segment
            int segmentVertexCount = (segmentXVertices + 1) * segmentYVertices;
            int segmentIndexCount = segmentXVertices * (segmentYVertices - 1) * 6;

            // Create a new segment
            createdSegments[newClosestSegmentIndex] = CreateSegment(createdSegments[newClosestSegmentIndex], int.Parse(closestSegment.name), segmentIndexCount, segmentVertexCount, 0, maxMeshLevelOfDetail);
        }
    }
}