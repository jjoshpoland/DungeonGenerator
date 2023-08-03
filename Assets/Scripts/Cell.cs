using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cell
{
    public CellType CellType;
    public StairType StairType;
    public int theme;
    public Vector3Int StairDirection;
    public int RoomID;
    public bool DoorWay;
    public bool Path;
    public Vector3Int PrevPath;
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
