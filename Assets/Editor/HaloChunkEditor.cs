using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RingWorldChunk))]
public class HaloChunkEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Get a reference to the target script
        RingWorldChunk haloChunk = (RingWorldChunk)target;

        // Add a space before the button
        GUILayout.Space(10);

        // Add a button to generate the halo
        if (GUILayout.Button("Generate"))
        {
            int segmentIndexCount = haloChunk.proceduralHaloChunks.segmentXVertices * (haloChunk.proceduralHaloChunks.segmentYVertices - 1) * 6;

            haloChunk.GenerateChunk(haloChunk.gameObject, segmentIndexCount);
        }

        if (GUILayout.Button("Split"))
        {
            haloChunk.SplitChunk();
        }
    }
} 