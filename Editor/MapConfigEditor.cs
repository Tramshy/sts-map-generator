using UnityEditor;
using UnityEngine;

namespace StSMapGenerator.InspectorEditor
{
    [CustomEditor(typeof(MapConfig))]
    public class MapConfigEditor : Editor
    {
        private SerializedProperty _prefabsForNodeTypesProperty;

        private void OnEnable()
        {
            _prefabsForNodeTypesProperty = serializedObject.FindProperty("PrefabsForLayerTypes");
        }

        public override void OnInspectorGUI()
        {
            SerializedProperty iterator = serializedObject.GetIterator();

            if (iterator.NextVisible(true))
            {
                do
                {
                    if (iterator.name == "m_Script" || iterator.name == "PrefabsForLayerTypes")
                        continue;

                    EditorGUILayout.PropertyField(iterator, true);
                } while (iterator.NextVisible(false));
            }

            GUILayout.Space(25);
            GUILayout.Label("Prefabs For Layer Types");

            MapConfig config = (MapConfig)target;

            var customNodeTypes = serializedObject.FindProperty("CustomLayerTypes");

            // Subtract 1 for custom enum value
            int enumLength = System.Enum.GetValues(typeof(LayerTypes)).Length - 1;
            int amountOfNodeTypes = enumLength;

            if (customNodeTypes != null)
                amountOfNodeTypes += customNodeTypes.arraySize;

            if (_prefabsForNodeTypesProperty.arraySize != amountOfNodeTypes)
                _prefabsForNodeTypesProperty.arraySize = amountOfNodeTypes;

            for (int i = 0; i < enumLength; i++)
            {
                var element = _prefabsForNodeTypesProperty.GetArrayElementAtIndex(i);
                var enumName = ((LayerTypes)i).ToString();
                element.FindPropertyRelative("ThisLayerType").enumValueIndex = i;

                EditorGUILayout.PropertyField(element, new GUIContent(enumName));
            }

            if (customNodeTypes == null)
                return;

            int customNodeIndex = 0;

            for (int i = enumLength; i < amountOfNodeTypes; i++)
            {
                var element = _prefabsForNodeTypesProperty.GetArrayElementAtIndex(i);
                var customNodeName = customNodeTypes.GetArrayElementAtIndex(customNodeIndex);
                customNodeIndex++;
                element.FindPropertyRelative("ThisLayerType").enumValueIndex = enumLength;
                element.FindPropertyRelative("LayerID").stringValue = customNodeName.stringValue;

                EditorGUILayout.PropertyField(element, new GUIContent(customNodeName.stringValue));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
