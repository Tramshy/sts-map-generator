using UnityEditor;
using UnityEngine;

namespace StSMapGenerator.InspectorEditor
{
    [InitializeOnLoad]
    public static class DependencyChecker
    {
        private const string DO_TWEEN_DEFINE_SYMBOL = "DOTWEEN_INSTALLED";

        static DependencyChecker()
        {
            // Try to find DOTween's main type
            var dotweenType = System.Type.GetType("DG.Tweening.DOTween, DOTween");
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);

            if (dotweenType == null)
            {
                Debug.LogError(
                    "<b>[MyPackage]</b> DOTween is not installed. " +
                    "Please install it from the Asset Store or via UPM before using this package."
                );

                // Remove define symbol
                if (defines.Contains(DO_TWEEN_DEFINE_SYMBOL))
                {
                    Debug.Log("Removing DOTween define symbol...");

                    defines = defines.Replace(DO_TWEEN_DEFINE_SYMBOL, "").Replace(";;", ";").Trim(';');
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
                    Debug.Log("DOTween logic disabled.");
                }
            }
            else if (!defines.Contains(DO_TWEEN_DEFINE_SYMBOL))
            {
                Debug.Log("DOTween is installed in project. Adding define symbol...");

                defines += ";" + DO_TWEEN_DEFINE_SYMBOL;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
                Debug.Log("DOTween logic enabled!");
            }
        }
    }
}
