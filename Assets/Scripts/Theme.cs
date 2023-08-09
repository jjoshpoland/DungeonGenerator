using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/New Theme")]
public class Theme : ScriptableObject
{
    public Material WallMaterial;
    public Material FloorMaterial;
    [Tooltip("If selected, only applies this material to room tiles. Otherwise, hallways and stairs will also be included.")]
    public bool RoomOnly;
    public int Priority;
    
    //Wall Decorations
    //Floor decorations
    //Wall lights
    //Floor lights
}
