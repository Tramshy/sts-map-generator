using System;
using System.Collections;
using System.Collections.Generic;
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
            _prefabsForNodeTypesProperty = serializedObject.FindProperty("PrefabsForNodeTypes");
        }

        public override void OnInspectorGUI()
        {
            SerializedProperty iterator = serializedObject.GetIterator();

            if (iterator.NextVisible(true))
            {
                do
                {
                    if (iterator.name == "m_Script" || iterator.name == "PrefabsForNodeTypes")
                        continue;

                    EditorGUILayout.PropertyField(iterator, true);
                } while (iterator.NextVisible(false));
            }

            GUILayout.Space(50);
            GUILayout.Label("PrefabsForNodeTypes");

            MapConfig config = (MapConfig)target;

            int amountOfNodeTypes = System.Enum.GetValues(typeof(NodeTypes)).Length;

            if (_prefabsForNodeTypesProperty.arraySize != amountOfNodeTypes)
                _prefabsForNodeTypesProperty.arraySize = amountOfNodeTypes;

            for (int i = 0; i < _prefabsForNodeTypesProperty.arraySize; i++)
            {
                var element = _prefabsForNodeTypesProperty.GetArrayElementAtIndex(i);
                var enumName = ((NodeTypes)i).ToString();
                element.FindPropertyRelative("ThisNodeType").enumValueIndex = i;

                EditorGUILayout.PropertyField(element, new GUIContent(enumName));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
