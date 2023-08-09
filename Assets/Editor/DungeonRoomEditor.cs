using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonRoom))]
public class DungeonRoomEditor : Editor
{
    SerializedProperty Theme;

    private void OnEnable()
    {
        Theme = serializedObject.FindProperty("RoomTheme");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(Theme);
        if(serializedObject.ApplyModifiedProperties())
        {
            DungeonRoom room = serializedObject.targetObject as DungeonRoom;
            room.Apply();
            room.DR.RenderArea_Editor(Vector3Int.RoundToInt(room.Room.bounds.center), room.Room.bounds.size + Vector3Int.one);
        }
    }
}
