using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;

[EditorTool("Wall Tool", typeof(DungeonWall))]
public class WallTool : EditorTool, IDrawSelectedHandles
{
    CellType cellType = CellType.Hallway;
    bool doorway = true;

    [Shortcut("Activate Wall Tool", typeof(SceneView), KeyCode.D)]
    static void PlatformToolShortcut()
    {
        if (Selection.GetFiltered<DungeonWall>(SelectionMode.TopLevel).Length > 0)
            ToolManager.SetActiveTool<WallTool>();
        else
            Debug.Log("No walls selected!");
    }
    public override void OnActivated()
    {
        SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Entering Wall Tool"), .1f);
    }

    public override void OnToolGUI(EditorWindow window)
    {
        //base.OnToolGUI(window);
        if (!(window is SceneView sceneView))
            return;

        Handles.BeginGUI();
        using (new GUILayout.HorizontalScope())
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("H = Hallway");
                GUILayout.Label("F = Room without doorway");
                GUILayout.Label("D = Room with door");
                GUILayout.Label("N = Erase");
                GUILayout.Label("S = Stairs Up");
                //doorway = EditorGUILayout.Toggle("Doorway", doorway);

                //if (GUILayout.Button("Hallway"))
                //{
                //    cellType = CellType.Hallway;
                //    SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Hallway"), .1f);
                //}

                //if (GUILayout.Button("Room"))
                //{

                //    cellType = CellType.Room;
                //    SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Room"), .1f);
                //}

                //if (GUILayout.Button("None"))
                //{
                //    SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Empty"), .1f);
                //    cellType = CellType.None;
                //}

            }

            GUILayout.FlexibleSpace();
        }
        Handles.EndGUI();


        foreach (var obj in targets)
        {
            if(!(obj is DungeonWall wall))
            {
                continue;
            }

            if(Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.N)
                {
                    Extrude(wall, CellType.None);
                }
                if (Event.current.keyCode == KeyCode.H)
                {
                    Extrude(wall, CellType.Hallway);
                }

                if (Event.current.keyCode == KeyCode.F)
                {
                    doorway = false;
                    Extrude(wall, CellType.Room);
                }

                if (Event.current.keyCode == KeyCode.D)
                {
                    doorway = true;
                    Extrude(wall, CellType.Room);
                }

                if (Event.current.keyCode == KeyCode.S)
                {
                    ExtrudeStair(wall);
                }
            }
        }
    }

    void Extrude(DungeonWall wall, CellType ct)
    {
        Vector3Int orientation = ct != CellType.None ? wall.Orientation : Vector3Int.zero;
        if (wall.Grid.InBounds(wall.Position + orientation))
        {
            Cell newCell = new Cell();
            wall.Grid[wall.Position + orientation] = newCell;
            newCell.CellType = ct;
            if(ct == CellType.Room)
            {
                newCell.DoorWay = doorway;
            }
            
        }
        else
        {
            Debug.LogWarning("You hit the edge");
        }

        wall.ParentRenderer.RenderArea_Editor(wall.Position, new Vector3Int(2, 2, 2));
    }

    void ExtrudeStair(DungeonWall wall)
    {
        CellType ct = CellType.Stairs;
        if(wall.Grid.InBounds(wall.Position + wall.Orientation) && wall.Grid.InBounds(wall.Position + wall.Orientation + Vector3Int.up))
        {
            wall.Grid[wall.Position + wall.Orientation] = new Cell();
            wall.Grid[wall.Position + wall.Orientation].CellType = CellType.Stairs;
            wall.Grid[wall.Position + wall.Orientation].StairType =StairType.Landing;
            wall.Grid[wall.Position + wall.Orientation].StairDirection = wall.Orientation + Vector3Int.up;
            wall.Grid[wall.Position + wall.Orientation * 2] = new Cell();
            wall.Grid[wall.Position + wall.Orientation * 2].CellType = CellType.Stairs;
            wall.Grid[wall.Position + wall.Orientation * 2].StairType = StairType.Staircase;
            wall.Grid[wall.Position + wall.Orientation * 2].StairDirection = wall.Orientation + Vector3Int.up;
            wall.Grid[wall.Position + Vector3Int.up + wall.Orientation] = new Cell();
            wall.Grid[wall.Position + Vector3Int.up + wall.Orientation].CellType = CellType.Stairs;
            wall.Grid[wall.Position + Vector3Int.up + wall.Orientation].StairType = StairType.Ceiling;
            wall.Grid[wall.Position + Vector3Int.up + wall.Orientation].StairDirection = wall.Orientation + Vector3Int.up;
            wall.Grid[wall.Position + Vector3Int.up + wall.Orientation * 2] = new Cell();
            wall.Grid[wall.Position + Vector3Int.up + wall.Orientation * 2].CellType = CellType.Stairs;
            wall.Grid[wall.Position + Vector3Int.up + wall.Orientation * 2].StairType = StairType.Top;
            wall.Grid[wall.Position + Vector3Int.up + wall.Orientation * 2].StairDirection = wall.Orientation + Vector3Int.up;

            wall.ParentRenderer.RenderArea_Editor(wall.Position, new Vector3Int(4, 4, 4));
        }
    }
    public void OnDrawHandles()
    {
        
    }

}
