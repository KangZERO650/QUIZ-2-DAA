using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Grid Settings")]
    public Vector2Int gridSize = new Vector2Int(32, 32);
    public float nodeRadius = 0.5f; 
    public LayerMask obstacleMask;  

    private Node[,] nodes;

    void Awake()
    {
        Instance = this;
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        nodes = new Node[gridSize.x, gridSize.y];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridSize.x / 2 - Vector3.forward * gridSize.y / 2;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x + 0.5f) + Vector3.forward * (y + 0.5f);
                
                Vector3 checkPos = worldPoint + Vector3.up * 0.5f;

                bool isObstacle = Physics.CheckSphere(checkPos, 0.3f, obstacleMask);
                
                nodes[x, y] = new Node(new Vector2Int(x, y), worldPoint, !isObstacle);
            }
        }
    }

    public Node GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        float xOffset = worldPosition.x - transform.position.x + (gridSize.x / 2f);
        float zOffset = worldPosition.z - transform.position.z + (gridSize.y / 2f);

        int x = Mathf.FloorToInt(xOffset);
        int z = Mathf.FloorToInt(zOffset);

        x = Mathf.Clamp(x, 0, gridSize.x - 1);
        z = Mathf.Clamp(z, 0, gridSize.y - 1);

        return nodes[x, z];
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        Vector2Int[] directions = {
            new Vector2Int(0, 1),  
            new Vector2Int(0, -1), 
            new Vector2Int(1, 0),  
            new Vector2Int(-1, 0)  
        };

        foreach (var dir in directions)
        {
            int checkX = node.gridPosition.x + dir.x;
            int checkY = node.gridPosition.y + dir.y;

            if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y)
            {
                neighbors.Add(nodes[checkX, checkY]);
            }
        }

        return neighbors;
    }

    public void ResetAllNodes()
    {
        if (nodes == null) return;
        foreach (Node node in nodes)
        {
            node.ResetNode();
        }
    }

    public Node GetRandomWalkableNode(Node excludeNode = null)
    {
        if (nodes == null) return null;

        int maxAttempts = 100;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            int x = Random.Range(0, gridSize.x);
            int y = Random.Range(0, gridSize.y);
            
            Node candidateNode = nodes[x, y];

            if (candidateNode.isWalkable && candidateNode != excludeNode)
            {
                return candidateNode;
            }
            attempts++;
        }
        
        return nodes[gridSize.x / 2, gridSize.y / 2];
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridSize.x, 1, gridSize.y));

        if (nodes != null)
        {
            foreach (Node n in nodes)
            {
                Gizmos.color = n.isWalkable ? new Color(1, 1, 1, 0.2f) : new Color(1, 0, 0, 0.5f);
                Gizmos.DrawCube(n.worldPosition + Vector3.up * 0.1f, new Vector3(0.9f, 0.1f, 0.9f));
            }
        }
    }
}