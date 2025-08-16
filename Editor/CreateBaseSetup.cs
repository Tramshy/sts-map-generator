using UnityEditor;
using UnityEngine;

namespace StSMapGenerator.InspectorEditor
{
    public class CreateBaseSetup
    {
        [MenuItem("Tools/StS-Like Generation/Create Basic Setup for Scene")]
        public static void InstantiateBaseSetup()
        {
            var path = "Basic Map Generation Setup";
            GameObject setupObject = Resources.Load<GameObject>(path);

            if (setupObject == null)
            {
                Debug.LogError("Setup object is missing from package!");

                return;
            }

            var instance = GameObject.Instantiate(setupObject);
            instance.name = "Base Setup (Add UI EventSystem)";
        }
    }
}
