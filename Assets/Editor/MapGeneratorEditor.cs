using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(HaloGenerator))]
public class MapGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		HaloGenerator mapGen = (HaloGenerator)target;

		if (DrawDefaultInspector())
		{
			if (mapGen.autoUpdate) 
			{
				mapGen.GenerateHalo();
			}
		}

		if (GUILayout.Button("Generate")) {
			mapGen.GenerateHalo();
		}
	}
}
