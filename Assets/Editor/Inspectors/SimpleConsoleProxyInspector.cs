using RLTK.MonoBehaviours;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace RLTK.EditorNS
{
    [CustomEditor(typeof(SimpleConsoleProxy))]
    public class SimpleConsoleProxyInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var pixelCam = Object.FindObjectOfType<PixelPerfectCamera>();
            if( pixelCam != null && pixelCam.isActiveAndEnabled && Application.isPlaying)
            {
                var ratio = pixelCam.pixelRatio;
                EditorGUILayout.LabelField($"Pixel ratio {ratio}");
            }
        }

        public override bool RequiresConstantRepaint() => true;
    }
}