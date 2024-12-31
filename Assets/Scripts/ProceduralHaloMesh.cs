using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralHaloMesh : MonoBehaviour
{
    [Range(3, 300)]
    public int CircleSegmentCount;

    [Range(0.01f, 1)]
    public float width;

    public bool renderInnerHalo = true;
    public bool renderBothSides = false;

    private Mesh mesh;
    private Vector3[] vertices;
    private Vector3[] normals;
    private Color32[] cubeUV;

    private int CircleVertexCount;
    private int CircleIndexCount;


    private void Awake()
    {
        Generate();
    }

    private void Update()
    {
        Generate();
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

    private void Generate()
    {
        WaitForSeconds wait = new WaitForSeconds(0.05f);

        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Halo";
        GenerateCircleMesh();
    }

    private Mesh GenerateCircleMesh()
    {
        var circle = mesh;

        CircleVertexCount = (CircleSegmentCount * 2) + 2;
        CircleIndexCount = renderBothSides ? (CircleSegmentCount * 3 * 2 * 2) : (CircleSegmentCount * 3 * 2);

        var vert = new List<Vector3>(CircleVertexCount);
        Vector2[] uv = new Vector2[CircleVertexCount];
        var indices = new int[CircleIndexCount];


        //loop through each segment
        var angle = 0f;
        var segmentWidth = Mathf.PI * 2f / CircleSegmentCount;
        for (int v = 0; v < CircleSegmentCount + 1; v++)
        {
            vert.Add(new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)));
            vert.Add(new Vector3(Mathf.Cos(angle), width, Mathf.Sin(angle)));

            angle -= segmentWidth;

            var startVert = v * 2;
            var startIndex = v * 6 * (renderBothSides ? 2 : 1);

            if (v < CircleSegmentCount)
            {
                if (renderInnerHalo)
                {
                    //First Triangle
                    indices[startIndex] = startVert;
                    indices[startIndex + 1] = startVert + 1;
                    indices[startIndex + 2] = startVert + 2;

                    //Second Triangle
                    indices[startIndex + 3] = startVert + 3;
                    indices[startIndex + 4] = startVert + 2;
                    indices[startIndex + 5] = startVert + 1;

                }

                //Other Side for normals
                if (renderInnerHalo == false && v < CircleSegmentCount - 1)
                {
                    //First Triangle
                    indices[startIndex + 6] = startVert + 2;
                    indices[startIndex + 7] = startVert + 1;
                    indices[startIndex + 8] = startVert;

                    //Second Triangle Triangle
                    indices[startIndex + 9] = startVert + 1;
                    indices[startIndex + 10] = startVert + 2;
                    indices[startIndex + 11] = startVert + 3;
                }
            }
        }

        //Calculate UVs
        for (int segment = 0; segment <= CircleSegmentCount; segment++)
        {
            var startVert = segment * 2;

            float segmentRatio = (float)((double)segment / (double)CircleSegmentCount);

            //UVs
            uv[startVert] = new Vector2(segmentRatio, 0);
            uv[startVert + 1] = new Vector2(segmentRatio, 1);
        }

        circle.SetVertices(vert);
        circle.SetUVs(0, uv);
        circle.SetIndices(indices, MeshTopology.Triangles, 0);

        circle.RecalculateBounds();

        circle.RecalculateNormals();

        vertices = vert.ToArray();

        mesh = circle;
        return circle;
    }

    /*
	private void OnDrawGizmos()
	{
		if (vertices == null)
		{
			return;
		}

		Gizmos.color = Color.black;
		for (int i = 0; i < vertices.Length; i++)
		{
			Gizmos.color = Color.black;
			Gizmos.DrawSphere(vertices[i], 0.001f);

			UnityEditor.Handles.Label(vertices[i], i.ToString());
		}
	}
	*/
}