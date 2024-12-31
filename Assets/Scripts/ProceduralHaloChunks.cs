using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
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

    [Range(3, 256)]
    public int CircleSegmentCount;

    [Range(0.01f, 300)]
    public float widthInMeters;

    [Range(0.1f, 10000f)]
    public float radiusInMeters;

    [Range(2, 256)]
    public int segmentXVertices = 64; // Number of vertices along the X axis

    [Range(2, 256)]
    public int segmentYVertices = 4; // Number of vertices along the Y axis (top and bottom)

    private Mesh mesh;
    private float circumference;
    private float uvScaleX;

    private void Awake()
    {
        Generate();
    }

    private void Update()
    {
        // Regenerate mesh if parameters are invalid
        if (mesh == null || CircleSegmentCount < 3 || widthInMeters <= 0 || radiusInMeters <= 0)
        {
            Generate();
        }
    }

    private void Generate()
    {
        mesh = new Mesh { name = "Procedural Halo" };

        GetComponent<MeshFilter>().mesh = mesh;
        GenerateCircleMesh();
    }

    private void GenerateCircleMesh()
    {
        Debug.Log("Generating circle mesh...");

        // Delete previous segments
        DeletePreviousSegments();

        circumference = 2 * Mathf.PI * radiusInMeters;
        uvScaleX = circumference / widthInMeters;

        // Calculate vertex and index counts for a single segment
        int segmentVertexCount = (segmentXVertices + 1) * segmentYVertices;
        int segmentIndexCount = segmentXVertices * (segmentYVertices - 1) * 6;

        for (int segment = 0; segment < CircleSegmentCount; segment++)
        {
            var vertices = new List<Vector3>(segmentVertexCount);
            var uv = new Vector2[segmentVertexCount];
            var indices = new int[segmentIndexCount];

            // Create a new HaloSegment instance
            var haloSegment = new HaloSegment(segmentXVertices, segmentYVertices, radiusInMeters, widthInMeters, uvScaleX, renderOption, transform, GetComponent<MeshRenderer>().material);

            haloSegment.GenerateSegment(CircleSegmentCount, segment, segmentIndexCount, segmentVertexCount);
        }
    }

    private void DeletePreviousSegments()
    {
        Debug.Log("Deleting previous segments...");
        foreach (Transform child in transform)
        {
            if (child.name == "HaloSegment")
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
        if (CircleSegmentCount < 3)
        {
            CircleSegmentCount = 3;
        }

        if (!Application.isPlaying)
        {
            Generate();
        }
    }
}