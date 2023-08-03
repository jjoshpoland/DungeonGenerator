using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DungeonData
{
    public List<Cell> Cells;
    public Vector3Int Size;
    public Vector3Int Offset;
}
