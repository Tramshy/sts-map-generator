using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace StSMapGenerator.InspectorEditor
{
    [CustomPropertyDrawer(typeof(MapLayer))]
    public class MapLayerDrawer : PropertyDrawer
    {
        private const int _fieldPadding = 4, _finalMultiplier = 4;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded, label, true
            );

            if (!property.isExpanded)
                return;

            Rect fieldRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + _fieldPadding, position.width, EditorGUIUtility.singleLineHeight);

            // Base fields
            var layerType = property.FindPropertyRelative("LayerType");
            EditorGUI.PropertyField(fieldRect, layerType, true);
            fieldRect.y += EditorGUI.GetPropertyHeight(layerType, true) + _fieldPadding;

            var yPadding = property.FindPropertyRelative("YPaddingFromPreviousLayer");
            EditorGUI.PropertyField(fieldRect, yPadding, true);
            fieldRect.y += EditorGUI.GetPropertyHeight(yPadding, true) + _fieldPadding;

            var randomizePosition = property.FindPropertyRelative("RandomizePosition");
            EditorGUI.PropertyField(fieldRect, randomizePosition, true);
            fieldRect.y += EditorGUI.GetPropertyHeight(randomizePosition, true) + _fieldPadding;

            var useRandomRange = property.FindPropertyRelative("UseRandomRange");
            EditorGUI.PropertyField(fieldRect, useRandomRange, true);
            fieldRect.y += EditorGUI.GetPropertyHeight(useRandomRange, true) + _fieldPadding;

            // Amount of nodes (Conditional fields)
            if (useRandomRange.boolValue)
            {
                var randomRange = property.FindPropertyRelative("_randomNodeRange");
                EditorGUI.PropertyField(fieldRect, randomRange, true);
                fieldRect.y += EditorGUI.GetPropertyHeight(randomRange, true) + _fieldPadding * _finalMultiplier;
            }
            else
            {
                var nodeAmount = property.FindPropertyRelative("_amountOfNodes");
                EditorGUI.PropertyField(fieldRect, nodeAmount, true);
                fieldRect.y += EditorGUI.GetPropertyHeight(nodeAmount, true) + _fieldPadding * _finalMultiplier;
            }

            NodeTypes layerValue = (NodeTypes)layerType.enumValueIndex;

            // Node ID (Conditional)
            if (layerValue != NodeTypes.Custom)
                return;

            var parent = GetParentObjectOfProperty(property);
            var optionsFieldInfo = parent.GetType().GetField("CustomNodeTypes",
                                    BindingFlags.Public | BindingFlags.Instance);

            if (optionsFieldInfo == null || optionsFieldInfo.FieldType != typeof(string[]))
            {
                Debug.LogError("Something has gone very wrong. The custom node types array has switched types or disappeared");

                return;
            }

            string[] options = (string[])optionsFieldInfo.GetValue(parent);

            if (options == null || options.Length == 0)
                return;

            fieldRect.y += 10;

            var nodeID = property.FindPropertyRelative("NodeID");

            int currentIndex = Mathf.Max(0, System.Array.IndexOf(options, nodeID.stringValue));
            int selectedIndex = EditorGUI.Popup(fieldRect, "Custom Node Type Data", currentIndex, options);

            if (selectedIndex >= 0 && selectedIndex < options.Length)
            {
                nodeID.stringValue = options[selectedIndex];
            }

            fieldRect.y += EditorGUIUtility.singleLineHeight + _fieldPadding * _finalMultiplier;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            float alwaysDisplayedFields = 6;
            // Subtract one for the final fields. Their height has double padding.
            float height = alwaysDisplayedFields * EditorGUIUtility.singleLineHeight + _fieldPadding * (alwaysDisplayedFields - 1);
            height += _fieldPadding * _finalMultiplier;

            // Condition check for custom node types
            var layerType = property.FindPropertyRelative("LayerType");
            NodeTypes layerValue = (NodeTypes)layerType.enumValueIndex;

            if (layerValue != NodeTypes.Custom)
                return height;

            var parent = GetParentObjectOfProperty(property);
            var optionsFieldInfo = parent.GetType().GetField("CustomNodeTypes",
                                    BindingFlags.Public | BindingFlags.Instance);
            var optionsLength = ((string[])optionsFieldInfo.GetValue(parent)).Length;

            if (optionsLength == 0)
                return height;

            height += 10;
            height += EditorGUIUtility.singleLineHeight + _fieldPadding * _finalMultiplier;

            return height;
        }

        // Warning, confusion below.
        public static object GetParentObjectOfProperty(SerializedProperty property)
        {
            string path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            string[] elements = path.Split('.');

            for (int i = 0; i < elements.Length - 1; i++)
            {
                if (elements[i].Contains("["))
                {
                    string elementName = elements[i].Substring(0, elements[i].IndexOf("["));
                    int index = Convert.ToInt32(elements[i].Substring(elements[i].IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, elements[i]);
                }
            }

            return obj;
        }

        private static object GetValue(object source, string name)
        {
            if (source == null)
                return null;

            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
                return f.GetValue(source);

            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p != null)
                return p.GetValue(source, null);

            return null;
        }

        private static object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            while (index-- >= 0) enm.MoveNext();
            return enm.Current;
        }
    }
}
