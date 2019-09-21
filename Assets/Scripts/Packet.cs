using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Packet : MonoBehaviour
{
    public PacketAsset packet;
    private SpriteRenderer sr;

    public Vector3 spritePivot = new Vector3(0.5f, 0.5f);

    public Grid grid;
    public bool selected;

    public TileBase pathTile; 
    public Tilemap pathTilemap;

    public Path path;

    public List<Vector3Int> positions;
    private Vector3Int lastMouseCell;

    public static TileBase h; // Horizontal path
    public static TileBase v; // Vertical path
    public static TileBase rb; // Corner right bottom
    public static TileBase lb; // Corner left bottom
    public static TileBase rt; // Corner right top
    public static TileBase lt; // Corner left top

    public TileBase hTile; // Horizontal path
    public TileBase vTile; // Vertical path
    public TileBase rbTile; // Corner right bottom
    public TileBase lbTile; // Corner left bottom
    public TileBase rtTile; // Corner right top
    public TileBase ltTile; // Corner left top



    // Start is called before the first frame update
    void Start()
    {
        if (grid == null)
        {
            grid = FindObjectOfType<Grid>();
        }

        if (pathTilemap == null)
        {
            GameObject temp = new GameObject(string.Format("{0} path", packet.name));

            GameObject tilemap = Instantiate(temp, grid.transform);
            Destroy(temp);
            tilemap.AddComponent<Tilemap>();
            tilemap.AddComponent<TilemapRenderer>();
            pathTilemap = tilemap.GetComponent<Tilemap>();
        }

        sr = GetComponent<SpriteRenderer>();
        sr.sprite = packet.sprite;

        path = new Path(grid.WorldToCell(transform.position), pathTilemap, pathTile);
        h = hTile;
        v = vTile;
        rb = rbTile;
        lb = lbTile;
        rt = rtTile;
        lt = ltTile;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (grid.WorldToCell(mousePos).Equals(grid.WorldToCell(transform.position)))
            {
                // Reset
                selected = true;
                path.Clear();
                path = new Path(grid.WorldToCell(mousePos), pathTilemap, pathTile);
            }
        }

        if (Input.GetMouseButtonUp(0) && selected)
        {
            selected = false;
        }

        if (Input.GetMouseButton(0) && selected)
        {
            // Register path
            Vector3Int mouseCell = grid.WorldToCell(mousePos);
            if (!mouseCell.Equals(lastMouseCell))
            {
                path.AddDestination(mouseCell);
                positions = path.PathPositions();
            }
            lastMouseCell = mouseCell;
        }
        
    }

    private void FixedUpdate()
    {
        Vector3 newPos = grid.CellToWorld(path.Move()) + spritePivot;

        transform.position = newPos;
        positions = path.PathPositions();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Packet other = collision.GetComponent<Packet>();
        if (other != null)
        {

        }
    }

    private void OnDestroy()
    {
        Destroy(pathTilemap.gameObject);
    }
}

public class Path
{
    PathNode head;
    PathNode tail;

    Tilemap pathMap;
    TileBase tile;

    public Path(Vector3Int start, Tilemap pathMap, TileBase tile)
    {
        PathNode node = new PathNode(start);
        head = node;
        tail = node;

        this.pathMap = pathMap;
        this.tile = tile;
    }

    public void AddDestination(Vector3Int position)
    {
        PathNode end = tail;
        LinkedList<Vector3Int> cellsToAdd = new LinkedList<Vector3Int>();

        Vector3Int relative = position - end.value;
        Vector3Int correctivePos = end.value;


        while (Mathf.Abs(relative.x) > 1 || Mathf.Abs(relative.y) > 1)
        {
            Vector3Int relDirection = new Vector3Int(SignOrZero(relative.x), SignOrZero(relative.y), 0);
            
            if (relDirection.x != 0 && relDirection.y != 0)
            {
                float rng = Random.Range(0f, 1f);

                // Pick a random direction
                correctivePos += new Vector3Int(relDirection.x * Mathf.RoundToInt(rng), relDirection.y * Mathf.RoundToInt(1 - rng), 0);
            }
            else
            {
                // If only one has sign, or both are 0, we can add the direction;
                correctivePos += relDirection;
            }

            AddCellToEnd(correctivePos);

            relative = position - correctivePos;
        }

        
        if (relative.x != 0 && relative.y != 0)
        {
            // There is a diagonal that needs to be fixed.
            float rng = Random.Range(0f, 1f);

            Vector3Int intermediatePosition = correctivePos;

            if (rng >= 0.5f)
            {
                intermediatePosition += new Vector3Int(relative.x, 0, 0);
            }
            else
            {
                intermediatePosition += new Vector3Int(0, relative.y, 0);
            }

            AddCellToEnd(intermediatePosition);
        }

        AddCellToEnd(position);
    }

    /// <summary>
    /// If there is a next cell on the path, moves that to the head of the path.
    /// </summary>
    /// <returns>Head of the path.</returns>
    public Vector3Int Move()
    {
        if (head.next != null)
        {
            if (pathMap != null)
            {
                pathMap.SetTile(head.value, null);
                pathMap.SetTile(head.next.value, null);
            }
            

            head = head.next;
            head.previous = null;

            return head.value;
        }else
        {
            return head.value;
        }
    }

    public List<Vector3Int> PathPositions()
    {
        List<Vector3Int> path = new List<Vector3Int>();

        PathNode searchNode = head;
        while (searchNode != null)
        {
            path.Add(searchNode.value);
            searchNode = searchNode.next;
        }

        return path;
    }

    public void Clear()
    {
        ClearFollowingSteps(head);
    }

    private int SignOrZero(int n)
    {
        return n == 0 ? 0 : Mathf.RoundToInt(Mathf.Sign(n));
    }

    private void AddCellToEnd(Vector3Int cell)
    {
        PathNode searchNode = head;
        PathNode newNode = new PathNode(cell);

        do
        {
            // If the position being added is already on the path, we want the path to stop at that position, removing the following steps.
            if (searchNode.value.Equals(cell))
            {
                ClearFollowingSteps(searchNode);
                searchNode.next = null;
                tail = searchNode;

                DrawPath(searchNode);

                return;
            }
            else
            {
                if (searchNode.next != null)
                {
                    searchNode = searchNode.next;
                }
            }
        } while (searchNode.next != null);


        searchNode.next = newNode;
        newNode.previous = searchNode;
        tail = newNode;


        // Draw the path for the updates tiles
        if (searchNode.previous != null)
        {
            DrawPath(searchNode.previous);
        }else
        {
            DrawPath(searchNode);
        }
    }

    private void ClearFollowingSteps(PathNode node)
    {
        PathNode tmpNode = node;
        while(tmpNode != null)
        {
            pathMap.SetTile(tmpNode.value, null);
            tmpNode = tmpNode.next;
        }
        
    }

    private void DrawPath(PathNode start)
    {
        
        // If we're starting at the head of the path, skip it as to not draw it
        

        while (start != null)
        {
            if (start.value != head.value && pathMap != null)
            {
                DrawTile(start);
            }
            
            start = start.next;
        }
    }

    private void DrawTile(PathNode node)
    {
        TileBase tile = Packet.h;
        Vector3Int nextPosition = node.next != null ? node.next.value : node.value;
        if (node.previous == null)
        {
            // If we're at the head of the path we don't want to draw anything
            return;
        }
        Vector3Int previousPosition = node.previous.value;

        Vector3Int relativeNext = nextPosition - node.value;
        Vector3Int relativePrev = node.value - previousPosition;

        // Only have to check one direction at a time because packets can't move diagonally
        if (relativeNext.x > 0)
        {
            // Goes right
            if (relativePrev.x > 0)
            {
                // Comes from left
                // Horizontal tile
                tile = Packet.h;
            }
            else if (relativePrev.y > 0)
            {
                // Comes from bottom
                // corner r b
                tile = Packet.rb;
            }
            else if (relativePrev.y < 0)
            {
                // Comes from top
                // corner r t
                tile = Packet.rt;
            }
            else
            {
                // Nothing previous
                // Draw horizontal
                tile = Packet.h;
            }

        }
        else if (relativeNext.x < 0)
        {
            // Goes left

            if (relativePrev.x < 0)
            {
                // Comes from right
                // Horizontal tile
                tile = Packet.h;
            }
            else if (relativePrev.y > 0)
            {
                // Comes from bottom
                // corner l b
                tile = Packet.lb;
            }
            else if (relativePrev.y < 0)
            {
                // Comes from top
                // corner l t
                tile = Packet.lt;
            }
            else
            {
                // Nothing previous
                // Draw horizontal
                tile = Packet.h;
            }
        }
        else if (relativeNext.y > 0)
        {
            // Going up

            if (relativePrev.x < 0)
            {
                // Comes from right
                // corner r t
                tile = Packet.rt;
            }
            else if (relativePrev.x > 0)
            {
                // Comes from left
                // corner l t
                tile = Packet.lt;
            }
            else if (relativePrev.y > 0)
            {
                // Comes from bottom
                // Vertical tile
                tile = Packet.v;
            }
            else
            {
                // Nothing previous
                // Draw vertical
                tile = Packet.v;
            }
        }
        else if (relativeNext.y < 0)
        {
            // goes down

            if (relativePrev.x < 0)
            {
                // Comes from right
                // corner r b
                tile = Packet.rb;
            }
            else if (relativePrev.x > 0)
            {
                // Comes from left
                // corner l b
                tile = Packet.lb;
            }
            else if (relativePrev.y < 0)
            {
                // Comes from top
                // Vertical tile
                tile = Packet.v;
            }
            else
            {
                // Nothing previous
                // Draw vertical
                tile = Packet.v;
            }
        }else
        {
            // No next, check where previous comes from and draw a straight path from it
            if (relativePrev.x != 0)
            {
                // Comes from sides 
                // Horizontal tile
                tile = Packet.h;
            }
            else if (relativePrev.y != 0)
            {
                // Comes from top or bottom
                // Vertical
                tile = Packet.v;
            }
        }

        pathMap.SetTile(node.value, tile);
    }

}

public class PathNode
{
    public Vector3Int value;
    public PathNode next;
    public PathNode previous;

    public PathNode(Vector3Int position)
    {
        value = position;
        next = null;
        previous = null;
    }
}