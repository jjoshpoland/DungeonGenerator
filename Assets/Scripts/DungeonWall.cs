using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonWall : MonoBehaviour
{
    public DungeonRenderer ParentRenderer;
    public Vector3Int Position;
    public Vector3Int Orientation;
    public Grid3D<Cell> Grid;
}
