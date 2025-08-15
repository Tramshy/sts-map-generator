using UnityEditor;
using UnityEngine;

namespace StSMapGenerator.InspectorEditor
{
    [CustomEditor(typeof(MapGeneration))]
    public class MapGenerationEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MapGeneration generator = (MapGeneration)target;

            if (GUILayout.Button("Regenerate Map"))
                generator.RecreateBoard();
        }
    }
}
