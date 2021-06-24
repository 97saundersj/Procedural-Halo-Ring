using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PolygonMesh : MonoBehaviour
{
	public int CircleSegmentCount;
	public float width;

	private Mesh mesh;
	private Vector3[] vertices;
	private Vector3[] normals;
	private Color32[] cubeUV;



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
		if (!Application.isPlaying)
		{
			Generate();
		}
	}

	private void Generate()
	{
		WaitForSeconds wait = new WaitForSeconds(0.05f);

		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Sphere";
		GenerateCircleMesh();

		
	}

	private Mesh GenerateCircleMesh()
	{
		int CircleVertexCount = (CircleSegmentCount * 2);
		int CircleIndexCount = (CircleSegmentCount * 3 * 2 * 2);

		var circle = mesh;
		var vert = new List<Vector3>(CircleVertexCount);
		var indices = new int[CircleIndexCount];
		var segmentWidth = Mathf.PI * 2f / CircleSegmentCount;
		var angle = 0f;
		//vert.Add(Vector3.zero);

		//loop through each segment
		for (int v = 0; v < CircleSegmentCount; v++)
		{
			vert.Add(new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)));
			vert.Add(new Vector3(Mathf.Cos(angle), width, Mathf.Sin(angle)));

			angle -= segmentWidth;

			var startVert = v * 2;
			var startIndex = v * 6 * 2;

			if ( v < CircleSegmentCount -1)
			{
				//First Triangle
				indices[startIndex] = startVert;
				indices[startIndex + 1] = startVert + 1;
				indices[startIndex + 2] = startVert + 2;

				//Second Triangle Triangle
				indices[startIndex + 3] = startVert + 3;
				indices[startIndex + 4] = startVert + 2;
				indices[startIndex + 5] = startVert + 1;

				//Other Side for normals
				//First Triangle
				indices[startIndex + 6] = startVert + 2;
				indices[startIndex + 7] = startVert + 1;
				indices[startIndex + 8] = startVert ;

				//Second Triangle Triangle
				indices[startIndex + 9] = startVert + 1;
				indices[startIndex + 10] = startVert + 2;
				indices[startIndex + 11] = startVert + 3;

			}
			
			else if(v == CircleSegmentCount - 1)
            {
				//First Triangle
				indices[startIndex] = startVert;
				indices[startIndex + 1] = startVert + 1;
				indices[startIndex + 2] = 0;
				
				//Second Triangle Triangle
				indices[startIndex + 3] = startVert + 1;
				indices[startIndex + 4] = 1;
				indices[startIndex + 5] = 0;

				//Other Side for normals
				//First Triangle
				indices[startIndex + 6] = 0;
				indices[startIndex + 7] = startVert + 1;
				indices[startIndex + 8] = startVert;

				//Second Triangle Triangle
				indices[startIndex + 9] = 0;
				indices[startIndex + 10] = 1;
				indices[startIndex + 11] = startVert + 1;

			}
			
		}

		circle.SetVertices(vert);
		circle.SetIndices(indices, MeshTopology.Triangles, 0);
		
		circle.RecalculateBounds();

		//circle.RecalculateNormals();
		//circle.RecalculateUVDistributionMetrics();


		vertices = vert.ToArray();

		mesh = circle;
		return circle;
	}

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

}