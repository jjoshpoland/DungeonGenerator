using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public CellType CellType;
    public StairType StairType;
    public Theme theme;
    public Vector3Int StairDirection;
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
