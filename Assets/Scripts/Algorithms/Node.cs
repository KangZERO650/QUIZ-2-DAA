using UnityEngine;

public class Node
{
    public Vector2Int gridPosition;
    public Vector3 worldPosition;
    public bool isWalkable;

    public int gCost;
    public Node parent;

    public Node(Vector2Int gridPos, Vector3 worldPos, bool walkable)
    {
        this.gridPosition = gridPos;
        this.worldPosition = worldPos;
        this.isWalkable = walkable;
        ResetNode();
    }

    public void ResetNode()
    {
        gCost = int.MaxValue;
        parent = null;
    }
}