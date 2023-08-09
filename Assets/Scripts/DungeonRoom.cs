using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRoom : MonoBehaviour
{
    public Theme RoomTheme;
    public float HandleSize;
    [HideInInspector]
    public Generator3D.Room Room;
    [HideInInspector]
    public DungeonRenderer DR;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(HandleSize, HandleSize, HandleSize));
    }


    public void Apply()
    {
        if (DR != null && Room != null)
        {
            DR.ApplyRoom(Room, RoomTheme);
        }
    }
}
