using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralHaloShell : MonoBehaviour
{
    // Enum for rendering options
    public enum HaloRenderOption
    {
        Inside,
        Outside,
        Both
    }

    [Range(3, 360)]
    public int CircleSegmentCount;

    [Range(0.01f, 3000f)]
    public float widthInMeters;

    [Range(0.1f, 50000f)]
    public float radiusInMeters;

    public float wallHeight = 1.0f;

    public float wallWidth = 1.0f;
    
    private Mesh mesh;

    private MeshCollider meshCollider;

    private bool needsColliderCleanup = false;

    private void Awake()
    {
        // Ensure the GameObject has a MeshFilter component
        if (GetComponent<MeshFilter>() == null)
        {
            gameObject.AddComponent<MeshFilter>();
        }

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
        // Initialize the mesh
        mesh = new Mesh { name = "Procedural Halo" };

        // Assign the mesh to the MeshFilter
        GetComponent<MeshFilter>().mesh = mesh;

        // Generate the mesh geometry
        GenerateCircleMesh();

        // Remove all existing MeshColliders
        foreach (var collider in GetComponents<MeshCollider>())
        {
            if (!Application.isPlaying)
            {
                // Schedule destruction for the end of the frame
                StartCoroutine(DestroyColliderAtEndOfFrame(collider));
            }
        }

        // Add a new MeshCollider
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh; // Update the MeshCollider with the new mesh
    }

    private IEnumerator DestroyColliderAtEndOfFrame(MeshCollider collider)
    {
        yield return new WaitForEndOfFrame();
        DestroyImmediate(collider);
    }

    private void GenerateCircleMesh()
    {
        int circleIndices = 6;
        int wallIndices = 6;
        int wallToRingIndices = 6;

        // Calculate vertex and index counts
        int circleVertexCount = (CircleSegmentCount + 1) * 4; // Each segment has 4 vertices
        int circleIndexCount = CircleSegmentCount * (circleIndices + wallIndices + (wallToRingIndices * 2)); // Each segment has 24 indices (6 for circle, 6 for wall, 24 for connecting wall to ring)

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
        int circleIndices = 6;
        int wallIndices = 6;
        int wallToRingIndices = 6;

        float segmentWidth = Mathf.PI * 2f / CircleSegmentCount;
        float angle = 0f;
        float innerRadius = radiusInMeters - wallHeight; // Calculate the inner radius for the wall

        for (int v = 0; v <= CircleSegmentCount; v++)
        {
            // Create vertices for the circle
            vertices.Add(new Vector3(Mathf.Cos(angle) * radiusInMeters, 0f, Mathf.Sin(angle) * radiusInMeters));
            vertices.Add(new Vector3(Mathf.Cos(angle) * radiusInMeters, widthInMeters, Mathf.Sin(angle) * radiusInMeters));

            // Create vertices for the wall closer to the center
            vertices.Add(new Vector3(Mathf.Cos(angle) * innerRadius, widthInMeters, Mathf.Sin(angle) * innerRadius));
            vertices.Add(new Vector3(Mathf.Cos(angle) * innerRadius, widthInMeters + wallWidth, Mathf.Sin(angle) * innerRadius));

            // Calculate index positions
            if (v < CircleSegmentCount)
            {
                int startVert = v * 4;
                int startIndex = v * (circleIndices + wallIndices + (wallToRingIndices * 2));

                // Generate triangles for the circle
                CreateOuterSideTriangles(indices, startVert, startIndex);
                CreateWallTriangles(indices, startVert, startIndex + 6);
            }

            angle += segmentWidth; // Increment angle for the next vertex
        }
    }

    private void CreateOuterSideTriangles(int[] indices, int startVert, int startIndex)
    {
        // First Triangle (correct order for outside)
        indices[startIndex] = startVert;
        indices[startIndex + 1] = startVert + 1;
        indices[startIndex + 2] = startVert + 4;

        // Second Triangle (correct order for outside)
        indices[startIndex + 3] = startVert + 4;
        indices[startIndex + 4] = startVert + 1;
        indices[startIndex + 5] = startVert + 5;
    }

    private void CreateWallTriangles(int[] indices, int startVert, int startIndex)
    {
        // First Triangle for Connecting the wall to the exterior ring
        indices[startIndex] = startVert + 3; // Top of inner wall
        indices[startIndex + 1] = startVert + 5; // Top of outer wall
        indices[startIndex + 2] = startVert + 1; // Bottom of outer wall

        // Second Triangle for Connecting the wall to the exterior ring
        indices[startIndex + 3] = startVert + 3; // Top of inner wall
        indices[startIndex + 4] = startVert + 7; // Top of next segment's outer wall
        indices[startIndex + 5] = startVert + 5; // Top of outer wall

        // First Triangle for creating the surface of the wall that faces the center
        indices[startIndex + 6] = startVert + 2; // Bottom of inner wall
        indices[startIndex + 7] = startVert + 6; // Bottom of next segment's inner wall
        indices[startIndex + 8] = startVert + 3; // Top of inner wall

        // Second Triangle for creating the surface of the wall that faces the center
        indices[startIndex + 9] = startVert + 6; // Bottom of next segment's inner wall
        indices[startIndex + 10] = startVert + 7; // Top of next segment's inner wall
        indices[startIndex + 11] = startVert + 3; // Top of inner wall

        // First Triangle for connecting the inside edge of the wall to the bottom edge of the ring
        indices[startIndex + 12] = startVert + 2; // Bottom of inner wall
        indices[startIndex + 13] = startVert + 1;     // Bottom of outer wall
        indices[startIndex + 14] = startVert + 6; // Bottom of next segment's inner wall

        // Second Triangle for connecting the inside edge of the wall to the bottom edge of the ring
        indices[startIndex + 15] = startVert + 5;
        indices[startIndex + 16] = startVert + 6;
        indices[startIndex + 17] = startVert + 1;
    }

    private void CalculateUVs(Vector2[] uv)
    {
        float circumference = 2 * Mathf.PI * radiusInMeters;
        float uvScaleX = circumference / widthInMeters;

        for (int segment = 0; segment <= CircleSegmentCount; segment++)
        {
            float segmentRatio = (float)segment / CircleSegmentCount;
            int startVert = segment * 4; // Adjusted for 4 vertices per segment

            // Adjust UV coordinates based on the calculated scale
            uv[startVert] = new Vector2(segmentRatio * uvScaleX, 0);
            uv[startVert + 1] = new Vector2(segmentRatio * uvScaleX, 1);
            uv[startVert + 2] = new Vector2(segmentRatio * uvScaleX, 0); // Wall bottom
            uv[startVert + 3] = new Vector2(segmentRatio * uvScaleX, wallHeight / widthInMeters); // Wall top
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
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            needsColliderCleanup = true;
            EditorApplication.update += CleanupColliders;
        }
#endif
    }

#if UNITY_EDITOR
    private void OnDestroy()
    {
        // Unsubscribe from the update event to prevent accessing a destroyed object
        EditorApplication.update -= CleanupColliders;
    }
#endif

    private void CleanupColliders()
    {
#if UNITY_EDITOR
        if (needsColliderCleanup)
        {
            if (this == null) return; // Check if the object is still valid

            foreach (var collider in GetComponents<MeshCollider>())
            {
                DestroyImmediate(collider);
            }

            needsColliderCleanup = false;
            EditorApplication.update -= CleanupColliders;

            Generate();
        }
#endif
    }
}