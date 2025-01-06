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

        // Add a space before the slider
        GUILayout.Space(10);

        // Temporary float variables for the slider
        float minIndex = proceduralHaloChunks.minSegmentIndex;
        float maxIndex = proceduralHaloChunks.maxSegmentIndex;

        // Create a horizontal layout for the number inputs and slider
        EditorGUILayout.LabelField("Segment Index Range");
        EditorGUILayout.BeginHorizontal();
        
        // Create number input for min index
        minIndex = EditorGUILayout.FloatField(minIndex, GUILayout.Width(50));

        // Create a min-max slider for segment indices
        EditorGUILayout.MinMaxSlider(ref minIndex, ref maxIndex, 0f, proceduralHaloChunks.CircleSegmentCount);

        // Create number input for max index
        maxIndex = EditorGUILayout.FloatField(maxIndex, GUILayout.Width(50));

        EditorGUILayout.EndHorizontal();

        // Convert the float values back to integers
        proceduralHaloChunks.minSegmentIndex = Mathf.RoundToInt(minIndex);
        proceduralHaloChunks.maxSegmentIndex = Mathf.RoundToInt(maxIndex);

        // Add a space before the button
        GUILayout.Space(10);

        // Add a button to generate the halo
        if (GUILayout.Button("Generate"))
        {
            proceduralHaloChunks.Generate();
        }
    }
} 