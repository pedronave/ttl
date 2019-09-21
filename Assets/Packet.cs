using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Packet : MonoBehaviour
{
    public Grid grid;
    public bool selected;

    public TileBase pathTile; 
    public Tilemap pathTilemap;

    public Path path;

    public List<Vector3Int> positions;

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
        path = new Path(grid.WorldToCell(transform.position), pathTilemap, pathTile);
        h = hTile;
        v = vTile;
        rb = rbTile;
        lb = lbTile;
        rt = rtTile;
        lt =ltTile;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (grid.WorldToCell(mousePos).Equals(grid.WorldToCell(transform.position)))
            {
                Debug.Log("clicked packet");
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
            path.AddDestination(mouseCell);
            positions = path.PathPositions();
        }

        
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
        PathNode searchNode = head;
        bool foundSame = false;

        while (searchNode.next != null)
        {
            // If the position being added is already on the path, we want the path to stop at that position, removing the following steps.
            if (searchNode.value.Equals(position))
            {
                ClearFollowingSteps(searchNode.next);
                searchNode.next = null;
                foundSame = true;
                break;
            }else
            {
                searchNode = searchNode.next;
            }
        }

        PathNode pathEnd = searchNode;


        if (!foundSame && !searchNode.value.Equals(position))
        {
            Vector3Int relative = position - searchNode.value;
            while (Mathf.Abs(relative.x) > 1 || Mathf.Abs(relative.y) > 1)
            {
                Vector3Int correctivePos = searchNode.value;

                // Skipped intermediate positions.
                if (Mathf.Abs(relative.x) > 1)
                {
                    correctivePos += new Vector3Int(Mathf.RoundToInt(Mathf.Sign(relative.x)), 0, 0);
                } else
                {
                    correctivePos += new Vector3Int(0, Mathf.RoundToInt(Mathf.Sign(relative.y)), 0);
                }

                PathNode correctiveNode = new PathNode(correctivePos);
                correctiveNode.previous = searchNode;
                searchNode.next = correctiveNode;
                searchNode = correctiveNode;

                relative = position - searchNode.value;
            }

            if (relative.x != 0 && relative.y != 0)
            {
                // There is a diagonal that needs to be fixed.
                float rng = Random.Range(0f, 1f);

                Vector3Int intermediatePosition = searchNode.value;

                if (rng >= 0.5f)
                {
                    intermediatePosition += new Vector3Int(relative.x, 0, 0);
                }
                else
                {
                    intermediatePosition += new Vector3Int(0, relative.y, 0);
                }
                PathNode intermediateNode = new PathNode(intermediatePosition);

                searchNode.next = intermediateNode;
                intermediateNode.previous = searchNode;
                searchNode = searchNode.next;
            }

            PathNode newNode = new PathNode(position);

            searchNode.next = newNode;
            newNode.previous = searchNode;
            tail = newNode;
            DrawPath(pathEnd);
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

    private void AddCell(Vector3Int cell)
    {

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
        while (start != null)
        {
            DrawTile(start);
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