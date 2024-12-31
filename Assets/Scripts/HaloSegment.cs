using UnityEngine;
using System.Collections.Generic;

public class HaloSegment
{
    private int segmentXVertices;
    private int segmentYVertices;
    private float radiusInMeters;
    private float widthInMeters;
    private float uvScaleX;
    private ProceduralHaloChunks.HaloRenderOption renderOption;
    private Transform transform;
    private Material material;

    public HaloSegment(int segmentXVertices, int segmentYVertices, float radiusInMeters, float widthInMeters, float uvScaleX, ProceduralHaloChunks.HaloRenderOption renderOption, Transform transform, Material material)
    {
        this.segmentXVertices = segmentXVertices;
        this.segmentYVertices = segmentYVertices;
        this.radiusInMeters = radiusInMeters;
        this.widthInMeters = widthInMeters;
        this.uvScaleX = uvScaleX;
        this.renderOption = renderOption;
        this.transform = transform;
        this.material = material;
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
        UpdateSegmentMesh(vertices, uv, indices);
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
        segmentObject.AddComponent<MeshRenderer>().material = material;
        segmentObject.transform.SetParent(transform, false);
    }
} 