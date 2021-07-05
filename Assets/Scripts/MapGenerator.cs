using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour {

	public enum DrawMode {NoiseMap, ColourMap, Mesh};
	public DrawMode drawMode;

	[Range(1, 1000)]
	public int widthScale;

	[Range(1, 100)]
	public int heightScale;

	public int mapWidth;
	public int mapHeight;

	private int mapChunkfactor = 24;

	private int mapChunkSize = 241;

	[Range(0,6)]
	public int levelOfDetail;
	public float noiseScale;

	public int octaves;
	[Range(0,1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;

	public bool autoUpdate;

	public TerrainType[] regions;

	public bool createHalo;

	private void Awake()
	{
		GenerateMap();
	}
	
	public void GenerateMap() {
		
		mapWidth = (widthScale * 24) + 1;
		mapHeight = (heightScale * 24) + 1;

		float[,] noiseMap = Noise.OLDGenerateNoiseMap (mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
		
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
			display.DrawTexture (TextureGenerator.OLDTextureFromHeightMap (noiseMap));
		} else if (drawMode == DrawMode.ColourMap) {
			display.DrawTexture (TextureGenerator.TextureFromColourMap (colourMap, mapWidth, mapHeight));
		} else if (drawMode == DrawMode.Mesh) {
			display.DrawMesh (MeshGenerator.OLDGenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail, createHalo), TextureGenerator.TextureFromColourMap (colourMap, mapWidth, mapHeight));
		}
		
	}

	void OnValidate() {
		if (lacunarity < 1) {
			lacunarity = 1;
		}
		if (octaves < 0) {
			octaves = 0;
		}

		if(mapWidth != (widthScale * 24) + 1)
        {
			mapWidth = (widthScale * 24) + 1;
		}
		if (mapHeight != (heightScale * 24) + 1)
		{
			mapHeight = (heightScale * 24) + 1;
		}
	}
}

[System.Serializable]
public struct TerrainType {
	public string name;
	public float height;
	public Color colour;
}