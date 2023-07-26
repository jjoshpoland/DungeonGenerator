using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Graphs;
using UnityEngine.Events;

[ExecuteInEditMode]
public class Generator3D : MonoBehaviour {
    

    class Room {
        public BoundsInt bounds;

        public Room(Vector3Int location, Vector3Int size) {
            bounds = new BoundsInt(location, size);
        }

        public static bool Intersect(Room a, Room b) {
            return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
                || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y)
                || (a.bounds.position.z >= (b.bounds.position.z + b.bounds.size.z)) || ((a.bounds.position.z + a.bounds.size.z) <= b.bounds.position.z));
        }
    }

    [SerializeField]
    Vector3Int size;
    [SerializeField]
    int roomCount;
    [SerializeField]
    Vector3Int roomMaxSize;
    [SerializeField]
    GameObject cubePrefab;
    [SerializeField]
    Material redMaterial;
    [SerializeField]
    Material blueMaterial;
    [SerializeField]
    Material greenMaterial;
    public UnityEvent OnGenerated;

    Random random;
    Grid3D<Cell> grid;
    List<Room> rooms;
    Delaunay3D delaunay;
    HashSet<Prim.Edge> selectedEdges;

    public Grid3D<Cell> Grid => grid;
    public Vector3Int Size => size;

    public void Generate()
    {
        random = new Random((int)System.DateTime.Now.Ticks);
        grid = new Grid3D<Cell>(size, Vector3Int.zero);
        rooms = new List<Room>();

        for (int i = 0; i < grid.Data.Length; i++)
        {
            grid.Data[i] = new Cell();
        }

        PlaceRooms();
        Triangulate();
        CreateHallways();
        PathfindHallways();

        OnGenerated.Invoke();
    }

    public void EditorGenerate()
    {
        EditorClear();
        Generate();
    }

    public void EditorClear()
    {
        for (int i = this.transform.childCount; i > 0; i--)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        if(grid != null)
        {
            grid.Clear();
        }
        
    }

    public void Clear()
    {
        foreach(Transform t in transform)
        {
            Destroy(t.gameObject);
        }
        if(grid != null)
        {
            grid.Clear();
        }
    }

    void PlaceRooms() {
        for (int i = 0; i < roomCount; i++) {
            Vector3Int location = new Vector3Int(
                random.Next(0, size.x),
                random.Next(0, size.y),
                random.Next(0, size.z)
            );

            Vector3Int roomSize = new Vector3Int(
                random.Next(1, roomMaxSize.x + 1),
                random.Next(1, roomMaxSize.y + 1),
                random.Next(1, roomMaxSize.z + 1)
            );

            bool add = true;
            Room newRoom = new Room(location, roomSize);
            Room buffer = new Room(location + new Vector3Int(-1, 0, -1), roomSize + new Vector3Int(2, 0, 2));

            foreach (var room in rooms) {
                if (Room.Intersect(room, buffer)) {
                    add = false;
                    break;
                }
            }

            if (newRoom.bounds.xMin < 0 || newRoom.bounds.xMax >= size.x
                || newRoom.bounds.yMin < 0 || newRoom.bounds.yMax >= size.y
                || newRoom.bounds.zMin < 0 || newRoom.bounds.zMax >= size.z) {
                add = false;
            }

            if (add) {
                rooms.Add(newRoom);
                //PlaceRoom(newRoom.bounds.position, newRoom.bounds.size);

                foreach (var pos in newRoom.bounds.allPositionsWithin) {
                    grid[pos].CellType = CellType.Room;
                }
            }
        }
    }

    void Triangulate() {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var room in rooms) {
            vertices.Add(new Vertex<Room>((Vector3)room.bounds.position + ((Vector3)room.bounds.size) / 2, room));
        }

        delaunay = Delaunay3D.Triangulate(vertices);
    }

    void CreateHallways() {
        List<Prim.Edge> edges = new List<Prim.Edge>();

        foreach (var edge in delaunay.Edges) {
            edges.Add(new Prim.Edge(edge.U, edge.V));
        }

        List<Prim.Edge> minimumSpanningTree = Prim.MinimumSpanningTree(edges, edges[0].U);

        selectedEdges = new HashSet<Prim.Edge>(minimumSpanningTree);
        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(selectedEdges);

        foreach (var edge in remainingEdges) {
            if (random.NextDouble() < 0.125) {
                selectedEdges.Add(edge);
            }
        }
    }

    void PathfindHallways() {
        DungeonPathfinder3D aStar = new DungeonPathfinder3D(size);

        foreach (var edge in selectedEdges) {
            var startRoom = (edge.U as Vertex<Room>).Item;
            var endRoom = (edge.V as Vertex<Room>).Item;

            var startPosf = startRoom.bounds.center;
            var endPosf = endRoom.bounds.center;
            var startPos = new Vector3Int((int)startPosf.x, (int)startPosf.y, (int)startPosf.z);
            var endPos = new Vector3Int((int)endPosf.x, (int)endPosf.y, (int)endPosf.z);

            var path = aStar.FindPath(startPos, endPos, (DungeonPathfinder3D.Node a, DungeonPathfinder3D.Node b) => {
                var pathCost = new DungeonPathfinder3D.PathCost();

                var delta = b.Position - a.Position;

                if (delta.y == 0) {
                    //flat hallway
                    pathCost.cost = Vector3Int.Distance(b.Position, endPos);    //heuristic

                    if (grid[b.Position].CellType == CellType.Stairs) {
                        return pathCost;
                    } else if (grid[b.Position].CellType == CellType.Room) {
                        pathCost.cost += 5;
                    } else if (grid[b.Position].CellType == CellType.None) {
                        pathCost.cost += 1;
                    }

                    pathCost.traversable = true;
                } else {
                    //staircase
                    if ((grid[a.Position].CellType != CellType.None && grid[a.Position].CellType != CellType.Hallway)
                        || (grid[b.Position].CellType != CellType.None && grid[b.Position].CellType != CellType.Hallway)) return pathCost;

                    pathCost.cost = 100 + Vector3Int.Distance(b.Position, endPos);    //base cost + heuristic

                    int xDir = Mathf.Clamp(delta.x, -1, 1);
                    int zDir = Mathf.Clamp(delta.z, -1, 1);
                    Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                    Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

                    if (!grid.InBounds(a.Position + verticalOffset)
                        || !grid.InBounds(a.Position + horizontalOffset)
                        || !grid.InBounds(a.Position + verticalOffset + horizontalOffset)) {
                        return pathCost;
                    }

                    if (grid[a.Position + horizontalOffset].CellType != CellType.None
                        || grid[a.Position + horizontalOffset * 2].CellType != CellType.None
                        || grid[a.Position + verticalOffset + horizontalOffset].CellType != CellType.None
                        || grid[a.Position + verticalOffset + horizontalOffset * 2].CellType != CellType.None) {
                        return pathCost;
                    }

                    pathCost.traversable = true;
                    pathCost.isStairs = true;
                }

                return pathCost;
            });

            if (path != null) {
                for (int i = 0; i < path.Count; i++) {
                    var current = path[i];

                    if (grid[current].CellType == CellType.None) {
                        grid[current].CellType = CellType.Hallway;
                    }

                    if (i > 0) {
                        var prev = path[i - 1];

                        var delta = current - prev;

                        if (delta.y != 0) {
                            int xDir = Mathf.Clamp(delta.x, -1, 1);
                            int zDir = Mathf.Clamp(delta.z, -1, 1);
                            Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                            Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);
                            Vector3Int direction = new Vector3Int(xDir, delta.y, zDir);
                            bool up = delta.y > 0;
                            
                            grid[prev + horizontalOffset].CellType = CellType.Stairs;
                            grid[prev + horizontalOffset].StairType = up ? StairType.Landing : StairType.Top;
                            grid[prev + horizontalOffset].StairDirection = direction;
                            grid[prev + horizontalOffset * 2].CellType = CellType.Stairs;
                            grid[prev + horizontalOffset * 2].StairType = up ? StairType.Staircase : StairType.Ceiling;
                            grid[prev + horizontalOffset * 2].StairDirection = direction;
                            grid[prev + verticalOffset + horizontalOffset].CellType = CellType.Stairs;
                            grid[prev + verticalOffset + horizontalOffset].StairType = up ? StairType.Ceiling : StairType.Staircase;
                            grid[prev + verticalOffset + horizontalOffset].StairDirection = direction;
                            grid[prev + verticalOffset + horizontalOffset * 2].CellType = CellType.Stairs;
                            grid[prev + verticalOffset + horizontalOffset * 2].StairType = up ? StairType.Top : StairType.Landing;
                            grid[prev + verticalOffset + horizontalOffset * 2].StairDirection = direction;

                            //PlaceStairs(prev + horizontalOffset);
                            //PlaceStairs(prev + horizontalOffset * 2);
                            //PlaceStairs(prev + verticalOffset + horizontalOffset);
                            //PlaceStairs(prev + verticalOffset + horizontalOffset * 2);
                        }

                        //Debug.DrawLine(prev + new Vector3(0.5f, 0.5f, 0.5f), current + new Vector3(0.5f, 0.5f, 0.5f), Color.blue, 100, false);
                    }
                }

                //foreach (var pos in path) {
                //    if (grid[pos].CellType == CellType.Hallway) {
                //        PlaceHallway(pos);
                //    }
                //}
            }
        }
    }

    void PlaceCube(Vector3Int location, Vector3Int size, Material material) {
        GameObject go = Instantiate(cubePrefab, location, Quaternion.identity);
        go.GetComponent<Transform>().localScale = size;
        go.GetComponent<MeshRenderer>().material = material;
        go.transform.parent = transform;
    }

    void PlaceRoom(Vector3Int location, Vector3Int size) {
        PlaceCube(location, size, redMaterial);
    }

    void PlaceHallway(Vector3Int location) {
        PlaceCube(location, new Vector3Int(1, 1, 1), blueMaterial);
    }

    void PlaceStairs(Vector3Int location) {
        PlaceCube(location, new Vector3Int(1, 1, 1), greenMaterial);
    }
}
