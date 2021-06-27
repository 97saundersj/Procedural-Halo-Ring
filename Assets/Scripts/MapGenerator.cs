using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour {

	public enum DrawMode {NoiseMap, ColourMap, Mesh};
	public DrawMode drawMode;

	public int mapWidth;
	public int mapHeight;
	public float noiseScale;

	[Range(1, 10)]
	public int octaves;
	[Range(0,1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

	public bool autoUpdate;

	public TerrainType[] regions;

	[Range(0, 100)]
	public float heightScale;

	public bool createHalo;
	private MeshData meshData;

	public void GenerateMap() {
		float[,] noiseMap = Noise.GenerateNoiseMap (mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

		Color[] colourMap = new Color[mapWidth * mapHeight];
		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {
				float currentHeight = noiseMap [x, y];
				for (int i = 0; i < regions.Length; i++) {
					if (currentHeight <= regions [i].height) {
						colourMap [y * mapWidth + x] = regions [i].colour;
						break;
					}
				}
			}
		}

		MapDisplay display = FindObjectOfType<MapDisplay> ();
		if (drawMode == DrawMode.NoiseMap) {
			display.DrawTexture (TextureGenerator.TextureFromHeightMap (noiseMap));
		} else if (drawMode == DrawMode.ColourMap) {
			display.DrawTexture (TextureGenerator.TextureFromColourMap (colourMap, mapWidth, mapHeight));
		} else if (drawMode == DrawMode.Mesh) {
			meshData = MeshGenerator.GenerateTerrainMesh(noiseMap, heightScale, createHalo);
			display.DrawMesh (meshData, TextureGenerator.TextureFromColourMap (colourMap, mapWidth, mapHeight));
		}
	}

	void OnValidate() {
		if (mapWidth < 1) {
			mapWidth = 1;
		}
		if (mapHeight < 1) {
			mapHeight = 1;
		}
		if (lacunarity < 1) {
			lacunarity = 1;
		}
		if (octaves < 0) {
			octaves = 0;
		}
	}
/*
	private void OnDrawGizmos()
	{
		if (meshData == null || meshData.vertices == null) 
		{
			return;
		}
		
		Gizmos.color = Color.black;
		for (int i = meshData.vertices.Length- 5; i < meshData.vertices.Length; i++)
		{
			Gizmos.color = Color.black;
			Gizmos.DrawSphere(meshData.vertices[i], 0.001f);

			UnityEditor.Handles.Label(meshData.vertices[i], $"({meshData.vertices[i].x.ToString()},{meshData.vertices[i].z.ToString()})");
		}
		
	}*/
}

[System.Serializable]
public struct TerrainType {
	public string name;
	public float height;
	public Color colour;
}