using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HaloSegment))]
public class HaloChunkEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Get a reference to the target script
        HaloSegment haloChunk = (HaloSegment)target;

        // Add a space before the button
        GUILayout.Space(10);

        // Add a button to generate the halo
        if (GUILayout.Button("Generate"))
        {
            int segmentVertexCount = (haloChunk.proceduralHaloChunks.segmentXVertices + 1) * haloChunk.proceduralHaloChunks.segmentYVertices;
            int segmentIndexCount = haloChunk.proceduralHaloChunks.segmentXVertices * (haloChunk.proceduralHaloChunks.segmentYVertices - 1) * 6;

            haloChunk.GenerateChunk(haloChunk.gameObject, segmentIndexCount, segmentVertexCount);
        }
    }
} 