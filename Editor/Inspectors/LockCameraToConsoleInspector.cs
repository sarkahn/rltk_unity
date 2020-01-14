using RLTK.MonoBehaviours;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LockCameraToConsole))]
public class LockCameraToConsoleInspector : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var consoleProp = serializedObject.FindProperty("_consoleProxy");

        var oldVal = consoleProp.objectReferenceValue;

        var tar = target as LockCameraToConsole;

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();

            if (check.changed && consoleProp.objectReferenceValue != oldVal)
            {
                tar.SetTarget(consoleProp.objectReferenceValue as SimpleConsoleProxy, tar.transform);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
