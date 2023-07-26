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
    public GameObject RampCeiling;

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

                    if (c.CellType == CellType.Hallway || c.CellType == CellType.Room)
                    {
                        GameObject floor = Instantiate(Floor, Generator.transform);
                        floor.transform.position = pos;
                        CellBase = floor;

                        if(c.CellType == CellType.Room)
                        {
                            floor.GetComponent<MeshRenderer>().sharedMaterial = RoomMaterial;
                        }

                        for (int n = 0; n < 4; n++)
                        {
                            if (!Generator.Grid.InBounds(pos + Grid.Directions[n]))
                            {
                                GameObject wall = Instantiate(Wall, CellBase.transform);
                                wall.transform.position = pos;
                                wall.transform.rotation = Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0));
                                continue;
                            }
                            Cell neighbor = Generator.Grid[pos + Grid.Directions[n]];
                            if(neighbor.CellType == CellType.None)
                            {
                                GameObject wall = Instantiate(Wall, CellBase.transform);
                                wall.transform.position = pos;
                                wall.transform.rotation = Quaternion.Euler(new Vector3(0, (n * 90) + 180, 0));
                            }
                        }
                    }
                    else if (c.CellType == CellType.Stairs)
                    {
                        float verticalDirection = c.StairDirection.y > 0 ? 180 : 0;
                        bool up = c.StairDirection.y > 0;
                        Quaternion rot = Quaternion.Euler(new Vector3(0, (Grid.DirectionFromVector(c.StairDirection) * 90) - 90 - verticalDirection, 0));

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
                                CellBase = Instantiate(RampCeiling, Generator.transform);
                                CellBase.transform.rotation = rot;
                                CellBase.transform.position = pos;
                                break;
                            case StairType.Landing:
                                CellBase = Instantiate(FloorRamp, Generator.transform);
                                CellBase.transform.rotation = rot;
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
                }
            }
            
        }
    }
}
