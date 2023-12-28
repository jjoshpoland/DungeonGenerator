using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class DungeonRenderer : MonoBehaviour
{
    public Generator3D Generator;

    public float Scale = 1f;
    public Theme DefaultTheme;
    public List<Theme> Themes;
    public GameObject Floor;
    public DungeonWall Floor_DW;
    public DungeonWall Wall;
    public GameObject Ceiling;
    public DungeonWall Ceiling_DW;
    public GameObject FloorRamp;
    public GameObject RampCeilingLow;
    public GameObject RampCeilingHigh;
    public GameObject HalfFoundation;
    public GameObject SteepRamp;
    public GameObject Doorway;
    public GameObject Archway;
    public DungeonRoom RoomHandlePrefab;

    public Material RoomMaterial;
    public Material HallMaterial;
    public Material UpMaterial;

    Grid3D<GameObject> CellBases;

    [HideInInspector]
    public List<GameObject> SpawnTiles;

    public UnityEvent OnRendered;
    
    public void RenderAll()
    {
        CellBases = new Grid3D<GameObject>(Generator.Grid.Size, Generator.Grid.Offset);
        SpawnTiles = new List<GameObject>();

        foreach(Generator3D.Room room in Generator.Rooms)
        {
            CreateRoomHandle(room);
        }

        for (int x = 0; x < Generator.Size.x; x++)
        {
            for (int y = 0; y < Generator.Size.y; y++)
            {
                for (int z = 0; z < Generator.Size.z; z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    
                    Cell c = Generator.Grid[pos];

                    RenderCell(pos, c);
                    
                }
            }
            
        }

        OnRendered.Invoke();
    }

    public void CreateRoomHandle(Generator3D.Room room)
    {
        DungeonRoom newRoom = Instantiate(RoomHandlePrefab, Generator.transform);
        newRoom.Room = room;
        newRoom.DR = this;
        newRoom.transform.position = new Vector3(room.bounds.center.x * Scale, room.bounds.center.y * Scale, room.bounds.center.z * Scale);

        if (room.theme >= 0)
        {
            newRoom.RoomTheme = Themes[room.theme];
            ApplyRoom(room, newRoom.RoomTheme);
        }
        else
        {
            ApplyRoom(room, null);
        }
        
    }

    public void ApplyRoom(Generator3D.Room room, Theme roomTheme)
    {
        if (Generator.Grid == null) return;
        if(roomTheme != null && Themes.Contains(roomTheme))
        {
            room.theme = Themes.IndexOf(roomTheme);
        }
        else if(roomTheme == null)
        {
            room.theme = -1;
        }
        for (int x = room.bounds.x; x < room.bounds.xMax; x++)
        {
            for (int y = room.bounds.y; y < room.bounds.yMax; y++)
            {
                for (int z = room.bounds.z; z < room.bounds.zMax; z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);

                    Generator.Grid[pos].theme = room.theme;
                }
            }
        }

    }

    public void RenderArea_Editor(Vector3Int start, Vector3Int size)
    {
        if(CellBases == null)
        {
            Debug.LogWarning("Grid has not been initialized"); 
            Generator.LoadDungeon();
        }
        if(size.x <= 0 || size.y <= 0 || size.z <= 0)
        {
            Debug.LogError("Cannot render a negative size");
            return;
        }
        for (int x = start.x - size.x; x <= start.x + size.x; x++)
        {
            for (int y = start.y - size.y; y <= start.y + size.y; y++)
            {
                for (int z = start.z - size.z; z <= start.z + size.z; z++)
                {
                    
                    Vector3Int pos = new Vector3Int(x, y, z);

                    if (!Generator.Grid.InBounds(pos)) continue;

                    DestroyImmediate(CellBases[pos]);

                    Cell c = Generator.Grid[pos];

                    RenderCell(pos, c);

                } 
            }

        }
    }

    public void RenderCell(Vector3Int pos, Cell c)
    {
        GameObject CellBase = null;

        Theme theme = null;
        if(c.theme >= 0)
        {
            theme = Themes[c.theme];
        }
        else
        {
            theme = DefaultTheme;
        }

        if (c.CellType == CellType.Hallway)
        {
            GameObject floor = RenderWall(Floor_DW, Generator.transform, pos, Quaternion.identity, Vector3Int.down, theme.WallMaterial).gameObject;
            CellBase = floor;
            DungeonWall ceiling = RenderWall(Ceiling_DW, CellBase.transform, pos, Quaternion.identity, Vector3Int.up, theme.WallMaterial);
            
        }
        else if (c.CellType == CellType.Room)
        {
            
            if (Generator.Grid.InBounds(pos + GridMath.Directions[(int)GridDirections.Down]))
            {
                Cell down = Generator.Grid[pos + GridMath.Directions[(int)GridDirections.Down]];
                if (down.CellType != CellType.Room || down.RoomID != c.RoomID)
                {
                    GameObject floor = RenderWall(Floor_DW, Generator.transform, pos, Quaternion.identity, Vector3Int.down, theme.WallMaterial).gameObject;
                    CellBase = floor;
                    floor.GetComponent<MeshRenderer>().sharedMaterial = theme.FloorMaterial;
                }

                
            }
            else
            {
                GameObject floor = RenderWall(Floor_DW, Generator.transform, pos, Quaternion.identity, Vector3Int.down, theme.WallMaterial).gameObject; 
                CellBase = floor;
                floor.GetComponent<MeshRenderer>().sharedMaterial = theme.FloorMaterial;
            }

            if(c.SpawnRoom)
            {
                SpawnTiles.Add(CellBase);
            }

            if (Generator.Grid.InBounds(pos + GridMath.Directions[(int)GridDirections.Up]))
            {
                Cell up = Generator.Grid[pos + GridMath.Directions[(int)GridDirections.Up]];
                if (up.CellType != CellType.Room || up.RoomID != c.RoomID)
                {
                    DungeonWall ceiling = RenderWall(Ceiling_DW, Generator.transform, pos, Quaternion.identity, Vector3Int.up, theme.WallMaterial);
                    if (CellBase == null)
                    {
                        CellBase = ceiling.gameObject;
                    }
                    else
                    {
                        ceiling.transform.parent = CellBase.transform;
                    }
                    ceiling.GetComponent<MeshRenderer>().sharedMaterial = theme.WallMaterial;
                }
            }
            else
            {
                DungeonWall ceiling = RenderWall(Ceiling_DW, Generator.transform, pos, Quaternion.identity, Vector3Int.up, theme.WallMaterial);
                if (CellBase == null)
                {
                    CellBase = ceiling.gameObject;
                }
                else
                {
                    ceiling.transform.parent = CellBase.transform;
                }
                ceiling.GetComponent<MeshRenderer>().sharedMaterial = theme.WallMaterial;
            }

            //if (c.Path)
            //{
            //    Cell down = Generator.Grid[pos + Grid.Directions[(int)GridDirections.Down]];
            //    if (down.CellType == CellType.Room && down.RoomID == c.RoomID)
            //    {
            //        GameObject floor = RenderDungeonPiece(Floor, CellBase.transform, pos, Quaternion.identity, Vector3.zero, theme.FloorMaterial);
            //        floor.GetComponent<MeshRenderer>().material = theme.FloorMaterial;
            //    }
            //}

            if(c.RoomFloor)
            {
                GameObject floor = RenderDungeonPiece(Floor, CellBase.transform, pos, Quaternion.identity, Vector3.zero, theme.FloorMaterial);
                floor.GetComponent<MeshRenderer>().material = theme.FloorMaterial;
            }
            if(c.StairType != StairType.None)
            {
                Quaternion rot = Quaternion.Euler(new Vector3(0, (GridMath.HorizontalDirectionFromVector(c.StairDirection) * 90) - 90, 0));
                RenderStair(c, pos, rot, theme, false, false);
            }
            if (c.Covered)
            {
                RenderDungeonPiece(Archway, CellBase.transform, pos, Quaternion.identity, Vector3.zero, theme.WallMaterial);
            }
        }
        else if (c.CellType == CellType.Stairs)
        {
            float verticalDirection = c.StairDirection.y > 0 ? 180 : 0;
            bool up = c.StairDirection.y > 0;
            Quaternion rot = Quaternion.Euler(new Vector3(0, (GridMath.HorizontalDirectionFromVector(c.StairDirection) * 90) - 90 - verticalDirection, 0));

            CellBase = RenderStair(c, pos, rot, theme, up);

            if (CellBase != null && up)
            {
                CellBase.GetComponent<MeshRenderer>().material = theme.FloorMaterial;
            }
        }

        //if (CellBase != null)
        //{
        //    CellBase.transform.localScale = new Vector3(Scale, Scale, Scale);
        //}

        if (c.CellType != CellType.None)
        {
            for (int n = 0; n < 4; n++)
            {
                if (!Generator.Grid.InBounds(pos + GridMath.Directions[n]))
                {
                    RenderWall(Wall, CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)), GridMath.Directions[n], theme.WallMaterial);
                    continue;
                }
                Cell neighbor = Generator.Grid[pos + GridMath.Directions[n]];
                if (neighbor.CellType == CellType.None)
                {
                    RenderWall(Wall, CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)), GridMath.Directions[n], theme.WallMaterial);
                }
                else if (neighbor.CellType == CellType.Stairs)
                {
                    int d = GridMath.HorizontalDirectionFromVector(neighbor.StairDirection);
                    if (c.CellType != CellType.Stairs) //if the neighbor is a stair, but i am not
                    {

                        if (neighbor.StairType == StairType.Top || neighbor.StairType == StairType.Landing)
                        {
                            if (d != n && (d + 2) % 4 != n)
                            {
                                RenderWall(Wall, CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)), GridMath.Directions[n], theme.WallMaterial);
                            }

                        }
                        else if (c.CellType != CellType.Stairs)
                        {
                            RenderWall(Wall, CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)), GridMath.Directions[n], theme.WallMaterial);
                        }
                    }
                    else //if i am a stair and my neighbor is a stair
                    {
                        int cd = GridMath.HorizontalDirectionFromVector(c.StairDirection);

                        if (cd != d) //if not going in the same direction
                        {
                            if ((cd + 2) % 4 != d) //if not going in the opposite direction
                            {
                                //perpendicular
                                RenderWall(Wall, CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)), GridMath.Directions[n], theme.WallMaterial);
                            }
                            else
                            {
                                //parallel
                                if (c.StairType != neighbor.StairType) //criss crossing
                                {
                                    RenderWall(Wall, CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)), GridMath.Directions[n], theme.WallMaterial);
                                }

                            }
                        }
                        else
                        {
                            //parallel
                            if (c.StairType != neighbor.StairType && cd != n && (cd + 2) % 4 != n)
                            {
                                RenderWall(Wall, CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)), GridMath.Directions[n], theme.WallMaterial);
                            }
                        }
                    }

                }
                else if (c.CellType == CellType.Room)
                {
                    

                    if (neighbor.CellType == CellType.Hallway)
                    {
                        if (c.DoorWay)
                        {
                            RenderDungeonPiece(Doorway, CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)), Vector3.zero, theme.WallMaterial);
                        }
                        else
                        {
                            RenderWall(Wall, CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)), GridMath.Directions[n], theme.WallMaterial);
                        }

                    }

                    //TODO: update this case as a c.Doorway = true in the generator pass if needed. Maybe include a random chance to avoid multiple doors side by side
                    //if(neighbor.CellType == CellType.Room && neighbor.RoomID != c.RoomID)
                    //{
                    //    RenderDungeonPiece(Doorway, CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)), Vector3.zero, theme.WallMaterial);
                    //}
                }




            }
        }
        
        CellBases[pos] = CellBase;
    }

    DungeonWall RenderWall(DungeonWall wallPrefab, Transform parent, Vector3Int position, Quaternion rotation, Vector3Int orientation, Material mat)
    {
        DungeonWall newWall = Instantiate(wallPrefab);
        newWall.transform.rotation = rotation;
        newWall.transform.position = new Vector3(position.x * Scale, position.y * Scale, position.z * Scale);
        newWall.transform.localScale = new Vector3(Scale, Scale, Scale);
        newWall.transform.parent = parent;
        newWall.Position = position;
        newWall.Orientation = orientation;
        newWall.Grid = Generator.Grid;
        newWall.ParentRenderer = this;
        newWall.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return newWall;
    }

    GameObject RenderDungeonPiece(GameObject piece, Transform parent, Vector3Int position, Quaternion rotation, Vector3 offset, Material mat)
    {
        GameObject newPiece = Instantiate(piece);
        newPiece.transform.rotation = rotation;
        newPiece.transform.position = new Vector3((position.x + offset.x) * Scale, (position.y + offset.y) * Scale, (position.z + offset.z) * Scale);
        newPiece.transform.localScale = new Vector3(Scale, Scale, Scale);
        newPiece.transform.parent = parent;
        newPiece.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return newPiece;
    }

    GameObject RenderStair(Cell c, Vector3Int pos, Quaternion rot, Theme theme, bool up, bool hasCeiling = true)
    {
        GameObject stairPiece = null;
        switch (c.StairType)
        {
            case StairType.Top:
                stairPiece = RenderDungeonPiece(FloorRamp, Generator.transform, pos, rot, Vector3.zero, theme.FloorMaterial);
                if(hasCeiling)
                {
                    DungeonWall ceiling = RenderWall(Ceiling_DW, stairPiece.transform, pos, Quaternion.identity, Vector3Int.up, theme.WallMaterial);
                    if (up) ceiling.GetComponent<MeshRenderer>().material = theme.WallMaterial;
                }
                
                break;
            case StairType.Ceiling:
                if(hasCeiling)
                {
                    stairPiece = RenderWall(Ceiling_DW, Generator.transform, pos, Quaternion.identity, Vector3Int.up, theme.WallMaterial).gameObject;
                }
                
                break;
            case StairType.Landing:
                stairPiece = RenderDungeonPiece(FloorRamp, Generator.transform, pos, rot, new Vector3(0, .5f, 0), theme.FloorMaterial);

                break;
            case StairType.Staircase:
                stairPiece = RenderDungeonPiece(HalfFoundation, Generator.transform, pos, rot, new Vector3(0, .5f, 0), theme.FloorMaterial);
                break;
            default:
                break;
        }

        return stairPiece;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(new Vector3(Generator.Size.x * Scale / 2f, Generator.Size.y * Scale / 2f, Generator.Size.z * Scale / 2f), new Vector3(Generator.Size.x * Scale, Generator.Size.y * Scale, Generator.Size.z * Scale));
    }
}
