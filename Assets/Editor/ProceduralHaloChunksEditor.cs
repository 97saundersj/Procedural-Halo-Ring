using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralHaloChunks))]
public class ProceduralHaloChunksEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Get a reference to the target script
        ProceduralHaloChunks proceduralHaloChunks = (ProceduralHaloChunks)target;

        // Add a space before the button
        GUILayout.Space(10);

        // Add a button to generate the halo
        if (GUILayout.Button("Generate"))
        {
            proceduralHaloChunks.Generate();
        }
    }
} 