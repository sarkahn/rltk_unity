using RLTK.MonoBehaviours;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SimpleConsoleProxy))]
public class MapInspector : Editor
{


    private void OnEnable()
    {
        
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();


    }
}
