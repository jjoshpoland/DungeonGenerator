using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cell
{
    public CellType CellType;
    public StairType StairType;
    public int theme = -1;
    public Vector3Int StairDirection;
    public int RoomID = -1;
    public bool DoorWay;
    public bool Path;
    public Vector3Int PrevPath;
    public bool SpawnRoom;
}

public enum CellType
{
    None,
    Room,
    Hallway,
    Stairs
}

public enum StairType
{
    Top,
    Ceiling,
    Staircase,
    Landing
}
