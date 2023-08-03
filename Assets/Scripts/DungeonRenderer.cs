using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DungeonRenderer : MonoBehaviour
{
    public Generator3D Generator;

    public float Scale = 1f;

    public GameObject Floor;
    public DungeonWall Wall;
    public GameObject Ceiling;
    public GameObject FloorRamp;
    public GameObject RampCeilingLow;
    public GameObject RampCeilingHigh;
    public GameObject HalfFoundation;
    public GameObject SteepRamp;
    public GameObject Doorway;

    public Material RoomMaterial;
    public Material HallMaterial;
    public Material UpMaterial;

    Grid3D<GameObject> CellBases;
    
    public void RenderAll()
    {
        CellBases = new Grid3D<GameObject>(Generator.Grid.Size, Generator.Grid.Offset);

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
    }

    public void RenderArea_Editor(Vector3Int start, Vector3Int size)
    {
        if(CellBases == null)
        {
            Debug.LogError("Grid has not been initialized");
            return;
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

        if (c.CellType == CellType.Hallway)
        {
            GameObject floor = RenderDungeonPiece(Floor, Generator.transform, pos, Quaternion.identity, Vector3.zero);
            CellBase = floor;
            GameObject ceiling = RenderDungeonPiece(Ceiling, CellBase.transform, pos, Quaternion.identity, Vector3.zero);
            if (c.CellType == CellType.Room)
            {
                ceiling.GetComponent<MeshRenderer>().sharedMaterial = RoomMaterial;
            }
        }
        else if (c.CellType == CellType.Room)
        {
            if (Generator.Grid.InBounds(pos + Grid.Directions[(int)GridDirections.Down]))
            {
                Cell down = Generator.Grid[pos + Grid.Directions[(int)GridDirections.Down]];
                if (down.CellType != CellType.Room || down.RoomID != c.RoomID)
                {
                    GameObject floor = RenderDungeonPiece(Floor, Generator.transform, pos, Quaternion.identity, Vector3.zero);
                    CellBase = floor;
                    floor.GetComponent<MeshRenderer>().sharedMaterial = RoomMaterial;
                }
            }
            else
            {
                GameObject floor = RenderDungeonPiece(Floor, Generator.transform, pos, Quaternion.identity, Vector3.zero);
                CellBase = floor;
                floor.GetComponent<MeshRenderer>().sharedMaterial = RoomMaterial;
            }

            if (Generator.Grid.InBounds(pos + Grid.Directions[(int)GridDirections.Up]))
            {
                Cell up = Generator.Grid[pos + Grid.Directions[(int)GridDirections.Up]];
                if (up.CellType != CellType.Room || up.RoomID != c.RoomID)
                {
                    GameObject ceiling = RenderDungeonPiece(Ceiling, Generator.transform, pos, Quaternion.identity, Vector3.zero);
                    if(CellBase == null)
                    {
                        CellBase = ceiling;
                    }
                    else
                    {
                        ceiling.transform.parent = CellBase.transform;
                    }
                    ceiling.GetComponent<MeshRenderer>().sharedMaterial = RoomMaterial;
                }
            }
            else
            {
                GameObject ceiling = RenderDungeonPiece(Ceiling, Generator.transform, pos, Quaternion.identity, Vector3.zero);
                if (CellBase == null)
                {
                    CellBase = ceiling;
                }
                else
                {
                    ceiling.transform.parent = CellBase.transform;
                }
                ceiling.GetComponent<MeshRenderer>().sharedMaterial = RoomMaterial;
            }
        }
        else if (c.CellType == CellType.Stairs)
        {
            float verticalDirection = c.StairDirection.y > 0 ? 180 : 0;
            bool up = c.StairDirection.y > 0;
            Quaternion rot = Quaternion.Euler(new Vector3(0, (Grid.HorizontalDirectionFromVector(c.StairDirection) * 90) - 90 - verticalDirection, 0));

            switch (c.StairType)
            {
                case StairType.Top:
                    CellBase = RenderDungeonPiece(FloorRamp, Generator.transform, pos, rot, Vector3.zero);

                    GameObject ceiling = RenderDungeonPiece(Ceiling, CellBase.transform, pos, rot, Vector3.zero);
                    if (up) ceiling.GetComponent<MeshRenderer>().material = UpMaterial;
                    break;
                case StairType.Ceiling:
                    CellBase = RenderDungeonPiece(Ceiling, Generator.transform, pos, rot, Vector3.zero);
                    break;
                case StairType.Landing:
                    CellBase = RenderDungeonPiece(FloorRamp, Generator.transform, pos, rot, new Vector3(0, .5f, 0));

                    break;
                case StairType.Staircase:
                    CellBase = RenderDungeonPiece(HalfFoundation, Generator.transform, pos, rot, new Vector3(0, .5f, 0));
                    break;
                default:
                    break;
            }

            if (CellBase != null && up)
            {
                CellBase.GetComponent<MeshRenderer>().material = UpMaterial;
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
                if (!Generator.Grid.InBounds(pos + Grid.Directions[n]))
                {
                    RenderWall(CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)), Grid.Directions[n]);
                    continue;
                }
                Cell neighbor = Generator.Grid[pos + Grid.Directions[n]];
                if (neighbor.CellType == CellType.None)
                {
                    RenderWall(CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)), Grid.Directions[n]);
                }
                else if (neighbor.CellType == CellType.Stairs)
                {
                    int d = Grid.HorizontalDirectionFromVector(neighbor.StairDirection);
                    if (c.CellType != CellType.Stairs) //if the neighbor is a stair, but i am not
                    {

                        if (neighbor.StairType == StairType.Top || neighbor.StairType == StairType.Landing)
                        {
                            if (d != n && (d + 2) % 4 != n)
                            {
                                RenderWall(CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)), Grid.Directions[n]);
                            }

                        }
                        else if (c.CellType != CellType.Stairs)
                        {
                            RenderWall(CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)), Grid.Directions[n]);
                        }
                    }
                    else //if i am a stair and my neighbor is a stair
                    {
                        int cd = Grid.HorizontalDirectionFromVector(c.StairDirection);

                        if (cd != d) //if not going in the same direction
                        {
                            if ((cd + 2) % 4 != d) //if not going in the opposite direction
                            {
                                //perpendicular
                                RenderWall(CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)), Grid.Directions[n]);
                            }
                            else
                            {
                                //parallel
                                if (c.StairType != neighbor.StairType) //criss crossing
                                {
                                    RenderWall(CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)), Grid.Directions[n]);
                                }
                            }
                        }
                        else
                        {
                            //parallel and aligned
                        }
                    }

                }
                else if (c.CellType == CellType.Room)
                {
                    if (c.Path && (neighbor.CellType == CellType.Hallway || neighbor.CellType == CellType.Stairs))
                    {
                        Cell down = Generator.Grid[pos + Grid.Directions[(int)GridDirections.Down]];
                        if (down.CellType == CellType.Room && down.RoomID == c.RoomID)
                        {
                            GameObject steepStair = RenderDungeonPiece(SteepRamp, CellBase.transform, pos + Vector3Int.down, Quaternion.Euler(new Vector3(0, (n * 90) + 90, 0)), Vector3.zero);
                            steepStair.GetComponent<MeshRenderer>().material = RoomMaterial;
                        }
                    }

                    if (neighbor.CellType == CellType.Hallway)
                    {
                        if (c.DoorWay)
                        {
                            RenderDungeonPiece(Doorway, CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)), Vector3.zero);
                        }
                        else
                        {
                            RenderWall(CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)), Grid.Directions[n]);
                        }

                    }

                }




            }
        }
        
        CellBases[pos] = CellBase;
    }

    DungeonWall RenderWall(Transform parent, Vector3Int position, Quaternion rotation, Vector3Int orientation)
    {
        DungeonWall newWall = Instantiate(Wall);
        newWall.transform.rotation = rotation;
        newWall.transform.position = new Vector3(position.x * Scale, position.y * Scale, position.z * Scale);
        newWall.transform.localScale = new Vector3(Scale, Scale, Scale);
        newWall.transform.parent = parent;
        newWall.Position = position;
        newWall.Orientation = orientation;
        newWall.Grid = Generator.Grid;
        newWall.ParentRenderer = this;
        return newWall;
    }

    GameObject RenderDungeonPiece(GameObject piece, Transform parent, Vector3Int position, Quaternion rotation, Vector3 offset)
    {
        GameObject newPiece = Instantiate(piece);
        newPiece.transform.rotation = rotation;
        newPiece.transform.position = new Vector3((position.x + offset.x) * Scale, (position.y + offset.y) * Scale, (position.z + offset.z) * Scale);
        newPiece.transform.localScale = new Vector3(Scale, Scale, Scale);
        newPiece.transform.parent = parent;
        return newPiece;
    }

}
