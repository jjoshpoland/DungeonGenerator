using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DungeonRenderer : MonoBehaviour
{
    public Generator3D Generator;

    public GameObject Floor;
    public GameObject Wall;
    public GameObject Ceiling;
    public GameObject FloorRamp;
    public GameObject RampCeilingLow;
    public GameObject RampCeilingHigh;
    public GameObject HalfFoundation;
    public GameObject SteepRamp;

    public Material RoomMaterial;
    public Material HallMaterial;
    public Material UpMaterial;
    
    public void Render()
    {
        for (int x = 0; x < Generator.Size.x; x++)
        {
            for (int y = 0; y < Generator.Size.y; y++)
            {
                for (int z = 0; z < Generator.Size.z; z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    
                    Cell c = Generator.Grid[pos];

                    GameObject CellBase = null;

                    if (c.CellType == CellType.Hallway)
                    {
                        GameObject floor = Instantiate(Floor, Generator.transform);
                        floor.transform.position = pos;
                        CellBase = floor;

                        GameObject ceiling = Instantiate(Ceiling, floor.transform);
                        

                        if(c.CellType == CellType.Room)
                        {
                            ceiling.GetComponent<MeshRenderer>().sharedMaterial = RoomMaterial;
                        }
                    }
                    else if(c.CellType == CellType.Room)
                    {
                        if(Generator.Grid.InBounds(pos + Grid.Directions[(int)GridDirections.Down]))
                        {
                            Cell down = Generator.Grid[pos + Grid.Directions[(int)GridDirections.Down]];
                            if(down.CellType != CellType.Room || down.RoomID != c.RoomID)
                            {
                                GameObject floor = Instantiate(Floor, Generator.transform);
                                floor.transform.position = pos;
                                CellBase = floor;
                                floor.GetComponent<MeshRenderer>().sharedMaterial = RoomMaterial;
                            }
                        }
                        else
                        {
                            GameObject floor = Instantiate(Floor, Generator.transform);
                            floor.transform.position = pos;
                            CellBase = floor;
                            floor.GetComponent<MeshRenderer>().sharedMaterial = RoomMaterial;
                        }

                        if (Generator.Grid.InBounds(pos + Grid.Directions[(int)GridDirections.Up]))
                        {
                            Cell up = Generator.Grid[pos + Grid.Directions[(int)GridDirections.Up]];
                            if (up.CellType != CellType.Room || up.RoomID != c.RoomID || up.Path)
                            {
                                GameObject ceiling = Instantiate(Ceiling, Generator.transform);
                                ceiling.transform.position = pos;
                                CellBase = ceiling;
                                ceiling.GetComponent<MeshRenderer>().sharedMaterial = RoomMaterial;
                            }
                        }
                        else
                        {
                            GameObject ceiling = Instantiate(Ceiling, Generator.transform);
                            ceiling.transform.position = pos;
                            CellBase = ceiling;
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
                                CellBase = Instantiate(FloorRamp, Generator.transform);
                                CellBase.transform.rotation = rot;
                                CellBase.transform.position = pos;

                                GameObject ceiling = Instantiate(Ceiling, CellBase.transform);
                                ceiling.transform.position = pos;
                                if (up) ceiling.GetComponent<MeshRenderer>().material = UpMaterial;
                                break;
                            case StairType.Ceiling:
                                CellBase = Instantiate(Ceiling, Generator.transform);
                                CellBase.transform.rotation = rot;
                                CellBase.transform.position = pos;
                                break;
                            case StairType.Landing:
                                CellBase = Instantiate(FloorRamp, Generator.transform);
                                CellBase.transform.rotation = rot;
                                CellBase.transform.position = pos + new Vector3(0, .5f, 0);

                                break;
                            case StairType.Staircase:
                                CellBase = Instantiate(HalfFoundation, Generator.transform);
                                CellBase.transform.position = pos + new Vector3(0, .5f, 0);

                                break;
                            default:
                                break;
                        }

                        if(CellBase != null && up)
                        {
                            CellBase.GetComponent<MeshRenderer>().material = UpMaterial;
                        }
                    }

                    if(c.CellType != CellType.None)
                    {
                        for (int n = 0; n < 4; n++)
                        {
                            if (!Generator.Grid.InBounds(pos + Grid.Directions[n]))
                            {
                                RenderWall(CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)));
                                continue;
                            }
                            Cell neighbor = Generator.Grid[pos + Grid.Directions[n]];
                            if (neighbor.CellType == CellType.None)
                            {
                                RenderWall(CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)));
                            }
                            else if (neighbor.CellType == CellType.Stairs /*&& (neighbor.StairType == StairType.Top || neighbor.StairType == StairType.Ceiling)*/)
                            {
                                int d = Grid.HorizontalDirectionFromVector(neighbor.StairDirection);
                                if (c.CellType != CellType.Stairs) //if the neighbor is a stair, but i am not
                                {
                                    
                                    if (neighbor.StairType == StairType.Top || neighbor.StairType == StairType.Landing)
                                    {
                                        if (d != n && (d + 2) % 4 != n)
                                        {
                                            RenderWall(CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)));
                                        }

                                    }
                                    else if (c.CellType != CellType.Stairs/* || (c.CellType == CellType.Stairs && c.StairType == StairType.Ceiling)*/)
                                    {
                                        RenderWall(CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)));
                                    }
                                }
                                else //if i am a stair and my neighbor is a stair
                                {
                                    int cd = Grid.HorizontalDirectionFromVector(c.StairDirection);
                                    
                                    if (cd != d) //if not going in the same direction
                                    {
                                        if((cd + 2) % 4 != d) //if not going in the opposite direction
                                        {
                                            //perpendicular
                                            RenderWall(CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)));
                                        }
                                        else
                                        {
                                            //parallel
                                            if(c.StairType != neighbor.StairType) //criss crossing
                                            {
                                                RenderWall(CellBase.transform, pos, Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0)));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //parallel and aligned
                                    }
                                }
                                
                            }
                            else if(c.CellType == CellType.Room && c.Path && neighbor.CellType == CellType.Hallway)
                            {
                                Cell down = Generator.Grid[pos + Grid.Directions[(int)GridDirections.Down]];
                                if (down.CellType == CellType.Room && down.RoomID == c.RoomID)
                                {
                                    GameObject steepStair = Instantiate(SteepRamp, Generator.transform);
                                    steepStair.transform.position = pos;
                                    steepStair.transform.rotation = Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0));
                                    CellBase = steepStair;
                                    steepStair.GetComponent<MeshRenderer>().sharedMaterial = RoomMaterial;
                                }
                            }
                            
                        }
                    }
                    
                }
            }
            
        }
    }

    GameObject RenderWall(Transform parent, Vector3 position, Quaternion rotation)
    {
        GameObject newWall = Instantiate(Wall, parent);
        newWall.transform.rotation = rotation;
        newWall.transform.position = position;
        return newWall;
    }

}
