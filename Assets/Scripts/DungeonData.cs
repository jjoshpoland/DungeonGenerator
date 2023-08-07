using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DungeonData
{
    public string Version = "0.1";
    public List<Cell> Cells;
    public Vector3Int Size;
    public Vector3Int Offset;
}
