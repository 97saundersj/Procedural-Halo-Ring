using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralHaloMesh : MonoBehaviour
{
    // Enum for rendering options
    public enum HaloRenderOption
    {
        Inside,
        Outside,
        Both
    }

    // Public field for selecting rendering option
    public HaloRenderOption renderOption;

    [Range(3, 300)]
    public int CircleSegmentCount;

    [Range(0.01f, 30000)]
    public float widthInMeters;

    [Range(0.1f, 1000000f)]
    public float radiusInMeters;

    private Mesh mesh;
    private const float WaitTime = 0.05f;

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
        // Calculate vertex and index counts
        int circleVertexCount = (CircleSegmentCount * 2) + 2;
        int circleIndexCount = CircleSegmentCount * 6 * (renderOption == HaloRenderOption.Both ? 2 : 1);

        var vertices = new List<Vector3>(circleVertexCount);
        var uv = new Vector2[circleVertexCount];
        var indices = new int[circleIndexCount];

        // Generate vertices and indices
        GenerateVerticesAndIndices(vertices, indices);

        // Calculate UVs
        CalculateUVs(uv);

        // Set mesh data
        UpdateMesh(vertices, uv, indices);
    }

    private void GenerateVerticesAndIndices(List<Vector3> vertices, int[] indices)
    {
        float segmentWidth = Mathf.PI * 2f / CircleSegmentCount;
        float angle = 0f;

        for (int v = 0; v < CircleSegmentCount + 1; v++)
        {
            // Create vertices for the circle
            vertices.Add(new Vector3(Mathf.Cos(angle) * radiusInMeters, 0f, Mathf.Sin(angle) * radiusInMeters));
            vertices.Add(new Vector3(Mathf.Cos(angle) * radiusInMeters, widthInMeters, Mathf.Sin(angle) * radiusInMeters));

            // Calculate index positions
            int startVert = v * 2;
            int startIndex = v * 6 * (renderOption == HaloRenderOption.Both ? 2 : 1);

            // Generate triangles based on the selected option
            if (v < CircleSegmentCount)
            {
                if (renderOption == HaloRenderOption.Inside || renderOption == HaloRenderOption.Both)
                {
                    CreateInnerHaloTriangles(indices, startVert, startIndex);
                }
                if (renderOption == HaloRenderOption.Outside || renderOption == HaloRenderOption.Both)
                {
                    CreateOuterSideTriangles(indices, startVert, startIndex + (renderOption == HaloRenderOption.Both ? 6 : 0));
                }
            }

            angle -= segmentWidth; // Increment angle for the next vertex
        }
    }

    private void CreateInnerHaloTriangles(int[] indices, int startVert, int startIndex)
    {
        // First Triangle
        indices[startIndex] = startVert;
        indices[startIndex + 1] = startVert + 1;
        indices[startIndex + 2] = startVert + 2;

        // Second Triangle
        indices[startIndex + 3] = startVert + 3;
        indices[startIndex + 4] = startVert + 2;
        indices[startIndex + 5] = startVert + 1;
    }

    private void CreateOuterSideTriangles(int[] indices, int startVert, int startIndex)
    {
        // First Triangle (correct order for outside)
        indices[startIndex] = startVert + 2;
        indices[startIndex + 1] = startVert + 1;
        indices[startIndex + 2] = startVert;

        // Second Triangle (correct order for outside)
        indices[startIndex + 3] = startVert + 2;
        indices[startIndex + 4] = startVert + 3;
        indices[startIndex + 5] = startVert + 1;
    }

    private void CalculateUVs(Vector2[] uv)
    {
        float circumference = 2 * Mathf.PI * radiusInMeters;
        float uvScaleX = circumference / widthInMeters;
    
        for (int segment = 0; segment <= CircleSegmentCount; segment++)
        {
            float segmentRatio = (float)segment / CircleSegmentCount;
            int startVert = segment * 2;
    
            // Adjust UV coordinates based on the calculated scale
            uv[startVert] = new Vector2(segmentRatio * uvScaleX, 0);
            uv[startVert + 1] = new Vector2(segmentRatio * uvScaleX, 1);
        }
    }

    private void UpdateMesh(List<Vector3> vertices, Vector2[] uv, int[] indices)
    {
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uv);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
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