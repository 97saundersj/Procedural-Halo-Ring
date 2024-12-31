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
        int segmentVertexCount = 4; // Two vertices per edge, two edges per segment
        int segmentIndexCount = 6; // Two triangles per segment

        for (int segment = 0; segment < CircleSegmentCount; segment++)
        {
            var vertices = new List<Vector3>(segmentVertexCount);
            var uv = new Vector2[segmentVertexCount];
            var indices = new int[segmentIndexCount];

            // Generate vertices and indices for this segment
            GenerateSegmentVerticesAndIndices(segment, vertices, indices);

            // Calculate UVs for this segment
            CalculateSegmentUVs(segment, uv);

            // Set mesh data for this segment
            UpdateSegmentMesh(vertices, uv, indices);
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

    private void GenerateSegmentVerticesAndIndices(int segment, List<Vector3> vertices, int[] indices)
    {
        float segmentWidth = Mathf.PI * 2f / CircleSegmentCount;
        float angle = segment * segmentWidth;

        // Create vertices for the segment
        vertices.Add(new Vector3(Mathf.Cos(angle) * radiusInMeters, 0f, Mathf.Sin(angle) * radiusInMeters));
        vertices.Add(new Vector3(Mathf.Cos(angle) * radiusInMeters, widthInMeters, Mathf.Sin(angle) * radiusInMeters));
        vertices.Add(new Vector3(Mathf.Cos(angle + segmentWidth) * radiusInMeters, 0f, Mathf.Sin(angle + segmentWidth) * radiusInMeters));
        vertices.Add(new Vector3(Mathf.Cos(angle + segmentWidth) * radiusInMeters, widthInMeters, Mathf.Sin(angle + segmentWidth) * radiusInMeters));

        if (renderOption == HaloRenderOption.Inside)
        {
            // Reverse the order of indices for inside rendering
            indices[0] = 0;
            indices[1] = 2;
            indices[2] = 1;
            indices[3] = 1;
            indices[4] = 2;
            indices[5] = 3;
        }
        else if (renderOption == HaloRenderOption.Outside)
        {
            // Default order for outside rendering
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 1;
            indices[4] = 3;
            indices[5] = 2;
        }
    }

    private void CalculateSegmentUVs(int segment, Vector2[] uv)
    {
        float segmentRatio = (float)segment / CircleSegmentCount;
        float nextSegmentRatio = (float)(segment + 1) / CircleSegmentCount;

        uv[0] = new Vector2(segmentRatio * uvScaleX, 1);
        uv[1] = new Vector2(segmentRatio * uvScaleX, 0);
        uv[2] = new Vector2(nextSegmentRatio * uvScaleX, 1);
        uv[3] = new Vector2(nextSegmentRatio * uvScaleX, 0);
    }

    private void UpdateSegmentMesh(List<Vector3> vertices, Vector2[] uv, int[] indices)
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
        segmentObject.AddComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
        segmentObject.transform.SetParent(transform, false);
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