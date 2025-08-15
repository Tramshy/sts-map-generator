using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StSMapGenerator.InspectorEditor
{
    [InitializeOnLoad]
    public static class DependencyChecker
    {
        static DependencyChecker()
        {
            // Try to find DOTween's main type
            var dotweenType = System.Type.GetType("DG.Tweening.DOTween, DOTween");

            if (dotweenType == null)
            {
                Debug.LogError(
                    "<b>[MyPackage]</b> DOTween is not installed. " +
                    "Please install it from the Asset Store or via UPM before using this package."
                );
            }
        }
    }
}
