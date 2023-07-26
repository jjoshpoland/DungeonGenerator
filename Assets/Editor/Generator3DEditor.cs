using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Generator3D))]
public class Generator3DEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Generator3D generator3D = (Generator3D)target;
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate"))
        {
            generator3D.EditorGenerate();
        }
        if(GUILayout.Button("Clear"))
        {
            generator3D.EditorClear();
        }
    }
}
