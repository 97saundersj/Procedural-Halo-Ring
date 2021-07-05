using UnityEngine;
using System.Collections;

public class MapPreview : MonoBehaviour {

	public Renderer textureRender;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;


	public enum DrawMode {NoiseMap, Mesh, FalloffMap};
	public DrawMode drawMode;

	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
	public TextureData textureData;

	public TerrainType[] regions;
	public Material terrainMaterial;



	[Range(0,MeshSettings.numSupportedLODs-1)]
	public int editorPreviewLOD;
	public bool autoUpdate;




	public void DrawMapInEditor() {
		
		HeightMap heightMap = HeightMapGenerator.GenerateHeightMap (meshSettings.numXVertsPerLine, meshSettings.numYVertsPerLine, heightMapSettings, Vector2.zero);

		Color[] colourMap = new Color[meshSettings.numXVertsPerLine * meshSettings.numYVertsPerLine];

		for (int y = 0; y < meshSettings.numYVertsPerLine; y++)
		{
			for (int x = 0; x < meshSettings.numXVertsPerLine; x++)
			{
				float currentHeight = heightMap.values[x, y];
				for (int i = 0; i < regions.Length; i++)
				{
					if (currentHeight <= regions[i].height)
					{
						colourMap[y * meshSettings.numXVertsPerLine + x] = regions[i].colour;
						break;
					}
				}
			}
		}
		MapDisplay display = FindObjectOfType<MapDisplay>();
		if (drawMode == DrawMode.NoiseMap) {
			DrawTexture (TextureGenerator.TextureFromHeightMap (heightMap));
		} else if (drawMode == DrawMode.Mesh) {
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh (heightMap.values, meshSettings, editorPreviewLOD, heightMapSettings), TextureGenerator.TextureFromColourMap(colourMap, meshSettings.numXVertsPerLine, meshSettings.numYVertsPerLine));
		} else if (drawMode == DrawMode.FalloffMap) {
			DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numXVertsPerLine),0,1)));
		}

		terrainMaterial.SetVector("_elevationMinMax", new Vector4(MeshGenerator.elevationMinMax.Min, MeshGenerator.elevationMinMax.Max));


		textureData.ApplyToMaterial(terrainMaterial);
		textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
		Debug.Log("min: " + heightMapSettings.minHeight + ". max: " + heightMapSettings.maxHeight);

		Debug.Log("min: " + MeshGenerator.elevationMinMax.Min + ". max: " + MeshGenerator.elevationMinMax.Max);
	}





	public void DrawTexture(Texture2D texture) {
		textureRender.sharedMaterial.mainTexture = texture;
		textureRender.transform.localScale = new Vector3 (texture.width, 1, texture.height) /10f;

		textureRender.gameObject.SetActive (true);
		meshFilter.gameObject.SetActive (false);
	}

	public void DrawMesh(MeshData meshData) {
		meshFilter.sharedMesh = meshData.CreateMesh ();

		textureRender.gameObject.SetActive (false);
		meshFilter.gameObject.SetActive (true);
	}



	void OnValuesUpdated() {
		if (!Application.isPlaying) {
			DrawMapInEditor ();
		}
	}

	void OnTextureValuesUpdated() {
		textureData.ApplyToMaterial (terrainMaterial);
	}

	void OnValidate() {

		if (meshSettings != null) {
			meshSettings.OnValuesUpdated -= OnValuesUpdated;
			meshSettings.OnValuesUpdated += OnValuesUpdated;
		}
		if (heightMapSettings != null) {
			heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
			heightMapSettings.OnValuesUpdated += OnValuesUpdated;
		}
		if (textureData != null) {
			textureData.OnValuesUpdated -= OnTextureValuesUpdated;
			textureData.OnValuesUpdated += OnTextureValuesUpdated;
		}

	}

}
